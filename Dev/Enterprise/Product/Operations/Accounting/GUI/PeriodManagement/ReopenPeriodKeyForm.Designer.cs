namespace Enterprise.Accounting.GUI.PeriodManagement
{
	partial class ReopenPeriodKeyForm
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
		new void InitializeComponent()
		{
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RequestGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReopenForAdjustmentsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReopenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReopenGeneralLedgerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReopenSubLedgerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.KeyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RequestButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReopenRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.RequestRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.RequestGroupBox.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 407, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.PeriodManagement.ReopenPeriodKeyBusinessObject);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.RequestGroupBox);
			this.MainPanel.Controls.Add(this.TopPanel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 431, true);
			this.MainPanel.TabIndex = 1;
			// 
			// RequestGroupBox
			// 
			this.RequestGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReopenPeriodKeyForm|15167b34-75c2-45a3-a818-c024ffb7b041", "Request / Enter Period Reopen Key");
			this.RequestGroupBox.Controls.Add(this.ReopenForAdjustmentsCheckBox);
			this.RequestGroupBox.Controls.Add(this.ReopenButton);
			this.RequestGroupBox.Controls.Add(this.ReopenGeneralLedgerCheckBox);
			this.RequestGroupBox.Controls.Add(this.ReopenSubLedgerCheckBox);
			this.RequestGroupBox.Controls.Add(this.KeyTextBox);
			this.RequestGroupBox.Controls.Add(this.RequestButton);
			this.RequestGroupBox.Controls.Add(this.ReopenRadioButton);
			this.RequestGroupBox.Controls.Add(this.RequestRadioButton);
			this.RequestGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RequestGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 129, true);
			this.RequestGroupBox.Name = "RequestGroupBox";
			this.RequestGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 302, true);
			this.RequestGroupBox.TabIndex = 1;
			this.RequestGroupBox.TabStop = false;
			// 
			// ReopenForAdjustmentsCheckBox
			// 
			this.ReopenForAdjustmentsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ReopenForAdjustmentsCheckBox, "ReopenForAdjustmentsSelected");
			this.ReopenForAdjustmentsCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReopenPeriodKeyForm|166765c2-7b59-4905-b023-305e11d9e55b", "Reopen For Adjustments");
			this.ReopenForAdjustmentsCheckBox.Enabled = false;
			this.ReopenForAdjustmentsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReopenForAdjustmentsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 216, true);
			this.ReopenForAdjustmentsCheckBox.Name = "ReopenForAdjustmentsCheckBox";
			this.ReopenForAdjustmentsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ReopenForAdjustmentsCheckBox.TabIndex = 6;
			// 
			// ReopenButton
			// 
			this.ReopenButton.AutoSize = true;
			this.ReopenButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReopenPeriodKeyForm|b5c96186-962f-45c3-bba4-c029ecd8a12b", "Reopen Period");
			this.ReopenButton.Enabled = false;
			this.ReopenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 246, true);
			this.ReopenButton.Name = "ReopenButton";
			this.ReopenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.ReopenButton.TabIndex = 7;
			this.ReopenButton.Click += new System.EventHandler(this.ReopenButton_Click);
			// 
			// ReopenGeneralLedgerCheckBox
			// 
			this.ReopenGeneralLedgerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ReopenGeneralLedgerCheckBox, "ReopenGeneralLedgerSelected");
			this.ReopenGeneralLedgerCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReopenPeriodKeyForm|e5b41581-c359-4ee8-a52b-abd7753be8f8", "Reopen General Ledger Period");
			this.ReopenGeneralLedgerCheckBox.Enabled = false;
			this.ReopenGeneralLedgerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReopenGeneralLedgerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 196, true);
			this.ReopenGeneralLedgerCheckBox.Name = "ReopenGeneralLedgerCheckBox";
			this.ReopenGeneralLedgerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ReopenGeneralLedgerCheckBox.TabIndex = 5;
			// 
			// ReopenSubLedgerCheckBox
			// 
			this.ReopenSubLedgerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ReopenSubLedgerCheckBox, "ReopenSubLedgerSelected");
			this.ReopenSubLedgerCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReopenPeriodKeyForm|2667332f-04be-43ff-ba37-0f12e5e571dc", "Reopen Sub Ledger Period");
			this.ReopenSubLedgerCheckBox.Enabled = false;
			this.ReopenSubLedgerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReopenSubLedgerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 176, true);
			this.ReopenSubLedgerCheckBox.Name = "ReopenSubLedgerCheckBox";
			this.ReopenSubLedgerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ReopenSubLedgerCheckBox.TabIndex = 4;
			// 
			// KeyTextBox
			// 
			this.BindingSource.SetBindingMember(this.KeyTextBox, "Key");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PeriodManagement.ReopenPeriodKeyBusinessObject)(null)).Key)));
			this.KeyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReopenPeriodKeyForm|c9eb24a8-9544-435f-8be8-a5953d9f9948", "Reopen Period Key");
			this.KeyTextBox.Enabled = false;
			this.KeyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 150, true);
			this.KeyTextBox.Name = "KeyTextBox";
			this.KeyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 20, true);
			this.KeyTextBox.TabIndex = 3;
			// 
			// RequestButton
			// 
			this.RequestButton.AutoSize = true;
			this.RequestButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReopenPeriodKeyForm|2b620acb-4e91-47b1-99a7-0f9d87655e2c", "Request Reopen Period Key");
			this.RequestButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 78, true);
			this.RequestButton.Name = "RequestButton";
			this.RequestButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 23, true);
			this.RequestButton.TabIndex = 2;
			this.RequestButton.Click += new System.EventHandler(this.RequestButton_Click);
			// 
			// ReopenRadioButton
			// 
			this.ReopenRadioButton.AutoCheck = false;
			this.ReopenRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ReopenRadioButton, "ReopenSelected");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.PeriodManagement.ReopenPeriodKeyBusinessObject)(null)).ReopenSelected)));
			this.ReopenRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReopenPeriodKeyForm|5b587cec-3252-4a58-aba9-1026ed76b175", "I have a  Reopen Period Key");
			this.ReopenRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReopenRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 118, true);
			this.ReopenRadioButton.Name = "ReopenRadioButton";
			this.ReopenRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.ReopenRadioButton.TabIndex = 1;
			this.ReopenRadioButton.TabStop = true;
			this.ReopenRadioButton.CheckedChanged += new System.EventHandler(this.ReopenRadioButton_CheckedChanged);
			// 
			// RequestRadioButton
			// 
			this.RequestRadioButton.AutoCheck = false;
			this.RequestRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RequestRadioButton, "RequestSelected");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.PeriodManagement.ReopenPeriodKeyBusinessObject)(null)).RequestSelected)));
			this.RequestRadioButton.Checked = true;
			this.RequestRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReopenPeriodKeyForm|58fa3df3-e8eb-4ac9-83a2-bd87cb0d7d85", "I want to request a key by creating a Customer Service Request.");
			this.RequestRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RequestRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 41, true);
			this.RequestRadioButton.Name = "RequestRadioButton";
			this.RequestRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.RequestRadioButton.TabIndex = 0;
			this.RequestRadioButton.TabStop = true;
			this.RequestRadioButton.CheckedChanged += new System.EventHandler(this.RequestRadioButton_CheckedChanged);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.MessageLabel);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 129, true);
			this.TopPanel.TabIndex = 0;
			// 
			// MessageLabel
			// 
			this.MessageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 129, true);
			this.MessageLabel.TabIndex = 0;
			// 
			// ReopenPeriodKeyForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 431, true);
			this.Controls.Add(this.MainPanel);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.PeriodManagement.ReopenPeriodKeyBusinessObject);
			this.Name = "ReopenPeriodKeyForm";
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.RequestGroupBox.ResumeLayout(false);
			this.RequestGroupBox.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox RequestGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		private Enterprise.ZArchitecture.ZLabel MessageLabel;
		public Enterprise.ZArchitecture.GUI.ZButton RequestButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ReopenRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton RequestRadioButton;
		private Enterprise.ZArchitecture.ZTextBox KeyTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton ReopenButton;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ReopenGeneralLedgerCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ReopenSubLedgerCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ReopenForAdjustmentsCheckBox;

	}
}
