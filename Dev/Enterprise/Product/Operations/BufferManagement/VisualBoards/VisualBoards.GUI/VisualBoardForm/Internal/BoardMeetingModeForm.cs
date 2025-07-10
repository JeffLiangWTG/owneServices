using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.VisualBoards.GUI
{
	public partial class BoardMeetingModeForm : ZChildForm
	{
		public BoardMeetingModeForm()
		{
			InitializeComponent();
		}

		public BoardMeetingModeForm(VisualBoardForm visualBoardForm)
			: this()
		{
			this.visualBoardForm = visualBoardForm;

			MeetingModeStopWatchControl.StartOrResume();
		}

		readonly VisualBoardForm visualBoardForm;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				MeetingModeStopWatchControl.Dispose();
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			if (visualBoardForm != null)
			{
				visualBoardForm.LeaveBoardMeetingMode();
			}
		}

		internal void ShowPreviousChannel()
		{
			var message = new Message();
			visualBoardForm.HandlePressedKeys(ref message, BoardMeetingModeShortcuts.PreviousChannelKeyboardShortcut);
		}

		internal void ShowNextChannel()
		{
			var message = new Message();
			visualBoardForm.HandlePressedKeys(ref message, BoardMeetingModeShortcuts.NextChannelKeyboardShortcut);
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			return base.ProcessCmdKey(ref msg, keyData) || visualBoardForm.HandlePressedKeys(ref msg, keyData);
		}

		#region For Test

		public void ShowPreviousChannel_ForTest()
		{
			MeetingModeStopWatchControl.FindSingle<ZButton>("PreviousChannelButton").PerformClick();
		}

		public void ShowNextChannel_ForTest()
		{
			MeetingModeStopWatchControl.FindSingle<ZButton>("NextChannelButton").PerformClick();
		}

		#endregion
	}
}
