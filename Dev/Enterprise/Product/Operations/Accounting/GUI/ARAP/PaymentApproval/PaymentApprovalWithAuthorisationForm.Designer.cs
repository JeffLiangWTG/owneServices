using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Core.Forms;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class PaymentApprovalWithAuthorisationForm
	{


		#region Windows Form Designer generated code

		protected ZLabel zLabel10;
		protected ZLabel zLabel9;
		protected ZLabel zLabel8;
		protected ZButton SecondAuthorisationButton;
		protected ZButton ThirdAuthorisationButton;
		protected ZGroupBox AuthorisationGroupBox;
		protected ZLabel Label;
		protected ZLabel AuthorisationTypeLabel;
		protected ZButton PaymentDetailButton;
		protected ZDropEdit StatusDropEdit;
		protected ZGroupBox PaymentDetailsGroupBox;
		protected ZGuidFindBox OrganizationGuidFindBox;
		protected ZTextBox DescriptionTextBox;
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
		protected ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
		private IContainer components;
		protected ZGuidFindBox FundingBankAccountFindBox;
		private ZCodeFindBox FundingCurrencyCodeFindBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new Container();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			this.AH_NumberOfSupportingDocumentsCalcEdit = new ZCalcEdit();
			this.PaymentDetailsGroupBox = new ZGroupBox();
			this.StatusDropEdit = new ZDropEdit();
			this.PaymentNoTextBox = new ZTextBox();
			this.PostDateEdit = new ZDateEdit();
			this.InvoiceDateEdit = new ZDateEdit();
			this.DescriptionTextBox = new ZTextBox();
			this.OrganizationGuidFindBox = new ZGuidFindBox();
			this.AddressWithContactControl = new PaymentAddressWithContactControl();
			this.BankDetailsGroupBox = new ZGroupBox();
			this.PaymentReasonDropEdit = new ZDropEdit();
			this.ServiceProviderLabel = new ZLabel();
			this.AutoAllocateZLabel = new ZLabel();
			this.AutoPrintZLabel = new ZLabel();
			this.ProviderLogoPictureBox = new ZPictureBox();
			this.ChequeNoTextBox = new ZTextBox();
			this.ChequeBookGuidFindBox = new ZGuidFindBox();
			this.DisclaimerMessageLabel = new ZLabel();
			this.BankAccountGuidFindBox = new ZGuidFindBox();
			this.PaymentTypeDropEdit = new ZDropEdit();
			this.PaymentAmountGroupBox = new ZGroupBox();
			this.CheckExRateButton = new ZButton();
			this.PaymentDetailButton = new ZButton();
			this.LearnMoreButton = new ZButton();
			this.LocalAmountCalcFindBox = new ZCalcFindBox();
			this.PaymentAmountCalcFindBox = new ZCalcFindBox();
			this.ExchangeRateControl = new ZExchangeRateControl();
			this.zTabControl1 = new ZTemplateTabControl();
			this.PaymentTabPage = new ZTabPage();
			this.TabControlBankAndEPayment = new ZTabControl();
			this.BankDetailsTab = new ZTabPage();
			this.AuthorizationTab = new ZTabPage();
			this.AuthorisationGroupBox = new ZGroupBox();
			this.ThirdAuthorisationStaffCodeFindBox = new ZCodeFindBox();
			this.SecondAuthorisationStaffCodeFindBox = new ZCodeFindBox();
			this.FirstAuthorisationStaffCodeFindBox = new ZCodeFindBox();
			this.AuthorisationTypeLabel = new ZLabel();
			this.FirstAuthorisationButton = new ZButton();
			this.zLabel10 = new ZLabel();
			this.zLabel9 = new ZLabel();
			this.zLabel8 = new ZLabel();
			this.ThirdAuthorisationButton = new ZButton();
			this.SecondAuthorisationButton = new ZButton();
			this.Authorisation3rdLabel = new ZLabel();
			this.Authorisation2ndLabel = new ZLabel();
			this.Authorisation1stLabel = new ZLabel();
			this.Label = new ZLabel();
			this.RejectGroupBox = new ZGroupBox();
			this.RejectReasonTextBox = new ZTextBox();
			this.RejectReasonCodeDropEdit = new ZDropEdit();
			this.RejectButton = new ZButton();
			this.EPaymentTab = new ZTabPage();
			this.zGroupBox1 = new ZGroupBox();
			this.AcceptQuoteButton = new ZButton();
			this.refreshButton = new ZButton();
			this.zGrid1 = new ZGrid();
			this.DealGroupBox = new ZGroupBox();
			this.DealProviderTextBox = new ZTextBox();
			this.DealReferenceTextBox = new ZTextBox();
			this.DealStatusTextBox = new ZTextBox();
			this.DealSubmittedDateEdit = new ZDateEdit();
			this.DealResponseDateEdit = new ZDateEdit();
			this.DealCostCalcFindBox = new ZCalcFindBox();
			this.DealErrorTextBox = new ZTextBox();
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.EventTabPage = new ZLogsTabPage();
			this.PostingButtonsUserControl = new ZPostingButtonsUserControl();
			this.ProcessEPaymentButton = new ZButton();
			this.FundingBankAccountFindBox = new ZGuidFindBox();
			this.FundingCurrencyCodeFindBox = new ZCodeFindBox();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PaymentDetailsGroupBox.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.PostDateEdit.SuspendLayout();
			this.InvoiceDateEdit.SuspendLayout();
			this.OrganizationGuidFindBox.SuspendLayout();
			this.AddressWithContactControl.SuspendLayout();
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
			this.TabControlBankAndEPayment.SuspendLayout();
			this.BankDetailsTab.SuspendLayout();
			this.AuthorizationTab.SuspendLayout();
			this.AuthorisationGroupBox.SuspendLayout();
			this.ThirdAuthorisationStaffCodeFindBox.SuspendLayout();
			this.SecondAuthorisationStaffCodeFindBox.SuspendLayout();
			this.FirstAuthorisationStaffCodeFindBox.SuspendLayout();
			this.RejectGroupBox.SuspendLayout();
			this.RejectReasonCodeDropEdit.SuspendLayout();
			this.EPaymentTab.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			((ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.DealGroupBox.SuspendLayout();
			this.DealSubmittedDateEdit.SuspendLayout();
			this.DealResponseDateEdit.SuspendLayout();
			this.DealCostCalcFindBox.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.FundingBankAccountFindBox.SuspendLayout();
			this.FundingCurrencyCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 743, true);
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
			this.AH_NumberOfSupportingDocumentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 39, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.Name = "AH_NumberOfSupportingDocumentsCalcEdit";
			this.AH_NumberOfSupportingDocumentsCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.TabIndex = 4;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Text = "0";
			this.AH_NumberOfSupportingDocumentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PaymentDetailsGroupBox
			// 
			this.PaymentDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|4119862e-05af-4e05-9beb-fd3916c460c9", "Payment Details");
			this.PaymentDetailsGroupBox.Controls.Add(this.AH_NumberOfSupportingDocumentsCalcEdit);
			this.PaymentDetailsGroupBox.Controls.Add(this.StatusDropEdit);
			this.PaymentDetailsGroupBox.Controls.Add(this.PaymentNoTextBox);
			this.PaymentDetailsGroupBox.Controls.Add(this.PostDateEdit);
			this.PaymentDetailsGroupBox.Controls.Add(this.InvoiceDateEdit);
			this.PaymentDetailsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.PaymentDetailsGroupBox.Controls.Add(this.OrganizationGuidFindBox);
			this.PaymentDetailsGroupBox.Controls.Add(this.AddressWithContactControl);
			this.PaymentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
			this.PaymentDetailsGroupBox.Name = "PaymentDetailsGroupBox";
			this.PaymentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(913, 176, true);
			this.PaymentDetailsGroupBox.TabIndex = 0;
			this.PaymentDetailsGroupBox.TabStop = false;
			// 
			// FundingCurrencyCodeFindBox
			// 
			this.FundingCurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FundingCurrencyCodeFindBox, "FundingCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).FundingCurrency)));
			this.FundingCurrencyCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|a5afbe87-e264-4abd-a181-2f47dfe6f677", "Funding Currency");
			this.FundingCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 149, true);
			this.FundingCurrencyCodeFindBox.Name = "FundingCurrencyCodeFindBox";
			this.FundingCurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FundingCurrencyCodeFindBox.ParentType = null;
			this.FundingCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 20, true);
			this.FundingCurrencyCodeFindBox.TabIndex = 14;
			this.FundingCurrencyCodeFindBox.Visible = false;
			// 
			// FundingBankAccountFindBox
			// 
			this.FundingBankAccountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FundingBankAccountFindBox, "FundingBankAccountPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PaymentApprovalWithAuthorisation)(null)).FundingBankAccountPK)));
			this.FundingBankAccountFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|92983ebd-0474-4b8b-91ab-9a97788f2e47", "Funding Bank Account");
			this.FundingBankAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 149, true);
			this.FundingBankAccountFindBox.Name = "FundingBankAccountFindBox";
			this.FundingBankAccountFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FundingBankAccountFindBox.ParentType = null;
			this.FundingBankAccountFindBox.PopupCaption = null;
			this.FundingBankAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 20, true);
			this.FundingBankAccountFindBox.TabIndex = 13;
			this.FundingBankAccountFindBox.Visible = false;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "AV_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentApprovalWithAuthorisation)(null)).AV_Status)));
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 94, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.ShouldResizeByMaxLength = true;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.StatusDropEdit.TabIndex = 8;
			// 
			// PaymentNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.PaymentNoTextBox, "HeaderTransactionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).HeaderTransactionNumber)));
			this.PaymentNoTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|dbf367de-37a8-45da-b613-1adab234cad4", "Payment", "Payment", "");
			this.PaymentNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 68, true);
			this.PaymentNoTextBox.Name = "PaymentNoTextBox";
			this.PaymentNoTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.PaymentNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 20, true);
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
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "AV_PaymentComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).AV_PaymentComment)));
			this.DescriptionTextBox.CaptionResourceString = null;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 121, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 20, true);
			this.DescriptionTextBox.TabIndex = 9;
			// 
			// OrganizationGuidFindBox
			// 
			this.OrganizationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganizationGuidFindBox, "AV_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PaymentApprovalWithAuthorisation)(null)).AV_OH)));
			this.OrganizationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(523, 16, true);
			this.OrganizationGuidFindBox.Name = "OrganizationGuidFindBox";
			this.OrganizationGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OrganizationGuidFindBox.ParentType = null;
			this.OrganizationGuidFindBox.PopupCaption = null;
			this.OrganizationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 20, true);
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
			this.AddressWithContactControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(523, 36, true);
			this.AddressWithContactControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 132, true);
			this.AddressWithContactControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 132, true);
			this.AddressWithContactControl.Name = "AddressWithContactControl";
			this.AddressWithContactControl.OnlyStopOnDebtor = true;
			this.AddressWithContactControl.PopupCaption = "";
			this.AddressWithContactControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 132, true);
			this.AddressWithContactControl.TabIndex = 11;
			// 
			// BankDetailsGroupBox
			// 
			this.BankDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|e17fb6cc-66db-4fda-9e25-1a1c9ba9030f", "Bank Details");
			this.BankDetailsGroupBox.Controls.Add(this.PaymentReasonDropEdit);
			this.BankDetailsGroupBox.Controls.Add(this.ServiceProviderLabel);
			this.BankDetailsGroupBox.Controls.Add(this.AutoAllocateZLabel);
			this.BankDetailsGroupBox.Controls.Add(this.AutoPrintZLabel);
			this.BankDetailsGroupBox.Controls.Add(this.ProviderLogoPictureBox);
			this.BankDetailsGroupBox.Controls.Add(this.ChequeNoTextBox);
			this.BankDetailsGroupBox.Controls.Add(this.ChequeBookGuidFindBox);
			this.BankDetailsGroupBox.Controls.Add(this.DisclaimerMessageLabel);
			this.BankDetailsGroupBox.Controls.Add(this.BankAccountGuidFindBox);
			this.BankDetailsGroupBox.Controls.Add(this.PaymentTypeDropEdit);
			this.BankDetailsGroupBox.Controls.Add(this.FundingCurrencyCodeFindBox);
			this.BankDetailsGroupBox.Controls.Add(this.FundingBankAccountFindBox);
			this.BankDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 6, true);
			this.BankDetailsGroupBox.Name = "BankDetailsGroupBox";
			this.BankDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(893, 177, true);
			this.BankDetailsGroupBox.TabIndex = 1;
			this.BankDetailsGroupBox.TabStop = false;
			// 
			// PaymentReasonDropEdit
			// 
			this.PaymentReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentReasonDropEdit, "AV_EPaymentReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentApprovalWithAuthorisation)(null)).AV_EPaymentReasonCode)));
			this.PaymentReasonDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f1711ff2-b4ec-4549-9ade-8490969a1b73", "Payment Reason");
			this.PaymentReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 123, true);
			this.PaymentReasonDropEdit.Name = "PaymentReasonDropEdit";
			this.PaymentReasonDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 20, true);
			this.PaymentReasonDropEdit.TabIndex = 12;
			// 
			// ServiceProviderLabel
			// 
			this.ServiceProviderLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4ec0a6e9-5ae5-4dba-a8a1-640bdb6f6234", "Service Provider");
			this.ServiceProviderLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ServiceProviderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(698, 87, true);
			this.ServiceProviderLabel.Name = "ServiceProviderLabel";
			this.ServiceProviderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.ServiceProviderLabel.TabIndex = 7;
			// 
			// AutoAllocateZLabel
			// 
			this.AutoAllocateZLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoAllocateZLabel, "AV_Calc_ChequeNumberIsAutoAllocatedLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).AV_Calc_ChequeNumberIsAutoAllocatedLabel)));
			this.AutoAllocateZLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|09c21257-eeea-424c-9ebf-05c9a584e089", "Check Number Auto Allocated on Posting");
			this.AutoAllocateZLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AutoAllocateZLabel.ForeColor = System.Drawing.Color.Red;
			this.AutoAllocateZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 100, true);
			this.AutoAllocateZLabel.Name = "AutoAllocateZLabel";
			this.AutoAllocateZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 13, true);
			this.AutoAllocateZLabel.TabIndex = 11;
			// 
			// AutoPrintZLabel
			// 
			this.AutoPrintZLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoPrintZLabel, "AV_Calc_ChequeIsAutoPrintedLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).AV_Calc_ChequeIsAutoPrintedLabel)));
			this.AutoPrintZLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|5f4b301c-ca61-4036-81ed-94f54bb00656", "Auto Print");
			this.AutoPrintZLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AutoPrintZLabel.ForeColor = System.Drawing.Color.Red;
			this.AutoPrintZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 76, true);
			this.AutoPrintZLabel.Name = "AutoPrintZLabel";
			this.AutoPrintZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 13, true);
			this.AutoPrintZLabel.TabIndex = 10;
			// 
			// ProviderLogoPictureBox
			// 
			this.ProviderLogoPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(804, 82, true);
			this.ProviderLogoPictureBox.Name = "ProviderLogoPictureBox";
			this.ProviderLogoPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 35, true);
			this.ProviderLogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.ProviderLogoPictureBox.TabIndex = 6;
			this.ProviderLogoPictureBox.TabStop = false;
			this.ProviderLogoPictureBox.Click += new EventHandler(this.ProviderLogoPictureBox_Click);
			// 
			// ChequeNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChequeNoTextBox, "AV_ChequeOrReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).AV_ChequeOrReference)));
			this.ChequeNoTextBox.CaptionResourceString = null;
			this.ChequeNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 97, true);
			this.ChequeNoTextBox.Name = "ChequeNoTextBox";
			this.ChequeNoTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ChequeNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
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
			this.ChequeBookGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.ChequeBookGuidFindBox.TabIndex = 5;
			// 
			// DisclaimerMessageLabel
			// 
			this.DisclaimerMessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1e74c820-1894-4dd7-9235-1369144ba7c2", "E-Payment Disclaimer Message");
			this.DisclaimerMessageLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DisclaimerMessageLabel.ForeColor = System.Drawing.SystemColors.Highlight;
			this.DisclaimerMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(534, 16, true);
			this.DisclaimerMessageLabel.Name = "DisclaimerMessageLabel";
			this.DisclaimerMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 59, true);
			this.DisclaimerMessageLabel.TabIndex = 4;
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
			this.BankAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 20, true);
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
			this.PaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 20, true);
			this.PaymentTypeDropEdit.TabIndex = 1;
			// 
			// PaymentAmountGroupBox
			// 
			this.PaymentAmountGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|360f72c8-8ffa-4203-9db1-11647073cf02", "Payment Amount");
			this.PaymentAmountGroupBox.Controls.Add(this.CheckExRateButton);
			this.PaymentAmountGroupBox.Controls.Add(this.PaymentDetailButton);
			this.PaymentAmountGroupBox.Controls.Add(this.LearnMoreButton);
			this.PaymentAmountGroupBox.Controls.Add(this.LocalAmountCalcFindBox);
			this.PaymentAmountGroupBox.Controls.Add(this.PaymentAmountCalcFindBox);
			this.PaymentAmountGroupBox.Controls.Add(this.ExchangeRateControl);
			this.PaymentAmountGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 186, true);
			this.PaymentAmountGroupBox.Name = "PaymentAmountGroupBox";
			this.PaymentAmountGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(893, 115, true);
			this.PaymentAmountGroupBox.TabIndex = 2;
			this.PaymentAmountGroupBox.TabStop = false;
			// 
			// CheckExRateButton
			// 
			this.CheckExRateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5034b080-6b9c-44e5-90f8-381c8ac56bdd", "Check Ex Rate for E-Payment");
			this.CheckExRateButton.IsCaptionOverridden = false;
			this.CheckExRateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 81, true);
			this.CheckExRateButton.Name = "CheckExRateButton";
			this.CheckExRateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CheckExRateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 23, true);
			this.CheckExRateButton.TabIndex = 5;
			this.CheckExRateButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CheckExRateButton.ToolTipCaption = null;
			this.CheckExRateButton.UseVisualStyleBackColor = true;
			this.CheckExRateButton.Click += new EventHandler(this.CheckExRateButton_Click);
			// 
			// PaymentDetailButton
			// 
			this.PaymentDetailButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|bf860ad0-8997-41d0-a7ad-40ecfd9ee766", "Payment Detail");
			this.PaymentDetailButton.IsCaptionOverridden = false;
			this.PaymentDetailButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 21, true);
			this.PaymentDetailButton.Name = "PaymentDetailButton";
			this.PaymentDetailButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PaymentDetailButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 23, true);
			this.PaymentDetailButton.TabIndex = 3;
			this.PaymentDetailButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.PaymentDetailButton.ToolTipCaption = null;
			this.PaymentDetailButton.UseVisualStyleBackColor = true;
			this.PaymentDetailButton.Click += new EventHandler(this.PaymentDetailButton_Click);
			// 
			// LearnMoreButton
			// 
			this.LearnMoreButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("120cdc41-93ab-4008-bb5c-66e0b2a130a2", "Learn More");
			this.LearnMoreButton.IsCaptionOverridden = false;
			this.LearnMoreButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 81, true);
			this.LearnMoreButton.Name = "LearnMoreButton";
			this.LearnMoreButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.LearnMoreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 23, true);
			this.LearnMoreButton.TabIndex = 6;
			this.LearnMoreButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.LearnMoreButton.ToolTipCaption = null;
			this.LearnMoreButton.UseVisualStyleBackColor = true;
			this.LearnMoreButton.Click += new EventHandler(this.LearnMoreButton_Click);
			// 
			// LocalAmountCalcFindBox
			// 
			this.LocalAmountCalcFindBox.AllowDrop = true;
			this.LocalAmountCalcFindBox.BindToAmount = "AV_Calc_LocalAmount";
			this.LocalAmountCalcFindBox.BindToUnit = "AV_Calc_LocalCurrency";
			this.LocalAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|a298612f-2d3c-4476-b163-f1a8edbb7986", "Local", "Local Amount", "");
			this.LocalAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LocalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 50, true);
			this.LocalAmountCalcFindBox.Name = "LocalAmountCalcFindBox";
			this.LocalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.LocalAmountCalcFindBox.TabIndex = 4;
			// 
			// PaymentAmountCalcFindBox
			// 
			this.PaymentAmountCalcFindBox.AllowDrop = true;
			this.PaymentAmountCalcFindBox.BindToAmount = "AV_Amount";
			this.PaymentAmountCalcFindBox.BindToUnit = "OSCurrencyForDisplay";
			this.PaymentAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.PaymentAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 50, true);
			this.PaymentAmountCalcFindBox.Name = "PaymentAmountCalcFindBox";
			this.PaymentAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.PaymentAmountCalcFindBox.TabIndex = 2;
			// 
			// ExchangeRateControl
			// 
			this.ExchangeRateControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExchangeRateControl, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((PaymentApprovalWithAuthorisation)(null)).ExchangeRate)));
			this.ExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|278c1995-a884-49cd-9091-060a36a39ce0", "Exchange", "Exchange", "");
			this.ExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 25, true);
			this.ExchangeRateControl.Name = "ExchangeRateControl";
			this.ExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.ExchangeRateControl.TabIndex = 1;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Controls.Add(this.PaymentTabPage);
			this.zTabControl1.Controls.Add(this.WorkflowTabPage);
			this.zTabControl1.Controls.Add(this.zStmNoteTabPage1);
			this.zTabControl1.Controls.Add(this.EventTabPage);
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 7, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(931, 649, true);
			this.zTabControl1.TabIndex = 0;
			// 
			// PaymentTabPage
			// 
			this.PaymentTabPage.AutoScroll = true;
			this.PaymentTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|f09e5d3c-5f47-4ae8-ac87-d961dff5b58e", "Payment");
			this.PaymentTabPage.Controls.Add(this.TabControlBankAndEPayment);
			this.PaymentTabPage.Controls.Add(this.PaymentDetailsGroupBox);
			this.PaymentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PaymentTabPage.Name = "PaymentTabPage";
			this.PaymentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 622, true);
			this.PaymentTabPage.TabIndex = 0;
			// 
			// TabControlBankAndEPayment
			// 
			this.TabControlBankAndEPayment.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControlBankAndEPayment.Controls.Add(this.BankDetailsTab);
			this.TabControlBankAndEPayment.Controls.Add(this.AuthorizationTab);
			this.TabControlBankAndEPayment.Controls.Add(this.EPaymentTab);
			this.TabControlBankAndEPayment.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 188, true);
			this.TabControlBankAndEPayment.Name = "TabControlBankAndEPayment";
			this.TabControlBankAndEPayment.SelectedIndex = 0;
			this.TabControlBankAndEPayment.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(913, 363, true);
			this.TabControlBankAndEPayment.TabIndex = 12;
			// 
			// BankDetailsTab
			// 
			this.BankDetailsTab.AutoScroll = true;
			this.BankDetailsTab.BackColor = System.Drawing.SystemColors.Control;
			this.BankDetailsTab.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("33d2e116-d7f2-4486-8d8c-d3da1ce6921b", "Bank Details");
			this.BankDetailsTab.Controls.Add(this.PaymentAmountGroupBox);
			this.BankDetailsTab.Controls.Add(this.BankDetailsGroupBox);
			this.BankDetailsTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BankDetailsTab.Name = "BankDetailsTab";
			this.BankDetailsTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.BankDetailsTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 336, true);
			this.BankDetailsTab.TabIndex = 0;
			// 
			// AuthorizationTab
			// 
			this.AuthorizationTab.BackColor = System.Drawing.SystemColors.Control;
			this.AuthorizationTab.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c78d6b35-bb5f-429c-927c-168f87ab3b85", "Authorization");
			this.AuthorizationTab.Controls.Add(this.AuthorisationGroupBox);
			this.AuthorizationTab.Controls.Add(this.RejectGroupBox);
			this.AuthorizationTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AuthorizationTab.Name = "AuthorizationTab";
			this.AuthorizationTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AuthorizationTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 336, true);
			this.AuthorizationTab.TabIndex = 2;
			// 
			// AuthorisationGroupBox
			// 
			this.AuthorisationGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|27bc4b0c-fa80-405f-a55c-814808561ff8", "Authorization");
			this.AuthorisationGroupBox.Controls.Add(this.ThirdAuthorisationStaffCodeFindBox);
			this.AuthorisationGroupBox.Controls.Add(this.SecondAuthorisationStaffCodeFindBox);
			this.AuthorisationGroupBox.Controls.Add(this.FirstAuthorisationStaffCodeFindBox);
			this.AuthorisationGroupBox.Controls.Add(this.AuthorisationTypeLabel);
			this.AuthorisationGroupBox.Controls.Add(this.FirstAuthorisationButton);
			this.AuthorisationGroupBox.Controls.Add(this.zLabel10);
			this.AuthorisationGroupBox.Controls.Add(this.zLabel9);
			this.AuthorisationGroupBox.Controls.Add(this.zLabel8);
			this.AuthorisationGroupBox.Controls.Add(this.ThirdAuthorisationButton);
			this.AuthorisationGroupBox.Controls.Add(this.SecondAuthorisationButton);
			this.AuthorisationGroupBox.Controls.Add(this.Authorisation3rdLabel);
			this.AuthorisationGroupBox.Controls.Add(this.Authorisation2ndLabel);
			this.AuthorisationGroupBox.Controls.Add(this.Authorisation1stLabel);
			this.AuthorisationGroupBox.Controls.Add(this.Label);
			this.AuthorisationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.AuthorisationGroupBox.Name = "AuthorisationGroupBox";
			this.AuthorisationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 163, true);
			this.AuthorisationGroupBox.TabIndex = 3;
			this.AuthorisationGroupBox.TabStop = false;
			// 
			// ThirdAuthorisationStaffCodeFindBox
			// 
			this.ThirdAuthorisationStaffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ThirdAuthorisationStaffCodeFindBox, "AV_GS_NKApproval3rd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).AV_GS_NKApproval3rd)));
			this.ThirdAuthorisationStaffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 131, true);
			this.ThirdAuthorisationStaffCodeFindBox.Name = "ThirdAuthorisationStaffCodeFindBox";
			this.ThirdAuthorisationStaffCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ThirdAuthorisationStaffCodeFindBox.ParentType = null;
			this.ThirdAuthorisationStaffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.ThirdAuthorisationStaffCodeFindBox.TabIndex = 18;
			// 
			// SecondAuthorisationStaffCodeFindBox
			// 
			this.SecondAuthorisationStaffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecondAuthorisationStaffCodeFindBox, "AV_GS_NKApproval2nd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).AV_GS_NKApproval2nd)));
			this.SecondAuthorisationStaffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 100, true);
			this.SecondAuthorisationStaffCodeFindBox.Name = "SecondAuthorisationStaffCodeFindBox";
			this.SecondAuthorisationStaffCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SecondAuthorisationStaffCodeFindBox.ParentType = null;
			this.SecondAuthorisationStaffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.SecondAuthorisationStaffCodeFindBox.TabIndex = 17;
			// 
			// FirstAuthorisationStaffCodeFindBox
			// 
			this.FirstAuthorisationStaffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FirstAuthorisationStaffCodeFindBox, "AV_GS_NKApproval1st");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).AV_GS_NKApproval1st)));
			this.FirstAuthorisationStaffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 69, true);
			this.FirstAuthorisationStaffCodeFindBox.Name = "FirstAuthorisationStaffCodeFindBox";
			this.FirstAuthorisationStaffCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FirstAuthorisationStaffCodeFindBox.ParentType = null;
			this.FirstAuthorisationStaffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.FirstAuthorisationStaffCodeFindBox.TabIndex = 7;
			// 
			// AuthorisationTypeLabel
			// 
			this.AuthorisationTypeLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AuthorisationTypeLabel, "AV_Calc_DescriptionOfAuthorisationRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MultilingualString)(((PaymentApprovalWithAuthorisation)(null)).AV_Calc_DescriptionOfAuthorisationRequired)));
			this.AuthorisationTypeLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|a1f2fae5-04d2-4993-97ab-f827f9f996b5", "Please enter an amount");
			this.AuthorisationTypeLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.AuthorisationTypeLabel.IsFontBold = true;
			this.AuthorisationTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 19, true);
			this.AuthorisationTypeLabel.Name = "AuthorisationTypeLabel";
			this.AuthorisationTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 13, true);
			this.AuthorisationTypeLabel.TabIndex = 1;
			this.AuthorisationTypeLabel.Text = "Please enter an amount";
			// 
			// FirstAuthorisationButton
			// 
			this.FirstAuthorisationButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|aae4b187-c969-4282-8de0-433dbcc8e048", "Authorize");
			this.FirstAuthorisationButton.IsCaptionOverridden = false;
			this.FirstAuthorisationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 67, true);
			this.FirstAuthorisationButton.Name = "FirstAuthorisationButton";
			this.FirstAuthorisationButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.FirstAuthorisationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.FirstAuthorisationButton.TabIndex = 8;
			this.FirstAuthorisationButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.FirstAuthorisationButton.ToolTipCaption = null;
			this.FirstAuthorisationButton.UseVisualStyleBackColor = true;
			this.FirstAuthorisationButton.Click += new EventHandler(this.FirstAuthorisationButton_Click);
			// 
			// zLabel10
			// 
			this.zLabel10.AutoSize = true;
			this.zLabel10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|861e8cd4-2093-4ea9-a207-4191671b15ac", "Actions");
			this.zLabel10.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel10.IsFontBold = true;
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 43, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel10.TabIndex = 4;
			// 
			// zLabel9
			// 
			this.zLabel9.AutoSize = true;
			this.zLabel9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|d15f7ffc-b052-4736-adcf-8a2a142c22c9", "Status");
			this.zLabel9.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel9.IsFontBold = true;
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 43, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel9.TabIndex = 2;
			// 
			// zLabel8
			// 
			this.zLabel8.AutoSize = true;
			this.zLabel8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|8d5d7fbb-6371-4a40-be2e-cefeb3fafa13", "Authorization Details");
			this.zLabel8.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel8.IsFontBold = true;
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 43, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel8.TabIndex = 3;
			// 
			// ThirdAuthorisationButton
			// 
			this.ThirdAuthorisationButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|bc25722d-ad1b-4d4b-85fd-cc81a3880a71", "Authorize");
			this.ThirdAuthorisationButton.IsCaptionOverridden = false;
			this.ThirdAuthorisationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 129, true);
			this.ThirdAuthorisationButton.Name = "ThirdAuthorisationButton";
			this.ThirdAuthorisationButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ThirdAuthorisationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ThirdAuthorisationButton.TabIndex = 16;
			this.ThirdAuthorisationButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ThirdAuthorisationButton.ToolTipCaption = null;
			this.ThirdAuthorisationButton.UseVisualStyleBackColor = true;
			this.ThirdAuthorisationButton.Click += new EventHandler(this.ThirdAuthorisationButton_Click);
			// 
			// SecondAuthorisationButton
			// 
			this.SecondAuthorisationButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|c2116048-92c9-44a4-8f46-bc3e7fec6425", "Authorize");
			this.SecondAuthorisationButton.IsCaptionOverridden = false;
			this.SecondAuthorisationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 98, true);
			this.SecondAuthorisationButton.Name = "SecondAuthorisationButton";
			this.SecondAuthorisationButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SecondAuthorisationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SecondAuthorisationButton.TabIndex = 12;
			this.SecondAuthorisationButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SecondAuthorisationButton.ToolTipCaption = null;
			this.SecondAuthorisationButton.UseVisualStyleBackColor = true;
			this.SecondAuthorisationButton.Click += new EventHandler(this.SecondAuthorisationButton_Click);
			// 
			// Authorisation3rdLabel
			// 
			this.Authorisation3rdLabel.AutoSize = true;
			this.Authorisation3rdLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|73ba52a0-8af6-4317-980a-8a652133c57b", "3rd");
			this.Authorisation3rdLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Authorisation3rdLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 134, true);
			this.Authorisation3rdLabel.Name = "Authorisation3rdLabel";
			this.Authorisation3rdLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.Authorisation3rdLabel.TabIndex = 13;
			// 
			// Authorisation2ndLabel
			// 
			this.Authorisation2ndLabel.AutoSize = true;
			this.Authorisation2ndLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|ecb829c7-77a7-4803-bcf1-d8233e7b1676", "2nd");
			this.Authorisation2ndLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Authorisation2ndLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 103, true);
			this.Authorisation2ndLabel.Name = "Authorisation2ndLabel";
			this.Authorisation2ndLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.Authorisation2ndLabel.TabIndex = 9;
			// 
			// Authorisation1stLabel
			// 
			this.Authorisation1stLabel.AutoSize = true;
			this.Authorisation1stLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|41034410-2788-493e-be6c-c9a7dbf42ed2", "1st");
			this.Authorisation1stLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Authorisation1stLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 72, true);
			this.Authorisation1stLabel.Name = "Authorisation1stLabel";
			this.Authorisation1stLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.Authorisation1stLabel.TabIndex = 5;
			// 
			// Label
			// 
			this.Label.AutoSize = true;
			this.Label.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|e45dcc7d-6c4c-4b21-b278-738878c14077", "Authorization Required");
			this.Label.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 19, true);
			this.Label.Name = "Label";
			this.Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.Label.TabIndex = 0;
			// 
			// RejectGroupBox
			// 
			this.RejectGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|e337de17-9150-4e86-8775-4a11a4d52cba", "Rejection");
			this.RejectGroupBox.Controls.Add(this.RejectReasonTextBox);
			this.RejectGroupBox.Controls.Add(this.RejectReasonCodeDropEdit);
			this.RejectGroupBox.Controls.Add(this.RejectButton);
			this.RejectGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 175, true);
			this.RejectGroupBox.Name = "RejectGroupBox";
			this.RejectGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 91, true);
			this.RejectGroupBox.TabIndex = 4;
			this.RejectGroupBox.TabStop = false;
			// 
			// RejectReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.RejectReasonTextBox, "AV_RejectionReasonDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).AV_RejectionReasonDetails)));
			this.RejectReasonTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|96bd7887-58c7-4c03-8321-c382ab9d8f05", "Rejection Details");
			this.RejectReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 56, true);
			this.RejectReasonTextBox.Name = "RejectReasonTextBox";
			this.RejectReasonTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.RejectReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.RejectReasonTextBox.TabIndex = 20;
			// 
			// RejectReasonCodeDropEdit
			// 
			this.RejectReasonCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RejectReasonCodeDropEdit, "AV_RejectionReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PaymentApprovalWithAuthorisation)(null)).AV_RejectionReasonCode)));
			this.RejectReasonCodeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|d4b1bf1e-3589-4743-8c1a-2a0feb73d622", "Reject Reason");
			this.RejectReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 25, true);
			this.RejectReasonCodeDropEdit.Name = "RejectReasonCodeDropEdit";
			this.RejectReasonCodeDropEdit.ShouldResizeByMaxLength = true;
			this.RejectReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.RejectReasonCodeDropEdit.TabIndex = 19;
			// 
			// RejectButton
			// 
			this.RejectButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|3036c909-28d7-451a-8afc-2a1ad072472a", "Reject");
			this.RejectButton.IsCaptionOverridden = false;
			this.RejectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 23, true);
			this.RejectButton.Name = "RejectButton";
			this.RejectButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RejectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.RejectButton.TabIndex = 21;
			this.RejectButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RejectButton.ToolTipCaption = null;
			this.RejectButton.UseVisualStyleBackColor = true;
			this.RejectButton.Click += new EventHandler(this.RejectButton_Click);
			// 
			// EPaymentTab
			// 
			this.EPaymentTab.AutoScroll = true;
			this.EPaymentTab.BackColor = System.Drawing.SystemColors.Control;
			this.EPaymentTab.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1f6ef0b8-13b3-4966-9bad-ae9aa2bf5db9", "E-Payment Processing");
			this.EPaymentTab.Controls.Add(this.zGroupBox1);
			this.EPaymentTab.Controls.Add(this.DealGroupBox);
			this.EPaymentTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EPaymentTab.Name = "EPaymentTab";
			this.EPaymentTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EPaymentTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 336, true);
			this.EPaymentTab.TabIndex = 1;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a65dfd53-d25c-4ef6-89af-37d76eda33dc", "E-Quote");
			this.zGroupBox1.Controls.Add(this.AcceptQuoteButton);
			this.zGroupBox1.Controls.Add(this.refreshButton);
			this.zGroupBox1.Controls.Add(this.zGrid1);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 6, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(893, 219, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			// 
			// AcceptQuoteButton
			// 
			this.AcceptQuoteButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e4b477a5-0da3-4d1f-80fb-98aac0c16b67", "Accept Quote");
			this.AcceptQuoteButton.IsCaptionOverridden = false;
			this.AcceptQuoteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 20, true);
			this.AcceptQuoteButton.Name = "AcceptQuoteButton";
			this.AcceptQuoteButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AcceptQuoteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 26, true);
			this.AcceptQuoteButton.TabIndex = 2;
			this.AcceptQuoteButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AcceptQuoteButton.ToolTipCaption = null;
			this.AcceptQuoteButton.UseVisualStyleBackColor = true;
			this.AcceptQuoteButton.Click += new EventHandler(this.AcceptQuoteButton_Click);
			// 
			// refreshButton
			// 
			this.refreshButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e771f49a-26ae-429e-b92b-168e43239316", "Refresh");
			this.refreshButton.IsCaptionOverridden = false;
			this.refreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.refreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 27, true);
			this.refreshButton.TabIndex = 1;
			this.refreshButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.refreshButton.ToolTipCaption = null;
			this.refreshButton.UseVisualStyleBackColor = true;
			this.refreshButton.Click += new EventHandler(this.RefreshButton_Click);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "PaymentQuotes_ForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).ProviderCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).ToCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).ToAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).ExchangeRateInverted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).FromAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).FeeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).ErrorDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).ProviderReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).LastResponseReceivedLocalTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).InternalReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteForDisplay)(((System.Collections.IList)(((PaymentApprovalWithAuthorisation)(null)).PaymentQuotes_ForDisplay)).SyncRoot)).FromCurrency)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("66e43921-0a69-4cf4-b6ff-ecb2e964b3b0", "Status");
			zTextBoxColumnStyleInfo1.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("80850ff6-1dce-47dd-bd74-e46940c17323", "Provider");
			zTextBoxColumnStyleInfo2.ColumnName = "ProviderCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("252e4723-f62a-4421-b9fb-efdc04d6b6b4", "Payment Currency");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ToCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f0978d7e-d593-4138-ad94-09f249f3364f", "Payment Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "ToAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9818887a-258a-4941-a768-e8f87c4530db", "Ex Rate", "Exchange Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "ExchangeRate";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a2ea84a4-eeed-42b8-93a0-e4a5a3f29574", "Inverse Ex Rate", "Inverse Exchange Rate");
			zCalcEditColumnStyleInfo3.ColumnName = "ExchangeRateInverted";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6312a8b6-4574-4c70-b181-41f887f566f9", "Funding Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "FromAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c7180547-c378-4b5f-af49-4384002daacd", "Fee", "Processing Fee");
			zCalcEditColumnStyleInfo5.ColumnName = "FeeAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9cb08019-8da2-42bb-89eb-581f125650b2", "Error", "Error Message");
			zTextBoxColumnStyleInfo3.ColumnName = "ErrorDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d55640b2-9c3e-4e66-a8ae-4f00e62a5580", "Provider Ref", "Provider Reference");
			zTextBoxColumnStyleInfo4.ColumnName = "ProviderReference";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3cf25412-d433-431b-85f5-22b75eab2b7c", "Received", "Received Date/Time");
			zDateEditColumnStyleInfo1.ColumnName = "LastResponseReceivedLocalTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a7656810-6cde-465f-ab6b-b9e4a5cbdbab", "Quote #", "Quote Number");
			zTextBoxColumnStyleInfo5.ColumnName = "InternalReference";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BC86C7D6-F465-4FA2-8E0B-FEFF2EA30BC8", "Funding Currency");
			zTextBoxColumnStyleInfo7.ColumnName = "FromCurrency";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zGrid1.GridId = "7b04337f-028f-41b4-9366-cf3374add5a3";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 52, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 164, true);
			this.zGrid1.TabIndex = 3;
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
			this.DealGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 231, true);
			this.DealGroupBox.Name = "DealGroupBox";
			this.DealGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(893, 102, true);
			this.DealGroupBox.TabIndex = 2;
			this.DealGroupBox.TabStop = false;
			// 
			// DealProviderTextBox
			// 
			this.BindingSource.SetBindingMember(this.DealProviderTextBox, "DealProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).DealProvider)));
			this.DealProviderTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1b25e24f-9562-45a1-ab87-b786809edd21", "Provider");
			this.DealProviderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 25, true);
			this.DealProviderTextBox.Name = "DealProviderTextBox";
			this.DealProviderTextBox.ReadOnly = true;
			this.DealProviderTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DealProviderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 20, true);
			this.DealProviderTextBox.TabIndex = 1;
			// 
			// DealReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.DealReferenceTextBox, "DealProviderReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).DealProviderReference)));
			this.DealReferenceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7e44115e-f840-4f58-8f2a-abb721005ab7", "Provider Reference");
			this.DealReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 49, true);
			this.DealReferenceTextBox.Name = "DealReferenceTextBox";
			this.DealReferenceTextBox.ReadOnly = true;
			this.DealReferenceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DealReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 20, true);
			this.DealReferenceTextBox.TabIndex = 2;
			// 
			// DealStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.DealStatusTextBox, "DealStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).DealStatusDescription)));
			this.DealStatusTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d9d7923a-07fc-4501-ab6e-03dd2c0ccdfe", "Status");
			this.DealStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 73, true);
			this.DealStatusTextBox.Name = "DealStatusTextBox";
			this.DealStatusTextBox.ReadOnly = true;
			this.DealStatusTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DealStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 20, true);
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
			this.DealSubmittedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 25, true);
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
			this.DealResponseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 49, true);
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
			this.DealCostCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(652, 25, true);
			this.DealCostCalcFindBox.Name = "DealCostCalcFindBox";
			this.DealCostCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.DealCostCalcFindBox.TabIndex = 6;
			// 
			// DealErrorTextBox
			// 
			this.BindingSource.SetBindingMember(this.DealErrorTextBox, "DealErrorMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalWithAuthorisation)(null)).DealErrorMessage)));
			this.DealErrorTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2a0843bc-7630-4767-ac07-e9ee2d9e691a", "Message");
			this.DealErrorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 73, true);
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
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 602, true);
			this.WorkflowTabPage.TabIndex = 1;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 602, true);
			this.zStmNoteTabPage1.TabIndex = 2;
			// 
			// EventTabPage
			// 
			this.EventTabPage.ExcludeFromBindingOnSave = true;
			this.EventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EventTabPage.Name = "EventTabPage";
			this.EventTabPage.ShouldBeReadOnlyInViewMode = false;
			this.EventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 622, true);
			this.EventTabPage.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 676, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 2;
			// 
			// ProcessEPaymentButton
			// 
			this.ProcessEPaymentButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ProcessEPaymentButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2997bc73-4ee4-49a7-befc-90ba908c8d96", "Process E-Payment");
			this.ProcessEPaymentButton.IsCaptionOverridden = false;
			this.ProcessEPaymentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 677, true);
			this.ProcessEPaymentButton.Name = "ProcessEPaymentButton";
			this.ProcessEPaymentButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ProcessEPaymentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 24, true);
			this.ProcessEPaymentButton.TabIndex = 1;
			this.ProcessEPaymentButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ProcessEPaymentButton.ToolTipCaption = null;
			this.ProcessEPaymentButton.UseVisualStyleBackColor = true;
			this.ProcessEPaymentButton.Click += new EventHandler(this.ProcessEPaymentButton_Click);
			// 
			// PaymentApprovalWithAuthorisationForm
			// 
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentApprovalWithAuthorisationForm|88adc718-1bd7-46b9-9784-0fa7fde8d328", "Payment Approval With Authorization Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(942, 767, true);
			this.Controls.Add(this.ProcessEPaymentButton);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.zTabControl1);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(PaymentApprovalWithAuthorisation);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.PaymentApproval.PaymentApprovalWithAuthorisation";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "PaymentApprovalWithAuthorisationForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.zTabControl1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.ProcessEPaymentButton, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.PaymentDetailsGroupBox.ResumeLayout(false);
			this.PaymentDetailsGroupBox.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.PostDateEdit.ResumeLayout(true);
			this.PostDateEdit.PerformLayout();
			this.InvoiceDateEdit.ResumeLayout(true);
			this.InvoiceDateEdit.PerformLayout();
			this.OrganizationGuidFindBox.ResumeLayout(true);
			this.OrganizationGuidFindBox.PerformLayout();
			this.AddressWithContactControl.ResumeLayout(true);
			this.AddressWithContactControl.PerformLayout();
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
			this.TabControlBankAndEPayment.ResumeLayout(false);
			this.TabControlBankAndEPayment.PerformLayout();
			this.BankDetailsTab.ResumeLayout(false);
			this.BankDetailsTab.PerformLayout();
			this.AuthorizationTab.ResumeLayout(false);
			this.AuthorizationTab.PerformLayout();
			this.AuthorisationGroupBox.ResumeLayout(false);
			this.AuthorisationGroupBox.PerformLayout();
			this.ThirdAuthorisationStaffCodeFindBox.ResumeLayout(true);
			this.ThirdAuthorisationStaffCodeFindBox.PerformLayout();
			this.SecondAuthorisationStaffCodeFindBox.ResumeLayout(true);
			this.SecondAuthorisationStaffCodeFindBox.PerformLayout();
			this.FirstAuthorisationStaffCodeFindBox.ResumeLayout(true);
			this.FirstAuthorisationStaffCodeFindBox.PerformLayout();
			this.RejectGroupBox.ResumeLayout(false);
			this.RejectGroupBox.PerformLayout();
			this.RejectReasonCodeDropEdit.ResumeLayout(true);
			this.RejectReasonCodeDropEdit.PerformLayout();
			this.EPaymentTab.ResumeLayout(false);
			this.EPaymentTab.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
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
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}