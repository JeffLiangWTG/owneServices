using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI
{
	partial class ImportFromTemporaryStorageRegisterModuleForm
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
		private new void InitializeComponent()
		{
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OK_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FilterControlPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 376, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 24, true);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.ButtonsPanel);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 339, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 37, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Controls.Add(this.OK_Button);
			this.ButtonsPanel.Controls.Add(this.Cancel_Button);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(934, 0, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 37, true);
			this.ButtonsPanel.TabIndex = 1;
			// 
			// OK_Button
			// 
			this.OK_Button.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("AC64BFE6-3EAB-4847-B4C8-A0EF122E7D4C", "Select");
			this.OK_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
			this.OK_Button.Name = "OK_Button";
			this.OK_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.OK_Button.TabIndex = 0;
			this.OK_Button.ToolTipCaption = null;
			this.OK_Button.Click += new System.EventHandler(this.OK_Button_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("F3723E44-EF1B-4A29-A0A8-36F41DE7FB84", "Cancel");
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 7, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.Cancel_Button.TabIndex = 1;
			this.Cancel_Button.ToolTipCaption = null;
			this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
			// 
			// FilterControlPanel
			// 
			this.FilterControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterControlPanel.Name = "FilterControlPanel";
			this.FilterControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 339, true);
			this.FilterControlPanel.TabIndex = 1;
			// 
			// ImportFromTemporaryStorageRegisterModuleForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 400, true);
			this.Controls.Add(this.FilterControlPanel);
			this.Controls.Add(this.BottomPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1116, 439, true);
			this.Name = "ImportFromTemporaryStorageRegisterModuleForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.FilterControlPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZPanel FilterControlPanel;
		private ZPanel BottomPanel;
		private ZPanel ButtonsPanel;
		internal ZButton OK_Button;
		internal ZButton Cancel_Button;
	}
}
