using CargoWise.Common;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class NonResettableService : IBoardFactoryService
	{
		#region IBoardFactoryService Members

		BoardServiceStalenessPolicy IBoardFactoryService.StalenessPolicy => BoardServiceStalenessPolicy.NeverStale;

		void IBoardFactoryService.ClearCache()
		{
			ErrorReporter.ReportOnce("'Never stale' services should never be cleared by the board, ever. For any reason.");
		}

		#endregion
	}
}
