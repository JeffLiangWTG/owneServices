using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BoardSectionDataSourceParameters
	{
		public BoardSectionDataSourceParameters(
			PropertyCache cache,
			ZQuery workflowSectionFilter,
			ZQuery taskSectionFilter,
			IEnumerable<BMBoardSectionChannel> channels,
			IEnumerable<IVisualBoardChannel> allChannels)
		{
			Cache = cache;
			WorkflowSectionFilter = workflowSectionFilter;
			TaskSectionFilter = taskSectionFilter;
			BMBoardChannels = channels;
			AllChannels = allChannels;
		}

		public PropertyCache Cache { get; }
		public ZQuery WorkflowSectionFilter { get; }
		public ZQuery TaskSectionFilter { get; }
		public IEnumerable<BMBoardSectionChannel> BMBoardChannels { get; }
		public IEnumerable<IVisualBoardChannel> AllChannels { get; }
	}
}
