using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.Module
{
	partial class SimplifiedDeclarationModuleForm
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
			this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.panelOkCancelButtons = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OK_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FilterControlPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonPanel.SuspendLayout();
			this.panelOkCancelButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 270, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 24, true);
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Controls.Add(this.panelOkCancelButtons);
			this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 233, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 37, true);
			this.ButtonPanel.TabIndex = 2;
			// 
			// panelOkCancelButtons
			// 
			this.panelOkCancelButtons.Controls.Add(this.OK_Button);
			this.panelOkCancelButtons.Controls.Add(this.Cancel_Button);
			this.panelOkCancelButtons.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelOkCancelButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(918, 0, true);
			this.panelOkCancelButtons.Name = "panelOkCancelButtons";
			this.panelOkCancelButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 37, true);
			this.panelOkCancelButtons.TabIndex = 1;
			// 
			// OK_Button
			// 
			this.OK_Button.CaptionResourceString = Enterprise.Customs.DE.Module.Res.GetData("EmbeddedModulePopup|37df085d-e8ea-4b7a-9972-716d78ece4e8", "Select");
			this.OK_Button.IsCaptionOverridden = false;
			this.OK_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
			this.OK_Button.Name = "OK_Button";
			this.OK_Button.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OK_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.OK_Button.TabIndex = 0;
			this.OK_Button.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OK_Button.ToolTipCaption = null;
			this.OK_Button.Click += new System.EventHandler(this.OK_Button_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.CaptionResourceString = Enterprise.Customs.DE.Module.Res.GetData("EmbeddedModulePopup|56b37ed7-e1da-463b-a4c1-76eb3c2e18b1", "Cancel");
			this.Cancel_Button.IsCaptionOverridden = false;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 7, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.Cancel_Button.TabIndex = 1;
			this.Cancel_Button.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.Cancel_Button.ToolTipCaption = null;
			this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
			// 
			// FilterControlPanel
			// 
			this.FilterControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterControlPanel.Name = "FilterControlPanel";
			this.FilterControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 233, true);
			this.FilterControlPanel.TabIndex = 1;
			// 
			// SimplifiedDeclarationModuleForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 294, true);
			this.Controls.Add(this.FilterControlPanel);
			this.Controls.Add(this.ButtonPanel);
			this.Name = "SimplifiedDeclarationModuleForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonPanel, 0);
			this.Controls.SetChildIndex(this.FilterControlPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonPanel.ResumeLayout(false);
			this.ButtonPanel.PerformLayout();
			this.panelOkCancelButtons.ResumeLayout(false);
			this.panelOkCancelButtons.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZPanel FilterControlPanel;
		private ZPanel ButtonPanel;
		private ZPanel panelOkCancelButtons;
		private ZButton OK_Button;
		private ZButton Cancel_Button;
	}
}
