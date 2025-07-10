using System;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public sealed class LoadingIndicatorEventArgs : EventArgs
	{
		public static LoadingIndicatorEventArgs ForInitialLayoutRender()
		{
			return new LoadingIndicatorEventArgs(Res.GetString("9d1295b1-9367-47b5-a986-f8124c78a09f", "Loading..."), BoardRefreshType.Reload);
		}

		public static LoadingIndicatorEventArgs ForExistingLayoutRefresh(BoardRefreshType refreshType, bool didUserForceRefresh = false)
		{
			return new LoadingIndicatorEventArgs(Res.GetString("5ba633f4-bbe7-42a6-8977-c10f084f86ca", "Refreshing, please wait..."), refreshType, didUserForceRefresh);
		}

		public static LoadingIndicatorEventArgs ForSpecificMessage(string message, BoardRefreshType refreshType)
		{
			return new LoadingIndicatorEventArgs(message, refreshType);
		}

		public static LoadingIndicatorEventArgs ForRemovingShownMessage(BoardRefreshType refreshType)
		{
			return new LoadingIndicatorEventArgs(null, refreshType);
		}

		LoadingIndicatorEventArgs(string message, BoardRefreshType refreshType, bool didUserForceRefresh = false)
		{
			Message = message;
			RefreshType = refreshType;
			this.didUserForceRefresh = didUserForceRefresh;
		}

		public string Message { get; }
		public BoardRefreshType RefreshType { get; }
		readonly bool didUserForceRefresh;

		public bool IsForInitialLayoutRender => RefreshType == BoardRefreshType.Reload;

		public bool IsForInitialLayoutRenderOrUserForcedRefresh => IsForInitialLayoutRender || didUserForceRefresh;
	}
}
