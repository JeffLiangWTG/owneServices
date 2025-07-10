namespace Enterprise.VisualBoards.Business
{
	public class FullRefreshOperation : IBoardRefreshContext
	{
		public BoardRefreshType RefreshType
		{
			get { return BoardRefreshType.Full; }
		}
	}
}
