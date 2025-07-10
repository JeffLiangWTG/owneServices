namespace Enterprise.VisualBoards.Business.Test
{
	public class DummyBoardFactoryService : IBoardFactoryService
	{
		public BoardServiceStalenessPolicy StalenessPolicy { get; set; } = BoardServiceStalenessPolicy.StaleBeforeBoardRefresh;

		void IBoardFactoryService.ClearCache()
		{
			CacheRefreshedCount++;
		}

		public int CacheRefreshedCount { get; private set; }
	}
}
