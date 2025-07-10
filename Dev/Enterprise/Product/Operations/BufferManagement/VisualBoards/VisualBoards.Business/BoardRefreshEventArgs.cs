using System;

namespace Enterprise.VisualBoards.Business
{
	public class BoardRefreshEventArgs : EventArgs
	{
		public Action ReloadAction { get; set; }

		public bool TriggeredBySlideshowTimer { get; set; }
		public bool TriggeredBySlideshowControl { get; set; }
		public bool TriggeredByRefreshTimer { get; set; }
		public bool TriggeredByUserRefreshingBoardOrSection { get; set; }
		public bool TriggeredByShortcut { get; set; }
		public bool TriggeredByBoardConfigForm { get; set; }

		public bool IsInitialLoad { get; set; }
		public bool IsReloading { get; set; }
		public bool ShouldReload { get; set; }
		public bool ShouldReloadSlides { get; set; }
		public bool IsDifferentialRefresh { get; set; }
		public bool IsForcedReload { get; set; }

		public bool CachedLayoutWasDisposed { get; set; }

		public bool IsSlideshowProgression => TriggeredBySlideshowTimer || TriggeredBySlideshowControl;

		public static new BoardRefreshEventArgs Empty => new BoardRefreshEventArgs();
	}
}
