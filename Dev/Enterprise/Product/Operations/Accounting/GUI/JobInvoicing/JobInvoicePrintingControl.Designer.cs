using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.Base;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobInvoicePrintingControl
	{


		#region Component Designer generated code

		private ZPanel FilterPanel;
		private ZPanel GridPanel;
		private ZPanel PrintButtonPanel;
		internal ZGrid InvoicesGrid;
		private ZButton PrintButton;
		private ZButton ClearButton;
		private ZButton FindButton;
		private ZGuidFindBox DebtorFindBox;
		private ZGuidFindBox JobHeaderFindBox;
		private ZDropEdit TransactionTypeDropEdit;
		private System.ComponentModel.Container components = null;
		private ZLabel InvoiceHidingMessageLabel;

		private void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZDateEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo amendStatusCodeColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();

			this.PrintButton = new ZButton();
			this.ClearButton = new ZButton();
			this.FindButton = new ZButton();
			this.DebtorFindBox = new ZGuidFindBox();
			this.JobHeaderFindBox = new ZGuidFindBox();
			this.TransactionTypeDropEdit = new ZDropEdit();
			this.FilterPanel = new ZPanel();
			this.InvoiceHidingMessageLabel = new ZLabel();
			this.FetchButton = new ZButton();
			this.GridPanel = new ZPanel();
			this.InvoicesGrid = new ZGrid();
			this.PrintButtonPanel = new ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DebtorFindBox.SuspendLayout();
			this.JobHeaderFindBox.SuspendLayout();
			this.TransactionTypeDropEdit.SuspendLayout();
			this.FilterPanel.SuspendLayout();
			this.GridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).BeginInit();
			this.InvoicesGrid.SuspendLayout();
			this.PrintButtonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobARInvoicePrintingFilter);
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicePrintingControl|3ac75de1-f69f-4e8d-a92d-34e5b0ee02bf", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 9, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.PrintButton.TabIndex = 9;
			this.PrintButton.ToolTipCaption = null;
			this.PrintButton.Click += new EventHandler(this.PrintInvoices);
			// 
			// ClearButton
			// 
			this.ClearButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicePrintingControl|28b5b20a-6763-4057-a7fc-1862134dd0e9", "Clear");
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 59, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ClearButton.TabIndex = 7;
			this.ClearButton.ToolTipCaption = null;
			this.ClearButton.Click += new EventHandler(this.ClearButton_Click);
			// 
			// FindButton
			// 
			this.FindButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicePrintingControl|4d4c3bd0-cce5-4856-9c3b-4b1a87bfeb6b", "Find");
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 59, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.FindButton.TabIndex = 6;
			this.FindButton.ToolTipCaption = null;
			this.FindButton.Click += new EventHandler(this.FindButton_Click);
			// 
			// DebtorFindBox
			// 
			this.DebtorFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DebtorFindBox, "DebtorOrCreditor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((JobARInvoicePrintingFilter)(null)).DebtorOrCreditor)));
			this.DebtorFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicePrintingControl|114175c9-5308-4964-bbeb-1a489169a913", "Debtor");
			this.DebtorFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 11, true);
			this.DebtorFindBox.Name = "DebtorFindBox";
			this.DebtorFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DebtorFindBox.ParentType = null;
			this.DebtorFindBox.PopupCaption = null;
			this.DebtorFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 17, true);
			this.DebtorFindBox.TabIndex = 1;
			// 
			// JobHeaderFindBox
			// 
			this.JobHeaderFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JobHeaderFindBox, "JobNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((JobARInvoicePrintingFilter)(null)).JobNumber)));
			this.JobHeaderFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicePrintingControl|f2ca8b3f-8396-441d-b0f8-9aa1ffac0bb1", "Job Number");
			this.JobHeaderFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 59, true);
			this.JobHeaderFindBox.Name = "JobHeaderFindBox";
			this.JobHeaderFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JobHeaderFindBox.ParentType = null;
			this.JobHeaderFindBox.PopupCaption = null;
			this.JobHeaderFindBox.ShowDescriptionBox = false;
			this.JobHeaderFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.JobHeaderFindBox.TabIndex = 5;
			// 
			// TransactionTypeDropEdit
			// 
			this.TransactionTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransactionTypeDropEdit, "TransactionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobARInvoicePrintingFilter)(null)).TransactionType)));
			this.TransactionTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicePrintingControl|716f2fb5-0287-4bbc-ae26-6191e33fb39f", "Transaction Type");
			this.TransactionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 35, true);
			this.TransactionTypeDropEdit.Name = "TransactionTypeDropEdit";
			this.TransactionTypeDropEdit.PreBoundMaxLength = 3;
			this.TransactionTypeDropEdit.ShowDescriptionBox = false;
			this.TransactionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.TransactionTypeDropEdit.TabIndex = 3;
			// 
			// FilterPanel
			// 
			this.FilterPanel.Controls.Add(this.InvoiceHidingMessageLabel);
			this.FilterPanel.Controls.Add(this.FetchButton);
			this.FilterPanel.Controls.Add(this.JobHeaderFindBox);
			this.FilterPanel.Controls.Add(this.TransactionTypeDropEdit);
			this.FilterPanel.Controls.Add(this.ClearButton);
			this.FilterPanel.Controls.Add(this.DebtorFindBox);
			this.FilterPanel.Controls.Add(this.FindButton);
			this.FilterPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.FilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 112, true);
			this.FilterPanel.TabIndex = 10;
			// 
			// InvoiceHidingMessageLabel
			// 
			this.InvoiceHidingMessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b115724e-da61-402c-aff8-5d8b230901f6", "Invoice/credit notes with Job Header branch / dept outside your login permission are not listed.");
			this.InvoiceHidingMessageLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.InvoiceHidingMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 87, true);
			this.InvoiceHidingMessageLabel.Name = "InvoiceHidingMessageLabel";
			this.InvoiceHidingMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 15, true);
			this.InvoiceHidingMessageLabel.TabIndex = 0;
			this.InvoiceHidingMessageLabel.Visible = false;
			// 
			// FetchButton
			// 
			this.FetchButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicePrintingControl|c22f1a77-5eac-423b-9de1-7caf47f2d1df", "Get Outstanding Details");
			this.FetchButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 59, true);
			this.FetchButton.Name = "FetchButton";
			this.FetchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.FetchButton.TabIndex = 8;
			this.FetchButton.ToolTipCaption = null;
			this.FetchButton.Click += new EventHandler(this.FetchButton_Click);
			// 
			// GridPanel
			// 
			this.GridPanel.Controls.Add(this.InvoicesGrid);
			this.GridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
			this.GridPanel.Name = "GridPanel";
			this.GridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 416, true);
			this.GridPanel.TabIndex = 11;
			// 
			// InvoicesGrid
			// 
			this.InvoicesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InvoicesGrid, "FilteredTransactions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_ConsolidatedInvoiceRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).FullyPaidDateBindable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDate)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_ComplianceDocumentDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_OSTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).OutstandingAmountBindable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_TransactionReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_InvoiceTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_InvoiceTermDescriptionCalc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).PaymentStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_ComplianceSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).DisplayInvoiceAddressOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).DisplayInvoiceContactOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).EInvoicingBatchNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).EInvoicingLastResponseReceivedUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).EInvoicingLastSentTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).EInvoicingAuthorisationNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).EInvoicingError)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).EInvoicingeHubAllocatedNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).EInvoicingGovernmentAllocatedNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).EInvoicingStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((TransactionHeader)(((System.Collections.IList)(((JobARInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_GB_TaxBranch)));
			this.InvoicesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "AH_Ledger";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "AH_TransactionType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "AH_ConsolidatedInvoiceRef";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.Caption = "";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fa103fa2-c65d-44f7-a13f-d2877147e166", "Sell Reference");
			zTextBoxColumnStyleInfo5.ColumnName = "AH_ChequeOrReference";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicePrintingControl|2098891b-590f-44ce-a6a0-995c31695498", "Fully Paid Date");
			zDateEditColumnStyleInfo3.ColumnName = "FullyPaidDateBindable";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.ColumnName = "AH_ComplianceDocumentDate";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AH_RX_NKTransactionCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicePrintingControl|3388f97a-1fee-44b6-bc37-b40592c85f3d", "Invoice Amt");
			zCalcEditColumnStyleInfo1.ColumnName = "AH_OSTotal";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicePrintingControl|3e936f75-4cb6-4057-b7be-52dc061c9c07", "Outstanding Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "OutstandingAmountBindable";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("279E3E00-6BD5-4056-B46A-BF4C3CDB9AB8", "Compliance Number");
			zTextBoxColumnStyleInfo6.ColumnName = "AH_TransactionReference";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo7.ColumnName = "AH_InvoiceTerm";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicePrintingControl|3866ca73-48d7-43a6-9c68-52952701841f", "Invoice Term Description");
			zTextBoxColumnStyleInfo8.ColumnName = "AH_InvoiceTermDescriptionCalc";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicePrintingControl|ad1b6460-965e-4952-b397-188be74a5ae5", "Payment Status");
			zTextBoxColumnStyleInfo9.ColumnName = "PaymentStatus";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.ColumnName = "AH_ComplianceSubType";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("58b11c0d-e1ba-417d-9db4-9b973e4c0a3e", "Address");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "DisplayInvoiceAddressOverride";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("acf2d959-12ec-4fcd-a0a4-81da4d0be77d", "Contact");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "DisplayInvoiceContactOverride";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.ColumnName = "EInvoicingBatchNumber";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo5.ColumnName = "EInvoicingLastResponseReceivedUtc";
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo6.ColumnName = "EInvoicingLastSentTimeUtc";
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.ColumnName = "EInvoicingAuthorisationNumber";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.ColumnName = "EInvoicingError";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.ColumnName = "EInvoicingeHubAllocatedNumber";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.ColumnName = "EInvoicingGovernmentAllocatedNumber";
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.ColumnName = "EInvoicingStatus";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			amendStatusCodeColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("67FCC8DA-834B-40B9-B568-A393ACAE2AC8", "Amend Status Code");
			amendStatusCodeColumnStyleInfo.ColumnName = "AmendStatusCodeAndDescription";
			amendStatusCodeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "AH_GB";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.ColumnName = "AH_GB_TaxBranch";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.InvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.InvoicesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.InvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.InvoicesGrid.ColumnStyles.Add(amendStatusCodeColumnStyleInfo);
			this.InvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.InvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.InvoicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoicesGrid.GridId = "f5f4c71d-360b-495f-9e24-ab651a00b0d8";
			this.InvoicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoicesGrid.LayoutKey = "zGrid1";
			this.InvoicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoicesGrid.Name = "InvoicesGrid";
			this.InvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 416, true);
			this.InvoicesGrid.TabIndex = 8;
			// 
			// PrintButtonPanel
			// 
			this.PrintButtonPanel.Controls.Add(this.PrintButton);
			this.PrintButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PrintButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 528, true);
			this.PrintButtonPanel.Name = "PrintButtonPanel";
			this.PrintButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 40, true);
			this.PrintButtonPanel.TabIndex = 12;
			// 
			// JobInvoicePrintingControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GridPanel);
			this.Controls.Add(this.PrintButtonPanel);
			this.Controls.Add(this.FilterPanel);
			this.Name = "JobInvoicePrintingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 568, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DebtorFindBox.ResumeLayout(true);
			this.DebtorFindBox.PerformLayout();
			this.JobHeaderFindBox.ResumeLayout(true);
			this.JobHeaderFindBox.PerformLayout();
			this.TransactionTypeDropEdit.ResumeLayout(true);
			this.TransactionTypeDropEdit.PerformLayout();
			this.FilterPanel.ResumeLayout(false);
			this.FilterPanel.PerformLayout();
			this.GridPanel.ResumeLayout(false);
			this.GridPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).EndInit();
			this.InvoicesGrid.ResumeLayout(false);
			this.InvoicesGrid.PerformLayout();
			this.PrintButtonPanel.ResumeLayout(false);
			this.PrintButtonPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
