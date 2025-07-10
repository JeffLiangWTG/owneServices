using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class ShipmentHeldLetterForm : ZChildForm
	{
		private Enterprise.ZArchitecture.ZLabel ContactNameLabel;
		private Enterprise.ZArchitecture.ZTextBox ContactNameBoundTextBox;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.ZTextBox ContactNumberBoundTextBox;
		private Enterprise.ZArchitecture.ZLabel ReasonLabel;
		protected Enterprise.ZArchitecture.GUI.ZButton OKButton;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ReasonCodeBoundDropEdit;
		protected Enterprise.ZArchitecture.ZTextBox ReasonTextBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox1;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox QueueForBatchPrintBoundCheckBox;
		protected Enterprise.ZArchitecture.ZLabel HeadingLabel;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox DeliveryMethodGroupBox;

		protected override void InitializeComponent()
		{
			this.ContactNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.ContactNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReasonCodeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReasonLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReasonTextBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HeadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeliveryMethodGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.QueueForBatchPrintBoundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.DeliveryMethodGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 321, true);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(157);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(157);
			// 
			// ContactNameBoundTextBox
			// 
			this.ContactNameBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.ContactNameBoundTextBox.BindTo = "UPSContactName";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.ShipmentHeldLetterBusinessObject)(null)).UPSContactNameInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.ShipmentHeldLetterBusinessObject)(null)).UPSContactName)));
			this.ContactNameBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 48, true);
			this.ContactNameBoundTextBox.Name = "ContactNameBoundTextBox";
			this.ContactNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.ContactNameBoundTextBox.TabIndex = 0;
			this.ContactNameBoundTextBox.Text = "Maria";
			// 
			// ContactNameLabel
			// 
			this.ContactNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 48, true);
			this.ContactNameLabel.Name = "ContactNameLabel";
			this.ContactNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.ContactNameLabel.TabIndex = 2;
			this.ContactNameLabel.Text = "UPS Contact Name:";
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 72, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.zLabel1.TabIndex = 4;
			this.zLabel1.Text = "UPS Contact No.:";
			// 
			// ContactNumberBoundTextBox
			// 
			this.ContactNumberBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.ContactNumberBoundTextBox.BindTo = "UPSContactPhone";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.ShipmentHeldLetterBusinessObject)(null)).UPSContactPhoneInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.ShipmentHeldLetterBusinessObject)(null)).UPSContactPhone)));
			this.ContactNumberBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 72, true);
			this.ContactNumberBoundTextBox.Name = "ContactNumberBoundTextBox";
			this.ContactNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.ContactNumberBoundTextBox.TabIndex = 1;
			this.ContactNumberBoundTextBox.Text = "02 9313 2222";
			// 
			// ReasonCodeBoundDropEdit
			// 
			this.ReasonCodeBoundDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.ReasonCodeBoundDropEdit.BindTo = "ReasonCode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.ShipmentHeldLetterBusinessObject)(null)).ReasonCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.ShipmentHeldLetterBusinessObject)(null)).ReasonCode)));
			this.ReasonCodeBoundDropEdit.BindToList = "ReasonList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.ShipmentHeldLetterBusinessObject)(null)).ReasonList)));
			this.ReasonCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 96, true);
			this.ReasonCodeBoundDropEdit.Name = "ReasonCodeBoundDropEdit";
			this.ReasonCodeBoundDropEdit.PreBoundMaxLength = 5;
			this.ReasonCodeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.ReasonCodeBoundDropEdit.TabIndex = 2;
			// 
			// ReasonLabel
			// 
			this.ReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 96, true);
			this.ReasonLabel.Name = "ReasonLabel";
			this.ReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.ReasonLabel.TabIndex = 6;
			this.ReasonLabel.Text = "Reason for Hold:";
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 288, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 23, true);
			this.OKButton.TabIndex = 5;
			this.OKButton.Text = "OK";
			this.OKButton.Click += new System.EventHandler(this.OnOKButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 288, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 23, true);
			this.CloseButton.TabIndex = 6;
			this.CloseButton.Text = "Cancel";
			this.CloseButton.Click += new System.EventHandler(this.OnCloseButton_Click);
			// 
			// ReasonTextBoundTextBox
			// 
			this.ReasonTextBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.ReasonTextBoundTextBox.BindTo = "ReasonText";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.ShipmentHeldLetterBusinessObject)(null)).ReasonTextInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.ShipmentHeldLetterBusinessObject)(null)).ReasonText)));
			this.ReasonTextBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReasonTextBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 120, true);
			this.ReasonTextBoundTextBox.Multiline = true;
			this.ReasonTextBoundTextBox.Name = "ReasonTextBoundTextBox";
			this.ReasonTextBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 72, true);
			this.ReasonTextBoundTextBox.TabIndex = 3;
			this.ReasonTextBoundTextBox.Text = "Goods exploded, please pick up the debris";
			// 
			// HeadingLabel
			// 
			this.HeadingLabel.IsFontBold = true;
			this.HeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 8, true);
			this.HeadingLabel.Name = "HeadingLabel";
			this.HeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 23, true);
			this.HeadingLabel.TabIndex = 13;
			this.HeadingLabel.Text = "Consignee Customer Notification";
			// 
			// DeliveryMethodGroupBox
			// 
			this.DeliveryMethodGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.DeliveryMethodGroupBox.Controls.Add(this.zCheckBox1);
			this.DeliveryMethodGroupBox.Controls.Add(this.QueueForBatchPrintBoundCheckBox);
			this.DeliveryMethodGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DeliveryMethodGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 200, true);
			this.DeliveryMethodGroupBox.Name = "DeliveryMethodGroupBox";
			this.DeliveryMethodGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 80, true);
			this.DeliveryMethodGroupBox.TabIndex = 4;
			this.DeliveryMethodGroupBox.TabStop = false;
			this.DeliveryMethodGroupBox.Text = "Delivery Method";
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.BindTo = "DeliverByEmailFax";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.ShipmentHeldLetterBusinessObject)(null)).DeliverByEmailFax)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.ShipmentHeldLetterBusinessObject)(null)).DeliverByEmailFaxInfo)));
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 47, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 24, true);
			this.zCheckBox1.TabIndex = 16;
			this.zCheckBox1.Text = "Deliver by Email/Fax (next dialog)";
			// 
			// QueueForBatchPrintBoundCheckBox
			// 
			this.QueueForBatchPrintBoundCheckBox.BindTo = "QueueForBatchPrint";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.ShipmentHeldLetterBusinessObject)(null)).QueueForBatchPrint)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.ShipmentHeldLetterBusinessObject)(null)).QueueForBatchPrintInfo)));
			this.QueueForBatchPrintBoundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.QueueForBatchPrintBoundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 21, true);
			this.QueueForBatchPrintBoundCheckBox.Name = "QueueForBatchPrintBoundCheckBox";
			this.QueueForBatchPrintBoundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 24, true);
			this.QueueForBatchPrintBoundCheckBox.TabIndex = 15;
			this.QueueForBatchPrintBoundCheckBox.Text = "Queue for Batch Print";
			// 
			// ShipmentHeldLetterForm
			// 
			this.AcceptButton = this.OKButton;

			this.CancelButton = this.CloseButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 345, true);
			this.Controls.Add(this.DeliveryMethodGroupBox);
			this.Controls.Add(this.HeadingLabel);
			this.Controls.Add(this.ReasonTextBoundTextBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.ReasonLabel);
			this.Controls.Add(this.ReasonCodeBoundDropEdit);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.ContactNumberBoundTextBox);
			this.Controls.Add(this.ContactNameLabel);
			this.Controls.Add(this.ContactNameBoundTextBox);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.ShipmentHeldLetterBusinessObject";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "ShipmentHeldLetterForm";
			this.Text = "Customer Notification";
			this.Controls.SetChildIndex(this.ContactNameBoundTextBox, 0);
			this.Controls.SetChildIndex(this.ContactNameLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ContactNumberBoundTextBox, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.ReasonCodeBoundDropEdit, 0);
			this.Controls.SetChildIndex(this.ReasonLabel, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ReasonTextBoundTextBox, 0);
			this.Controls.SetChildIndex(this.HeadingLabel, 0);
			this.Controls.SetChildIndex(this.DeliveryMethodGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.DeliveryMethodGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
