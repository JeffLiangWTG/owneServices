using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.ARAP.Statements
{
	public partial class StatementPrintForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.PrintButton = new ZButton();
			this.FormCancelButton = new ZButton();
			this.CurrencyGuidFindBox = new ZGuidFindBox();
			this.OrganisationGroupBox = new ZGroupBox();
			this.zDropEdit5 = new ZDropEdit();
			this.CreditRatingDropEdit = new ZDropEdit();
			this.AccountsRelationshipDropEdit = new ZDropEdit();
			this.ConsolidationCategoryDropEdit = new ZDropEdit();
			this.zGuidFindBox4 = new ZGuidFindBox();
			this.zGuidFindBox3 = new ZGuidFindBox();
			this.SalesRepFindBox = new ZGuidFindBox();
			this.DebtorGroupFindBox = new ZGuidFindBox();
			this.OrganisationBranchFindBox = new ZGuidFindBox();
			this.OrganisationGuidFindBox = new ZGuidFindBox();
			this.TransactionGroupBox = new ZGroupBox();
			this.IncludeTransactionsInActiveBatchCheckBox = new ZCheckBox();
			this.DepartmentGuidFindBox = new ZGuidFindBox();
			this.TransactionBranchFindBox = new ZGuidFindBox();
			this.DisbursementInvoicesCheckBox = new ZCheckBox();
			this.OutstandingAmountCalcEdit = new ZArchitecture.ZCalcEdit();
			this.CutoffDateEdit = new ZDateEdit();
			this.CutoffPeriodEdit = new ZPeriodEdit();
			this.DocumentGroupBox = new ZGroupBox();
			this.IncludeDebtorSummaryPageCheckBox = new ZCheckBox();
			this.DepartmentCheckBox = new ZCheckBox();
			this.IssueStatementPackDropEdit = new ZDropEdit();
			this.TypeOfDocumentToPrintDropEdit = new ZDropEdit();
			this.IssueStatementsBySettlementGroupCheckBox = new ZCheckBox();
			this.IssueByTransactionBranchCheckBox = new ZCheckBox();
			this.grpBxAccountMovement = new ZGroupBox();
			this.dedPrintAccMovementSOAGroupBy = new ZDropEdit();
			this.dtpPrintAccMovementSOATo = new ZDateEdit();
			this.dtpPrintAccMovementSOAFrom = new ZDateEdit();
			this.chkPrintAccMovementSOA = new ZCheckBox();
			this.zGroupBox1 = new ZGroupBox();
			this.accFeeTaxIDMessageLabel = new ZArchitecture.ZLabel();
			this.AL_AT_GSTTaxIDGuidFindBox = new ZGuidFindBox();
			this.dtpAccFeeToDate = new ZDateEdit();
			this.dtpAccFeeFromDate = new ZDateEdit();
			this.txtInvoiceDescription = new ZArchitecture.ZTextBox();
			this.chkFeePosting = new ZCheckBox();
			this.dtpAccFeePostDate = new ZDateEdit();
			this.dtpAccFeeInvoiceDate = new ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CurrencyGuidFindBox.SuspendLayout();
			this.OrganisationGroupBox.SuspendLayout();
			this.zDropEdit5.SuspendLayout();
			this.CreditRatingDropEdit.SuspendLayout();
			this.AccountsRelationshipDropEdit.SuspendLayout();
			this.ConsolidationCategoryDropEdit.SuspendLayout();
			this.zGuidFindBox4.SuspendLayout();
			this.zGuidFindBox3.SuspendLayout();
			this.SalesRepFindBox.SuspendLayout();
			this.DebtorGroupFindBox.SuspendLayout();
			this.OrganisationBranchFindBox.SuspendLayout();
			this.OrganisationGuidFindBox.SuspendLayout();
			this.TransactionGroupBox.SuspendLayout();
			this.DepartmentGuidFindBox.SuspendLayout();
			this.TransactionBranchFindBox.SuspendLayout();
			this.CutoffDateEdit.SuspendLayout();
			this.DocumentGroupBox.SuspendLayout();
			this.IssueStatementPackDropEdit.SuspendLayout();
			this.TypeOfDocumentToPrintDropEdit.SuspendLayout();
			this.grpBxAccountMovement.SuspendLayout();
			this.dedPrintAccMovementSOAGroupBy.SuspendLayout();
			this.dtpPrintAccMovementSOATo.SuspendLayout();
			this.dtpPrintAccMovementSOAFrom.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.AL_AT_GSTTaxIDGuidFindBox.SuspendLayout();
			this.dtpAccFeeToDate.SuspendLayout();
			this.dtpAccFeeFromDate.SuspendLayout();
			this.dtpAccFeePostDate.SuspendLayout();
			this.dtpAccFeeInvoiceDate.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 493, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 22, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 7;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(177);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(177);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Statement);
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|2c88dc1c-83a1-4404-90bb-67ef5ae426f6", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(795, 469, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PrintButton.TabIndex = 5;
			this.PrintButton.Click += new EventHandler(this.PrintButton_Click);
			// 
			// FormCancelButton
			// 
			this.FormCancelButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.FormCancelButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|a17fdc48-a4c2-4235-bc23-e7f4feb3baa7", "Cancel");
			this.FormCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(875, 469, true);
			this.FormCancelButton.Name = "FormCancelButton";
			this.FormCancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.FormCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.FormCancelButton.TabIndex = 6;
			this.FormCancelButton.Click += new EventHandler(this.FormCancelButton_Click);
			// 
			// CurrencyGuidFindBox
			// 
			this.CurrencyGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrencyGuidFindBox, "RX_PK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Statement)(null)).RX_PK)));
			this.CurrencyGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|b4b5399e-8b9d-4d47-97b6-49e8a2505f97", "Currency", "Transaction Currency", "");
			this.CurrencyGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 19, true);
			this.CurrencyGuidFindBox.Name = "CurrencyGuidFindBox";
			this.CurrencyGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 17, true);
			this.CurrencyGuidFindBox.TabIndex = 0;
			// 
			// OrganisationGroupBox
			// 
			this.OrganisationGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|62dacc30-d665-4565-9281-51e06b7ee646", "Select which Debtors(s) to Print For");
			this.OrganisationGroupBox.Controls.Add(this.zDropEdit5);
			this.OrganisationGroupBox.Controls.Add(this.CreditRatingDropEdit);
			this.OrganisationGroupBox.Controls.Add(this.AccountsRelationshipDropEdit);
			this.OrganisationGroupBox.Controls.Add(this.ConsolidationCategoryDropEdit);
			this.OrganisationGroupBox.Controls.Add(this.zGuidFindBox4);
			this.OrganisationGroupBox.Controls.Add(this.zGuidFindBox3);
			this.OrganisationGroupBox.Controls.Add(this.SalesRepFindBox);
			this.OrganisationGroupBox.Controls.Add(this.DebtorGroupFindBox);
			this.OrganisationGroupBox.Controls.Add(this.OrganisationBranchFindBox);
			this.OrganisationGroupBox.Controls.Add(this.OrganisationGuidFindBox);
			this.OrganisationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 139, true);
			this.OrganisationGroupBox.Name = "OrganisationGroupBox";
			this.OrganisationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(941, 170, true);
			this.OrganisationGroupBox.TabIndex = 2;
			this.OrganisationGroupBox.TabStop = false;
			// 
			// zDropEdit5
			// 
			this.zDropEdit5.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit5, "CreditStatements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Statement)(null)).CreditStatements)));
			this.zDropEdit5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|8f2b2bde-7bac-441e-9749-cb8116629ba2", "Credit Statements");
			this.zDropEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(618, 135, true);
			this.zDropEdit5.Name = "zDropEdit5";
			this.zDropEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 17, true);
			this.zDropEdit5.TabIndex = 9;
			// 
			// CreditRatingDropEdit
			// 
			this.CreditRatingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CreditRatingDropEdit, "CreditRating");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Statement)(null)).CreditRating)));
			this.CreditRatingDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|4956bd7d-a36b-4ba9-b278-ae841ab47813", "Credit Rating", "The Credit Rating.");
			this.CreditRatingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(618, 72, true);
			this.CreditRatingDropEdit.Name = "CreditRatingDropEdit";
			this.CreditRatingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 17, true);
			this.CreditRatingDropEdit.TabIndex = 5;
			// 
			// AccountsRelationshipDropEdit
			// 
			this.AccountsRelationshipDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AccountsRelationshipDropEdit, "AccountsRelationShip");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Statement)(null)).AccountsRelationShip)));
			this.AccountsRelationshipDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|895d5613-4661-4117-9912-e5dd3ee922d9", "Accounts Relationship", "The Accounts Relationship.");
			this.AccountsRelationshipDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(618, 23, true);
			this.AccountsRelationshipDropEdit.Name = "AccountsRelationshipDropEdit";
			this.AccountsRelationshipDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 17, true);
			this.AccountsRelationshipDropEdit.TabIndex = 1;
			// 
			// ConsolidationCategoryDropEdit
			// 
			this.ConsolidationCategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsolidationCategoryDropEdit, "ConsolidationCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Statement)(null)).ConsolidationCategory)));
			this.ConsolidationCategoryDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|984fce74-499b-4660-a64e-e9f1c93f8ebf", "Consolidation Category", "The Consolidation Category.");
			this.ConsolidationCategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(618, 47, true);
			this.ConsolidationCategoryDropEdit.Name = "ConsolidationCategoryDropEdit";
			this.ConsolidationCategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 17, true);
			this.ConsolidationCategoryDropEdit.TabIndex = 3;
			// 
			// zGuidFindBox4
			// 
			this.zGuidFindBox4.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox4, "CreditController_PK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Statement)(null)).CreditController_PK)));
			this.zGuidFindBox4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|f5a4914b-b866-4d3f-9224-941402bfa7a4", "Credit Controller", "The Credit Controller.");
			this.zGuidFindBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(618, 109, true);
			this.zGuidFindBox4.Name = "zGuidFindBox4";
			this.zGuidFindBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 17, true);
			this.zGuidFindBox4.TabIndex = 7;
			// 
			// zGuidFindBox3
			// 
			this.zGuidFindBox3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox3, "CustomerServiceRep_PK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Statement)(null)).CustomerServiceRep_PK)));
			this.zGuidFindBox3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|2b4ef836-d43b-46b2-b04e-c6e76a1dbba6", "Customer Service Rep", "The Customer Service Representative.");
			this.zGuidFindBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 135, true);
			this.zGuidFindBox3.Name = "zGuidFindBox3";
			this.zGuidFindBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 17, true);
			this.zGuidFindBox3.TabIndex = 8;
			// 
			// SalesRepFindBox
			// 
			this.SalesRepFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SalesRepFindBox, "SalesRep_PK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Statement)(null)).SalesRep_PK)));
			this.SalesRepFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|6c42baf4-737e-4623-a001-ad62e9438c29", "Sales Rep", "The Sales Representative.");
			this.SalesRepFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 109, true);
			this.SalesRepFindBox.Name = "SalesRepFindBox";
			this.SalesRepFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 17, true);
			this.SalesRepFindBox.TabIndex = 6;
			// 
			// DebtorGroupFindBox
			// 
			this.DebtorGroupFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DebtorGroupFindBox, "OJ_PK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Statement)(null)).OJ_PK)));
			this.DebtorGroupFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|1eee166a-2bc6-44d0-95b6-0007c0986668", "Group", "Debtor Group", "");
			this.DebtorGroupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 46, true);
			this.DebtorGroupFindBox.Name = "DebtorGroupFindBox";
			this.DebtorGroupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 17, true);
			this.DebtorGroupFindBox.TabIndex = 2;
			// 
			// OrganisationBranchFindBox
			// 
			this.OrganisationBranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganisationBranchFindBox, "GB_PK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Statement)(null)).GB_PK)));
			this.OrganisationBranchFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|f60c8097-4d69-448d-b286-968ff41338f1", "Branch", "Organization Branch", "");
			this.OrganisationBranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 69, true);
			this.OrganisationBranchFindBox.Name = "OrganisationBranchFindBox";
			this.OrganisationBranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 17, true);
			this.OrganisationBranchFindBox.TabIndex = 4;
			// 
			// OrganisationGuidFindBox
			// 
			this.OrganisationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganisationGuidFindBox, "OH_PK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Statement)(null)).OH_PK)));
			this.OrganisationGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|48657934-9507-4382-bb1a-ed47e671872b", "Debtor", "Debtor", "");
			this.OrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 23, true);
			this.OrganisationGuidFindBox.Name = "OrganisationGuidFindBox";
			this.OrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 17, true);
			this.OrganisationGuidFindBox.TabIndex = 0;
			// 
			// TransactionGroupBox
			// 
			this.TransactionGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|6e33b5ba-4de9-4b5e-80b3-efa0e4ae534f", "Only Print Transactions with the Following Criteria");
			this.TransactionGroupBox.Controls.Add(this.IncludeTransactionsInActiveBatchCheckBox);
			this.TransactionGroupBox.Controls.Add(this.DepartmentGuidFindBox);
			this.TransactionGroupBox.Controls.Add(this.TransactionBranchFindBox);
			this.TransactionGroupBox.Controls.Add(this.DisbursementInvoicesCheckBox);
			this.TransactionGroupBox.Controls.Add(this.OutstandingAmountCalcEdit);
			this.TransactionGroupBox.Controls.Add(this.CutoffDateEdit);
			this.TransactionGroupBox.Controls.Add(this.CurrencyGuidFindBox);
			this.TransactionGroupBox.Controls.Add(this.CutoffPeriodEdit);
			this.TransactionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 312, true);
			this.TransactionGroupBox.Name = "TransactionGroupBox";
			this.TransactionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 149, true);
			this.TransactionGroupBox.TabIndex = 3;
			this.TransactionGroupBox.TabStop = false;
			// 
			// IncludeTransactionsInActiveBatchCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IncludeTransactionsInActiveBatchCheckBox, "IncludeTransactionsInActiveBatch");
			this.IncludeTransactionsInActiveBatchCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|D0D89401-0C37-4107-B298-BFD4C5706EC5", "Include Transactions In Active Collection Batches", "If ticked, transactions in active collections batches where collection Batch Type is ticked as Include will be added in the list, otherwise none of transactions in active collections batches will be listed. You can configure Batch Type in Registry > Accounting > Receivable Defaults > Default Settings > Collection Batch Types.");
			this.IncludeTransactionsInActiveBatchCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeTransactionsInActiveBatchCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 107, true);
			this.IncludeTransactionsInActiveBatchCheckBox.Name = nameof(IncludeTransactionsInActiveBatchCheckBox);
			this.IncludeTransactionsInActiveBatchCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 22, true);
			this.IncludeTransactionsInActiveBatchCheckBox.TabIndex = 6;
			// 
			// DepartmentGuidFindBox
			// 
			this.DepartmentGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartmentGuidFindBox, "TransactionDepartment_PK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Statement)(null)).TransactionDepartment_PK)));
			this.DepartmentGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|2480f817-cfa0-450d-b31c-187a5720f0bf", "Transaction Department");
			this.DepartmentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 63, true);
			this.DepartmentGuidFindBox.Name = "DepartmentGuidFindBox";
			this.DepartmentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 17, true);
			this.DepartmentGuidFindBox.TabIndex = 2;
			// 
			// TransactionBranchFindBox
			// 
			this.TransactionBranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransactionBranchFindBox, "TransactionBranch_PK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Statement)(null)).TransactionBranch_PK)));
			this.TransactionBranchFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|a08ab9b0-2cf8-4e12-9335-c081f79de68b", "Branch", "Transaction Branch", "");
			this.TransactionBranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 41, true);
			this.TransactionBranchFindBox.Name = "TransactionBranchFindBox";
			this.TransactionBranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 17, true);
			this.TransactionBranchFindBox.TabIndex = 1;
			// 
			// DisbursementInvoicesCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DisbursementInvoicesCheckBox, "DisbursementInvoicesOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Statement)(null)).DisbursementInvoicesOnly)));
			this.DisbursementInvoicesCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|f3599200-606c-4809-9500-db3a6276352a", "Disbursement Invoices Only");
			this.DisbursementInvoicesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DisbursementInvoicesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 126, true);
			this.DisbursementInvoicesCheckBox.Name = "DisbursementInvoicesCheckBox";
			this.DisbursementInvoicesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 22, true);
			this.DisbursementInvoicesCheckBox.TabIndex = 5;
			// 
			// OutstandingAmountCalcEdit
			// 
			this.OutstandingAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OutstandingAmountCalcEdit, "OutstandingAmountGreaterThan");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Statement)(null)).OutstandingAmountGreaterThan)));
			this.OutstandingAmountCalcEdit.DecimalPlaces = 2;
			this.OutstandingAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 85, true);
			this.OutstandingAmountCalcEdit.Name = "OutstandingAmountCalcEdit";
			this.OutstandingAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.OutstandingAmountCalcEdit.TabIndex = 4;
			this.OutstandingAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CutoffDateEdit
			// 
			this.CutoffDateEdit.AllowDrop = true;
			this.CutoffDateEdit.AutoCompleteMonthThreshold = 1;
			this.CutoffDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CutoffDateEdit, "CutOffDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Statement)(null)).CutOffDate)));
			this.CutoffDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|4ba15905-50e2-4a3c-a88c-183bcf501ba8", "Cutoff Date");
			this.CutoffDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 85, true);
			this.CutoffDateEdit.Name = "CutoffDateEdit";
			this.CutoffDateEdit.TabIndex = 3;
			// 
			// CutoffPeriodEdit
			// 
			this.CutoffPeriodEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CutoffPeriodEdit, "CutOffPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZInt)(((Statement)(null)).CutOffPeriod)));
			this.CutoffPeriodEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|60610074-16e0-4918-8003-368866a8f84b", "Cutoff Period", "Statements as at end of selected period", "");
			this.CutoffPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 85, true);
			this.CutoffPeriodEdit.Name = "CutoffPeriodEdit";
			this.CutoffPeriodEdit.TabIndex = 3;
			// 
			// DocumentGroupBox
			// 
			this.DocumentGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|69bc48c0-8130-4101-8cd7-62dd9f1bf26f", "Select the Type of Document to Print");
			this.DocumentGroupBox.Controls.Add(this.IncludeDebtorSummaryPageCheckBox);
			this.DocumentGroupBox.Controls.Add(this.DepartmentCheckBox);
			this.DocumentGroupBox.Controls.Add(this.IssueStatementPackDropEdit);
			this.DocumentGroupBox.Controls.Add(this.TypeOfDocumentToPrintDropEdit);
			this.DocumentGroupBox.Controls.Add(this.IssueStatementsBySettlementGroupCheckBox);
			this.DocumentGroupBox.Controls.Add(this.IssueByTransactionBranchCheckBox);
			this.DocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.DocumentGroupBox.Name = "DocumentGroupBox";
			this.DocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(941, 73, true);
			this.DocumentGroupBox.TabIndex = 0;
			this.DocumentGroupBox.TabStop = false;
			// 
			// IncludeDebtorSummaryPageCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IncludeDebtorSummaryPageCheckBox, "IncludeDebtorSummaryPage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Statement)(null)).IncludeDebtorSummaryPage)));
			this.IncludeDebtorSummaryPageCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|dd0078df-adef-4233-9f93-32ed94232984", "Include Summary Page");
			this.IncludeDebtorSummaryPageCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeDebtorSummaryPageCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(482, 18, true);
			this.IncludeDebtorSummaryPageCheckBox.Name = "IncludeDebtorSummaryPageCheckBox";
			this.IncludeDebtorSummaryPageCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 22, true);
			this.IncludeDebtorSummaryPageCheckBox.TabIndex = 2;
			// 
			// DepartmentCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DepartmentCheckBox, "IssueByTransactionDepartment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Statement)(null)).IssueByTransactionDepartment)));
			this.DepartmentCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|4080d194-5bce-410b-b0fc-db8a5cbfc318", "Separate by Transaction Department");
			this.DepartmentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.DepartmentCheckBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.DepartmentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 46, true);
			this.DepartmentCheckBox.Name = "DepartmentCheckBox";
			this.DepartmentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 22, true);
			this.DepartmentCheckBox.TabIndex = 5;
			// 
			// IssueStatementPackDropEdit
			// 
			this.IssueStatementPackDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IssueStatementPackDropEdit, "IssueStatementPack");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Statement)(null)).IssueStatementPack)));
			this.IssueStatementPackDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|29971baf-634a-4ea3-a2ad-faf541bddf97", "Issue Statement Pack");
			this.IssueStatementPackDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 46, true);
			this.IssueStatementPackDropEdit.Name = "IssueStatementPackDropEdit";
			this.IssueStatementPackDropEdit.PreBoundMaxLength = 4;
			this.IssueStatementPackDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 17, true);
			this.IssueStatementPackDropEdit.TabIndex = 1;
			// 
			// TypeOfDocumentToPrintDropEdit
			// 
			this.TypeOfDocumentToPrintDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeOfDocumentToPrintDropEdit, "DocumentToPrint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Statement)(null)).DocumentToPrint)));
			this.TypeOfDocumentToPrintDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|ddf8f230-dc46-4ea0-aa41-aa8d6fbdd42c", "Document To Print", "Document To Print", "");
			this.TypeOfDocumentToPrintDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 20, true);
			this.TypeOfDocumentToPrintDropEdit.Name = "TypeOfDocumentToPrintDropEdit";
			this.TypeOfDocumentToPrintDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 17, true);
			this.TypeOfDocumentToPrintDropEdit.TabIndex = 0;
			// 
			// IssueStatementsBySettlementGroupCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IssueStatementsBySettlementGroupCheckBox, "IssueBySettlementGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Statement)(null)).IssueBySettlementGroup)));
			this.IssueStatementsBySettlementGroupCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|4df78d76-94bd-40a8-b11e-0cfe37052743", "Issue by Settlement Group");
			this.IssueStatementsBySettlementGroupCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.IssueStatementsBySettlementGroupCheckBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.IssueStatementsBySettlementGroupCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(482, 46, true);
			this.IssueStatementsBySettlementGroupCheckBox.Name = "IssueStatementsBySettlementGroupCheckBox";
			this.IssueStatementsBySettlementGroupCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 22, true);
			this.IssueStatementsBySettlementGroupCheckBox.TabIndex = 3;
			// 
			// IssueByTransactionBranchCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IssueByTransactionBranchCheckBox, "IssueByTransactionBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Statement)(null)).IssueByTransactionBranch)));
			this.IssueByTransactionBranchCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|8cd00716-6735-4ca2-b1b8-219b6e0badac", "Separate by Transaction Branch");
			this.IssueByTransactionBranchCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.IssueByTransactionBranchCheckBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.IssueByTransactionBranchCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 20, true);
			this.IssueByTransactionBranchCheckBox.Name = "IssueByTransactionBranchCheckBox";
			this.IssueByTransactionBranchCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 23, true);
			this.IssueByTransactionBranchCheckBox.TabIndex = 4;
			// 
			// grpBxAccountMovement
			// 
			this.grpBxAccountMovement.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d057e6e9-d57a-4c7f-b253-515aaf379baa", "Account Movement");
			this.grpBxAccountMovement.Controls.Add(this.dedPrintAccMovementSOAGroupBy);
			this.grpBxAccountMovement.Controls.Add(this.dtpPrintAccMovementSOATo);
			this.grpBxAccountMovement.Controls.Add(this.dtpPrintAccMovementSOAFrom);
			this.grpBxAccountMovement.Controls.Add(this.chkPrintAccMovementSOA);
			this.grpBxAccountMovement.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 86, true);
			this.grpBxAccountMovement.Name = "grpBxAccountMovement";
			this.grpBxAccountMovement.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(941, 49, true);
			this.grpBxAccountMovement.TabIndex = 1;
			this.grpBxAccountMovement.TabStop = false;
			// 
			// dedPrintAccMovementSOAGroupBy
			// 
			this.dedPrintAccMovementSOAGroupBy.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dedPrintAccMovementSOAGroupBy, "GroupByAccountMovementSOALine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Statement)(null)).GroupByAccountMovementSOALine)));
			this.dedPrintAccMovementSOAGroupBy.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0d3db0bd-d2de-4000-bc0c-eb2ba7b620e0", "Group By");
			this.dedPrintAccMovementSOAGroupBy.Enabled = false;
			this.dedPrintAccMovementSOAGroupBy.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(403, 21, true);
			this.dedPrintAccMovementSOAGroupBy.Name = "dedPrintAccMovementSOAGroupBy";
			this.dedPrintAccMovementSOAGroupBy.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 17, true);
			this.dedPrintAccMovementSOAGroupBy.TabIndex = 3;
			// 
			// dtpPrintAccMovementSOATo
			// 
			this.dtpPrintAccMovementSOATo.AllowDrop = true;
			this.dtpPrintAccMovementSOATo.AutoCompleteMonthThreshold = 1;
			this.dtpPrintAccMovementSOATo.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dtpPrintAccMovementSOATo, "PrintAccountMovementToDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Statement)(null)).PrintAccountMovementToDate)));
			this.dtpPrintAccMovementSOATo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c0975a93-a725-4d9a-bfb0-75ed9a0e0922", "To");
			this.dtpPrintAccMovementSOATo.Enabled = false;
			this.dtpPrintAccMovementSOATo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 21, true);
			this.dtpPrintAccMovementSOATo.Name = "dtpPrintAccMovementSOATo";
			this.dtpPrintAccMovementSOATo.TabIndex = 2;
			// 
			// dtpPrintAccMovementSOAFrom
			// 
			this.dtpPrintAccMovementSOAFrom.AllowDrop = true;
			this.dtpPrintAccMovementSOAFrom.AutoCompleteMonthThreshold = 1;
			this.dtpPrintAccMovementSOAFrom.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dtpPrintAccMovementSOAFrom, "PrintAccountMovementFromDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Statement)(null)).PrintAccountMovementFromDate)));
			this.dtpPrintAccMovementSOAFrom.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ceedcf5a-27d8-41cb-a289-e3b1748f6e60", "Post Date From");
			this.dtpPrintAccMovementSOAFrom.Enabled = false;
			this.dtpPrintAccMovementSOAFrom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 21, true);
			this.dtpPrintAccMovementSOAFrom.Name = "dtpPrintAccMovementSOAFrom";
			this.dtpPrintAccMovementSOAFrom.TabIndex = 1;
			// 
			// chkPrintAccMovementSOA
			// 
			this.chkPrintAccMovementSOA.AutoSize = true;
			this.BindingSource.SetBindingMember(this.chkPrintAccMovementSOA, "PrintAccountMovementSOA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Statement)(null)).PrintAccountMovementSOA)));
			this.chkPrintAccMovementSOA.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("25132f7f-b925-4e17-b2fa-06f20d83b8ae", "Print Account Movement Listing");
			this.chkPrintAccMovementSOA.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkPrintAccMovementSOA.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.chkPrintAccMovementSOA.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 0, true);
			this.chkPrintAccMovementSOA.Name = "chkPrintAccMovementSOA";
			this.chkPrintAccMovementSOA.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.chkPrintAccMovementSOA.TabIndex = 0;
			this.chkPrintAccMovementSOA.UseVisualStyleBackColor = true;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0e34d148-cbcc-42cb-914e-b678330c9ff0", "Account Fee");
			this.zGroupBox1.Controls.Add(this.accFeeTaxIDMessageLabel);
			this.zGroupBox1.Controls.Add(this.AL_AT_GSTTaxIDGuidFindBox);
			this.zGroupBox1.Controls.Add(this.dtpAccFeeToDate);
			this.zGroupBox1.Controls.Add(this.dtpAccFeeFromDate);
			this.zGroupBox1.Controls.Add(this.txtInvoiceDescription);
			this.zGroupBox1.Controls.Add(this.chkFeePosting);
			this.zGroupBox1.Controls.Add(this.dtpAccFeePostDate);
			this.zGroupBox1.Controls.Add(this.dtpAccFeeInvoiceDate);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 313, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 149, true);
			this.zGroupBox1.TabIndex = 4;
			this.zGroupBox1.TabStop = false;
			// 
			// accFeeTaxIDMessageLabel
			// 
			this.accFeeTaxIDMessageLabel.AutoSize = true;
			this.accFeeTaxIDMessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("61a57258-5590-4bf3-a300-58ad636603f6", "Applied only if Tax is applicable for a Debtor");
			this.accFeeTaxIDMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 124, true);
			this.accFeeTaxIDMessageLabel.Name = "accFeeTaxIDMessageLabel";
			this.accFeeTaxIDMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 13, true);
			this.accFeeTaxIDMessageLabel.TabIndex = 7;
			// 
			// AL_AT_GSTTaxIDGuidFindBox
			// 
			this.AL_AT_GSTTaxIDGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AL_AT_GSTTaxIDGuidFindBox, "AccountFeeInvoiceCreator.AccFeeTaxID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Statement)(null)).AccountFeeInvoiceCreator.AccFeeTaxID)));
			this.AL_AT_GSTTaxIDGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("df102ac7-02cc-4576-a6ec-f800eabaf579", "GST Tax ID");
			this.AL_AT_GSTTaxIDGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 99, true);
			this.AL_AT_GSTTaxIDGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccTaxRate;
			this.AL_AT_GSTTaxIDGuidFindBox.Name = "AL_AT_GSTTaxIDGuidFindBox";
			this.AL_AT_GSTTaxIDGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 17, true);
			this.AL_AT_GSTTaxIDGuidFindBox.TabIndex = 6;
			// 
			// dtpAccFeeToDate
			// 
			this.dtpAccFeeToDate.AllowDrop = true;
			this.dtpAccFeeToDate.AutoCompleteMonthThreshold = 1;
			this.dtpAccFeeToDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dtpAccFeeToDate, "AccountFeeInvoiceCreator+AccFeeToDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Statement)(null)).AccountFeeInvoiceCreator.AccFeeToDate)));
			this.dtpAccFeeToDate.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8eca55f3-31f3-41b1-9508-b916ec56fe70", "To");
			this.dtpAccFeeToDate.Enabled = false;
			this.dtpAccFeeToDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 26, true);
			this.dtpAccFeeToDate.Name = "dtpAccFeeToDate";
			this.dtpAccFeeToDate.TabIndex = 2;
			// 
			// dtpAccFeeFromDate
			// 
			this.dtpAccFeeFromDate.AllowDrop = true;
			this.dtpAccFeeFromDate.AutoCompleteMonthThreshold = 1;
			this.dtpAccFeeFromDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dtpAccFeeFromDate, "AccountFeeInvoiceCreator+AccFeeFromDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Statement)(null)).AccountFeeInvoiceCreator.AccFeeFromDate)));
			this.dtpAccFeeFromDate.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("90f13ff0-5f4c-422d-ba1b-1d9e9f15586a", "App. Period From");
			this.dtpAccFeeFromDate.Enabled = false;
			this.dtpAccFeeFromDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 26, true);
			this.dtpAccFeeFromDate.Name = "dtpAccFeeFromDate";
			this.dtpAccFeeFromDate.TabIndex = 1;
			// 
			// txtInvoiceDescription
			// 
			this.BindingSource.SetBindingMember(this.txtInvoiceDescription, "AccountFeeInvoiceCreator+AccFeeInvoiceDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Statement)(null)).AccountFeeInvoiceCreator.AccFeeInvoiceDescription)));
			this.txtInvoiceDescription.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0bac3ada-65d0-49e4-b37a-5617442614b5", "Invoice Header Desc.");
			this.txtInvoiceDescription.Enabled = false;
			this.txtInvoiceDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 50, true);
			this.txtInvoiceDescription.Name = "txtInvoiceDescription";
			this.txtInvoiceDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 17, true);
			this.txtInvoiceDescription.TabIndex = 3;
			// 
			// chkFeePosting
			// 
			this.chkFeePosting.AutoSize = true;
			this.BindingSource.SetBindingMember(this.chkFeePosting, "DoAccountFeeTransaction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Statement)(null)).DoAccountFeeTransaction)));
			this.chkFeePosting.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("979f11fe-82a7-48a1-82ee-b51c4e639863", "Enable Fee Posting");
			this.chkFeePosting.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkFeePosting.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.chkFeePosting.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 0, true);
			this.chkFeePosting.Name = "chkFeePosting";
			this.chkFeePosting.AutoSize = true;
			this.chkFeePosting.TabIndex = 0;
			// 
			// dtpAccFeePostDate
			// 
			this.dtpAccFeePostDate.AllowDrop = true;
			this.dtpAccFeePostDate.AutoCompleteMonthThreshold = 1;
			this.dtpAccFeePostDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dtpAccFeePostDate, "AccountFeeInvoiceCreator+AccFeePostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Statement)(null)).AccountFeeInvoiceCreator.AccFeePostDate)));
			this.dtpAccFeePostDate.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d365b457-816a-4f65-bd04-03c31c3169e2", "Post Date");
			this.dtpAccFeePostDate.Enabled = false;
			this.dtpAccFeePostDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 74, true);
			this.dtpAccFeePostDate.Name = "dtpAccFeePostDate";
			this.dtpAccFeePostDate.TabIndex = 5;
			// 
			// dtpAccFeeInvoiceDate
			// 
			this.dtpAccFeeInvoiceDate.AllowDrop = true;
			this.dtpAccFeeInvoiceDate.AutoCompleteMonthThreshold = 1;
			this.dtpAccFeeInvoiceDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dtpAccFeeInvoiceDate, "AccountFeeInvoiceCreator+AccFeeInvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Statement)(null)).AccountFeeInvoiceCreator.AccFeeInvoiceDate)));
			this.dtpAccFeeInvoiceDate.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ae44b3c3-94c8-4298-8eb9-fb356968c4e3", "Invoice Date");
			this.dtpAccFeeInvoiceDate.Enabled = false;
			this.dtpAccFeeInvoiceDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 74, true);
			this.dtpAccFeeInvoiceDate.Name = "dtpAccFeeInvoiceDate";
			this.dtpAccFeeInvoiceDate.TabIndex = 4;
			// 
			// StatementPrintForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StatementPrintForm|388d3a55-92d7-4a67-bf3b-18fea863e92a", "Print Statement Of Account / Collection Letter");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 515, true);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.grpBxAccountMovement);
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.FormCancelButton);
			this.Controls.Add(this.DocumentGroupBox);
			this.Controls.Add(this.TransactionGroupBox);
			this.Controls.Add(this.OrganisationGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Statement);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.Invoicing.Statement";
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "StatementPrintForm";
			this.Controls.SetChildIndex(this.OrganisationGroupBox, 0);
			this.Controls.SetChildIndex(this.TransactionGroupBox, 0);
			this.Controls.SetChildIndex(this.DocumentGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FormCancelButton, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.grpBxAccountMovement, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CurrencyGuidFindBox.ResumeLayout(true);
			this.CurrencyGuidFindBox.PerformLayout();
			this.OrganisationGroupBox.ResumeLayout(false);
			this.OrganisationGroupBox.PerformLayout();
			this.zDropEdit5.ResumeLayout(true);
			this.zDropEdit5.PerformLayout();
			this.CreditRatingDropEdit.ResumeLayout(true);
			this.CreditRatingDropEdit.PerformLayout();
			this.AccountsRelationshipDropEdit.ResumeLayout(true);
			this.AccountsRelationshipDropEdit.PerformLayout();
			this.ConsolidationCategoryDropEdit.ResumeLayout(true);
			this.ConsolidationCategoryDropEdit.PerformLayout();
			this.zGuidFindBox4.ResumeLayout(true);
			this.zGuidFindBox4.PerformLayout();
			this.zGuidFindBox3.ResumeLayout(true);
			this.zGuidFindBox3.PerformLayout();
			this.SalesRepFindBox.ResumeLayout(true);
			this.SalesRepFindBox.PerformLayout();
			this.DebtorGroupFindBox.ResumeLayout(true);
			this.DebtorGroupFindBox.PerformLayout();
			this.OrganisationBranchFindBox.ResumeLayout(true);
			this.OrganisationBranchFindBox.PerformLayout();
			this.OrganisationGuidFindBox.ResumeLayout(true);
			this.OrganisationGuidFindBox.PerformLayout();
			this.TransactionGroupBox.ResumeLayout(false);
			this.TransactionGroupBox.PerformLayout();
			this.DepartmentGuidFindBox.ResumeLayout(true);
			this.DepartmentGuidFindBox.PerformLayout();
			this.TransactionBranchFindBox.ResumeLayout(true);
			this.TransactionBranchFindBox.PerformLayout();
			this.CutoffDateEdit.ResumeLayout(true);
			this.CutoffDateEdit.PerformLayout();
			this.DocumentGroupBox.ResumeLayout(false);
			this.DocumentGroupBox.PerformLayout();
			this.IssueStatementPackDropEdit.ResumeLayout(true);
			this.IssueStatementPackDropEdit.PerformLayout();
			this.TypeOfDocumentToPrintDropEdit.ResumeLayout(true);
			this.TypeOfDocumentToPrintDropEdit.PerformLayout();
			this.grpBxAccountMovement.ResumeLayout(false);
			this.grpBxAccountMovement.PerformLayout();
			this.dedPrintAccMovementSOAGroupBy.ResumeLayout(true);
			this.dedPrintAccMovementSOAGroupBy.PerformLayout();
			this.dtpPrintAccMovementSOATo.ResumeLayout(true);
			this.dtpPrintAccMovementSOATo.PerformLayout();
			this.dtpPrintAccMovementSOAFrom.ResumeLayout(true);
			this.dtpPrintAccMovementSOAFrom.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.AL_AT_GSTTaxIDGuidFindBox.ResumeLayout(true);
			this.AL_AT_GSTTaxIDGuidFindBox.PerformLayout();
			this.dtpAccFeeToDate.ResumeLayout(true);
			this.dtpAccFeeToDate.PerformLayout();
			this.dtpAccFeeFromDate.ResumeLayout(true);
			this.dtpAccFeeFromDate.PerformLayout();
			this.dtpAccFeePostDate.ResumeLayout(true);
			this.dtpAccFeePostDate.PerformLayout();
			this.dtpAccFeeInvoiceDate.ResumeLayout(true);
			this.dtpAccFeeInvoiceDate.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}
