using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.OrgCollectionCalls;
using Enterprise.Accounting.GUI.ARAP.Statements;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.OrgCollectionCalls
{
	public partial class CollectionCallsTransactionsPrintingControl
	{


		#region Component Designer generated code

		internal ZDropEdit DateFilterDropEdit;
		ZDateEdit AH_FromDateDateEdit;
		ZDateEdit AH_ToDateDateEdit;
		ZCodeFindBox AH_RX_NKCodeFindBox;
		ZGuidFindBox AH_GEGuidFindBox;
		ZGuidFindBox AH_GBGuidFindBox;
		ZDropEdit PaymentStatusDropEdit;
		ZCheckBox IsDisbursementCheckBox;
		ZGroupBox TransactionFiltersGroupBox;
		ZButton PrintInvoiceButton;
		ZDropEdit TransactionTypeDropEdit;
		ZButton PrintStatementButton;
		ZLabel TwoTermsHeadingLabel;
		ZLabel DueTodayHeadingLabel;
		ZLabel TermsHeadingLabelLabel;
		ZLabel TotalOutstandingHeadingLabel;
		ZLabel OverThreeTermsHeadingLabel;
		ZLabel ThreeTermsHeadingLabel;
		ZCalcEdit DisbursementOutstandingCalcEdit;
		ZCalcEdit StandardOverTwoTermsPastDueCalcEdit;
		ZCalcEdit DisbursementOverTwoTermsPastDueCalcEdit;
		ZCalcEdit TotalOverTwoTermsPastDueCalcEdit;
		ZCalcEdit StandardNotYetDueCalcEdit;
		ZLabel NotYetDueHeadingLabel;
		ZLabel GrandTotalLabel;
		ZCalcEdit TotalDueTodayCalcEdit;
		ZCalcEdit DisbursementDueTodayCalcEdit;
		ZCalcEdit StandardDueTodayCalcEdit;
		ZCalcEdit TotalOneTermPastDueCalcEdit;
		ZCalcEdit DisbursementOneTermPastDueCalcEdit;
		ZCalcEdit DisbursementNotYetDueCalcEdit;
		ZLabel TotalPastDueLabel;
		ZLabel TransactionsPastDueHeadingLabel;
		ZLabel TotalOutstandingLabel;
		ZLabel TotalPastDueHeadingLabel;
		ZLabel TotalPercentagePastDueLabel;
		ZCalcEdit TotalTwoTermsPastDueCalcEdit;
		ZCalcEdit DisbursementTwoTermsPastDueCalcEdit;
		ZCalcEdit StandardTwoTermsPastDueCalcEdit;
		ZCalcEdit StandardOneTermPastDueCalcEdit;
		ZCalcEdit TotalNotYetDueCalcEdit;
		ZCalcEdit StandardOutstandingCalcEdit;
		internal ZDropEdit NumberFilterDropEdit;
		ZTextBox AH_NumberTextBox;
		ZButton ClearButton;
		ZButton FindButton;
		ZGrid InvoicesGrid;
		ZTabControl statusTermTabControl;
		ZTabPage agingTabPage;
		ZTabPage termsTabPage;
		ZPanel termsPanel;
		System.ComponentModel.IContainer components;

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			this.ClearButton = new ZButton();
			this.FindButton = new ZButton();
			this.InvoicesGrid = new ZGrid();
			this.AH_RX_NKCodeFindBox = new ZCodeFindBox();
			this.AH_GEGuidFindBox = new ZGuidFindBox();
			this.AH_GBGuidFindBox = new ZGuidFindBox();
			this.PaymentStatusDropEdit = new ZDropEdit();
			this.IsDisbursementCheckBox = new ZCheckBox();
			this.TransactionFiltersGroupBox = new ZGroupBox();
			this.AH_FromDateDateEdit = new ZDateEdit();
			this.AH_ToDateDateEdit = new ZDateEdit();
			this.DateFilterDropEdit = new ZDropEdit();
			this.AH_NumberTextBox = new ZTextBox();
			this.NumberFilterDropEdit = new ZDropEdit();
			this.TransactionTypeDropEdit = new ZDropEdit();
			this.PrintInvoiceButton = new ZButton();
			this.PrintStatementButton = new ZButton();
			this.TotalPastDueLabel = new ZLabel();
			this.TotalPastDueHeadingLabel = new ZLabel();
			this.TotalPercentagePastDueLabel = new ZLabel();
			this.TotalOutstandingLabel = new ZLabel();
			this.TotalOverTwoTermsPastDueCalcEdit = new ZCalcEdit();
			this.TotalTwoTermsPastDueCalcEdit = new ZCalcEdit();
			this.TotalOneTermPastDueCalcEdit = new ZCalcEdit();
			this.TotalDueTodayCalcEdit = new ZCalcEdit();
			this.TotalNotYetDueCalcEdit = new ZCalcEdit();
			this.GrandTotalLabel = new ZLabel();
			this.DisbursementOutstandingCalcEdit = new ZCalcEdit();
			this.DisbursementOverTwoTermsPastDueCalcEdit = new ZCalcEdit();
			this.DisbursementTwoTermsPastDueCalcEdit = new ZCalcEdit();
			this.DisbursementOneTermPastDueCalcEdit = new ZCalcEdit();
			this.DisbursementDueTodayCalcEdit = new ZCalcEdit();
			this.DisbursementNotYetDueCalcEdit = new ZCalcEdit();
			this.StandardOutstandingCalcEdit = new ZCalcEdit();
			this.StandardOverTwoTermsPastDueCalcEdit = new ZCalcEdit();
			this.StandardTwoTermsPastDueCalcEdit = new ZCalcEdit();
			this.StandardOneTermPastDueCalcEdit = new ZCalcEdit();
			this.StandardDueTodayCalcEdit = new ZCalcEdit();
			this.StandardNotYetDueCalcEdit = new ZCalcEdit();
			this.TransactionsPastDueHeadingLabel = new ZLabel();
			this.TermsHeadingLabelLabel = new ZLabel();
			this.TotalOutstandingHeadingLabel = new ZLabel();
			this.OverThreeTermsHeadingLabel = new ZLabel();
			this.ThreeTermsHeadingLabel = new ZLabel();
			this.TwoTermsHeadingLabel = new ZLabel();
			this.DueTodayHeadingLabel = new ZLabel();
			this.NotYetDueHeadingLabel = new ZLabel();
			this.statusTermTabControl = new ZTabControl();
			this.agingTabPage = new ZTabPage();
			this.termsTabPage = new ZTabPage();
			this.termsPanel = new ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).BeginInit();
			this.InvoicesGrid.SuspendLayout();
			this.AH_RX_NKCodeFindBox.SuspendLayout();
			this.AH_GEGuidFindBox.SuspendLayout();
			this.AH_GBGuidFindBox.SuspendLayout();
			this.PaymentStatusDropEdit.SuspendLayout();
			this.TransactionFiltersGroupBox.SuspendLayout();
			this.AH_FromDateDateEdit.SuspendLayout();
			this.AH_ToDateDateEdit.SuspendLayout();
			this.DateFilterDropEdit.SuspendLayout();
			this.NumberFilterDropEdit.SuspendLayout();
			this.TransactionTypeDropEdit.SuspendLayout();
			this.statusTermTabControl.SuspendLayout();
			this.agingTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CollectionNotesTransactionsFilter);
			// 
			// ClearButton
			// 
			this.ClearButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|0235e017-fa0d-4e9c-a648-0d61b9ed52a4", "Clear");
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(703, 54, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ClearButton.TabIndex = 21;
			this.ClearButton.Click += new EventHandler(this.ClearButton_Click);
			// 
			// FindButton
			// 
			this.FindButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.FindButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|3e220090-6f96-4de9-8bb6-5a4d3373852b", "Find");
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(703, 29, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.FindButton.TabIndex = 20;
			this.FindButton.Click += new EventHandler(this.FindButton_Click);
			// 
			// InvoicesGrid
			// 
			this.InvoicesGrid.AllowNavigation = false;
			this.InvoicesGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InvoicesGrid, "Transactions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_ConsolidatedInvoiceRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_IsDisbursementCalc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_TransactionCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_OSTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_LocalTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_OutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).DaysOverdue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_TransactionReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_FullyPaidDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).Transactions)).SyncRoot)).AH_Desc)));
			this.InvoicesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "AH_Ledger";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "AH_TransactionType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|149d9a8e-0d1d-4459-a415-78c35b9965d9", "Transaction Num.", "Transaction Number");
			zTextBoxColumnStyleInfo3.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|a51a0a8f-c047-4d6c-bf9b-6f7156baf7c9", "Job Invoice Number");
			zTextBoxColumnStyleInfo4.ColumnName = "AH_ConsolidatedInvoiceRef";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "AH_ChequeOrReference";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "AH_DueDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AH_RX_NKTransactionCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AH_GB";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AH_GE";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|8a8ad504-6710-4b4e-a1e4-df33e65c0a5a", "Disbursement");
			zCheckBoxColumnStyleInfo1.ColumnName = "AH_IsDisbursementCalc";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "AH_TransactionCategory";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|8bd00b33-ce78-4e76-9821-86fd8d4fc705", "Trans. Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "AH_OSTotal";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|6f98b7eb-bd51-4f98-8ac4-724587d20f11", "Local Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "AH_LocalTotal";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "AH_OutstandingAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|2fbf5417-9323-4add-ab88-3fd3b192f194", "Days Overdue");
			zCalcEditColumnStyleInfo4.ColumnName = "DaysOverdue";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|291f66e0-516f-4ea6-a3da-071beb74935b", "Post Date");
			zDateEditColumnStyleInfo3.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|26336055-f250-40e0-ab9c-c42610741999", "Govt Tax Invoice Num.", "Govt Tax Invoice Number");
			zTextBoxColumnStyleInfo7.ColumnName = "AH_TransactionReference";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDateEditColumnStyleInfo4.ColumnName = "AH_FullyPaidDate";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|568be95f-1710-45cf-9474-fd2b83d8b922", "Invoice Desc.", "Invoice Description");
			zTextBoxColumnStyleInfo8.ColumnName = "AH_Desc";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.InvoicesGrid.CopySelectedRowsAllowed = true;
			this.InvoicesGrid.GridId = "286666aa-32c8-40e2-955b-ed775819443e";
			this.InvoicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoicesGrid.IsWholeRowSelectedOnClick = true;
			this.InvoicesGrid.LayoutKey = "zGrid1";
			this.InvoicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 91, true);
			this.InvoicesGrid.Name = "InvoicesGrid";
			this.InvoicesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.InvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 111, true);
			this.InvoicesGrid.TabIndex = 1;
			this.InvoicesGrid.DoubleClick += new EventHandler(this.ViewInvoices);
			// 
			// AH_RX_NKCodeFindBox
			// 
			this.AH_RX_NKCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AH_RX_NKCodeFindBox, "AH_RX_NKTransactionCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CollectionNotesTransactionsFilter)(null)).AH_RX_NKTransactionCurrency)));
			this.AH_RX_NKCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|81f9709c-3550-4bc6-8b27-977f3dc50b83", "Currency", "Currency", "");
			this.AH_RX_NKCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(578, 16, true);
			this.AH_RX_NKCodeFindBox.Name = "AH_RX_NKCodeFindBox";
			this.AH_RX_NKCodeFindBox.ShowDescriptionBox = false;
			this.AH_RX_NKCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.AH_RX_NKCodeFindBox.TabIndex = 6;
			// 
			// AH_GEGuidFindBox
			// 
			this.AH_GEGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AH_GEGuidFindBox, "AH_GE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((CollectionNotesTransactionsFilter)(null)).AH_GE)));
			this.AH_GEGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|1931b96c-926f-4a1a-966a-b74a8cea9179", "Dept", "Department", "Dept.");
			this.AH_GEGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(578, 60, true);
			this.AH_GEGuidFindBox.Name = "AH_GEGuidFindBox";
			this.AH_GEGuidFindBox.ShowDescriptionBox = false;
			this.AH_GEGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.AH_GEGuidFindBox.TabIndex = 19;
			// 
			// AH_GBGuidFindBox
			// 
			this.AH_GBGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AH_GBGuidFindBox, "AH_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((CollectionNotesTransactionsFilter)(null)).AH_GB)));
			this.AH_GBGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|cbd005f6-1d91-4b9e-99c7-1065bf274c14", "Branch", "Branch", "");
			this.AH_GBGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(578, 38, true);
			this.AH_GBGuidFindBox.Name = "AH_GBGuidFindBox";
			this.AH_GBGuidFindBox.ShowDescriptionBox = false;
			this.AH_GBGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.AH_GBGuidFindBox.TabIndex = 11;
			// 
			// PaymentStatusDropEdit
			// 
			this.PaymentStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentStatusDropEdit, "PaymentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CollectionNotesTransactionsFilter)(null)).PaymentStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).PaymentStatusList)));
			this.PaymentStatusDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|1d64f553-9517-473c-826b-07d19e63f096", "Status", "Payment Status", "");
			this.PaymentStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 16, true);
			this.PaymentStatusDropEdit.Name = "PaymentStatusDropEdit";
			this.PaymentStatusDropEdit.PreBoundMaxLength = 4;
			this.PaymentStatusDropEdit.ShowDescriptionBox = false;
			this.PaymentStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 17, true);
			this.PaymentStatusDropEdit.TabIndex = 1;
			// 
			// IsDisbursementCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsDisbursementCheckBox, "AH_IsDisbursement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((CollectionNotesTransactionsFilter)(null)).AH_IsDisbursement)));
			this.IsDisbursementCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsDisbursementCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|5f3918fd-2212-4e4d-8b63-323f338682dd", "Disbursement Only", "Disbursement Only", "");
			this.IsDisbursementCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsDisbursementCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 15, true);
			this.IsDisbursementCheckBox.Name = "IsDisbursementCheckBox";
			this.IsDisbursementCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 24, true);
			this.IsDisbursementCheckBox.TabIndex = 4;
			// 
			// TransactionFiltersGroupBox
			// 
			this.TransactionFiltersGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.TransactionFiltersGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|92dbe8c7-8e83-47c5-8990-7da8cae9a005", "Transaction Filters");
			this.TransactionFiltersGroupBox.Controls.Add(this.AH_FromDateDateEdit);
			this.TransactionFiltersGroupBox.Controls.Add(this.AH_ToDateDateEdit);
			this.TransactionFiltersGroupBox.Controls.Add(this.DateFilterDropEdit);
			this.TransactionFiltersGroupBox.Controls.Add(this.AH_NumberTextBox);
			this.TransactionFiltersGroupBox.Controls.Add(this.NumberFilterDropEdit);
			this.TransactionFiltersGroupBox.Controls.Add(this.TransactionTypeDropEdit);
			this.TransactionFiltersGroupBox.Controls.Add(this.PaymentStatusDropEdit);
			this.TransactionFiltersGroupBox.Controls.Add(this.ClearButton);
			this.TransactionFiltersGroupBox.Controls.Add(this.FindButton);
			this.TransactionFiltersGroupBox.Controls.Add(this.AH_GBGuidFindBox);
			this.TransactionFiltersGroupBox.Controls.Add(this.IsDisbursementCheckBox);
			this.TransactionFiltersGroupBox.Controls.Add(this.AH_GEGuidFindBox);
			this.TransactionFiltersGroupBox.Controls.Add(this.AH_RX_NKCodeFindBox);
			this.TransactionFiltersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.TransactionFiltersGroupBox.Name = "TransactionFiltersGroupBox";
			this.TransactionFiltersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 83, true);
			this.TransactionFiltersGroupBox.TabIndex = 0;
			this.TransactionFiltersGroupBox.TabStop = false;
			// 
			// AH_FromDateDateEdit
			// 
			this.AH_FromDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_FromDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_FromDateDateEdit, "AH_FromDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CollectionNotesTransactionsFilter)(null)).AH_FromDate)));
			this.AH_FromDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|69546a1c-1c42-42a9-b262-06dd2ff68b62", "From", "From", "");
			this.AH_FromDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 60, true);
			this.AH_FromDateDateEdit.Name = "AH_FromDateDateEdit";
			this.AH_FromDateDateEdit.TabIndex = 15;
			// 
			// AH_ToDateDateEdit
			// 
			this.AH_ToDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_ToDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_ToDateDateEdit, "AH_ToDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CollectionNotesTransactionsFilter)(null)).AH_ToDate)));
			this.AH_ToDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|77e9a69e-153c-485d-bb2d-c6f18ead6a5b", "To");
			this.AH_ToDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(409, 60, true);
			this.AH_ToDateDateEdit.Name = "AH_ToDateDateEdit";
			this.AH_ToDateDateEdit.TabIndex = 17;
			// 
			// DateFilterDropEdit
			// 
			this.BindingSource.SetBindingMember(this.DateFilterDropEdit, "AH_DateFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CollectionNotesTransactionsFilter)(null)).AH_DateFilter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).AH_DateFilter_List)));
			this.DateFilterDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DateFilterDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|6549038c-890e-4e71-94d8-85b88dfbe26a", "Dates");
			this.DateFilterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 60, true);
			this.DateFilterDropEdit.Name = "DateFilterDropEdit";
			this.DateFilterDropEdit.ShowDescriptionBox = false;
			this.DateFilterDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.DateFilterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.DateFilterDropEdit.TabIndex = 13;
			// 
			// AH_NumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_NumberTextBox, "AH_Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CollectionNotesTransactionsFilter)(null)).AH_Number)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AH_NumberTextBox, false);
			this.AH_NumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 38, true);
			this.AH_NumberTextBox.Name = "AH_NumberTextBox";
			this.AH_NumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 17, true);
			this.AH_NumberTextBox.TabIndex = 9;
			// 
			// NumberFilterDropEdit
			// 
			this.BindingSource.SetBindingMember(this.NumberFilterDropEdit, "AH_NumberFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CollectionNotesTransactionsFilter)(null)).AH_NumberFilter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).AH_NumberFilterList)));
			this.NumberFilterDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NumberFilterDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|4e1df782-7894-4947-9e7f-d8d9e03d4aa4", "Numbers", "Numbers", "");
			this.NumberFilterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 38, true);
			this.NumberFilterDropEdit.Name = "NumberFilterDropEdit";
			this.NumberFilterDropEdit.ShowDescriptionBox = false;
			this.NumberFilterDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.NumberFilterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.NumberFilterDropEdit.TabIndex = 8;
			// 
			// TransactionTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.TransactionTypeDropEdit, "AH_TransactionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CollectionNotesTransactionsFilter)(null)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CollectionNotesTransactionsFilter)(null)).TransactionTypeList)));
			this.TransactionTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|1fa6adc2-69c8-4e66-85ba-271b84353e16", "Type", "Transaction Type", "");
			this.TransactionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 16, true);
			this.TransactionTypeDropEdit.Name = "TransactionTypeDropEdit";
			this.TransactionTypeDropEdit.PreBoundMaxLength = 4;
			this.TransactionTypeDropEdit.ShowDescriptionBox = false;
			this.TransactionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 17, true);
			this.TransactionTypeDropEdit.TabIndex = 3;
			// 
			// PrintInvoiceButton
			// 
			this.PrintInvoiceButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PrintInvoiceButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|cac77b80-8efc-4597-ad25-86d8555b5b95", "Print Selected Invoices");
			this.PrintInvoiceButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 206, true);
			this.PrintInvoiceButton.Name = "PrintInvoiceButton";
			this.PrintInvoiceButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 23, true);
			this.PrintInvoiceButton.TabIndex = 2;
			this.PrintInvoiceButton.Click += new EventHandler(this.PrintInvoices);
			// 
			// PrintStatementButton
			// 
			this.PrintStatementButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PrintStatementButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|d4e6268d-2211-4555-a391-8ea308ef2caa", "Print Collection Document");
			this.PrintStatementButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 206, true);
			this.PrintStatementButton.Name = "PrintStatementButton";
			this.PrintStatementButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 23, true);
			this.PrintStatementButton.TabIndex = 3;
			this.PrintStatementButton.Click += new EventHandler(this.PrintStatement_Click);
			// 
			// TotalPastDueLabel
			// 
			this.BindingSource.SetBindingMember(this.TotalPastDueLabel, "TotalPastDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CollectionNotesTransactionsFilter)(null)).TotalPastDue)));
			this.TotalPastDueLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TotalPastDueLabel.IsFontBold = true;
			this.TotalPastDueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(501, 105, true);
			this.TotalPastDueLabel.Name = "TotalPastDueLabel";
			this.TotalPastDueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalPastDueLabel.TabIndex = 32;
			this.TotalPastDueLabel.Text = "0.00";
			this.TotalPastDueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TotalPastDueHeadingLabel
			// 
			this.TotalPastDueHeadingLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|3723092f-1e2c-4ce0-a50d-33315807056f", "Total Past Due");
			this.TotalPastDueHeadingLabel.IsFontBold = true;
			this.TotalPastDueHeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(401, 105, true);
			this.TotalPastDueHeadingLabel.Name = "TotalPastDueHeadingLabel";
			this.TotalPastDueHeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalPastDueHeadingLabel.TabIndex = 31;
			this.TotalPastDueHeadingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TotalPercentagePastDueLabel
			// 
			this.BindingSource.SetBindingMember(this.TotalPercentagePastDueLabel, "TotalPercentagePastDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CollectionNotesTransactionsFilter)(null)).TotalPercentagePastDue)));
			this.TotalPercentagePastDueLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TotalPercentagePastDueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 105, true);
			this.TotalPercentagePastDueLabel.Name = "TotalPercentagePastDueLabel";
			this.TotalPercentagePastDueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 20, true);
			this.TotalPercentagePastDueLabel.TabIndex = 33;
			this.TotalPercentagePastDueLabel.Text = "%";
			this.TotalPercentagePastDueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TotalOutstandingLabel
			// 
			this.BindingSource.SetBindingMember(this.TotalOutstandingLabel, "TotalOutstanding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CollectionNotesTransactionsFilter)(null)).TotalOutstanding)));
			this.TotalOutstandingLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TotalOutstandingLabel.IsFontBold = true;
			this.TotalOutstandingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(620, 79, true);
			this.TotalOutstandingLabel.Name = "TotalOutstandingLabel";
			this.TotalOutstandingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.TotalOutstandingLabel.TabIndex = 30;
			this.TotalOutstandingLabel.Text = "0.00";
			this.TotalOutstandingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TotalOverTwoTermsPastDueCalcEdit
			// 
			this.TotalOverTwoTermsPastDueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalOverTwoTermsPastDueCalcEdit, "TotalOverTwoTermsPastDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).TotalOverTwoTermsPastDue)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalOverTwoTermsPastDueCalcEdit, false);
			this.TotalOverTwoTermsPastDueCalcEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TotalOverTwoTermsPastDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(501, 79, true);
			this.TotalOverTwoTermsPastDueCalcEdit.Name = "TotalOverTwoTermsPastDueCalcEdit";
			this.TotalOverTwoTermsPastDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.TotalOverTwoTermsPastDueCalcEdit.TabIndex = 29;
			this.TotalOverTwoTermsPastDueCalcEdit.Text = "0.00";
			this.TotalOverTwoTermsPastDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalTwoTermsPastDueCalcEdit
			// 
			this.TotalTwoTermsPastDueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalTwoTermsPastDueCalcEdit, "TotalTwoTermsPastDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).TotalTwoTermsPastDue)));
			this.TotalTwoTermsPastDueCalcEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalTwoTermsPastDueCalcEdit, false);
			this.TotalTwoTermsPastDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(401, 79, true);
			this.TotalTwoTermsPastDueCalcEdit.Name = "TotalTwoTermsPastDueCalcEdit";
			this.TotalTwoTermsPastDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.TotalTwoTermsPastDueCalcEdit.TabIndex = 28;
			this.TotalTwoTermsPastDueCalcEdit.Text = "0.00";
			this.TotalTwoTermsPastDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalOneTermPastDueCalcEdit
			// 
			this.TotalOneTermPastDueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalOneTermPastDueCalcEdit, "TotalOneTermPastDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).TotalOneTermPastDue)));
			this.TotalOneTermPastDueCalcEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TotalOneTermPastDueCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalOneTermPastDueCalcEdit, false);
			this.TotalOneTermPastDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 79, true);
			this.TotalOneTermPastDueCalcEdit.Name = "TotalOneTermPastDueCalcEdit";
			this.TotalOneTermPastDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.TotalOneTermPastDueCalcEdit.TabIndex = 27;
			this.TotalOneTermPastDueCalcEdit.Text = "0.00";
			this.TotalOneTermPastDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalDueTodayCalcEdit
			// 
			this.TotalDueTodayCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalDueTodayCalcEdit, "TotalDueToday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).TotalDueToday)));
			this.TotalDueTodayCalcEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TotalDueTodayCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalDueTodayCalcEdit, false);
			this.TotalDueTodayCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 79, true);
			this.TotalDueTodayCalcEdit.Name = "TotalDueTodayCalcEdit";
			this.TotalDueTodayCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.TotalDueTodayCalcEdit.TabIndex = 26;
			this.TotalDueTodayCalcEdit.Text = "0.00";
			this.TotalDueTodayCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalNotYetDueCalcEdit
			// 
			this.TotalNotYetDueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalNotYetDueCalcEdit, "TotalNotYetDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).TotalNotYetDue)));
			this.TotalNotYetDueCalcEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TotalNotYetDueCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalNotYetDueCalcEdit, false);
			this.TotalNotYetDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 79, true);
			this.TotalNotYetDueCalcEdit.Name = "TotalNotYetDueCalcEdit";
			this.TotalNotYetDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.TotalNotYetDueCalcEdit.TabIndex = 25;
			this.TotalNotYetDueCalcEdit.Text = "0.00";
			this.TotalNotYetDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrandTotalLabel
			// 
			this.GrandTotalLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GrandTotalLabel, "TotalLabelText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CollectionNotesTransactionsFilter)(null)).TotalLabelText)));
			this.GrandTotalLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|a1320116-27a6-4d36-85e7-74adead36271", "Total");
			this.GrandTotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 81, true);
			this.GrandTotalLabel.Name = "GrandTotalLabel";
			this.GrandTotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 13, true);
			this.GrandTotalLabel.TabIndex = 24;
			// 
			// DisbursementOutstandingCalcEdit
			// 
			this.DisbursementOutstandingCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DisbursementOutstandingCalcEdit, "DisbursementOutstanding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).DisbursementOutstanding)));
			this.DisbursementOutstandingCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DisbursementOutstandingCalcEdit, false);
			this.DisbursementOutstandingCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(620, 60, true);
			this.DisbursementOutstandingCalcEdit.Name = "DisbursementOutstandingCalcEdit";
			this.DisbursementOutstandingCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.DisbursementOutstandingCalcEdit.TabIndex = 23;
			this.DisbursementOutstandingCalcEdit.Text = "0.00";
			this.DisbursementOutstandingCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DisbursementOverTwoTermsPastDueCalcEdit
			// 
			this.DisbursementOverTwoTermsPastDueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DisbursementOverTwoTermsPastDueCalcEdit, "DisbursementOverTwoTermsPastDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).DisbursementOverTwoTermsPastDue)));
			this.DisbursementOverTwoTermsPastDueCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DisbursementOverTwoTermsPastDueCalcEdit, false);
			this.DisbursementOverTwoTermsPastDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(501, 60, true);
			this.DisbursementOverTwoTermsPastDueCalcEdit.Name = "DisbursementOverTwoTermsPastDueCalcEdit";
			this.DisbursementOverTwoTermsPastDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.DisbursementOverTwoTermsPastDueCalcEdit.TabIndex = 22;
			this.DisbursementOverTwoTermsPastDueCalcEdit.Text = "0.00";
			this.DisbursementOverTwoTermsPastDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DisbursementTwoTermsPastDueCalcEdit
			// 
			this.DisbursementTwoTermsPastDueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DisbursementTwoTermsPastDueCalcEdit, "DisbursementTwoTermsPastDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).DisbursementTwoTermsPastDue)));
			this.DisbursementTwoTermsPastDueCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DisbursementTwoTermsPastDueCalcEdit, false);
			this.DisbursementTwoTermsPastDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(401, 60, true);
			this.DisbursementTwoTermsPastDueCalcEdit.Name = "DisbursementTwoTermsPastDueCalcEdit";
			this.DisbursementTwoTermsPastDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.DisbursementTwoTermsPastDueCalcEdit.TabIndex = 21;
			this.DisbursementTwoTermsPastDueCalcEdit.Text = "0.00";
			this.DisbursementTwoTermsPastDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DisbursementOneTermPastDueCalcEdit
			// 
			this.DisbursementOneTermPastDueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DisbursementOneTermPastDueCalcEdit, "DisbursementOneTermPastDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).DisbursementOneTermPastDue)));
			this.DisbursementOneTermPastDueCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DisbursementOneTermPastDueCalcEdit, false);
			this.DisbursementOneTermPastDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 60, true);
			this.DisbursementOneTermPastDueCalcEdit.Name = "DisbursementOneTermPastDueCalcEdit";
			this.DisbursementOneTermPastDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.DisbursementOneTermPastDueCalcEdit.TabIndex = 20;
			this.DisbursementOneTermPastDueCalcEdit.Text = "0.00";
			this.DisbursementOneTermPastDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DisbursementDueTodayCalcEdit
			// 
			this.DisbursementDueTodayCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DisbursementDueTodayCalcEdit, "DisbursementDueToday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).DisbursementDueToday)));
			this.DisbursementDueTodayCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DisbursementDueTodayCalcEdit, false);
			this.DisbursementDueTodayCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 60, true);
			this.DisbursementDueTodayCalcEdit.Name = "DisbursementDueTodayCalcEdit";
			this.DisbursementDueTodayCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.DisbursementDueTodayCalcEdit.TabIndex = 19;
			this.DisbursementDueTodayCalcEdit.Text = "0.00";
			this.DisbursementDueTodayCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DisbursementNotYetDueCalcEdit
			// 
			this.DisbursementNotYetDueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DisbursementNotYetDueCalcEdit, "DisbursementNotYetDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).DisbursementNotYetDue)));
			this.DisbursementNotYetDueCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DisbursementNotYetDueCalcEdit, false);
			this.DisbursementNotYetDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 60, true);
			this.DisbursementNotYetDueCalcEdit.Name = "DisbursementNotYetDueCalcEdit";
			this.DisbursementNotYetDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.DisbursementNotYetDueCalcEdit.TabIndex = 18;
			this.DisbursementNotYetDueCalcEdit.Text = "0.00";
			this.DisbursementNotYetDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StandardOutstandingCalcEdit
			// 
			this.StandardOutstandingCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.StandardOutstandingCalcEdit, "StandardOutstanding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).StandardOutstanding)));
			this.StandardOutstandingCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StandardOutstandingCalcEdit, false);
			this.StandardOutstandingCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(620, 41, true);
			this.StandardOutstandingCalcEdit.Name = "StandardOutstandingCalcEdit";
			this.StandardOutstandingCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.StandardOutstandingCalcEdit.TabIndex = 15;
			this.StandardOutstandingCalcEdit.Text = "0.00";
			this.StandardOutstandingCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StandardOverTwoTermsPastDueCalcEdit
			// 
			this.StandardOverTwoTermsPastDueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.StandardOverTwoTermsPastDueCalcEdit, "StandardOverTwoTermsPastDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).StandardOverTwoTermsPastDue)));
			this.StandardOverTwoTermsPastDueCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StandardOverTwoTermsPastDueCalcEdit, false);
			this.StandardOverTwoTermsPastDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(501, 41, true);
			this.StandardOverTwoTermsPastDueCalcEdit.Name = "StandardOverTwoTermsPastDueCalcEdit";
			this.StandardOverTwoTermsPastDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.StandardOverTwoTermsPastDueCalcEdit.TabIndex = 14;
			this.StandardOverTwoTermsPastDueCalcEdit.Text = "0.00";
			this.StandardOverTwoTermsPastDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StandardTwoTermsPastDueCalcEdit
			// 
			this.StandardTwoTermsPastDueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.StandardTwoTermsPastDueCalcEdit, "StandardTwoTermsPastDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).StandardTwoTermsPastDue)));
			this.StandardTwoTermsPastDueCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StandardTwoTermsPastDueCalcEdit, false);
			this.StandardTwoTermsPastDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(401, 41, true);
			this.StandardTwoTermsPastDueCalcEdit.Name = "StandardTwoTermsPastDueCalcEdit";
			this.StandardTwoTermsPastDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.StandardTwoTermsPastDueCalcEdit.TabIndex = 13;
			this.StandardTwoTermsPastDueCalcEdit.Text = "0.00";
			this.StandardTwoTermsPastDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StandardOneTermPastDueCalcEdit
			// 
			this.StandardOneTermPastDueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.StandardOneTermPastDueCalcEdit, "StandardOneTermPastDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).StandardOneTermPastDue)));
			this.StandardOneTermPastDueCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StandardOneTermPastDueCalcEdit, false);
			this.StandardOneTermPastDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 41, true);
			this.StandardOneTermPastDueCalcEdit.Name = "StandardOneTermPastDueCalcEdit";
			this.StandardOneTermPastDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.StandardOneTermPastDueCalcEdit.TabIndex = 12;
			this.StandardOneTermPastDueCalcEdit.Text = "0.00";
			this.StandardOneTermPastDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StandardDueTodayCalcEdit
			// 
			this.StandardDueTodayCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.StandardDueTodayCalcEdit, "StandardDueToday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).StandardDueToday)));
			this.StandardDueTodayCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StandardDueTodayCalcEdit, false);
			this.StandardDueTodayCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 41, true);
			this.StandardDueTodayCalcEdit.Name = "StandardDueTodayCalcEdit";
			this.StandardDueTodayCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.StandardDueTodayCalcEdit.TabIndex = 11;
			this.StandardDueTodayCalcEdit.Text = "0.00";
			this.StandardDueTodayCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StandardNotYetDueCalcEdit
			// 
			this.StandardNotYetDueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.StandardNotYetDueCalcEdit, "StandardNotYetDue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CollectionNotesTransactionsFilter)(null)).StandardNotYetDue)));
			this.StandardNotYetDueCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StandardNotYetDueCalcEdit, false);
			this.StandardNotYetDueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 41, true);
			this.StandardNotYetDueCalcEdit.Name = "StandardNotYetDueCalcEdit";
			this.StandardNotYetDueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.StandardNotYetDueCalcEdit.TabIndex = 10;
			this.StandardNotYetDueCalcEdit.Text = "0.00";
			this.StandardNotYetDueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TransactionsPastDueHeadingLabel
			// 
			this.TransactionsPastDueHeadingLabel.AutoSize = true;
			this.TransactionsPastDueHeadingLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|543f5380-5c94-45f3-a071-4ea2aa49bda5", "Transactions Past Due Aged by Credit Terms");
			this.TransactionsPastDueHeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 6, true);
			this.TransactionsPastDueHeadingLabel.Name = "TransactionsPastDueHeadingLabel";
			this.TransactionsPastDueHeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 13, true);
			this.TransactionsPastDueHeadingLabel.TabIndex = 0;
			// 
			// TermsHeadingLabelLabel
			// 
			this.TermsHeadingLabelLabel.AutoSize = true;
			this.TermsHeadingLabelLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|6d5bdf99-293a-4d2f-ab09-af117b291da8", "Terms");
			this.TermsHeadingLabelLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 24, true);
			this.TermsHeadingLabelLabel.Name = "TermsHeadingLabelLabel";
			this.TermsHeadingLabelLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 13, true);
			this.TermsHeadingLabelLabel.TabIndex = 1;
			// 
			// TotalOutstandingHeadingLabel
			// 
			this.TotalOutstandingHeadingLabel.AutoSize = true;
			this.TotalOutstandingHeadingLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|4fdde9b9-14b6-45c0-8bfa-53559844338c", "Total Outstanding");
			this.TotalOutstandingHeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(625, 24, true);
			this.TotalOutstandingHeadingLabel.Name = "TotalOutstandingHeadingLabel";
			this.TotalOutstandingHeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 13, true);
			this.TotalOutstandingHeadingLabel.TabIndex = 7;
			// 
			// OverThreeTermsHeadingLabel
			// 
			this.OverThreeTermsHeadingLabel.AutoSize = true;
			this.OverThreeTermsHeadingLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|a5d5362e-3731-4ef2-a934-9b129ffa0acb", "> 2 Terms");
			this.OverThreeTermsHeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(521, 24, true);
			this.OverThreeTermsHeadingLabel.Name = "OverThreeTermsHeadingLabel";
			this.OverThreeTermsHeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 13, true);
			this.OverThreeTermsHeadingLabel.TabIndex = 6;
			// 
			// ThreeTermsHeadingLabel
			// 
			this.ThreeTermsHeadingLabel.AutoSize = true;
			this.ThreeTermsHeadingLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|9dfb14fc-ce14-4773-a5cb-ae15ec846a6d", "+ 2 Terms");
			this.ThreeTermsHeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 24, true);
			this.ThreeTermsHeadingLabel.Name = "ThreeTermsHeadingLabel";
			this.ThreeTermsHeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 13, true);
			this.ThreeTermsHeadingLabel.TabIndex = 5;
			// 
			// TwoTermsHeadingLabel
			// 
			this.TwoTermsHeadingLabel.AutoSize = true;
			this.TwoTermsHeadingLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|8e8fab76-3a9c-4d48-8b0a-f01644beab18", "+1 Term");
			this.TwoTermsHeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 24, true);
			this.TwoTermsHeadingLabel.Name = "TwoTermsHeadingLabel";
			this.TwoTermsHeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 13, true);
			this.TwoTermsHeadingLabel.TabIndex = 4;
			// 
			// DueTodayHeadingLabel
			// 
			this.DueTodayHeadingLabel.AutoSize = true;
			this.DueTodayHeadingLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|4901338d-f99b-4c2d-9440-794892ba972a", "Due Today");
			this.DueTodayHeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 24, true);
			this.DueTodayHeadingLabel.Name = "DueTodayHeadingLabel";
			this.DueTodayHeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.DueTodayHeadingLabel.TabIndex = 3;
			// 
			// NotYetDueHeadingLabel
			// 
			this.NotYetDueHeadingLabel.AutoSize = true;
			this.NotYetDueHeadingLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionCallsTransactionsPrintingControl|2937b3cd-0f88-4f5c-9059-2cda3a68d763", "Not Yet Due");
			this.NotYetDueHeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 24, true);
			this.NotYetDueHeadingLabel.Name = "NotYetDueHeadingLabel";
			this.NotYetDueHeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 13, true);
			this.NotYetDueHeadingLabel.TabIndex = 2;
			// 
			//termsPanel
			//
			this.termsPanel.Name = "termsPanel";
			this.termsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.termsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 130, true);
			this.termsPanel.Dock = DockStyle.Fill;
			// 
			// statusTermTabControl
			// 
			this.statusTermTabControl.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.statusTermTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 233, true);
			this.statusTermTabControl.Name = "statusTermTabControl";
			this.statusTermTabControl.SelectedIndex = 0;
			this.statusTermTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 153, true);
			this.statusTermTabControl.TabPages.Add(this.agingTabPage);
			this.statusTermTabControl.TabPages.Add(this.termsTabPage);
			// 
			// agingTabPage
			// 
			this.agingTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("06689ce5-4284-4ad6-ae0a-9c132d4857a7", "Aging Analysis by Organization\'s Credit Terms");
			this.agingTabPage.Controls.Add(this.TotalPercentagePastDueLabel);
			this.agingTabPage.Controls.Add(this.TotalOutstandingLabel);
			this.agingTabPage.Controls.Add(this.TotalOverTwoTermsPastDueCalcEdit);
			this.agingTabPage.Controls.Add(this.TotalTwoTermsPastDueCalcEdit);
			this.agingTabPage.Controls.Add(this.TotalOneTermPastDueCalcEdit);
			this.agingTabPage.Controls.Add(this.TotalDueTodayCalcEdit);
			this.agingTabPage.Controls.Add(this.TotalNotYetDueCalcEdit);
			this.agingTabPage.Controls.Add(this.GrandTotalLabel);
			this.agingTabPage.Controls.Add(this.DisbursementOutstandingCalcEdit);
			this.agingTabPage.Controls.Add(this.DisbursementOverTwoTermsPastDueCalcEdit);
			this.agingTabPage.Controls.Add(this.DisbursementTwoTermsPastDueCalcEdit);
			this.agingTabPage.Controls.Add(this.DisbursementOneTermPastDueCalcEdit);
			this.agingTabPage.Controls.Add(this.DisbursementDueTodayCalcEdit);
			this.agingTabPage.Controls.Add(this.DisbursementNotYetDueCalcEdit);
			this.agingTabPage.Controls.Add(this.StandardOutstandingCalcEdit);
			this.agingTabPage.Controls.Add(this.StandardOverTwoTermsPastDueCalcEdit);
			this.agingTabPage.Controls.Add(this.StandardTwoTermsPastDueCalcEdit);
			this.agingTabPage.Controls.Add(this.StandardOneTermPastDueCalcEdit);
			this.agingTabPage.Controls.Add(this.StandardDueTodayCalcEdit);
			this.agingTabPage.Controls.Add(this.StandardNotYetDueCalcEdit);
			this.agingTabPage.Controls.Add(this.TransactionsPastDueHeadingLabel);
			this.agingTabPage.Controls.Add(this.TermsHeadingLabelLabel);
			this.agingTabPage.Controls.Add(this.TotalOutstandingHeadingLabel);
			this.agingTabPage.Controls.Add(this.OverThreeTermsHeadingLabel);
			this.agingTabPage.Controls.Add(this.ThreeTermsHeadingLabel);
			this.agingTabPage.Controls.Add(this.TwoTermsHeadingLabel);
			this.agingTabPage.Controls.Add(this.DueTodayHeadingLabel);
			this.agingTabPage.Controls.Add(this.NotYetDueHeadingLabel);
			this.agingTabPage.Controls.Add(this.TotalPastDueLabel);
			this.agingTabPage.Controls.Add(this.TotalPastDueHeadingLabel);
			this.agingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.agingTabPage.Name = "agingTabPage";
			this.agingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 131, true);
			// 
			// termsTabPage
			// 
			this.termsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f1b2c9a4-5287-47b4-a9fc-ec3846d1127d", "Invoice Terms");
			this.termsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.termsTabPage.Name = "termsTabPage";
			this.termsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 131, true);
			this.termsTabPage.TabIndex = 1;
			this.termsTabPage.Controls.Add(termsPanel);
			this.termsTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.termsTabPage_InitializeTab));
			// 
			// CollectionCallsTransactionsPrintingControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PrintStatementButton);
			this.Controls.Add(this.PrintInvoiceButton);
			this.Controls.Add(this.TransactionFiltersGroupBox);
			this.Controls.Add(this.InvoicesGrid);
			this.Controls.Add(this.statusTermTabControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(797, 392, true);
			this.Name = "CollectionCallsTransactionsPrintingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(797, 392, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).EndInit();
			this.InvoicesGrid.ResumeLayout(false);
			this.InvoicesGrid.PerformLayout();
			this.AH_RX_NKCodeFindBox.ResumeLayout(true);
			this.AH_RX_NKCodeFindBox.PerformLayout();
			this.AH_GEGuidFindBox.ResumeLayout(true);
			this.AH_GEGuidFindBox.PerformLayout();
			this.AH_GBGuidFindBox.ResumeLayout(true);
			this.AH_GBGuidFindBox.PerformLayout();
			this.PaymentStatusDropEdit.ResumeLayout(true);
			this.PaymentStatusDropEdit.PerformLayout();
			this.TransactionFiltersGroupBox.ResumeLayout(false);
			this.TransactionFiltersGroupBox.PerformLayout();
			this.AH_FromDateDateEdit.ResumeLayout(true);
			this.AH_FromDateDateEdit.PerformLayout();
			this.AH_ToDateDateEdit.ResumeLayout(true);
			this.AH_ToDateDateEdit.PerformLayout();
			this.DateFilterDropEdit.ResumeLayout(true);
			this.DateFilterDropEdit.PerformLayout();
			this.NumberFilterDropEdit.ResumeLayout(true);
			this.NumberFilterDropEdit.PerformLayout();
			this.TransactionTypeDropEdit.ResumeLayout(true);
			this.TransactionTypeDropEdit.PerformLayout();
			this.statusTermTabControl.ResumeLayout(false);
			this.statusTermTabControl.PerformLayout();
			this.agingTabPage.ResumeLayout(false);
			this.agingTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}