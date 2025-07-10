namespace Enterprise.Customs.DE.GUI
{
	partial class SumARegisterForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.SumARegisterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SumARegisterControl = new Enterprise.Customs.DE.GUI.SumARegisterUserControl();
			this.MainTabControl.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.LogsTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SumARegisterPanel.SuspendLayout();
			this.SumARegisterControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1234, 784, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.SumARegisterPanel);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1130, 442, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1130, 442, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 757, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1234, 784, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1234, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageRegHeader);
			// 
			// SumARegisterPanel
			// 
			this.SumARegisterPanel.Controls.Add(this.SumARegisterControl);
			this.SumARegisterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SumARegisterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SumARegisterPanel.Name = "SumARegisterPanel";
			this.SumARegisterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 757, true);
			this.SumARegisterPanel.TabIndex = 1;
			// 
			// SumARegisterControl
			// 
			this.SumARegisterControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SumARegisterControl, ".");
			this.SumARegisterControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SumARegisterControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SumARegisterControl.Name = "SumARegisterControl";
			this.SumARegisterControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 757, true);
			this.SumARegisterControl.TabIndex = 0;
			// 
			// SumARegisterForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1234, 840, true);
			this.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageRegHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 725, true);
			this.Name = "SumARegisterForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.LogsTabPage.ResumeLayout(false);
			this.LogsTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SumARegisterPanel.ResumeLayout(false);
			this.SumARegisterPanel.PerformLayout();
			this.SumARegisterControl.ResumeLayout(true);
			this.SumARegisterControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel SumARegisterPanel;
		private SumARegisterUserControl SumARegisterControl;
	}
}
