namespace Enterprise.ZArchitecture.GUI
{
	partial class ZTextBoxPopupForm
	{
		ZButton okButton;
		ZButton cancelButton;
		ZTextBox textBox;
		ZPanel buttonPanel;

		new void InitializeComponent()
		{
			this.textBox = new ZTextBox();
			this.buttonPanel = new ZPanel();
			this.okButton = new ZButton();
			this.cancelButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.buttonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 243, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// textBox
			// 
			this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.textBox, false);
			this.textBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.textBox.Multiline = true;
			this.textBox.Name = "textBox";
			this.textBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 262, true);
			this.textBox.TabIndex = 1;
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 267, true);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 5, 0, 0, true);
			this.buttonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 28, true);
			this.buttonPanel.TabIndex = 2;
			// 
			// okButton
			// 
			this.okButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("91803978-fdf8-409c-8a3a-3bbb9aea0732", "OK");
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 5, true);
			this.okButton.Name = "okButton";
			this.okButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 0;
			this.okButton.ToolTipCaption = null;
			this.okButton.Click += OkButton_Click;
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("649b0e38-855e-4623-b56c-f55adee1f0b4", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 5, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.Click += CancelButton_Click;
			// 
			// ZTextBoxPopupForm
			// 
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("cca904d4-4e00-4307-b9b7-591e332595ce", "Edit Text");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 300, true);
			this.Controls.Add(this.textBox);
			this.Controls.Add(this.buttonPanel);
			this.Name = "ZTextBoxPopupForm";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Controls.SetChildIndex(this.buttonPanel, 0);
			this.Controls.SetChildIndex(this.textBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.buttonPanel.ResumeLayout(false);
			this.buttonPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
