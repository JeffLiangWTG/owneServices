using System;

namespace Enterprise.ZArchitecture.GUI
{
	 partial class GlowDataWizardImportForm
	 {
		internal ZButton closeButton;
		internal SerializableTextBox progressTextBox;

		new void InitializeComponent()
		{
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.progressTextBox = new SerializableTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 556, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 10, true);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.closeButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("GlowDataWizardImportForm|e8fa6d4e-27bd-4c6a-8023-feb147484d8e", "Close");
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 520, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 30, true);
			this.closeButton.TabIndex = 5;
			this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// progressTextBox
			// 
			this.progressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.progressTextBox.Multiline = true;
			this.progressTextBox.Name = "progressTextBox";
			this.progressTextBox.ReadOnly = true;
			this.progressTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.progressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 453, true);
			this.progressTextBox.TabIndex = 5;
			// 
			// GlowDataWizardImportForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("GlowDataWizardImportForm|60e983f3-c44c-4616-b829-eccf448f73f2", "Advanced Data Automation Wizard");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 566, true);
			this.Controls.Add(this.progressTextBox);
			this.Controls.Add(this.closeButton);
			this.Name = "GlowDataWizardImportForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.closeButton, 0);
			this.Controls.SetChildIndex(this.progressTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
