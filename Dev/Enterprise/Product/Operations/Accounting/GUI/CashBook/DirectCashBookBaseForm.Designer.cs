using System;
using CargoWise.Common;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class DirectCashBookBaseForm
	{
		#region Windows Form Designer generated code

		private ZTemplateTabControl zTabControl1;
		private ZLogsTabPage zEventTabPage1;
		protected ZTabPage CashBookTransactionTabPage1;
		protected ZGroupBox CashBookLineGroupBox;
		protected ZGrid CashBookLineBoundGrid;
		protected ZTextBox AH_DrawerBranchTextBox;
		protected ZTextBox AH_DrawerBankTextBox;
		protected ZTextBox AH_ChequeDrawerTextBox;
		private ZGroupBox ChequeBoxGroupBox;
		private ZStmNoteTabPage zStmNoteTabPage1;
		protected ZCalcEdit AH_NumberOfSupportingDocumentsCalcEdit;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			CashBookTransactionTabPage1.RunWhenBindingOrFirstShown(delegate
			{
				CashBookLineBoundGrid.ColumnLayoutContext = CashBookLineGridContext.Direct.ToString();
			});
			InitializeAdditionalCaptions();
		}

		void InitializeAdditionalCaptions()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				CashBookTransactionGUIHelper.ConfigureColumns(CashBookLineBoundGrid, Direct.AH_TransactionType, Direct);
			}
		}

		void InitTaxBranchSetting()
		{
			AH_GB_TaxBranchGuidFindBox.Visible = AccountingMasterFilesUtils.IsTaxBranchApplicable;
			CashBookLineBoundGrid.SetAvailability(AccountingMasterFilesUtils.IsTaxBranchApplicable, [DependentTransactionLine.Schema.AL_GB_TaxBranch, DependentTransactionLine.Schema.TaxBranchName]);
		}

		new void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo7 = new ZGuidFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo8 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			this.AH_NumberOfSupportingDocumentsCalcEdit = new ZCalcEdit();
			this.zTabControl1 = new ZTemplateTabControl();
			this.CashBookTransactionTabPage1 = new ZTabPage();
			this.CashBookLineGroupBox = new ZGroupBox();
			this.CashBookLineBoundGrid = new ZGrid();
			this.BottomDetailsPanel = new ZPanel();
			this.AH_LocalExTaxAmountCalcFindBox = new ZCalcFindBox();
			this.AH_OSTotalAmountCalcFindBox = new ZCalcFindBox();
			this.TopPanel = new ZPanel();
			this.AH_GB_TaxBranchGuidFindBox = new ZGuidFindBox();
			this.zDropEditPlaceOfSupply = new ZDropEdit();
			this.AH_ReceiptTypeDropEdit = new ZDropEdit();
			this.AH_PostDateDateEdit = new ZDateEdit();
			this.AH_ChequeOrReferenceTextBox = new ZTextBox();
			this.AH_TransactionNumTextBox = new ZTextBox();
			this.AH_DescTextBox = new ZTextBox();
			this.ExchangeRateControl = new ZExchangeRateControl();
			this.ChequeBookFindBox = new ZGuidFindBox();
			this.BankAccountsFindBox = new ZGuidFindBox();
			this.AH_InvoiceDateDateEdit = new ZDateEdit();
			this.ChequeOrReferenceLabel = new ZLabel();
			this.AdditionalDetailsPanel = new ZPanel();
			this.DirectCashBookSubAccountsGroupBox = new ZGroupBox();
			this.DirectCashBookSubAccountsControl = new DirectCashBookSubAccountsControl();
			this.ChequeBoxGroupBox = new ZGroupBox();
			this.AH_DrawerBranchTextBox = new ZTextBox();
			this.AH_DrawerBankTextBox = new ZTextBox();
			this.AH_ChequeDrawerTextBox = new ZTextBox();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			this.BottomPanel = new ZPanel();
			this.PostingButtonsUserControl = new ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zTabControl1.SuspendLayout();
			this.CashBookTransactionTabPage1.SuspendLayout();
			this.CashBookLineGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CashBookLineBoundGrid)).BeginInit();
			this.CashBookLineBoundGrid.SuspendLayout();
			this.BottomDetailsPanel.SuspendLayout();
			this.AH_LocalExTaxAmountCalcFindBox.SuspendLayout();
			this.AH_OSTotalAmountCalcFindBox.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.AH_GB_TaxBranchGuidFindBox.SuspendLayout();
			this.zDropEditPlaceOfSupply.SuspendLayout();
			this.AH_ReceiptTypeDropEdit.SuspendLayout();
			this.AH_PostDateDateEdit.SuspendLayout();
			this.ExchangeRateControl.SuspendLayout();
			this.ChequeBookFindBox.SuspendLayout();
			this.BankAccountsFindBox.SuspendLayout();
			this.AH_InvoiceDateDateEdit.SuspendLayout();
			this.AdditionalDetailsPanel.SuspendLayout();
			this.DirectCashBookSubAccountsGroupBox.SuspendLayout();
			this.DirectCashBookSubAccountsControl.SuspendLayout();
			this.ChequeBoxGroupBox.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 537, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(413);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(414);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.CashBook.DirectPayment.DirectPayment);
			// 
			// AH_NumberOfSupportingDocumentsCalcEdit
			// 
			this.AH_NumberOfSupportingDocumentsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_NumberOfSupportingDocumentsCalcEdit, "AH_NumberOfSupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CashBook.DirectPayment.DirectPayment)(null)).AH_NumberOfSupportingDocuments)));
			this.AH_NumberOfSupportingDocumentsCalcEdit.CaptionResourceString = null;
			this.AH_NumberOfSupportingDocumentsCalcEdit.DecimalPlaces = 2;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 81, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.Name = "AH_NumberOfSupportingDocumentsCalcEdit";
			this.AH_NumberOfSupportingDocumentsCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.TabIndex = 9;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Text = "0";
			this.AH_NumberOfSupportingDocumentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.zTabControl1.Controls.Add(this.CashBookTransactionTabPage1);
			this.zTabControl1.Controls.Add(this.zStmNoteTabPage1);
			this.zTabControl1.Controls.Add(this.zEventTabPage1);
			this.zTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 505, true);
			this.zTabControl1.TabIndex = 0;
			// 
			// CashBookTransactionTabPage1
			// 
			this.CashBookTransactionTabPage1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|031c0efa-883c-4c00-8372-0120d2c7817d", "XXXXXX Details");
			this.CashBookTransactionTabPage1.Controls.Add(this.CashBookLineGroupBox);
			this.CashBookTransactionTabPage1.Controls.Add(this.TopPanel);
			this.CashBookTransactionTabPage1.Controls.Add(this.AdditionalDetailsPanel);
			this.CashBookTransactionTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CashBookTransactionTabPage1.Name = "CashBookTransactionTabPage1";
			this.CashBookTransactionTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 478, true);
			this.CashBookTransactionTabPage1.TabIndex = 0;
			// 
			// CashBookLineGroupBox
			// 
			this.CashBookLineGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|0c1b2665-bf58-4ad2-916e-de3a0fe8196d", "XXXXXX Details");
			this.CashBookLineGroupBox.Controls.Add(this.CashBookLineBoundGrid);
			this.CashBookLineGroupBox.Controls.Add(this.BottomDetailsPanel);
			this.CashBookLineGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CashBookLineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 128, true);
			this.CashBookLineGroupBox.Name = "CashBookLineGroupBox";
			this.CashBookLineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 246, true);
			this.CashBookLineGroupBox.TabIndex = 2;
			this.CashBookLineGroupBox.TabStop = false;
			// 
			// CashBookLineBoundGrid
			// 
			this.CashBookLineBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CashBookLineBoundGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_AG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_Calc_RXDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_OSExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_AT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_TaxDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_A9_VATClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_Calc_RXDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_OSTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_Calc_RXDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_OverseasTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_OSGSTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_OSExtraTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_LocalGSTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_LocalExtraTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_Calc_InputGSTVATRecoverablePercentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_OSTaxAmount_Recoverable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_OSTaxAmount_NotRecoverable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_LocalTaxAmount_Recoverable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_LocalTaxAmount_NotRecoverable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_GovtChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_LocalTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_LocalTotalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_LocalExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).BranchName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).DepartmentDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_PlaceOfSupply)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_Calc_FirstSubClassParent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_Calc_FirstSubClassParentId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_Calc_SecondSubClassParent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_Calc_SecondSubClassParentId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_SupplyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AL_GB_TaxBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).TaxBranchName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AlternateGLAccountNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DependentTransactionLine)(((System.Collections.IList)(((Business.CashBook.DirectPayment.DirectPayment)(null)).Lines)).SyncRoot)).AlternateGLAccountDescription)));
			this.CashBookLineBoundGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AL_AG";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AL_GB";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|c074d8e5-1efe-43d0-b6f1-9ccf2d64f4a6", "Dept");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "AL_GE";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "AL_Desc";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "AL_Calc_RXDecimals";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|b7ac3b7f-f6ee-4cba-b9e9-71c9de464f2d", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "AL_OSExTaxAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "AL_AT";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "AL_TaxDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.ColumnName = "AL_A9_VATClass";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = "AL_Calc_RXDecimals";
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|5c2eb591-ce0f-427b-bb7d-099bcfde12c3", "Tax Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "AL_OSTaxAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = "AL_Calc_RXDecimals";
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|cbb95093-d603-4613-b094-9f5f46f32f95", "Total");
			zCalcEditColumnStyleInfo3.ColumnName = "AL_OverseasTotal";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "AL_OSGSTAmount";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "AL_OSExtraTaxAmount";
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "AL_LocalGSTAmount";
			zCalcEditColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "AL_LocalExtraTaxAmount";
			zCalcEditColumnStyleInfo7.IsReadOnly = true;
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "AL_Calc_InputGSTVATRecoverablePercentage";
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "AL_OSTaxAmount_Recoverable";
			zCalcEditColumnStyleInfo9.IsVisible = false;
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.ColumnName = "AL_OSTaxAmount_NotRecoverable";
			zCalcEditColumnStyleInfo10.IsVisible = false;
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.ColumnName = "AL_LocalTaxAmount_Recoverable";
			zCalcEditColumnStyleInfo11.IsVisible = false;
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.ColumnName = "AL_LocalTaxAmount_NotRecoverable";
			zCalcEditColumnStyleInfo12.IsVisible = false;
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c1054e67-9353-44a3-a538-13f3c566b51c", "Government Charge Code");
			zTextBoxColumnStyleInfo2.ColumnName = "AL_GovtChargeCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|830A0660-1056-4B47-9147-88F819E6FE75", "Local Tax Amount");
			zCalcEditColumnStyleInfo13.ColumnName = "AL_LocalTaxAmount";
			zCalcEditColumnStyleInfo13.IsReadOnly = true;
			zCalcEditColumnStyleInfo13.IsVisible = false;
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|954720DA-1511-4ABA-B20B-3DEF703A36B4", "Local Total Amount");
			zCalcEditColumnStyleInfo14.ColumnName = "AL_LocalTotalAmount";
			zCalcEditColumnStyleInfo14.IsReadOnly = true;
			zCalcEditColumnStyleInfo14.IsVisible = false;
			zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|106BC8F5-C25C-4D2B-909A-25352823AB3D", "Local Amount");
			zCalcEditColumnStyleInfo15.ColumnName = "AL_LocalExTaxAmount";
			zCalcEditColumnStyleInfo15.IsVisible = false;
			zCalcEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|c4abe6c1-4868-4b2c-be38-50c6b0f47184", "Branch Name");
			zTextBoxColumnStyleInfo3.ColumnName = "BranchName";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|427c518a-a56b-4889-a104-a49f2f6d9880", "Department Description");
			zTextBoxColumnStyleInfo4.ColumnName = "DepartmentDescription";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "AL_PlaceOfSupply";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "AL_Calc_FirstSubClassParent";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo6.ColumnName = "AL_Calc_FirstSubClassParentId";
			zGuidFindBoxColumnStyleInfo6.IsVisible = false;
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "AL_Calc_SecondSubClassParent";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo7.ColumnName = "AL_Calc_SecondSubClassParentId";
			zGuidFindBoxColumnStyleInfo7.IsVisible = false;
			zGuidFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|45285413-03CB-4C9E-8B47-DA0EE83C82B1}", "Supply Type");
			zDropEditColumnStyleInfo2.ColumnName = "AL_SupplyType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo8.ColumnName = "AL_GB_TaxBranch";
			zGuidFindBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5bd4bead-af92-49d4-bdec-4dcc0b1aee82", "Tax Branch Name");
			zTextBoxColumnStyleInfo7.ColumnName = "TaxBranchName";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DD6A2861-9B69-4B04-9FD5-C3B97CCCE8EB", "Alternate Account");
			zTextBoxColumnStyleInfo8.ColumnName = "AlternateGLAccountNumber";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("03723EF6-E68A-455D-9A61-C1A72BD6F9F2", "Alternate Account Name");
			zTextBoxColumnStyleInfo9.ColumnName = "AlternateGLAccountDescription";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo7);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo8);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.CashBookLineBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.CashBookLineBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CashBookLineBoundGrid.GridId = "8f0567c2-9947-4d8c-a04c-a8825feeac17";
			this.CashBookLineBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CashBookLineBoundGrid.LayoutKey = "zGrid1";
			this.CashBookLineBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CashBookLineBoundGrid.Name = "CashBookLineBoundGrid";
			this.CashBookLineBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 193, true);
			this.CashBookLineBoundGrid.TabIndex = 0;
			// 
			// BottomDetailsPanel
			// 
			this.BottomDetailsPanel.Controls.Add(this.AH_LocalExTaxAmountCalcFindBox);
			this.BottomDetailsPanel.Controls.Add(this.AH_OSTotalAmountCalcFindBox);
			this.BottomDetailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 209, true);
			this.BottomDetailsPanel.Name = "BottomDetailsPanel";
			this.BottomDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 34, true);
			this.BottomDetailsPanel.TabIndex = 1;
			// 
			// AH_LocalExTaxAmountCalcFindBox
			// 
			this.AH_LocalExTaxAmountCalcFindBox.AllowDrop = true;
			this.AH_LocalExTaxAmountCalcFindBox.BindToAmount = "AH_LocalTotalAmount";
			this.AH_LocalExTaxAmountCalcFindBox.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.AH_LocalExTaxAmountCalcFindBox.BindToUnit = "AH_Calc_LocalRX";
			this.AH_LocalExTaxAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(669, 6, true);
			this.AH_LocalExTaxAmountCalcFindBox.Name = "AH_LocalExTaxAmountCalcFindBox";
			this.AH_LocalExTaxAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.AH_LocalExTaxAmountCalcFindBox.TabIndex = 1;
			// 
			// AH_OSTotalAmountCalcFindBox
			// 
			this.AH_OSTotalAmountCalcFindBox.AllowDrop = true;
			this.AH_OSTotalAmountCalcFindBox.BindToAmount = "AH_OSTotalAmount";
			this.AH_OSTotalAmountCalcFindBox.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.AH_OSTotalAmountCalcFindBox.BindToUnit = "AH_Readonly_RXCode";
			this.AH_OSTotalAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_OSTotalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 6, true);
			this.AH_OSTotalAmountCalcFindBox.Name = "AH_OSTotalAmountCalcFindBox";
			this.AH_OSTotalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.AH_OSTotalAmountCalcFindBox.TabIndex = 0;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.AH_GB_TaxBranchGuidFindBox);
			this.TopPanel.Controls.Add(this.zDropEditPlaceOfSupply);
			this.TopPanel.Controls.Add(this.AH_ReceiptTypeDropEdit);
			this.TopPanel.Controls.Add(this.AH_PostDateDateEdit);
			this.TopPanel.Controls.Add(this.AH_ChequeOrReferenceTextBox);
			this.TopPanel.Controls.Add(this.AH_TransactionNumTextBox);
			this.TopPanel.Controls.Add(this.AH_DescTextBox);
			this.TopPanel.Controls.Add(this.ExchangeRateControl);
			this.TopPanel.Controls.Add(this.ChequeBookFindBox);
			this.TopPanel.Controls.Add(this.BankAccountsFindBox);
			this.TopPanel.Controls.Add(this.AH_InvoiceDateDateEdit);
			this.TopPanel.Controls.Add(this.ChequeOrReferenceLabel);
			this.TopPanel.Controls.Add(this.AH_NumberOfSupportingDocumentsCalcEdit);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 128, true);
			this.TopPanel.TabIndex = 0;
			// 
			// AH_GB_TaxBranchGuidFindBox
			// 
			this.AH_GB_TaxBranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AH_GB_TaxBranchGuidFindBox, "AH_GB_TaxBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.CashBook.DirectPayment.DirectPayment)(null)).AH_GB_TaxBranch)));
			this.AH_GB_TaxBranchGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("af0d5b80-b6fd-44f4-b2f7-2af5601bb556", "Tax Branch");
			this.AH_GB_TaxBranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(664, 78, true);
			this.AH_GB_TaxBranchGuidFindBox.Name = "AH_GB_TaxBranchGuidFindBox";
			this.AH_GB_TaxBranchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AH_GB_TaxBranchGuidFindBox.ParentType = null;
			this.AH_GB_TaxBranchGuidFindBox.ShowDescriptionBox = false;
			this.AH_GB_TaxBranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.AH_GB_TaxBranchGuidFindBox.TabIndex = 10;
			this.AH_GB_TaxBranchGuidFindBox.Visible = false;
			// 
			// zDropEditPlaceOfSupply
			// 
			this.zDropEditPlaceOfSupply.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEditPlaceOfSupply, "AH_PlaceOfSupply");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.CashBook.DirectPayment.DirectPayment)(null)).AH_PlaceOfSupply)));
			this.zDropEditPlaceOfSupply.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d137fa83-1966-433e-a7e3-018aa7a36572", "Place of Supply");
			this.zDropEditPlaceOfSupply.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(664, 102, true);
			this.zDropEditPlaceOfSupply.Name = "zDropEditPlaceOfSupply";
			this.zDropEditPlaceOfSupply.ShouldResizeByMaxLength = true;
			this.zDropEditPlaceOfSupply.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.zDropEditPlaceOfSupply.TabIndex = 12;
			// 
			// AH_ReceiptTypeDropEdit
			// 
			this.AH_ReceiptTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AH_ReceiptTypeDropEdit, "AH_ReceiptType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.CashBook.DirectPayment.DirectPayment)(null)).AH_ReceiptType)));
			this.AH_ReceiptTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(664, 29, true);
			this.AH_ReceiptTypeDropEdit.Name = "AH_ReceiptTypeDropEdit";
			this.AH_ReceiptTypeDropEdit.ShouldResizeByMaxLength = true;
			this.AH_ReceiptTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.AH_ReceiptTypeDropEdit.TabIndex = 4;
			// 
			// AH_PostDateDateEdit
			// 
			this.AH_PostDateDateEdit.AllowDrop = true;
			this.AH_PostDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_PostDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_PostDateDateEdit, "AH_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.CashBook.DirectPayment.DirectPayment)(null)).AH_PostDate)));
			this.AH_PostDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|0b7fb8da-9e4d-4b03-a025-07559b69273e", "Post Date");
			this.AH_PostDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 6, true);
			this.AH_PostDateDateEdit.Name = "AH_PostDateDateEdit";
			this.AH_PostDateDateEdit.TabIndex = 1;
			// 
			// AH_ChequeOrReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_ChequeOrReferenceTextBox, "AH_ChequeOrReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CashBook.DirectPayment.DirectPayment)(null)).AH_ChequeOrReference)));
			this.AH_ChequeOrReferenceTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AH_ChequeOrReferenceTextBox, false);
			this.AH_ChequeOrReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(664, 54, true);
			this.AH_ChequeOrReferenceTextBox.Name = "AH_ChequeOrReferenceTextBox";
			this.AH_ChequeOrReferenceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AH_ChequeOrReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.AH_ChequeOrReferenceTextBox.TabIndex = 7;
			// 
			// AH_TransactionNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_TransactionNumTextBox, "AH_TransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CashBook.DirectPayment.DirectPayment)(null)).AH_TransactionNum)));
			this.AH_TransactionNumTextBox.CaptionResourceString = null;
			this.AH_TransactionNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(664, 6, true);
			this.AH_TransactionNumTextBox.Name = "AH_TransactionNumTextBox";
			this.AH_TransactionNumTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AH_TransactionNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.AH_TransactionNumTextBox.TabIndex = 2;
			// 
			// AH_DescTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_DescTextBox, "AH_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CashBook.DirectPayment.DirectPayment)(null)).AH_Desc)));
			this.AH_DescTextBox.CaptionResourceString = null;
			this.AH_DescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 102, true);
			this.AH_DescTextBox.Name = "AH_DescTextBox";
			this.AH_DescTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AH_DescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.AH_DescTextBox.TabIndex = 11;
			// 
			// ExchangeRateControl
			// 
			this.ExchangeRateControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExchangeRateControl, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((Business.CashBook.DirectPayment.DirectPayment)(null)).ExchangeRate)));
			this.ExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|ab6efc38-f09e-4f46-9e9a-d5545e73d75f", "Exchange Rate");
			this.ExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 78, true);
			this.ExchangeRateControl.Name = "ExchangeRateControl";
			this.ExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 24, true);
			this.ExchangeRateControl.TabIndex = 8;
			// 
			// ChequeBookFindBox
			// 
			this.ChequeBookFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChequeBookFindBox, "ChequeBookPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.CashBook.DirectPayment.DirectPayment)(null)).ChequeBookPK)));
			this.ChequeBookFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|cb502833-3611-45be-b014-30aaa8e3629f", "Check Book");
			this.ChequeBookFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ChequeBookFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 54, true);
			this.ChequeBookFindBox.Name = "ChequeBookFindBox";
			this.ChequeBookFindBox.ShouldResize = true;
			this.ChequeBookFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ChequeBookFindBox.ParentType = null;
			this.ChequeBookFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.ChequeBookFindBox.TabIndex = 5;
			// 
			// BankAccountsFindBox
			// 
			this.BankAccountsFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BankAccountsFindBox, "AH_AB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.CashBook.DirectPayment.DirectPayment)(null)).AH_AB)));
			this.BankAccountsFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.BankAccountsFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 30, true);
			this.BankAccountsFindBox.Name = "BankAccountsFindBox";
			this.BankAccountsFindBox.ShouldResize = true;
			this.BankAccountsFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BankAccountsFindBox.ParentType = null;
			this.BankAccountsFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.BankAccountsFindBox.TabIndex = 3;
			// 
			// AH_InvoiceDateDateEdit
			// 
			this.AH_InvoiceDateDateEdit.AllowDrop = true;
			this.AH_InvoiceDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_InvoiceDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_InvoiceDateDateEdit, "AH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.CashBook.DirectPayment.DirectPayment)(null)).AH_InvoiceDate)));
			this.AH_InvoiceDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 6, true);
			this.AH_InvoiceDateDateEdit.Name = "AH_InvoiceDateDateEdit";
			this.AH_InvoiceDateDateEdit.TabIndex = 0;
			// 
			// ChequeOrReferenceLabel
			// 
			this.BindingSource.SetBindingMember(this.ChequeOrReferenceLabel, "AH_Calc_ReceiptTypeLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CashBook.DirectPayment.DirectPayment)(null)).AH_Calc_ReceiptTypeLabel)));
			this.ChequeOrReferenceLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|b2168acc-19db-497a-b76f-75d1ec0077ef", "Reference No.");
			this.ChequeOrReferenceLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ChequeOrReferenceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(501, 54, true);
			this.ChequeOrReferenceLabel.Name = "ChequeOrReferenceLabel";
			this.ChequeOrReferenceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.ChequeOrReferenceLabel.TabIndex = 6;
			this.ChequeOrReferenceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// AdditionalDetailsPanel
			// 
			this.AdditionalDetailsPanel.Controls.Add(this.DirectCashBookSubAccountsGroupBox);
			this.AdditionalDetailsPanel.Controls.Add(this.ChequeBoxGroupBox);
			this.AdditionalDetailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AdditionalDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 370, true);
			this.AdditionalDetailsPanel.Name = "AdditionalDetailsPanel";
			this.AdditionalDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 108, true);
			this.AdditionalDetailsPanel.TabIndex = 4;
			// 
			// DirectCashBookSubAccountsGroupBox
			// 
			this.DirectCashBookSubAccountsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("090aade3-9632-407e-a750-5d399d04d750", "Sub Accounts");
			this.DirectCashBookSubAccountsGroupBox.Controls.Add(this.DirectCashBookSubAccountsControl);
			this.DirectCashBookSubAccountsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DirectCashBookSubAccountsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DirectCashBookSubAccountsGroupBox.Name = "DirectCashBookSubAccountsGroupBox";
			this.DirectCashBookSubAccountsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 108, true);
			this.DirectCashBookSubAccountsGroupBox.TabIndex = 4;
			this.DirectCashBookSubAccountsGroupBox.TabStop = false;
			// 
			// DirectCashBookSubAccountsControl
			// 
			this.DirectCashBookSubAccountsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DirectCashBookSubAccountsControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransactionHeaderWithLines)(((Business.CashBook.DirectPayment.DirectPayment)(null)))));
			this.DirectCashBookSubAccountsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DirectCashBookSubAccountsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DirectCashBookSubAccountsControl.Name = "DirectCashBookSubAccountsControl";
			this.DirectCashBookSubAccountsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 89, true);
			this.DirectCashBookSubAccountsControl.TabIndex = 4;
			// 
			// ChequeBoxGroupBox
			// 
			this.ChequeBoxGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|7127bf70-90a4-417a-896a-1b089d8a536a", "Additional Details");
			this.ChequeBoxGroupBox.Controls.Add(this.AH_DrawerBranchTextBox);
			this.ChequeBoxGroupBox.Controls.Add(this.AH_DrawerBankTextBox);
			this.ChequeBoxGroupBox.Controls.Add(this.AH_ChequeDrawerTextBox);
			this.ChequeBoxGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.ChequeBoxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(343, 0, true);
			this.ChequeBoxGroupBox.Name = "ChequeBoxGroupBox";
			this.ChequeBoxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 108, true);
			this.ChequeBoxGroupBox.TabIndex = 3;
			this.ChequeBoxGroupBox.TabStop = false;
			// 
			// AH_DrawerBranchTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_DrawerBranchTextBox, "AH_DrawerBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CashBook.DirectPayment.DirectPayment)(null)).AH_DrawerBranch)));
			this.AH_DrawerBranchTextBox.CaptionResourceString = null;
			this.AH_DrawerBranchTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 68, true);
			this.AH_DrawerBranchTextBox.Name = "AH_DrawerBranchTextBox";
			this.AH_DrawerBranchTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AH_DrawerBranchTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.AH_DrawerBranchTextBox.TabIndex = 2;
			// 
			// AH_DrawerBankTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_DrawerBankTextBox, "AH_DrawerBank");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CashBook.DirectPayment.DirectPayment)(null)).AH_DrawerBank)));
			this.AH_DrawerBankTextBox.CaptionResourceString = null;
			this.AH_DrawerBankTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 44, true);
			this.AH_DrawerBankTextBox.Name = "AH_DrawerBankTextBox";
			this.AH_DrawerBankTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AH_DrawerBankTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.AH_DrawerBankTextBox.TabIndex = 1;
			// 
			// AH_ChequeDrawerTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_ChequeDrawerTextBox, "AH_ChequeDrawer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CashBook.DirectPayment.DirectPayment)(null)).AH_ChequeDrawer)));
			this.AH_ChequeDrawerTextBox.CaptionResourceString = null;
			this.AH_ChequeDrawerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 20, true);
			this.AH_ChequeDrawerTextBox.Name = "AH_ChequeDrawerTextBox";
			this.AH_ChequeDrawerTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AH_ChequeDrawerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.AH_ChequeDrawerTextBox.TabIndex = 0;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 478, true);
			this.zStmNoteTabPage1.TabIndex = 3;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 478, true);
			this.zEventTabPage1.TabIndex = 2;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 505, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 32, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(531, 5, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 0;
			// 
			// DirectCashBookBaseForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectCashBookBaseForm|f6278681-7104-46e8-8d73-ed116585d5c8", "Direct Cash Book Base Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 561, true);
			this.Controls.Add(this.zTabControl1);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Business.CashBook.DirectPayment.DirectPayment);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 600, true);
			this.Name = "DirectCashBookBaseForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.zTabControl1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zTabControl1.ResumeLayout(false);
			this.zTabControl1.PerformLayout();
			this.CashBookTransactionTabPage1.ResumeLayout(false);
			this.CashBookTransactionTabPage1.PerformLayout();
			this.CashBookLineGroupBox.ResumeLayout(false);
			this.CashBookLineGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CashBookLineBoundGrid)).EndInit();
			this.CashBookLineBoundGrid.ResumeLayout(false);
			this.CashBookLineBoundGrid.PerformLayout();
			this.BottomDetailsPanel.ResumeLayout(false);
			this.BottomDetailsPanel.PerformLayout();
			this.AH_LocalExTaxAmountCalcFindBox.ResumeLayout(true);
			this.AH_LocalExTaxAmountCalcFindBox.PerformLayout();
			this.AH_OSTotalAmountCalcFindBox.ResumeLayout(true);
			this.AH_OSTotalAmountCalcFindBox.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.AH_GB_TaxBranchGuidFindBox.ResumeLayout(true);
			this.AH_GB_TaxBranchGuidFindBox.PerformLayout();
			this.zDropEditPlaceOfSupply.ResumeLayout(true);
			this.zDropEditPlaceOfSupply.PerformLayout();
			this.AH_ReceiptTypeDropEdit.ResumeLayout(true);
			this.AH_ReceiptTypeDropEdit.PerformLayout();
			this.AH_PostDateDateEdit.ResumeLayout(true);
			this.AH_PostDateDateEdit.PerformLayout();
			this.ExchangeRateControl.ResumeLayout(true);
			this.ExchangeRateControl.PerformLayout();
			this.ChequeBookFindBox.ResumeLayout(true);
			this.ChequeBookFindBox.PerformLayout();
			this.BankAccountsFindBox.ResumeLayout(true);
			this.BankAccountsFindBox.PerformLayout();
			this.AH_InvoiceDateDateEdit.ResumeLayout(true);
			this.AH_InvoiceDateDateEdit.PerformLayout();
			this.AdditionalDetailsPanel.ResumeLayout(false);
			this.AdditionalDetailsPanel.PerformLayout();
			this.DirectCashBookSubAccountsGroupBox.ResumeLayout(false);
			this.DirectCashBookSubAccountsGroupBox.PerformLayout();
			this.DirectCashBookSubAccountsControl.ResumeLayout(true);
			this.DirectCashBookSubAccountsControl.PerformLayout();
			this.ChequeBoxGroupBox.ResumeLayout(false);
			this.ChequeBoxGroupBox.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}
