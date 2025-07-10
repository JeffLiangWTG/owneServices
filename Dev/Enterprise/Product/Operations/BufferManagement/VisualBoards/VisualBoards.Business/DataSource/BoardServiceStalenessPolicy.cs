using System;

namespace Enterprise.VisualBoards.Business
{
	[Flags]
	public enum BoardServiceStalenessPolicy
	{
		None = 0,
		NeverStale = 1 << 1,
		StaleBeforeBoardRefresh = 1 << 2,
		StaleBeforeSavingTicket = 1 << 3,
	}
}
