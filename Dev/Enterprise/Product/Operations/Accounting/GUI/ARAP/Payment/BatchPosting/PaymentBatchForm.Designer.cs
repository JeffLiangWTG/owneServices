using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP.TransactionView;
using Enterprise.Core.Forms;
using Enterprise.Environment;
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
	public partial class PaymentBatchForm
	{


		#region Windows Form Designer generated code

		private ZCalcFindBox BalanceCalcFindBox;
		private ZGroupBox zGroupBox1;
		private ZGrid PaymentBatchGrid;
		private ZGroupBox zGroupBox2;
		private ZGrid MatchTransactionsGrid;
		private ZButton OverpaymentButton;
		private ZCalcFindBox ExchangeGainLossZCalcFindBox;
		private ZCalcFindBox DiscountZCalcFindBox;
		private ZButton ExchangeDiffButton;
		private ZCalcFindBox OverpaymentZCalcFindBox;
		private ZButton DiscountButton;
		private ZGuidFindBox CreditorFindBox;
		private ZGroupBox PaymentDetailsGroupBox;
		private ZDateEdit InvoiceDateEdit;
		private ZDateEdit PostDateEdit;
		private ZGroupBox BankDetailsGroupBox;
		private ZDropEdit PaymentTypeDropEdit;
		private ZGuidFindBox BankAccountGuidFindBox;
		private ZGuidFindBox ChequeBookGuidFindBox;
		private ZTextBox ChequeNoTextBox;
		private ZTabControl TabControlBankAndEPayment;
		private ZTabPage zTabPage1;
		private ZTabPage zTabPage2;
		private ZPanel QuoteFilterPanel;
		private ZPanel QuoteGridPanel;
		private ZPanel QuoteNotificationPanel;
		private ZLabel QuoteInfoLabel;
		private ZGrid QuoteGrid;
		private ZGroupBox zGroupBox3;
		private ZButton RefreshButton;
		private ZGrid quoteSummaryGrid;
		private ZTabControl TabControlSummaryAndDetails;
		private ZTabPage zTabPage3;
		private ZTabPage zTabPage4;
		private ZTextBox BatchNumberTextBox;
		private ZCalcEdit TotalSelectedAmountEdit;
		private ZGrid CurrencySummaryGrid;
		private ZGroupBox CurrencySummaryGroupBox;
		private ZCalcEdit CurrencySummaryTotalPaymentTextBox;
		private ZButton ProcessEPaymentsButton;
		private ZCalcEdit CurrencySummaryTotalEPaymentTextBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZDateEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZTextBoxColumnStyleInfo();
			ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new ZOrganisationFindBoxColumnStyleInfo();
			ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo2 = new ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new ZCalcEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new ZDateEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo10 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo11 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo25 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo12 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo13 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo26 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo27 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo28 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo29 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo30 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo14 = new ZDateEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo17 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo18 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo19 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo31 = new ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo20 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo32 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo15 = new ZDateEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo21 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo22 = new ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo23 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo24 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo33 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo34 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo35 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo36 = new ZTextBoxColumnStyleInfo();
			this.PaymentDetailsGroupBox = new ZGroupBox();
			this.PostDateEdit = new ZDateEdit();
			this.PostPaymentsAsPaymentApprovalsCheckBox = new ZCheckBox();
			this.ApplyEXXButton = new ZButton();
			this.CheckExRateButton = new ZButton();
			this.InvoiceDateEdit = new ZDateEdit();
			this.BankDetailsGroupBox = new ZGroupBox();
			this.FundingCurrencyCodeFindBox = new ZCodeFindBox();
			this.FundingBankAccountFindBox = new ZGuidFindBox();
			this.CardSecurityCodeTextBox = new ZTextBox();
			this.ChequeNoTextBox = new ZTextBox();
			this.ChequeBookGuidFindBox = new ZGuidFindBox();
			this.BankAccountGuidFindBox = new ZGuidFindBox();
			this.PaymentTypeDropEdit = new ZDropEdit();
			this.BatchNumberTextBox = new ZTextBox();
			this.AutoAllocateZLabel = new ZLabel();
			this.AutoPrintZLabel = new ZLabel();
			this.zGroupBox2 = new ZGroupBox();
			this.BalanceCalcFindBox = new ZCalcFindBox();
			this.TotalSelectedAmountEdit = new ZCalcEdit();
			this.CreditorFindBox = new ZGuidFindBox();
			this.OverpaymentButton = new ZButton();
			this.MatchTransactionsGrid = new ZGrid();
			this.ExchangeGainLossZCalcFindBox = new ZCalcFindBox();
			this.DiscountZCalcFindBox = new ZCalcFindBox();
			this.ExchangeDiffButton = new ZButton();
			this.OverpaymentZCalcFindBox = new ZCalcFindBox();
			this.DiscountButton = new ZButton();
			this.zGroupBox1 = new ZGroupBox();
			this.PaymentBatchGrid = new ZGrid();
			this.SaveAndCloseButton = new ZPostOrCancelButton();
			this.SaveButton = new ZPostOrCancelButton();
			this.CancelPostingButton = new ZPostOrCancelButton();
			this.SaveAsDraftButton = new ZPostOrCancelButton();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.TabControlBankAndEPayment = new ZTabControl();
			this.zTabPage1 = new ZTabPage();
			this.ServiceProviderLabel = new ZLabel();
			this.ProviderLogoPictureBox = new ZPictureBox();
			this.LearnMoreButton = new ZButton();
			this.DisclaimerMessageLabel = new ZLabel();
			this.CurrencySummaryGroupBox = new ZGroupBox();
			this.CurrencySummaryTotalEPaymentFeesTextBox = new ZCalcEdit();
			this.CurrencySummaryGrid = new ZGrid();
			this.CurrencySummaryTotalPaymentTextBox = new ZCalcEdit();
			this.CurrencySummaryTotalEPaymentTextBox = new ZCalcEdit();
			this.zTabPage2 = new ZTabPage();
			this.TabControlSummaryAndDetails = new ZTabControl();
			this.zTabPage3 = new ZTabPage();
			this.zGroupBox3 = new ZGroupBox();
			this.SyncRecipientsButton = new ZButton();
			this.AcceptQuotesButton = new ZButton();
			this.RefreshButton = new ZButton();
			this.quoteSummaryGrid = new ZGrid();
			this.zTabPage4 = new ZTabPage();
			this.QuoteFilterPanel = new ZPanel();
			this.QuoteGridPanel = new ZPanel();
			this.QuoteGrid = new ZGrid();
			this.QuoteNotificationPanel = new ZPanel();
			this.QuoteInfoLabel = new ZLabel();
			this.ProcessEPaymentsButton = new ZButton();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PaymentDetailsGroupBox.SuspendLayout();
			this.PostDateEdit.SuspendLayout();
			this.InvoiceDateEdit.SuspendLayout();
			this.BankDetailsGroupBox.SuspendLayout();
			this.FundingCurrencyCodeFindBox.SuspendLayout();
			this.FundingBankAccountFindBox.SuspendLayout();
			this.ChequeBookGuidFindBox.SuspendLayout();
			this.BankAccountGuidFindBox.SuspendLayout();
			this.PaymentTypeDropEdit.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.BalanceCalcFindBox.SuspendLayout();
			this.CreditorFindBox.SuspendLayout();
			((ISupportInitialize)(this.MatchTransactionsGrid)).BeginInit();
			this.MatchTransactionsGrid.SuspendLayout();
			this.ExchangeGainLossZCalcFindBox.SuspendLayout();
			this.DiscountZCalcFindBox.SuspendLayout();
			this.OverpaymentZCalcFindBox.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			((ISupportInitialize)(this.PaymentBatchGrid)).BeginInit();
			this.PaymentBatchGrid.SuspendLayout();
			((ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.TabControlBankAndEPayment.SuspendLayout();
			this.zTabPage1.SuspendLayout();
			((ISupportInitialize)(this.ProviderLogoPictureBox)).BeginInit();
			this.CurrencySummaryGroupBox.SuspendLayout();
			((ISupportInitialize)(this.CurrencySummaryGrid)).BeginInit();
			this.CurrencySummaryGrid.SuspendLayout();
			this.zTabPage2.SuspendLayout();
			this.TabControlSummaryAndDetails.SuspendLayout();
			this.zTabPage3.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			((ISupportInitialize)(this.quoteSummaryGrid)).BeginInit();
			this.quoteSummaryGrid.SuspendLayout();
			this.zTabPage4.SuspendLayout();
			this.QuoteFilterPanel.SuspendLayout();
			this.QuoteGridPanel.SuspendLayout();
			((ISupportInitialize)(this.QuoteGrid)).BeginInit();
			this.QuoteGrid.SuspendLayout();
			this.QuoteNotificationPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 662, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1234, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 1;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(538);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(APPaymentBatchPoster);
			// 
			// PaymentDetailsGroupBox
			// 
			this.PaymentDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|ae9f4f1a-758e-48ce-aff3-f83551e2b121", "Payment Details");
			this.PaymentDetailsGroupBox.Controls.Add(this.PostDateEdit);
			this.PaymentDetailsGroupBox.Controls.Add(this.PostPaymentsAsPaymentApprovalsCheckBox);
			this.PaymentDetailsGroupBox.Controls.Add(this.ApplyEXXButton);
			this.PaymentDetailsGroupBox.Controls.Add(this.CheckExRateButton);
			this.PaymentDetailsGroupBox.Controls.Add(this.InvoiceDateEdit);
			this.PaymentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 117, true);
			this.PaymentDetailsGroupBox.Name = "PaymentDetailsGroupBox";
			this.PaymentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 110, true);
			this.PaymentDetailsGroupBox.TabIndex = 1;
			this.PaymentDetailsGroupBox.TabStop = false;
			// 
			// PostDateEdit
			// 
			this.PostDateEdit.AllowDrop = true;
			this.PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PostDateEdit, "APB_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((APPaymentBatchPoster)(null)).APB_PostDate)));
			this.PostDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|c4576999-6b32-4360-ac7e-9291a7a1dfa8", "Post Date");
			this.PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 23, true);
			this.PostDateEdit.Name = "PostDateEdit";
			this.PostDateEdit.TabIndex = 3;
			// 
			// PostPaymentsAsPaymentApprovalsCheckBox
			// 
			this.PostPaymentsAsPaymentApprovalsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PostPaymentsAsPaymentApprovalsCheckBox, "PostPaymentsAsPaymentApprovals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((APPaymentBatchPoster)(null)).PostPaymentsAsPaymentApprovals)));
			this.PostPaymentsAsPaymentApprovalsCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|5e71e061-ee41-4aff-b630-83c9c70b9012", "Post Payments as Payment Approvals");
			this.PostPaymentsAsPaymentApprovalsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 48, true);
			this.PostPaymentsAsPaymentApprovalsCheckBox.Name = "PostPaymentsAsPaymentApprovalsCheckBox";
			this.PostPaymentsAsPaymentApprovalsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 17, true);
			this.PostPaymentsAsPaymentApprovalsCheckBox.TabIndex = 3;
			this.PostPaymentsAsPaymentApprovalsCheckBox.UseVisualStyleBackColor = true;
			// 
			// ApplyEXXButton
			// 
			this.ApplyEXXButton.AutoSize = true;
			this.ApplyEXXButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2361676d-d162-4804-8b21-56a0b474e198", "Apply EXX gain/loss to all", "Apply exchange gain/loss to all");
			this.ApplyEXXButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 70, true);
			this.ApplyEXXButton.Name = "ApplyEXXButton";
			this.ApplyEXXButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 35, true);
			this.ApplyEXXButton.TabIndex = 13;
			this.ApplyEXXButton.ToolTipCaption = null;
			this.ApplyEXXButton.Click += new EventHandler(this.ApplyEXXButton_Click);
			// 
			// CheckExRateButton
			// 
			this.CheckExRateButton.AutoSize = true;
			this.CheckExRateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("76d96e10-faff-4ad8-9e13-6a1e5799e460", "Check E-Pay rate");
			this.CheckExRateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 70, true);
			this.CheckExRateButton.Name = "CheckExRateButton";
			this.CheckExRateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 35, true);
			this.CheckExRateButton.TabIndex = 14;
			this.CheckExRateButton.ToolTipCaption = null;
			this.CheckExRateButton.Click += new EventHandler(this.CheckExRateButton_Click);
			// 
			// InvoiceDateEdit
			// 
			this.InvoiceDateEdit.AllowDrop = true;
			this.InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.InvoiceDateEdit, "APB_PaymentDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((APPaymentBatchPoster)(null)).APB_PaymentDate)));
			this.InvoiceDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|d1dd247e-1f52-40ae-9951-b56a118ee60c", "Date");
			this.InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 23, true);
			this.InvoiceDateEdit.Name = "InvoiceDateEdit";
			this.InvoiceDateEdit.TabIndex = 1;
			// 
			// BankDetailsGroupBox
			// 
			this.BankDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|c0aa1da6-6359-453b-b5ea-229117cbe1c7", "Bank Details");
			this.BankDetailsGroupBox.Controls.Add(this.FundingCurrencyCodeFindBox);
			this.BankDetailsGroupBox.Controls.Add(this.FundingBankAccountFindBox);
			this.BankDetailsGroupBox.Controls.Add(this.CardSecurityCodeTextBox);
			this.BankDetailsGroupBox.Controls.Add(this.ChequeNoTextBox);
			this.BankDetailsGroupBox.Controls.Add(this.ChequeBookGuidFindBox);
			this.BankDetailsGroupBox.Controls.Add(this.BankAccountGuidFindBox);
			this.BankDetailsGroupBox.Controls.Add(this.PaymentTypeDropEdit);
			this.BankDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 3, true);
			this.BankDetailsGroupBox.Name = "BankDetailsGroupBox";
			this.BankDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 113, true);
			this.BankDetailsGroupBox.TabIndex = 0;
			this.BankDetailsGroupBox.TabStop = false;
			// 
			// FundingCurrencyCodeFindBox
			// 
			this.FundingCurrencyCodeFindBox.AllowDrop = false;
			this.BindingSource.SetBindingMember(this.FundingCurrencyCodeFindBox, "FundingBankAccountCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APPaymentBatchPoster)(null)).FundingBankAccountCurrency)));
			this.FundingCurrencyCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|B56E209D-C62D-448F-A55A-DF73E7E92C46", "Funding Currency");
			this.FundingCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 63, true);
			this.FundingCurrencyCodeFindBox.Name = "FundingCurrencyCodeFindBox";
			this.FundingCurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FundingCurrencyCodeFindBox.ParentType = null;
			this.FundingCurrencyCodeFindBox.PreBoundMaxLength = 3;
			this.FundingCurrencyCodeFindBox.ShowDescriptionBox = false;
			this.FundingCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.FundingCurrencyCodeFindBox.TabIndex = 6;
			this.FundingCurrencyCodeFindBox.Visible = false;
			this.FundingCurrencyCodeFindBox.ReadOnly = true;
			// 
			// FundingBankAccountFindBox
			// 
			this.FundingBankAccountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FundingBankAccountFindBox, "APB_AB_FundingBankAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((APPaymentBatchPoster)(null)).APB_AB_FundingBankAccount)));
			this.FundingBankAccountFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|BFB22AEC-7131-4203-B734-F60CC79929A3", "Funding Bank Account");
			this.FundingBankAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 64, true);
			this.FundingBankAccountFindBox.Name = "FundingBankAccountFindBox";
			this.FundingBankAccountFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FundingBankAccountFindBox.ParentType = null;
			this.FundingBankAccountFindBox.ShowDescriptionBox = false;
			this.FundingBankAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.FundingBankAccountFindBox.TabIndex = 5;
			this.FundingBankAccountFindBox.Visible = false;
			// 
			// CardSecurityCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CardSecurityCodeTextBox, "CardSecurityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APPaymentBatchPoster)(null)).CardSecurityCode)));
			this.CardSecurityCodeTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|21c91b88-c695-48e3-a615-52a90decf8a3", "Card Security Code");
			this.CardSecurityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 88, true);
			this.CardSecurityCodeTextBox.Name = "CardSecurityCodeTextBox";
			this.CardSecurityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.CardSecurityCodeTextBox.TabIndex = 12;
			// 
			// ChequeNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChequeNoTextBox, "APB_ChequeOrReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APPaymentBatchPoster)(null)).APB_ChequeOrReference)));
			this.ChequeNoTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|4b4be2b5-b898-4ca0-b96f-47dcb45ea14b", "Start Reference No.");
			this.ChequeNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 88, true);
			this.ChequeNoTextBox.Name = "ChequeNoTextBox";
			this.ChequeNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.ChequeNoTextBox.TabIndex = 7;
			// 
			// ChequeBookGuidFindBox
			// 
			this.ChequeBookGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChequeBookGuidFindBox, "APB_AK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((APPaymentBatchPoster)(null)).APB_AK)));
			this.ChequeBookGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|ddfcbcd4-6f84-4f1e-9ecb-b681e0fb6b82", "Check Book");
			this.ChequeBookGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 64, true);
			this.ChequeBookGuidFindBox.Name = "ChequeBookGuidFindBox";
			this.ChequeBookGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ChequeBookGuidFindBox.ParentType = null;
			this.ChequeBookGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 20, true);
			this.ChequeBookGuidFindBox.TabIndex = 4;
			// 
			// BankAccountGuidFindBox
			// 
			this.BankAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BankAccountGuidFindBox, "APB_AB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((APPaymentBatchPoster)(null)).APB_AB)));
			this.BankAccountGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|f123337e-0b23-482f-ab90-feddccd79cdb", "Bank Account");
			this.BankAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 40, true);
			this.BankAccountGuidFindBox.Name = "BankAccountGuidFindBox";
			this.BankAccountGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BankAccountGuidFindBox.ParentType = null;
			this.BankAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 20, true);
			this.BankAccountGuidFindBox.TabIndex = 3;
			// 
			// PaymentTypeDropEdit
			// 
			this.PaymentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTypeDropEdit, "APB_PaymentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((APPaymentBatchPoster)(null)).APB_PaymentType)));
			this.PaymentTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|4e0395e9-8235-4d34-a604-f499065f4ee2", "Payment Type");
			this.PaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 16, true);
			this.PaymentTypeDropEdit.Name = "PaymentTypeDropEdit";
			this.PaymentTypeDropEdit.PreBoundMaxLength = 5;
			this.PaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 20, true);
			this.PaymentTypeDropEdit.TabIndex = 1;
			// 
			// BatchNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BatchNumberTextBox, "APB_BatchNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APPaymentBatchPoster)(null)).APB_BatchNumber)));
			this.BatchNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(785, 207, true);
			this.BatchNumberTextBox.Name = "BatchNumberTextBox";
			this.BatchNumberTextBox.ReadOnly = true;
			this.BatchNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.BatchNumberTextBox.TabIndex = 8;
			// 
			// AutoAllocateZLabel
			// 
			this.AutoAllocateZLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoAllocateZLabel, "Calc_ChequeNumberIsAutoAllocatedLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APPaymentBatchPoster)(null)).Calc_ChequeNumberIsAutoAllocatedLabel)));
			this.AutoAllocateZLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|40611f62-36ef-498f-95ae-5f9437960fdf", "Auto Allocate");
			this.AutoAllocateZLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AutoAllocateZLabel.ForeColor = System.Drawing.Color.Red;
			this.AutoAllocateZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 196, true);
			this.AutoAllocateZLabel.Name = "AutoAllocateZLabel";
			this.AutoAllocateZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 13, true);
			this.AutoAllocateZLabel.TabIndex = 11;
			this.AutoAllocateZLabel.UseMnemonic = false;
			// 
			// AutoPrintZLabel
			// 
			this.AutoPrintZLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoPrintZLabel, "Calc_ChequeIsAutoPrintedLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APPaymentBatchPoster)(null)).Calc_ChequeIsAutoPrintedLabel)));
			this.AutoPrintZLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|6748c9d4-b8ff-4b25-9df0-c51efd74601b", "Auto Print");
			this.AutoPrintZLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AutoPrintZLabel.ForeColor = System.Drawing.Color.Red;
			this.AutoPrintZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 180, true);
			this.AutoPrintZLabel.Name = "AutoPrintZLabel";
			this.AutoPrintZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 13, true);
			this.AutoPrintZLabel.TabIndex = 10;
			this.AutoPrintZLabel.UseMnemonic = false;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|fbf3a341-bb05-4392-b9f9-30be20982644", "Selected Payment Details");
			this.zGroupBox2.Controls.Add(this.BalanceCalcFindBox);
			this.zGroupBox2.Controls.Add(this.TotalSelectedAmountEdit);
			this.zGroupBox2.Controls.Add(this.CreditorFindBox);
			this.zGroupBox2.Controls.Add(this.OverpaymentButton);
			this.zGroupBox2.Controls.Add(this.MatchTransactionsGrid);
			this.zGroupBox2.Controls.Add(this.ExchangeGainLossZCalcFindBox);
			this.zGroupBox2.Controls.Add(this.DiscountZCalcFindBox);
			this.zGroupBox2.Controls.Add(this.ExchangeDiffButton);
			this.zGroupBox2.Controls.Add(this.OverpaymentZCalcFindBox);
			this.zGroupBox2.Controls.Add(this.DiscountButton);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 2, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1208, 216, true);
			this.zGroupBox2.TabIndex = 0;
			this.zGroupBox2.TabStop = false;
			// 
			// BalanceCalcFindBox
			// 
			this.BalanceCalcFindBox.AllowDrop = true;
			this.BalanceCalcFindBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BalanceCalcFindBox.BindToAmount = "Balance";
			this.BalanceCalcFindBox.BindToUnit = "LocalCurrency";
			this.BalanceCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|00bc2226-23c0-4c1c-816a-50cccf22ef82", "Balance");
			this.BalanceCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.BalanceCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 162, true);
			this.BalanceCalcFindBox.Name = "BalanceCalcFindBox";
			this.BalanceCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.BalanceCalcFindBox.TabIndex = 4;
			// 
			// TotalSelectedAmountEdit
			// 
			this.TotalSelectedAmountEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.TotalSelectedAmountEdit, "PaymentItemsTotalAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((APPaymentBatchPoster)(null)).PaymentItemsTotalAmount)));
			this.TotalSelectedAmountEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|ee5606e9-d843-4742-a11e-929dac69d443", "Payment Items Total");
			this.TotalSelectedAmountEdit.DecimalPlaces = 2;
			this.TotalSelectedAmountEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 165, true);
			this.TotalSelectedAmountEdit.Name = "TotalSelectedAmountEdit";
			this.TotalSelectedAmountEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.TotalSelectedAmountEdit.TabIndex = 2;
			this.TotalSelectedAmountEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalSelectedAmountEdit.TrackDisposedAccess = true;
			// 
			// CreditorFindBox
			// 
			this.CreditorFindBox.AllowDrop = true;
			this.CreditorFindBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.CreditorFindBox, "Creditor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((APPaymentBatchPoster)(null)).Creditor)));
			this.CreditorFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|7bc45a45-685f-4354-8470-6af7749f922b", "Creditor");
			this.CreditorFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 162, true);
			this.CreditorFindBox.Name = "CreditorFindBox";
			this.CreditorFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CreditorFindBox.ParentType = null;
			this.CreditorFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 20, true);
			this.CreditorFindBox.TabIndex = 6;
			// 
			// OverpaymentButton
			// 
			this.OverpaymentButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.OverpaymentButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|f8567533-ec2d-4cc6-8bb9-6c1a02630d61", "New");
			this.OverpaymentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 186, true);
			this.OverpaymentButton.Name = "OverpaymentButton";
			this.OverpaymentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 23, true);
			this.OverpaymentButton.TabIndex = 9;
			this.OverpaymentButton.ToolTipCaption = null;
			this.OverpaymentButton.Click += new EventHandler(this.OverpaymentButton_Click);
			// 
			// MatchTransactionsGrid
			// 
			this.MatchTransactionsGrid.AllowNavigation = false;
			this.MatchTransactionsGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MatchTransactionsGrid, "MatchingCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).Organisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).TransactionNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).MatchDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).CurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).OSOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).OutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).ConsolidatedRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).TransactionReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).BranchGuid)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).InvoiceBatchNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).RelatedTransactionDebtorsAsString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).CreatingUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).PaymentCriticality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).PaymentRequestedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).RelatedClaimStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).MatchingCollection)).SyncRoot)).QueryNumber)));
			this.MatchTransactionsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|f1a83b4e-d632-4be0-9084-4916fad6e6aa", "Organization");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Organisation";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|538c7cc9-46ba-423e-b043-019af3934640", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "TransactionType";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|d2f27f7c-52fc-4620-ae0a-e262f47367c0", "Transaction Num.", "Transaction Number.");
			zTextBoxColumnStyleInfo2.ColumnName = "TransactionNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|25171219-37ee-4334-9a20-84aa81d8c734", "Check/Reference");
			zTextBoxColumnStyleInfo3.ColumnName = "ChequeOrReference";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|ecd0fac1-7e3c-4c14-81c0-2ceb61f2fbca", "Trans. Date");
			zDateEditColumnStyleInfo1.ColumnName = "InvoiceDate";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|edb16a94-cc45-4613-b84a-9a14c2119231", "Due Date");
			zDateEditColumnStyleInfo2.ColumnName = "DueDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|39f6ae60-61a3-48f5-81a8-cdc5d2e2ae75", "Match Date");
			zDateEditColumnStyleInfo3.ColumnName = "MatchDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|fffd8435-da25-4b34-af89-a72f46c021e0", "Currency");
			zTextBoxColumnStyleInfo4.ColumnName = "CurrencyCode";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|77dda87f-d644-4501-be3d-7938e31736ed", "OS Outstanding Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "OSOutstandingAmount";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|386298a1-61ff-478c-be42-ccf6b172f498", "Outstanding Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "OutstandingAmount";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|a3dbdb17-7135-4989-bd44-17be7f9646d9", "Job Inv. Num.", "Job Invoice Number.");
			zTextBoxColumnStyleInfo5.ColumnName = "ConsolidatedRef";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|3184e502-dc44-40ca-949e-f2ed2e55886b", "Govt Tax Inv.", "Govt Tax Invoice.");
			zTextBoxColumnStyleInfo6.ColumnName = "TransactionReference";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|61a37993-9cef-4022-b8fa-de2ed68db299", "Post Date");
			zDateEditColumnStyleInfo4.ColumnName = "PostDate";
			zDateEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|e3651a4d-9731-42d1-a7f9-3f90444adf1d", "Branch");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "BranchGuid";
			zGuidFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|72f685f3-3f72-42d5-8d4a-b1e48e166857", "Description");
			zTextBoxColumnStyleInfo7.ColumnName = "Description";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|5d2b4adc-2940-496c-acd9-e780f215ac37", "Inv. Batch Number", "Invoice Batch Number.");
			zTextBoxColumnStyleInfo8.ColumnName = "InvoiceBatchNumber";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|f4b2a0bb-4baf-4846-b7fe-cbc52de47ad4", "Related Transaction Debtors");
			zTextBoxColumnStyleInfo9.ColumnName = "RelatedTransactionDebtorsAsString";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|09d2ef3b-d28c-4377-aea9-e85fc23e39fb", "Creating User");
			zTextBoxColumnStyleInfo10.ColumnName = "CreatingUser";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|56a003a5-5bdc-49e3-a9c0-1b6662b888cb", "Payment Criticality");
			zTextBoxColumnStyleInfo11.ColumnName = "PaymentCriticality";
			zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|267b696c-d92b-4df6-a6fa-799634cb1abe", "Payment Date");
			zDateEditColumnStyleInfo5.ColumnName = "PaymentRequestedDate";
			zDateEditColumnStyleInfo5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3df529ba-8ee0-4704-8802-c7da9866c77c", "Related Claim Status");
			zTextBoxColumnStyleInfo12.ColumnName = "RelatedClaimStatus";
			zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1c95374d-8c58-4231-a6d3-a42dba96470f", "Query Number");
			zTextBoxColumnStyleInfo13.ColumnName = "QueryNumber";
			zTextBoxColumnStyleInfo13.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.MatchTransactionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MatchTransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MatchTransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MatchTransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MatchTransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.MatchTransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MatchTransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.MatchTransactionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.MatchTransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.MatchTransactionsGrid.GridId = "28a98588-e7c6-4013-84ca-772c28d1c20b";
			this.MatchTransactionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MatchTransactionsGrid.IsWholeRowSelectedOnClick = true;
			this.MatchTransactionsGrid.LayoutKey = "zDisplayGrid1";
			this.MatchTransactionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 17, true);
			this.MatchTransactionsGrid.Name = "MatchTransactionsGrid";
			this.MatchTransactionsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.MatchTransactionsGrid.ShouldSetErrorsOnTabPage = false;
			this.MatchTransactionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1191, 141, true);
			this.MatchTransactionsGrid.TabIndex = 0;
			// 
			// ExchangeGainLossZCalcFindBox
			// 
			this.ExchangeGainLossZCalcFindBox.AllowDrop = true;
			this.ExchangeGainLossZCalcFindBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ExchangeGainLossZCalcFindBox.BindToAmount = "ExchangeDifferenceAmount";
			this.ExchangeGainLossZCalcFindBox.BindToUnit = "LocalCurrency";
			this.ExchangeGainLossZCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|0df2f82d-4b2e-4554-9217-c8042c4f5de3", "Exchange Gain/Loss");
			this.ExchangeGainLossZCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.ExchangeGainLossZCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(651, 189, true);
			this.ExchangeGainLossZCalcFindBox.Name = "ExchangeGainLossZCalcFindBox";
			this.ExchangeGainLossZCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.ExchangeGainLossZCalcFindBox.TabIndex = 14;
			// 
			// DiscountZCalcFindBox
			// 
			this.DiscountZCalcFindBox.AllowDrop = true;
			this.DiscountZCalcFindBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DiscountZCalcFindBox.BindToAmount = "DiscountAmount";
			this.DiscountZCalcFindBox.BindToUnit = "LocalCurrency";
			this.DiscountZCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|7950d2a7-ad29-4d31-9d25-95ee52de3fb8", "Discount");
			this.DiscountZCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.DiscountZCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 189, true);
			this.DiscountZCalcFindBox.Name = "DiscountZCalcFindBox";
			this.DiscountZCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.DiscountZCalcFindBox.TabIndex = 11;
			// 
			// ExchangeDiffButton
			// 
			this.ExchangeDiffButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ExchangeDiffButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|0265fcda-7ea4-48f6-b168-f00a90001bc0", "New");
			this.ExchangeDiffButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(777, 186, true);
			this.ExchangeDiffButton.Name = "ExchangeDiffButton";
			this.ExchangeDiffButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 23, true);
			this.ExchangeDiffButton.TabIndex = 15;
			this.ExchangeDiffButton.ToolTipCaption = null;
			this.ExchangeDiffButton.Click += new EventHandler(this.ExchangeDiffButton_Click);
			// 
			// OverpaymentZCalcFindBox
			// 
			this.OverpaymentZCalcFindBox.AllowDrop = true;
			this.OverpaymentZCalcFindBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.OverpaymentZCalcFindBox.BindToAmount = "OSOverpaymentAmount";
			this.OverpaymentZCalcFindBox.BindToUnit = "ForeignCurrency";
			this.OverpaymentZCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|629e924e-da4e-4217-b10d-ec05cbdae416", "Overpayment");
			this.OverpaymentZCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OverpaymentZCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 186, true);
			this.OverpaymentZCalcFindBox.Name = "OverpaymentZCalcFindBox";
			this.OverpaymentZCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.OverpaymentZCalcFindBox.TabIndex = 8;
			// 
			// DiscountButton
			// 
			this.DiscountButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DiscountButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|1da1ef60-a6e3-4279-9c98-4c76012dd7bc", "New");
			this.DiscountButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 186, true);
			this.DiscountButton.Name = "DiscountButton";
			this.DiscountButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 23, true);
			this.DiscountButton.TabIndex = 12;
			this.DiscountButton.ToolTipCaption = null;
			this.DiscountButton.Click += new EventHandler(this.DiscountButton_Click);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|833876a6-0499-43fc-aa63-d032f7dca995", "Payment Batch");
			this.zGroupBox1.Controls.Add(this.PaymentBatchGrid);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 2, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1220, 151, true);
			this.zGroupBox1.TabIndex = 2;
			this.zGroupBox1.TabStop = false;
			// 
			// PaymentBatchGrid
			// 
			this.PaymentBatchGrid.AllowNavigation = false;
			this.PaymentBatchGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PaymentBatchGrid, "PaymentApprovalCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AV_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AV_OA_AddressOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AV_OC_ContactOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AV_PaymentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AV_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AV_AB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AV_AK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AV_ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AV_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AV_Calc_LocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).PaymentItemsTotalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).OSOverpaymentAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).DiscountAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).ExchangeDifferenceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).MatchedTransactionsBalanceWithPaymentAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AV_PayExRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AV_PaymentDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AV_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AV_RX_NKPaymentCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).DealStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).DealSubmittedLocalTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).DealLastResponseLocalTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).DealErrorMessage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).DealProviderReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AV_EPaymentReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).BankCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).BankCreateTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).BankLastEditTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).BankLastEditUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).PayeeBankName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).AccountTitle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).PayeeBankAccountCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).PayeeBankBSB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).PayeeBankAccountNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).EPaymentRecipientListLastUpdatedTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((PaymentApprovalBase)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).PaymentApprovalCollection)).SyncRoot)).EPaymentBeneficiaryLastEditTimeLocal)));
			this.PaymentBatchGrid.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("356ae970-f39e-4a5b-9afe-31b3ad8681d0", "Settlement Group");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AV_OH";
			zOrganisationFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d7224555-2bbe-4724-9cc3-abfcdd587094", "Address");
			zAddressDropEditColumnStyleInfo1.ColumnName = "AV_OA_AddressOverride";
			zAddressDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8a8c8cf6-fa4e-4aae-8052-e163d1595015", "Contact");
			zAddressDropEditColumnStyleInfo2.ColumnName = "AV_OC_ContactOverride";
			zAddressDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zAddressDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("aa9932ae-bcd4-4009-b698-bf541dd520f3", "Payment Type");
			zDropEditColumnStyleInfo1.ColumnName = "AV_PaymentType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("364eaa67-2550-44dc-ba79-eaa017c66393", "Status");
			zTextBoxColumnStyleInfo14.ColumnName = "AV_Status";
			zTextBoxColumnStyleInfo14.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "AV_AB";
			zGuidFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "AV_AK";
			zGuidFindBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9c392431-366d-4dfd-8783-044adb3ea9f7", "Check Or Reference");
			zTextBoxColumnStyleInfo15.ColumnName = "AV_ChequeOrReference";
			zTextBoxColumnStyleInfo15.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("27ddc743-b252-44f5-8cd9-884197fcd00c", "Payment Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "AV_Amount";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ff86b508-08c8-4e16-8c8a-d1daf1a58c41", "Local Payment Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "AV_Calc_LocalAmount";
			zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("225b24fe-f76f-45c9-84ed-79cef5bf2479", "Payment Items Total");
			zCalcEditColumnStyleInfo5.ColumnName = "PaymentItemsTotalAmount";
			zCalcEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4c2305e1-f745-4734-bcd6-f01fa4dc0700", "Overpayment Amount");
			zCalcEditColumnStyleInfo6.ColumnName = "OSOverpaymentAmount";
			zCalcEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4c4a0376-cdbb-4ac4-aa16-efcb233ecf91", "Discount Amount");
			zCalcEditColumnStyleInfo7.ColumnName = "DiscountAmount";
			zCalcEditColumnStyleInfo7.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("24101488-8147-4464-9fea-c413649e9253", "Exch. Diff. Amount", "Exchange Difference Amount");
			zCalcEditColumnStyleInfo8.ColumnName = "ExchangeDifferenceAmount";
			zCalcEditColumnStyleInfo8.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e86dfc42-bdd2-4c3a-839e-88be91e34b43", "Balance");
			zCalcEditColumnStyleInfo9.ColumnName = "MatchedTransactionsBalanceWithPaymentAmount";
			zCalcEditColumnStyleInfo9.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.ColumnName = "AV_PayExRate";
			zCalcEditColumnStyleInfo10.Decimals = 4;
			zCalcEditColumnStyleInfo10.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo6.ColumnName = "AV_PaymentDate";
			zDateEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo7.ColumnName = "AV_PostDate";
			zDateEditColumnStyleInfo7.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AV_RX_NKPaymentCurrency";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5b2ee25f-843d-49e6-9ce1-de0fb2a2f32b", "E-Payment Status");
			zTextBoxColumnStyleInfo16.ColumnName = "DealStatusDescription";
			zTextBoxColumnStyleInfo16.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8dabe22e-b865-40e7-b263-c96ab200dd4f", "E-Payment Submitted Date");
			zDateEditColumnStyleInfo8.ColumnName = "DealSubmittedLocalTime";
			zDateEditColumnStyleInfo8.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("63406a33-81c4-4147-af65-8aadfdc16e3b", "E-Payment Last Response Received Date");
			zDateEditColumnStyleInfo9.ColumnName = "DealLastResponseLocalTime";
			zDateEditColumnStyleInfo9.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ea9037a9-f5a2-4573-b342-242f5c931f8b", "E-Payment Message");
			zTextBoxColumnStyleInfo17.ColumnName = "DealErrorMessage";
			zTextBoxColumnStyleInfo17.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ae7f7277-8921-4f22-9beb-6608694b2f00", "E-Payment Provider Reference");
			zTextBoxColumnStyleInfo18.ColumnName = "DealProviderReference";
			zTextBoxColumnStyleInfo18.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3247626f-64ec-4d23-90e4-46750cd7d296", "Payment Reason");
			zDropEditColumnStyleInfo2.ColumnName = "AV_EPaymentReasonCode";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0c8ba2a4-339a-4029-9473-231b320c28bb", "Bank Created By");
			zTextBoxColumnStyleInfo19.ColumnName = "BankCreateUser";
			zTextBoxColumnStyleInfo19.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo19.IsVisible = false;
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("349bf531-a3f7-4be2-adb5-313883886dd7", "Bank Created Time");
			zDateEditColumnStyleInfo10.ColumnName = "BankCreateTimeLocal";
			zDateEditColumnStyleInfo10.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo10.IsReadOnly = true;
			zDateEditColumnStyleInfo10.IsVisible = false;
			zDateEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1917ce38-6802-4869-82da-760ede319fe2", "Bank Last Edit");
			zDateEditColumnStyleInfo11.ColumnName = "BankLastEditTimeLocal";
			zDateEditColumnStyleInfo11.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo11.IsVisible = false;
			zDateEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3ecfc29d-2b68-4f9c-99ec-64b6a35883e7", "Bank Last Edited By");
			zTextBoxColumnStyleInfo20.ColumnName = "BankLastEditUser";
			zTextBoxColumnStyleInfo20.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo20.IsVisible = false;
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e630eb3f-4347-4784-8275-186366e4f776", "Payee Bank");
			zTextBoxColumnStyleInfo21.ColumnName = "PayeeBankName";
			zTextBoxColumnStyleInfo21.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo21.GroupName = Enterprise.Accounting.GUI.Res.GetData("3c8549db-3705-4b7f-9a90-4c944e02fa58", "Payee Account Details");
			zTextBoxColumnStyleInfo21.IsVisible = false;
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2845fad8-ddeb-4b4f-8b31-4a38203f5378", "Payee Account Name");
			zTextBoxColumnStyleInfo22.ColumnName = "AccountTitle";
			zTextBoxColumnStyleInfo22.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo22.GroupName = Enterprise.Accounting.GUI.Res.GetData("3c8549db-3705-4b7f-9a90-4c944e02fa58", "Payee Account Details");
			zTextBoxColumnStyleInfo22.IsVisible = false;
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo23.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("199b8967-cb24-44ca-aed6-3a9b044a0f1f", "Country");
			zTextBoxColumnStyleInfo23.ColumnName = "PayeeBankAccountCountry";
			zTextBoxColumnStyleInfo23.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo23.GroupName = Enterprise.Accounting.GUI.Res.GetData("3c8549db-3705-4b7f-9a90-4c944e02fa58", "Payee Account Details");
			zTextBoxColumnStyleInfo23.IsVisible = false;
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo24.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e53a59cc-79f6-4f0a-9927-4598386a19fe", "Bank/Branch");
			zTextBoxColumnStyleInfo24.ColumnName = "PayeeBankBSB";
			zTextBoxColumnStyleInfo24.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo24.GroupName = Enterprise.Accounting.GUI.Res.GetData("3c8549db-3705-4b7f-9a90-4c944e02fa58", "Payee Account Details");
			zTextBoxColumnStyleInfo24.IsVisible = false;
			zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo25.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("340a3d8b-5b59-41e5-9a8b-80f3c00ecf19", "Payee Account Number");
			zTextBoxColumnStyleInfo25.ColumnName = "PayeeBankAccountNumber";
			zTextBoxColumnStyleInfo25.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo25.GroupName = Enterprise.Accounting.GUI.Res.GetData("3c8549db-3705-4b7f-9a90-4c944e02fa58", "Payee Account Details");
			zTextBoxColumnStyleInfo25.IsVisible = false;
			zTextBoxColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6b99dff0-ca7c-42bf-86a0-bf79baa03f5a", "E-Payment Last Synced Date/Time");
			zDateEditColumnStyleInfo12.ColumnName = "EPaymentRecipientListLastUpdatedTimeLocal";
			zDateEditColumnStyleInfo12.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo12.GroupName = Enterprise.Accounting.GUI.Res.GetData("9bac75bb-6e6d-41d1-8572-40b89cfb343f", "E-Payment Sync Details");
			zDateEditColumnStyleInfo12.IsVisible = false;
			zDateEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d4883b76-e374-45a2-b2a9-9313123e8505", "E-Payment Details Last Updated");
			zDateEditColumnStyleInfo13.ColumnName = "EPaymentBeneficiaryLastEditTimeLocal";
			zDateEditColumnStyleInfo13.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo13.GroupName = Enterprise.Accounting.GUI.Res.GetData("9bac75bb-6e6d-41d1-8572-40b89cfb343f", "E-Payment Sync Details");
			zDateEditColumnStyleInfo13.IsVisible = false;
			zDateEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PaymentBatchGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.PaymentBatchGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.PaymentBatchGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo2);
			this.PaymentBatchGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PaymentBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.PaymentBatchGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.PaymentBatchGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.PaymentBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.PaymentBatchGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PaymentBatchGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.PaymentBatchGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.PaymentBatchGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.PaymentBatchGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.PaymentBatchGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.PaymentBatchGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.PaymentBatchGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.PaymentBatchGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.PaymentBatchGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.PaymentBatchGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.PaymentBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.PaymentBatchGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.PaymentBatchGrid.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.PaymentBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.PaymentBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.PaymentBatchGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PaymentBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.PaymentBatchGrid.ColumnStyles.Add(zDateEditColumnStyleInfo10);
			this.PaymentBatchGrid.ColumnStyles.Add(zDateEditColumnStyleInfo11);
			this.PaymentBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.PaymentBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.PaymentBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.PaymentBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.PaymentBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.PaymentBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo25);
			this.PaymentBatchGrid.ColumnStyles.Add(zDateEditColumnStyleInfo12);
			this.PaymentBatchGrid.ColumnStyles.Add(zDateEditColumnStyleInfo13);
			this.PaymentBatchGrid.GridId = "cb07584c-6404-4fec-bbb8-c739c1d16ef5";
			this.PaymentBatchGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PaymentBatchGrid.LayoutKey = "zDisplayGrid1";
			this.PaymentBatchGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 17, true);
			this.PaymentBatchGrid.Name = "PaymentBatchGrid";
			this.PaymentBatchGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.PaymentBatchGrid.ShouldSetErrorsOnTabPage = false;
			this.PaymentBatchGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1191, 129, true);
			this.PaymentBatchGrid.TabIndex = 0;
			// 
			// SaveAndCloseButton
			// 
			this.SaveAndCloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveAndCloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|d918a1fb-abde-4bda-8a60-3af6b67b7b73", "Save And Close");
			this.SaveAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(980, 225, true);
			this.SaveAndCloseButton.Name = "SaveAndCloseButton";
			this.SaveAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 23, true);
			this.SaveAndCloseButton.TabIndex = 3;
			this.SaveAndCloseButton.ToolTipCaption = null;
			this.SaveAndCloseButton.UseVisualStyleBackColor = true;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|F823C80E-FD14-47AE-BFAD-FB7ED98250CF", "Save");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(853, 225, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.SaveButton.TabIndex = 2;
			this.SaveButton.ToolTipCaption = null;
			this.SaveButton.UseVisualStyleBackColor = true;
			// 
			// CancelPostingButton
			// 
			this.CancelPostingButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPostingButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|35c1be61-a7b7-4e5a-9450-f4a259935e90", "Cancel");
			this.CancelPostingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1136, 225, true);
			this.CancelPostingButton.Name = "CancelPostingButton";
			this.CancelPostingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 23, true);
			this.CancelPostingButton.TabIndex = 4;
			this.CancelPostingButton.ToolTipCaption = null;
			this.CancelPostingButton.UseVisualStyleBackColor = true;
			// 
			// SaveAsDraftButton
			// 
			this.SaveAsDraftButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveAsDraftButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|607ada4b-126f-48c7-907b-6d71818ac2f8", "Save as Draft");
			this.SaveAsDraftButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(758, 225, true);
			this.SaveAsDraftButton.Name = "SaveAsDraftButton";
			this.SaveAsDraftButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.SaveAsDraftButton.TabIndex = 1;
			this.SaveAsDraftButton.ToolTipCaption = null;
			this.SaveAsDraftButton.UseVisualStyleBackColor = true;
			this.SaveAsDraftButton.Click += new EventHandler(this.SaveAsDraftButton_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 454, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.CancelPostingButton);
			this.splitContainer1.Panel2.Controls.Add(this.zGroupBox2);
			this.splitContainer1.Panel2.Controls.Add(this.SaveAndCloseButton);
			this.splitContainer1.Panel2.Controls.Add(this.SaveButton);
			this.splitContainer1.Panel2.Controls.Add(this.SaveAsDraftButton);
			this.splitContainer1.Panel2.Controls.Add(this.ProcessEPaymentsButton);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1234, 686, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(425);
			this.splitContainer1.TabIndex = 0;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer2.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 300, true);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.TabControlBankAndEPayment);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.zGroupBox1);
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1234, 425, true);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(265);
			this.splitContainer2.TabIndex = 0;
			// 
			// TabControlBankAndEPayment
			// 
			this.TabControlBankAndEPayment.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControlBankAndEPayment.Controls.Add(this.zTabPage1);
			this.TabControlBankAndEPayment.Controls.Add(this.zTabPage2);
			this.TabControlBankAndEPayment.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControlBankAndEPayment.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControlBankAndEPayment.Name = "TabControlBankAndEPayment";
			this.TabControlBankAndEPayment.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1234, 265, true);
			this.TabControlBankAndEPayment.TabIndex = 12;
			// 
			// zTabPage1
			// 
			this.zTabPage1.BackColor = System.Drawing.SystemColors.Control;
			this.zTabPage1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("33d2e116-d7f2-4486-8d8c-d3da1ce6921b", "Bank Details");
			this.zTabPage1.Controls.Add(this.ServiceProviderLabel);
			this.zTabPage1.Controls.Add(this.ProviderLogoPictureBox);
			this.zTabPage1.Controls.Add(this.LearnMoreButton);
			this.zTabPage1.Controls.Add(this.DisclaimerMessageLabel);
			this.zTabPage1.Controls.Add(this.BankDetailsGroupBox);
			this.zTabPage1.Controls.Add(this.CurrencySummaryGroupBox);
			this.zTabPage1.Controls.Add(this.BatchNumberTextBox);
			this.zTabPage1.Controls.Add(this.PaymentDetailsGroupBox);
			this.zTabPage1.Controls.Add(this.AutoAllocateZLabel);
			this.zTabPage1.Controls.Add(this.AutoPrintZLabel);
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 238, true);
			this.zTabPage1.TabIndex = 0;
			// 
			// ServiceProviderLabel
			// 
			this.ServiceProviderLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("34dd51a3-b444-449d-9a5f-b104f8ab12a7", "Service Provider");
			this.ServiceProviderLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ServiceProviderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(900, 93, true);
			this.ServiceProviderLabel.Name = "ServiceProviderLabel";
			this.ServiceProviderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.ServiceProviderLabel.TabIndex = 16;
			this.ServiceProviderLabel.UseMnemonic = false;
			// 
			// ProviderLogoPictureBox
			// 
			this.ProviderLogoPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1006, 92, true);
			this.ProviderLogoPictureBox.Name = "ProviderLogoPictureBox";
			this.ProviderLogoPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 36, true);
			this.ProviderLogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.ProviderLogoPictureBox.TabIndex = 15;
			this.ProviderLogoPictureBox.TabStop = false;
			this.ProviderLogoPictureBox.Click += new EventHandler(this.ProviderLogoPictureBox_Click);
			// 
			// LearnMoreButton
			// 
			this.LearnMoreButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("543cf118-9748-47e3-b312-ea329b406fd7", "Learn More");
			this.LearnMoreButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1105, 108, true);
			this.LearnMoreButton.Name = "LearnMoreButton";
			this.LearnMoreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 23, true);
			this.LearnMoreButton.TabIndex = 13;
			this.LearnMoreButton.ToolTipCaption = null;
			this.LearnMoreButton.UseVisualStyleBackColor = true;
			this.LearnMoreButton.Click += new EventHandler(this.LearnMoreButton_Click);
			// 
			// DisclaimerMessageLabel
			// 
			this.DisclaimerMessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6c887849-80f5-4885-908c-ce08bf9137ce", "E-Payment Disclaimer Message");
			this.DisclaimerMessageLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DisclaimerMessageLabel.ForeColor = System.Drawing.SystemColors.Highlight;
			this.DisclaimerMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(900, 14, true);
			this.DisclaimerMessageLabel.Name = "DisclaimerMessageLabel";
			this.DisclaimerMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 73, true);
			this.DisclaimerMessageLabel.TabIndex = 12;
			this.DisclaimerMessageLabel.UseMnemonic = false;
			// 
			// CurrencySummaryGroupBox
			// 
			this.CurrencySummaryGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|4f70ce2b-ed30-4681-9657-896e49427600", "Summary by Currency");
			this.CurrencySummaryGroupBox.Controls.Add(this.CurrencySummaryTotalEPaymentFeesTextBox);
			this.CurrencySummaryGroupBox.Controls.Add(this.CurrencySummaryGrid);
			this.CurrencySummaryGroupBox.Controls.Add(this.CurrencySummaryTotalPaymentTextBox);
			this.CurrencySummaryGroupBox.Controls.Add(this.CurrencySummaryTotalEPaymentTextBox);
			this.CurrencySummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(454, 3, true);
			this.CurrencySummaryGroupBox.Name = "CurrencySummaryGroupBox";
			this.CurrencySummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 176, true);
			this.CurrencySummaryGroupBox.TabIndex = 0;
			this.CurrencySummaryGroupBox.TabStop = false;
			// 
			// CurrencySummaryTotalEPaymentFeesTextBox
			// 
			this.BindingSource.SetBindingMember(this.CurrencySummaryTotalEPaymentFeesTextBox, "TotalEPaymentFeeAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((APPaymentBatchPoster)(null)).TotalEPaymentFeeAmount)));
			this.CurrencySummaryTotalEPaymentFeesTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ED8422BE-8A9D-411F-902D-70C6F4436944", "Total Provider Fees in Funding Currency");
			this.CurrencySummaryTotalEPaymentFeesTextBox.DecimalPlaces = 2;
			this.CurrencySummaryTotalEPaymentFeesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 127, true);
			this.CurrencySummaryTotalEPaymentFeesTextBox.Name = "CurrencySummaryTotalEPaymentFeesTextBox";
			this.CurrencySummaryTotalEPaymentFeesTextBox.ReadOnly = true;
			this.CurrencySummaryTotalEPaymentFeesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.CurrencySummaryTotalEPaymentFeesTextBox.TabIndex = 3;
			this.CurrencySummaryTotalEPaymentFeesTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.CurrencySummaryTotalEPaymentFeesTextBox.TrackDisposedAccess = true;
			// 
			// CurrencySummaryGrid
			// 
			this.CurrencySummaryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CurrencySummaryGrid, "CurrencySummary.SummaryRows");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((APPaymentBatchPoster)(null)).CurrencySummary.SummaryRows)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CurrencySummaryRow)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).CurrencySummary.SummaryRows)).SyncRoot)).Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).CurrencySummary.SummaryRows)).SyncRoot)).ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).CurrencySummary.SummaryRows)).SyncRoot)).Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).CurrencySummary.SummaryRows)).SyncRoot)).LocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).CurrencySummary.SummaryRows)).SyncRoot)).AverageExRate)));
			this.CurrencySummaryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo26.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|b1e4cec9-e560-41f0-b334-8e657afba5dd", "Currency");
			zTextBoxColumnStyleInfo26.ColumnName = "Currency";
			zTextBoxColumnStyleInfo26.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo26.IsReadOnly = true;
			zTextBoxColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(56);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|a46bf7eb-5515-471c-9108-852700a20a19", "Exchange Rate");
			zCalcEditColumnStyleInfo11.ColumnName = "ExchangeRate";
			zCalcEditColumnStyleInfo11.Decimals = 4;
			zCalcEditColumnStyleInfo11.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|88c1ce47-9211-48d1-841f-8aad8ad6a807", "OS Total Amount");
			zCalcEditColumnStyleInfo12.ColumnName = "Amount";
			zCalcEditColumnStyleInfo12.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo12.IsReadOnly = true;
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(56);
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|104944b3-d6cd-4cf3-9a7b-04ca3e6fe5af", "Local Total Amount");
			zCalcEditColumnStyleInfo13.ColumnName = "LocalAmount";
			zCalcEditColumnStyleInfo13.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo13.IsReadOnly = true;
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(56);
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|29d9bc27-4de6-4c97-bf94-e6372bac0c57", "Average Exchange Rate");
			zCalcEditColumnStyleInfo14.ColumnName = "AverageExRate";
			zCalcEditColumnStyleInfo14.Decimals = 4;
			zCalcEditColumnStyleInfo14.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo14.IsReadOnly = true;
			zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(56);
			this.CurrencySummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo26);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.CurrencySummaryGrid.GridId = "7ad8da0e-aaa6-4c92-b1ff-8a8e478a1244";
			this.CurrencySummaryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CurrencySummaryGrid.LayoutKey = "CurrencySummary";
			this.CurrencySummaryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 17, true);
			this.CurrencySummaryGrid.Name = "CurrencySummaryGrid";
			this.CurrencySummaryGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.CurrencySummaryGrid.RowHeadersVisible = false;
			this.CurrencySummaryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 82, true);
			this.CurrencySummaryGrid.TabIndex = 0;
			// 
			// CurrencySummaryTotalPaymentTextBox
			// 
			this.BindingSource.SetBindingMember(this.CurrencySummaryTotalPaymentTextBox, "CurrencySummary.TransactionsLocalAmountTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((APPaymentBatchPoster)(null)).CurrencySummary.TransactionsLocalAmountTotal)));
			this.CurrencySummaryTotalPaymentTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|531c0b6d-42b3-49e7-88d8-53c3b87e806a", "Total for all Payments in Local Currency");
			this.CurrencySummaryTotalPaymentTextBox.DecimalPlaces = 2;
			this.CurrencySummaryTotalPaymentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 103, true);
			this.CurrencySummaryTotalPaymentTextBox.Name = "CurrencySummaryTotalPaymentTextBox";
			this.CurrencySummaryTotalPaymentTextBox.ReadOnly = true;
			this.CurrencySummaryTotalPaymentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.CurrencySummaryTotalPaymentTextBox.TabIndex = 1;
			this.CurrencySummaryTotalPaymentTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.CurrencySummaryTotalPaymentTextBox.TrackDisposedAccess = true;
			// 
			// CurrencySummaryTotalEPaymentTextBox
			// 
			this.BindingSource.SetBindingMember(this.CurrencySummaryTotalEPaymentTextBox, "TotalEPaymentCostAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((APPaymentBatchPoster)(null)).TotalEPaymentCostAmount)));
			this.CurrencySummaryTotalEPaymentTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|63B2CEB7-352E-4D72-A6A5-137A39E634D2", "Total Cost of E-Payments in Funding Currency");
			this.CurrencySummaryTotalEPaymentTextBox.DecimalPlaces = 2;
			this.CurrencySummaryTotalEPaymentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 151, true);
			this.CurrencySummaryTotalEPaymentTextBox.Name = "CurrencySummaryTotalEPaymentTextBox";
			this.CurrencySummaryTotalEPaymentTextBox.ReadOnly = true;
			this.CurrencySummaryTotalEPaymentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.CurrencySummaryTotalEPaymentTextBox.TabIndex = 2;
			this.CurrencySummaryTotalEPaymentTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.CurrencySummaryTotalEPaymentTextBox.TrackDisposedAccess = true;
			// 
			// zTabPage2
			// 
			this.zTabPage2.BackColor = System.Drawing.SystemColors.Control;
			this.zTabPage2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a75e6330-c5c2-4c9b-8094-f21562d46f2b", "E-Payment Processing");
			this.zTabPage2.Controls.Add(this.TabControlSummaryAndDetails);
			this.zTabPage2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage2.Name = "zTabPage2";
			this.zTabPage2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPage2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 238, true);
			this.zTabPage2.TabIndex = 1;
			// 
			// TabControlSummaryAndDetails
			// 
			this.TabControlSummaryAndDetails.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControlSummaryAndDetails.Controls.Add(this.zTabPage3);
			this.TabControlSummaryAndDetails.Controls.Add(this.zTabPage4);
			this.TabControlSummaryAndDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControlSummaryAndDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TabControlSummaryAndDetails.Name = "TabControlSummaryAndDetails";
			this.TabControlSummaryAndDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1220, 232, true);
			this.TabControlSummaryAndDetails.TabIndex = 0;
			// 
			// zTabPage3
			// 
			this.zTabPage3.BackColor = System.Drawing.SystemColors.Control;
			this.zTabPage3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c0ddff5a-d15d-44d9-ab83-0d8f214af318", "Summary");
			this.zTabPage3.Controls.Add(this.zGroupBox3);
			this.zTabPage3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage3.Name = "zTabPage3";
			this.zTabPage3.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPage3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1212, 205, true);
			this.zTabPage3.TabIndex = 1;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6aba063d-8b4e-4605-875a-faf13a2ff181", "Quote Summary");
			this.zGroupBox3.Controls.Add(this.SyncRecipientsButton);
			this.zGroupBox3.Controls.Add(this.AcceptQuotesButton);
			this.zGroupBox3.Controls.Add(this.RefreshButton);
			this.zGroupBox3.Controls.Add(this.quoteSummaryGrid);
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 6, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1201, 192, true);
			this.zGroupBox3.TabIndex = 1;
			this.zGroupBox3.TabStop = false;
			// 
			// SyncRecipientsButton
			// 
			this.SyncRecipientsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f88a861b-af71-4e01-8a47-ba2cc547e89b", "Sync Recipients");
			this.SyncRecipientsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 19, true);
			this.SyncRecipientsButton.Name = "SyncRecipientsButton";
			this.SyncRecipientsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 27, true);
			this.SyncRecipientsButton.TabIndex = 3;
			this.SyncRecipientsButton.ToolTipCaption = null;
			this.SyncRecipientsButton.UseVisualStyleBackColor = true;
			this.SyncRecipientsButton.Click += new EventHandler(this.SyncRecipientsButton_Click);
			// 
			// AcceptQuotesButton
			// 
			this.AcceptQuotesButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("41a2932c-09c6-4e5f-b883-eb4a9503b977", "Accept Quotes");
			this.AcceptQuotesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(201, 19, true);
			this.AcceptQuotesButton.Name = "AcceptQuotesButton";
			this.AcceptQuotesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 27, true);
			this.AcceptQuotesButton.TabIndex = 2;
			this.AcceptQuotesButton.ToolTipCaption = null;
			this.AcceptQuotesButton.UseVisualStyleBackColor = true;
			this.AcceptQuotesButton.Click += new EventHandler(this.AcceptQuotesButton_Click);
			// 
			// RefreshButton
			// 
			this.RefreshButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("32273d61-95c2-4369-97fe-deda7ba8049d", "Refresh");
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 27, true);
			this.RefreshButton.TabIndex = 1;
			this.RefreshButton.ToolTipCaption = null;
			this.RefreshButton.UseVisualStyleBackColor = true;
			this.RefreshButton.Click += new EventHandler(this.RefreshButton_Click);
			// 
			// quoteSummaryGrid
			// 
			this.quoteSummaryGrid.AllowNavigation = false;
			this.quoteSummaryGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.quoteSummaryGrid, "EPaymentQuotesSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((APPaymentBatchPoster)(null)).EPaymentQuotesSummary)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteSummary)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).EPaymentQuotesSummary)).SyncRoot)).ProviderCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteSummary)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).EPaymentQuotesSummary)).SyncRoot)).PaymentCurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteSummary)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).EPaymentQuotesSummary)).SyncRoot)).FundingCurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteSummary)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).EPaymentQuotesSummary)).SyncRoot)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EPaymentQuoteSummary)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).EPaymentQuotesSummary)).SyncRoot)).ErrorMessage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((EPaymentQuoteSummary)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).EPaymentQuotesSummary)).SyncRoot)).MostRecentUpdateLocalTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteSummary)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).EPaymentQuotesSummary)).SyncRoot)).TotalPaymentAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteSummary)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).EPaymentQuotesSummary)).SyncRoot)).TotalFundingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteSummary)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).EPaymentQuotesSummary)).SyncRoot)).TotalFeeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteSummary)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).EPaymentQuotesSummary)).SyncRoot)).AverageExRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((EPaymentQuoteSummary)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).EPaymentQuotesSummary)).SyncRoot)).AverageInverseExRate)));
			this.quoteSummaryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo27.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("185ec6a4-16d4-4467-a7bf-ce6efd7af98f", "Provider");
			zTextBoxColumnStyleInfo27.ColumnName = "ProviderCode";
			zTextBoxColumnStyleInfo27.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo28.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c28edace-8e29-4d15-ab3f-c057608f723f", "Payment Currency");
			zTextBoxColumnStyleInfo28.ColumnName = "PaymentCurrencyCode";
			zTextBoxColumnStyleInfo28.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo28.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo29.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c55487c9-faf0-493b-9987-0b0a26faddaa", "Status");
			zTextBoxColumnStyleInfo29.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo29.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo29.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo30.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("436ade93-58c5-4a5d-b15f-ccd8d8812ed9", "Error", "Error Message");
			zTextBoxColumnStyleInfo30.ColumnName = "ErrorMessage";
			zTextBoxColumnStyleInfo30.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo30.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("81e01165-ce2c-485d-a79a-f26c02e88751", "Received", "Received Date/Time");
			zDateEditColumnStyleInfo14.ColumnName = "MostRecentUpdateLocalTime";
			zDateEditColumnStyleInfo14.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo14.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("aaad4ccd-47c1-458f-9d6f-43a0aaffcd69", "Payment Amount");
			zCalcEditColumnStyleInfo15.ColumnName = "TotalPaymentAmount";
			zCalcEditColumnStyleInfo15.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo16.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0ba173fc-bed8-446b-ab83-80fa291af7a9", "Funding Amount");
			zCalcEditColumnStyleInfo16.ColumnName = "TotalFundingAmount";
			zCalcEditColumnStyleInfo16.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo17.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1596797c-1d8b-4a10-8ae3-fd84a1cd90de", "Fee Amount");
			zCalcEditColumnStyleInfo17.ColumnName = "TotalFeeAmount";
			zCalcEditColumnStyleInfo17.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo18.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo18.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8766f580-c087-42c3-922d-284569907fd1", "Ex Rate", "Exchange Rate");
			zCalcEditColumnStyleInfo18.ColumnName = "AverageExRate";
			zCalcEditColumnStyleInfo18.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo19.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo19.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f8ab7c91-42bc-4be4-99af-71ebf9ce5226", "Inverse Ex Rate", "Inverse Exchange Rate");
			zCalcEditColumnStyleInfo19.ColumnName = "AverageInverseExRate";
			zCalcEditColumnStyleInfo19.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo36.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bd080175-1b40-4d64-84d9-aaa8084d351b", "Funding Currency");
			zTextBoxColumnStyleInfo36.ColumnName = "FundingCurrencyCode";
			zTextBoxColumnStyleInfo36.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.quoteSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo27);
			this.quoteSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo28);
			this.quoteSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.quoteSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo29);
			this.quoteSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo30);
			this.quoteSummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo14);
			this.quoteSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo36);
			this.quoteSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
			this.quoteSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
			this.quoteSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo18);
			this.quoteSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo19);
			this.quoteSummaryGrid.GridId = "0e54f899-f977-4319-918b-cb89fe432180";
			this.quoteSummaryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.quoteSummaryGrid.LayoutKey = "quoteSummaryGrid";
			this.quoteSummaryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 52, true);
			this.quoteSummaryGrid.Name = "quoteSummaryGrid";
			this.quoteSummaryGrid.ReadOnly = true;
			this.quoteSummaryGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.quoteSummaryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 134, true);
			this.quoteSummaryGrid.TabIndex = 3;
			// 
			// zTabPage4
			// 
			this.zTabPage4.BackColor = System.Drawing.SystemColors.Control;
			this.zTabPage4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("59a0bbd0-9984-44ea-944e-075c44e7b5dc", "Details");
			this.zTabPage4.Controls.Add(this.QuoteFilterPanel);
			this.zTabPage4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage4.Name = "zTabPage4";
			this.zTabPage4.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPage4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1212, 205, true);
			this.zTabPage4.TabIndex = 0;
			// 
			// QuoteFilterPanel
			// 
			this.QuoteFilterPanel.Controls.Add(this.QuoteGridPanel);
			this.QuoteFilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QuoteFilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.QuoteFilterPanel.Name = "QuoteFilterPanel";
			this.QuoteFilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1206, 199, true);
			this.QuoteFilterPanel.TabIndex = 0;
			// 
			// QuoteGridPanel
			// 
			this.QuoteGridPanel.Controls.Add(this.QuoteGrid);
			this.QuoteGridPanel.Controls.Add(this.QuoteNotificationPanel);
			this.QuoteGridPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.QuoteGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 38, true);
			this.QuoteGridPanel.Name = "QuoteGridPanel";
			this.QuoteGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1206, 161, true);
			this.QuoteGridPanel.TabIndex = 10;
			// 
			// QuoteGrid
			// 
			this.QuoteGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.QuoteGrid, "FilteredEPaymentQuotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((APPaymentBatchPoster)(null)).FilteredEPaymentQuotes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Business.ARAP.EPayment.EPaymentQuote)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).FilteredEPaymentQuotes)).SyncRoot)).QU_ProviderCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Business.ARAP.EPayment.EPaymentQuote)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).FilteredEPaymentQuotes)).SyncRoot)).QU_RX_NKToCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.ARAP.EPayment.EPaymentQuote)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).FilteredEPaymentQuotes)).SyncRoot)).QU_ToAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Business.ARAP.EPayment.EPaymentQuote)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).FilteredEPaymentQuotes)).SyncRoot)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((Business.ARAP.EPayment.EPaymentQuote)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).FilteredEPaymentQuotes)).SyncRoot)).LastResponseReceivedLocalTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.ARAP.EPayment.EPaymentQuote)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).FilteredEPaymentQuotes)).SyncRoot)).QU_ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.ARAP.EPayment.EPaymentQuote)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).FilteredEPaymentQuotes)).SyncRoot)).QU_ExchangeRateInverted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Business.ARAP.EPayment.EPaymentQuote)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).FilteredEPaymentQuotes)).SyncRoot)).QU_RX_NKFromCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.ARAP.EPayment.EPaymentQuote)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).FilteredEPaymentQuotes)).SyncRoot)).QU_FromAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.ARAP.EPayment.EPaymentQuote)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).FilteredEPaymentQuotes)).SyncRoot)).QU_FeeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Business.ARAP.EPayment.EPaymentQuote)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).FilteredEPaymentQuotes)).SyncRoot)).QU_InternalReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Business.ARAP.EPayment.EPaymentQuote)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).FilteredEPaymentQuotes)).SyncRoot)).QU_ProviderReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Business.ARAP.EPayment.EPaymentQuote)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).FilteredEPaymentQuotes)).SyncRoot)).QU_ErrorDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Business.ARAP.EPayment.EPaymentQuote)(((System.Collections.IList)(((APPaymentBatchPoster)(null)).FilteredEPaymentQuotes)).SyncRoot)).OrganisationPK)));
			this.QuoteGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo31.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("80850ff6-1dce-47dd-bd74-e46940c17323", "Provider");
			zTextBoxColumnStyleInfo31.ColumnName = "QU_ProviderCode";
			zTextBoxColumnStyleInfo31.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo31.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("659b9e9d-4379-48a2-9eb3-7e73803a5013", "Payment Currency");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "QU_RX_NKToCurrency";
			zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo20.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo20.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("70b9abad-b222-4927-8acb-75bc3c27e49f", "Payment Amount");
			zCalcEditColumnStyleInfo20.ColumnName = "QU_ToAmount";
			zCalcEditColumnStyleInfo20.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo32.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("93400ec6-84ab-4b33-adf8-7994e16aad4f", "Status");
			zTextBoxColumnStyleInfo32.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo32.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo32.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3cf25412-d433-431b-85f5-22b75eab2b7c", "Received", "Received Date/Time");
			zDateEditColumnStyleInfo15.ColumnName = "LastResponseReceivedLocalTime";
			zDateEditColumnStyleInfo15.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo15.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo21.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo21.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9818887a-258a-4941-a768-e8f87c4530db", "Ex Rate", "Exchange Rate");
			zCalcEditColumnStyleInfo21.ColumnName = "QU_ExchangeRate";
			zCalcEditColumnStyleInfo21.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo22.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo22.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a2ea84a4-eeed-42b8-93a0-e4a5a3f29574", "Inverse Ex Rate", "Inverse Exchange Rate");
			zCalcEditColumnStyleInfo22.ColumnName = "QU_ExchangeRateInverted";
			zCalcEditColumnStyleInfo22.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5d95df0e-1859-468a-8a57-dde14bdbaa15", "Funding Currency");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "QU_RX_NKFromCurrency";
			zCodeFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo23.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo23.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9646c1f4-b35f-434d-a0c3-7b352e6bbdc1", "Funding Amount");
			zCalcEditColumnStyleInfo23.ColumnName = "QU_FromAmount";
			zCalcEditColumnStyleInfo23.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo24.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo24.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c7180547-c378-4b5f-af49-4384002daacd", "Fee", "Processing Fee");
			zCalcEditColumnStyleInfo24.ColumnName = "QU_FeeAmount";
			zCalcEditColumnStyleInfo24.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo33.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("298dd41b-8ff7-4bc5-9c7c-b8680e1dc4df", "Quote #", "Quote Number");
			zTextBoxColumnStyleInfo33.ColumnName = "QU_InternalReference";
			zTextBoxColumnStyleInfo33.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo33.IsVisible = false;
			zTextBoxColumnStyleInfo33.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo34.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d55640b2-9c3e-4e66-a8ae-4f00e62a5580", "Provider Ref", "Provider Reference");
			zTextBoxColumnStyleInfo34.ColumnName = "QU_ProviderReference";
			zTextBoxColumnStyleInfo34.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo34.IsVisible = false;
			zTextBoxColumnStyleInfo34.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo35.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9cb08019-8da2-42bb-89eb-581f125650b2", "Error", "Error Message");
			zTextBoxColumnStyleInfo35.ColumnName = "QU_ErrorDescription";
			zTextBoxColumnStyleInfo35.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo35.IsVisible = false;
			zTextBoxColumnStyleInfo35.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("67df2677-c049-4998-bef8-5cfbbc979c10", "Creditor");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "OrganisationPK";
			zGuidFindBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.QuoteGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo31);
			this.QuoteGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.QuoteGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo20);
			this.QuoteGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo32);
			this.QuoteGrid.ColumnStyles.Add(zDateEditColumnStyleInfo15);
			this.QuoteGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo21);
			this.QuoteGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo22);
			this.QuoteGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.QuoteGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo23);
			this.QuoteGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo24);
			this.QuoteGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo33);
			this.QuoteGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo34);
			this.QuoteGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo35);
			this.QuoteGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.QuoteGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QuoteGrid.GridId = "f25d2950-3e08-439c-91bf-a72688895fa8";
			this.QuoteGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QuoteGrid.IsWholeRowSelectedOnClick = true;
			this.QuoteGrid.LayoutKey = "QuoteGrid";
			this.QuoteGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.QuoteGrid.Name = "QuoteGrid";
			this.QuoteGrid.ReadOnly = true;
			this.QuoteGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.QuoteGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1206, 129, true);
			this.QuoteGrid.TabIndex = 1;
			// 
			// QuoteNotificationPanel
			// 
			this.QuoteNotificationPanel.Controls.Add(this.QuoteInfoLabel);
			this.QuoteNotificationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.QuoteNotificationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QuoteNotificationPanel.Name = "QuoteNotificationPanel";
			this.QuoteNotificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1206, 32, true);
			this.QuoteNotificationPanel.TabIndex = 0;
			// 
			// QuoteInfoLabel
			// 
			this.QuoteInfoLabel.AutoSize = true;
			this.QuoteInfoLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9ed48d83-45fb-4e2d-bfe4-e1c57b8e4490", "Please enter the filter criteria.");
			this.QuoteInfoLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.QuoteInfoLabel.ForeColor = System.Drawing.SystemColors.InfoText;
			this.QuoteInfoLabel.IsFontBold = true;
			this.QuoteInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.QuoteInfoLabel.Name = "QuoteInfoLabel";
			this.QuoteInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 13, true);
			this.QuoteInfoLabel.TabIndex = 0;
			this.QuoteInfoLabel.UseMnemonic = false;
			// 
			// ProcessEPaymentsButton
			// 
			this.ProcessEPaymentsButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ProcessEPaymentsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b6d795b1-1dc8-470f-8cb3-b860848ad332", "Process E-Payments");
			this.ProcessEPaymentsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 225, true);
			this.ProcessEPaymentsButton.Name = "ProcessEPaymentsButton";
			this.ProcessEPaymentsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 24, true);
			this.ProcessEPaymentsButton.TabIndex = 0;
			this.ProcessEPaymentsButton.ToolTipCaption = null;
			this.ProcessEPaymentsButton.UseVisualStyleBackColor = true;
			this.ProcessEPaymentsButton.Click += new EventHandler(this.ProcessEPaymentsButton_Click);
			// 
			// PaymentBatchForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchForm|a1c60680-4a57-40df-a674-448424ccfc88", "AP Payment Batch Posting");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1234, 686, true);
			this.Controls.Add(this.splitContainer1);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(APPaymentBatchPoster);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.ReceiptPayment.APPaymentBatchPoster";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1250, 725, true);
			this.Name = "PaymentBatchForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.splitContainer1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.PaymentDetailsGroupBox.ResumeLayout(false);
			this.PaymentDetailsGroupBox.PerformLayout();
			this.PostDateEdit.ResumeLayout(true);
			this.PostDateEdit.PerformLayout();
			this.InvoiceDateEdit.ResumeLayout(true);
			this.InvoiceDateEdit.PerformLayout();
			this.BankDetailsGroupBox.ResumeLayout(false);
			this.BankDetailsGroupBox.PerformLayout();
			this.FundingCurrencyCodeFindBox.ResumeLayout(true);
			this.FundingCurrencyCodeFindBox.PerformLayout();
			this.FundingBankAccountFindBox.ResumeLayout(true);
			this.FundingBankAccountFindBox.PerformLayout();
			this.ChequeBookGuidFindBox.ResumeLayout(true);
			this.ChequeBookGuidFindBox.PerformLayout();
			this.BankAccountGuidFindBox.ResumeLayout(true);
			this.BankAccountGuidFindBox.PerformLayout();
			this.PaymentTypeDropEdit.ResumeLayout(true);
			this.PaymentTypeDropEdit.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.BalanceCalcFindBox.ResumeLayout(true);
			this.BalanceCalcFindBox.PerformLayout();
			this.CreditorFindBox.ResumeLayout(true);
			this.CreditorFindBox.PerformLayout();
			((ISupportInitialize)(this.MatchTransactionsGrid)).EndInit();
			this.MatchTransactionsGrid.ResumeLayout(false);
			this.MatchTransactionsGrid.PerformLayout();
			this.ExchangeGainLossZCalcFindBox.ResumeLayout(true);
			this.ExchangeGainLossZCalcFindBox.PerformLayout();
			this.DiscountZCalcFindBox.ResumeLayout(true);
			this.DiscountZCalcFindBox.PerformLayout();
			this.OverpaymentZCalcFindBox.ResumeLayout(true);
			this.OverpaymentZCalcFindBox.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((ISupportInitialize)(this.PaymentBatchGrid)).EndInit();
			this.PaymentBatchGrid.ResumeLayout(false);
			this.PaymentBatchGrid.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer2.PerformLayout();
			this.TabControlBankAndEPayment.ResumeLayout(false);
			this.TabControlBankAndEPayment.PerformLayout();
			this.zTabPage1.ResumeLayout(false);
			this.zTabPage1.PerformLayout();
			((ISupportInitialize)(this.ProviderLogoPictureBox)).EndInit();
			this.CurrencySummaryGroupBox.ResumeLayout(false);
			this.CurrencySummaryGroupBox.PerformLayout();
			((ISupportInitialize)(this.CurrencySummaryGrid)).EndInit();
			this.CurrencySummaryGrid.ResumeLayout(false);
			this.CurrencySummaryGrid.PerformLayout();
			this.zTabPage2.ResumeLayout(false);
			this.zTabPage2.PerformLayout();
			this.TabControlSummaryAndDetails.ResumeLayout(false);
			this.TabControlSummaryAndDetails.PerformLayout();
			this.zTabPage3.ResumeLayout(false);
			this.zTabPage3.PerformLayout();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			((ISupportInitialize)(this.quoteSummaryGrid)).EndInit();
			this.quoteSummaryGrid.ResumeLayout(false);
			this.quoteSummaryGrid.PerformLayout();
			this.zTabPage4.ResumeLayout(false);
			this.zTabPage4.PerformLayout();
			this.QuoteFilterPanel.ResumeLayout(false);
			this.QuoteFilterPanel.PerformLayout();
			this.QuoteGridPanel.ResumeLayout(false);
			this.QuoteGridPanel.PerformLayout();
			((ISupportInitialize)(this.QuoteGrid)).EndInit();
			this.QuoteGrid.ResumeLayout(false);
			this.QuoteGrid.PerformLayout();
			this.QuoteNotificationPanel.ResumeLayout(false);
			this.QuoteNotificationPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
