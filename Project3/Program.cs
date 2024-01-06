using System;
using System.Collections.Generic;
using System.Linq;
 
public class Graph
{
    public int VerticesCount { get; } // number of vertices in the graph
    public readonly List<(int, int)>[] edges; // adjacency list of edges
    public Graph(int verticesCount) // constructor
    {
        VerticesCount = verticesCount;
        edges = new List<(int, int)>[verticesCount];
        // initialize adjacency lists for all the vertices
        for (int i = 0; i < verticesCount; i++)
            edges[i] = new List<(int, int)>();
    }

    // add an edge from vertex from -> vertex to with duration
    public void AddEdge(int from, int to, int duration)
    {
        edges[from].Add((to, duration));
    }

    // calculate project data which includes earliest start, latest start, slack, and critical paths
    public (int[] earliestStart, int[] latestStart, int[] slack, List<List<int>> criticalPaths) CalculateProjectData()
    {
        var inDegrees = new int[VerticesCount];
        // calculate in-degrees of all vertices
        foreach (var adjacent in edges)
            foreach (var (to, _) in adjacent)
                inDegrees[to]++;

        // calculate project data
        var topologicalOrder = TopologicalSort(inDegrees);
        var (earliestStart, latestStart) = CalculateTimes(topologicalOrder);
        var slack = latestStart.Select((latest, i) => latest - earliestStart[i]).ToArray();
        var criticalPaths = FindCriticalPaths(earliestStart, latestStart);

        return (earliestStart, latestStart, slack, criticalPaths);
    }

    // topological sort using Kahn's algorithm
    private List<int> TopologicalSort(int[] inDegrees)
    {
        var zeroInDegreeQueue = new Queue<int>();
        // enqueue all vertices with in-degree 0
        for (int i = 0; i < VerticesCount; i++)
            if (inDegrees[i] == 0)
                zeroInDegreeQueue.Enqueue(i);

        var topologicalOrder = new List<int>();
        // dequeue a vertex with in-degree 0 and add it to topological order
        while (zeroInDegreeQueue.Count > 0)
        {
            int u = zeroInDegreeQueue.Dequeue();
            topologicalOrder.Add(u);
            // decrement in-degree of all adjacent vertices
            foreach (var (v, _) in edges[u])
            {
                inDegrees[v]--;
                if (inDegrees[v] == 0)
                    zeroInDegreeQueue.Enqueue(v);
            }
        }

        // if topological order doesn't contain all vertices, then there is a cycle
        if (topologicalOrder.Count != VerticesCount)
            throw new InvalidOperationException("Graph has at least one cycle and is not a DAG.");

        // print topological order
        Console.WriteLine("Topological Order: " + string.Join(" -> ", topologicalOrder.Select(v => $"V{v + 1}")));

        return topologicalOrder;
    }

    // calculate earliest start and latest start times
    private (int[] earliestStart, int[] latestStart) CalculateTimes(List<int> topologicalOrder)
    {
        var earliestStart = new int[VerticesCount]; 
        var latestStart = new int[VerticesCount];

        // Forward pass to calculate earliest start times
        foreach (int u in topologicalOrder)
        {
            foreach (var (v, weight) in edges[u])
            {
                earliestStart[v] = Math.Max(earliestStart[v], earliestStart[u] + weight); // earliest start time of v is max of all its incoming edges
            }
        }

        

        // Initialize latest start times to maximum earliest start time
        int maxDuration = earliestStart.Max();
        for (int i = 0; i < VerticesCount; i++)
        {
            latestStart[i] = maxDuration; 
        }

        // Backward pass to calculate latest start times
        for (int i = topologicalOrder.Count - 1; i >= 0; i--)
        {
            int u = topologicalOrder[i];
            foreach (var (v, weight) in edges[u])
            {
                latestStart[u] = Math.Min(latestStart[u], latestStart[v] - weight); // latest start time of u is min of all its outgoing edges
            }
        }

        return (earliestStart, latestStart);
    }

    // find all critical paths
    private List<List<int>> FindCriticalPaths(int[] earliestStart, int[] latestStart)
    {
        var criticalPaths = new List<List<int>>(); // list of critical paths
        // find all paths from source to sink with same earliest and latest start times
        DFS(0, new List<int>(), earliestStart, latestStart, criticalPaths);
        return criticalPaths;
    }

    // DFS to find all paths from source to sink with same earliest and latest start times
    private void DFS(int u, List<int> path, int[] earliestStart, int[] latestStart, List<List<int>> criticalPaths)
    {
        path.Add(u); // add current vertex to path
        // if we reached sink and all vertices in path have same earliest and latest start times, then add path to critical paths
        if (u == VerticesCount - 1 && path.All(v => earliestStart[v] == latestStart[v]))
        {
            criticalPaths.Add(new List<int>(path));
        }
        // else recursively call DFS on all adjacent vertices with same earliest and latest start times
        else
        {
            // find all adjacent vertices with same earliest and latest start times
            foreach (var (v, weight) in edges[u])
            {
                if (earliestStart[v] == earliestStart[u] + weight)
                {
                    DFS(v, path, earliestStart, latestStart, criticalPaths);
                }
            }
        }
        // remove current vertex from path
        path.RemoveAt(path.Count - 1);
    }

}

public class Program
{
    public static void Main()
    {
        Graph projectGraph = new Graph(9); // total number of vertices in the graph
        // Add edges to the graph here with durations
        projectGraph.AddEdge(0, 1, 6);
        projectGraph.AddEdge(0, 2, 4);
        projectGraph.AddEdge(0, 3, 5);
        projectGraph.AddEdge(1, 4, 1);
        projectGraph.AddEdge(2, 4, 1);
        projectGraph.AddEdge(3, 5, 2);
        projectGraph.AddEdge(4, 6, 9);
        projectGraph.AddEdge(4, 7, 7);
        projectGraph.AddEdge(5, 7, 4);
        projectGraph.AddEdge(6, 8, 2);
        projectGraph.AddEdge(7, 8, 4);

        // print out the graph
        for (int i = 0; i < projectGraph.VerticesCount; i++)
        {
            Console.Write($"V{i + 1}: ");
            foreach (var (v, weight) in projectGraph.edges[i])
            {
                Console.Write($"V{v + 1}({weight}) ");
            }
            Console.WriteLine();
        }

        // Once all edges are added, calculate project data
        var (earliestStart, latestStart, slack, criticalPaths) = projectGraph.CalculateProjectData();

        // Output the project data here
        Console.WriteLine("Project Duration: " + earliestStart.Max());
        Console.WriteLine("Critical Paths: ");
        foreach (var path in criticalPaths)
        {
            Console.WriteLine(string.Join(" -> ", path.Select(v => $"V{v + 1}")));
        }

        // print earliest start
        Console.WriteLine("Earliest Start: ");
        for (int i = 0; i < earliestStart.Length-1; i++)
        {
            Console.WriteLine($"V{i + 1}: {earliestStart[i]+1}");
        }

        // print latest start
        Console.WriteLine("Latest Start: ");
        for (int i = 0; i < latestStart.Length-1; i++)
        {
            Console.WriteLine($"V{i + 1}: {latestStart[i]+1}");
        }

        // print slack
        Console.WriteLine("Slack: ");
        for (int i = 0; i < slack.Length-1; i++)
        {
            Console.WriteLine($"V{i + 1}: {slack[i]}");
        }
    }
}
