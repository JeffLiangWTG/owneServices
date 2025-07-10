using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class DogHitXRayForm : ZChildForm
	{
		private ZButton CloseButton;
		internal ZButton OkButton;
		private ZLabel TrackingNumberLabel;
		internal ZTextBox TrackingNumberTextBox;
		private ZRadioButton CustomsRadioButton;
		private ZRadioButton QuarantineRadioButton;
		internal ZTextBox RemarksZTextBox;
		private ZLabel RemarksLabel;
		private ZGroupBox ReasonGroupBox;
		private ZDropEdit MasterbillNumberDropEdit;
		private ZLabel MasterbillNumberLabel;

		protected override void InitializeComponent()
		{
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TrackingNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TrackingNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.QuarantineRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.RemarksZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RemarksLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReasonGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MasterbillNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MasterbillNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.ReasonGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 310, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 281, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 6;
			this.CloseButton.Text = "&Close";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 281, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OkButton.TabIndex = 5;
			this.OkButton.Text = "&OK";
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// TrackingNumberLabel
			// 
			this.TrackingNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.TrackingNumberLabel.Name = "TrackingNumberLabel";
			this.TrackingNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 16, true);
			this.TrackingNumberLabel.TabIndex = 0;
			this.TrackingNumberLabel.Text = "Tracking Number:";
			// 
			// TrackingNumberTextBox
			// 
			this.TrackingNumberTextBox.BindTo = "TrackingNumber";
			this.TrackingNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 7, true);
			this.TrackingNumberTextBox.Name = "TrackingNumberTextBox";
			this.TrackingNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.TrackingNumberTextBox.TabIndex = 1;
			// 
			// CustomsRadioButton
			// 
			this.CustomsRadioButton.AutoCheck = false;
			this.CustomsRadioButton.AutoSize = true;
			this.CustomsRadioButton.BindTo = "CustomsHold";
			this.CustomsRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CustomsRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 19, true);
			this.CustomsRadioButton.Name = "CustomsRadioButton";
			this.CustomsRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 17, true);
			this.CustomsRadioButton.TabIndex = 0;
			this.CustomsRadioButton.TabStop = true;
			this.CustomsRadioButton.Text = "Customs";
			this.CustomsRadioButton.UseVisualStyleBackColor = true;
			// 
			// QuarantineRadioButton
			// 
			this.QuarantineRadioButton.AutoCheck = false;
			this.QuarantineRadioButton.AutoSize = true;
			this.QuarantineRadioButton.BindTo = "QuarantineHold";
			this.QuarantineRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.QuarantineRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 42, true);
			this.QuarantineRadioButton.Name = "QuarantineRadioButton";
			this.QuarantineRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 17, true);
			this.QuarantineRadioButton.TabIndex = 1;
			this.QuarantineRadioButton.TabStop = true;
			this.QuarantineRadioButton.Text = "Quarantine";
			this.QuarantineRadioButton.UseVisualStyleBackColor = true;
			// 
			// RemarksZTextBox
			// 
			this.RemarksZTextBox.BindTo = "Remarks";
			this.RemarksZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 95, true);
			this.RemarksZTextBox.Multiline = true;
			this.RemarksZTextBox.Name = "RemarksZTextBox";
			this.RemarksZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 94, true);
			this.RemarksZTextBox.TabIndex = 3;
			// 
			// RemarksLabel
			// 
			this.RemarksLabel.AutoSize = true;
			this.RemarksLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 73, true);
			this.RemarksLabel.Name = "RemarksLabel";
			this.RemarksLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 13, true);
			this.RemarksLabel.TabIndex = 2;
			this.RemarksLabel.Text = "Remarks:";
			// 
			// ReasonGroupBox
			// 
			this.ReasonGroupBox.Controls.Add(this.CustomsRadioButton);
			this.ReasonGroupBox.Controls.Add(this.RemarksLabel);
			this.ReasonGroupBox.Controls.Add(this.QuarantineRadioButton);
			this.ReasonGroupBox.Controls.Add(this.RemarksZTextBox);
			this.ReasonGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReasonGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 68, true);
			this.ReasonGroupBox.Name = "ReasonGroupBox";
			this.ReasonGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 199, true);
			this.ReasonGroupBox.TabIndex = 4;
			this.ReasonGroupBox.TabStop = false;
			this.ReasonGroupBox.Text = "Reason";
			// 
			// MasterbillNumberDropEdit
			// 
			this.MasterbillNumberDropEdit.BindTo = "MasterbillNumber";
			this.MasterbillNumberDropEdit.BindToList = "MasterbillNumberList";
			this.MasterbillNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 33, true);
			this.MasterbillNumberDropEdit.MaxItemsToShowInDropDown = 4;
			this.MasterbillNumberDropEdit.Name = "MasterbillNumberDropEdit";
			this.MasterbillNumberDropEdit.PreBoundMaxLength = 11;
			this.MasterbillNumberDropEdit.ShowDescriptionBox = false;
			this.MasterbillNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.MasterbillNumberDropEdit.TabIndex = 3;
			// 
			// MasterbillNumberLabel
			// 
			this.MasterbillNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 37, true);
			this.MasterbillNumberLabel.Name = "MasterbillNumberLabel";
			this.MasterbillNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 16, true);
			this.MasterbillNumberLabel.TabIndex = 2;
			this.MasterbillNumberLabel.Text = "Masterbill Number:";
			// 
			// DogHitXRayForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 334, true);
			this.Controls.Add(this.MasterbillNumberLabel);
			this.Controls.Add(this.MasterbillNumberDropEdit);
			this.Controls.Add(this.ReasonGroupBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.TrackingNumberLabel);
			this.Controls.Add(this.TrackingNumberTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "DogHitXRayForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TrackingNumberTextBox, 0);
			this.Controls.SetChildIndex(this.TrackingNumberLabel, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ReasonGroupBox, 0);
			this.Controls.SetChildIndex(this.MasterbillNumberDropEdit, 0);
			this.Controls.SetChildIndex(this.MasterbillNumberLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ReasonGroupBox.ResumeLayout(false);
			this.ReasonGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
