using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.Accounting.DataTransfer.BankStatement;
using Enterprise.Accounting.GUI.CashBook.BankReconciliation;
using Enterprise.Billing.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.BankStatement
{
	public partial class BankStatementForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new Container();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZCheckBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			this.OpenFileDialog = new ZOpenFileDialog();
			this.FilterGroupBox = new ZGroupBox();
			this.ChequeREferenceFilterTextBox = new ZTextBox();
			this.AmountFilterCalcEdit = new ZCalcEdit();
			this.TypeFilterDropEdit = new ZDropEdit();
			this.DebitCreditFilterDropEdit = new ZDropEdit();
			this.FindFilterButton = new ZButton();
			this.MaxPageCalcEdit1 = new ZCalcEdit();
			this.ClearFilterButton = new ZButton();
			this.PageNoCalcEdit = new ZCalcEdit();
			this.StatementDateFilterDateEdit = new ZDateEdit();
			this.ImportStatementButton = new ZButton();
			this.StatementsTabControl = new ZTabControl();
			this.UnreconciledTabPage = new ZTabPage();
			this.UnreconciledStatementsGrid = new ZGrid();
			this.ReconciledTabPage = new ZTabPage();
			this.ReconciledStatementsGrid = new ZGrid();
			this.BankTransactionButton = new ZButton();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.StatementDateDateEdit = new ZDateEdit();
			this.StatementBalanceCalcEdit = new ZCalcEdit();
			this.OpeningBalanceCalcEdit = new ZCalcEdit();
			this.PageBalanceTotalCalcEdit = new ZCalcEdit();
			this.PageCreditTotalLabel = new ZLabel();
			this.PageDebitTotalLabel = new ZLabel();
			this.CreditTotalCountLabel = new ZLabel();
			this.DebitTotalCountLabel = new ZLabel();
			this.NettAmountCalcEdit = new ZCalcEdit();
			this.TotalCreditLabel = new ZLabel();
			this.TotalDebitLabel = new ZLabel();
			this.PageDebitLabel = new ZLabel();
			this.PageCreditLabel = new ZLabel();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FilterGroupBox.SuspendLayout();
			this.StatementsTabControl.SuspendLayout();
			this.UnreconciledTabPage.SuspendLayout();
			((ISupportInitialize)(this.UnreconciledStatementsGrid)).BeginInit();
			this.ReconciledTabPage.SuspendLayout();
			((ISupportInitialize)(this.ReconciledStatementsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 560, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 22, true);
			this.MainStatusBar.TabIndex = 23;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(736);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.Base.AccStatement.BankStatement);
			// 
			// FilterGroupBox
			// 
			this.FilterGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.FilterGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|88b7da67-858d-45d2-b5da-1ff077bdc533", "Filter");
			this.FilterGroupBox.Controls.Add(this.ChequeREferenceFilterTextBox);
			this.FilterGroupBox.Controls.Add(this.AmountFilterCalcEdit);
			this.FilterGroupBox.Controls.Add(this.TypeFilterDropEdit);
			this.FilterGroupBox.Controls.Add(this.DebitCreditFilterDropEdit);
			this.FilterGroupBox.Controls.Add(this.FindFilterButton);
			this.FilterGroupBox.Controls.Add(this.MaxPageCalcEdit1);
			this.FilterGroupBox.Controls.Add(this.ClearFilterButton);
			this.FilterGroupBox.Controls.Add(this.PageNoCalcEdit);
			this.FilterGroupBox.Controls.Add(this.StatementDateFilterDateEdit);
			this.FilterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 28, true);
			this.FilterGroupBox.Name = "FilterGroupBox";
			this.FilterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(732, 67, true);
			this.FilterGroupBox.TabIndex = 6;
			this.FilterGroupBox.TabStop = false;
			// 
			// ChequeREferenceFilterTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChequeREferenceFilterTextBox, "ChequeReferenceFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Business.Base.AccStatement.BankStatement)(null)).ChequeReferenceFilter)));
			this.ChequeREferenceFilterTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|47478f56-84f0-4a8f-b2c1-dcb96cf99c2c", "Check/Reference");
			this.ChequeREferenceFilterTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 44, true);
			this.ChequeREferenceFilterTextBox.Name = "ChequeREferenceFilterTextBox";
			this.ChequeREferenceFilterTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 20, true);
			this.ChequeREferenceFilterTextBox.TabIndex = 13;
			// 
			// AmountFilterCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AmountFilterCalcEdit, "AmountFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.Base.AccStatement.BankStatement)(null)).AmountFilter)));
			this.AmountFilterCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|1d2a5e07-4d6e-408e-9043-ca7b0050c171", "Amount");
			this.AmountFilterCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 44, true);
			this.AmountFilterCalcEdit.Name = "AmountFilterCalcEdit";
			this.AmountFilterCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.AmountFilterCalcEdit.TabIndex = 3;
			this.AmountFilterCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TypeFilterDropEdit
			// 
			this.BindingSource.SetBindingMember(this.TypeFilterDropEdit, "TypeFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.Base.AccStatement.BankStatement)(null)).TypeFilter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).AB_Type_List)));
			this.TypeFilterDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|7e8b78b3-31c6-4d96-a27f-dc81e43adad0", "Type");
			this.TypeFilterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 20, true);
			this.TypeFilterDropEdit.Name = "TypeFilterDropEdit";
			this.TypeFilterDropEdit.PreBoundMaxLength = 4;
			this.TypeFilterDropEdit.ShowDescriptionBox = false;
			this.TypeFilterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.TypeFilterDropEdit.TabIndex = 7;
			// 
			// DebitCreditFilterDropEdit
			// 
			this.BindingSource.SetBindingMember(this.DebitCreditFilterDropEdit, "DebitCreditFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.Base.AccStatement.BankStatement)(null)).DebitCreditFilter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).AB_DebitCredit_List)));
			this.DebitCreditFilterDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|078c0d17-6f3a-46be-a4a5-3bade63febf3", "Debit/Credit");
			this.DebitCreditFilterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 19, true);
			this.DebitCreditFilterDropEdit.Name = "DebitCreditFilterDropEdit";
			this.DebitCreditFilterDropEdit.PreBoundMaxLength = 3;
			this.DebitCreditFilterDropEdit.ShowDescriptionBox = false;
			this.DebitCreditFilterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.DebitCreditFilterDropEdit.TabIndex = 5;
			// 
			// FindFilterButton
			// 
			this.FindFilterButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.FindFilterButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|d73008ac-43e1-4d1b-8257-ebd6544b7d1f", "Find");
			this.FindFilterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(651, 15, true);
			this.FindFilterButton.Name = "FindFilterButton";
			this.FindFilterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.FindFilterButton.TabIndex = 14;
			this.FindFilterButton.Click += new EventHandler(this.FildFilterButton_Click);
			// 
			// MaxPageCalcEdit1
			// 
			this.MaxPageCalcEdit1.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.MaxPageCalcEdit1, "MaxPage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.Base.AccStatement.BankStatement)(null)).MaxPage)));
			this.MaxPageCalcEdit1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.MaxPageCalcEdit1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|3085b82b-60bf-408e-a798-4c7b3bb3eecf", "Of");
			this.MaxPageCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 22, true);
			this.MaxPageCalcEdit1.Name = "MaxPageCalcEdit1";
			this.MaxPageCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 13, true);
			this.MaxPageCalcEdit1.TabIndex = 11;
			// 
			// ClearFilterButton
			// 
			this.ClearFilterButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearFilterButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|e87e9e60-4e3e-4220-9a88-53a7b0840984", "Clear");
			this.ClearFilterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(651, 40, true);
			this.ClearFilterButton.Name = "ClearFilterButton";
			this.ClearFilterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.ClearFilterButton.TabIndex = 15;
			this.ClearFilterButton.Click += new EventHandler(this.ClearFilterButton_Click);
			// 
			// PageNoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PageNoCalcEdit, "CurrentPageFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.Base.AccStatement.BankStatement)(null)).CurrentPageFilter)));
			this.PageNoCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|70cbfb14-3d2b-4605-b951-aa9df41765cc", "Page");
			this.PageNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(535, 19, true);
			this.PageNoCalcEdit.Name = "PageNoCalcEdit";
			this.PageNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.PageNoCalcEdit.TabIndex = 9;
			this.PageNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StatementDateFilterDateEdit
			// 
			this.StatementDateFilterDateEdit.AutoCompleteMonthThreshold = 1;
			this.StatementDateFilterDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StatementDateFilterDateEdit, "StatementDateFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.Base.AccStatement.BankStatement)(null)).StatementDateFilter)));
			this.StatementDateFilterDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|9b6fb9b9-b40f-48f9-acdb-8bca450d43e8", "Statement Date");
			this.StatementDateFilterDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 19, true);
			this.StatementDateFilterDateEdit.Name = "StatementDateFilterDateEdit";
			this.StatementDateFilterDateEdit.TabIndex = 1;
			// 
			// ImportStatementButton
			// 
			this.ImportStatementButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ImportStatementButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|54ece1dd-3d10-497c-96b1-2e7cd6df9899", "Import &Statement");
			this.ImportStatementButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 533, true);
			this.ImportStatementButton.Name = "ImportStatementButton";
			this.ImportStatementButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 22, true);
			this.ImportStatementButton.TabIndex = 20;
			this.ImportStatementButton.UseVisualStyleBackColor = true;
			this.ImportStatementButton.Click += new EventHandler(this.ImportStatementButton_Click);
			// 
			// StatementsTabControl
			// 
			this.StatementsTabControl.Controls.Add(this.UnreconciledTabPage);
			this.StatementsTabControl.Controls.Add(this.ReconciledTabPage);
			this.StatementsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 100, true);
			this.StatementsTabControl.Name = "StatementsTabControl";
			this.StatementsTabControl.SelectedIndex = 0;
			this.StatementsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 383, true);
			this.StatementsTabControl.TabIndex = 7;
			// 
			// UnreconciledTabPage
			// 
			this.UnreconciledTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|d9ad21f8-c7de-4c1d-b5f8-d55dd20d5ad2", "Unreconciled");
			this.UnreconciledTabPage.Controls.Add(this.UnreconciledStatementsGrid);
			this.UnreconciledTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.UnreconciledTabPage.Name = "UnreconciledTabPage";
			this.UnreconciledTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UnreconciledTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 356, true);
			this.UnreconciledTabPage.TabIndex = 0;
			this.UnreconciledTabPage.UseVisualStyleBackColor = true;
			// 
			// UnreconciledStatementsGrid
			// 
			this.UnreconciledStatementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UnreconciledStatementsGrid, "UnreconciledStatements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).UnreconciledStatements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).UnreconciledStatements)).SyncRoot)).AS_DebitCredit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).UnreconciledStatements)).SyncRoot)).Lookups.AS_DebitCredit_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).UnreconciledStatements)).SyncRoot)).AS_ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).UnreconciledStatements)).SyncRoot)).AS_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).UnreconciledStatements)).SyncRoot)).AS_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).UnreconciledStatements)).SyncRoot)).Lookups.AS_Type_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).UnreconciledStatements)).SyncRoot)).AS_IsCleared)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).UnreconciledStatements)).SyncRoot)).AS_StatementDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).UnreconciledStatements)).SyncRoot)).AS_PageNumber)));
			this.UnreconciledStatementsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|8e901500-2c03-489c-afcd-119e206fdd40", "Debit Credit");
			zDropEditColumnStyleInfo1.ColumnName = "AS_DebitCredit";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|f28d23d8-aa2f-4725-8b1d-71999760574a", "Check Or Reference");
			zTextBoxColumnStyleInfo1.ColumnName = "AS_ChequeOrReference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AS_Amount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|7f6e4030-54d5-4da4-9528-46d3327a0743", "Type");
			zDropEditColumnStyleInfo2.ColumnName = "AS_Type";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo1.ColumnName = "AS_IsCleared";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo1.ColumnName = "AS_StatementDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "AS_PageNumber";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			this.UnreconciledStatementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.UnreconciledStatementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.UnreconciledStatementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.UnreconciledStatementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.UnreconciledStatementsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.UnreconciledStatementsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.UnreconciledStatementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.UnreconciledStatementsGrid.GridId = "1445db34-d4bc-4ecc-b010-1be9d17f0f1a";
			this.UnreconciledStatementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnreconciledStatementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UnreconciledStatementsGrid.LayoutKey = "UnreconciledStatementsGrid";
			this.UnreconciledStatementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.UnreconciledStatementsGrid.Name = "UnreconciledStatementsGrid";
			this.UnreconciledStatementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 350, true);
			this.UnreconciledStatementsGrid.TabIndex = 0;
			// 
			// ReconciledTabPage
			// 
			this.ReconciledTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|14229444-245a-4276-8f8f-d42eeb7aea5a", "Reconciled");
			this.ReconciledTabPage.Controls.Add(this.ReconciledStatementsGrid);
			this.ReconciledTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ReconciledTabPage.Name = "ReconciledTabPage";
			this.ReconciledTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ReconciledTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 356, true);
			this.ReconciledTabPage.TabIndex = 1;
			this.ReconciledTabPage.UseVisualStyleBackColor = true;
			// 
			// ReconciledStatementsGrid
			// 
			this.ReconciledStatementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReconciledStatementsGrid, "ReconciledStatements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).ReconciledStatements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).ReconciledStatements)).SyncRoot)).AS_DebitCredit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).ReconciledStatements)).SyncRoot)).Lookups.AS_DebitCredit_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).ReconciledStatements)).SyncRoot)).AS_ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).ReconciledStatements)).SyncRoot)).AS_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).ReconciledStatements)).SyncRoot)).AS_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).ReconciledStatements)).SyncRoot)).Lookups.AS_Type_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).ReconciledStatements)).SyncRoot)).AS_IsCleared)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).ReconciledStatements)).SyncRoot)).AS_StatementDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Statement)(((System.Collections.IList)(((Business.Base.AccStatement.BankStatement)(null)).ReconciledStatements)).SyncRoot)).AS_PageNumber)));
			this.ReconciledStatementsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|9b9ecbef-811a-445d-a4a3-0c912ea99c56", "Debit Credit");
			zDropEditColumnStyleInfo3.ColumnName = "AS_DebitCredit";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|2c1f1b36-b3e1-4e9d-9cd0-57d0ab953e77", "Check Or Reference");
			zTextBoxColumnStyleInfo2.ColumnName = "AS_ChequeOrReference";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "AS_Amount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|211ced63-09d0-4dbe-9073-2ac362512644", "Type");
			zDropEditColumnStyleInfo4.ColumnName = "AS_Type";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo2.ColumnName = "AS_IsCleared";
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo2.ColumnName = "AS_StatementDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "AS_PageNumber";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			this.ReconciledStatementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ReconciledStatementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReconciledStatementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ReconciledStatementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ReconciledStatementsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ReconciledStatementsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ReconciledStatementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ReconciledStatementsGrid.GridId = "1854d9be-4509-448e-9388-e14468bfd716";
			this.ReconciledStatementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReconciledStatementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReconciledStatementsGrid.LayoutKey = "ReconciledStatementsGrid";
			this.ReconciledStatementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ReconciledStatementsGrid.Name = "ReconciledStatementsGrid";
			this.ReconciledStatementsGrid.ReadOnly = true;
			this.ReconciledStatementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 350, true);
			this.ReconciledStatementsGrid.TabIndex = 0;
			// 
			// BankTransactionButton
			// 
			this.BankTransactionButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BankTransactionButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|e23dda33-275e-48eb-a8d3-fba102838b8d", "Bank &Transaction");
			this.BankTransactionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 533, true);
			this.BankTransactionButton.Name = "BankTransactionButton";
			this.BankTransactionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 22, true);
			this.BankTransactionButton.TabIndex = 21;
			this.BankTransactionButton.UseVisualStyleBackColor = true;
			this.BankTransactionButton.Click += new EventHandler(this.BankTransactionButton_Click);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.AutoSize = true;
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(495, 532, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 26, true);
			this.PostingButtonsUserControl.TabIndex = 22;
			// 
			// StatementDateDateEdit
			// 
			this.StatementDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.StatementDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StatementDateDateEdit, "AB_LastStatementDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.Base.AccStatement.BankStatement)(null)).AB_LastStatementDate)));
			this.StatementDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 4, true);
			this.StatementDateDateEdit.Name = "StatementDateDateEdit";
			this.StatementDateDateEdit.TabIndex = 1;
			// 
			// StatementBalanceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.StatementBalanceCalcEdit, "AB_StatementBalance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.Base.AccStatement.BankStatement)(null)).AB_StatementBalance)));
			this.StatementBalanceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(538, 4, true);
			this.StatementBalanceCalcEdit.Name = "StatementBalanceCalcEdit";
			this.StatementBalanceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.StatementBalanceCalcEdit.TabIndex = 5;
			this.StatementBalanceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OpeningBalanceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OpeningBalanceCalcEdit, "OpeningStatementBalance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.Base.AccStatement.BankStatement)(null)).OpeningStatementBalance)));
			this.OpeningBalanceCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|5f8a29d6-d9d5-41ab-a498-9dc527c7f8cc", "Opening Balance");
			this.OpeningBalanceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 3, true);
			this.OpeningBalanceCalcEdit.Name = "OpeningBalanceCalcEdit";
			this.OpeningBalanceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.OpeningBalanceCalcEdit.TabIndex = 3;
			this.OpeningBalanceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PageBalanceTotalCalcEdit
			// 
			this.PageBalanceTotalCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PageBalanceTotalCalcEdit, "PageBalanceAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.Base.AccStatement.BankStatement)(null)).PageBalanceAmount)));
			this.PageBalanceTotalCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|b0454ae0-5c34-4a0c-be7c-c1a2a47c562e", "Page Bal.", "Page Balance");
			this.PageBalanceTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(639, 488, true);
			this.PageBalanceTotalCalcEdit.Name = "PageBalanceTotalCalcEdit";
			this.PageBalanceTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.PageBalanceTotalCalcEdit.TabIndex = 17;
			this.PageBalanceTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PageCreditTotalLabel
			// 
			this.PageCreditTotalLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PageCreditTotalLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PageCreditTotalLabel, "PageCreditTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Business.Base.AccStatement.BankStatement)(null)).PageCreditTotal)));
			this.PageCreditTotalLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|fb326402-bfa4-49f0-9a82-ccfe4da9360e", "Credit Total");
			this.PageCreditTotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 490, true);
			this.PageCreditTotalLabel.Name = "PageCreditTotalLabel";
			this.PageCreditTotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 13, true);
			this.PageCreditTotalLabel.TabIndex = 13;
			this.PageCreditTotalLabel.Text = "Credit Total";
			// 
			// PageDebitTotalLabel
			// 
			this.PageDebitTotalLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PageDebitTotalLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PageDebitTotalLabel, "PageDebitTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Business.Base.AccStatement.BankStatement)(null)).PageDebitTotal)));
			this.PageDebitTotalLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|d721a027-25e9-4c7f-959a-11b9c8f8fd54", "Debit Total");
			this.PageDebitTotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 490, true);
			this.PageDebitTotalLabel.Name = "PageDebitTotalLabel";
			this.PageDebitTotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 13, true);
			this.PageDebitTotalLabel.TabIndex = 9;
			this.PageDebitTotalLabel.Text = "Debit Total";
			// 
			// CreditTotalCountLabel
			// 
			this.CreditTotalCountLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CreditTotalCountLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CreditTotalCountLabel, "CreditTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Business.Base.AccStatement.BankStatement)(null)).CreditTotal)));
			this.CreditTotalCountLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|8c112735-3242-43df-9ed7-250f60e790a7", "Credit Total");
			this.CreditTotalCountLabel.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.CreditTotalCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 512, true);
			this.CreditTotalCountLabel.Name = "CreditTotalCountLabel";
			this.CreditTotalCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 13, true);
			this.CreditTotalCountLabel.TabIndex = 15;
			this.CreditTotalCountLabel.Text = "Credit Total";
			// 
			// DebitTotalCountLabel
			// 
			this.DebitTotalCountLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DebitTotalCountLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DebitTotalCountLabel, "DebitTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Business.Base.AccStatement.BankStatement)(null)).DebitTotal)));
			this.DebitTotalCountLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|790bd404-6174-4573-ba74-81524361e795", "Debit Total");
			this.DebitTotalCountLabel.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.DebitTotalCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 512, true);
			this.DebitTotalCountLabel.Name = "DebitTotalCountLabel";
			this.DebitTotalCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 13, true);
			this.DebitTotalCountLabel.TabIndex = 11;
			this.DebitTotalCountLabel.Text = "Debit Total";
			// 
			// NettAmountCalcEdit
			// 
			this.NettAmountCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NettAmountCalcEdit, "BalanceAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.Base.AccStatement.BankStatement)(null)).BalanceAmount)));
			this.NettAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|a97ecd47-8831-4615-9c63-7780f80505c7", "Stmt. Bal.", "Statement Balance");
			this.NettAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(639, 510, true);
			this.NettAmountCalcEdit.Name = "NettAmountCalcEdit";
			this.NettAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.NettAmountCalcEdit.TabIndex = 19;
			this.NettAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalCreditLabel
			// 
			this.TotalCreditLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.TotalCreditLabel.AutoSize = true;
			this.TotalCreditLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|861f0a3d-ccf9-4da7-8ec3-9c21673ce29d", "Stmt. Credit", "Statement Credit");
			this.TotalCreditLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(285, 512, true);
			this.TotalCreditLabel.Name = "TotalCreditLabel";
			this.TotalCreditLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 13, true);
			this.TotalCreditLabel.TabIndex = 14;
			// 
			// TotalDebitLabel
			// 
			this.TotalDebitLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.TotalDebitLabel.AutoSize = true;
			this.TotalDebitLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|e6166a2c-37d9-4f73-9e19-f198a850deea", "Stmt. Debit", "Statement Debit");
			this.TotalDebitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 512, true);
			this.TotalDebitLabel.Name = "TotalDebitLabel";
			this.TotalDebitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.TotalDebitLabel.TabIndex = 10;
			// 
			// PageDebitLabel
			// 
			this.PageDebitLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PageDebitLabel.AutoSize = true;
			this.PageDebitLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|2e3279c8-ce24-4f08-8a9b-fd17f78a3fd2", "Page Debit");
			this.PageDebitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 490, true);
			this.PageDebitLabel.Name = "PageDebitLabel";
			this.PageDebitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 13, true);
			this.PageDebitLabel.TabIndex = 8;
			// 
			// PageCreditLabel
			// 
			this.PageCreditLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PageCreditLabel.AutoSize = true;
			this.PageCreditLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|d473b7c2-d2f3-4018-8a90-6e67701431d7", "Page Credit");
			this.PageCreditLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 490, true);
			this.PageCreditLabel.Name = "PageCreditLabel";
			this.PageCreditLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 13, true);
			this.PageCreditLabel.TabIndex = 12;
			// 
			// BankStatementForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 582, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankStatementForm|b0596dc4-569e-4c4c-bcef-f3c289ea62f9", "Bank Statement");
			this.Controls.Add(this.PageBalanceTotalCalcEdit);
			this.Controls.Add(this.PageCreditTotalLabel);
			this.Controls.Add(this.PageDebitTotalLabel);
			this.Controls.Add(this.CreditTotalCountLabel);
			this.Controls.Add(this.DebitTotalCountLabel);
			this.Controls.Add(this.NettAmountCalcEdit);
			this.Controls.Add(this.TotalCreditLabel);
			this.Controls.Add(this.TotalDebitLabel);
			this.Controls.Add(this.PageDebitLabel);
			this.Controls.Add(this.PageCreditLabel);
			this.Controls.Add(this.StatementDateDateEdit);
			this.Controls.Add(this.StatementBalanceCalcEdit);
			this.Controls.Add(this.OpeningBalanceCalcEdit);
			this.Controls.Add(this.BankTransactionButton);
			this.Controls.Add(this.StatementsTabControl);
			this.Controls.Add(this.ImportStatementButton);
			this.Controls.Add(this.FilterGroupBox);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Business.Base.AccStatement.BankStatement);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.Base.AccStatement.BankStatement";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 585, true);
			this.Name = "BankStatementForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.FilterGroupBox, 0);
			this.Controls.SetChildIndex(this.ImportStatementButton, 0);
			this.Controls.SetChildIndex(this.StatementsTabControl, 0);
			this.Controls.SetChildIndex(this.BankTransactionButton, 0);
			this.Controls.SetChildIndex(this.OpeningBalanceCalcEdit, 0);
			this.Controls.SetChildIndex(this.StatementBalanceCalcEdit, 0);
			this.Controls.SetChildIndex(this.StatementDateDateEdit, 0);
			this.Controls.SetChildIndex(this.PageCreditLabel, 0);
			this.Controls.SetChildIndex(this.PageDebitLabel, 0);
			this.Controls.SetChildIndex(this.TotalDebitLabel, 0);
			this.Controls.SetChildIndex(this.TotalCreditLabel, 0);
			this.Controls.SetChildIndex(this.NettAmountCalcEdit, 0);
			this.Controls.SetChildIndex(this.DebitTotalCountLabel, 0);
			this.Controls.SetChildIndex(this.CreditTotalCountLabel, 0);
			this.Controls.SetChildIndex(this.PageDebitTotalLabel, 0);
			this.Controls.SetChildIndex(this.PageCreditTotalLabel, 0);
			this.Controls.SetChildIndex(this.PageBalanceTotalCalcEdit, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.FilterGroupBox.ResumeLayout(false);
			this.FilterGroupBox.PerformLayout();
			this.StatementsTabControl.ResumeLayout(false);
			this.UnreconciledTabPage.ResumeLayout(false);
			((ISupportInitialize)(this.UnreconciledStatementsGrid)).EndInit();
			this.ReconciledTabPage.ResumeLayout(false);
			((ISupportInitialize)(this.ReconciledStatementsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}