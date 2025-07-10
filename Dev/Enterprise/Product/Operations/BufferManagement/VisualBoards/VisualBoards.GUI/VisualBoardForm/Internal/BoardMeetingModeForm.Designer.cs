namespace Enterprise.VisualBoards.GUI
{
	partial class BoardMeetingModeForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.MeetingModeStopWatchControl = new Enterprise.VisualBoards.GUI.StopWatchControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MeetingModeStopWatchControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Visible = false;
			// 
			// MeetingModeStopWatchControl
			// 
			this.MeetingModeStopWatchControl.AllowDrop = true;
			this.MeetingModeStopWatchControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.MeetingModeStopWatchControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MeetingModeStopWatchControl.Name = "MeetingModeStopWatchControl";
			this.MeetingModeStopWatchControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 63, true);
			this.MeetingModeStopWatchControl.TabIndex = 1;
			// 
			// BoardMeetingModeForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.VisualBoards.GUI.Res.GetData("5d5b1317-1ed9-41f3-ade3-fe401f58ceef", "Board Meeting Mode");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 66, true);
			this.Controls.Add(this.MeetingModeStopWatchControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 66, true);
			this.Name = "BoardMeetingModeForm";
			this.TopMost = true;
			this.Controls.SetChildIndex(this.MeetingModeStopWatchControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MeetingModeStopWatchControl.ResumeLayout(true);
			this.MeetingModeStopWatchControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private VisualBoards.GUI.StopWatchControl MeetingModeStopWatchControl;
	}
}
