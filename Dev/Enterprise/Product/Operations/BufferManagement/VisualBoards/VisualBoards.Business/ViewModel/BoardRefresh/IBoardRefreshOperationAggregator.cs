using System.Collections.Generic;

namespace Enterprise.VisualBoards.Business
{
	/// <summary>
	/// This is mandatory for all refresh related operations to allow future developers
	/// A point where they have full access to all state involved in refresh.
	/// </summary>
	public interface IBoardRefreshOperationAggregator
	{
		void AggregateRefresh(IEnumerable<IBoardRefreshable> refreshables, IBoardRefreshContext refreshContext);
	}
}
