
namespace Enterprise.Client.UPE.GUI
{
	public partial class RefundProcessingForm
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
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RefundRejectedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.RefundApprovedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.refundProcessingFeeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.RefundActionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RefundDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AmountToBeCreditedGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.writeOffAndRefundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.reasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ammountRefundedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.writeOffCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.remarksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.additionalChargesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.refundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.atFaultDropDownBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RefundRejectedDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanelRefundRejected = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanelRefundApproved = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RefundActionGroupBox.SuspendLayout();
			this.RefundDetailsGroupBox.SuspendLayout();
			this.AmountToBeCreditedGroupBox.SuspendLayout();
			this.RefundRejectedDetailsGroupBox.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.zPanelRefundRejected.SuspendLayout();
			this.zPanelRefundApproved.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 408, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 5;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.Business.ClientRefund);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 7, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 3;
			this.OKButton.Text = "&OK";
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 7, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "&Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.OnCancelButton_Click);
			// 
			// RefundRejectedRadioButton
			// 
			this.RefundRejectedRadioButton.AutoCheck = false;
			this.RefundRejectedRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RefundRejectedRadioButton, "T10_IsRefundRejected");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.ClientRefund)(null)).T10_IsRefundRejected)));
			this.RefundRejectedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RefundRejectedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 20, true);
			this.RefundRejectedRadioButton.Name = "RefundRejectedRadioButton";
			this.RefundRejectedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 17, true);
			this.RefundRejectedRadioButton.TabIndex = 0;
			this.RefundRejectedRadioButton.TabStop = true;
			this.RefundRejectedRadioButton.Text = "Refund Rejected";
			this.RefundRejectedRadioButton.UseVisualStyleBackColor = true;
			this.RefundRejectedRadioButton.CheckedChanged += new System.EventHandler(this.RefundRejectedRadioButton_CheckedChanged);
			// 
			// RefundApprovedRadioButton
			// 
			this.RefundApprovedRadioButton.AutoCheck = false;
			this.RefundApprovedRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RefundApprovedRadioButton, "T10_IsRefundApproved");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.ClientRefund)(null)).T10_IsRefundApproved)));
			this.RefundApprovedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RefundApprovedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 43, true);
			this.RefundApprovedRadioButton.Name = "RefundApprovedRadioButton";
			this.RefundApprovedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 17, true);
			this.RefundApprovedRadioButton.TabIndex = 1;
			this.RefundApprovedRadioButton.TabStop = true;
			this.RefundApprovedRadioButton.Text = "Refund Approved";
			this.RefundApprovedRadioButton.UseVisualStyleBackColor = true;
			this.RefundApprovedRadioButton.CheckedChanged += new System.EventHandler(this.RefundApprovedRadioButton_CheckedChanged);
			// 
			// refundProcessingFeeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.refundProcessingFeeCalcEdit, "T10_RefundProcessingFee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.ClientRefund)(null)).T10_RefundProcessingFee)));
			this.refundProcessingFeeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 18, true);
			this.refundProcessingFeeCalcEdit.Name = "refundProcessingFeeCalcEdit";
			this.refundProcessingFeeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.refundProcessingFeeCalcEdit.TabIndex = 1;
			this.refundProcessingFeeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 22, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 13, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.Text = "Refund Processing Fee:";
			// 
			// RefundActionGroupBox
			// 
			this.RefundActionGroupBox.Controls.Add(this.RefundRejectedRadioButton);
			this.RefundActionGroupBox.Controls.Add(this.RefundApprovedRadioButton);
			this.RefundActionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RefundActionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RefundActionGroupBox.Name = "RefundActionGroupBox";
			this.RefundActionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 408, true);
			this.RefundActionGroupBox.TabIndex = 1;
			this.RefundActionGroupBox.TabStop = false;
			this.RefundActionGroupBox.Text = "Action";
			// 
			// RefundDetailsGroupBox
			// 
			this.RefundDetailsGroupBox.Controls.Add(this.AmountToBeCreditedGroupBox);
			this.RefundDetailsGroupBox.Controls.Add(this.zLabel1);
			this.RefundDetailsGroupBox.Controls.Add(this.refundProcessingFeeCalcEdit);
			this.RefundDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RefundDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RefundDetailsGroupBox.Name = "RefundDetailsGroupBox";
			this.RefundDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 372, true);
			this.RefundDetailsGroupBox.TabIndex = 0;
			this.RefundDetailsGroupBox.TabStop = false;
			this.RefundDetailsGroupBox.Text = "Details";
			// 
			// AmountToBeCreditedGroupBox
			// 
			this.AmountToBeCreditedGroupBox.Controls.Add(this.writeOffAndRefundCalcEdit);
			this.AmountToBeCreditedGroupBox.Controls.Add(this.zLabel9);
			this.AmountToBeCreditedGroupBox.Controls.Add(this.reasonDropEdit);
			this.AmountToBeCreditedGroupBox.Controls.Add(this.ammountRefundedCalcEdit);
			this.AmountToBeCreditedGroupBox.Controls.Add(this.zLabel8);
			this.AmountToBeCreditedGroupBox.Controls.Add(this.writeOffCalcEdit);
			this.AmountToBeCreditedGroupBox.Controls.Add(this.zLabel7);
			this.AmountToBeCreditedGroupBox.Controls.Add(this.zLabel6);
			this.AmountToBeCreditedGroupBox.Controls.Add(this.zLabel5);
			this.AmountToBeCreditedGroupBox.Controls.Add(this.zLabel2);
			this.AmountToBeCreditedGroupBox.Controls.Add(this.remarksTextBox);
			this.AmountToBeCreditedGroupBox.Controls.Add(this.zLabel4);
			this.AmountToBeCreditedGroupBox.Controls.Add(this.additionalChargesCalcEdit);
			this.AmountToBeCreditedGroupBox.Controls.Add(this.zLabel3);
			this.AmountToBeCreditedGroupBox.Controls.Add(this.refundCalcEdit);
			this.AmountToBeCreditedGroupBox.Controls.Add(this.atFaultDropDownBox);
			this.AmountToBeCreditedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 72, true);
			this.AmountToBeCreditedGroupBox.Name = "AmountToBeCreditedGroupBox";
			this.AmountToBeCreditedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 294, true);
			this.AmountToBeCreditedGroupBox.TabIndex = 2;
			this.AmountToBeCreditedGroupBox.TabStop = false;
			this.AmountToBeCreditedGroupBox.Text = "Amount to be Credited:";
			// 
			// writeOffAndRefundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.writeOffAndRefundCalcEdit, "WriteOffAndRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.ClientRefund)(null)).WriteOffAndRefund)));
			this.writeOffAndRefundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 68, true);
			this.writeOffAndRefundCalcEdit.Name = "writeOffAndRefundCalcEdit";
			this.writeOffAndRefundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.writeOffAndRefundCalcEdit.TabIndex = 7;
			this.writeOffAndRefundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel9
			// 
			this.zLabel9.AutoSize = true;
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 71, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 13, true);
			this.zLabel9.TabIndex = 6;
			this.zLabel9.Text = "Write Off && Refund:";
			// 
			// reasonDropEdit
			// 
			this.BindingSource.SetBindingMember(this.reasonDropEdit, "T10_RefundReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.UPE.Business.ClientRefund)(null)).T10_RefundReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.UPE.Business.ClientRefund)(null)).ReasonTypesPairList)));
			this.reasonDropEdit.BindToList = "ReasonTypesPairList";
			this.reasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 108, true);
			this.reasonDropEdit.Name = "reasonDropEdit";
			this.reasonDropEdit.ShowDescriptionBox = false;
			this.reasonDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.reasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.reasonDropEdit.TabIndex = 9;
			// 
			// ammountRefundedCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ammountRefundedCalcEdit, "T10_AmountRefundedToUPS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.ClientRefund)(null)).T10_AmountRefundedToUPS)));
			this.ammountRefundedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(484, 68, true);
			this.ammountRefundedCalcEdit.Name = "ammountRefundedCalcEdit";
			this.ammountRefundedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ammountRefundedCalcEdit.TabIndex = 11;
			this.ammountRefundedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel8
			// 
			this.zLabel8.AutoSize = true;
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 71, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 13, true);
			this.zLabel8.TabIndex = 10;
			this.zLabel8.Text = "Amount Refunded to UPS:";
			// 
			// writeOffCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.writeOffCalcEdit, "T10_WriteOffAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.ClientRefund)(null)).T10_WriteOffAmount)));
			this.writeOffCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 42, true);
			this.writeOffCalcEdit.Name = "writeOffCalcEdit";
			this.writeOffCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.writeOffCalcEdit.TabIndex = 3;
			this.writeOffCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel7
			// 
			this.zLabel7.AutoSize = true;
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 42, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 13, true);
			this.zLabel7.TabIndex = 2;
			this.zLabel7.Text = "Write Off:";
			// 
			// zLabel6
			// 
			this.zLabel6.AutoSize = true;
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 111, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.zLabel6.TabIndex = 8;
			this.zLabel6.Text = "Reason:";
			// 
			// zLabel5
			// 
			this.zLabel5.AutoSize = true;
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 136, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 13, true);
			this.zLabel5.TabIndex = 12;
			this.zLabel5.Text = "At Fault:";
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 159, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 13, true);
			this.zLabel2.TabIndex = 14;
			this.zLabel2.Text = "Remarks:";
			// 
			// remarksTextBox
			// 
			this.BindingSource.SetBindingMember(this.remarksTextBox, "Remarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.ClientRefund)(null)).Remarks)));
			this.remarksTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.remarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 159, true);
			this.remarksTextBox.Multiline = true;
			this.remarksTextBox.Name = "remarksTextBox";
			this.remarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 124, true);
			this.remarksTextBox.TabIndex = 15;
			// 
			// zLabel4
			// 
			this.zLabel4.AutoSize = true;
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 18, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 13, true);
			this.zLabel4.TabIndex = 0;
			this.zLabel4.Text = "Refund:";
			// 
			// additionalChargesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.additionalChargesCalcEdit, "T10_AdditionalCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.ClientRefund)(null)).T10_AdditionalCharges)));
			this.additionalChargesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(484, 15, true);
			this.additionalChargesCalcEdit.Name = "additionalChargesCalcEdit";
			this.additionalChargesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.additionalChargesCalcEdit.TabIndex = 5;
			this.additionalChargesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel3
			// 
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 9, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 31, true);
			this.zLabel3.TabIndex = 4;
			this.zLabel3.Text = "Additional Charges for Invoicing to Customer:";
			// 
			// refundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.refundCalcEdit, "T10_RefundAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.ClientRefund)(null)).T10_RefundAmount)));
			this.refundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 15, true);
			this.refundCalcEdit.Name = "refundCalcEdit";
			this.refundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.refundCalcEdit.TabIndex = 1;
			this.refundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// atFaultDropDownBox
			// 
			this.BindingSource.SetBindingMember(this.atFaultDropDownBox, "T10_GS_NKAtFaultUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.UPE.Business.ClientRefund)(null)).T10_GS_NKAtFaultUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.UPE.Business.ClientRefund)(null)).AtFaultList)));
			this.atFaultDropDownBox.BindToList = "AtFaultList";
			this.atFaultDropDownBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 133, true);
			this.atFaultDropDownBox.Name = "atFaultDropDownBox";
			this.atFaultDropDownBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.atFaultDropDownBox.TabIndex = 13;
			// 
			// RefundRejectedDetailsGroupBox
			// 
			this.RefundRejectedDetailsGroupBox.Controls.Add(this.DetailsTextBox);
			this.RefundRejectedDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RefundRejectedDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RefundRejectedDetailsGroupBox.Name = "RefundRejectedDetailsGroupBox";
			this.RefundRejectedDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 372, true);
			this.RefundRejectedDetailsGroupBox.TabIndex = 2;
			this.RefundRejectedDetailsGroupBox.TabStop = false;
			this.RefundRejectedDetailsGroupBox.Text = "Details";
			// 
			// DetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DetailsTextBox, "T10_RefundRejectedDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.ClientRefund)(null)).T10_RefundRejectedDetails)));
			this.DetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 19, true);
			this.DetailsTextBox.Multiline = true;
			this.DetailsTextBox.Name = "DetailsTextBox";
			this.DetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 297, true);
			this.DetailsTextBox.TabIndex = 0;
			// 
			// zPanel1
			// 
			this.zPanel1.AutoSize = true;
			this.zPanel1.Controls.Add(this.cancelButton);
			this.zPanel1.Controls.Add(this.OKButton);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 372, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 36, true);
			this.zPanel1.TabIndex = 6;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.RefundActionGroupBox);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Left;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 408, true);
			this.zPanel2.TabIndex = 7;
			// 
			// zPanelRefundRejected
			// 
			this.zPanelRefundRejected.Controls.Add(this.RefundRejectedDetailsGroupBox);
			this.zPanelRefundRejected.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanelRefundRejected.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 0, true);
			this.zPanelRefundRejected.Name = "zPanelRefundRejected";
			this.zPanelRefundRejected.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 372, true);
			this.zPanelRefundRejected.TabIndex = 8;
			// 
			// zPanelRefundApproved
			// 
			this.zPanelRefundApproved.Controls.Add(this.RefundDetailsGroupBox);
			this.zPanelRefundApproved.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanelRefundApproved.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 0, true);
			this.zPanelRefundApproved.Name = "zPanelRefundApproved";
			this.zPanelRefundApproved.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 372, true);
			this.zPanelRefundApproved.TabIndex = 9;
			// 
			// RefundProcessingForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 432, true);
			this.Controls.Add(this.zPanelRefundApproved);
			this.Controls.Add(this.zPanelRefundRejected);
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.zPanel2);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceType = typeof(Enterprise.Client.UPE.Business.ClientRefund);
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.ClientRefund";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "RefundProcessingForm";
			this.Text = "Process Refund";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel2, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.zPanelRefundRejected, 0);
			this.Controls.SetChildIndex(this.zPanelRefundApproved, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RefundActionGroupBox.ResumeLayout(false);
			this.RefundActionGroupBox.PerformLayout();
			this.RefundDetailsGroupBox.ResumeLayout(false);
			this.RefundDetailsGroupBox.PerformLayout();
			this.AmountToBeCreditedGroupBox.ResumeLayout(false);
			this.AmountToBeCreditedGroupBox.PerformLayout();
			this.RefundRejectedDetailsGroupBox.ResumeLayout(false);
			this.RefundRejectedDetailsGroupBox.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel2.ResumeLayout(false);
			this.zPanelRefundRejected.ResumeLayout(false);
			this.zPanelRefundApproved.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton OKButton;
		internal Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton RefundRejectedRadioButton;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton RefundApprovedRadioButton;
		private Enterprise.ZArchitecture.ZCalcEdit refundProcessingFeeCalcEdit;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.GUI.ZGroupBox RefundActionGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox RefundDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AmountToBeCreditedGroupBox;
		private Enterprise.ZArchitecture.ZLabel zLabel6;
		private Enterprise.ZArchitecture.ZLabel zLabel5;
		private Enterprise.ZArchitecture.ZLabel zLabel2;
		private Enterprise.ZArchitecture.ZTextBox remarksTextBox;
		private Enterprise.ZArchitecture.ZLabel zLabel4;
		private Enterprise.ZArchitecture.ZCalcEdit additionalChargesCalcEdit;
		private Enterprise.ZArchitecture.ZLabel zLabel3;
		private Enterprise.ZArchitecture.ZCalcEdit refundCalcEdit;
		private Enterprise.ZArchitecture.ZLabel zLabel8;
		private Enterprise.ZArchitecture.ZCalcEdit writeOffCalcEdit;
		private Enterprise.ZArchitecture.ZLabel zLabel7;
		private Enterprise.ZArchitecture.ZCalcEdit ammountRefundedCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit reasonDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit writeOffAndRefundCalcEdit;
		private Enterprise.ZArchitecture.ZLabel zLabel9;
		private Enterprise.ZArchitecture.GUI.ZGroupBox RefundRejectedDetailsGroupBox;
		private Enterprise.ZArchitecture.ZTextBox DetailsTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit atFaultDropDownBox;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel1;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel2;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanelRefundRejected;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanelRefundApproved;
	}
}
