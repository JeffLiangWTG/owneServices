using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.VisualBoards.GUI.Test
{
	[TestsSubclassesOf(typeof(IBoardFilter))]
	public abstract class BoardFilterTestCase<T> : VisualBoardsTestCase
		where T : IBoardFilter
	{
		public abstract void TestFilterName();
		public abstract void TestAllowMultiple();
		public abstract void TestEquals();
		public abstract void TestWhenRemovedThroughFilterManager_ShouldRestoreVisualState();

		protected abstract T GetFilter();

		public void TestWhenFilterDoesNotRequireFullRedraw_ShouldImplementIUndoableFilter()
		{
			var filter = GetFilter();
			if (filter.RequiresRedraw)
			{
				Assert(true);
			}
			else
			{
				var message = string.Format("Filter type [{0}] should implement [{1}] so that it can be undone manually rather than relying on a full board redraw.",
					filter.GetType().FullName,
					typeof(IUndoableFilter).FullName);

				Assert(message, filter is IUndoableFilter);
			}
		}
	}
}
