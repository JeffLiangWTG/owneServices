using System;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.GUI;
using Microsoft.Win32;

namespace Enterprise.BufferManagement.GUI.Test
{
	class VisualBoardFormForRefreshTest : VisualBoardForm
	{
		public VisualBoardFormForRefreshTest(BoardSlideshowViewModel viewModel)
			: base(viewModel)
		{
			// Tests that minimize/maximise the window might be slower/faster depending on machine.
			ResizeBegin += (sender, args) => AutoRefreshForTest.IsPaused = true;
			ResizeEnd += (sender, args) => AutoRefreshForTest.IsPaused = false;
		}

#if !WINZOR
		public void SetEnterpriseChannel(EnterpriseChannel channel)
		{
			EnterpriseChannel = channel;
		}
#endif

		public void SetRefreshDelayAndRestartTimer(TimeSpan refreshDelay)
		{
			AutoRefreshForTest.RefreshDelay = refreshDelay;
			AutoRefreshForTest.TimeUntilRefresh = refreshDelay;
		}

		public void SimulateSessionSwitch(SessionSwitchReason reason)
		{
#if !WINZOR
			HandleSessionSwitchMessage(reason);
#endif
		}

		public void OnActivated_Exposed()
		{
			OnActivated(new EventArgs());
		}

		public void OnMouseButtonClick_Exposed()
		{
			OnMouseButtonClick();
		}
	}
}
