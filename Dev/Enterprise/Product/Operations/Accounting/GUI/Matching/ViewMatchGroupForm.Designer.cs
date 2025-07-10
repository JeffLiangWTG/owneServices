using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class ViewMatchGroupForm
	{
		#region Windows Form Designer generated code

		private ZGroupBox MatchedTransactionsGroupBox;
		private ZArchitecture.ZGrid TransactionsGrid;

		private readonly System.ComponentModel.Container components = null;

		new void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			this.MatchedTransactionsGroupBox = new ZGroupBox();
			this.TransactionsGrid = new ZArchitecture.ZGrid();
			this.TopPanel = new ZPanel();
			this.UnmatchDateEdit = new ZDateEdit();
			this.MatchDateDateEdit = new ZDateEdit();
			this.MatchGroupTextBox = new ZArchitecture.ZTextBox();
			this.BottomPanel = new ZPanel();
			this.oPostingButtonsUserControl1 = new ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MatchedTransactionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransactionsGrid)).BeginInit();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 229, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 23, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(256);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(257);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(UnmatchingRow);
			// 
			// MatchedTransactionsGroupBox
			// 
			this.MatchedTransactionsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|21b5ca42-8c0d-440e-8b40-ebb5608ecea3", "Matched Transactions");
			this.MatchedTransactionsGroupBox.Controls.Add(this.TransactionsGrid);
			this.MatchedTransactionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchedTransactionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 39, true);
			this.MatchedTransactionsGroupBox.Name = "MatchedTransactionsGroupBox";
			this.MatchedTransactionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 155, true);
			this.MatchedTransactionsGroupBox.TabIndex = 1;
			this.MatchedTransactionsGroupBox.TabStop = false;
			// 
			// TransactionsGrid
			// 
			this.TransactionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TransactionsGrid, "MatchedTransactions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_Calc_OSOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_OSTotalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_LocalExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_TransactionReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_ConsolidatedInvoiceRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_IsDisbursementCalc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_TransactionCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(((System.Collections.IList)(((UnmatchingRow)(null)).MatchedTransactions)).SyncRoot)).AH_Calc_LocalRXCode)));
			this.TransactionsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|e521cecd-a143-4d9d-b255-dd78e73434ad", "Trans. No.");
			zTextBoxColumnStyleInfo1.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo2.ColumnName = "AH_Ledger";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|865a6469-7954-4d7c-a225-5b4cc108c476", "Type");
			zTextBoxColumnStyleInfo3.ColumnName = "AH_TransactionType";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|ccdf375d-e6d6-4a37-a588-e51c289e21ce", "Organization");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|ffa7ae81-9075-4386-8375-e70dc998caa0", "Outstanding Amt");
			zCalcEditColumnStyleInfo1.ColumnName = "AH_Calc_OSOutstandingAmount";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|0db2eec7-2c02-4f15-8e4d-9d15438e1ae3", "Invoice Amt");
			zCalcEditColumnStyleInfo2.ColumnName = "AH_OSTotalAmount";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AH_RX_NKTransactionCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|d073fdba-c853-461b-90ca-8f2bda1803d3", "Local Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "AH_LocalExTaxAmount";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|0b268fcd-38a4-43c8-a5a2-1ee328ec463c", "Desc.", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "AH_Desc";
			zDateEditColumnStyleInfo1.ColumnName = "AH_DueDate";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|b830c5d3-1a01-4a0b-90b9-982cc105dadd", "Transaction Reference");
			zTextBoxColumnStyleInfo5.ColumnName = "AH_TransactionReference";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|a4321517-b505-4124-aeaa-8cee1a8e543c", "Check Or Reference");
			zTextBoxColumnStyleInfo6.ColumnName = "AH_ChequeOrReference";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|d7f38c25-8047-4284-8718-a060c96de43a", "Job Inv. Num.", "Job Invoice Number.");
			zTextBoxColumnStyleInfo7.ColumnName = "AH_ConsolidatedInvoiceRef";
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|c7ddc0fc-3239-47d3-ba20-45224632cf79", "Post Date");
			zDateEditColumnStyleInfo2.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AH_GB";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "AH_ExchangeRate";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|32f49938-9fcf-46b2-82f4-dcc3ffe96376", "Is Disb.", "Is Disbursement");
			zCheckBoxColumnStyleInfo1.ColumnName = "AH_IsDisbursementCalc";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo8.ColumnName = "AH_TransactionCategory";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|8765ce6f-8efd-433f-896e-9259a29c63c4", "Local Currency");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "AH_Calc_LocalRXCode";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TransactionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.TransactionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.TransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.TransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.TransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.TransactionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.TransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.TransactionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.TransactionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.TransactionsGrid.GridId = "5AA22881-5FC5-4CF7-8984-EBF03C120102";
			this.TransactionsGrid.CopySelectedRowsAllowed = true;
			this.TransactionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransactionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransactionsGrid.IsWholeRowSelectedOnClick = true;
			this.TransactionsGrid.LayoutKey = "TransactionsGrid";
			this.TransactionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TransactionsGrid.Name = "TransactionsGrid";
			this.TransactionsGrid.ReadOnly = true;
			this.TransactionsGrid.ShouldSetErrorsOnTabPage = false;
			this.TransactionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 136, true);
			this.TransactionsGrid.TabIndex = 0;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.UnmatchDateEdit);
			this.TopPanel.Controls.Add(this.MatchDateDateEdit);
			this.TopPanel.Controls.Add(this.MatchGroupTextBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 39, true);
			this.TopPanel.TabIndex = 0;
			// 
			// UnmatchDateEdit
			// 
			this.UnmatchDateEdit.AllowDrop = true;
			this.UnmatchDateEdit.AutoCompleteMonthThreshold = 1;
			this.UnmatchDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.UnmatchDateEdit, "UnmatchDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((UnmatchingRow)(null)).UnmatchDate)));
			this.UnmatchDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|77a21e15-4868-4046-9d00-d9d7c56704ef", "Unmatch Date");
			this.UnmatchDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 9, true);
			this.UnmatchDateEdit.Name = "UnmatchDateEdit";
			this.UnmatchDateEdit.TabIndex = 2;
			// 
			// MatchDateDateEdit
			// 
			this.MatchDateDateEdit.AllowDrop = true;
			this.MatchDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.MatchDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.MatchDateDateEdit, "MatchDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((UnmatchingRow)(null)).MatchDate)));
			this.MatchDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|ff5c148b-71b2-4ba9-8b08-3afb38e67f4f", "Match Date");
			this.MatchDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 9, true);
			this.MatchDateDateEdit.Name = "MatchDateDateEdit";
			this.MatchDateDateEdit.TabIndex = 1;
			// 
			// MatchGroupTextBox
			// 
			this.BindingSource.SetBindingMember(this.MatchGroupTextBox, "MatchGroupNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((UnmatchingRow)(null)).MatchGroupNum)));
			this.MatchGroupTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|89062d5e-4b0f-46b8-b0d7-88fccf3e2881", "Match Group");
			this.MatchGroupTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 9, true);
			this.MatchGroupTextBox.Name = "MatchGroupTextBox";
			this.MatchGroupTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MatchGroupTextBox.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.oPostingButtonsUserControl1);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 194, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 35, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// oPostingButtonsUserControl1
			// 
			this.oPostingButtonsUserControl1.AllowDrop = true;
			this.oPostingButtonsUserControl1.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oPostingButtonsUserControl1.AutoSize = true;
			this.oPostingButtonsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(321, 5, true);
			this.oPostingButtonsUserControl1.Name = "oPostingButtonsUserControl1";
			this.oPostingButtonsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 27, true);
			this.oPostingButtonsUserControl1.TabIndex = 0;
			// 
			// ViewMatchGroupForm
			// 
			this.AutoAddPreviousNextButtons = false;

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 252, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ViewMatchGroupForm|6e6a2904-403b-422f-bcfd-4c6be2a73fba", "Match Group");
			this.Controls.Add(this.MatchedTransactionsGroupBox);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.TopPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(UnmatchingRow);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.Base.Unmatching.UnmatchingRow";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 290, true);
			this.Name = "ViewMatchGroupForm";
			this.RememberFormSize = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MatchedTransactionsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MatchedTransactionsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TransactionsGrid)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

	}
}
