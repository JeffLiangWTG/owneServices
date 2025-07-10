using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class PopupStatusChangeForm
	{
		new void InitializeComponent()
		{
			this.reasonLabel = new ZArchitecture.ZLabel();
			this.reasonTextBox = new ZArchitecture.ZTextBox();
			this.oKButton = new ZButton();
			this.cancelButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 102, true);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(288);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(289);
			// 
			// ReasonLabel
			// 
			this.reasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.reasonLabel.Name = "ReasonLabel";
			this.reasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 23, true);
			this.reasonLabel.TabIndex = 1;
			this.reasonLabel.Text = "Reason:";
			// 
			// ReasonTextBox
			// 
			this.reasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.reasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.reasonTextBox.Name = "ReasonTextBox";
			this.reasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 20, true);
			this.reasonTextBox.TabIndex = 2;
			this.reasonTextBox.Text = "";
			// 
			// OKButton
			// 
			this.oKButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 72, true);
			this.oKButton.Name = "OKButton";
			this.oKButton.TabIndex = 3;
			this.oKButton.Text = "OK";
			this.oKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelButton
			// 
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 72, true);
			this.cancelButton.Name = "CancelButton";
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// PopupStatusChangeForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 126, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.oKButton);
			this.Controls.Add(this.reasonTextBox);
			this.Controls.Add(this.reasonLabel);
			this.Name = "PopupStatusChangeForm";
			this.Text = "Set Status";
			this.Controls.SetChildIndex(this.reasonLabel, 0);
			this.Controls.SetChildIndex(this.reasonTextBox, 0);
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}

		ZArchitecture.ZLabel reasonLabel;
		ZButton oKButton;
		ZButton cancelButton;
		ZArchitecture.ZTextBox reasonTextBox;
	}
}
