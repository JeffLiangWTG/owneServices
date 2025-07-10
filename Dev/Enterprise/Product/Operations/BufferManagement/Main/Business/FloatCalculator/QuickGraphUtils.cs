using System.Collections.Generic;
using QuikGraph;
using QuikGraph.Algorithms.Search;

namespace Enterprise.BufferManagement.Business
{
	public static class QuickGraphUtils
	{
		public static IEnumerable<TVertex> GetCyclicVertexes<TVertex, TEdge>(IVertexListGraph<TVertex, TEdge> g)
				where TEdge : IEdge<TVertex>
		{
			var cyclicVertexes = new List<TVertex>();

			var dfs = new DepthFirstSearchAlgorithm<TVertex, TEdge>(g);
			var action = new EdgeAction<TVertex, TEdge>(x =>
			{
				cyclicVertexes.Add(x.Source);
				cyclicVertexes.Add(x.Target);
			});
			try
			{
				dfs.BackEdge += action;
				dfs.Compute();
			}
			finally
			{
				dfs.BackEdge -= action;
			}

			return cyclicVertexes;
		}
	}
}
