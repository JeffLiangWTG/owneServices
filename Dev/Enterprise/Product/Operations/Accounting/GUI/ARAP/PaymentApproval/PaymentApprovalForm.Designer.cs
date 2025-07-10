using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class PaymentApprovalForm
	{


		#region Windows Form Designer generated code

		protected ZGroupBox PaymentDetailsGroupBox;
		protected ZTextBox PaymentNoTextBox;
		protected ZDateEdit PostDateEdit;
		protected ZDateEdit InvoiceDateEdit;
		protected ZTextBox DescriptionTextBox;
		protected ZGuidFindBox OrganizationGuidFindBox;
		protected ZGroupBox BankDetailsGroupBox;
		protected ZDropEdit PaymentTypeDropEdit;
		protected ZGuidFindBox BankAccountGuidFindBox;
		protected ZGuidFindBox ChequeBookGuidFindBox;
		protected ZTextBox ChequeNoTextBox;
		protected ZGroupBox PaymentAmountGroupBox;
		protected ZCalcFindBox LocalAmountCalcFindBox;
		protected ZCalcFindBox PaymentAmountCalcFindBox;
		protected ZExchangeRateControl ExchangeRateControl;
		protected ZTemplateTabControl zTabControl1;
		protected ZLogsTabPage EventTabPage;
		protected ZTabPage PaymentTabPage;
		protected ZCalcEdit AH_NumberOfSupportingDocumentsCalcEdit;
		private IContainer components;
		private ZWorkflowTabPage WorkflowTabPage;
		private ZGroupBox DealGroupBox;
		private ZTextBox DealProviderTextBox;
		private ZTextBox DealReferenceTextBox;
		private ZTextBox DealStatusTextBox;
		private ZTextBox DealErrorTextBox;
		private ZDateEdit DealSubmittedDateEdit;
		private ZDateEdit DealResponseDateEdit;
		private ZCalcFindBox DealCostCalcFindBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new Container();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			this.AH_NumberOfSupportingDocumentsCalcEdit = new ZCalcEdit();
			this.BankDetailsGroupBox = new ZGroupBox();
			this.PaymentReasonDropEdit = new ZDropEdit();
			this.ServiceProviderLabel = new ZLabel();
			this.CardSecurityCodeTextBox = new ZTextBox();
			this.AutoAllocateZLabel = new ZLabel();
			this.ProviderLogoPictureBox = new ZPictureBox();
			this.AutoPrintZLabel = new ZLabel();
			this.ChequeNoTextBox = new ZTextBox();
			this.ChequeBookGuidFindBox = new ZGuidFindBox();
			this.DisclaimerMessageLabel = new ZLabel();
			this.BankAccountGuidFindBox = new ZGuidFindBox();
			this.PaymentTypeDropEdit = new ZDropEdit();
			this.PaymentAmountGroupBox = new ZGroupBox();
			this.CheckEPayRateButton = new ZButton();
			this.LocalAmountCalcFindBox = new ZCalcFindBox();
			this.LearnMoreButton = new ZButton();
			this.PaymentAmountCalcFindBox = new ZCalcFindBox();
			this.ExchangeRateControl = new ZExchangeRateControl();
			this.zTabControl1 = new ZTemplateTabControl();
			this.PaymentTabPage = new ZTabPage();
			this.BankAndEPaymentTabControl = new ZTabControl();
			this.BankDetailsTabPage = new ZTabPage();
			this.EPaymentTabPage = new ZTabPage();
			this.EQuoteGroupBox = new ZGroupBox();
			this.AcceptQuoteButton = new ZButton();
			this.RefreshEPaymentButton = new ZButton();
			this.EPaymentGrid = new ZGrid();
			this.DealGroupBox = new ZGroupBox();
			this.DealProviderTextBox = new ZTextBox();
			this.DealReferenceTextBox = new ZTextBox();
			this.DealStatusTextBox = new ZTextBox();
			this.DealSubmittedDateEdit = new ZDateEdit();
			this.DealResponseDateEdit = new ZDateEdit();
			this.DealCostCalcFindBox = new ZCalcFindBox();
			this.DealErrorTextBox = new ZTextBox();
			this.PaymentDetailsGroupBox = new ZGroupBox();
			this.PaymentNoTextBox = new ZTextBox();
			this.PostDateEdit = new ZDateEdit();
			this.InvoiceDateEdit = new ZDateEdit();
			this.DescriptionTextBox = new ZTextBox();
			this.OrganizationGuidFindBox = new ZGuidFindBox();
			this.AddressWithContactControl = new PaymentAddressWithContactControl();
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.EventTabPage = new ZLogsTabPage();
			this.PaymentDetailButton = new ZButton();
			this.CloseButton = new ZButton();
			this.PostWithoutMatchingButton = new ZButton();
			this.SaveAsDraftButton = new ZButton();
			this.ProcessEPaymentButton = new ZButton();
			this.SubmitForApprovalButton = new ZButton();
			this.ApproveForPostingButton = new ZButton();
			this.StatusDropEdit = new ZDropEdit();
			this.FundingBankAccountFindBox = new ZGuidFindBox();
			this.FundingCurrencyCodeFindBox = new ZCodeFindBox();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BankDetailsGroupBox.SuspendLayout();
			this.PaymentReasonDropEdit.SuspendLayout();
			((ISupportInitialize)(this.ProviderLogoPictureBox)).BeginInit();
			this.ChequeBookGuidFindBox.SuspendLayout();
			this.BankAccountGuidFindBox.SuspendLayout();
			this.PaymentTypeDropEdit.SuspendLayout();
			this.PaymentAmountGroupBox.SuspendLayout();
			this.LocalAmountCalcFindBox.SuspendLayout();
			this.PaymentAmountCalcFindBox.SuspendLayout();
			this.ExchangeRateControl.SuspendLayout();
			this.zTabControl1.SuspendLayout();
			this.PaymentTabPage.SuspendLayout();
			this.BankAndEPaymentTabControl.SuspendLayout();
			this.BankDetailsTabPage.SuspendLayout();
			this.EPaymentTabPage.SuspendLayout();
			this.EQuoteGroupBox.SuspendLayout();
			((ISupportInitialize)(this.EPaymentGrid)).BeginInit();
			this.EPaymentGrid.SuspendLayout();
			this.DealGroupBox.SuspendLayout();
			this.DealSubmittedDateEdit.SuspendLayout();
			this.DealResponseDateEdit.SuspendLayout();
			this.DealCostCalcFindBox.SuspendLayout();
			this.PaymentDetailsGroupBox.SuspendLayout();
			this.PostDateEdit.SuspendLayout();
			this.InvoiceDateEdit.SuspendLayout();
			this.OrganizationGuidFindBox.SuspendLayout();
			this.AddressWithContactControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.FundingBankAccountFindBox.SuspendLayout();
			this.FundingCurrencyCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 710, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(942, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(282);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(282);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(PaymentApprovalWithAuthorisation);
			// 
			// AH_NumberOfSupportingDocumentsCalcEdit
			// 
			this.AH_NumberOfSupportingDocumentsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_NumberOfSupportingDocumentsCalcEdit, "AH_NumberOfSupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PaymentApprovalWithAuthorisation)(null)).AH_NumberOfSupportingDocuments)));
			this.AH_NumberOfSupportingDocumentsCalcEdit.CaptionResourceString = null;
			this.AH_NumberOfSupportingDocumentsCalcEdit.DecimalPlaces = 2;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 40, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.Name = "AH_NumberOfSupportingDocumentsCalcEdit";
			this.AH_NumberOfSupportingDocumentsCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 15, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.TabIndex = 4;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Text = "0";
			this.AH_NumberOfSupportingDocumentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BankDetailsGroupBox
			// 
			this.BankDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|b21707ca-5948-4959-8d19-5ec8eb9fe7e3", "Bank Details");
			this.BankDetailsGroupBox.Controls.Add(this.PaymentReasonDropEdit);
			this.BankDetailsGroupBox.Controls.Add(this.ServiceProviderLabel);
			this.BankDetailsGroupBox.Controls.Add(this.CardSecurityCodeTextBox);
			this.BankDetailsGroupBox.Controls.Add(this.AutoAllocateZLabel);
			this.BankDetailsGroupBox.Controls.Add(this.ProviderLogoPictureBox);
			this.BankDetailsGroupBox.Controls.Add(this.AutoPrintZLabel);
			this.BankDetailsGroupBox.Controls.Add(this.ChequeNoTextBox);
			this.BankDetailsGroupBox.Controls.Add(this.ChequeBookGuidFindBox);
			this.BankDetailsGroupBox.Controls.Add(this.DisclaimerMessageLabel);
			this.BankDetailsGroupBox.Controls.Add(this.BankAccountGuidFindBox);
			this.BankDetailsGroupBox.Controls.Add(this.PaymentTypeDropEdit);
			this.BankDetailsGroupBox.Controls.Add(this.FundingCurrencyCodeFindBox);
			this.BankDetailsGroupBox.Controls.Add(this.FundingBankAccountFindBox);
			this.BankDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.BankDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BankDetailsGroupBox.Name = "BankDetailsGroupBox";
			this.BankDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 177, true);
			this.BankDetailsGroupBox.TabIndex = 1;
			this.BankDetailsGroupBox.TabStop = false;
			// 
			// PaymentReasonDropEdit
			// 
			this.PaymentReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentReasonDropEdit, "AV_EPaymentReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentApprovalWithAuthorisation)(null)).AV_EPaymentReasonCode)));
			this.PaymentReasonDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9f972663-f49e-490b-a1e7-8402ad9237c5", "Payment Reason");
			this.PaymentReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 120, true);
			this.PaymentReasonDropEdit.Name = "PaymentReasonDropEdit";
			this.PaymentReasonDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 15, true);
			this.PaymentReasonDropEdit.TabIndex = 11;
			// 
			// ServiceProviderLabel
			// 
			this.ServiceProviderLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c5a96e75-e7a5-438a-86f3-37d8a0a4b311", "Service Provider");
			this.ServiceProviderLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ServiceProviderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(714, 89, true);
			this.ServiceProviderLabel.Name = "ServiceProviderLabel";
			this.ServiceProviderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.ServiceProviderLabel.TabIndex = 7;
			// 
			// CardSecurityCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CardSecurityCodeTextBox, "CreditCardSecurityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).CreditCardSecurityCode)));
			this.CardSecurityCodeTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|639a322f-e75b-40b5-a46c-3acc4cae23a5", "Security Code");
			this.CardSecurityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 120, true);
			this.CardSecurityCodeTextBox.Name = "CardSecurityCodeTextBox";
			this.CardSecurityCodeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CardSecurityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 15, true);
			this.CardSecurityCodeTextBox.TabIndex = 10;
			// 
			// AutoAllocateZLabel
			// 
			this.AutoAllocateZLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoAllocateZLabel, "AV_Calc_ChequeNumberIsAutoAllocatedLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).AV_Calc_ChequeNumberIsAutoAllocatedLabel)));
			this.AutoAllocateZLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|c8ff4a15-9cd3-4312-9851-9055d2b52b16", "Auto Allocate");
			this.AutoAllocateZLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AutoAllocateZLabel.ForeColor = System.Drawing.Color.Red;
			this.AutoAllocateZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 100, true);
			this.AutoAllocateZLabel.Name = "AutoAllocateZLabel";
			this.AutoAllocateZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.AutoAllocateZLabel.TabIndex = 9;
			// 
			// ProviderLogoPictureBox
			// 
			this.ProviderLogoPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(807, 84, true);
			this.ProviderLogoPictureBox.Name = "ProviderLogoPictureBox";
			this.ProviderLogoPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 33, true);
			this.ProviderLogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.ProviderLogoPictureBox.TabIndex = 6;
			this.ProviderLogoPictureBox.TabStop = false;
			this.ProviderLogoPictureBox.Click += new EventHandler(this.ProviderLogoPictureBox_Click);
			// 
			// AutoPrintZLabel
			// 
			this.AutoPrintZLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoPrintZLabel, "AV_Calc_ChequeIsAutoPrintedLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).AV_Calc_ChequeIsAutoPrintedLabel)));
			this.AutoPrintZLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|e3a79c85-1aff-4113-b281-384b5104e96f", "Auto Print");
			this.AutoPrintZLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AutoPrintZLabel.ForeColor = System.Drawing.Color.Red;
			this.AutoPrintZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 76, true);
			this.AutoPrintZLabel.Name = "AutoPrintZLabel";
			this.AutoPrintZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 16, true);
			this.AutoPrintZLabel.TabIndex = 8;
			// 
			// ChequeNoTextBox
			// 
			this.ChequeNoTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ChequeNoTextBox, "AV_ChequeOrReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).AV_ChequeOrReference)));
			this.ChequeNoTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|bc9d7af5-26c4-4c22-a7d8-51cd4d4cedda", "Check #:", "Check Num.:", "Check Number:", "");
			this.ChequeNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 97, true);
			this.ChequeNoTextBox.Name = "ChequeNoTextBox";
			this.ChequeNoTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ChequeNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 15, true);
			this.ChequeNoTextBox.TabIndex = 7;
			// 
			// ChequeBookGuidFindBox
			// 
			this.ChequeBookGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChequeBookGuidFindBox, "AV_AK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PaymentApprovalWithAuthorisation)(null)).AV_AK)));
			this.ChequeBookGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 73, true);
			this.ChequeBookGuidFindBox.Name = "ChequeBookGuidFindBox";
			this.ChequeBookGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ChequeBookGuidFindBox.ParentType = null;
			this.ChequeBookGuidFindBox.PopupCaption = null;
			this.ChequeBookGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 15, true);
			this.ChequeBookGuidFindBox.TabIndex = 5;
			// 
			// DisclaimerMessageLabel
			// 
			this.DisclaimerMessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("74412775-5fbc-457e-abf7-abfd13bcbeaf", "E-Payment Disclaimer Message");
			this.DisclaimerMessageLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DisclaimerMessageLabel.ForeColor = System.Drawing.SystemColors.Highlight;
			this.DisclaimerMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 14, true);
			this.DisclaimerMessageLabel.Name = "DisclaimerMessageLabel";
			this.DisclaimerMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(357, 66, true);
			this.DisclaimerMessageLabel.TabIndex = 3;
			// 
			// BankAccountGuidFindBox
			// 
			this.BankAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BankAccountGuidFindBox, "AV_AB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PaymentApprovalWithAuthorisation)(null)).AV_AB)));
			this.BankAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 49, true);
			this.BankAccountGuidFindBox.Name = "BankAccountGuidFindBox";
			this.BankAccountGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BankAccountGuidFindBox.ParentType = null;
			this.BankAccountGuidFindBox.PopupCaption = null;
			this.BankAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 15, true);
			this.BankAccountGuidFindBox.TabIndex = 3;
			// 
			// PaymentTypeDropEdit
			// 
			this.PaymentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTypeDropEdit, "AV_PaymentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentApprovalWithAuthorisation)(null)).AV_PaymentType)));
			this.PaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 25, true);
			this.PaymentTypeDropEdit.Name = "PaymentTypeDropEdit";
			this.PaymentTypeDropEdit.PreBoundMaxLength = 5;
			this.PaymentTypeDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 15, true);
			this.PaymentTypeDropEdit.TabIndex = 1;
			// 
			// PaymentAmountGroupBox
			// 
			this.PaymentAmountGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|6a7ca8e1-fbb8-453d-9da0-41458ee61300", "Payment Amount");
			this.PaymentAmountGroupBox.Controls.Add(this.CheckEPayRateButton);
			this.PaymentAmountGroupBox.Controls.Add(this.LocalAmountCalcFindBox);
			this.PaymentAmountGroupBox.Controls.Add(this.LearnMoreButton);
			this.PaymentAmountGroupBox.Controls.Add(this.PaymentAmountCalcFindBox);
			this.PaymentAmountGroupBox.Controls.Add(this.ExchangeRateControl);
			this.PaymentAmountGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 186, true);
			this.PaymentAmountGroupBox.Name = "PaymentAmountGroupBox";
			this.PaymentAmountGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 118, true);
			this.PaymentAmountGroupBox.TabIndex = 2;
			this.PaymentAmountGroupBox.TabStop = false;
			// 
			// CheckEPayRateButton
			// 
			this.CheckEPayRateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8e222354-dbab-418c-bae4-4e1fda0dd674", "Check Ex Rate for E-Payment");
			this.CheckEPayRateButton.IsCaptionOverridden = false;
			this.CheckEPayRateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 78, true);
			this.CheckEPayRateButton.Name = "CheckEPayRateButton";
			this.CheckEPayRateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CheckEPayRateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 23, true);
			this.CheckEPayRateButton.TabIndex = 7;
			this.CheckEPayRateButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.CheckEPayRateButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CheckEPayRateButton.ToolTipCaption = null;
			this.CheckEPayRateButton.UseVisualStyleBackColor = true;
			this.CheckEPayRateButton.Click += new EventHandler(this.CheckEPayRateButton_Click);
			// 
			// LocalAmountCalcFindBox
			// 
			this.LocalAmountCalcFindBox.AllowDrop = true;
			this.LocalAmountCalcFindBox.BindToAmount = "AV_Calc_LocalAmount";
			this.LocalAmountCalcFindBox.BindToUnit = "AV_Calc_LocalCurrency";
			this.LocalAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|46b20573-f532-4b3b-8d14-6ca3d62c87a4", "Local Amount");
			this.LocalAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LocalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 47, true);
			this.LocalAmountCalcFindBox.Name = "LocalAmountCalcFindBox";
			this.LocalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.LocalAmountCalcFindBox.TabIndex = 6;
			// 
			// LearnMoreButton
			// 
			this.LearnMoreButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6eb90c39-05d7-4a51-9931-1b07a2b6c9a3", "Learn More");
			this.LearnMoreButton.IsCaptionOverridden = false;
			this.LearnMoreButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 78, true);
			this.LearnMoreButton.Name = "LearnMoreButton";
			this.LearnMoreButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.LearnMoreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 23, true);
			this.LearnMoreButton.TabIndex = 5;
			this.LearnMoreButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.LearnMoreButton.ToolTipCaption = null;
			this.LearnMoreButton.UseVisualStyleBackColor = true;
			this.LearnMoreButton.Click += new EventHandler(this.LearnMoreButton_Click);
			// 
			// PaymentAmountCalcFindBox
			// 
			this.PaymentAmountCalcFindBox.AllowDrop = true;
			this.PaymentAmountCalcFindBox.BindToAmount = "AV_Amount";
			this.PaymentAmountCalcFindBox.BindToUnit = "OSCurrencyForDisplay";
			this.PaymentAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.PaymentAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 49, true);
			this.PaymentAmountCalcFindBox.Name = "PaymentAmountCalcFindBox";
			this.PaymentAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.PaymentAmountCalcFindBox.TabIndex = 3;
			// 
			// ExchangeRateControl
			// 
			this.ExchangeRateControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExchangeRateControl, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((PaymentApprovalWithAuthorisation)(null)).ExchangeRate)));
			this.ExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|073ef7a8-dff1-4519-a9a4-38efe53812a2", "Exchange");
			this.ExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 25, true);
			this.ExchangeRateControl.Name = "ExchangeRateControl";
			this.ExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ExchangeRateControl.TabIndex = 1;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Controls.Add(this.PaymentTabPage);
			this.zTabControl1.Controls.Add(this.WorkflowTabPage);
			this.zTabControl1.Controls.Add(this.zStmNoteTabPage1);
			this.zTabControl1.Controls.Add(this.EventTabPage);
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 668, true);
			this.zTabControl1.TabIndex = 0;
			// 
			// PaymentTabPage
			// 
			this.PaymentTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|b2e7f2ea-90c7-448b-9b94-f2bcdbceb13c", "Payment");
			this.PaymentTabPage.Controls.Add(this.BankAndEPaymentTabControl);
			this.PaymentTabPage.Controls.Add(this.PaymentDetailsGroupBox);
			this.PaymentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.PaymentTabPage.Name = "PaymentTabPage";
			this.PaymentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 645, true);
			this.PaymentTabPage.TabIndex = 0;
			// 
			// BankAndEPaymentTabControl
			// 
			this.BankAndEPaymentTabControl.Controls.Add(this.BankDetailsTabPage);
			this.BankAndEPaymentTabControl.Controls.Add(this.EPaymentTabPage);
			this.BankAndEPaymentTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 188, true);
			this.BankAndEPaymentTabControl.Name = "BankAndEPaymentTabControl";
			this.BankAndEPaymentTabControl.SelectedIndex = 0;
			this.BankAndEPaymentTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 455, true);
			this.BankAndEPaymentTabControl.TabIndex = 3;
			// 
			// BankDetailsTabPage
			// 
			this.BankDetailsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.BankDetailsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c5d663ca-825c-4bff-8823-4fcb9fc82a74", "Bank Details");
			this.BankDetailsTabPage.Controls.Add(this.BankDetailsGroupBox);
			this.BankDetailsTabPage.Controls.Add(this.PaymentAmountGroupBox);
			this.BankDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.BankDetailsTabPage.Name = "BankDetailsTabPage";
			this.BankDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.BankDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 394, true);
			this.BankDetailsTabPage.TabIndex = 0;
			// 
			// EPaymentTabPage
			// 
			this.EPaymentTabPage.AutoScroll = true;
			this.EPaymentTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.EPaymentTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2ae8a01c-9c5a-474b-b44e-e720c040aa11", "E-Payment Processing");
			this.EPaymentTabPage.Controls.Add(this.EQuoteGroupBox);
			this.EPaymentTabPage.Controls.Add(this.DealGroupBox);
			this.EPaymentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.EPaymentTabPage.Name = "EPaymentTabPage";
			this.EPaymentTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EPaymentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 432, true);
			this.EPaymentTabPage.TabIndex = 1;
			// 
			// EQuoteGroupBox
			// 
			this.EQuoteGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4ef53fc8-12bf-43af-86cb-a2c2b218c1a7", "E-Quote");
			this.EQuoteGroupBox.Controls.Add(this.AcceptQuoteButton);
			this.EQuoteGroupBox.Controls.Add(this.RefreshEPaymentButton);
			this.EQuoteGroupBox.Controls.Add(this.EPaymentGrid);
			this.EQuoteGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EQuoteGroupBox.Name = "EQuoteGroupBox";
			this.EQuoteGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 219, true);
			this.EQuoteGroupBox.TabIndex = 2;
			this.EQuoteGroupBox.TabStop = false;
			// 
			// AcceptQuoteButton
			// 
			this.AcceptQuoteButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("64499063-6756-4272-a9e2-8382acf29eb2", "Accept Quote");
			this.AcceptQuoteButton.IsCaptionOverridden = false;
			this.AcceptQuoteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 21, true);
			this.AcceptQuoteButton.Name = "AcceptQuoteButton";
			this.AcceptQuoteButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AcceptQuoteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 23, true);
			this.AcceptQuoteButton.TabIndex = 1;
			this.AcceptQuoteButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AcceptQuoteButton.ToolTipCaption = null;
			this.AcceptQuoteButton.UseVisualStyleBackColor = true;
			this.AcceptQuoteButton.Click += new EventHandler(this.AcceptQuoteButton_Click);
			// 
			// RefreshEPaymentButton
			// 
			this.RefreshEPaymentButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8e01a9d3-95f6-45f2-a8e4-be1361c452aa", "Refresh");
			this.RefreshEPaymentButton.IsCaptionOverridden = false;
			this.RefreshEPaymentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 21, true);
			this.RefreshEPaymentButton.Name = "RefreshEPaymentButton";
			this.RefreshEPaymentButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RefreshEPaymentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 23, true);
			this.RefreshEPaymentButton.TabIndex = 0;
			this.RefreshEPaymentButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RefreshEPaymentButton.ToolTipCaption = null;
			this.RefreshEPaymentButton.UseVisualStyleBackColor = true;
			this.RefreshEPaymentButton.Click += new EventHandler(this.RefreshEPaymentButton_Click);
			// 
			// EPaymentGrid
			// 
			this.EPaymentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EPaymentGrid, "PaymentQuotes_ForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).ProviderCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).ToCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).FromCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).ToAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).ExchangeRateInverted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).FromAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).FeeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).ErrorDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).ProviderReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).LastResponseReceivedLocalTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).InternalReference)));
			this.EPaymentGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f2f5af18-e4a7-45d4-a85b-7bc577a03b04", "Status");
			zTextBoxColumnStyleInfo1.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a47b651d-00b8-4f4d-97aa-2b35632e8f6e", "Provider");
			zTextBoxColumnStyleInfo2.ColumnName = "ProviderCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a2023a26-5991-4d9f-8dc3-6b0bf8368570", "Payment Currency");
			zTextBoxColumnStyleInfo3.ColumnName = "ToCurrency";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3743ba88-4229-4960-be06-5f970b3de56c", "Payment Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "ToAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("aa50698b-e406-4aab-b829-c652e3c8f428", "Ex Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "ExchangeRate";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4d554529-80f8-44e7-ae9d-35b0c2d0aa53", "Inverse Ex Rate");
			zCalcEditColumnStyleInfo3.ColumnName = "ExchangeRateInverted";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("524b8837-d236-4638-bf27-1b33dfa26fa0", "Funding Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "FromAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f8c6218a-2124-4898-beb4-be8c465d169e", "Fee");
			zCalcEditColumnStyleInfo5.ColumnName = "FeeAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("99bc9968-423f-4df4-a7fc-cecd2ea69e70", "Error");
			zTextBoxColumnStyleInfo4.ColumnName = "ErrorDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("67e3ac13-b3b4-4179-9e23-1c678621bbf1", "Provider Ref");
			zTextBoxColumnStyleInfo5.ColumnName = "ProviderReference";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f0705dd0-2b05-4ebd-9afd-25e5ac5487d8", "Received");
			zDateEditColumnStyleInfo1.ColumnName = "LastResponseReceivedLocalTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("24a376f1-6c90-49ca-a487-bb91268b6c0f", "Quote #");
			zTextBoxColumnStyleInfo6.ColumnName = "InternalReference";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BC86C7D6-F465-4FA2-8E0B-FEFF2EA30BC8", "Funding Currency");
			zTextBoxColumnStyleInfo7.ColumnName = "FromCurrency";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.EPaymentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EPaymentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EPaymentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EPaymentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EPaymentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EPaymentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EPaymentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.EPaymentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.EPaymentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.EPaymentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EPaymentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EPaymentGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EPaymentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.EPaymentGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.EPaymentGrid.GridId = "5f1a9107-0003-4f46-9b96-c1bc4cc18221";
			this.EPaymentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EPaymentGrid.LayoutKey = "EPaymentGrid";
			this.EPaymentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 54, true);
			this.EPaymentGrid.Name = "EPaymentGrid";
			this.EPaymentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(902, 164, true);
			this.EPaymentGrid.TabIndex = 2;
			// 
			// DealGroupBox
			// 
			this.DealGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1c1b5969-0036-439a-b4f3-629b3be93c96", "E-Payment");
			this.DealGroupBox.Controls.Add(this.DealProviderTextBox);
			this.DealGroupBox.Controls.Add(this.DealReferenceTextBox);
			this.DealGroupBox.Controls.Add(this.DealStatusTextBox);
			this.DealGroupBox.Controls.Add(this.DealSubmittedDateEdit);
			this.DealGroupBox.Controls.Add(this.DealResponseDateEdit);
			this.DealGroupBox.Controls.Add(this.DealCostCalcFindBox);
			this.DealGroupBox.Controls.Add(this.DealErrorTextBox);
			this.DealGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 231, true);
			this.DealGroupBox.Name = "DealGroupBox";
			this.DealGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(904, 104, true);
			this.DealGroupBox.TabIndex = 2;
			this.DealGroupBox.TabStop = false;
			// 
			// DealProviderTextBox
			// 
			this.BindingSource.SetBindingMember(this.DealProviderTextBox, "DealProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).DealProvider)));
			this.DealProviderTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1b25e24f-9562-45a1-ab87-b786809edd21", "Provider");
			this.DealProviderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 25, true);
			this.DealProviderTextBox.Name = "DealProviderTextBox";
			this.DealProviderTextBox.ReadOnly = true;
			this.DealProviderTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DealProviderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 15, true);
			this.DealProviderTextBox.TabIndex = 1;
			// 
			// DealReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.DealReferenceTextBox, "DealProviderReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).DealProviderReference)));
			this.DealReferenceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7e44115e-f840-4f58-8f2a-abb721005ab7", "Provider Reference");
			this.DealReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 49, true);
			this.DealReferenceTextBox.Name = "DealReferenceTextBox";
			this.DealReferenceTextBox.ReadOnly = true;
			this.DealReferenceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DealReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 15, true);
			this.DealReferenceTextBox.TabIndex = 2;
			// 
			// DealStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.DealStatusTextBox, "DealStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).DealStatusDescription)));
			this.DealStatusTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d9d7923a-07fc-4501-ab6e-03dd2c0ccdfe", "Status");
			this.DealStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 73, true);
			this.DealStatusTextBox.Name = "DealStatusTextBox";
			this.DealStatusTextBox.ReadOnly = true;
			this.DealStatusTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DealStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 15, true);
			this.DealStatusTextBox.TabIndex = 3;
			// 
			// DealSubmittedDateEdit
			// 
			this.DealSubmittedDateEdit.AllowDrop = true;
			this.DealSubmittedDateEdit.AutoCompleteMonthThreshold = 1;
			this.DealSubmittedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DealSubmittedDateEdit, "DealSubmittedLocalTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentApprovalWithAuthorisation)(null)).DealSubmittedLocalTime)));
			this.DealSubmittedDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("60a4f04f-8a95-4f9e-aba7-f44677a2b88c", "Submitted");
			this.DealSubmittedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 25, true);
			this.DealSubmittedDateEdit.Name = "DealSubmittedDateEdit";
			this.DealSubmittedDateEdit.TabIndex = 4;
			// 
			// DealResponseDateEdit
			// 
			this.DealResponseDateEdit.AllowDrop = true;
			this.DealResponseDateEdit.AutoCompleteMonthThreshold = 1;
			this.DealResponseDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DealResponseDateEdit, "DealLastResponseLocalTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentApprovalWithAuthorisation)(null)).DealLastResponseLocalTime)));
			this.DealResponseDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0ca34483-3fa7-4834-88ba-c2176454b1cd", "Last Response");
			this.DealResponseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 49, true);
			this.DealResponseDateEdit.Name = "DealResponseDateEdit";
			this.DealResponseDateEdit.TabIndex = 5;
			// 
			// DealCostCalcFindBox
			// 
			this.DealCostCalcFindBox.AllowDrop = true;
			this.DealCostCalcFindBox.BindToAmount = "DealTotalCost";
			this.DealCostCalcFindBox.BindToUnit = "DealTotalCostCurrencyCode";
			this.DealCostCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6e7df30a-0644-4112-8457-33516389b14e", "Total Cost");
			this.DealCostCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.DealCostCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(677, 25, true);
			this.DealCostCalcFindBox.Name = "DealCostCalcFindBox";
			this.DealCostCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.DealCostCalcFindBox.TabIndex = 6;
			// 
			// DealErrorTextBox
			// 
			this.BindingSource.SetBindingMember(this.DealErrorTextBox, "DealErrorMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).DealErrorMessage)));
			this.DealErrorTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2a0843bc-7630-4767-ac07-e9ee2d9e691a", "Message");
			this.DealErrorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 74, true);
			this.DealErrorTextBox.Name = "DealErrorTextBox";
			this.DealErrorTextBox.ReadOnly = true;
			this.DealErrorTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DealErrorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 15, true);
			this.DealErrorTextBox.TabIndex = 7;
			// 
			// PaymentDetailsGroupBox
			// 
			this.PaymentDetailsGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PaymentDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|c7dede22-c668-4400-a20c-e0e85c0e8455", "Payment Details");
			this.PaymentDetailsGroupBox.Controls.Add(this.StatusDropEdit);
			this.PaymentDetailsGroupBox.Controls.Add(this.AH_NumberOfSupportingDocumentsCalcEdit);
			this.PaymentDetailsGroupBox.Controls.Add(this.PaymentNoTextBox);
			this.PaymentDetailsGroupBox.Controls.Add(this.PostDateEdit);
			this.PaymentDetailsGroupBox.Controls.Add(this.InvoiceDateEdit);
			this.PaymentDetailsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.PaymentDetailsGroupBox.Controls.Add(this.OrganizationGuidFindBox);
			this.PaymentDetailsGroupBox.Controls.Add(this.AddressWithContactControl);
			this.PaymentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
			this.PaymentDetailsGroupBox.Name = "PaymentDetailsGroupBox";
			this.PaymentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(916, 175, true);
			this.PaymentDetailsGroupBox.TabIndex = 0;
			this.PaymentDetailsGroupBox.TabStop = false;
			// 
			// PaymentNoTextBox
			// 
			this.PaymentNoTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PaymentNoTextBox, "HeaderTransactionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).HeaderTransactionNumber)));
			this.PaymentNoTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|41e85aae-9ef4-4930-a6e2-a74838cbd01f", "Payment", "Payment", "");
			this.PaymentNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 66, true);
			this.PaymentNoTextBox.Name = "PaymentNoTextBox";
			this.PaymentNoTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.PaymentNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 15, true);
			this.PaymentNoTextBox.TabIndex = 7;
			// 
			// PostDateEdit
			// 
			this.PostDateEdit.AllowDrop = true;
			this.PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateEdit, "AV_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentApprovalWithAuthorisation)(null)).AV_PostDate)));
			this.PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 40, true);
			this.PostDateEdit.Name = "PostDateEdit";
			this.PostDateEdit.TabIndex = 3;
			// 
			// InvoiceDateEdit
			// 
			this.InvoiceDateEdit.AllowDrop = true;
			this.InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.InvoiceDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.InvoiceDateEdit, "AV_PaymentDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentApprovalWithAuthorisation)(null)).AV_PaymentDate)));
			this.InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 16, true);
			this.InvoiceDateEdit.Name = "InvoiceDateEdit";
			this.InvoiceDateEdit.TabIndex = 1;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "AV_PaymentComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).AV_PaymentComment)));
			this.DescriptionTextBox.CaptionResourceString = null;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 117, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 15, true);
			this.DescriptionTextBox.TabIndex = 8;
			// 
			// OrganizationGuidFindBox
			// 
			this.OrganizationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganizationGuidFindBox, "AV_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PaymentApprovalWithAuthorisation)(null)).AV_OH)));
			this.OrganizationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 14, true);
			this.OrganizationGuidFindBox.Name = "OrganizationGuidFindBox";
			this.OrganizationGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OrganizationGuidFindBox.ParentType = null;
			this.OrganizationGuidFindBox.PopupCaption = null;
			this.OrganizationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 15, true);
			this.OrganizationGuidFindBox.TabIndex = 5;
			// 
			// AddressWithContactControl
			// 
			this.AddressWithContactControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressWithContactControl, "OrganisationAddressWithContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZAddressWithContact)(((PaymentApprovalWithAuthorisation)(null)).OrganisationAddressWithContact)));
			this.AddressWithContactControl.CaptionResourceString = null;
			this.AddressWithContactControl.ContactInfoTabVisible = true;
			this.AddressWithContactControl.InvoiceContactTabCaption = Enterprise.Accounting.GUI.Res.GetData("ZAddressWithContactControl|1ef7010f-583b-43f3-9c68-c27893e282de", "Payment Contact");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddressWithContactControl, false);
			this.AddressWithContactControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 34, true);
			this.AddressWithContactControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 132, true);
			this.AddressWithContactControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 132, true);
			this.AddressWithContactControl.Name = "AddressWithContactControl";
			this.AddressWithContactControl.OnlyStopOnDebtor = true;
			this.AddressWithContactControl.PopupCaption = "";
			this.AddressWithContactControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 132, true);
			this.AddressWithContactControl.TabIndex = 10;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 626, true);
			this.WorkflowTabPage.TabIndex = 1;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 626, true);
			this.zStmNoteTabPage1.TabIndex = 2;
			// 
			// EventTabPage
			// 
			this.EventTabPage.ExcludeFromBindingOnSave = true;
			this.EventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.EventTabPage.Name = "EventTabPage";
			this.EventTabPage.ShouldBeReadOnlyInViewMode = false;
			this.EventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 645, true);
			this.EventTabPage.TabIndex = 1;
			// 
			// PaymentDetailButton
			// 
			this.PaymentDetailButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PaymentDetailButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|ff39b422-b338-4603-86ae-936e83674a37", "Payment Detail");
			this.PaymentDetailButton.IsCaptionOverridden = false;
			this.PaymentDetailButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(746, 684, true);
			this.PaymentDetailButton.Name = "PaymentDetailButton";
			this.PaymentDetailButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PaymentDetailButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.PaymentDetailButton.TabIndex = 7;
			this.PaymentDetailButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.PaymentDetailButton.ToolTipCaption = null;
			this.PaymentDetailButton.UseVisualStyleBackColor = true;
			this.PaymentDetailButton.Click += new EventHandler(this.PaymentDetailButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|2c0a7946-1853-4e19-bc73-77e1167ed754", "Close");
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(840, 684, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.CloseButton.TabIndex = 8;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// PostWithoutMatchingButton
			// 
			this.PostWithoutMatchingButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostWithoutMatchingButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|a6aee3db-1ce7-4e4e-b99c-66878ddbd816", "Post Without Matching");
			this.PostWithoutMatchingButton.IsCaptionOverridden = false;
			this.PostWithoutMatchingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(610, 684, true);
			this.PostWithoutMatchingButton.Name = "PostWithoutMatchingButton";
			this.PostWithoutMatchingButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PostWithoutMatchingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 23, true);
			this.PostWithoutMatchingButton.TabIndex = 7;
			this.PostWithoutMatchingButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.PostWithoutMatchingButton.ToolTipCaption = null;
			this.PostWithoutMatchingButton.UseVisualStyleBackColor = true;
			this.PostWithoutMatchingButton.Click += new EventHandler(this.PostWithoutMatchingButton_Click);
			// 
			// SaveAsDraftButton
			// 
			this.SaveAsDraftButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveAsDraftButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|e7224c37-93e9-436f-a4b0-db0c44db6234", "Save as Draft");
			this.SaveAsDraftButton.IsCaptionOverridden = false;
			this.SaveAsDraftButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 684, true);
			this.SaveAsDraftButton.Name = "SaveAsDraftButton";
			this.SaveAsDraftButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveAsDraftButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.SaveAsDraftButton.TabIndex = 7;
			this.SaveAsDraftButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SaveAsDraftButton.ToolTipCaption = null;
			this.SaveAsDraftButton.UseVisualStyleBackColor = true;
			this.SaveAsDraftButton.Click += new EventHandler(this.SaveAsDraftButton_Click);
			// 
			// ProcessEPaymentButton
			// 
			this.ProcessEPaymentButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ProcessEPaymentButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("503ce3e9-6d1a-4032-a897-7aa76b0bb472", "Process E-Payment");
			this.ProcessEPaymentButton.IsCaptionOverridden = false;
			this.ProcessEPaymentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 684, true);
			this.ProcessEPaymentButton.Name = "ProcessEPaymentButton";
			this.ProcessEPaymentButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ProcessEPaymentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.ProcessEPaymentButton.TabIndex = 7;
			this.ProcessEPaymentButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ProcessEPaymentButton.ToolTipCaption = null;
			this.ProcessEPaymentButton.UseVisualStyleBackColor = true;
			this.ProcessEPaymentButton.Click += new EventHandler(this.ProcessEPaymentButton_Click);
			// 
			// SubmitForApprovalButton
			// 
			this.SubmitForApprovalButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SubmitForApprovalButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|308CEF72-717B-4D17-B126-C66783F21BCC", "Submit for Approval");
			this.SubmitForApprovalButton.IsCaptionOverridden = false;
			this.SubmitForApprovalButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(267, 684, true);
			this.SubmitForApprovalButton.Name = "SubmitForApprovalButton";
			this.SubmitForApprovalButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SubmitForApprovalButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 23, true);
			this.SubmitForApprovalButton.TabIndex = 7;
			this.SubmitForApprovalButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SubmitForApprovalButton.ToolTipCaption = null;
			this.SubmitForApprovalButton.UseVisualStyleBackColor = true;
			this.SubmitForApprovalButton.Visible = false;
			this.SubmitForApprovalButton.Click += new EventHandler(this.OnSubmitForApprovalButton_Click);
			// 
			// ApproveForPostingButton
			// 
			this.ApproveForPostingButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ApproveForPostingButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|CDE31E27-0B89-4F1E-9FB5-7CB6733CEEED", "Approve for Posting");
			this.ApproveForPostingButton.IsCaptionOverridden = false;
			this.ApproveForPostingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(267, 684, true);
			this.ApproveForPostingButton.Name = "ApproveForPostingButton";
			this.ApproveForPostingButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ApproveForPostingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 23, true);
			this.ApproveForPostingButton.TabIndex = 7;
			this.ApproveForPostingButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ApproveForPostingButton.ToolTipCaption = null;
			this.ApproveForPostingButton.UseVisualStyleBackColor = true;
			this.ApproveForPostingButton.Visible = false;
			this.ApproveForPostingButton.Click += new EventHandler(this.OnApproveForPostingButton_Click);
			// 
			// FundingCurrencyCodeFindBox
			// 
			this.FundingCurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FundingCurrencyCodeFindBox, "FundingCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).FundingCurrency)));
			this.FundingCurrencyCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|a5afbe87-e264-4abd-a181-2f47dfe6f677", "Funding Currency");
			this.FundingCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(538, 144, true);
			this.FundingCurrencyCodeFindBox.Name = "FundingCurrencyCodeFindBox";
			this.FundingCurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FundingCurrencyCodeFindBox.ParentType = null;
			this.FundingCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.FundingCurrencyCodeFindBox.TabIndex = 13;
			this.FundingCurrencyCodeFindBox.Visible = false;
			// 
			// FundingBankAccountFindBox
			// 
			this.FundingBankAccountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FundingBankAccountFindBox, "FundingBankAccountPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PaymentApprovalWithAuthorisation)(null)).FundingBankAccountPK)));
			this.FundingBankAccountFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|92983ebd-0474-4b8b-91ab-9a97788f2e47", "Funding Bank Account");
			this.FundingBankAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 144, true);
			this.FundingBankAccountFindBox.Name = "FundingBankAccountFindBox";
			this.FundingBankAccountFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FundingBankAccountFindBox.ParentType = null;
			this.FundingBankAccountFindBox.PopupCaption = null;
			this.FundingBankAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 20, true);
			this.FundingBankAccountFindBox.TabIndex = 12;
			this.FundingBankAccountFindBox.Visible = false;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "AV_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentApprovalWithAuthorisation)(null)).AV_Status)));
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 92, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.ShouldResizeByMaxLength = true;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 15, true);
			this.StatusDropEdit.TabIndex = 11;
			// 
			// PaymentApprovalForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalForm|f4b53c1e-3f17-4493-b58d-64db8c1bbf9c", "Payment Approval Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(942, 734, true);
			this.Controls.Add(this.ProcessEPaymentButton);
			this.Controls.Add(this.ApproveForPostingButton);
			this.Controls.Add(this.SubmitForApprovalButton);
			this.Controls.Add(this.SaveAsDraftButton);
			this.Controls.Add(this.PostWithoutMatchingButton);
			this.Controls.Add(this.PaymentDetailButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.zTabControl1);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(PaymentApprovalWithAuthorisation);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.PaymentApproval.PaymentApprovalWithAuthorisat" +
			"ion";
			this.Name = "PaymentApprovalForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zTabControl1, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.PaymentDetailButton, 0);
			this.Controls.SetChildIndex(this.PostWithoutMatchingButton, 0);
			this.Controls.SetChildIndex(this.SaveAsDraftButton, 0);
			this.Controls.SetChildIndex(this.SubmitForApprovalButton, 0);
			this.Controls.SetChildIndex(this.ApproveForPostingButton, 0);
			this.Controls.SetChildIndex(this.ProcessEPaymentButton, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.BankDetailsGroupBox.ResumeLayout(false);
			this.BankDetailsGroupBox.PerformLayout();
			this.PaymentReasonDropEdit.ResumeLayout(true);
			this.PaymentReasonDropEdit.PerformLayout();
			((ISupportInitialize)(this.ProviderLogoPictureBox)).EndInit();
			this.ChequeBookGuidFindBox.ResumeLayout(true);
			this.ChequeBookGuidFindBox.PerformLayout();
			this.BankAccountGuidFindBox.ResumeLayout(true);
			this.BankAccountGuidFindBox.PerformLayout();
			this.PaymentTypeDropEdit.ResumeLayout(true);
			this.PaymentTypeDropEdit.PerformLayout();
			this.PaymentAmountGroupBox.ResumeLayout(false);
			this.PaymentAmountGroupBox.PerformLayout();
			this.LocalAmountCalcFindBox.ResumeLayout(true);
			this.LocalAmountCalcFindBox.PerformLayout();
			this.PaymentAmountCalcFindBox.ResumeLayout(true);
			this.PaymentAmountCalcFindBox.PerformLayout();
			this.ExchangeRateControl.ResumeLayout(true);
			this.ExchangeRateControl.PerformLayout();
			this.zTabControl1.ResumeLayout(false);
			this.zTabControl1.PerformLayout();
			this.PaymentTabPage.ResumeLayout(false);
			this.PaymentTabPage.PerformLayout();
			this.BankAndEPaymentTabControl.ResumeLayout(false);
			this.BankAndEPaymentTabControl.PerformLayout();
			this.BankDetailsTabPage.ResumeLayout(false);
			this.BankDetailsTabPage.PerformLayout();
			this.EPaymentTabPage.ResumeLayout(false);
			this.EPaymentTabPage.PerformLayout();
			this.EQuoteGroupBox.ResumeLayout(false);
			this.EQuoteGroupBox.PerformLayout();
			((ISupportInitialize)(this.EPaymentGrid)).EndInit();
			this.EPaymentGrid.ResumeLayout(false);
			this.EPaymentGrid.PerformLayout();
			this.DealGroupBox.ResumeLayout(false);
			this.DealGroupBox.PerformLayout();
			this.DealSubmittedDateEdit.ResumeLayout(true);
			this.DealSubmittedDateEdit.PerformLayout();
			this.DealResponseDateEdit.ResumeLayout(true);
			this.DealResponseDateEdit.PerformLayout();
			this.DealCostCalcFindBox.ResumeLayout(true);
			this.DealCostCalcFindBox.PerformLayout();
			this.PaymentDetailsGroupBox.ResumeLayout(false);
			this.PaymentDetailsGroupBox.PerformLayout();
			this.PostDateEdit.ResumeLayout(true);
			this.PostDateEdit.PerformLayout();
			this.InvoiceDateEdit.ResumeLayout(true);
			this.InvoiceDateEdit.PerformLayout();
			this.OrganizationGuidFindBox.ResumeLayout(true);
			this.OrganizationGuidFindBox.PerformLayout();
			this.AddressWithContactControl.ResumeLayout(true);
			this.AddressWithContactControl.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
