using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.CashBook.DepositBatch
{
	public partial class DepositBatchForm
	{


		#region Windows Form Designer generated code

		private ZLabel zLabel1;
		private ZTemplateTabControl BatchTabControl;
		private ZTabPage zTabPage1;
		private ZPanel zPanel1;
		private ZGroupBox CompanyFilterGroupBox;
		private ZGuidFindBox BranchGuidFindBox;
		private ZRadioButton BranchRadioButton;
		private ZRadioButton CompanyRadioButton;
		private ZGroupBox CurrencyFilterGroupBox;
		private ZRadioButton AllCurrenciesRadioButton;
		private ZRadioButton ForeignRadioButton;
		private ZRadioButton LocalCurrencyRadioButton;
		private ZGroupBox BatchHeaderGroupBox;
		private ZTextBox BatchNumberTextBox;
		private ZDateEdit DepositDateDateEdit;
		private ZGroupBox TransactionsGroupBox;
		private ZGrid TransactionsGrid;
		private ZGroupBox BankDetailsGroupBox;
		private ZGrid BankGrid;
		private ZButton UnSelectAllButton;
		private ZButton SelectAllButton;
		private ZTemplateTabControl SummaryTabControl;
		private ZTabPage BatchSummarySelectedTabPage;
		private ZTabPage BatchSummaryNotSelectedTabPage;
		private ZLabel zLabel12;
		private ZLabel zLabel13;
		private ZLabel zLabel14;
		private ZLabel zLabel15;
		private ZLabel zLabel16;
		private ZCalcEdit zCalcEdit16;
		private ZCalcEdit zCalcEdit17;
		private ZCalcEdit zCalcEdit18;
		private ZCalcEdit zCalcEdit19;
		private ZCalcEdit zCalcEdit20;
		private ZCalcEdit zCalcEdit21;
		private ZCalcEdit zCalcEdit22;
		private ZCalcEdit zCalcEdit23;
		private ZCalcEdit zCalcEdit24;
		private ZCalcEdit zCalcEdit25;
		private ZCalcEdit TotalAmountCalcEdit;
		private ZCalcEdit CashAmountCalcEdit;
		private ZCalcEdit ChequeAmountCalcEdit;
		private ZCalcEdit DirectCreditAmountCalcEdit;
		private ZCalcEdit CreditCardAmountCalcEdit;
		private ZCalcEdit TotalCalcEdit;
		private ZCalcEdit CashCountCalcEdit;
		private ZCalcEdit ChequeCalcEdit;
		private ZCalcEdit DirectCreditCalcEdit;
		private ZCalcEdit CreditCardCalcEdit;
		private ZLabel zLabel7;
		private ZLabel zLabel8;
		private ZLabel zLabel9;
		private ZLabel zLabel10;
		private ZLabel zLabel11;
		private ZGroupBox SummaryGroupBox;
		private ZLabel BankCodeLabel;
		private ZButton BankUnSelectAllButton;
		private ZButton BankSelectAllButton;
		private ZLabel CancelReasonLabel;
		private System.ComponentModel.IContainer components;
		private ZLogsTabPage zEventTabPage1;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new ZTextBoxColumnStyleInfo();
			this.BatchTabControl = new ZTemplateTabControl();
			this.zTabPage1 = new ZTabPage();
			this.zPanel1 = new ZPanel();
			this.SummaryGroupBox = new ZGroupBox();
			this.SummaryTabControl = new ZTemplateTabControl();
			this.BatchSummarySelectedTabPage = new ZTabPage();
			this.TotalAmountCalcEdit = new ZCalcEdit();
			this.CashAmountCalcEdit = new ZCalcEdit();
			this.ChequeAmountCalcEdit = new ZCalcEdit();
			this.DirectCreditAmountCalcEdit = new ZCalcEdit();
			this.CreditCardAmountCalcEdit = new ZCalcEdit();
			this.TotalCalcEdit = new ZCalcEdit();
			this.CashCountCalcEdit = new ZCalcEdit();
			this.ChequeCalcEdit = new ZCalcEdit();
			this.DirectCreditCalcEdit = new ZCalcEdit();
			this.CreditCardCalcEdit = new ZCalcEdit();
			this.zLabel7 = new ZLabel();
			this.zLabel8 = new ZLabel();
			this.zLabel9 = new ZLabel();
			this.zLabel10 = new ZLabel();
			this.zLabel11 = new ZLabel();
			this.BatchSummaryNotSelectedTabPage = new ZTabPage();
			this.zCalcEdit21 = new ZCalcEdit();
			this.zCalcEdit22 = new ZCalcEdit();
			this.zCalcEdit23 = new ZCalcEdit();
			this.zCalcEdit24 = new ZCalcEdit();
			this.zCalcEdit25 = new ZCalcEdit();
			this.zCalcEdit16 = new ZCalcEdit();
			this.zCalcEdit17 = new ZCalcEdit();
			this.zCalcEdit18 = new ZCalcEdit();
			this.zCalcEdit19 = new ZCalcEdit();
			this.zCalcEdit20 = new ZCalcEdit();
			this.zLabel12 = new ZLabel();
			this.zLabel13 = new ZLabel();
			this.zLabel14 = new ZLabel();
			this.zLabel15 = new ZLabel();
			this.zLabel16 = new ZLabel();
			this.TransactionsGroupBox = new ZGroupBox();
			this.zLabel1 = new ZLabel();
			this.UnSelectAllButton = new ZButton();
			this.SelectAllButton = new ZButton();
			this.TransactionsGrid = new ZGrid();
			this.BankCodeLabel = new ZLabel();
			this.BankDetailsGroupBox = new ZGroupBox();
			this.BankUnSelectAllButton = new ZButton();
			this.BankSelectAllButton = new ZButton();
			this.BankGrid = new ZGrid();
			this.CompanyFilterGroupBox = new ZGroupBox();
			this.BranchGuidFindBox = new ZGuidFindBox();
			this.BranchRadioButton = new ZRadioButton();
			this.CompanyRadioButton = new ZRadioButton();
			this.CurrencyFilterGroupBox = new ZGroupBox();
			this.AllCurrenciesRadioButton = new ZRadioButton();
			this.ForeignRadioButton = new ZRadioButton();
			this.LocalCurrencyRadioButton = new ZRadioButton();
			this.BatchHeaderGroupBox = new ZGroupBox();
			this.DepositPostDateDateEdit = new ZDateEdit();
			this.CancelReasonLabel = new ZLabel();
			this.BatchNumberTextBox = new ZTextBox();
			this.DepositDateDateEdit = new ZDateEdit();
			this.BottomPanel = new ZPanel();
			this.PostingButtonsUserControl = new ZPostingButtonsUserControl();
			this.zEventTabPage1 = new ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BatchTabControl.SuspendLayout();
			this.zTabPage1.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.SummaryGroupBox.SuspendLayout();
			this.SummaryTabControl.SuspendLayout();
			this.BatchSummarySelectedTabPage.SuspendLayout();
			this.BatchSummaryNotSelectedTabPage.SuspendLayout();
			this.TransactionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransactionsGrid)).BeginInit();
			this.BankDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BankGrid)).BeginInit();
			this.CompanyFilterGroupBox.SuspendLayout();
			this.CurrencyFilterGroupBox.SuspendLayout();
			this.BatchHeaderGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 569, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(757);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(DepositBatchParent);
			// 
			// BatchTabControl
			// 
			this.BatchTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BatchTabControl.Controls.Add(this.zTabPage1);
			this.BatchTabControl.Controls.Add(this.zEventTabPage1);
			this.BatchTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BatchTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BatchTabControl.Name = "BatchTabControl";
			this.BatchTabControl.SelectedIndex = 0;
			this.BatchTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 538, true);
			this.BatchTabControl.TabIndex = 0;
			// 
			// zTabPage1
			// 
			this.zTabPage1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|34ed6c04-9314-4a7c-8052-34cfc8541710", "Deposit Batch");
			this.zTabPage1.Controls.Add(this.zPanel1);
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 511, true);
			this.zTabPage1.TabIndex = 0;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.SummaryGroupBox);
			this.zPanel1.Controls.Add(this.TransactionsGroupBox);
			this.zPanel1.Controls.Add(this.BankDetailsGroupBox);
			this.zPanel1.Controls.Add(this.CompanyFilterGroupBox);
			this.zPanel1.Controls.Add(this.CurrencyFilterGroupBox);
			this.zPanel1.Controls.Add(this.BatchHeaderGroupBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 511, true);
			this.zPanel1.TabIndex = 0;
			// 
			// SummaryGroupBox
			// 
			this.SummaryGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SummaryGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|9aeba925-b7a9-40b1-9f66-8004eb1d809a", "Batch Summary");
			this.SummaryGroupBox.Controls.Add(this.SummaryTabControl);
			this.SummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 371, true);
			this.SummaryGroupBox.Name = "SummaryGroupBox";
			this.SummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(743, 130, true);
			this.SummaryGroupBox.TabIndex = 5;
			this.SummaryGroupBox.TabStop = false;
			// 
			// SummaryTabControl
			// 
			this.SummaryTabControl.Controls.Add(this.BatchSummarySelectedTabPage);
			this.SummaryTabControl.Controls.Add(this.BatchSummaryNotSelectedTabPage);
			this.SummaryTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 20, true);
			this.SummaryTabControl.Name = "SummaryTabControl";
			this.SummaryTabControl.SelectedIndex = 0;
			this.SummaryTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 102, true);
			this.SummaryTabControl.TabIndex = 0;
			// 
			// BatchSummarySelectedTabPage
			// 
			this.BatchSummarySelectedTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.BatchSummarySelectedTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|66d996b0-e3da-4ba5-bca9-d65de08925e3", "Selected Transactions");
			this.BatchSummarySelectedTabPage.Controls.Add(this.TotalAmountCalcEdit);
			this.BatchSummarySelectedTabPage.Controls.Add(this.CashAmountCalcEdit);
			this.BatchSummarySelectedTabPage.Controls.Add(this.ChequeAmountCalcEdit);
			this.BatchSummarySelectedTabPage.Controls.Add(this.DirectCreditAmountCalcEdit);
			this.BatchSummarySelectedTabPage.Controls.Add(this.CreditCardAmountCalcEdit);
			this.BatchSummarySelectedTabPage.Controls.Add(this.TotalCalcEdit);
			this.BatchSummarySelectedTabPage.Controls.Add(this.CashCountCalcEdit);
			this.BatchSummarySelectedTabPage.Controls.Add(this.ChequeCalcEdit);
			this.BatchSummarySelectedTabPage.Controls.Add(this.DirectCreditCalcEdit);
			this.BatchSummarySelectedTabPage.Controls.Add(this.CreditCardCalcEdit);
			this.BatchSummarySelectedTabPage.Controls.Add(this.zLabel7);
			this.BatchSummarySelectedTabPage.Controls.Add(this.zLabel8);
			this.BatchSummarySelectedTabPage.Controls.Add(this.zLabel9);
			this.BatchSummarySelectedTabPage.Controls.Add(this.zLabel10);
			this.BatchSummarySelectedTabPage.Controls.Add(this.zLabel11);
			this.BatchSummarySelectedTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BatchSummarySelectedTabPage.Name = "BatchSummarySelectedTabPage";
			this.BatchSummarySelectedTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.BatchSummarySelectedTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 75, true);
			this.BatchSummarySelectedTabPage.TabIndex = 0;
			// 
			// TotalAmountCalcEdit
			// 
			this.TotalAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalAmountCalcEdit, "DepositBatchLines.TotalDepositOSAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).TotalDepositOSAmount)));
			this.TotalAmountCalcEdit.BindToDecimalPlaces = "DepositBatchLines.BankCurrencyDecimals";
			this.TotalAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|66412d56-9856-44bf-8a44-c3aaf4045e89", "", "Deselect all banks from the list");
			this.TotalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 51, true);
			this.TotalAmountCalcEdit.Name = "TotalAmountCalcEdit";
			this.TotalAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.TotalAmountCalcEdit.TabIndex = 14;
			this.TotalAmountCalcEdit.Text = "0.00";
			this.TotalAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CashAmountCalcEdit
			// 
			this.CashAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CashAmountCalcEdit, "DepositBatchLines.CashSelectedAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).CashSelectedAmount)));
			this.CashAmountCalcEdit.BindToDecimalPlaces = "DepositBatchLines.BankCurrencyDecimals";
			this.CashAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|69f28bb3-446a-4a96-9553-ec2d739ff0a4", "Amount");
			this.CashAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 51, true);
			this.CashAmountCalcEdit.Name = "CashAmountCalcEdit";
			this.CashAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.CashAmountCalcEdit.TabIndex = 10;
			this.CashAmountCalcEdit.Text = "0.00";
			this.CashAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChequeAmountCalcEdit
			// 
			this.ChequeAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ChequeAmountCalcEdit, "DepositBatchLines.ChequeSelectedAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).ChequeSelectedAmount)));
			this.ChequeAmountCalcEdit.BindToDecimalPlaces = "DepositBatchLines.BankCurrencyDecimals";
			this.ChequeAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|8a4f66ad-be87-42e0-971b-2914134f4922", "", "Deselect all banks from the list");
			this.ChequeAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 51, true);
			this.ChequeAmountCalcEdit.Name = "ChequeAmountCalcEdit";
			this.ChequeAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.ChequeAmountCalcEdit.TabIndex = 11;
			this.ChequeAmountCalcEdit.Text = "0.00";
			this.ChequeAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DirectCreditAmountCalcEdit
			// 
			this.DirectCreditAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DirectCreditAmountCalcEdit, "DepositBatchLines.DirectCreditSelectedAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).DirectCreditSelectedAmount)));
			this.DirectCreditAmountCalcEdit.BindToDecimalPlaces = "DepositBatchLines.BankCurrencyDecimals";
			this.DirectCreditAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|32b08c86-6f91-4a3b-b8ae-283fe8ee49af", "", "Deselect all banks from the list");
			this.DirectCreditAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 51, true);
			this.DirectCreditAmountCalcEdit.Name = "DirectCreditAmountCalcEdit";
			this.DirectCreditAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.DirectCreditAmountCalcEdit.TabIndex = 13;
			this.DirectCreditAmountCalcEdit.Text = "0.00";
			this.DirectCreditAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CreditCardAmountCalcEdit
			// 
			this.CreditCardAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CreditCardAmountCalcEdit, "DepositBatchLines.CreditCardSelectedAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).CreditCardSelectedAmount)));
			this.CreditCardAmountCalcEdit.BindToDecimalPlaces = "DepositBatchLines.BankCurrencyDecimals";
			this.CreditCardAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|980a4d3d-ea51-4737-90cc-35e8940da99c", "", "Deselect all banks from the list");
			this.CreditCardAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 51, true);
			this.CreditCardAmountCalcEdit.Name = "CreditCardAmountCalcEdit";
			this.CreditCardAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.CreditCardAmountCalcEdit.TabIndex = 12;
			this.CreditCardAmountCalcEdit.Text = "0.00";
			this.CreditCardAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalCalcEdit
			// 
			this.TotalCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalCalcEdit, "DepositBatchLines.TotalDepositCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).TotalDepositCount)));
			this.TotalCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|507d6df0-e479-4cab-8518-ec5e02030a06", "", "Deselect all banks from the list");
			this.TotalCalcEdit.Decimals = 0;
			this.TotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 26, true);
			this.TotalCalcEdit.Name = "TotalCalcEdit";
			this.TotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.TotalCalcEdit.TabIndex = 9;
			this.TotalCalcEdit.Text = "0";
			this.TotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CashCountCalcEdit
			// 
			this.CashCountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CashCountCalcEdit, "DepositBatchLines.CashTransactionSelectedCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).CashTransactionSelectedCount)));
			this.CashCountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|32053efb-6920-4d91-b910-13db22e96866", "Count");
			this.CashCountCalcEdit.Decimals = 0;
			this.CashCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 26, true);
			this.CashCountCalcEdit.Name = "CashCountCalcEdit";
			this.CashCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.CashCountCalcEdit.TabIndex = 5;
			this.CashCountCalcEdit.Text = "0";
			this.CashCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChequeCalcEdit
			// 
			this.ChequeCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ChequeCalcEdit, "DepositBatchLines.ChequeTransactionSelectedCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).ChequeTransactionSelectedCount)));
			this.ChequeCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|09972233-c730-43f9-8e90-f74e3929ee60", "", "Deselect all banks from the list");
			this.ChequeCalcEdit.Decimals = 0;
			this.ChequeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 26, true);
			this.ChequeCalcEdit.Name = "ChequeCalcEdit";
			this.ChequeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.ChequeCalcEdit.TabIndex = 6;
			this.ChequeCalcEdit.Text = "0";
			this.ChequeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DirectCreditCalcEdit
			// 
			this.DirectCreditCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DirectCreditCalcEdit, "DepositBatchLines.DirectCreditTransactionSelectedCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).DirectCreditTransactionSelectedCount)));
			this.DirectCreditCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|5a9e01eb-8b3d-496a-ae1b-8d3c36258a83", "", "Deselect all banks from the list");
			this.DirectCreditCalcEdit.Decimals = 0;
			this.DirectCreditCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 26, true);
			this.DirectCreditCalcEdit.Name = "DirectCreditCalcEdit";
			this.DirectCreditCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.DirectCreditCalcEdit.TabIndex = 8;
			this.DirectCreditCalcEdit.Text = "0";
			this.DirectCreditCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CreditCardCalcEdit
			// 
			this.CreditCardCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CreditCardCalcEdit, "DepositBatchLines.CreditCardTransactionSelectedCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).CreditCardTransactionSelectedCount)));
			this.CreditCardCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|226daf20-394c-4511-91a8-fd756be32524", "", "Deselect all banks from the list");
			this.CreditCardCalcEdit.Decimals = 0;
			this.CreditCardCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 26, true);
			this.CreditCardCalcEdit.Name = "CreditCardCalcEdit";
			this.CreditCardCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.CreditCardCalcEdit.TabIndex = 7;
			this.CreditCardCalcEdit.Text = "0";
			this.CreditCardCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel7
			// 
			this.zLabel7.AutoSize = true;
			this.zLabel7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|6692c618-1c0a-4b87-85b1-bd1802dc5022", "Direct Credit");
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 7, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 13, true);
			this.zLabel7.TabIndex = 3;
			// 
			// zLabel8
			// 
			this.zLabel8.AutoSize = true;
			this.zLabel8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|04090a2d-75f8-4222-a604-6788ccbcca6c", "Credit Card");
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 7, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 13, true);
			this.zLabel8.TabIndex = 2;
			// 
			// zLabel9
			// 
			this.zLabel9.AutoSize = true;
			this.zLabel9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|e15007a1-df22-4d53-b72f-e86ea23866c8", "Check");
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 7, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 13, true);
			this.zLabel9.TabIndex = 1;
			// 
			// zLabel10
			// 
			this.zLabel10.AutoSize = true;
			this.zLabel10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|90b12f1a-8759-44a1-9cc6-7a63aaabc8b8", "Cash");
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 7, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 13, true);
			this.zLabel10.TabIndex = 0;
			// 
			// zLabel11
			// 
			this.zLabel11.AutoSize = true;
			this.zLabel11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|6a19710b-bb28-4e33-aa7f-bb90d86b0192", "Total");
			this.zLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 7, true);
			this.zLabel11.Name = "zLabel11";
			this.zLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 13, true);
			this.zLabel11.TabIndex = 4;
			// 
			// BatchSummaryNotSelectedTabPage
			// 
			this.BatchSummaryNotSelectedTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.BatchSummaryNotSelectedTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|58b62602-b352-4e8c-a78f-960f844ee0a6", "Not Selected Transactions()", "Not Selected Transactions.");
			this.BatchSummaryNotSelectedTabPage.Controls.Add(this.zCalcEdit21);
			this.BatchSummaryNotSelectedTabPage.Controls.Add(this.zCalcEdit22);
			this.BatchSummaryNotSelectedTabPage.Controls.Add(this.zCalcEdit23);
			this.BatchSummaryNotSelectedTabPage.Controls.Add(this.zCalcEdit24);
			this.BatchSummaryNotSelectedTabPage.Controls.Add(this.zCalcEdit25);
			this.BatchSummaryNotSelectedTabPage.Controls.Add(this.zCalcEdit16);
			this.BatchSummaryNotSelectedTabPage.Controls.Add(this.zCalcEdit17);
			this.BatchSummaryNotSelectedTabPage.Controls.Add(this.zCalcEdit18);
			this.BatchSummaryNotSelectedTabPage.Controls.Add(this.zCalcEdit19);
			this.BatchSummaryNotSelectedTabPage.Controls.Add(this.zCalcEdit20);
			this.BatchSummaryNotSelectedTabPage.Controls.Add(this.zLabel12);
			this.BatchSummaryNotSelectedTabPage.Controls.Add(this.zLabel13);
			this.BatchSummaryNotSelectedTabPage.Controls.Add(this.zLabel14);
			this.BatchSummaryNotSelectedTabPage.Controls.Add(this.zLabel15);
			this.BatchSummaryNotSelectedTabPage.Controls.Add(this.zLabel16);
			this.BatchSummaryNotSelectedTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BatchSummaryNotSelectedTabPage.Name = "BatchSummaryNotSelectedTabPage";
			this.BatchSummaryNotSelectedTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.BatchSummaryNotSelectedTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 75, true);
			this.BatchSummaryNotSelectedTabPage.TabIndex = 1;
			// 
			// zCalcEdit21
			// 
			this.zCalcEdit21.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zCalcEdit21, "DepositBatchLines.TotalDepositNotSelectedAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).TotalDepositNotSelectedAmount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit21, false);
			this.zCalcEdit21.BindToDecimalPlaces = "DepositBatchLines.BankCurrencyDecimals";
			this.zCalcEdit21.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 51, true);
			this.zCalcEdit21.Name = "zCalcEdit21";
			this.zCalcEdit21.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.zCalcEdit21.TabIndex = 14;
			this.zCalcEdit21.Text = "0.00";
			this.zCalcEdit21.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit22
			// 
			this.zCalcEdit22.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zCalcEdit22, "DepositBatchLines.CashNotSelectedAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).CashNotSelectedAmount)));
			this.zCalcEdit22.BindToDecimalPlaces = "DepositBatchLines.BankCurrencyDecimals";
			this.zCalcEdit22.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|770355fe-de2b-48a9-804e-c9f5e64533f5", "Amount");
			this.zCalcEdit22.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 50, true);
			this.zCalcEdit22.Name = "zCalcEdit22";
			this.zCalcEdit22.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.zCalcEdit22.TabIndex = 10;
			this.zCalcEdit22.Text = "0.00";
			this.zCalcEdit22.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit23
			// 
			this.zCalcEdit23.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zCalcEdit23, "DepositBatchLines.ChequeNotSelectedAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).ChequeNotSelectedAmount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit23, false);
			this.zCalcEdit23.BindToDecimalPlaces = "DepositBatchLines.BankCurrencyDecimals";
			this.zCalcEdit23.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 51, true);
			this.zCalcEdit23.Name = "zCalcEdit23";
			this.zCalcEdit23.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.zCalcEdit23.TabIndex = 11;
			this.zCalcEdit23.Text = "0.00";
			this.zCalcEdit23.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit24
			// 
			this.zCalcEdit24.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zCalcEdit24, "DepositBatchLines.DirectCreditNotSelectedAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).DirectCreditNotSelectedAmount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit24, false);
			this.zCalcEdit24.BindToDecimalPlaces = "DepositBatchLines.BankCurrencyDecimals";
			this.zCalcEdit24.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 51, true);
			this.zCalcEdit24.Name = "zCalcEdit24";
			this.zCalcEdit24.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.zCalcEdit24.TabIndex = 13;
			this.zCalcEdit24.Text = "0.00";
			this.zCalcEdit24.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit25
			// 
			this.zCalcEdit25.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zCalcEdit25, "DepositBatchLines.CreditCardNotSelectedAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).CreditCardNotSelectedAmount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit25, false);
			this.zCalcEdit25.BindToDecimalPlaces = "DepositBatchLines.BankCurrencyDecimals";
			this.zCalcEdit25.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 51, true);
			this.zCalcEdit25.Name = "zCalcEdit25";
			this.zCalcEdit25.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.zCalcEdit25.TabIndex = 12;
			this.zCalcEdit25.Text = "0.00";
			this.zCalcEdit25.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit16
			// 
			this.zCalcEdit16.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zCalcEdit16, "DepositBatchLines.TotalDepositNotSelectedCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).TotalDepositNotSelectedCount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit16, false);
			this.zCalcEdit16.Decimals = 0;
			this.zCalcEdit16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 26, true);
			this.zCalcEdit16.Name = "zCalcEdit16";
			this.zCalcEdit16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.zCalcEdit16.TabIndex = 9;
			this.zCalcEdit16.Text = "0";
			this.zCalcEdit16.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit17
			// 
			this.zCalcEdit17.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zCalcEdit17, "DepositBatchLines.CashTransactionNotSelectedCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).CashTransactionNotSelectedCount)));
			this.zCalcEdit17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|1057aac5-0416-43d2-afaa-6ceda007b96a", "Count");
			this.zCalcEdit17.Decimals = 0;
			this.zCalcEdit17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 26, true);
			this.zCalcEdit17.Name = "zCalcEdit17";
			this.zCalcEdit17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.zCalcEdit17.TabIndex = 5;
			this.zCalcEdit17.Text = "0";
			this.zCalcEdit17.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit18
			// 
			this.zCalcEdit18.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zCalcEdit18, "DepositBatchLines.ChequeTransactionNotSelectedCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).ChequeTransactionNotSelectedCount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit18, false);
			this.zCalcEdit18.Decimals = 0;
			this.zCalcEdit18.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 26, true);
			this.zCalcEdit18.Name = "zCalcEdit18";
			this.zCalcEdit18.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.zCalcEdit18.TabIndex = 6;
			this.zCalcEdit18.Text = "0";
			this.zCalcEdit18.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit19
			// 
			this.zCalcEdit19.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zCalcEdit19, "DepositBatchLines.DirectCreditTransactionNotSelectedCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).DirectCreditTransactionNotSelectedCount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit19, false);
			this.zCalcEdit19.Decimals = 0;
			this.zCalcEdit19.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 26, true);
			this.zCalcEdit19.Name = "zCalcEdit19";
			this.zCalcEdit19.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.zCalcEdit19.TabIndex = 8;
			this.zCalcEdit19.Text = "0";
			this.zCalcEdit19.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit20
			// 
			this.zCalcEdit20.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zCalcEdit20, "DepositBatchLines.CreditCardTransactionNotSelectedCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).CreditCardTransactionNotSelectedCount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit20, false);
			this.zCalcEdit20.Decimals = 0;
			this.zCalcEdit20.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 26, true);
			this.zCalcEdit20.Name = "zCalcEdit20";
			this.zCalcEdit20.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.zCalcEdit20.TabIndex = 7;
			this.zCalcEdit20.Text = "0";
			this.zCalcEdit20.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel12
			// 
			this.zLabel12.AutoSize = true;
			this.zLabel12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|e39a93e1-b9f6-4aed-9895-7d61b92e426d", "Direct Credit");
			this.zLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(326, 7, true);
			this.zLabel12.Name = "zLabel12";
			this.zLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 13, true);
			this.zLabel12.TabIndex = 3;
			// 
			// zLabel13
			// 
			this.zLabel13.AutoSize = true;
			this.zLabel13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|ec4d6865-5fd7-405f-adc6-ecb1265d204e", "Credit Card");
			this.zLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 7, true);
			this.zLabel13.Name = "zLabel13";
			this.zLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 13, true);
			this.zLabel13.TabIndex = 2;
			// 
			// zLabel14
			// 
			this.zLabel14.AutoSize = true;
			this.zLabel14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|4cdc1e8d-bbb0-451b-9e1a-8b8903dffa11", "Check");
			this.zLabel14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 7, true);
			this.zLabel14.Name = "zLabel14";
			this.zLabel14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 13, true);
			this.zLabel14.TabIndex = 1;
			// 
			// zLabel15
			// 
			this.zLabel15.AutoSize = true;
			this.zLabel15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|af97f425-1fa9-4d77-81a7-9a99e24c6d1a", "Cash");
			this.zLabel15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 7, true);
			this.zLabel15.Name = "zLabel15";
			this.zLabel15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 13, true);
			this.zLabel15.TabIndex = 0;
			// 
			// zLabel16
			// 
			this.zLabel16.AutoSize = true;
			this.zLabel16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|f5eb14ef-2494-43af-b15b-e20b0dc2758b", "Total");
			this.zLabel16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 7, true);
			this.zLabel16.Name = "zLabel16";
			this.zLabel16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 13, true);
			this.zLabel16.TabIndex = 4;
			// 
			// TransactionsGroupBox
			// 
			this.TransactionsGroupBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.TransactionsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|68c6c0f1-bda0-40fa-9a24-7f19e2b244bf", "Receipt Transactions");
			this.TransactionsGroupBox.Controls.Add(this.zLabel1);
			this.TransactionsGroupBox.Controls.Add(this.UnSelectAllButton);
			this.TransactionsGroupBox.Controls.Add(this.SelectAllButton);
			this.TransactionsGroupBox.Controls.Add(this.TransactionsGrid);
			this.TransactionsGroupBox.Controls.Add(this.BankCodeLabel);
			this.TransactionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 264, true);
			this.TransactionsGroupBox.Name = "TransactionsGroupBox";
			this.TransactionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(743, 101, true);
			this.TransactionsGroupBox.TabIndex = 4;
			this.TransactionsGroupBox.TabStop = false;
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|93681dfd-68b6-4ae7-b90c-70d6cb36777a", "Selected Bank Account:");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(522, 78, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 13, true);
			this.zLabel1.TabIndex = 3;
			// 
			// UnSelectAllButton
			// 
			this.UnSelectAllButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.UnSelectAllButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|c85b015a-1fc8-48a3-8e35-5adffde40abb", "Deselect All");
			this.UnSelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 72, true);
			this.UnSelectAllButton.Name = "UnSelectAllButton";
			this.UnSelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 24, true);
			this.UnSelectAllButton.TabIndex = 2;
			this.UnSelectAllButton.UseVisualStyleBackColor = true;
			this.UnSelectAllButton.Click += new EventHandler(this.UnSelectAllButton_Click);
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectAllButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|84e6cbf0-f497-45ca-b1b0-5af485f62e05", "Select All");
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 72, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 24, true);
			this.SelectAllButton.TabIndex = 1;
			this.SelectAllButton.UseVisualStyleBackColor = true;
			this.SelectAllButton.Click += new EventHandler(this.SelectAllButton_Click);
			// 
			// TransactionsGrid
			// 
			this.TransactionsGrid.AllowNavigation = false;
			this.TransactionsGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TransactionsGrid, "DepositBatchLines.Transactions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).IsSelected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).AH_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).CurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).BankCurrencyDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).TotalDepositAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).AH_ReceiptType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).AH_ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).AH_ChequeDrawer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).AH_DrawerBank)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).AH_DrawerBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).BankCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DepositBatchTransactionLine)(((System.Collections.IList)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).Transactions)).SyncRoot)).BranchCode)));
			this.TransactionsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|43493a81-57b9-4f8f-a5fd-f5a71a7a8c7d", "Selected");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsSelected";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.ColumnName = "AH_Ledger";
			zTextBoxColumnStyleInfo2.ColumnName = "AH_TransactionType";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|91038885-e29e-4bc7-a011-e931cd7f1042", "Transaction Num.", "Transaction Number");
			zTextBoxColumnStyleInfo3.ColumnName = "AH_TransactionNum";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|f2116a54-c5c8-4e69-b53c-200fea554dcd", "Transaction Date");
			zDateEditColumnStyleInfo1.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|93fb529d-b926-486f-87a4-49f78114997b", "Currency");
			zTextBoxColumnStyleInfo4.ColumnName = "CurrencyCode";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "BankCurrencyDecimals";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|f343618d-4ab7-4d9e-87bd-78fb521dd660", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "TotalDepositAmount";
			zTextBoxColumnStyleInfo5.ColumnName = "AH_ReceiptType";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|58e84027-72c0-4f63-b4e2-fe316edd4146", "Check Or Reference");
			zTextBoxColumnStyleInfo6.ColumnName = "AH_ChequeOrReference";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|a4d72765-be31-42e9-b32a-123e16d8df55", "Drawer Name");
			zTextBoxColumnStyleInfo7.ColumnName = "AH_ChequeDrawer";
			zTextBoxColumnStyleInfo8.ColumnName = "AH_DrawerBank";
			zTextBoxColumnStyleInfo9.ColumnName = "AH_DrawerBranch";
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|58261031-692d-4287-9404-4393f07ccabb", "Bank Code");
			zTextBoxColumnStyleInfo10.ColumnName = "BankCode";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|9f73e518-add7-4dea-8905-f63b9b17c9c4", "Branch");
			zTextBoxColumnStyleInfo11.ColumnName = "BranchCode";
			this.TransactionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.TransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.TransactionsGrid.GridId = "c172b1f8-338b-4175-8a2b-75c8f73d0fe6";
			this.TransactionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransactionsGrid.LayoutKey = "zGrid1";
			this.TransactionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 19, true);
			this.TransactionsGrid.Name = "TransactionsGrid";
			this.TransactionsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.TransactionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 47, true);
			this.TransactionsGrid.TabIndex = 0;
			// 
			// BankCodeLabel
			// 
			this.BankCodeLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BankCodeLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BankCodeLabel, "DepositBatchLines.BankCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).BankCode)));
			this.BankCodeLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|3b66ba7c-58d7-4bc5-be2d-950819ce6d6c", "Bank Code");
			this.BankCodeLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.BankCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(646, 78, true);
			this.BankCodeLabel.Name = "BankCodeLabel";
			this.BankCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 13, true);
			this.BankCodeLabel.TabIndex = 4;
			this.BankCodeLabel.TextChanged += new EventHandler(this.BankCodeLabel_TextChanged);
			// 
			// BankDetailsGroupBox
			// 
			this.BankDetailsGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BankDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|a6699c6d-9764-4f0e-9f0c-d89626306ef2", "Bank Details");
			this.BankDetailsGroupBox.Controls.Add(this.BankUnSelectAllButton);
			this.BankDetailsGroupBox.Controls.Add(this.BankSelectAllButton);
			this.BankDetailsGroupBox.Controls.Add(this.BankGrid);
			this.BankDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 132, true);
			this.BankDetailsGroupBox.Name = "BankDetailsGroupBox";
			this.BankDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(743, 126, true);
			this.BankDetailsGroupBox.TabIndex = 3;
			this.BankDetailsGroupBox.TabStop = false;
			// 
			// BankUnSelectAllButton
			// 
			this.BankUnSelectAllButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BankUnSelectAllButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|d21aed93-ff6c-44b7-b860-889c85402a16", "Deselect All");
			this.BankUnSelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 96, true);
			this.BankUnSelectAllButton.Name = "BankUnSelectAllButton";
			this.BankUnSelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 24, true);
			this.BankUnSelectAllButton.TabIndex = 2;
			this.BankUnSelectAllButton.UseVisualStyleBackColor = true;
			this.BankUnSelectAllButton.Click += new EventHandler(this.BankUnSelectAllButton_Click);
			// 
			// BankSelectAllButton
			// 
			this.BankSelectAllButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BankSelectAllButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|bf4a576a-84de-4cc1-a8e2-5370c1c91f0a", "Select All");
			this.BankSelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 96, true);
			this.BankSelectAllButton.Name = "BankSelectAllButton";
			this.BankSelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 24, true);
			this.BankSelectAllButton.TabIndex = 1;
			this.BankSelectAllButton.UseVisualStyleBackColor = true;
			this.BankSelectAllButton.Click += new EventHandler(this.BankSelectAllButton_Click);
			// 
			// BankGrid
			// 
			this.BankGrid.AllowNavigation = false;
			this.BankGrid.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BankGrid, "DepositBatchLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).IsSelected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).BankCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).BankName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).CurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).BankBSBNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).BankAccountNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CashBook.DepositBatch.DepositBatch)(((System.Collections.IList)(((DepositBatchParent)(null)).DepositBatchLines)).SyncRoot)).BankAddress)));
			this.BankGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|7564fb86-5295-4e25-80fe-a39fc37073aa", "Selected");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsSelected";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|7afdf6fd-45cd-421b-8607-0905facc37d7", "Bank Code");
			zTextBoxColumnStyleInfo12.ColumnName = "BankCode";
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|6f810677-3d44-4b07-9a52-b58f4b4eb330", "Bank Name");
			zTextBoxColumnStyleInfo13.ColumnName = "BankName";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|abcd216e-22f6-4fb1-81ca-2a8ccd5c1153", "Currency");
			zTextBoxColumnStyleInfo14.ColumnName = "CurrencyCode";
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|b5b33a8c-1188-4e92-a519-2c78b250eee7", "BSB Number");
			zTextBoxColumnStyleInfo15.ColumnName = "BankBSBNumber";
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|e53f6a10-d6b0-48a2-a1b3-5a85327c6a1a", "Account Number");
			zTextBoxColumnStyleInfo16.ColumnName = "BankAccountNumber";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|bfecd8de-4d6f-4316-89f1-60fe9549771d", "Address");
			zTextBoxColumnStyleInfo17.ColumnName = "BankAddress";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.BankGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.BankGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.BankGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.BankGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.BankGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.BankGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.BankGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.BankGrid.GridId = "a36753ba-276e-4f04-beba-b2f8de701330";
			this.BankGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BankGrid.LayoutKey = "zGrid1";
			this.BankGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 19, true);
			this.BankGrid.Name = "BankGrid";
			this.BankGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.BankGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 71, true);
			this.BankGrid.TabIndex = 0;
			// 
			// CompanyFilterGroupBox
			// 
			this.CompanyFilterGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|afa38475-f880-4e24-9ea5-d07df4ce06e0", "Filter by Company or Branch");
			this.CompanyFilterGroupBox.Controls.Add(this.BranchGuidFindBox);
			this.CompanyFilterGroupBox.Controls.Add(this.BranchRadioButton);
			this.CompanyFilterGroupBox.Controls.Add(this.CompanyRadioButton);
			this.CompanyFilterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 80, true);
			this.CompanyFilterGroupBox.Name = "CompanyFilterGroupBox";
			this.CompanyFilterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 46, true);
			this.CompanyFilterGroupBox.TabIndex = 1;
			this.CompanyFilterGroupBox.TabStop = false;
			// 
			// BranchGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "BranchFilterPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((DepositBatchParent)(null)).BranchFilterPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((DepositBatchParent)(null)).Branches)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BranchGuidFindBox, false);
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 19, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.PopupCaption = null;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 21, true);
			this.BranchGuidFindBox.TabIndex = 2;
			// 
			// BranchRadioButton
			// 
			this.BranchRadioButton.AutoCheck = false;
			this.BranchRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BranchRadioButton, "FilterByBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((DepositBatchParent)(null)).FilterByBranch)));
			this.BranchRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|6c3e7b50-8a2d-4637-863c-ce433cb16f31", "Branch");
			this.BranchRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BranchRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 19, true);
			this.BranchRadioButton.Name = "BranchRadioButton";
			this.BranchRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 17, true);
			this.BranchRadioButton.TabIndex = 1;
			this.BranchRadioButton.UseVisualStyleBackColor = true;
			// 
			// CompanyRadioButton
			// 
			this.CompanyRadioButton.AutoCheck = false;
			this.CompanyRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CompanyRadioButton, "FilterByCompany");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((DepositBatchParent)(null)).FilterByCompany)));
			this.CompanyRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|c72f146e-09c2-4f6c-a294-fb2927ba9c07", "Company");
			this.CompanyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CompanyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 19, true);
			this.CompanyRadioButton.Name = "CompanyRadioButton";
			this.CompanyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.CompanyRadioButton.TabIndex = 0;
			this.CompanyRadioButton.UseVisualStyleBackColor = true;
			// 
			// CurrencyFilterGroupBox
			// 
			this.CurrencyFilterGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.CurrencyFilterGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|738877d9-bc65-4c68-930a-0ffb9479c223", "Filter by Currency");
			this.CurrencyFilterGroupBox.Controls.Add(this.AllCurrenciesRadioButton);
			this.CurrencyFilterGroupBox.Controls.Add(this.ForeignRadioButton);
			this.CurrencyFilterGroupBox.Controls.Add(this.LocalCurrencyRadioButton);
			this.CurrencyFilterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 80, true);
			this.CurrencyFilterGroupBox.Name = "CurrencyFilterGroupBox";
			this.CurrencyFilterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 46, true);
			this.CurrencyFilterGroupBox.TabIndex = 2;
			this.CurrencyFilterGroupBox.TabStop = false;
			// 
			// AllCurrenciesRadioButton
			// 
			this.AllCurrenciesRadioButton.AutoCheck = false;
			this.AllCurrenciesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AllCurrenciesRadioButton, "FilterByAllCurrencies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((DepositBatchParent)(null)).FilterByAllCurrencies)));
			this.AllCurrenciesRadioButton.Checked = true;
			this.AllCurrenciesRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|e9a9b93c-dcda-48dd-9ce2-f53eb488a54d", "All Currencies");
			this.AllCurrenciesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AllCurrenciesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.AllCurrenciesRadioButton.Name = "AllCurrenciesRadioButton";
			this.AllCurrenciesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 17, true);
			this.AllCurrenciesRadioButton.TabIndex = 0;
			this.AllCurrenciesRadioButton.UseVisualStyleBackColor = true;
			// 
			// ForeignRadioButton
			// 
			this.ForeignRadioButton.AutoCheck = false;
			this.ForeignRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ForeignRadioButton, "FilterByForeignCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((DepositBatchParent)(null)).FilterByForeignCurrency)));
			this.ForeignRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|0860d3c8-db76-4dff-90b5-357d34e27bb8", "Foreign Currency");
			this.ForeignRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ForeignRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 19, true);
			this.ForeignRadioButton.Name = "ForeignRadioButton";
			this.ForeignRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 17, true);
			this.ForeignRadioButton.TabIndex = 2;
			this.ForeignRadioButton.UseVisualStyleBackColor = true;
			// 
			// LocalCurrencyRadioButton
			// 
			this.LocalCurrencyRadioButton.AutoCheck = false;
			this.LocalCurrencyRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LocalCurrencyRadioButton, "FilterByLocalCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((DepositBatchParent)(null)).FilterByLocalCurrency)));
			this.LocalCurrencyRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|3dafdcb9-a1d4-402e-bb43-467c2e7f02d6", "Local Currency");
			this.LocalCurrencyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LocalCurrencyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 19, true);
			this.LocalCurrencyRadioButton.Name = "LocalCurrencyRadioButton";
			this.LocalCurrencyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.LocalCurrencyRadioButton.TabIndex = 1;
			this.LocalCurrencyRadioButton.UseVisualStyleBackColor = true;
			// 
			// BatchHeaderGroupBox
			// 
			this.BatchHeaderGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BatchHeaderGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|3234f871-df33-4e35-931b-44a5f58cc971", "Batch Header");
			this.BatchHeaderGroupBox.Controls.Add(this.DepositPostDateDateEdit);
			this.BatchHeaderGroupBox.Controls.Add(this.CancelReasonLabel);
			this.BatchHeaderGroupBox.Controls.Add(this.BatchNumberTextBox);
			this.BatchHeaderGroupBox.Controls.Add(this.DepositDateDateEdit);
			this.BatchHeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 3, true);
			this.BatchHeaderGroupBox.Name = "BatchHeaderGroupBox";
			this.BatchHeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(743, 71, true);
			this.BatchHeaderGroupBox.TabIndex = 0;
			this.BatchHeaderGroupBox.TabStop = false;
			// 
			// DepositPostDateDateEdit
			// 
			this.DepositPostDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepositPostDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepositPostDateDateEdit, "DepositPostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((DepositBatchParent)(null)).DepositPostDate)));
			this.DepositPostDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|756605c5-89c4-41b3-9f7e-e0944714ba49", "Deposit Post Date");
			this.DepositPostDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 21, true);
			this.DepositPostDateDateEdit.Name = "DepositPostDateDateEdit";
			this.DepositPostDateDateEdit.TabIndex = 1;
			// 
			// CancelReasonLabel
			// 
			this.CancelReasonLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|57cf4a46-baf1-43e3-b701-c377f3e873ff", "Cancel Reason");
			this.CancelReasonLabel.ForeColor = System.Drawing.Color.Red;
			this.CancelReasonLabel.IsFontBold = true;
			this.CancelReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 44, true);
			this.CancelReasonLabel.Name = "CancelReasonLabel";
			this.CancelReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 17, true);
			this.CancelReasonLabel.TabIndex = 3;
			// 
			// BatchNumberTextBox
			// 
			this.BatchNumberTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BatchNumberTextBox, "BatchNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((DepositBatchParent)(null)).BatchNumber)));
			this.BatchNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|fec59331-9f82-4089-ac7c-76721875530c", "Batch Number:");
			this.BatchNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(648, 21, true);
			this.BatchNumberTextBox.Name = "BatchNumberTextBox";
			this.BatchNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.BatchNumberTextBox.TabIndex = 2;
			// 
			// DepositDateDateEdit
			// 
			this.DepositDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepositDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepositDateDateEdit, "DepositDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((DepositBatchParent)(null)).DepositDate)));
			this.DepositDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|e10fb8fc-9634-4bba-885a-53641b85cd71", "Deposit Date", "The creation date of this deposit batch.");
			this.DepositDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 21, true);
			this.DepositDateDateEdit.Name = "DepositDateDateEdit";
			this.DepositDateDateEdit.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 538, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 31, true);
			this.BottomPanel.TabIndex = 3;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 3, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 24, true);
			this.PostingButtonsUserControl.TabIndex = 2;
			// 
			// DepositBatchForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 593, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DepositBatchForm|947c302d-85a9-42b3-a12e-bf2b69b1e6ef", "Deposit Batch");
			this.Controls.Add(this.BatchTabControl);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(DepositBatchParent);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.CashBook.DepositBatch.DepositBatchParent";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 620, true);
			this.Name = "DepositBatchForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.BatchTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BatchTabControl.ResumeLayout(false);
			this.zTabPage1.ResumeLayout(false);
			this.zPanel1.ResumeLayout(false);
			this.SummaryGroupBox.ResumeLayout(false);
			this.SummaryTabControl.ResumeLayout(false);
			this.BatchSummarySelectedTabPage.ResumeLayout(false);
			this.BatchSummarySelectedTabPage.PerformLayout();
			this.BatchSummaryNotSelectedTabPage.ResumeLayout(false);
			this.BatchSummaryNotSelectedTabPage.PerformLayout();
			this.TransactionsGroupBox.ResumeLayout(false);
			this.TransactionsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransactionsGrid)).EndInit();
			this.BankDetailsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BankGrid)).EndInit();
			this.CompanyFilterGroupBox.ResumeLayout(false);
			this.CompanyFilterGroupBox.PerformLayout();
			this.CurrencyFilterGroupBox.ResumeLayout(false);
			this.CurrencyFilterGroupBox.PerformLayout();
			this.BatchHeaderGroupBox.ResumeLayout(false);
			this.BatchHeaderGroupBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private void BankCodeLabel_TextChanged(object sender, EventArgs e)
		{
			SummaryTabControl.TabPages[0].Text = Res.GetString("DepositBatchForm|E1BEB27B-D73D-4b21-9DD8-D8E97DAD8716", "Selected Transactions ({0})", ((ZLabel)sender).Text);
			SummaryTabControl.TabPages[1].Text = Res.GetString("DepositBatchForm|375B297B-B57B-434f-9915-412A8172DF94", "Not Selected Transactions ({0})", ((ZLabel)sender).Text);
			TransactionsGroupBox.Text = Res.GetString("DepositBatchForm|139C4315-C6FB-4bf2-8967-48B3FFF9A04D", "Receipt Transactions ({0})", ((ZLabel)sender).Text);
		}

		#endregion

	}
}