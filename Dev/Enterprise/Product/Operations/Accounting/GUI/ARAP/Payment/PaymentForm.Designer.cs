using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using PaymentBase = Enterprise.Accounting.Business.ARAP.ReceiptPayment.Payment;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class PaymentForm
	{


		#region Windows Form Designer generated code

		private ZGroupBox PaymentDetailsGroupBox;
		protected ZDateEdit InvoiceDateEdit;
		protected ZDateEdit PostDateEdit;
		private ZGuidFindBox OrganizationGuidFindBox;
		private ZTextBox DescriptionTextBox;
		protected ZTextBox PaymentNoTextBox;
		private ZGroupBox BankDetailsGroupBox;
		private ZDropEdit PaymentTypeDropEdit;
		private ZGuidFindBox BankAccountGuidFindBox;
		private ZGuidFindBox ChequeBookGuidFindBox;
		private ZTextBox ChequeNoTextBox;
		private ZGroupBox PaymentAmountGroupBox;
		private ZCalcFindBox LocalAmountCalcFindBox;
		private ZCalcFindBox PaymentAmountCalcFindBox;
		private ZExchangeRateControl ExchangeRateControl;
		private ZTemplateTabControl TabControl;
		private ZLogsTabPage EventTabPage;
		private ZTabPage PaymentTabPage;
		protected ZCalcEdit AH_NumberOfSupportingDocumentsCalcEdit;
		private ZStmNoteTabPage zStmNoteTabPage1;
		protected ZDateEdit UnmatchDateEdit;
		private PaymentAddressWithContactControl AddressWithContactControl;
		private MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		private ZButton CheckEPayRateButton;
		private ZTabControl BankAndEPaymentTabControl;
		private ZTabPage BankDetailsTabPage;
		private ZTabPage EPaymentTabPage;
		private ZGrid EPaymentGrid;
		private ZGroupBox EQuoteGroupBox;
		private ZButton AcceptQuoteButton;
		private ZButton RefreshButton;
		private ZGroupBox DealGroupBox;
		private ZTextBox DealProviderTextBox;
		private ZTextBox DealReferenceTextBox;
		private ZTextBox DealStatusTextBox;
		private ZTextBox DealErrorTextBox;
		private ZDateEdit DealSubmittedDateEdit;
		private ZDateEdit DealResponseDateEdit;
		private ZCalcFindBox DealCostCalcFindBox;
		private ZButton ProcessEPaymentButton;
		protected ZGuidFindBox FundingBankAccountFindBox;
		private ZCodeFindBox FundingCurrencyCodeFindBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
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
			this.CheckEPayRateButton = new ZButton();
			this.PaymentDetailsGroupBox = new ZGroupBox();
			this.AddressWithContactControl = new PaymentAddressWithContactControl();
			this.UnmatchDateEdit = new ZDateEdit();
			this.AH_NumberOfSupportingDocumentsCalcEdit = new ZCalcEdit();
			this.PaymentNoTextBox = new ZTextBox();
			this.DescriptionTextBox = new ZTextBox();
			this.OrganizationGuidFindBox = new ZGuidFindBox();
			this.PostDateEdit = new ZDateEdit();
			this.InvoiceDateEdit = new ZDateEdit();
			this.BankDetailsGroupBox = new ZGroupBox();
			this.PaymentReasonDropEdit = new ZDropEdit();
			this.ChequeNoTextBox = new ZTextBox();
			this.ChequeBookGuidFindBox = new ZGuidFindBox();
			this.BankAccountGuidFindBox = new ZGuidFindBox();
			this.PaymentTypeDropEdit = new ZDropEdit();
			this.PaymentAmountGroupBox = new ZGroupBox();
			this.LocalAmountCalcFindBox = new ZCalcFindBox();
			this.PaymentAmountCalcFindBox = new ZCalcFindBox();
			this.ExchangeRateControl = new ZExchangeRateControl();
			this.TabControl = new ZTemplateTabControl();
			this.PaymentTabPage = new ZTabPage();
			this.BankAndEPaymentTabControl = new ZTabControl();
			this.BankDetailsTabPage = new ZTabPage();
			this.EPaymentTabPage = new ZTabPage();
			this.EQuoteGroupBox = new ZGroupBox();
			this.RefreshButton = new ZButton();
			this.AcceptQuoteButton = new ZButton();
			this.EPaymentGrid = new ZGrid();
			this.DealGroupBox = new ZGroupBox();
			this.DealProviderTextBox = new ZTextBox();
			this.DealReferenceTextBox = new ZTextBox();
			this.DealStatusTextBox = new ZTextBox();
			this.DealSubmittedDateEdit = new ZDateEdit();
			this.DealResponseDateEdit = new ZDateEdit();
			this.DealCostCalcFindBox = new ZCalcFindBox();
			this.DealErrorTextBox = new ZTextBox();
			this.WorkflowTabPage = new MasterFiles.GUI.ZWorkflowTabPage();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.EventTabPage = new ZLogsTabPage();
			this.BottomPanel = new ZPanel();
			this.ProcessEPaymentButton = new ZButton();
			this.CloseButton = new Core.Forms.ZPostOrCancelButton();
			this.PaymentDetailButton = new Core.Forms.ZPostOrCancelButton();
			this.FundingBankAccountFindBox = new ZGuidFindBox();
			this.FundingCurrencyCodeFindBox = new ZCodeFindBox();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PaymentDetailsGroupBox.SuspendLayout();
			this.AddressWithContactControl.SuspendLayout();
			this.UnmatchDateEdit.SuspendLayout();
			this.OrganizationGuidFindBox.SuspendLayout();
			this.PostDateEdit.SuspendLayout();
			this.InvoiceDateEdit.SuspendLayout();
			this.BankDetailsGroupBox.SuspendLayout();
			this.PaymentReasonDropEdit.SuspendLayout();
			this.ChequeBookGuidFindBox.SuspendLayout();
			this.BankAccountGuidFindBox.SuspendLayout();
			this.PaymentTypeDropEdit.SuspendLayout();
			this.PaymentAmountGroupBox.SuspendLayout();
			this.LocalAmountCalcFindBox.SuspendLayout();
			this.PaymentAmountCalcFindBox.SuspendLayout();
			this.ExchangeRateControl.SuspendLayout();
			this.TabControl.SuspendLayout();
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
			this.WorkflowTabPage.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.FundingBankAccountFindBox.SuspendLayout();
			this.FundingCurrencyCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 695, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 33, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(538);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(PaymentBase);
			// 
			// CheckEPayRateButton
			// 
			this.CheckEPayRateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c07de801-67aa-4f52-888e-0b2eef2841a8", "Check Ex Rate for E-Payment");
			this.CheckEPayRateButton.IsCaptionOverridden = false;
			this.CheckEPayRateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 66, true);
			this.CheckEPayRateButton.Name = "CheckEPayRateButton";
			this.CheckEPayRateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CheckEPayRateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 23, true);
			this.CheckEPayRateButton.TabIndex = 7;
			this.CheckEPayRateButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.CheckEPayRateButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CheckEPayRateButton.ToolTipCaption = null;
			this.CheckEPayRateButton.UseVisualStyleBackColor = true;
			this.CheckEPayRateButton.Click += new EventHandler(this.CheckEPayRateButton_Click);
			// 
			// PaymentDetailsGroupBox
			// 
			this.PaymentDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|1c19b8d8-d055-456a-a02c-983cbcbb1ca5", "Payment Details");
			this.PaymentDetailsGroupBox.Controls.Add(this.AddressWithContactControl);
			this.PaymentDetailsGroupBox.Controls.Add(this.UnmatchDateEdit);
			this.PaymentDetailsGroupBox.Controls.Add(this.AH_NumberOfSupportingDocumentsCalcEdit);
			this.PaymentDetailsGroupBox.Controls.Add(this.PaymentNoTextBox);
			this.PaymentDetailsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.PaymentDetailsGroupBox.Controls.Add(this.OrganizationGuidFindBox);
			this.PaymentDetailsGroupBox.Controls.Add(this.PostDateEdit);
			this.PaymentDetailsGroupBox.Controls.Add(this.InvoiceDateEdit);
			this.PaymentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 2, true);
			this.PaymentDetailsGroupBox.Name = "PaymentDetailsGroupBox";
			this.PaymentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 175, true);
			this.PaymentDetailsGroupBox.TabIndex = 1;
			this.PaymentDetailsGroupBox.TabStop = false;
			// 
			// AddressWithContactControl
			// 
			this.AddressWithContactControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressWithContactControl, "OrganisationAddressWithContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZAddressWithContact)(((PaymentBase)(null)).OrganisationAddressWithContact)));
			this.AddressWithContactControl.CaptionResourceString = null;
			this.AddressWithContactControl.ContactInfoTabVisible = true;
			this.AddressWithContactControl.InvoiceContactTabCaption = Enterprise.Accounting.GUI.Res.GetData("ZAddressWithContactControl|1ef7010f-583b-43f3-9c68-c27893e282de", "Payment Contact");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddressWithContactControl, false);
			this.AddressWithContactControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(505, 36, true);
			this.AddressWithContactControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 132, true);
			this.AddressWithContactControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 132, true);
			this.AddressWithContactControl.Name = "AddressWithContactControl";
			this.AddressWithContactControl.OnlyStopOnDebtor = true;
			this.AddressWithContactControl.PopupCaption = "";
			this.AddressWithContactControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 132, true);
			this.AddressWithContactControl.TabIndex = 10;
			// 
			// UnmatchDateEdit
			// 
			this.UnmatchDateEdit.AllowDrop = true;
			this.UnmatchDateEdit.AutoCompleteMonthThreshold = 1;
			this.UnmatchDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.UnmatchDateEdit, "UnmatchDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentBase)(null)).UnmatchDate)));
			this.UnmatchDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|d0ba4886-4aa2-43af-be62-f29d213e4750", "Unmatch Date");
			this.UnmatchDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 16, true);
			this.UnmatchDateEdit.Name = "UnmatchDateEdit";
			this.UnmatchDateEdit.TabIndex = 2;
			// 
			// AH_NumberOfSupportingDocumentsCalcEdit
			// 
			this.AH_NumberOfSupportingDocumentsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_NumberOfSupportingDocumentsCalcEdit, "AH_NumberOfSupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PaymentBase)(null)).AH_NumberOfSupportingDocuments)));
			this.AH_NumberOfSupportingDocumentsCalcEdit.CaptionResourceString = null;
			this.AH_NumberOfSupportingDocumentsCalcEdit.DecimalPlaces = 2;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 40, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.Name = "AH_NumberOfSupportingDocumentsCalcEdit";
			this.AH_NumberOfSupportingDocumentsCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.TabIndex = 3;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Text = "0";
			this.AH_NumberOfSupportingDocumentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PaymentNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.PaymentNoTextBox, "AH_TransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentBase)(null)).AH_TransactionNum)));
			this.PaymentNoTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|f5cbd75a-71bd-4bb1-b6c9-49dd7c49155c", "Payment No.");
			this.PaymentNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 66, true);
			this.PaymentNoTextBox.Name = "PaymentNoTextBox";
			this.PaymentNoTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.PaymentNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.PaymentNoTextBox.TabIndex = 7;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "AH_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentBase)(null)).AH_Desc)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|309d0de0-7606-4ff6-97d5-9ac0db8c90b4", "Description");
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 118, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.DescriptionTextBox.TabIndex = 8;
			// 
			// OrganizationGuidFindBox
			// 
			this.OrganizationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganizationGuidFindBox, "AH_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PaymentBase)(null)).AH_OH)));
			this.OrganizationGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|2d72ff20-6ea7-4fba-8aa4-47a415cf5954", "Account");
			this.OrganizationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(505, 16, true);
			this.OrganizationGuidFindBox.Name = "OrganizationGuidFindBox";
			this.OrganizationGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OrganizationGuidFindBox.ParentType = null;
			this.OrganizationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 20, true);
			this.OrganizationGuidFindBox.TabIndex = 6;
			// 
			// PostDateEdit
			// 
			this.PostDateEdit.AllowDrop = true;
			this.PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateEdit, "AH_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentBase)(null)).AH_PostDate)));
			this.PostDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|65561de7-9369-434c-bbba-a0fae9576b74", "Post Date");
			this.PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 40, true);
			this.PostDateEdit.Name = "PostDateEdit";
			this.PostDateEdit.TabIndex = 1;
			// 
			// InvoiceDateEdit
			// 
			this.InvoiceDateEdit.AllowDrop = true;
			this.InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.InvoiceDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.InvoiceDateEdit, "AH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentBase)(null)).AH_InvoiceDate)));
			this.InvoiceDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|a6bc3f05-ba56-481c-ab45-d491fb991e2d", "Date");
			this.InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 16, true);
			this.InvoiceDateEdit.Name = "InvoiceDateEdit";
			this.InvoiceDateEdit.TabIndex = 0;
			// 
			// BankDetailsGroupBox
			// 
			this.BankDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|8a0052ba-4bad-40f3-aabf-5f908f717d91", "Bank Details");
			this.BankDetailsGroupBox.Controls.Add(this.PaymentReasonDropEdit);
			this.BankDetailsGroupBox.Controls.Add(this.ChequeNoTextBox);
			this.BankDetailsGroupBox.Controls.Add(this.ChequeBookGuidFindBox);
			this.BankDetailsGroupBox.Controls.Add(this.BankAccountGuidFindBox);
			this.BankDetailsGroupBox.Controls.Add(this.PaymentTypeDropEdit);
			this.BankDetailsGroupBox.Controls.Add(this.FundingCurrencyCodeFindBox);
			this.BankDetailsGroupBox.Controls.Add(this.FundingBankAccountFindBox);
			this.BankDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.BankDetailsGroupBox.Name = "BankDetailsGroupBox";
			this.BankDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(901, 165, true);
			this.BankDetailsGroupBox.TabIndex = 2;
			this.BankDetailsGroupBox.TabStop = false;
			// 
			// FundingCurrencyCodeFindBox
			// 
			this.FundingCurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FundingCurrencyCodeFindBox, "FundingCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentBase)(null)).FundingCurrency)));
			this.FundingCurrencyCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("387e24ab-3529-45a8-b837-07ce94acc377", "Funding Currency");
			this.FundingCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(515, 138, true);
			this.FundingCurrencyCodeFindBox.Name = "FundingCurrencyCodeFindBox";
			this.FundingCurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FundingCurrencyCodeFindBox.ParentType = null;
			this.FundingCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 20, true);
			this.FundingCurrencyCodeFindBox.TabIndex = 10;
			this.FundingCurrencyCodeFindBox.Visible = false;
			// 
			// FundingBankAccountFindBox
			// 
			this.FundingBankAccountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FundingBankAccountFindBox, "FundingBankAccountPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PaymentBase)(null)).FundingBankAccountPK)));
			this.FundingBankAccountFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a9ca46b1-cfd7-4be0-8d34-e5a112140da9", "Funding Bank Account");
			this.FundingBankAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 138, true);
			this.FundingBankAccountFindBox.Name = "FundingBankAccountFindBox";
			this.FundingBankAccountFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FundingBankAccountFindBox.ParentType = null;
			this.FundingBankAccountFindBox.PopupCaption = null;
			this.FundingBankAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.FundingBankAccountFindBox.TabIndex = 9;
			this.FundingBankAccountFindBox.Visible = false;
			// 
			// PaymentReasonDropEdit
			// 
			this.PaymentReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentReasonDropEdit, "EPaymentReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentBase)(null)).EPaymentReason)));
			this.PaymentReasonDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fcb0bd31-506c-462a-b490-2b22843c1f2d", "Payment Reason");
			this.PaymentReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 114, true);
			this.PaymentReasonDropEdit.Name = "PaymentReasonDropEdit";
			this.PaymentReasonDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 20, true);
			this.PaymentReasonDropEdit.TabIndex = 8;
			// 
			// ChequeNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChequeNoTextBox, "AH_ChequeOrReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentBase)(null)).AH_ChequeOrReference)));
			this.ChequeNoTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|66765ac2-522e-4949-9e0d-418a7bc7119f", "Reference #", "Reference No.", "");
			this.ChequeNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 88, true);
			this.ChequeNoTextBox.Name = "ChequeNoTextBox";
			this.ChequeNoTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ChequeNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 20, true);
			this.ChequeNoTextBox.TabIndex = 7;
			// 
			// ChequeBookGuidFindBox
			// 
			this.ChequeBookGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChequeBookGuidFindBox, "ChequeBook");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PaymentBase)(null)).ChequeBook)));
			this.ChequeBookGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|e6da5fc8-3bd1-4b87-8178-6f682bee1876", "Check Book");
			this.ChequeBookGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 64, true);
			this.ChequeBookGuidFindBox.Name = "ChequeBookGuidFindBox";
			this.ChequeBookGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ChequeBookGuidFindBox.ParentType = null;
			this.ChequeBookGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 20, true);
			this.ChequeBookGuidFindBox.TabIndex = 6;
			// 
			// BankAccountGuidFindBox
			// 
			this.BankAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BankAccountGuidFindBox, "AH_AB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PaymentBase)(null)).AH_AB)));
			this.BankAccountGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|7d64928a-29db-4737-9f3a-8492735c5ca5", "Bank Account");
			this.BankAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 40, true);
			this.BankAccountGuidFindBox.Name = "BankAccountGuidFindBox";
			this.BankAccountGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BankAccountGuidFindBox.ParentType = null;
			this.BankAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 20, true);
			this.BankAccountGuidFindBox.TabIndex = 5;
			// 
			// PaymentTypeDropEdit
			// 
			this.PaymentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTypeDropEdit, "AH_ReceiptType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentBase)(null)).AH_ReceiptType)));
			this.PaymentTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|a2ec22c2-e7e7-433c-ae7b-4aef431b2170", "Payment Type");
			this.PaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 16, true);
			this.PaymentTypeDropEdit.Name = "PaymentTypeDropEdit";
			this.PaymentTypeDropEdit.PreBoundMaxLength = 5;
			this.PaymentTypeDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 20, true);
			this.PaymentTypeDropEdit.TabIndex = 4;
			// 
			// PaymentAmountGroupBox
			// 
			this.PaymentAmountGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|1c333351-de10-46ad-9077-55b12f3ab957", "Payment Amount");
			this.PaymentAmountGroupBox.Controls.Add(this.LocalAmountCalcFindBox);
			this.PaymentAmountGroupBox.Controls.Add(this.PaymentAmountCalcFindBox);
			this.PaymentAmountGroupBox.Controls.Add(this.ExchangeRateControl);
			this.PaymentAmountGroupBox.Controls.Add(this.CheckEPayRateButton);
			this.PaymentAmountGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 173, true);
			this.PaymentAmountGroupBox.Name = "PaymentAmountGroupBox";
			this.PaymentAmountGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(901, 116, true);
			this.PaymentAmountGroupBox.TabIndex = 3;
			this.PaymentAmountGroupBox.TabStop = false;
			// 
			// LocalAmountCalcFindBox
			// 
			this.LocalAmountCalcFindBox.AllowDrop = true;
			this.LocalAmountCalcFindBox.BindToAmount = "AH_LocalExTaxAmount";
			this.LocalAmountCalcFindBox.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.LocalAmountCalcFindBox.BindToUnit = "AH_Calc_LocalRX";
			this.LocalAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|ed42862d-9a8e-4fda-a9d1-e7421e202b47", "Local Amount");
			this.LocalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 40, true);
			this.LocalAmountCalcFindBox.Name = "LocalAmountCalcFindBox";
			this.LocalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.LocalAmountCalcFindBox.TabIndex = 5;
			// 
			// PaymentAmountCalcFindBox
			// 
			this.PaymentAmountCalcFindBox.AllowDrop = true;
			this.PaymentAmountCalcFindBox.BindToAmount = "AH_OSExTaxAmount";
			this.PaymentAmountCalcFindBox.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.PaymentAmountCalcFindBox.BindToUnit = "AH_RX_NKTransactionCurrency";
			this.PaymentAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|fa10c47f-82ee-4bff-a6af-e7e3f801578d", "Payment Amount");
			this.PaymentAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.PaymentAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 40, true);
			this.PaymentAmountCalcFindBox.Name = "PaymentAmountCalcFindBox";
			this.PaymentAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 20, true);
			this.PaymentAmountCalcFindBox.TabIndex = 4;
			// 
			// ExchangeRateControl
			// 
			this.ExchangeRateControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExchangeRateControl, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((PaymentBase)(null)).ExchangeRate)));
			this.ExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|d2d96221-412e-4bc5-b950-9080bf093b97", "Exchange Rate");
			this.ExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 16, true);
			this.ExchangeRateControl.Name = "ExchangeRateControl";
			this.ExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 20, true);
			this.ExchangeRateControl.TabIndex = 3;
			// 
			// TabControl
			// 
			this.TabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.PaymentTabPage);
			this.TabControl.Controls.Add(this.WorkflowTabPage);
			this.TabControl.Controls.Add(this.zStmNoteTabPage1);
			this.TabControl.Controls.Add(this.EventTabPage);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 662, true);
			this.TabControl.TabIndex = 0;
			// 
			// PaymentTabPage
			// 
			this.PaymentTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|48347903-906d-4b5d-a911-ee85f23b4bdf", "Payment");
			this.PaymentTabPage.Controls.Add(this.BankAndEPaymentTabControl);
			this.PaymentTabPage.Controls.Add(this.PaymentDetailsGroupBox);
			this.PaymentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PaymentTabPage.Name = "PaymentTabPage";
			this.PaymentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 635, true);
			this.PaymentTabPage.TabIndex = 0;
			// 
			// BankAndEPaymentTabControl
			// 
			this.BankAndEPaymentTabControl.Controls.Add(this.BankDetailsTabPage);
			this.BankAndEPaymentTabControl.Controls.Add(this.EPaymentTabPage);
			this.BankAndEPaymentTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 188, true);
			this.BankAndEPaymentTabControl.Name = "BankAndEPaymentTabControl";
			this.BankAndEPaymentTabControl.SelectedIndex = 0;
			this.BankAndEPaymentTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 435, true);
			this.BankAndEPaymentTabControl.TabIndex = 3;
			// 
			// BankDetailsTabPage
			// 
			this.BankDetailsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.BankDetailsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c5d663ca-825c-4bff-8823-4fcb9fc82a74", "Bank Details");
			this.BankDetailsTabPage.Controls.Add(this.BankDetailsGroupBox);
			this.BankDetailsTabPage.Controls.Add(this.PaymentAmountGroupBox);
			this.BankDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BankDetailsTabPage.Name = "BankDetailsTabPage";
			this.BankDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.BankDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(903, 408, true);
			this.BankDetailsTabPage.TabIndex = 0;
			// 
			// EPaymentTabPage
			// 
			this.EPaymentTabPage.AutoScroll = true;
			this.EPaymentTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.EPaymentTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f7a758e0-7265-4091-b51a-3f7fdec6a084", "E-Payment Processing");
			this.EPaymentTabPage.Controls.Add(this.EQuoteGroupBox);
			this.EPaymentTabPage.Controls.Add(this.DealGroupBox);
			this.EPaymentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EPaymentTabPage.Name = "EPaymentTabPage";
			this.EPaymentTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EPaymentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(903, 408, true);
			this.EPaymentTabPage.TabIndex = 1;
			// 
			// EQuoteGroupBox
			// 
			this.EQuoteGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e60a1d51-8164-4a46-81f6-104b68f4fa12", "E-Quote");
			this.EQuoteGroupBox.Controls.Add(this.RefreshButton);
			this.EQuoteGroupBox.Controls.Add(this.AcceptQuoteButton);
			this.EQuoteGroupBox.Controls.Add(this.EPaymentGrid);
			this.EQuoteGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EQuoteGroupBox.Name = "EQuoteGroupBox";
			this.EQuoteGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 219, true);
			this.EQuoteGroupBox.TabIndex = 2;
			this.EQuoteGroupBox.TabStop = false;
			// 
			// RefreshButton
			// 
			this.RefreshButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("330677cc-e03f-447e-91d4-ed798e1d01e1", "Refresh");
			this.RefreshButton.IsCaptionOverridden = false;
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 21, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 27, true);
			this.RefreshButton.TabIndex = 0;
			this.RefreshButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RefreshButton.ToolTipCaption = null;
			this.RefreshButton.UseVisualStyleBackColor = true;
			this.RefreshButton.Click += new EventHandler(this.RefreshButton_Click);
			// 
			// AcceptQuoteButton
			// 
			this.AcceptQuoteButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("774db60e-f620-48d3-adde-02b45d67afc7", "Accept Quote");
			this.AcceptQuoteButton.IsCaptionOverridden = false;
			this.AcceptQuoteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 21, true);
			this.AcceptQuoteButton.Name = "AcceptQuoteButton";
			this.AcceptQuoteButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AcceptQuoteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 26, true);
			this.AcceptQuoteButton.TabIndex = 1;
			this.AcceptQuoteButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AcceptQuoteButton.ToolTipCaption = null;
			this.AcceptQuoteButton.UseVisualStyleBackColor = true;
			this.AcceptQuoteButton.Click += new EventHandler(this.AcceptQuoteButton_Click);
			// 
			// EPaymentGrid
			// 
			this.EPaymentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EPaymentGrid, "PaymentQuotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((PaymentBase)(null)).PaymentQuotes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentBase)(null)).PaymentQuotes)).SyncRoot)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentBase)(null)).PaymentQuotes)).SyncRoot)).ProviderCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentBase)(null)).PaymentQuotes)).SyncRoot)).ToCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentBase)(null)).PaymentQuotes)).SyncRoot)).FromCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentBase)(null)).PaymentQuotes)).SyncRoot)).ToAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentBase)(null)).PaymentQuotes)).SyncRoot)).ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentBase)(null)).PaymentQuotes)).SyncRoot)).ExchangeRateInverted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentBase)(null)).PaymentQuotes)).SyncRoot)).FromAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentBase)(null)).PaymentQuotes)).SyncRoot)).FeeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentBase)(null)).PaymentQuotes)).SyncRoot)).ErrorDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentBase)(null)).PaymentQuotes)).SyncRoot)).ProviderReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentBase)(null)).PaymentQuotes)).SyncRoot)).LastResponseReceivedLocalTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentBase)(null)).PaymentQuotes)).SyncRoot)).InternalReference)));
			this.EPaymentGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1536df42-7673-4a54-8467-280d0b04829a", "Status");
			zTextBoxColumnStyleInfo1.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("08eacddc-03ca-41c2-9fde-fec0c6a44e4c", "Provider");
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
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e2087e5e-89c0-4f0b-a4d7-2f471b9cdb6a", "Ex Rate", "Exchange Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "ExchangeRate";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("dec5bf4d-4276-45a4-a96f-e7b193dedfd4", "Inverse Ex Rate", "Inverse Exchange Rate");
			zCalcEditColumnStyleInfo3.ColumnName = "ExchangeRateInverted";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("524b8837-d236-4638-bf27-1b33dfa26fa0", "Funding Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "FromAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f57aee71-cb84-43d2-9f0b-3e96b01b1af2", "Fee", "Processing Fee");
			zCalcEditColumnStyleInfo5.ColumnName = "FeeAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("183798aa-50d0-495f-a9b8-5be67c190c20", "Error", "Error Message");
			zTextBoxColumnStyleInfo4.ColumnName = "ErrorDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9a1af095-8b41-4219-a362-158a6e1f1b13", "Provider Ref", "Provider Reference");
			zTextBoxColumnStyleInfo5.ColumnName = "ProviderReference";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ef8f0aef-f00d-4587-932f-e6896adad9d4", "Received", "Received Date/Time");
			zDateEditColumnStyleInfo1.ColumnName = "LastResponseReceivedLocalTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1d043c45-888f-43a7-b82f-dec4c365598c", "Quote #", "Quote Number");
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
			this.EPaymentGrid.GridId = "899A53B8-F626-4E33-B63F-72C542C8E47C";
			this.EPaymentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EPaymentGrid.LayoutKey = "EPaymentGrid";
			this.EPaymentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 52, true);
			this.EPaymentGrid.Name = "EPaymentGrid";
			this.EPaymentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 164, true);
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
			this.DealGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 228, true);
			this.DealGroupBox.Name = "DealGroupBox";
			this.DealGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 103, true);
			this.DealGroupBox.TabIndex = 2;
			this.DealGroupBox.TabStop = false;
			// 
			// DealProviderTextBox
			// 
			this.BindingSource.SetBindingMember(this.DealProviderTextBox, "DealProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentBase)(null)).DealProvider)));
			this.DealProviderTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1b25e24f-9562-45a1-ab87-b786809edd21", "Provider");
			this.DealProviderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 25, true);
			this.DealProviderTextBox.Name = "DealProviderTextBox";
			this.DealProviderTextBox.ReadOnly = true;
			this.DealProviderTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DealProviderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.DealProviderTextBox.TabIndex = 1;
			// 
			// DealReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.DealReferenceTextBox, "DealProviderReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentBase)(null)).DealProviderReference)));
			this.DealReferenceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7e44115e-f840-4f58-8f2a-abb721005ab7", "Provider Reference");
			this.DealReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 49, true);
			this.DealReferenceTextBox.Name = "DealReferenceTextBox";
			this.DealReferenceTextBox.ReadOnly = true;
			this.DealReferenceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DealReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.DealReferenceTextBox.TabIndex = 2;
			// 
			// DealStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.DealStatusTextBox, "DealStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentBase)(null)).DealStatusDescription)));
			this.DealStatusTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d9d7923a-07fc-4501-ab6e-03dd2c0ccdfe", "Status");
			this.DealStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 73, true);
			this.DealStatusTextBox.Name = "DealStatusTextBox";
			this.DealStatusTextBox.ReadOnly = true;
			this.DealStatusTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DealStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.DealStatusTextBox.TabIndex = 3;
			// 
			// DealSubmittedDateEdit
			// 
			this.DealSubmittedDateEdit.AllowDrop = true;
			this.DealSubmittedDateEdit.AutoCompleteMonthThreshold = 1;
			this.DealSubmittedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DealSubmittedDateEdit, "DealSubmittedLocalTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentBase)(null)).DealSubmittedLocalTime)));
			this.DealSubmittedDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("60a4f04f-8a95-4f9e-aba7-f44677a2b88c", "Submitted");
			this.DealSubmittedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(485, 25, true);
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentBase)(null)).DealLastResponseLocalTime)));
			this.DealResponseDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0ca34483-3fa7-4834-88ba-c2176454b1cd", "Last Response");
			this.DealResponseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(485, 49, true);
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
			this.DealCostCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(703, 25, true);
			this.DealCostCalcFindBox.Name = "DealCostCalcFindBox";
			this.DealCostCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.DealCostCalcFindBox.TabIndex = 6;
			// 
			// DealErrorTextBox
			// 
			this.BindingSource.SetBindingMember(this.DealErrorTextBox, "DealErrorMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentBase)(null)).DealErrorMessage)));
			this.DealErrorTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2a0843bc-7630-4767-ac07-e9ee2d9e691a", "Message");
			this.DealErrorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 73, true);
			this.DealErrorTextBox.Name = "DealErrorTextBox";
			this.DealErrorTextBox.ReadOnly = true;
			this.DealErrorTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DealErrorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(374, 20, true);
			this.DealErrorTextBox.TabIndex = 7;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 615, true);
			this.WorkflowTabPage.TabIndex = 1;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 615, true);
			this.zStmNoteTabPage1.TabIndex = 3;
			// 
			// EventTabPage
			// 
			this.EventTabPage.ExcludeFromBindingOnSave = true;
			this.EventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EventTabPage.Name = "EventTabPage";
			this.EventTabPage.ShouldBeReadOnlyInViewMode = false;
			this.EventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 635, true);
			this.EventTabPage.TabIndex = 2;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.ProcessEPaymentButton);
			this.BottomPanel.Controls.Add(this.CloseButton);
			this.BottomPanel.Controls.Add(this.PaymentDetailButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 662, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 33, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// ProcessEPaymentButton
			// 
			this.ProcessEPaymentButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ProcessEPaymentButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2997bc73-4ee4-49a7-befc-90ba908c8d96", "Process E-Payment");
			this.ProcessEPaymentButton.IsCaptionOverridden = false;
			this.ProcessEPaymentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
			this.ProcessEPaymentButton.Name = "ProcessEPaymentButton";
			this.ProcessEPaymentButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ProcessEPaymentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 24, true);
			this.ProcessEPaymentButton.TabIndex = 0;
			this.ProcessEPaymentButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ProcessEPaymentButton.ToolTipCaption = null;
			this.ProcessEPaymentButton.UseVisualStyleBackColor = true;
			this.ProcessEPaymentButton.Click += new EventHandler(this.ProcessEPaymentButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|1ae26694-a964-4f63-b57f-888ad2fbee27", "Close");
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(838, 4, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			// 
			// PaymentDetailButton
			// 
			this.PaymentDetailButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PaymentDetailButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|302e28f9-3efe-4969-b7ba-e5c80469f812", "Payment Detail");
			this.PaymentDetailButton.IsCaptionOverridden = false;
			this.PaymentDetailButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(742, 4, true);
			this.PaymentDetailButton.Name = "PaymentDetailButton";
			this.PaymentDetailButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PaymentDetailButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.PaymentDetailButton.TabIndex = 1;
			this.PaymentDetailButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.PaymentDetailButton.ToolTipCaption = null;
			// 
			// PaymentForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentForm|7e43c903-7279-4df1-b7c3-f4d48e5dee71", "Payment Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 728, true);
			this.Controls.Add(this.TabControl);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(PaymentBase);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.ReceiptPayment.Payment";
			this.Name = "PaymentForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.TabControl, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.PaymentDetailsGroupBox.ResumeLayout(false);
			this.PaymentDetailsGroupBox.PerformLayout();
			this.AddressWithContactControl.ResumeLayout(true);
			this.AddressWithContactControl.PerformLayout();
			this.UnmatchDateEdit.ResumeLayout(true);
			this.UnmatchDateEdit.PerformLayout();
			this.OrganizationGuidFindBox.ResumeLayout(true);
			this.OrganizationGuidFindBox.PerformLayout();
			this.PostDateEdit.ResumeLayout(true);
			this.PostDateEdit.PerformLayout();
			this.InvoiceDateEdit.ResumeLayout(true);
			this.InvoiceDateEdit.PerformLayout();
			this.BankDetailsGroupBox.ResumeLayout(false);
			this.BankDetailsGroupBox.PerformLayout();
			this.PaymentReasonDropEdit.ResumeLayout(true);
			this.PaymentReasonDropEdit.PerformLayout();
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
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
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
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
