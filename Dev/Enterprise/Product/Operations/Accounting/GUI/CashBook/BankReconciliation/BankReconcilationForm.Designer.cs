using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.BankStatement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.CashBook.BankReconciliation
{
	public partial class BankReconcilationForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.Container components = null;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			this.BankGuidFindBox = new ZGuidFindBox();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.BankReconGrid = new ZGrid();
			this.StatementDateDateEdit = new ZDateEdit();
			this.CashBookAmountCalcEdit = new ZCalcEdit();
			this.ClosingBalanceCalcEdit = new ZCalcEdit();
			this.UnclearedCashbookAmountCalcEdit = new ZCalcEdit();
			this.AutoReconcileButton = new ZButton();
			this.CashBookTotalCalcEdit = new ZCalcEdit();
			this.StatementTotalCalcEdit = new ZCalcEdit();
			this.TotalDifferenceCalcEdit = new ZCalcEdit();
			this.CurrentDebitTotalCalcEdit = new ZCalcEdit();
			this.CurrentCreditTotalCalcEdit = new ZCalcEdit();
			this.zCalcEdit1 = new ZCalcEdit();
			this.BankTransactionButton = new ZButton();
			this.ReconcileDateDateEdit = new ZDateEdit();
			this.zGroupBox1 = new ZGroupBox();
			this.BankStatementButton = new ZButton();
			this.IgnoreRefCheckBox = new ZCheckBox();
			this.zGroupBox2 = new ZGroupBox();
			this.AmendedBankStatementBalanceCalcEdit = new ZCalcEdit();
			this.UnclearedStatementAmountCalcEdit = new ZCalcEdit();
			this.ClosingBalanceCalcReadOnlyEdit = new ZCalcEdit();
			this.FindFilterButton = new ZButton();
			this.ClearFilterButton = new ZButton();
			this.TextSearchFilterTextBox = new ZTextBox();
			this.FromDateFilterDateEdit = new ZDateEdit();
			this.ToDateFilterDateEdit = new ZDateEdit();
			this.zCheckBox1 = new ZCheckBox();
			this.DateFilterDropEdit = new ZDropEdit();
			this.FilterGroupBox = new ZGroupBox();
			this.AmountFilterCalcEdit = new ZCalcEdit();
			this.MethodFilterDropEdit = new ZDropEdit();
			this.TypeFilterDropEdit = new ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BankReconGrid)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.FilterGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 476, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 24, true);
			this.MainStatusBar.TabIndex = 13;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(848);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.CashBook.BankReconciliation);
			// 
			// BankGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.BankGuidFindBox, "BankAccountPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Business.CashBook.BankReconciliation)(null)).BankAccountPK)));
			this.BankGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|45b55fe0-a0fb-4455-b6ac-5c6d5a9d5b84", "Bank");
			this.BankGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 23, true);
			this.BankGuidFindBox.Name = "BankGuidFindBox";
			this.BankGuidFindBox.PopupCaption = null;
			this.BankGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.BankGuidFindBox.TabIndex = 0;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(616, 449, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 12;
			// 
			// BankReconGrid
			// 
			this.BankReconGrid.AllowNavigation = false;
			this.BankReconGrid.DisableImportDataMenuItem = true;
			this.BankReconGrid.IsColourGridFactoryStandAlone = true;
			this.BankReconGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BankReconGrid, "MergedTransactions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.CashBook.BankReconciliation)(null)).MergedTransactions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IBankReconMergedTransaction)(((System.Collections.IList)(((Business.CashBook.BankReconciliation)(null)).MergedTransactions)).SyncRoot)).TransactionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IBankReconMergedTransaction)(((System.Collections.IList)(((Business.CashBook.BankReconciliation)(null)).MergedTransactions)).SyncRoot)).InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IBankReconMergedTransaction)(((System.Collections.IList)(((Business.CashBook.BankReconciliation)(null)).MergedTransactions)).SyncRoot)).Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IBankReconMergedTransaction)(((System.Collections.IList)(((Business.CashBook.BankReconciliation)(null)).MergedTransactions)).SyncRoot)).Method)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IBankReconMergedTransaction)(((System.Collections.IList)(((Business.CashBook.BankReconciliation)(null)).MergedTransactions)).SyncRoot)).Payee)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IBankReconMergedTransaction)(((System.Collections.IList)(((Business.CashBook.BankReconciliation)(null)).MergedTransactions)).SyncRoot)).ChequeRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZInt)(((IBankReconMergedTransaction)(((System.Collections.IList)(((Business.CashBook.BankReconciliation)(null)).MergedTransactions)).SyncRoot)).BankCurrencyDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IBankReconMergedTransaction)(((System.Collections.IList)(((Business.CashBook.BankReconciliation)(null)).MergedTransactions)).SyncRoot)).Debit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IBankReconMergedTransaction)(((System.Collections.IList)(((Business.CashBook.BankReconciliation)(null)).MergedTransactions)).SyncRoot)).Credit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((IBankReconMergedTransaction)(((System.Collections.IList)(((Business.CashBook.BankReconciliation)(null)).MergedTransactions)).SyncRoot)).IsCleared)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IBankReconMergedTransaction)(((System.Collections.IList)(((Business.CashBook.BankReconciliation)(null)).MergedTransactions)).SyncRoot)).LineType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IBankReconMergedTransaction)(((System.Collections.IList)(((Business.CashBook.BankReconciliation)(null)).MergedTransactions)).SyncRoot)).ClearedDate)));
			this.BankReconGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|188a5e49-57a7-4690-aa9a-c2baf1ef358c", "Post Date");
			zDateEditColumnStyleInfo1.ColumnName = "TransactionDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|503f57c3-b6c8-4aed-b318-6f4f35066835", "Trn. Date", "Transaction Date.");
			zDateEditColumnStyleInfo2.ColumnName = "InvoiceDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|7d029d41-1430-4560-a049-1f61702139aa", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "Type";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|6730e611-d7d4-4369-90e0-6c3d472d4567", "Method");
			zTextBoxColumnStyleInfo2.ColumnName = "Method";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|c7adedb5-a982-4275-abbe-14bac36f8f6a", "Payee");
			zTextBoxColumnStyleInfo3.ColumnName = "Payee";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|5feaf850-f8f6-4d33-952d-d0447b27a8c9", "Chq./Ref.", "Cheque/Ref.");
			zTextBoxColumnStyleInfo4.ColumnName = "ChequeRef";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "BankCurrencyDecimals";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|f81bc7ea-e832-4770-8c6d-9b0eefef74cb", "Debit/Stmt. Credit", "Debit/Statement Credit.");
			zCalcEditColumnStyleInfo1.ColumnName = "Debit";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = "BankCurrencyDecimals";
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|8cfb5c47-0768-4a02-aa48-aa043c25279c", "Credit/Stmt. Debit", "Credit/Statement Debit.");
			zCalcEditColumnStyleInfo2.ColumnName = "Credit";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|119d4f47-8153-4b3e-920a-1c4ef4a4e983", "Ticked");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsCleared";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|c2c7d205-9ef2-4149-93d5-436f04210976", "Source");
			zTextBoxColumnStyleInfo5.ColumnName = "LineType";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|4ce46369-abf2-4f7b-b245-70aff0ecc2ee", "Cleared Date");
			zDateEditColumnStyleInfo3.ColumnName = "ClearedDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.BankReconGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.BankReconGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.BankReconGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BankReconGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.BankReconGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.BankReconGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.BankReconGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.BankReconGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.BankReconGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.BankReconGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.BankReconGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.BankReconGrid.GridId = "cbf789f5-a316-44cf-a46d-1577f46e6242";
			this.BankReconGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BankReconGrid.LayoutKey = "BankReconGrid";
			this.BankReconGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 281, true);
			this.BankReconGrid.Name = "BankReconGrid";
			this.BankReconGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.BankReconGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 91, true);
			this.BankReconGrid.TabIndex = 3;
			// 
			// StatementDateDateEdit
			// 
			this.StatementDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.StatementDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StatementDateDateEdit, "StatementDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.CashBook.BankReconciliation)(null)).StatementDate)));
			this.StatementDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|9b5bb8df-7311-4ed9-957c-b4c4a03525ce", "Bank Statement Date");
			this.StatementDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 75, true);
			this.StatementDateDateEdit.Name = "StatementDateDateEdit";
			this.StatementDateDateEdit.TabIndex = 2;
			// 
			// CashBookAmountCalcEdit
			// 
			this.CashBookAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CashBookAmountCalcEdit, "CashBookBalance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.CashBook.BankReconciliation)(null)).CashBookBalance)));
			this.CashBookAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|a76bdea0-9392-4758-9251-049fc5db633b", "Cashbook Amount");

			this.CashBookAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 126, true);
			this.CashBookAmountCalcEdit.Name = "CashBookAmountCalcEdit";
			this.CashBookAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.CashBookAmountCalcEdit.TabIndex = 4;
			this.CashBookAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ClosingBalanceCalcEdit
			// 
			this.ClosingBalanceCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ClosingBalanceCalcEdit, "ClosingBalance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.CashBook.BankReconciliation)(null)).ClosingBalance)));
			this.ClosingBalanceCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|23190709-88dd-4c0b-af4f-88fdffe7dea8", "Statement Closing Bal.", "Statement Closing Balance");

			this.ClosingBalanceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 101, true);
			this.ClosingBalanceCalcEdit.Name = "ClosingBalanceCalcEdit";
			this.ClosingBalanceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.ClosingBalanceCalcEdit.TabIndex = 4;
			this.ClosingBalanceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UnclearedCashbookAmountCalcEdit
			// 
			this.UnclearedCashbookAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.UnclearedCashbookAmountCalcEdit, "UnclearedCashbookAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.CashBook.BankReconciliation)(null)).UnclearedCashbookAmount)));
			this.UnclearedCashbookAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|4e168c46-1506-450c-b8ba-837adbd16a3c", "Uncleared Cashbook Amount");
			this.UnclearedCashbookAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 47, true);
			this.UnclearedCashbookAmountCalcEdit.Name = "UnclearedCashbookAmountCalcEdit";
			this.UnclearedCashbookAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.UnclearedCashbookAmountCalcEdit.TabIndex = 1;
			this.UnclearedCashbookAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AutoReconcileButton
			// 
			this.AutoReconcileButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|7baaf60a-edd4-407b-9726-4f1f628e7fa9", "Auto &Reconcile");
			this.AutoReconcileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 150, true);
			this.AutoReconcileButton.Name = "AutoReconcileButton";
			this.AutoReconcileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 27, true);
			this.AutoReconcileButton.TabIndex = 7;
			this.AutoReconcileButton.Click += new EventHandler(this.AutoReconcileButton_Click);
			// 
			// CashBookTotalCalcEdit
			// 
			this.CashBookTotalCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CashBookTotalCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CashBookTotalCalcEdit, "CashbookTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.CashBook.BankReconciliation)(null)).CashbookTotal)));
			this.CashBookTotalCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|1bfb1579-276e-4590-85b1-41c77cfb9fd2", "Cashbook Total");

			this.CashBookTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 413, true);
			this.CashBookTotalCalcEdit.Name = "CashBookTotalCalcEdit";
			this.CashBookTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CashBookTotalCalcEdit.TabIndex = 6;
			this.CashBookTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StatementTotalCalcEdit
			// 
			this.StatementTotalCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.StatementTotalCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.StatementTotalCalcEdit, "StatementTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.CashBook.BankReconciliation)(null)).StatementTotal)));
			this.StatementTotalCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|eba1485e-6633-4b6e-aae0-4b92af93987c", "Ticked Statement Amount");

			this.StatementTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 413, true);
			this.StatementTotalCalcEdit.Name = "StatementTotalCalcEdit";
			this.StatementTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.StatementTotalCalcEdit.TabIndex = 8;
			this.StatementTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalDifferenceCalcEdit
			// 
			this.TotalDifferenceCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.TotalDifferenceCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalDifferenceCalcEdit, "TotalDifference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.CashBook.BankReconciliation)(null)).TotalDifference)));
			this.TotalDifferenceCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|bba72d93-e13d-484f-886f-8ca52a83adb8", "Ticked Difference");

			this.TotalDifferenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(616, 413, true);
			this.TotalDifferenceCalcEdit.Name = "TotalDifferenceCalcEdit";
			this.TotalDifferenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalDifferenceCalcEdit.TabIndex = 10;
			this.TotalDifferenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CurrentDebitTotalCalcEdit
			// 
			this.CurrentDebitTotalCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CurrentDebitTotalCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CurrentDebitTotalCalcEdit, "CurrentDebitTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.CashBook.BankReconciliation)(null)).CurrentDebitTotal)));
			this.CurrentDebitTotalCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|0c938639-b158-4bd9-b0a5-b320ad8b8f95", "Current Debit Total");

			this.CurrentDebitTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 381, true);
			this.CurrentDebitTotalCalcEdit.Name = "CurrentDebitTotalCalcEdit";
			this.CurrentDebitTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CurrentDebitTotalCalcEdit.TabIndex = 4;
			this.CurrentDebitTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CurrentCreditTotalCalcEdit
			// 
			this.CurrentCreditTotalCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CurrentCreditTotalCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CurrentCreditTotalCalcEdit, "CurrentCreditTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.CashBook.BankReconciliation)(null)).CurrentCreditTotal)));
			this.CurrentCreditTotalCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|1bdcf5f6-1d46-4c53-bbf9-69378bfe591e", "Current Credit Total", "Current Credit Total", "");

			this.CurrentCreditTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 381, true);
			this.CurrentCreditTotalCalcEdit.Name = "CurrentCreditTotalCalcEdit";
			this.CurrentCreditTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CurrentCreditTotalCalcEdit.TabIndex = 5;
			this.CurrentCreditTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit1
			// 
			this.zCalcEdit1.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "ReconError");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.CashBook.BankReconciliation)(null)).ReconError)));
			this.zCalcEdit1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|78ed0794-9d2e-4500-8080-2acc87f10f32", "Reconciliation Error");

			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 152, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.zCalcEdit1.TabIndex = 5;
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BankTransactionButton
			// 
			this.BankTransactionButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BankTransactionButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|b104f64f-7fab-4dfb-9a8d-577c330c1288", "Bank &Transactions");
			this.BankTransactionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 449, true);
			this.BankTransactionButton.Name = "BankTransactionButton";
			this.BankTransactionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.BankTransactionButton.TabIndex = 11;
			this.BankTransactionButton.UseVisualStyleBackColor = true;
			this.BankTransactionButton.Click += new EventHandler(this.BankTransactionButton_Click);
			// 
			// ReconcileDateDateEdit
			// 
			this.ReconcileDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReconcileDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReconcileDateDateEdit, "ReconcileDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.CashBook.BankReconciliation)(null)).ReconcileDate)));
			this.ReconcileDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|05c0a2aa-811b-4713-8dc8-0a185fc14dae", "GL Post Date");
			this.ReconcileDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 49, true);
			this.ReconcileDateDateEdit.Name = "ReconcileDateDateEdit";
			this.ReconcileDateDateEdit.TabIndex = 1;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|5318b84e-4781-4cac-be1b-a2c7b590c417", "Select Account and nominate Dates for Reconciliation");
			this.zGroupBox1.Controls.Add(this.BankStatementButton);
			this.zGroupBox1.Controls.Add(this.IgnoreRefCheckBox);
			this.zGroupBox1.Controls.Add(this.BankGuidFindBox);
			this.zGroupBox1.Controls.Add(this.AutoReconcileButton);
			this.zGroupBox1.Controls.Add(this.ReconcileDateDateEdit);
			this.zGroupBox1.Controls.Add(this.StatementDateDateEdit);
			this.zGroupBox1.Controls.Add(this.ClosingBalanceCalcEdit);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 188, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// BankStatementButton
			// 
			this.BankStatementButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|4044f407-6798-474a-bf8d-fcda6b0ed08d", "Enter &Bank Statement");
			this.BankStatementButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 100, true);
			this.BankStatementButton.Name = "BankStatementButton";
			this.BankStatementButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 26, true);
			this.BankStatementButton.TabIndex = 5;
			this.BankStatementButton.Click += new EventHandler(this.BankStatementButton_Click);
			// 
			// IgnoreRefCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IgnoreRefCheckBox, "ClearedFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Business.CashBook.BankReconciliation)(null)).ClearedFilter)));
			this.IgnoreRefCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|21ac55c7-1caa-4d53-a068-93d7c565141c", "Include Cleared Transactions");
			this.IgnoreRefCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IgnoreRefCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.IgnoreRefCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 131, true);
			this.IgnoreRefCheckBox.Name = "IgnoreRefCheckBox";
			this.IgnoreRefCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 46, true);
			this.IgnoreRefCheckBox.TabIndex = 6;
			this.IgnoreRefCheckBox.Visible = false;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|d86dafd6-c97a-41cc-89c0-2068ccbfb6fb", "Reconciliation Summary");
			this.zGroupBox2.Controls.Add(this.AmendedBankStatementBalanceCalcEdit);
			this.zGroupBox2.Controls.Add(this.UnclearedStatementAmountCalcEdit);
			this.zGroupBox2.Controls.Add(this.ClosingBalanceCalcReadOnlyEdit);
			this.zGroupBox2.Controls.Add(this.CashBookAmountCalcEdit);
			this.zGroupBox2.Controls.Add(this.zCalcEdit1);
			this.zGroupBox2.Controls.Add(this.UnclearedCashbookAmountCalcEdit);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 4, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 188, true);
			this.zGroupBox2.TabIndex = 1;
			this.zGroupBox2.TabStop = false;
			// 
			// AmendedBankStatementBalanceCalcEdit
			// 
			this.AmendedBankStatementBalanceCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AmendedBankStatementBalanceCalcEdit, "AmendedBankStatementBalance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.CashBook.BankReconciliation)(null)).AmendedBankStatementBalance)));
			this.AmendedBankStatementBalanceCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|444b5d18-4b5b-462b-b000-beff2daee12c", "Amended Bank Statement Balance");
			this.AmendedBankStatementBalanceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 99, true);
			this.AmendedBankStatementBalanceCalcEdit.Name = "AmendedBankStatementBalanceCalcEdit";
			this.AmendedBankStatementBalanceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.AmendedBankStatementBalanceCalcEdit.TabIndex = 3;
			this.AmendedBankStatementBalanceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UnclearedStatementAmountCalcEdit
			// 
			this.UnclearedStatementAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.UnclearedStatementAmountCalcEdit, "UnclearedStatementAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.CashBook.BankReconciliation)(null)).UnclearedStatementAmount)));
			this.UnclearedStatementAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|a1de1bdb-bfe0-4519-8e97-13ece4b5cf3f", "Uncleared Statement Amount");
			this.UnclearedStatementAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 73, true);
			this.UnclearedStatementAmountCalcEdit.Name = "UnclearedStatementAmountCalcEdit";
			this.UnclearedStatementAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.UnclearedStatementAmountCalcEdit.TabIndex = 2;
			this.UnclearedStatementAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ClosingBalanceCalcReadOnlyEdit
			// 
			this.ClosingBalanceCalcReadOnlyEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ClosingBalanceCalcReadOnlyEdit, "ClosingBalanceReadOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.CashBook.BankReconciliation)(null)).ClosingBalanceReadOnly)));
			this.ClosingBalanceCalcReadOnlyEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|d3420c80-f0c7-4653-8544-4a17b376d603", "Statement Closing Balance");
			this.ClosingBalanceCalcReadOnlyEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 21, true);
			this.ClosingBalanceCalcReadOnlyEdit.Name = "ClosingBalanceCalcReadOnlyEdit";
			this.ClosingBalanceCalcReadOnlyEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.ClosingBalanceCalcReadOnlyEdit.TabIndex = 0;
			this.ClosingBalanceCalcReadOnlyEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FindFilterButton
			// 
			this.FindFilterButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|be46e42c-3581-4d78-93c1-b48b6a29a965", "&Find", "Find.");
			this.FindFilterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(752, 15, true);
			this.FindFilterButton.Name = "FindFilterButton";
			this.FindFilterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.FindFilterButton.TabIndex = 8;
			this.FindFilterButton.Click += new EventHandler(this.FindFilterButton_Click);
			// 
			// ClearFilterButton
			// 
			this.ClearFilterButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|16ebb8b6-1163-41a7-a06f-ad4b4bf7b284", "C&lear", "Clear.");
			this.ClearFilterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(752, 42, true);
			this.ClearFilterButton.Name = "ClearFilterButton";
			this.ClearFilterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ClearFilterButton.TabIndex = 9;
			this.ClearFilterButton.Click += new EventHandler(this.ClearFilterButton_Click);
			// 
			// TextSearchFilterTextBox
			// 
			this.BindingSource.SetBindingMember(this.TextSearchFilterTextBox, "TextSearchFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Business.CashBook.BankReconciliation)(null)).TextSearchFilter)));
			this.TextSearchFilterTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|6aafebcd-3e77-4915-a2d6-fe8f9548b848", "Check Reference", "Check Reference Number", "");
			this.TextSearchFilterTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(594, 20, true);
			this.TextSearchFilterTextBox.Name = "TextSearchFilterTextBox";
			this.TextSearchFilterTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 20, true);
			this.TextSearchFilterTextBox.TabIndex = 3;
			// 
			// FromDateFilterDateEdit
			// 
			this.FromDateFilterDateEdit.AutoCompleteMonthThreshold = 1;
			this.FromDateFilterDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FromDateFilterDateEdit, "FromDateFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.CashBook.BankReconciliation)(null)).FromDateFilter)));
			this.FromDateFilterDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|b8fb6a04-3abb-4190-a051-4b25e8fc8a62", "From", "From Date", "");
			this.FromDateFilterDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(262, 50, true);
			this.FromDateFilterDateEdit.Name = "FromDateFilterDateEdit";
			this.FromDateFilterDateEdit.TabIndex = 5;
			// 
			// ToDateFilterDateEdit
			// 
			this.ToDateFilterDateEdit.AutoCompleteMonthThreshold = 1;
			this.ToDateFilterDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ToDateFilterDateEdit, "ToDateFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.CashBook.BankReconciliation)(null)).ToDateFilter)));
			this.ToDateFilterDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|73c72128-adf2-4486-8171-7027057b51b1", "To Date");
			this.ToDateFilterDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 50, true);
			this.ToDateFilterDateEdit.Name = "ToDateFilterDateEdit";
			this.ToDateFilterDateEdit.TabIndex = 6;
			// 
			// zCheckBox1
			// 
			this.BindingSource.SetBindingMember(this.zCheckBox1, "ClearedFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Business.CashBook.BankReconciliation)(null)).ClearedFilter)));
			this.zCheckBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|fe1c4e04-791f-461b-9231-fd46a62cfe17", "Include Cleared Transactions");
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(594, 47, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 30, true);
			this.zCheckBox1.TabIndex = 7;
			// 
			// DateFilterDropEdit
			// 
			this.BindingSource.SetBindingMember(this.DateFilterDropEdit, "DateFilterType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.CashBook.BankReconciliation)(null)).DateFilterType)));
			this.DateFilterDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|a095acec-954b-4d80-8056-be4623ec364f", "Date", "Date", "");
			this.DateFilterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 50, true);
			this.DateFilterDropEdit.Name = "DateFilterDropEdit";
			this.DateFilterDropEdit.PreBoundMaxLength = 3;
			this.DateFilterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 20, true);
			this.DateFilterDropEdit.TabIndex = 4;
			// 
			// FilterGroupBox
			// 
			this.FilterGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|92c52519-4106-4f0d-a7e0-a7efe40d5062", "Search Transactions");
			this.FilterGroupBox.Controls.Add(this.DateFilterDropEdit);
			this.FilterGroupBox.Controls.Add(this.AmountFilterCalcEdit);
			this.FilterGroupBox.Controls.Add(this.zCheckBox1);
			this.FilterGroupBox.Controls.Add(this.ToDateFilterDateEdit);
			this.FilterGroupBox.Controls.Add(this.FromDateFilterDateEdit);
			this.FilterGroupBox.Controls.Add(this.TextSearchFilterTextBox);
			this.FilterGroupBox.Controls.Add(this.MethodFilterDropEdit);
			this.FilterGroupBox.Controls.Add(this.TypeFilterDropEdit);
			this.FilterGroupBox.Controls.Add(this.ClearFilterButton);
			this.FilterGroupBox.Controls.Add(this.FindFilterButton);
			this.FilterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 192, true);
			this.FilterGroupBox.Name = "FilterGroupBox";
			this.FilterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 83, true);
			this.FilterGroupBox.TabIndex = 2;
			this.FilterGroupBox.TabStop = false;
			// 
			// AmountFilterCalcEdit
			// 
			this.AmountFilterCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AmountFilterCalcEdit, "AmountFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.CashBook.BankReconciliation)(null)).AmountFilter)));
			this.AmountFilterCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|789ddfa4-535f-4f4d-9a18-f405867e6867", "Amount Filter");
			this.AmountFilterCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(343, 20, true);
			this.AmountFilterCalcEdit.Name = "AmountFilterCalcEdit";
			this.AmountFilterCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.AmountFilterCalcEdit.TabIndex = 2;
			this.AmountFilterCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MethodFilterDropEdit
			// 
			this.BindingSource.SetBindingMember(this.MethodFilterDropEdit, "MethodFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.CashBook.BankReconciliation)(null)).MethodFilter)));
			this.MethodFilterDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|7562e259-a228-4b3e-92d4-ae4c1379553d", "Method", "Method", "Method\r\n.");
			this.MethodFilterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 20, true);
			this.MethodFilterDropEdit.Name = "MethodFilterDropEdit";
			this.MethodFilterDropEdit.PreBoundMaxLength = 3;
			this.MethodFilterDropEdit.ShowDescriptionBox = false;
			this.MethodFilterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.MethodFilterDropEdit.TabIndex = 1;
			// 
			// TypeFilterDropEdit
			// 
			this.BindingSource.SetBindingMember(this.TypeFilterDropEdit, "TypeFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.CashBook.BankReconciliation)(null)).TypeFilter)));
			this.TypeFilterDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|d33b9499-8ab6-415e-99c6-6bce7326b617", "Type", "Type", "");
			this.TypeFilterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 20, true);
			this.TypeFilterDropEdit.Name = "TypeFilterDropEdit";
			this.TypeFilterDropEdit.PreBoundMaxLength = 3;
			this.TypeFilterDropEdit.ShowDescriptionBox = false;
			this.TypeFilterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.TypeFilterDropEdit.TabIndex = 0;
			// 
			// BankReconcilationForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankReconcilationForm|1d9db754-1bbc-4c52-b88a-18e4bbfb619c", "Bank Reconciliation");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 500, true);
			this.Controls.Add(this.zGroupBox2);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.BankTransactionButton);
			this.Controls.Add(this.CurrentCreditTotalCalcEdit);
			this.Controls.Add(this.CurrentDebitTotalCalcEdit);
			this.Controls.Add(this.TotalDifferenceCalcEdit);
			this.Controls.Add(this.StatementTotalCalcEdit);
			this.Controls.Add(this.CashBookTotalCalcEdit);
			this.Controls.Add(this.FilterGroupBox);
			this.Controls.Add(this.BankReconGrid);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Business.CashBook.BankReconciliation);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.CashBook.BankReconciliation";
			this.Name = "BankReconcilationForm";
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.BankReconGrid, 0);
			this.Controls.SetChildIndex(this.FilterGroupBox, 0);
			this.Controls.SetChildIndex(this.CashBookTotalCalcEdit, 0);
			this.Controls.SetChildIndex(this.StatementTotalCalcEdit, 0);
			this.Controls.SetChildIndex(this.TotalDifferenceCalcEdit, 0);
			this.Controls.SetChildIndex(this.CurrentDebitTotalCalcEdit, 0);
			this.Controls.SetChildIndex(this.CurrentCreditTotalCalcEdit, 0);
			this.Controls.SetChildIndex(this.BankTransactionButton, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.zGroupBox2, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BankReconGrid)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.FilterGroupBox.ResumeLayout(false);
			this.FilterGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
	}
