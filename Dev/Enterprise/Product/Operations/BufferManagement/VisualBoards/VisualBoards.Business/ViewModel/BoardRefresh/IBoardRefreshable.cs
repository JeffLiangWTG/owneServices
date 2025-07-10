using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.VisualBoards.Business
{
	public interface IBoardRefreshable
	{
		IBoardRefreshable Parent { get; }

		IEnumerable<IBoardRefreshable> Children { get; }

		void PerformRefreshAction(IBoardRefreshContext refreshContext);

		void AddAggregator(IBoardRefreshContext context, HashSet<IBoardRefreshOperationAggregator> aggregators);

		string Name { get; }

		void BeforeDataRefresh();
	}

	public static class IBoardRefreshableExtensions
	{
		static void RefreshDescendants(this IBoardRefreshable refreshable, IBoardRefreshContext refreshContext)
		{
			var nodesNeedingRefresh = new[] { refreshable }.SelectDistinctRecursive(s => s.Children).ToArray();
			var aggregators = new HashSet<IBoardRefreshOperationAggregator>(new AggregatorEqualityComparer());

			nodesNeedingRefresh.ForEach(node => node.AddAggregator(refreshContext, aggregators));
			aggregators.ForEach(aggregator => aggregator.AggregateRefresh(nodesNeedingRefresh, refreshContext));

			foreach (var node in nodesNeedingRefresh)
			{
				node.PerformRefreshAction(refreshContext);
			}
		}

		public static void RefreshAll(this IBoardRefreshable refreshable, IBoardRefreshContext refreshContext)
		{
			var root = FindRoot(refreshable);
			root.RefreshDescendants(refreshContext);
		}

		static IBoardRefreshable FindRoot(IBoardRefreshable refreshable)
		{
			var current = refreshable;

			while (current.Parent != null)
			{
				current = current.Parent;
			}

			return current;
		}

		class AggregatorEqualityComparer : IEqualityComparer<IBoardRefreshOperationAggregator>
		{
			public bool Equals(IBoardRefreshOperationAggregator x, IBoardRefreshOperationAggregator y)
			{
				return x.GetType() == y.GetType();
			}

			public int GetHashCode(IBoardRefreshOperationAggregator obj)
			{
				return obj.GetType().GetHashCode();
			}
		}
	}
}
