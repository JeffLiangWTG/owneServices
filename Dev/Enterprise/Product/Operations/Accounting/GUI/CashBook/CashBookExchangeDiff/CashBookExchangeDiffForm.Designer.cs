using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class CashBookExchangeDiffForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new Container();
			this.AH_NumberOfSupportingDocumentsCalcEdit = new ZArchitecture.ZCalcEdit();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.zTabControl1 = new ZTemplateTabControl();
			this.zTabPage1 = new ZTabPage();
			this.AfterCurrencyAdjustmentGroupBox = new ZGroupBox();
			this.AH_Calc_BeforeCurrencyAdjBalanceLocalBoundCalcFindBox = new ZCalcFindBox();
			this.AH_Calc_AfterCurrencyAdjBalanceLocal = new ZCalcFindBox();
			this.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox = new ZCalcFindBox();
			this.AH_AGGuidFindBox = new ZGuidFindBox();
			this.AH_ABBoundZGuidFindBox = new ZGuidFindBox();
			this.AH_Calc_ExchangeRateBoundCalcFindBox = new ZCalcFindBox();
			this.AH_Calc_InvoiceAmountBoundCalcFindBox = new ZCalcFindBox();
			this.AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox = new ZCalcFindBox();
			this.AH_PostDateBoundDateEdit = new ZDateEdit();
			this.AH_TransactionNumBoundTextBox = new ZArchitecture.ZTextBox();
			this.AH_DescBoundTextBox = new ZArchitecture.ZTextBox();
			this.AH_InvoiceDateBoundDateEdit = new ZDateEdit();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.zTabControl1.SuspendLayout();
			this.zTabPage1.SuspendLayout();
			this.AfterCurrencyAdjustmentGroupBox.SuspendLayout();
			this.AH_Calc_BeforeCurrencyAdjBalanceLocalBoundCalcFindBox.SuspendLayout();
			this.AH_Calc_AfterCurrencyAdjBalanceLocal.SuspendLayout();
			this.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox.SuspendLayout();
			this.AH_AGGuidFindBox.SuspendLayout();
			this.AH_ABBoundZGuidFindBox.SuspendLayout();
			this.AH_Calc_ExchangeRateBoundCalcFindBox.SuspendLayout();
			this.AH_Calc_InvoiceAmountBoundCalcFindBox.SuspendLayout();
			this.AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox.SuspendLayout();
			this.AH_PostDateBoundDateEdit.SuspendLayout();
			this.AH_InvoiceDateBoundDateEdit.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 420, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 22, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(223);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(223);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CashbookExchangeDiff);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 390, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 26, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Controls.Add(this.zTabPage1);
			this.zTabControl1.Controls.Add(this.zStmNoteTabPage1);
			this.zTabControl1.Controls.Add(this.zEventTabPage1);
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 377, true);
			this.zTabControl1.TabIndex = 0;
			// 
			// zTabPage1
			// 
			this.zTabPage1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashBookExchangeDiffForm|96d98715-1f33-40f8-9d59-b1fef3add3b6", "Adjustment");
			this.zTabPage1.Controls.Add(this.AfterCurrencyAdjustmentGroupBox);
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 353, true);
			this.zTabPage1.TabIndex = 0;
			this.zTabPage1.Controls.Add(this.AH_AGGuidFindBox);
			this.zTabPage1.Controls.Add(this.AH_NumberOfSupportingDocumentsCalcEdit);
			this.zTabPage1.Controls.Add(this.AH_ABBoundZGuidFindBox);
			this.zTabPage1.Controls.Add(this.AH_Calc_ExchangeRateBoundCalcFindBox);
			this.zTabPage1.Controls.Add(this.AH_Calc_InvoiceAmountBoundCalcFindBox);
			this.zTabPage1.Controls.Add(this.AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox);
			this.zTabPage1.Controls.Add(this.AH_PostDateBoundDateEdit);
			this.zTabPage1.Controls.Add(this.AH_TransactionNumBoundTextBox);
			this.zTabPage1.Controls.Add(this.AH_DescBoundTextBox);
			this.zTabPage1.Controls.Add(this.AH_InvoiceDateBoundDateEdit);
			// 
			// AfterCurrencyAdjustmentGroupBox
			// 
			this.AfterCurrencyAdjustmentGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashBookExchangeDiffForm|4765ee4a-6744-4761-8bac-ed93803e1308", "Currency Adjustment");
			this.AfterCurrencyAdjustmentGroupBox.Controls.Add(this.AH_Calc_BeforeCurrencyAdjBalanceLocalBoundCalcFindBox);
			this.AfterCurrencyAdjustmentGroupBox.Controls.Add(this.AH_Calc_AfterCurrencyAdjBalanceLocal);
			this.AfterCurrencyAdjustmentGroupBox.Controls.Add(this.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox);
			this.AfterCurrencyAdjustmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 247, true);
			this.AfterCurrencyAdjustmentGroupBox.Name = "AfterCurrencyAdjustmentGroupBox";
			this.AfterCurrencyAdjustmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 97, true);
			this.AfterCurrencyAdjustmentGroupBox.TabIndex = 1;
			this.AfterCurrencyAdjustmentGroupBox.TabStop = false;
			// 
			// AH_Calc_BeforeCurrencyAdjBalanceLocalBoundCalcFindBox
			// 
			this.AH_Calc_BeforeCurrencyAdjBalanceLocalBoundCalcFindBox.BindToAmount = "LocalAmountBeforeAdjustment";
			this.AH_Calc_BeforeCurrencyAdjBalanceLocalBoundCalcFindBox.BindToUnit = "AH_Calc_LocalRX";
			this.AH_Calc_BeforeCurrencyAdjBalanceLocalBoundCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashBookExchangeDiffForm|1e4f2b94-6fc0-4da6-b6cf-eeee87307c5f", "Local Amount Before Adjustment");
			this.AH_Calc_BeforeCurrencyAdjBalanceLocalBoundCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 46, true);
			this.AH_Calc_BeforeCurrencyAdjBalanceLocalBoundCalcFindBox.Name = "AH_Calc_BeforeCurrencyAdjBalanceLocalBoundCalcFindBox";
			this.AH_Calc_BeforeCurrencyAdjBalanceLocalBoundCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.AH_Calc_BeforeCurrencyAdjBalanceLocalBoundCalcFindBox.TabIndex = 1;
			// 
			// AH_Calc_AfterCurrencyAdjBalanceLocal
			// 
			this.AH_Calc_AfterCurrencyAdjBalanceLocal.BindToAmount = "LocalAmountAfterAdjustment";
			this.AH_Calc_AfterCurrencyAdjBalanceLocal.BindToUnit = "AH_Calc_LocalRX";
			this.AH_Calc_AfterCurrencyAdjBalanceLocal.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashBookExchangeDiffForm|571fa657-a76d-40ae-a26c-7a17d805b83a", "Local Amount After Adjustment");
			this.AH_Calc_AfterCurrencyAdjBalanceLocal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 69, true);
			this.AH_Calc_AfterCurrencyAdjBalanceLocal.Name = "AH_Calc_AfterCurrencyAdjBalanceLocal";
			this.AH_Calc_AfterCurrencyAdjBalanceLocal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.AH_Calc_AfterCurrencyAdjBalanceLocal.TabIndex = 2;
			// 
			// AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox
			// 
			this.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox.BindToAmount = "BankCurrencyBalance";
			this.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox.BindToUnit = "AH_RX_NKTransactionCurrency";
			this.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashBookExchangeDiffForm|510b5752-052f-422f-a7f8-540845fba0d6", "Bank Currency Balance Amount");
			this.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 24, true);
			this.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox.Name = "AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox";
			this.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox.TabIndex = 0;
			// 
			// AH_NumberOfSupportingDocumentsCalcEdit
			// 
			this.AH_NumberOfSupportingDocumentsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_NumberOfSupportingDocumentsCalcEdit, "AH_NumberOfSupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ARAP.Contra)(null)).AH_NumberOfSupportingDocuments)));
			this.AH_NumberOfSupportingDocumentsCalcEdit.DecimalPlaces = 2;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 41, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.Name = "AH_NumberOfSupportingDocumentsCalcEdit";
			this.AH_NumberOfSupportingDocumentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 18, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.TabIndex = 3;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Text = "0";
			this.AH_NumberOfSupportingDocumentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;//
			// 
			// AH_ABBoundZGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.AH_ABBoundZGuidFindBox, "AH_AB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CashbookExchangeDiff)(null)).AH_AB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CashbookExchangeDiff)(null)).BankAccounts)));
			this.AH_ABBoundZGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 63, true);
			this.AH_ABBoundZGuidFindBox.Name = "AH_ABBoundZGuidFindBox";
			this.AH_ABBoundZGuidFindBox.PopupCaption = null;
			this.AH_ABBoundZGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 18, true);
			this.AH_ABBoundZGuidFindBox.TabIndex = 4;
			// 
			// AH_Calc_ExchangeRateBoundCalcFindBox
			// 
			this.AH_Calc_ExchangeRateBoundCalcFindBox.BindToAmount = "CurrentExchangeRate";
			this.AH_Calc_ExchangeRateBoundCalcFindBox.BindToDecimalPlaces = "ExchangeRateDecimalPlaces";
			this.AH_Calc_ExchangeRateBoundCalcFindBox.BindToUnit = "AH_RX_NKTransactionCurrency";
			this.AH_Calc_ExchangeRateBoundCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashBookExchangeDiffForm|b2fb6a6b-76f0-47ac-8f54-b66a1ba5e917", "Current Exchange Rate");
			this.AH_Calc_ExchangeRateBoundCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_Calc_ExchangeRateBoundCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 150, true);
			this.AH_Calc_ExchangeRateBoundCalcFindBox.Name = "AH_Calc_ExchangeRateBoundCalcFindBox";
			this.AH_Calc_ExchangeRateBoundCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.AH_Calc_ExchangeRateBoundCalcFindBox.TabIndex = 7;
			// 
			// AH_Calc_InvoiceAmountBoundCalcFindBox
			// 
			this.AH_Calc_InvoiceAmountBoundCalcFindBox.BindToAmount = "ForeignCurrencyGainLoss";
			this.AH_Calc_InvoiceAmountBoundCalcFindBox.BindToUnit = "AH_Calc_LocalRX";
			this.AH_Calc_InvoiceAmountBoundCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashBookExchangeDiffForm|534749e3-1fa4-4ca5-83da-bad18b5f7b89", "Exchange Gain/(Loss)");
			this.AH_Calc_InvoiceAmountBoundCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 173, true);
			this.AH_Calc_InvoiceAmountBoundCalcFindBox.Name = "AH_Calc_InvoiceAmountBoundCalcFindBox";
			this.AH_Calc_InvoiceAmountBoundCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.AH_Calc_InvoiceAmountBoundCalcFindBox.TabIndex = 8;
			// 
			// AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox
			// 
			this.AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox.BindToAmount = "AH_ExchangeRate";
			this.AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox.BindToDecimalPlaces = "ExchangeRateDecimalPlaces";
			this.AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox.BindToUnit = "AH_RX_NKTransactionCurrency";
			this.AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashBookExchangeDiffForm|68ff6018-89c5-4b2b-9f87-f0805b097d22", "New Exchange Rate");
			this.AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 128, true);
			this.AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox.Name = "AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox";
			this.AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox.TabIndex = 6;
			// 
			// AH_PostDateBoundDateEdit
			// 
			this.AH_PostDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_PostDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_PostDateBoundDateEdit, "AH_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CashbookExchangeDiff)(null)).AH_PostDate)));
			this.AH_PostDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 41, true);
			this.AH_PostDateBoundDateEdit.Name = "AH_PostDateBoundDateEdit";
			this.AH_PostDateBoundDateEdit.TabIndex = 2;
			// 
			// AH_TransactionNumBoundTextBox
			// 
			this.AH_TransactionNumBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.AH_TransactionNumBoundTextBox, "AH_TransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CashbookExchangeDiff)(null)).AH_TransactionNum)));
			this.AH_TransactionNumBoundTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashBookExchangeDiffForm|2b2bb8c1-f39c-4db8-a607-10b4fe14f745", "Adjustment No.");
			this.AH_TransactionNumBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 19, true);
			this.AH_TransactionNumBoundTextBox.Name = "AH_TransactionNumBoundTextBox";
			this.AH_TransactionNumBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 18, true);
			this.AH_TransactionNumBoundTextBox.TabIndex = 1;
			this.AH_TransactionNumBoundTextBox.Text = "OTEXTBOX2";
			// 
			// AH_DescBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_DescBoundTextBox, "AH_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CashbookExchangeDiff)(null)).AH_Desc)));
			this.AH_DescBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AH_DescBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 85, true);
			this.AH_DescBoundTextBox.Multiline = true;
			this.AH_DescBoundTextBox.Name = "AH_DescBoundTextBox";
			this.AH_DescBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 38, true);
			this.AH_DescBoundTextBox.TabIndex = 5;
			// 
			// AH_InvoiceDateBoundDateEdit
			// 
			this.AH_InvoiceDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_InvoiceDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_InvoiceDateBoundDateEdit, "AH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CashbookExchangeDiff)(null)).AH_InvoiceDate)));
			this.AH_InvoiceDateBoundDateEdit.CaptionResourceString = Res.GetData("CashBookExchangeDiffForm|481b0c20-096b-4e04-bf57-31f1bee5ff52", "Date");
			this.AH_InvoiceDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 19, true);
			this.AH_InvoiceDateBoundDateEdit.Name = "AH_InvoiceDateBoundDateEdit";
			this.AH_InvoiceDateBoundDateEdit.TabIndex = 0;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 336, true);
			this.zStmNoteTabPage1.TabIndex = 2;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 315, true);
			this.zEventTabPage1.TabIndex = 1;
			// 
			// AH_AGGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.AH_AGGuidFindBox, "AH_AG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CashbookExchangeDiff)(null)).AH_AG)));
			this.AH_AGGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d4b5253f-14ac-499f-adb6-8f91f5ecd9cb", "GL Account");
			this.AH_AGGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 194, true);
			this.AH_AGGuidFindBox.Name = "AH_AGGuidFindBox";
			this.AH_AGGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 18, true);
			this.AH_AGGuidFindBox.TabIndex = 9;
			this.AH_AGGuidFindBox.Visible = false;
			// 
			// CashBookExchangeDiffForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Res.GetData("CashBookExchangeDiffForm|0f406f6f-f3e6-4e79-b228-7684628f417a", "Cash Book Currency Exchange Adjustment");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 442, true);
			this.Controls.Add(this.zTabControl1);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceType = typeof(CashbookExchangeDiff);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 316, true);
			this.Name = "CashBookExchangeDiffForm";
			this.ShouldSerializeTabPageMethods = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.zTabControl1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.zTabControl1.ResumeLayout(false);
			this.zTabControl1.PerformLayout();
			this.zTabPage1.ResumeLayout(false);
			this.zTabPage1.PerformLayout();
			this.AfterCurrencyAdjustmentGroupBox.ResumeLayout(false);
			this.AfterCurrencyAdjustmentGroupBox.PerformLayout();
			this.AH_Calc_BeforeCurrencyAdjBalanceLocalBoundCalcFindBox.ResumeLayout(true);
			this.AH_Calc_BeforeCurrencyAdjBalanceLocalBoundCalcFindBox.PerformLayout();
			this.AH_Calc_AfterCurrencyAdjBalanceLocal.ResumeLayout(true);
			this.AH_Calc_AfterCurrencyAdjBalanceLocal.PerformLayout();
			this.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox.ResumeLayout(true);
			this.AH_Calc_AfterCurrencyAdjBalanceAmountBoundCalcFindBox.PerformLayout();
			this.AH_AGGuidFindBox.ResumeLayout(true);
			this.AH_AGGuidFindBox.PerformLayout();
			this.AH_ABBoundZGuidFindBox.ResumeLayout(true);
			this.AH_ABBoundZGuidFindBox.PerformLayout();
			this.AH_Calc_ExchangeRateBoundCalcFindBox.ResumeLayout(true);
			this.AH_Calc_ExchangeRateBoundCalcFindBox.PerformLayout();
			this.AH_Calc_InvoiceAmountBoundCalcFindBox.ResumeLayout(true);
			this.AH_Calc_InvoiceAmountBoundCalcFindBox.PerformLayout();
			this.AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox.ResumeLayout(true);
			this.AH_ExchangeRateBoundAccountingExchangeRateControlBoundCalcFindBox.PerformLayout();
			this.AH_PostDateBoundDateEdit.ResumeLayout(true);
			this.AH_PostDateBoundDateEdit.PerformLayout();
			this.AH_InvoiceDateBoundDateEdit.ResumeLayout(true);
			this.AH_InvoiceDateBoundDateEdit.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}