using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using JournalBase = Enterprise.Accounting.Business.ARAP.Journal.Journal;

namespace Enterprise.Accounting.GUI.ARAP.Journal
{
	public partial class JournalBaseForm
	{


		#region Windows Form Designer generated code

		private ZGroupBox JournalDetailsGroupBox;
		private ZExchangeRateControl CurrencyExchangeRateControl;
		private ZCalcFindBox LocalAmountCalcFindBox;
		private ZCalcFindBox JournalAmountCalcFindBox;
		private ZDropEdit DebitCreditDropEdit;
		private ZGuidFindBox DepartmentFindBox;
		private ZGuidFindBox BranchFindBox;
		private ZGuidFindBox GLAccountFindBox;
		private ZArchitecture.ZTextBox DescriptionTextEdit;
		private ZDateEdit TransactionDateEdit;
		private ZGuidFindBox AccountFindBox;
		private ZArchitecture.ZTextBox JournalNumberTextBox;
		private ZDateEdit PostDateEdit;
		private ZDateEdit AgeingDateEdit;
		private ZDropEdit AgreedPaymentDropEdit;
		private ZTemplateTabControl MainTabControl;
		private ZTabPage JournalTabPage;
		private ZLogsTabPage zEventTabPage1;
		private ZArchitecture.ZCalcEdit AH_NumberOfSupportingDocumentsCalcEdit;
		protected Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private ZArchitecture.ZGrid SubAccountsGrid;
		private ZArchitecture.ZLabel SubAccountsLabel;

		new void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AH_NumberOfSupportingDocumentsCalcEdit = new ZArchitecture.ZCalcEdit();
			this.JournalDetailsGroupBox = new ZGroupBox();
			this.SubAccountsLabel = new ZArchitecture.ZLabel();
			this.SubAccountsGrid = new ZArchitecture.ZGrid();
			this.CurrencyExchangeRateControl = new ZExchangeRateControl();
			this.LocalAmountCalcFindBox = new ZCalcFindBox();
			this.JournalAmountCalcFindBox = new ZCalcFindBox();
			this.DebitCreditDropEdit = new ZDropEdit();
			this.DepartmentFindBox = new ZGuidFindBox();
			this.BranchFindBox = new ZGuidFindBox();
			this.GLAccountFindBox = new ZGuidFindBox();
			this.DescriptionTextEdit = new ZArchitecture.ZTextBox();
			this.TransactionDateEdit = new ZDateEdit();
			this.AccountFindBox = new ZGuidFindBox();
			this.JournalNumberTextBox = new ZArchitecture.ZTextBox();
			this.PostDateEdit = new ZDateEdit();
			this.AgeingDateEdit = new ZDateEdit();
			this.AgreedPaymentDropEdit = new ZDropEdit();
			this.ButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.MainTabControl = new ZTemplateTabControl();
			this.JournalTabPage = new ZTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JournalDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubAccountsGrid)).BeginInit();
			this.SubAccountsGrid.SuspendLayout();
			this.CurrencyExchangeRateControl.SuspendLayout();
			this.LocalAmountCalcFindBox.SuspendLayout();
			this.JournalAmountCalcFindBox.SuspendLayout();
			this.DebitCreditDropEdit.SuspendLayout();
			this.DepartmentFindBox.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			this.GLAccountFindBox.SuspendLayout();
			this.TransactionDateEdit.SuspendLayout();
			this.AccountFindBox.SuspendLayout();
			this.PostDateEdit.SuspendLayout();
			this.AgeingDateEdit.SuspendLayout();
			this.AgreedPaymentDropEdit.SuspendLayout();
			this.ButtonsUserControl.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.JournalTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 475, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 20, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(283);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(283);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JournalBase);
			// 
			// AH_NumberOfSupportingDocumentsCalcEdit
			// 
			this.AH_NumberOfSupportingDocumentsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_NumberOfSupportingDocumentsCalcEdit, "AH_NumberOfSupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JournalBase)(null)).AH_NumberOfSupportingDocuments)));
			this.AH_NumberOfSupportingDocumentsCalcEdit.DecimalPlaces = 2;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 41, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.Name = "AH_NumberOfSupportingDocumentsCalcEdit";
			this.AH_NumberOfSupportingDocumentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.TabIndex = 24;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Text = "0";
			this.AH_NumberOfSupportingDocumentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JournalDetailsGroupBox
			// 
			this.JournalDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JournalBaseForm|1da23cba-0f04-4d6f-a879-2b7686c06f69", "Journal Details");
			this.JournalDetailsGroupBox.Controls.Add(this.SubAccountsLabel);
			this.JournalDetailsGroupBox.Controls.Add(this.SubAccountsGrid);
			this.JournalDetailsGroupBox.Controls.Add(this.AH_NumberOfSupportingDocumentsCalcEdit);
			this.JournalDetailsGroupBox.Controls.Add(this.CurrencyExchangeRateControl);
			this.JournalDetailsGroupBox.Controls.Add(this.LocalAmountCalcFindBox);
			this.JournalDetailsGroupBox.Controls.Add(this.JournalAmountCalcFindBox);
			this.JournalDetailsGroupBox.Controls.Add(this.DebitCreditDropEdit);
			this.JournalDetailsGroupBox.Controls.Add(this.DepartmentFindBox);
			this.JournalDetailsGroupBox.Controls.Add(this.BranchFindBox);
			this.JournalDetailsGroupBox.Controls.Add(this.GLAccountFindBox);
			this.JournalDetailsGroupBox.Controls.Add(this.DescriptionTextEdit);
			this.JournalDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 82, true);
			this.JournalDetailsGroupBox.Name = "JournalDetailsGroupBox";
			this.JournalDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 305, true);
			this.JournalDetailsGroupBox.TabIndex = 10;
			this.JournalDetailsGroupBox.TabStop = false;
			// 
			// SubAccountsLabel
			// 
			this.SubAccountsLabel.AutoSize = true;
			this.SubAccountsLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0abc2868-c4e1-4239-abc4-3e6d722cebfd", "Sub Accounts");
			this.SubAccountsLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SubAccountsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 114, true);
			this.SubAccountsLabel.Name = "SubAccountsLabel";
			this.SubAccountsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.SubAccountsLabel.TabIndex = 12;
			this.SubAccountsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// SubAccountsGrid
			// 
			this.SubAccountsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SubAccountsGrid, "SubAccounts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JournalBase)(null)).SubAccounts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ARAP.Journal.JournalSubAccount)(((System.Collections.IList)(((JournalBase)(null)).SubAccounts)).SyncRoot)).AHS_Calc_SubClassParent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.ARAP.Journal.JournalSubAccount)(((System.Collections.IList)(((JournalBase)(null)).SubAccounts)).SyncRoot)).AHS_SubClassParentId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ARAP.Journal.JournalSubAccount)(((System.Collections.IList)(((JournalBase)(null)).SubAccounts)).SyncRoot)).AHS_Calc_SubAccountDescription)));
			this.SubAccountsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "AHS_Calc_SubClassParent";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AHS_SubClassParentId";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo2.ColumnName = "AHS_Calc_SubAccountDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.SubAccountsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SubAccountsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.SubAccountsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SubAccountsGrid.GridId = "94a3f40c-2515-4465-bf7f-1ad6084a56be";
			this.SubAccountsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SubAccountsGrid.LayoutKey = "SubAccountsGrid";
			this.SubAccountsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 111, true);
			this.SubAccountsGrid.Name = "SubAccountsGrid";
			this.SubAccountsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.SubAccountsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 129, true);
			this.SubAccountsGrid.TabIndex = 12;
			// 
			// CurrencyExchangeRateControl
			// 
			this.CurrencyExchangeRateControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrencyExchangeRateControl, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((JournalBase)(null)).ExchangeRate)));
			this.CurrencyExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JournalBaseForm|e4b5475d-0248-4496-92a7-9b4ec6b0b746", "Currency", "Currency", "");
			this.CurrencyExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 41, true);
			this.CurrencyExchangeRateControl.Name = "CurrencyExchangeRateControl";
			this.CurrencyExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.CurrencyExchangeRateControl.TabIndex = 3;
			// 
			// LocalAmountCalcFindBox
			// 
			this.LocalAmountCalcFindBox.AllowDrop = true;
			this.LocalAmountCalcFindBox.BindToAmount = "AH_LocalExTaxAmount";
			this.LocalAmountCalcFindBox.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.LocalAmountCalcFindBox.BindToUnit = "AH_Calc_LocalRX";
			this.LocalAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JournalBaseForm|14526042-6f2d-42ec-ab72-72e93b69c987", "Local Amount");
			this.LocalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 63, true);
			this.LocalAmountCalcFindBox.Name = "LocalAmountCalcFindBox";
			this.LocalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.LocalAmountCalcFindBox.TabIndex = 7;
			// 
			// JournalAmountCalcFindBox
			// 
			this.JournalAmountCalcFindBox.AllowDrop = true;
			this.JournalAmountCalcFindBox.BindToAmount = "AH_OSExTaxAmount";
			this.JournalAmountCalcFindBox.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.JournalAmountCalcFindBox.BindToUnit = "JournalCurrencyReadOnly";
			this.JournalAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JournalBaseForm|a6b302b0-2d74-4c6d-9537-4ca21b8ce90d", "Journal Amount");
			this.JournalAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.JournalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 63, true);
			this.JournalAmountCalcFindBox.Name = "JournalAmountCalcFindBox";
			this.JournalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.JournalAmountCalcFindBox.TabIndex = 5;
			// 
			// DebitCreditDropEdit
			// 
			this.DebitCreditDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DebitCreditDropEdit, "DebitCreditSign");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JournalBase)(null)).DebitCreditSign)));
			this.DebitCreditDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JournalBaseForm|915f658c-40fd-45d7-865d-fa7eb3e3603c", "Debit/Credit");
			this.DebitCreditDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 85, true);
			this.DebitCreditDropEdit.Name = "DebitCreditDropEdit";
			this.DebitCreditDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.DebitCreditDropEdit.TabIndex = 9;
			// 
			// DepartmentFindBox
			// 
			this.DepartmentFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartmentFindBox, "AH_GE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JournalBase)(null)).AH_GE)));
			this.DepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 272, true);
			this.DepartmentFindBox.Name = "DepartmentFindBox";
			this.DepartmentFindBox.PopupCaption = null;
			this.DepartmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 20, true);
			this.DepartmentFindBox.TabIndex = 15;
			// 
			// BranchFindBox
			// 
			this.BranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchFindBox, "AH_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JournalBase)(null)).AH_GB)));
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 246, true);
			this.BranchFindBox.Name = "BranchFindBox";
			this.BranchFindBox.PopupCaption = null;
			this.BranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 20, true);
			this.BranchFindBox.TabIndex = 13;
			// 
			// GLAccountFindBox
			// 
			this.GLAccountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GLAccountFindBox, "AH_AG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JournalBase)(null)).AH_AG)));
			this.GLAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 85, true);
			this.GLAccountFindBox.Name = "GLAccountFindBox";
			this.GLAccountFindBox.PopupCaption = null;
			this.GLAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.GLAccountFindBox.TabIndex = 11;
			// 
			// DescriptionTextEdit
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextEdit, "AH_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JournalBase)(null)).AH_Desc)));
			this.DescriptionTextEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JournalBaseForm|0702c0b3-d18a-4dd2-a45a-78ea8004a81a", "Description");
			this.DescriptionTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 19, true);
			this.DescriptionTextEdit.Name = "DescriptionTextEdit";
			this.DescriptionTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 20, true);
			this.DescriptionTextEdit.TabIndex = 1;
			// 
			// TransactionDateEdit
			// 
			this.TransactionDateEdit.AllowDrop = true;
			this.TransactionDateEdit.AutoCompleteMonthThreshold = 1;
			this.TransactionDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.TransactionDateEdit, "AH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JournalBase)(null)).AH_InvoiceDate)));
			this.TransactionDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JournalBaseForm|ea0ef734-cc2b-4d83-be40-bfba820288a9", "Date");
			this.TransactionDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 11, true);
			this.TransactionDateEdit.Name = "TransactionDateEdit";
			this.TransactionDateEdit.TabIndex = 1;
			// 
			// AccountFindBox
			// 
			this.AccountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AccountFindBox, "AH_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JournalBase)(null)).AH_OH)));
			this.AccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 56, true);
			this.AccountFindBox.Name = "AccountFindBox";
			this.AccountFindBox.PopupCaption = null;
			this.AccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 20, true);
			this.AccountFindBox.TabIndex = 9;
			// 
			// JournalNumberTextBox
			// 
			this.JournalNumberTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.JournalNumberTextBox, "AH_TransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JournalBase)(null)).AH_TransactionNum)));
			this.JournalNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JournalBaseForm|c1da08b0-b220-4cc9-a73d-f639dbbe389f", "Journal Number");
			this.JournalNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 11, true);
			this.JournalNumberTextBox.Name = "JournalNumberTextBox";
			this.JournalNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JournalNumberTextBox.TabIndex = 3;
			// 
			// PostDateEdit
			// 
			this.PostDateEdit.AllowDrop = true;
			this.PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateEdit, "AH_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JournalBase)(null)).AH_PostDate)));
			this.PostDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JournalBaseForm|3b920e09-b58f-4269-a1a5-96557bfa535a", "Post Date");
			this.PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 33, true);
			this.PostDateEdit.Name = "PostDateEdit";
			this.PostDateEdit.TabIndex = 5;
			// 
			// AgeingDateEdit
			// 
			this.AgeingDateEdit.AllowDrop = true;
			this.AgeingDateEdit.AutoCompleteMonthThreshold = 1;
			this.AgeingDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AgeingDateEdit, "AH_DueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JournalBase)(null)).AH_DueDate)));
			this.AgeingDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JournalBaseForm|1e4d8832-3fdc-4f60-8b8c-8c47669e67f8", "Due Date");
			this.AgeingDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 33, true);
			this.AgeingDateEdit.Name = "AgeingDateEdit";
			this.AgeingDateEdit.TabIndex = 7;
			// 
			// AgreedPaymentDropEdit
			// 
			this.AgreedPaymentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgreedPaymentDropEdit, "AH_AgreedPaymentMethodOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JournalBase)(null)).AH_AgreedPaymentMethodOverride)));
			this.AgreedPaymentDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JournalBaseForm|72DE91DF-B8A5-4314-9E55-45778CD2FFD8", "Agreed Payment Method");
			this.AgreedPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 33, true);
			this.AgreedPaymentDropEdit.Name = "AgreedPaymentDropEdit";
			this.AgreedPaymentDropEdit.ShowDescriptionBox = false;
			this.AgreedPaymentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.AgreedPaymentDropEdit.TabIndex = 7;
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 438, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.TabIndex = 1;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.JournalTabPage);
			this.MainTabControl.Controls.Add(this.zEventTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 428, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// JournalTabPage
			// 
			this.JournalTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JournalBaseForm|aa911698-7d0a-4071-b7d4-7378736e3615", "Journal");
			this.JournalTabPage.Controls.Add(this.AccountFindBox);
			this.JournalTabPage.Controls.Add(this.AgeingDateEdit);
			this.JournalTabPage.Controls.Add(this.AgreedPaymentDropEdit);
			this.JournalTabPage.Controls.Add(this.PostDateEdit);
			this.JournalTabPage.Controls.Add(this.JournalDetailsGroupBox);
			this.JournalTabPage.Controls.Add(this.TransactionDateEdit);
			this.JournalTabPage.Controls.Add(this.JournalNumberTextBox);
			this.JournalTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.JournalTabPage.Name = "JournalTabPage";
			this.JournalTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 391, true);
			this.JournalTabPage.TabIndex = 0;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 411, true);
			this.zEventTabPage1.TabIndex = 1;
			// 
			// JournalBaseForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JournalBaseForm|8d9317cf-c961-45d8-ac2e-5d82226dee2f", "Journal Base Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 485, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.ButtonsUserControl);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(JournalBase);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.Journal.Journal";
			this.Name = "JournalBaseForm";
			this.RememberFormSize = false;
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.JournalDetailsGroupBox.ResumeLayout(false);
			this.JournalDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubAccountsGrid)).EndInit();
			this.SubAccountsGrid.ResumeLayout(false);
			this.SubAccountsGrid.PerformLayout();
			this.CurrencyExchangeRateControl.ResumeLayout(true);
			this.CurrencyExchangeRateControl.PerformLayout();
			this.LocalAmountCalcFindBox.ResumeLayout(true);
			this.LocalAmountCalcFindBox.PerformLayout();
			this.JournalAmountCalcFindBox.ResumeLayout(true);
			this.JournalAmountCalcFindBox.PerformLayout();
			this.DebitCreditDropEdit.ResumeLayout(true);
			this.DebitCreditDropEdit.PerformLayout();
			this.DepartmentFindBox.ResumeLayout(true);
			this.DepartmentFindBox.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			this.GLAccountFindBox.ResumeLayout(true);
			this.GLAccountFindBox.PerformLayout();
			this.TransactionDateEdit.ResumeLayout(true);
			this.TransactionDateEdit.PerformLayout();
			this.AccountFindBox.ResumeLayout(true);
			this.AccountFindBox.PerformLayout();
			this.PostDateEdit.ResumeLayout(true);
			this.PostDateEdit.PerformLayout();
			this.AgeingDateEdit.ResumeLayout(true);
			this.AgeingDateEdit.PerformLayout();
			this.AgreedPaymentDropEdit.ResumeLayout(true);
			this.AgreedPaymentDropEdit.PerformLayout();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.JournalTabPage.ResumeLayout(false);
			this.JournalTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
