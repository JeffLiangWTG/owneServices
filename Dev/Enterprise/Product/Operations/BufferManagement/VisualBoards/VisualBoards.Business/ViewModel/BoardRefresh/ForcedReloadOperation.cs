namespace Enterprise.VisualBoards.Business
{
	public sealed class ForcedReloadOperation : IBoardRefreshContext
	{
		BoardRefreshType IBoardRefreshContext.RefreshType => BoardRefreshType.Full;
	}
}
