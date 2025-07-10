namespace Enterprise.Startup.Tools
{
	partial class ThreadMonitorForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.PNLButtons = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ThreadGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SelectedThreadDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GrpManualTracking = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BtnCopy = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BtnClear = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BtnCollect = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GrpAutomaticTracking = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TxtIntervalNumber = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CmbIntervalType = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BtnStart = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LSTTimes = new Enterprise.ZArchitecture.GUI.ZListBox();
			this.TxtStackTrace = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PNLButtons.SuspendLayout();
			this.ThreadGroupBox.SuspendLayout();
			this.SelectedThreadDropEdit.SuspendLayout();
			this.GrpManualTracking.SuspendLayout();
			this.GrpAutomaticTracking.SuspendLayout();
			this.CmbIntervalType.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 649, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Startup.Tools.ThreadMonitor);
			// 
			// PNLButtons
			// 
			this.PNLButtons.Controls.Add(this.ThreadGroupBox);
			this.PNLButtons.Controls.Add(this.GrpManualTracking);
			this.PNLButtons.Controls.Add(this.GrpAutomaticTracking);
			this.PNLButtons.Dock = System.Windows.Forms.DockStyle.Top;
			this.PNLButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PNLButtons.Name = "PNLButtons";
			this.PNLButtons.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.PNLButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 150, true);
			this.PNLButtons.TabIndex = 6;
			// 
			// ThreadGroupBox
			// 
			this.ThreadGroupBox.CaptionResourceString = CargoWise.Main.Res.GetData("d4825919-d8b5-4d03-b12e-0f70e1b3ab6e", "Thread to Track");
			this.ThreadGroupBox.Controls.Add(this.SelectedThreadDropEdit);
			this.ThreadGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ThreadGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.ThreadGroupBox.Name = "ThreadGroupBox";
			this.ThreadGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 52, true);
			this.ThreadGroupBox.TabIndex = 2;
			this.ThreadGroupBox.TabStop = false;
			// 
			// SelectedThreadDropEdit
			// 
			this.SelectedThreadDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SelectedThreadDropEdit, "SelectedThreadId");
			this.SelectedThreadDropEdit.CaptionResourceString = CargoWise.Main.Res.GetData("ccce5123-f5de-4758-bc33-6f67b12d3b90", "Thread");
			this.SelectedThreadDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SelectedThreadDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 19, true);
			this.SelectedThreadDropEdit.Name = "SelectedThreadDropEdit";
			this.SelectedThreadDropEdit.PreBoundMaxLength = 10;
			this.SelectedThreadDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.SelectedThreadDropEdit.TabIndex = 1;
			// 
			// GrpManualTracking
			// 
			this.GrpManualTracking.CaptionResourceString = CargoWise.Main.Res.GetData("cd1e4fbd-3a72-4855-93da-a8ca202421b1", "Manual Tracking");
			this.GrpManualTracking.Controls.Add(this.BtnCopy);
			this.GrpManualTracking.Controls.Add(this.BtnClear);
			this.GrpManualTracking.Controls.Add(this.BtnCollect);
			this.GrpManualTracking.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 63, true);
			this.GrpManualTracking.Name = "GrpManualTracking";
			this.GrpManualTracking.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 77, true);
			this.GrpManualTracking.TabIndex = 1;
			this.GrpManualTracking.TabStop = false;
			// 
			// BtnCopy
			// 
			this.BtnCopy.BackColor = System.Drawing.Color.WhiteSmoke;
			this.BtnCopy.CaptionResourceString = CargoWise.Main.Res.GetData("91e9bdc1-ad28-49f1-8e6c-23c96a301430", "Copy All");
			this.BtnCopy.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 19, true);
			this.BtnCopy.Name = "BtnCopy";
			this.BtnCopy.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.BtnCopy.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 46, true);
			this.BtnCopy.TabIndex = 2;
			this.BtnCopy.UseVisualStyleBackColor = false;
			this.BtnCopy.Click += new System.EventHandler(this.BtnCopy_Click);
			// 
			// BtnClear
			// 
			this.BtnClear.BackColor = System.Drawing.Color.Salmon;
			this.BtnClear.CaptionResourceString = CargoWise.Main.Res.GetData("f410908b-19cb-4179-81d7-139efd5c3f8b", "Clear");
			this.BtnClear.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 19, true);
			this.BtnClear.Name = "BtnClear";
			this.BtnClear.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.BtnClear.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 46, true);
			this.BtnClear.TabIndex = 3;
			this.BtnClear.UseVisualStyleBackColor = false;
			this.BtnClear.Click += new System.EventHandler(this.BtnClear_Click);
			// 
			// BtnCollect
			// 
			this.BtnCollect.BackColor = System.Drawing.Color.SkyBlue;
			this.BtnCollect.CaptionResourceString = CargoWise.Main.Res.GetData("eff1efd2-3629-445f-872f-7bb54158659d", "Collect Thread Snapshot");
			this.BtnCollect.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.BtnCollect.Name = "BtnCollect";
			this.BtnCollect.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.BtnCollect.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 46, true);
			this.BtnCollect.TabIndex = 1;
			this.BtnCollect.UseVisualStyleBackColor = false;
			this.BtnCollect.Click += new System.EventHandler(this.BtnCollect_Click);
			// 
			// GrpAutomaticTracking
			// 
			this.GrpAutomaticTracking.CaptionResourceString = CargoWise.Main.Res.GetData("a4088234-12e2-43ac-b018-03ec0935d3d7", "Automatic Tracking");
			this.GrpAutomaticTracking.Controls.Add(this.TxtIntervalNumber);
			this.GrpAutomaticTracking.Controls.Add(this.CmbIntervalType);
			this.GrpAutomaticTracking.Controls.Add(this.BtnStart);
			this.GrpAutomaticTracking.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 63, true);
			this.GrpAutomaticTracking.Name = "GrpAutomaticTracking";
			this.GrpAutomaticTracking.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 77, true);
			this.GrpAutomaticTracking.TabIndex = 0;
			this.GrpAutomaticTracking.TabStop = false;
			// 
			// TxtIntervalNumber
			// 
			this.BindingSource.SetBindingMember(this.TxtIntervalNumber, "IntervalNumber");
			this.TxtIntervalNumber.CaptionResourceString = CargoWise.Main.Res.GetData("4c7f41b3-3047-40f6-b946-9dad5f679b0b", "Interval Number");
			this.TxtIntervalNumber.DecimalPlaces = 0;
			this.TxtIntervalNumber.Decimals = 0;
			this.TxtIntervalNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 19, true);
			this.TxtIntervalNumber.Name = "TxtIntervalNumber";
			this.TxtIntervalNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.TxtIntervalNumber.TabIndex = 0;
			this.TxtIntervalNumber.Text = "0";
			this.TxtIntervalNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CmbIntervalType
			// 
			this.CmbIntervalType.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CmbIntervalType, "IntervalType");
			this.CmbIntervalType.CaptionResourceString = CargoWise.Main.Res.GetData("a83a2644-3e6a-4945-a913-ff628ae2e4b7", "Interval Type");
			this.CmbIntervalType.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CmbIntervalType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 45, true);
			this.CmbIntervalType.Name = "CmbIntervalType";
			this.CmbIntervalType.PreBoundMaxLength = 10;
			this.CmbIntervalType.ShowDescriptionBox = false;
			this.CmbIntervalType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.CmbIntervalType.TabIndex = 1;
			// 
			// BtnStart
			// 
			this.BtnStart.BackColor = System.Drawing.Color.DarkSeaGreen;
			this.BtnStart.CaptionResourceString = CargoWise.Main.Res.GetData("0169b3de-ac31-4fd9-b613-fde4866e6339", "Start");
			this.BtnStart.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 19, true);
			this.BtnStart.Name = "BtnStart";
			this.BtnStart.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.BtnStart.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 46, true);
			this.BtnStart.TabIndex = 3;
			this.BtnStart.UseVisualStyleBackColor = false;
			this.BtnStart.Click += new System.EventHandler(this.BtnStart_Click);
			// 
			// LSTTimes
			// 
			this.LSTTimes.Dock = System.Windows.Forms.DockStyle.Left;
			this.LSTTimes.FormattingEnabled = true;
			this.LSTTimes.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 150, true);
			this.LSTTimes.Name = "LSTTimes";
			this.LSTTimes.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 499, true);
			this.LSTTimes.TabIndex = 8;
			this.LSTTimes.SelectedIndexChanged += new System.EventHandler(this.LSTTimes_SelectedIndexChanged);
			// 
			// TxtStackTrace
			// 
			this.TxtStackTrace.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TxtStackTrace.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TxtStackTrace.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 150, true);
			this.TxtStackTrace.Multiline = true;
			this.TxtStackTrace.Name = "TxtStackTrace";
			this.TxtStackTrace.ReadOnly = true;
			this.TxtStackTrace.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.TxtStackTrace.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 499, true);
			this.TxtStackTrace.TabIndex = 9;
			this.TxtStackTrace.WordWrap = false;
			// 
			// ThreadMonitorForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWise.Main.Res.GetData("35f6b9b3-8860-49e5-a710-54f38d7f34eb", "Thread Monitoring Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 673, true);
			this.Controls.Add(this.TxtStackTrace);
			this.Controls.Add(this.LSTTimes);
			this.Controls.Add(this.PNLButtons);
			this.DataSourceType = typeof(Enterprise.Startup.Tools.ThreadMonitor);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 600, true);
			this.Name = "ThreadMonitorForm";
			this.TopMost = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PNLButtons, 0);
			this.Controls.SetChildIndex(this.LSTTimes, 0);
			this.Controls.SetChildIndex(this.TxtStackTrace, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PNLButtons.ResumeLayout(false);
			this.PNLButtons.PerformLayout();
			this.ThreadGroupBox.ResumeLayout(false);
			this.ThreadGroupBox.PerformLayout();
			this.SelectedThreadDropEdit.ResumeLayout(true);
			this.SelectedThreadDropEdit.PerformLayout();
			this.GrpManualTracking.ResumeLayout(false);
			this.GrpManualTracking.PerformLayout();
			this.GrpAutomaticTracking.ResumeLayout(false);
			this.GrpAutomaticTracking.PerformLayout();
			this.CmbIntervalType.ResumeLayout(true);
			this.CmbIntervalType.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZPanel PNLButtons;
		private ZArchitecture.GUI.ZButton BtnClear;
		private ZArchitecture.GUI.ZButton BtnCollect;
		private ZArchitecture.GUI.ZButton BtnStart;
		private ZArchitecture.GUI.ZGroupBox GrpAutomaticTracking;
		private ZArchitecture.GUI.ZDropEdit CmbIntervalType;
		private ZArchitecture.GUI.ZGroupBox GrpManualTracking;
		private ZArchitecture.ZTextBox TxtStackTrace;
		private ZArchitecture.GUI.ZListBox LSTTimes;
		private ZArchitecture.ZCalcEdit TxtIntervalNumber;
		private ZArchitecture.GUI.ZButton BtnCopy;
		private ZArchitecture.GUI.ZDropEdit SelectedThreadDropEdit;
		private ZArchitecture.GUI.ZGroupBox ThreadGroupBox;
	}
}
