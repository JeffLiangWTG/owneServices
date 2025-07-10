using System.Collections.Generic;
using System.Linq;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class FullRefreshOperationAggregator : IBoardRefreshOperationAggregator
	{
		public void AggregateRefresh(IEnumerable<IBoardRefreshable> refreshables, IBoardRefreshContext refreshContext)
		{
			TagDefinitionCache tagDefinitionCache = null;

			foreach (var refreshable in refreshables.OfType<BMBoardSectionViewModel>())
			{
				refreshable.TagDefinitionCache = tagDefinitionCache ?? (tagDefinitionCache = new TagDefinitionCache(refreshable.FactoryProvider.GetBoardGUIThreadFactory()));
			}
		}
	}
}
