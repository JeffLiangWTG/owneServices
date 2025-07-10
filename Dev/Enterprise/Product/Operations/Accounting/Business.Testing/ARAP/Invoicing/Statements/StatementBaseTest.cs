using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AccountMovementLineGroupingOptions = Enterprise.Accounting.Business.ARAP.Invoicing.Statement.AccountMovementLineGroupingOptions;
using Constants = Enterprise.Accounting.Business.ARAP.Invoicing.Statement.Constants;
using CreditOptions = Enterprise.Accounting.Business.ARAP.Invoicing.Statement.CreditOptions;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class StatementBaseTest : NonPersistentBusinessObjectTestCase
	{
		#region Test construction and Default values

		public void TestSetDefaultValues()
		{
			AssertEquals("Default: DocumentToPrint", Core.Constants.StatementCollectionLetterType.StatementOfAccount, Statement.DocumentToPrint);
			AssertEquals("Default: Issue by Settlement Group", false, Statement.IssueBySettlementGroup);

			AssertEquals("Default: Organisation", ZGuid.Empty, Statement.OH_PK);
			AssertEquals("Default: Debtor Group", ZGuid.Empty, Statement.OJ_PK);
			AssertEquals("Default: Branch", ZGuid.Empty, Statement.GB_PK);
			AssertEquals("Default: Accounts Relationship", ZString.Empty, Statement.AccountsRelationShip);
			AssertEquals("Default: ConsolidationCategory", ZString.Empty, Statement.ConsolidationCategory);
			AssertEquals("Default: Credit Rating", ZString.Empty, Statement.CreditRating);
			AssertEquals("Default: Sales Rep", ZGuid.Empty, Statement.SalesRep_PK);
			AssertEquals("Default: Customer Service Rep", ZGuid.Empty, Statement.CustomerServiceRep_PK);
			AssertEquals("Default: Credit Controller", ZGuid.Empty, Statement.CreditController_PK);
			AssertEquals("Default: Credit Statements", CreditOptions.ExcludeDocumentsInCredit, Statement.CreditStatements);

			AssertEquals("Default: Currency", ZGuid.Empty, Statement.RX_PK);
			AssertEquals("Default: Cut off Date", ZDateTime.Empty, Statement.CutOffDate);
			AssertEquals("Default: Cut off Period", 0, Statement.CutOffPeriod);
			AssertEquals("Default: Outstanding Amount Greater than ", 0M, Statement.OutstandingAmountGreaterThan);
			AssertEquals("Default: Disbursement Invoice only", false, Statement.DisbursementInvoicesOnly);
			AssertEquals("Default: Include Transactions In Active Batch", true, Statement.IncludeTransactionsInActiveBatch);
			AssertEquals("Default: IssueStatementPack", AccountingConstants.IssueStatementPackType.Default, Statement.IssueStatementPack);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorEmpty()
		{
			Statement.New(null);
		}

		#endregion

		#region Test Validation

		public void TestNoValidateAccountFeeInvoiceFieldsWhenEnableFeePostingCheckboxDisabled()
		{
			AssertEquals("AccountFeeInvoiceCreator has errors", false, Statement.AccountFeeInvoiceCreator.HasErrors());
			Statement.CutOffDate = ZDateTime.Now;
			Statement.CutOffPeriod = Statement.PeriodCalculator_ForTestOnly.GetPeriodFromDate(ZDateTime.Today);
			Statement.PrintAccountMovementFromDate = ZDateTime.Today;
			Statement.PrintAccountMovementToDate = ZDateTime.Today.AddDays(2);
			AssertEquals("AccountFeeInvoiceCreator has errors", false, Statement.AccountFeeInvoiceCreator.HasErrors());
		}

		public void TestValidateDocumentToPrint_ValidCode()
		{
			AssertEquals("DocumentToPrint has errors", false, Statement.DocumentToPrintInfo.HasErrors());

			Statement.DocumentToPrint = "BLA";
			AssertEquals("DocumentToPrint has errors", true, Statement.DocumentToPrintInfo.HasErrors());

			Statement.DocumentToPrint = Statement.DocumentToPrint_List[0].Code;
			AssertEquals("DocumentToPrint has errors", false, Statement.DocumentToPrintInfo.HasErrors());
		}

		public void TestValidateDocumentToPrint_SecurityRights()
		{
			Env.Security.ReceivablesCollectionDocuments.IsAllowed = true;
			CheckErrorIsDisplayedWhenInsuffientSecurityRights(Env.Security.ReceivablesPrintStatement, Core.Constants.StatementCollectionLetterType.StatementOfAccount);
			CheckErrorIsDisplayedWhenInsuffientSecurityRights(Env.Security.ReceivablesPrintFirstReminder, Core.Constants.StatementCollectionLetterType.FirstReminder);
			CheckErrorIsDisplayedWhenInsuffientSecurityRights(Env.Security.ReceivablesPrintSecondReminder, Core.Constants.StatementCollectionLetterType.SecondReminder);
			CheckErrorIsDisplayedWhenInsuffientSecurityRights(Env.Security.ReceivablesPrintCollectionLetter, Core.Constants.StatementCollectionLetterType.CollectionLetter);
			CheckErrorIsDisplayedWhenInsuffientSecurityRights(Env.Security.ReceivablesPrintDemandLetter, Core.Constants.StatementCollectionLetterType.DemandLetter);
		}

		void CheckErrorIsDisplayedWhenInsuffientSecurityRights(SecurityCheckpoint checkPoint, ZString documentCode)
		{
			checkPoint.IsAllowed = true;
			Statement.DocumentToPrint = documentCode;
			Statement.ValidateDocumentToPrint();
			AssertEquals("Precondition: DocumentToPrint Info has errrors", false, Statement.DocumentToPrintInfo.HasErrors());

			checkPoint.IsAllowed = false;
			Statement.ValidateDocumentToPrint();
			AssertEquals("DocumentToPrint Info has errrors", true, Statement.DocumentToPrintInfo.HasErrors());

			ZString errorMessageToDisplay = "You do not have the appropriate security rights to print this type of document." + System.Environment.NewLine;
			errorMessageToDisplay += "If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow Access to:" + System.Environment.NewLine;
			errorMessageToDisplay += checkPoint.DisplayTextPathToSecurityRight;
			AssertEquals("Document Contains Error: " + System.Environment.NewLine + errorMessageToDisplay, true, Statement.DocumentToPrintInfo.HasError(errorMessageToDisplay));

			checkPoint.IsAllowed = true;
			Statement.ValidateDocumentToPrint();
			AssertEquals("DocumentToPrint Info has errrors", false, Statement.DocumentToPrintInfo.HasErrors());
		}

		public void TestValidateOH_PK()
		{
			Statement.ValidateOH_PK();
			AssertEquals("OH_PKInfo has errors", false, Statement.OH_PKInfo.HasErrors());

			ZDBOnlyQuery notDebtorQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery notDebtorSubQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			notDebtorSubQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.False);
			notDebtorSubQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			notDebtorQuery.AddSubQuery(notDebtorSubQuery, JoinCondition.And);

			OrgHeader headerThatIsNotDebtor = Factory.LoadTop1<OrgHeader>(notDebtorQuery);
			Statement.OH_PK = headerThatIsNotDebtor.PK;
			Statement.ValidateOH_PK();
			AssertEquals("OH_PKInfo has errors", true, Statement.OH_PKInfo.HasErrors());

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);

			OrgHeader header = Factory.LoadTop1<OrgHeader>(query);
			Statement.OH_PK = header.PK;
			Statement.ValidateOH_PK();
			AssertEquals("OH_PKInfo has errors", false, Statement.OH_PKInfo.HasErrors());
		}

		public void TestValidateOJ_PK()
		{
			Statement.ValidateOJ_PK();
			AssertEquals("AR Group has errors", false, Statement.OJ_PKInfo.HasErrors());

			Statement.OJ_PK = ZGuid.NewZGuid();
			Statement.ValidateOJ_PK();
			AssertEquals("AR Group has errors", true, Statement.OJ_PKInfo.HasErrors());

			OrgDebtorGroup debtor = Factory.LoadTop1<OrgDebtorGroup>(new ZQuery());
			Statement.OJ_PK = debtor.PK;
			Statement.ValidateOJ_PK();
			AssertEquals("AR Group has errors", false, Statement.OJ_PKInfo.HasErrors());
		}

		public void TestValidateGB_PK()
		{
			Statement.ValidateGB_PK();
			AssertEquals("Branch has errors", false, Statement.GB_PKInfo.HasErrors());

			Statement.GB_PK = ZGuid.NewZGuid();
			Statement.ValidateGB_PK();
			AssertEquals("Branch has errors", true, Statement.GB_PKInfo.HasErrors());

			GlbBranch branch = Factory.LoadTop1<GlbBranch>(new ZQuery());
			Statement.GB_PK = branch.PK;
			Statement.ValidateGB_PK();
			AssertEquals("Branch has errors", false, Statement.GB_PKInfo.HasErrors());
		}

		public void TestValidateAccountsRelationShip()
		{
			AssertEquals("AccountsRelationShip has errors", false, Statement.AccountsRelationShipInfo.HasErrors());

			Statement.AccountsRelationShip = "BLA";
			AssertEquals("AccountsRelationShip has errors", true, Statement.AccountsRelationShipInfo.HasErrors());

			Statement.AccountsRelationShip = Statement.AccountsRelationShip_List[0].Code;
			AssertEquals("AccountsRelationShip has errors", false, Statement.AccountsRelationShipInfo.HasErrors());
		}

		public void TestValidateConsolidationCategory()
		{
			AssertEquals("ConsolidationCategory has errors", false, Statement.ConsolidationCategoryInfo.HasErrors());

			Statement.ConsolidationCategory = "BLA";
			AssertEquals("ConsolidationCategory has errors", true, Statement.ConsolidationCategoryInfo.HasErrors());

			Statement.ConsolidationCategory = Statement.ConsolidationCategory_List[0].Code;
			AssertEquals("ConsolidationCategory has errors", false, Statement.ConsolidationCategoryInfo.HasErrors());
		}

		public void TestValidateCreditRating()
		{
			AssertEquals("CreditRating has errors", false, Statement.CreditRatingInfo.HasErrors());

			Statement.CreditRating = "BLA";
			AssertEquals("CreditRating has errors", true, Statement.CreditRatingInfo.HasErrors());

			Statement.CreditRating = Statement.CreditRating_List[0].Code;
			AssertEquals("CreditRating has errors", false, Statement.CreditRatingInfo.HasErrors());
		}

		public void TestValidateSalesRep_PK()
		{
			Statement.ValidateSalesRep_PK();
			AssertEquals("Sale Rep has errors", false, Statement.SalesRep_PKInfo.HasErrors());

			Statement.SalesRep_PK = ZGuid.NewZGuid();
			AssertEquals("Sale Rep has errors", true, Statement.SalesRep_PKInfo.HasErrors());

			GlbStaff staff = Factory.LoadTop1<GlbStaff>(new ZQuery());
			Statement.SalesRep_PK = staff.PK;
			Statement.ValidateSalesRep_PK();
			AssertEquals("Sale Rep has errors", false, Statement.SalesRep_PKInfo.HasErrors());
		}

		public void TestValidateCustomerServiceRep_PK()
		{
			Statement.ValidateCustomerServiceRep_PK();
			AssertEquals("Customer Service Rep has errors", false, Statement.CustomerServiceRep_PKInfo.HasErrors());

			Statement.CustomerServiceRep_PK = ZGuid.NewZGuid();
			AssertEquals("Customer Service Rep has errors", true, Statement.CustomerServiceRep_PKInfo.HasErrors());

			GlbStaff staff = Factory.LoadTop1<GlbStaff>(new ZQuery());
			Statement.CustomerServiceRep_PK = staff.PK;
			Statement.ValidateCustomerServiceRep_PK();
			AssertEquals("Customer Service Rep has errors", false, Statement.CustomerServiceRep_PKInfo.HasErrors());
		}

		public void TestValidateCreditController_PK()
		{
			Statement.ValidateCreditController_PK();
			AssertEquals("Credit Controller has errors", false, Statement.CreditController_PKInfo.HasErrors());

			Statement.CreditController_PK = ZGuid.NewZGuid();
			AssertEquals("Credit Controller has errors", true, Statement.CreditController_PKInfo.HasErrors());

			GlbStaff staff = Factory.LoadTop1<GlbStaff>(new ZQuery());
			Statement.CreditController_PK = staff.PK;
			Statement.ValidateCreditController_PK();
			AssertEquals("Credit Controller has errors", false, Statement.CreditController_PKInfo.HasErrors());
		}

		public void TestValidateCreditStatements()
		{
			AssertEquals("CreditStatements has errors", false, Statement.CreditStatementsInfo.HasErrors());

			Statement.CreditStatements = "BLA";
			AssertEquals("CreditStatements has errors", true, Statement.CreditStatementsInfo.HasErrors());

			Statement.CreditStatements = Statement.CreditStatements_List[0].Code;
			AssertEquals("CreditStatements has errors", false, Statement.CreditStatementsInfo.HasErrors());
		}

		public void TestValidateIssueStatementPack()
		{
			AssertEquals("IssueStatementPack should not has errors", false, Statement.IssueStatementPackInfo.HasErrors());

			Statement.IssueStatementPack = "BLA";
			AssertEquals("IssueStatementPack has errors", true, Statement.IssueStatementPackInfo.HasErrors());

			Statement.IssueStatementPack = Statement.IssueStatementPack_List[0].Code;
			AssertEquals("IssueStatementPack has errors", false, Statement.IssueStatementPackInfo.HasErrors());
		}

		public void TestValidateRX_PK()
		{
			Statement.ValidateRX_PK();
			AssertEquals("Currency has errors", false, Statement.RX_PKInfo.HasErrors());

			Statement.RX_PK = ZGuid.NewZGuid();
			AssertEquals("Currency has errors", true, Statement.RX_PKInfo.HasErrors());

			RefCurrency currency = Factory.LoadTop1<RefCurrency>(new ZQuery());
			Statement.RX_PK = currency.PK;
			Statement.ValidateRX_PK();
			AssertEquals("Currency has errors", false, Statement.RX_PKInfo.HasErrors());
		}

		public void TestValidateTransactionBranch_PK()
		{
			Statement.ValidateTransactionBranch_PK();
			AssertEquals("Transaction Branch has errors", false, Statement.TransactionBranch_PKInfo.HasErrors());

			Statement.TransactionBranch_PK = ZGuid.NewZGuid();
			Statement.ValidateTransactionBranch_PK();
			AssertEquals("Transaction Branch has errors", true, Statement.TransactionBranch_PKInfo.HasErrors());

			GlbBranch branch = Factory.LoadTop1<GlbBranch>(new ZQuery());
			Statement.TransactionBranch_PK = branch.PK;
			Statement.ValidateTransactionBranch_PK();
			AssertEquals("Transaction Branch has errors", false, Statement.TransactionBranch_PKInfo.HasErrors());
		}

		public void TestValidateTransactionDepartment_PK()
		{
			Statement.ValidateTransactionDepartment_PK();
			AssertEquals("Transaction Department has errors", false, Statement.TransactionDepartment_PKInfo.HasErrors());

			Statement.TransactionDepartment_PK = ZGuid.NewZGuid();
			Statement.ValidateTransactionDepartment_PK();
			AssertEquals("Transaction Department has errors", true, Statement.TransactionDepartment_PKInfo.HasErrors());

			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery());
			Statement.TransactionDepartment_PK = department.PK;
			Statement.ValidateTransactionDepartment_PK();
			AssertEquals("Transaction Department has errors", false, Statement.TransactionDepartment_PKInfo.HasErrors());
		}

		[TestDate(2012, 10, 12, 17, 33, 00)]
		public void TestValidateCutOffDate()
		{
			Statement.ValidateCutOffDate();
			AssertEquals("Cut Off Date has errors", false, Statement.CutOffDateInfo.HasErrors());

			Statement.CutOffDate = ZDateTime.Invalid;
			Statement.ValidateCutOffDate();
			AssertEquals("Cut off has errors", true, Statement.CutOffDateInfo.HasErrors());

			Statement.CutOffDate = ZDateTime.Today;
			Statement.ValidateCutOffDate();
			AssertNoErrors(Statement.CutOffDateInfo);
		}

		[TestDate(2012, 10, 12, 17, 33, 00)]
		public void TestValidateCutOffPeriod()
		{
			Statement.ValidateCutOffPeriod();
			AssertEquals("Cut Off Period has errors", false, Statement.CutOffPeriodInfo.HasErrors());

			Statement.CutOffPeriod = -1;
			Statement.ValidateCutOffPeriod();
			AssertEquals("Cut Off Period has errors", true, Statement.CutOffPeriodInfo.HasErrors());

			Statement.CutOffPeriod = ZInt.Parse(ZDateTime.Now.Year.ToString() + "01");
			Statement.ValidateCutOffPeriod();
			AssertEquals("Cut Off Period has errors", false, Statement.CutOffPeriodInfo.HasErrors());

			AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			int currentPeriod = Statement.PeriodCalculator_ForTestOnly.GetPeriodFromDate(ZDateTime.Today);
			Statement.CutOffPeriod = Statement.PeriodCalculator_ForTestOnly.GetPreviousPeriod(currentPeriod);
			Statement.ValidateCutOffPeriod();
			AssertHasError(Statement.CutOffPeriodInfo, "With the Invoice Payment Web Service enabled it must be only the current period");

			Statement.CutOffPeriod = currentPeriod;
			Statement.ValidateCutOffPeriod();
			AssertNoErrors(Statement.CutOffPeriodInfo);
		}

		public void TestValidateOutstandingAmountGreaterThan()
		{
			Statement.ValidateOutstandingAmountGreaterThan();
			AssertEquals("'Outstanding Amount Greater Than' has errors", false, Statement.OutstandingAmountGreaterThanInfo.HasErrors());

			Statement.OutstandingAmountGreaterThan = -100M;
			Statement.ValidateOutstandingAmountGreaterThan();
			AssertEquals("'Outstanding Amount Greater Than' has errors", true, Statement.OutstandingAmountGreaterThanInfo.HasErrors());

			Statement.OutstandingAmountGreaterThan = 100M;
			Statement.ValidateOutstandingAmountGreaterThan();
			AssertEquals("'Outstanding Amount Greater Than' has errors", false, Statement.OutstandingAmountGreaterThanInfo.HasErrors());
		}

		public void TestValidate_ValidCode()
		{
			AssertEquals("DocumentToPrint has errors", false, Statement.DocumentToPrintInfo.HasErrors());

			Statement.DocumentToPrint = "BLA";
			AssertEquals("DocumentToPrint has errors", true, Statement.DocumentToPrintInfo.HasErrors());

			Statement.DocumentToPrint = Statement.DocumentToPrint_List[0].Code;
			AssertEquals("DocumentToPrint has errors", false, Statement.DocumentToPrintInfo.HasErrors());
		}

		public void TestValidatePrintAccountMovementSOA()
		{
			Statement.DocumentToPrint = Statement.DocumentToPrint_List[Core.Constants.StatementCollectionLetterType.CollectionLetter].Code;
			AssertEquals("GroupByAccountMovementSOALine has NO error", false, Statement.DocumentToPrintInfo.HasErrors());

			Statement.PrintAccountMovementSOA = true;
			AssertEquals("GroupByAccountMovementSOALine has errors", true, Statement.DocumentToPrintInfo.HasErrors());

			Statement.PrintAccountMovementSOA = false;
			AssertEquals("GroupByAccountMovementSOALine NO error", false, Statement.DocumentToPrintInfo.HasErrors());

			Statement.DocumentToPrint = Statement.DocumentToPrint_List[Core.Constants.StatementCollectionLetterType.StatementOfAccount].Code;
			Statement.PrintAccountMovementSOA = true;
			AssertEquals("GroupByAccountMovementSOALine has NO error", false, Statement.DocumentToPrintInfo.HasErrors());

			Statement.DocumentToPrint = Statement.DocumentToPrint_List[Core.Constants.StatementCollectionLetterType.FirstReminder].Code;
			AssertEquals("GroupByAccountMovementSOALine has errors", true, Statement.DocumentToPrintInfo.HasErrors());
		}

		public void TestValidateGroupByAccountMovementSOALine_ValidCode()
		{
			AssertEquals("GroupByAccountMovementSOALine has errors", false, Statement.GroupByAccountMovementSOALineInfo.HasErrors());

			Statement.GroupByAccountMovementSOALine = "BLA";
			AssertEquals("GroupByAccountMovementSOALine has errors", true, Statement.GroupByAccountMovementSOALineInfo.HasErrors());

			Statement.GroupByAccountMovementSOALine = AccountMovementLineGroupingOptions.GROUP_BY_TRANSACTION_BRANCH;
			AssertEquals("GroupByAccountMovementSOALine has errors", false, Statement.DocumentToPrintInfo.HasErrors());
		}

		public void TestValidateGroupByAccountMovementSOALine()
		{
			Statement.PrintAccountMovementSOA = false;
			AssertEquals("GroupByAccountMovementSOALine has NO error", false, Statement.GroupByAccountMovementSOALineInfo.HasErrors());
			Statement.GroupByAccountMovementSOALine = ZString.Empty;
			AssertEquals("GroupByAccountMovementSOALine has NO error", false, Statement.GroupByAccountMovementSOALineInfo.HasErrors());

			Statement.PrintAccountMovementSOA = true;
			Statement.GroupByAccountMovementSOALine = "BLA";
			AssertEquals("GroupByAccountMovementSOALine has errors", true, Statement.GroupByAccountMovementSOALineInfo.HasErrors());

			Statement.GroupByAccountMovementSOALine = "NON";
			AssertEquals("GroupByAccountMovementSOALine has NO error", false, Statement.GroupByAccountMovementSOALineInfo.HasErrors());

			Statement.GroupByAccountMovementSOALine = "BLA";
			AssertEquals("GroupByAccountMovementSOALine has errors", true, Statement.GroupByAccountMovementSOALineInfo.HasErrors());

			Statement.PrintAccountMovementSOA = false;
			AssertEquals("GroupByAccountMovementSOALine has NO error", false, Statement.GroupByAccountMovementSOALineInfo.HasErrors());
		}

		public void TestValidatePrintAccountMovementDates()
		{
			Statement.PrintAccountMovementSOA = false;
			AssertEquals("PrintAccountMovementFromDate has NO error", false, Statement.PrintAccountMovementFromDateInfo.HasErrors());
			AssertEquals("PrintAccountMovementToDate has NO error", false, Statement.PrintAccountMovementToDateInfo.HasErrors());

			Statement.PrintAccountMovementSOA = true;
			Statement.PrintAccountMovementFromDate = ZDateTime.Empty;
			Statement.PrintAccountMovementToDate = ZDateTime.Empty;
			AssertEquals("PrintAccountMovementFromDate has errors", true, Statement.PrintAccountMovementFromDateInfo.HasErrors());
			AssertEquals("PrintAccountMovementToDate has has errors", true, Statement.PrintAccountMovementToDateInfo.HasErrors());

			Statement.PrintAccountMovementFromDate = ZDateTime.Invalid;
			Statement.PrintAccountMovementToDate = ZDateTime.Invalid;
			AssertEquals("PrintAccountMovementFromDate has errors", true, Statement.PrintAccountMovementFromDateInfo.HasErrors());
			AssertEquals("PrintAccountMovementToDate has has errors", true, Statement.PrintAccountMovementToDateInfo.HasErrors());

			Statement.PrintAccountMovementFromDate = ZDateTime.Today;
			Statement.PrintAccountMovementToDate = ZDateTime.Today.AddDays(2);
			AssertEquals("PrintAccountMovementFromDate has NO error", false, Statement.PrintAccountMovementFromDateInfo.HasErrors());
			AssertEquals("PrintAccountMovementToDate has NO error", false, Statement.PrintAccountMovementToDateInfo.HasErrors());

			Statement.PrintAccountMovementFromDate = ZDateTime.Empty;
			Statement.PrintAccountMovementToDate = ZDateTime.Empty;
			AssertEquals("PrintAccountMovementFromDate has errors", true, Statement.PrintAccountMovementFromDateInfo.HasErrors());
			AssertEquals("PrintAccountMovementToDate has has errors", true, Statement.PrintAccountMovementToDateInfo.HasErrors());

			Statement.PrintAccountMovementSOA = false;
			AssertEquals("PrintAccountMovementFromDate has errors", false, Statement.PrintAccountMovementFromDateInfo.HasErrors());
			AssertEquals("PrintAccountMovementToDate has has errors", false, Statement.PrintAccountMovementToDateInfo.HasErrors());
		}

		#endregion

		#region Test Properties

		public void TestOrganisation_List()
		{
			AssertEquals("Organisation list", typeof(OrgHeaderCollection), Statement.Organisation_List.GetType());
		}

		public void TestTransactionCurrency_List()
		{
			AssertEquals("Currency list", typeof(RefCurrencyCollection), Statement.TransactionCurrency_List.GetType());
		}

		public void TestTransactionBranch_List()
		{
			AssertEquals("Transaction Branch list", typeof(GlbBranchCollection), Statement.TransactionBranch_List.GetType());
		}

		public void TestDebtorGroup_List()
		{
			AssertEquals("AR Group list", typeof(OrgDebtorGroupCollection), Statement.DebtorGroup_List.GetType());
		}

		public void TestSalesRep_List()
		{
			AssertEquals("Staff list", typeof(GlbStaffCollection), Statement.SalesRep_List.GetType());
		}

		public void TestBranch_List()
		{
			AssertEquals("Branch list", typeof(GlbBranchCollection), Statement.Branch_List.GetType());
		}

		public void TestIssueStatementPack_List()
		{
			Assert(@"Issue Statement Pack list should contain ""Default"" value", Statement.IssueStatementPack_List.ContainsCode(AccountingConstants.IssueStatementPackType.Default));
			Assert(@"Issue Statement Pack list should contain ""Statement Only"" value", Statement.IssueStatementPack_List.ContainsCode(AccountingConstants.IssueStatementPackType.StatementOnly));
			Assert(@"Issue Statement Pack list should contain ""StatementAndInvoices"" value", Statement.IssueStatementPack_List.ContainsCode(AccountingConstants.IssueStatementPackType.StatementAndInvoices));
			AssertEquals("There should be 3 elements in collection", 3, Statement.IssueStatementPack_List.Count);
		}

		public void TestGroupByAccountMovementSOALine_List()
		{
			Assert(@"GroupByAccountMovementSOALine List should contain ""Default"" value", Statement.GroupByAccountMovementSOALine_List.ContainsCode(AccountMovementLineGroupingOptions.NO_GROUPING));
			Assert(@"GroupByAccountMovementSOALine List should contain ""Statement Only"" value", Statement.GroupByAccountMovementSOALine_List.ContainsCode(AccountMovementLineGroupingOptions.GROUP_BY_TRANSACTION_BRANCH));
			Assert(@"GroupByAccountMovementSOALine List contain ""StatementAndInvoices"" value", Statement.GroupByAccountMovementSOALine_List.ContainsCode(AccountMovementLineGroupingOptions.GROUP_BY_TRANSACTION_DEPARTMENT));
			Assert(@"GroupByAccountMovementSOALine List contain ""StatementAndInvoices"" value", Statement.GroupByAccountMovementSOALine_List.ContainsCode(AccountMovementLineGroupingOptions.GROUP_BY_TRANSACTION_BRANCH_DEPARTMENT));
			AssertEquals("There should be 4 elements in collection", 4, Statement.GroupByAccountMovementSOALine_List.Count);
		}

		public void TestStatementSpecificCountryCode()
		{
			AssertEquals("Specific Country Code", ExpectedSpecificCountryCode, Statement.SpecificCountryCode_ForTestOnly);
		}

		public void TestStatementIsClientSpecific()
		{
			AssertEquals("Is Client Specific", ExpectedIsClientSpecific, Statement.IsClientSpecific_ForTestOnly);
		}

		public void TestStatementTemplateName_LegacyInvoices()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderStatementDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertEquals("Template Name", ExpectedTemplateName, Statement.StatementMenuName_ForTestOnly);
		}

		public void TestStatementTemplateName_DocBuilderInvoices()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderStatementDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals("Template Name", ExpectedTemplateName_DocBuilderInvoices, Statement.StatementMenuName_ForTestOnly);
		}

		public void TestPrintAccountMovementSOA()
		{
			Statement.IncludeDebtorSummaryPage = true;
			Statement.IssueBySettlementGroup = true;
			Statement.IssueByTransactionBranch = true;
			Statement.IssueByTransactionDepartment = true;

			Statement.TransactionBranch_PK = GlbBranch.CurrentBranch.PK;
			Statement.TransactionDepartment_PK = GlbDepartment.CurrentDepartment.PK;
			Statement.CutOffDate = ZDateTime.Today;
			Statement.CutOffPeriod = 15;
			Statement.DisbursementInvoicesOnly = true;
			Statement.IncludeTransactionsInActiveBatch = true;
			Statement.PrintAccountMovementSOA = true;

			AssertEquals("IncludeDebtorSummaryPage", false, Statement.IncludeDebtorSummaryPage);
			AssertEquals("IssueBySettlementGroup", false, Statement.IssueBySettlementGroup);
			AssertEquals("IssueByTransactionBranch", false, Statement.IssueByTransactionBranch);
			AssertEquals("IssueByTransactionDepartment", false, Statement.IssueByTransactionDepartment);
			AssertEquals("TransactionBranch_PK", ZGuid.Empty, Statement.TransactionBranch_PK);
			AssertEquals("IssueBySettlementGroup", ZGuid.Empty, Statement.TransactionDepartment_PK);
			AssertEquals("CutOffDate", ZDateTime.Empty, Statement.CutOffDate);
			AssertEquals("CutOffPeriod", ZInt.Zero, Statement.CutOffPeriod);
			AssertEquals("DisbursementInvoicesOnly", false, Statement.DisbursementInvoicesOnly);
			AssertEquals("IncludeTransactionsInActiveBatch", true, Statement.IncludeTransactionsInActiveBatch);
		}

		protected virtual string ExpectedTemplateName
		{
			get { return "Statement Of Account"; }
		}

		protected virtual string ExpectedTemplateName_DocBuilderInvoices
		{
			get { return "Statement Of Account"; }
		}

		protected virtual ZBool ExpectedIsClientSpecific
		{
			get { return ZBool.False; }
		}

		protected virtual ZString ExpectedSpecificCountryCode
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Test SQL Generation

		public void TestAddStandardSelectClause_Statement()
		{
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;

			Statement.AddStandardSelectClause_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("Paramaters.Count", 5, SqlParameters.Count);

			ZSqlParameter ledger = ZSqlParameter.New("@Ledger", ZArchitecture.Core.LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
			Assert("Paramaters contains @Ledger", SqlParameters.Contains(ledger));

			ZSqlParameter company = ZSqlParameter.New("@Company", Statement.Company.PK, GlbCompanySchema.PK);
			Assert("Paramaters contains @Company", SqlParameters.Contains(company));

			SQLStringBuilder = new ZStringBuilder();
			SqlParameters = new ZSqlParameterCollection();

			int currentPeriod = Statement.PeriodCalculator_ForTestOnly.GetPeriodFromDate(ZDateTime.Today);

			Statement.CutOffPeriod = Statement.PeriodCalculator_ForTestOnly.GetPreviousPeriod(currentPeriod);
			Statement.AddStandardSelectClause_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("Paramaters.Count", 6, SqlParameters.Count);

			ZSqlParameter endOfPeriod = ZSqlParameter.New("@MatchDateOnOrBefore", Statement.EndOfPeriod_ForTestOnly.Date.AddDays(1), AccTransactionMatchLinkSchema.AP_MatchDate);
			Assert("Paramaters contains @EndOfPeriod", SqlParameters.Contains(endOfPeriod));
		}

		public void TestAddStandardSelectClause_ForAccountMovementSOA()
		{
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
			Statement.PrintAccountMovementSOA = true;

			SQLStringBuilder = new ZStringBuilder();
			Statement.AddStandardSelectClause_ForTestOnly(SQLStringBuilder, SqlParameters);
			var qry = SQLStringBuilder.ToString();

			AssertContains("SELECT: TransactionOrg.OH_PK", "TransactionOrg.OH_PK", qry);
			AssertContains("SELECT: StmntCurrencyTable.AH_RX_NKTransactionCurrency", "StmntCurrencyTable.AH_RX_NKTransactionCurrency", qry);
			AssertContains("SELECT: isMultipleCurrency IF part", "WHEN StmntCurrencyTable.AH_RX_NKTransactionCurrency = GC_RX_NKLocalCurrency THEN 1", qry);
			AssertContains("SELECT: isMultipleCurrency ELSE part", "ELSE 0", qry);
			AssertContains("SELECT: isMultipleCurrency alias", "AS isMultipleCurrency", qry);
			AssertContains("SELECT: Opening Balance", "StmntCurrencyTable.OpeningBalance AS OpeningBalance", qry);
			AssertContains("SELECT: OrgCode", "TransactionOrg.OH_Code", qry);
			AssertContains("FROM: fnGetAROutstandingBalance", "INNER JOIN (SELECT * FROM fnGetAROutstandingBalance(@PostDateFrom, @PostDateTo, @Company, NULL)) StmntCurrencyTable ON dbo.AccTransactionHeader.AH_OH = StmntCurrencyTable.AH_OH", qry);
			AssertContains("WHERE: PostDate Range", "AccTransactionHeader.AH_PostDate between @PostDateFrom and @PostDateTo", qry);
			AssertEquals("Paramaters.Count", 7, SqlParameters.Count);

			Statement.PrintAccountMovementSOA = false;
			SQLStringBuilder = new ZStringBuilder();
			SqlParameters = new ZSqlParameterCollection();
			Statement.AddStandardSelectClause_ForTestOnly(SQLStringBuilder, SqlParameters);
			qry = SQLStringBuilder.ToString();

			AssertContains("SELECT: TransactionOrg.OH_PK", "TransactionOrg.OH_PK", qry);
			AssertNotContains("SELECT: StmntCurrencyTable.AH_RX_NKTransactionCurrency", "StmntCurrencyTable.AH_RX_NKTransactionCurrency", qry);
			AssertNotContains("SELECT: isMultipleCurrency IF part", "WHEN StmntCurrencyTable.AH_RX_NKTransactionCurrency = GC_RX_NKLocalCurrency THEN 1", qry);
			AssertNotContains("SELECT: isMultipleCurrency ELSE part", "ELSE 0", qry);
			AssertNotContains("SELECT: isMultipleCurrency alias", "AS isMultipleCurrency", qry);
			AssertNotContains("SELECT: Opening Balance", "StmntCurrencyTable.OpeningBalance AS OpeningBalance", qry);
			AssertContains("SELECT: OrgCode", "TransactionOrg.OH_Code", qry);
			AssertNotContains("FROM: fnGetAROutstandingBalance", "INNER JOIN (SELECT * FROM fnGetAROutstandingBalance(@PostDateFrom, @PostDateTo, @Company, NULL)) StmntCurrencyTable ON AccTransactionHeader.AH_OH = StmntCurrencyTable.AH_OH", qry);
			AssertNotContains("WHERE: PostDate Range", "AccTransactionHeader.AH_PostDate between @PostDateFrom and @PostDateTo", qry);
			AssertEquals("Paramaters.Count", 5, SqlParameters.Count);
		}

		public void TestNoReferenceToOrgRelatedPartyWhenNotIssuingBySettlementGroup()
		{
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;

			Statement.AddStandardSelectClause_ForTestOnly(SQLStringBuilder, SqlParameters);
			Assert(!SQLStringBuilder.ToString().Contains(OrgRelatedPartySchema.Constants.TableName));

			Statement.IssueBySettlementGroup = true;
			Statement.AddStandardSelectClause_ForTestOnly(SQLStringBuilder, SqlParameters);
			Assert(SQLStringBuilder.ToString().Contains(OrgRelatedPartySchema.Constants.TableName));
		}

		public void TestAddStandardSelectClause_IssueBySettlementGroup()
		{
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
			Statement.IssueBySettlementGroup = true;

			Statement.AddStandardSelectClause_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("Paramaters.Count", 5, SqlParameters.Count);

			Statement.GetBusinessObjCollection_ForTestOnly();

			ZSqlParameter ledger = ZSqlParameter.New("@Ledger", ZArchitecture.Core.LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
			Assert("Paramaters contains @Ledger", SqlParameters.Contains(ledger));

			ZSqlParameter company = ZSqlParameter.New("@Company", Statement.Company.PK, GlbCompanySchema.PK);
			Assert("Paramaters contains @Company", SqlParameters.Contains(company));

			SQLStringBuilder = new ZStringBuilder();
			SqlParameters = new ZSqlParameterCollection();

			int currentPeriod = Statement.PeriodCalculator_ForTestOnly.GetPeriodFromDate(ZDateTime.Today);

			Statement.CutOffPeriod = Statement.PeriodCalculator_ForTestOnly.GetPreviousPeriod(currentPeriod);
			Statement.AddStandardSelectClause_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("Paramaters.Count", 6, SqlParameters.Count);

			ZSqlParameter endOfPeriod = ZSqlParameter.New("@MatchDateOnOrBefore", Statement.EndOfPeriod_ForTestOnly.Date.AddDays(1), AccTransactionMatchLinkSchema.AP_MatchDate);
			Assert("Paramaters contains @EndOfPeriod", SqlParameters.Contains(endOfPeriod));
		}

		public void TestAddStandardSelectClause_CollectionLetter()
		{
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.FirstReminder;

			Statement.AddStandardSelectClause_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("Paramaters.Count", 5, SqlParameters.Count);

			ZSqlParameter ledger = ZSqlParameter.New("@Ledger", ZArchitecture.Core.LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
			Assert("Paramaters contains @Ledger", SqlParameters.Contains(ledger));

			ZSqlParameter company = ZSqlParameter.New("@Company", Statement.Company.PK, GlbCompanySchema.PK);
			Assert("Paramaters contains @Company", SqlParameters.Contains(company));

			SQLStringBuilder = new ZStringBuilder();
			SqlParameters = new ZSqlParameterCollection();

			int currentPeriod = Statement.PeriodCalculator_ForTestOnly.GetPeriodFromDate(ZDateTime.Today);

			Statement.CutOffPeriod = Statement.PeriodCalculator_ForTestOnly.GetPreviousPeriod(currentPeriod);
			Statement.AddStandardSelectClause_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("Paramaters.Count", 6, SqlParameters.Count);

			ZSqlParameter endOfPeriod = ZSqlParameter.New("@MatchDateOnOrBefore", Statement.EndOfPeriod_ForTestOnly.Date.AddDays(1), AccTransactionMatchLinkSchema.AP_MatchDate);
			Assert("Paramaters contains @EndOfPeriod", SqlParameters.Contains(endOfPeriod));
		}

		public void TestAddFilterForOneOrganisation_EmptyOH_PK()
		{
			Statement.OH_PK = ZGuid.Empty;
			Statement.AddFilterForOneOrganisation_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForOneOrganisation_ValidOH_PK()
		{
			ZGuid organisationPK = ZGuid.NewZGuid();

			Statement.OH_PK = organisationPK;
			Statement.AddFilterForOneOrganisation_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedText = "AND " + Statement.OrganisationHeader_ForTestOnly + "." + OrgHeaderSchema.Constants.PK + " = @Organisation ";
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			ZSqlParameter organisation = ZSqlParameter.New("@Organisation", Statement.OH_PK, OrgHeaderSchema.PK);
			Assert("Paramaters contains @Organisation", SqlParameters.Contains(organisation));
		}

		public void TestAddFilterForBatchOfOrganisations()
		{
			ZGuid organisationPK = ZGuid.NewZGuid();
			Statement.BatchOfOrganisationsToPrint.Add(organisationPK);
			ZGuid organisationPK2 = ZGuid.NewZGuid();
			Statement.BatchOfOrganisationsToPrint.Add(organisationPK2);
			Statement.AddFilterForBatchOfOrganisations_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedText = "AND " + Statement.OrganisationHeader_ForTestOnly + "." + OrgHeaderSchema.Constants.PK + " IN (@Organisation0,@Organisation1) ";
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			ZSqlParameter organisation = ZSqlParameter.New("@Organisation0", Statement.BatchOfOrganisationsToPrint[0], OrgHeaderSchema.PK);
			Assert("Paramaters contains @Organisation0", SqlParameters.Contains(organisation));

			organisation = ZSqlParameter.New("@Organisation1", Statement.BatchOfOrganisationsToPrint[1], OrgHeaderSchema.PK);
			Assert("Paramaters contains @Organisation1", SqlParameters.Contains(organisation));
		}

		public void TestAddFilterForOneDebtorGroup_EmptyOJ_PK()
		{
			Statement.OJ_PK = ZGuid.Empty;
			Statement.AddFilterForOneDebtorGroup_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForOneDebtorGroup_ValidOJ_PK()
		{
			ZGuid debtorGroup = ZGuid.NewZGuid();
			Statement.OJ_PK = debtorGroup;
			Statement.AddFilterForOneDebtorGroup_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedText = "AND " + Statement.OrganisationCompanyData_ForTestOnly + "." + OrgCompanyDataSchema.Constants.OB_OJ_ARDebtorGroup + " = @ARGroup ";
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			ZSqlParameter aRGroup = ZSqlParameter.New("@ARGroup", Statement.OJ_PK, OrgCompanyDataSchema.OB_OJ_ARDebtorGroup);
			Assert("Paramaters contains @ARGroup", SqlParameters.Contains(aRGroup));
		}

		public void TestAddFilterForOrganisationBranch_EmptyGB_PK()
		{
			Statement.GB_PK = ZGuid.Empty;
			Statement.AddFilterForOrganisationBranch_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForOrganisationBranch_ValidGB_PK()
		{
			ZGuid branchPK = ZGuid.NewZGuid();
			Statement.GB_PK = branchPK;
			Statement.AddFilterForOrganisationBranch_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedText = "AND " + Statement.OrganisationCompanyData_ForTestOnly + "." + OrgCompanyDataSchema.Constants.OB_GB_ControllingBranch + " = @Branch ";
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			ZSqlParameter branch = ZSqlParameter.New("@Branch", Statement.GB_PK, GlbBranchSchema.PK);
			Assert("Paramaters contains @Branch", SqlParameters.Contains(branch));
		}

		public void TestAddFilterForAccountsRelationship_Empty()
		{
			Statement.AccountsRelationShip = ZString.Empty;
			Statement.AddFilterForAccountsRelationship_ForTestOnly(SQLStringBuilder, SqlParameters);

			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForAccountsRelationship_Valid()
		{
			Statement.AccountsRelationShip = Statement.AccountsRelationShip_List[0].Code;
			Statement.AddFilterForAccountsRelationship_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedText = "AND " + Statement.OrganisationCompanyData_ForTestOnly + "." + OrgCompanyDataSchema.Constants.OB_ARCategory + " = @AccountsRelationship ";
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			ZSqlParameter accountsRelationship = ZSqlParameter.New("@AccountsRelationship", Statement.AccountsRelationShip, OrgCompanyDataSchema.OB_ARCategory);
			Assert("Paramaters contains @AccountsRelationship", SqlParameters.Contains(accountsRelationship));
		}

		public void TestAddFilterForConsolidationCategory_Empty()
		{
			Statement.ConsolidationCategory = ZString.Empty;
			Statement.AddFilterForConsolidationCategory_ForTestOnly(SQLStringBuilder, SqlParameters);

			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForConsolidationCategory_Valid()
		{
			Statement.ConsolidationCategory = Statement.ConsolidationCategory_List[0].Code;
			Statement.AddFilterForConsolidationCategory_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedText = "AND " + Statement.OrganisationCompanyData_ForTestOnly + "." + OrgCompanyDataSchema.Constants.OB_ARConsolidatedAccountingCategory + " = @ConsolidationCategory ";
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			ZSqlParameter consolidationCategory = ZSqlParameter.New("@ConsolidationCategory", Statement.ConsolidationCategory, OrgCompanyDataSchema.OB_ARConsolidatedAccountingCategory);
			Assert("Paramaters contains @ConsolidationCategory", SqlParameters.Contains(consolidationCategory));
		}

		public void TestAddFilterForCreditRating_Empty()
		{
			Statement.CreditRating = ZString.Empty;
			Statement.AddFilterForCreditRating_ForTestOnly(SQLStringBuilder, SqlParameters);

			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForCreditRating_Valid()
		{
			Statement.CreditRating = Statement.CreditRating_List[0].Code;
			Statement.AddFilterForCreditRating_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedText = "AND " + Statement.OrganisationCompanyData_ForTestOnly + "." + OrgCompanyDataSchema.Constants.OB_ARCreditRating + " = @CreditRating ";
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			ZSqlParameter creditRating = ZSqlParameter.New("@CreditRating", Statement.CreditRating, OrgCompanyDataSchema.OB_ARCreditRating);
			Assert("Paramaters contains @CreditRating", SqlParameters.Contains(creditRating));
		}

		public void TestOrganisationCompanyDataActuallyUsed()
		{
			ZStringBuilder builder = new ZStringBuilder();
			Statement.IssueBySettlementGroup = false;
			Statement.AddStandardSelectClause_ForTestOnly(builder, new ZSqlParameterCollection());
			AssertContains("Query should use TransactionCompanyData as orgcompanydata alias", Constants.OrgCompanyData + " " + Statement.Constants.TransactionCompanyData, builder.ToString());

			builder = new ZStringBuilder();
			Statement.IssueBySettlementGroup = true;
			Statement.AddStandardSelectClause_ForTestOnly(builder, new ZSqlParameterCollection());
			AssertContains("Query should use TransactionCompanyData as orgcompanydata alias", Constants.OrgCompanyData + " " + Statement.Constants.SettlementCompanyData, builder.ToString());
		}

		public void TestAddFilterForSalesRep_Empty()
		{
			Statement.SalesRep_PK = ZGuid.Empty;
			Statement.AddFilterForSalesRep_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForSalesRep_Valid()
		{
			Statement.SalesRep_PK = GlbStaff.CurrentUser.PK;
			Statement.AddFilterForSalesRep_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedText = "AND " + Statement.OrganisationHeader_ForTestOnly + "." + OrgHeaderSchema.Constants.PK + " IN (";
			expectedText += "SELECT " + OrgStaffAssignmentsSchema.Constants.O8_OH + " ";
			expectedText += "FROM dbo." + OrgStaffAssignmentsSchema.Constants.TableName + " ";
			expectedText += "WHERE " + OrgStaffAssignmentsSchema.Constants.O8_GS_NKPersonResponsible + " = @SalesRep ";
			expectedText += "AND " + OrgStaffAssignmentsSchema.Constants.O8_Role + " = @SalesRepRole ";
			expectedText += "AND " + OrgStaffAssignmentsSchema.Constants.O8_Department + " = @SalesRepDepartment ";
			expectedText += "AND " + OrgStaffAssignmentsSchema.Constants.O8_GC + " = @SalesRepCompany " + ")";

			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			ZSqlParameter salesRep = ZSqlParameter.New("@SalesRep", Statement.SalesRep_Code_ForTestOnly, OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible);
			ZSqlParameter salesRepRole = ZSqlParameter.New("@SalesRepRole", StaffAssignmentRoles.Codes.SalesRep, OrgStaffAssignmentsSchema.O8_Role);
			ZSqlParameter salesRepDepartment = ZSqlParameter.New("@SalesRepDepartment", "ALL", OrgStaffAssignmentsSchema.O8_Department);
			ZSqlParameter salesRepCompany = ZSqlParameter.New("@SalesRepCompany", Statement.Company.PK, OrgStaffAssignmentsSchema.O8_GC);

			AssertEquals("Paramaters Count", 4, SqlParameters.Count);
			Assert("Paramaters contains @SalesRep", SqlParameters.Contains(salesRep));
			Assert("Paramaters contains @SalesRepRole", SqlParameters.Contains(salesRepRole));
			Assert("Paramaters contains @SalesRepDepartment", SqlParameters.Contains(salesRepDepartment));
			Assert("Paramaters contains @SalesRepCompany", SqlParameters.Contains(salesRepCompany));
		}

		public void TestAddFilterForCustomerServiceRep_Empty()
		{
			Statement.CustomerServiceRep_PK = ZGuid.Empty;
			Statement.AddFilterForCustomerServiceRep_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForCustomerServiceRep_Valid()
		{
			Statement.CustomerServiceRep_PK = GlbStaff.CurrentUser.PK;
			Statement.AddFilterForCustomerServiceRep_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedText = "AND " + Statement.OrganisationHeader_ForTestOnly + "." + OrgHeaderSchema.Constants.PK + " IN (";
			expectedText += "SELECT " + OrgStaffAssignmentsSchema.Constants.O8_OH + " ";
			expectedText += "FROM dbo." + OrgStaffAssignmentsSchema.Constants.TableName + " ";
			expectedText += "WHERE " + OrgStaffAssignmentsSchema.Constants.O8_GS_NKPersonResponsible + " = @CustomerServiceRep ";
			expectedText += "AND " + OrgStaffAssignmentsSchema.Constants.O8_Role + " = @CustomerServiceRepRole ";
			expectedText += "AND " + OrgStaffAssignmentsSchema.Constants.O8_Department + " = @CustomerServiceRepDepartment ";
			expectedText += "AND " + OrgStaffAssignmentsSchema.Constants.O8_GC + " = @CustomerServiceRepCompany " + ")";

			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			ZSqlParameter customerServiceRep = ZSqlParameter.New("@CustomerServiceRep", Statement.CustomerServiceRep_Code_ForTestOnly, OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible);
			ZSqlParameter customerServiceRepRole = ZSqlParameter.New("@CustomerServiceRepRole", StaffAssignmentRoles.Codes.CustomerServiceRep, OrgStaffAssignmentsSchema.O8_Role);
			ZSqlParameter customerServiceRepDepartment = ZSqlParameter.New("@CustomerServiceRepDepartment", "ALL", OrgStaffAssignmentsSchema.O8_Department);
			ZSqlParameter customerServiceRepCompany = ZSqlParameter.New("@CustomerServiceRepCompany", Statement.Company.PK, OrgStaffAssignmentsSchema.O8_GC);

			AssertEquals("Paramaters Count", 4, SqlParameters.Count);
			Assert("Paramaters contains @CustomerServiceRep", SqlParameters.Contains(customerServiceRep));
			Assert("Paramaters contains @CustomerServiceRepRole", SqlParameters.Contains(customerServiceRepRole));
			Assert("Paramaters contains @CustomerServiceRepDepartment", SqlParameters.Contains(customerServiceRepDepartment));
			Assert("Paramaters contains @CustomerServiceRepCompany", SqlParameters.Contains(customerServiceRepCompany));
		}

		public void TestAddFilterForCreditController_Empty()
		{
			Statement.CreditController_PK = ZGuid.Empty;
			Statement.AddFilterForCreditController_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForCreditController_Valid()
		{
			Statement.CreditController_PK = GlbStaff.CurrentUser.PK;
			Statement.AddFilterForCreditController_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedText = "AND " + Statement.OrganisationHeader_ForTestOnly + "." + OrgHeaderSchema.Constants.PK + " IN (";
			expectedText += "SELECT " + OrgStaffAssignmentsSchema.Constants.O8_OH + " ";
			expectedText += "FROM dbo." + OrgStaffAssignmentsSchema.Constants.TableName + " ";
			expectedText += "WHERE " + OrgStaffAssignmentsSchema.Constants.O8_GS_NKPersonResponsible + " = @CreditController ";
			expectedText += "AND " + OrgStaffAssignmentsSchema.Constants.O8_Role + " = @CreditControllerRole ";
			expectedText += "AND " + OrgStaffAssignmentsSchema.Constants.O8_Department + " = @CreditControllerDepartment ";
			expectedText += "AND " + OrgStaffAssignmentsSchema.Constants.O8_GC + " = @CreditControllerCompany " + ")";

			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			ZSqlParameter creditController = ZSqlParameter.New("@CreditController", Statement.CreditController_Code_ForTestOnly, OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible);
			ZSqlParameter creditControllerRole = ZSqlParameter.New("@CreditControllerRole", StaffAssignmentRoles.Codes.CreditController, OrgStaffAssignmentsSchema.O8_Role);
			ZSqlParameter creditControllerDepartment = ZSqlParameter.New("@CreditControllerDepartment", "ALL", OrgStaffAssignmentsSchema.O8_Department);
			ZSqlParameter creditControllerCompany = ZSqlParameter.New("@CreditControllerCompany", Statement.Company.PK, OrgStaffAssignmentsSchema.O8_GC);

			AssertEquals("Paramaters Count", 4, SqlParameters.Count);
			Assert("Paramaters contains @CreditController", SqlParameters.Contains(creditController));
			Assert("Paramaters contains @CreditControllerRole", SqlParameters.Contains(creditControllerRole));
			Assert("Paramaters contains @CreditControllerDepartment", SqlParameters.Contains(creditControllerDepartment));
			Assert("Paramaters contains @CreditControllerCompany", SqlParameters.Contains(creditControllerCompany));
		}

		public void TestAddFilterForCurrency_Empty()
		{
			Statement.RX_PK = ZGuid.Empty;
			Statement.AddFilterForCurrency_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForCurrency_Valid()
		{
			ZGuid currencyPK = ZGuid.NewZGuid();
			Statement.RX_PK = currencyPK;
			Statement.AddFilterForCurrency_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedText = "AND dbo." + AccTransactionHeaderSchema.Constants.TableName + "." + AccTransactionHeaderSchema.Constants.AH_RX_NKTransactionCurrency + " = @Currency ";
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			RefCurrency currency = Factory.Load<RefCurrency>(Statement.RX_PK);
			ZSqlParameter currency1 = ZSqlParameter.New("@Currency", currency != null ? currency.RX_Code : ZString.Empty, AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency);
			Assert("Paramaters contains @Currency", SqlParameters.Contains(currency1));
		}

		public void TestAddFilterForTransactionBranch_Empty()
		{
			Statement.TransactionBranch_PK = ZGuid.Empty;
			Statement.AddFilterForTransactionBranch_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForTransactionDepartment_Empty()
		{
			Statement.TransactionDepartment_PK = ZGuid.Empty;
			Statement.AddFilterForTransactionDepartment_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForTransactionBranch_Valid()
		{
			ZGuid transactionBranchPK = ZGuid.NewZGuid();
			Statement.TransactionBranch_PK = transactionBranchPK;
			Statement.IssueByTransactionBranch = true;
			Statement.AddFilterForTransactionBranch_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedText = "AND dbo." + AccTransactionHeaderSchema.Constants.TableName + "." + AccTransactionHeaderSchema.Constants.AH_GB + " = @TransactionBranch ";
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			ZSqlParameter transactionBranch = ZSqlParameter.New("@TransactionBranch", Statement.TransactionBranch_PK, AccTransactionHeaderSchema.AH_GB);
			Assert("Paramaters contains @TransactionBranch", SqlParameters.Contains(transactionBranch));
		}

		public void TestAddFilterForTransactionDepartment_Valid()
		{
			ZGuid transactionDepartmentPK = ZGuid.NewZGuid();
			Statement.TransactionDepartment_PK = transactionDepartmentPK;
			Statement.IssueByTransactionDepartment = true;
			Statement.AddFilterForTransactionDepartment_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedText = "AND dbo." + AccTransactionHeaderSchema.Constants.TableName + "." + AccTransactionHeaderSchema.Constants.AH_GE + " = @TransactionDepartment ";
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			ZSqlParameter transactionDepartment = ZSqlParameter.New("@TransactionDepartment", Statement.TransactionDepartment_PK, AccTransactionHeaderSchema.AH_GE);
			Assert("Paramaters contains @TransactionDepartment", SqlParameters.Contains(transactionDepartment));
		}

		public void TestAddFilterForInvoiceDateOnOrBefore_EmptyInvoiceDate()
		{
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
			Statement.CutOffDate = ZDateTime.Empty;

			Statement.AddFilterForInvoiceDateOnOrBefore_ForTestOnly(SQLStringBuilder, SqlParameters);

			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForInvoiceDateOnOrBefore_ValidInvoiceDate()
		{
			ZDateTime invoicesOnOrBefore = ZDateTime.Now.AddDays(-5);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
			Statement.CutOffDate = invoicesOnOrBefore;

			Statement.AddFilterForInvoiceDateOnOrBefore_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedString = "AND " + AccTransactionHeaderSchema.Constants.AH_InvoiceDate + " < @CutOffDate ";
			ZSqlParameter invoiceDate = ZSqlParameter.New("@CutOffDate", invoicesOnOrBefore.AddDays(1).Date, AccTransactionHeaderSchema.AH_InvoiceDate);

			AssertEquals("StringBuilder Contents", expectedString, SQLStringBuilder.ToString());
			Assert("Paramaters contains CutoffDate", SqlParameters.Contains(invoiceDate));
		}

		public void TestAddFilterForPostDateOnOrBefore_ValidPostDate()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.PostPeriodsForEntireYear(ZDateTime.Now.Year);
			Factory.Save();

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
			Statement.CutOffPeriod = new AccountingPeriodCalculator(Factory).GetPeriodFromDate(ZDateTime.Now);

			Statement.AddFilterForPostDateOnOrBefore_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedString = "AND " + AccTransactionHeaderSchema.Constants.AH_PostDate + " < @PostDateOnOrBefore ";
			ZSqlParameter endOfPeriod = ZSqlParameter.New("@PostDateOnOrBefore", Statement.EndOfPeriod_ForTestOnly.AddDays(1).Date, AccTransactionHeaderSchema.AH_PostDate);

			AssertEquals("StringBuilder Contents", expectedString, SQLStringBuilder.ToString());
			Assert("Paramaters contains EndOfPeriod", SqlParameters.Contains(endOfPeriod));
		}

		public void TestAddFilterForDueDateOnOrBefore_EmptyDueDate()
		{
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.FirstReminder;
			Statement.CutOffDate = ZDateTime.Empty;

			Statement.AddFilterForDueDatesOnOrBefore_ForTestOnly(SQLStringBuilder, SqlParameters);

			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForDueDateOnOrBefore_ValidDueDate()
		{
			ZDateTime invoicesOnOrBefore = ZDateTime.Now.AddDays(-5);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
			Statement.CutOffDate = invoicesOnOrBefore;

			Statement.AddFilterForInvoiceDateOnOrBefore_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedString = "AND " + AccTransactionHeaderSchema.Constants.AH_InvoiceDate + " < @CutOffDate ";
			ZSqlParameter invoiceDate = ZSqlParameter.New("@CutOffDate", invoicesOnOrBefore.AddDays(1).Date, AccTransactionHeaderSchema.AH_InvoiceDate);

			AssertEquals("StringBuilder Contents", expectedString, SQLStringBuilder.ToString());
			Assert("Paramaters contains CutoffDate", SqlParameters.Contains(invoiceDate));
		}

		public void TestAddFilterForOutStandingAmount_AmountIsZero()
		{
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.FirstReminder;
			Statement.OutstandingAmountGreaterThan = 0M;

			Statement.AddFilterForOutStandingAmount_ForTestOnly(SQLStringBuilder, SqlParameters);

			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForOutStandingAmount_AmountIsMoreThanZero()
		{
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.FirstReminder;
			Statement.OutstandingAmountGreaterThan = 50M;

			Statement.AddFilterForOutStandingAmount_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedString = "AND ABS(" + Constants.AH_OutstandingAmount + ") >= @OutstandingAmount ";
			ZSqlParameter outstandingAmount = ZSqlParameter.New("@OutstandingAmount", Statement.OutstandingAmountGreaterThan, AccTransactionHeaderSchema.AH_OutstandingAmount);

			AssertEquals("StringBuilder Contents", expectedString, SQLStringBuilder.ToString());
			Assert("Paramaters contains @OutstandingAmount", SqlParameters.Contains(outstandingAmount));

			SQLStringBuilder = new ZStringBuilder();
			SqlParameters = new ZSqlParameterCollection();

			int currentPeriod = Statement.PeriodCalculator_ForTestOnly.GetPeriodFromDate(ZDateTime.Today);

			Statement.CutOffPeriod = Statement.PeriodCalculator_ForTestOnly.GetPreviousPeriod(currentPeriod);
			Statement.AddFilterForOutStandingAmount_ForTestOnly(SQLStringBuilder, SqlParameters);

			expectedString = "AND ABS(" + Constants.AH_LocalTotal + " - ISNULL(SumOfMatches, 0) ) >= @OutstandingAmount ";
			AssertEquals("StringBuilder Contents", expectedString, SQLStringBuilder.ToString());
			Assert("Paramaters contains @OutstandingAmount", SqlParameters.Contains(outstandingAmount));
		}

		public void TestAddFilterForDisbursementInvoicesOnly_False()
		{
			Statement.AddFilterForDisbursementInvoicesOnly_ForTestOnly(SQLStringBuilder, SqlParameters);

			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
			AssertEquals("Paramaters count", 0, SqlParameters.Count);
		}

		public void TestAddFilterForDisbursementInvoicesOnly_True()
		{
			Statement.DisbursementInvoicesOnly = true;
			Statement.AddFilterForDisbursementInvoicesOnly_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedString = "AND " + AccTransactionHeaderSchema.Constants.AH_TransactionCategory + " in (@Disbursement1, @Disbursement2, @Disbursement3, @Disbursement4) ";
			string[] disbursementTypes = InvoiceTypeCalculationProvider.DisbursementInvoiceTypes;
			ZSqlParameter disbursement1 = ZSqlParameter.New("@Disbursement1", disbursementTypes[0],
				AccTransactionHeaderSchema.AH_TransactionCategory);
			ZSqlParameter disbursement2 = ZSqlParameter.New("@Disbursement2", disbursementTypes[1],
				AccTransactionHeaderSchema.AH_TransactionCategory);
			ZSqlParameter disbursement3 = ZSqlParameter.New("@Disbursement3", disbursementTypes[2],
				AccTransactionHeaderSchema.AH_TransactionCategory);
			ZSqlParameter disbursement4 = ZSqlParameter.New("@Disbursement4", disbursementTypes[3],
				AccTransactionHeaderSchema.AH_TransactionCategory);

			AssertEquals("StringBuilder Contents", expectedString, SQLStringBuilder.ToString());
			Assert("Paramaters contain @Disbursement1", SqlParameters.Contains(disbursement1));
			Assert("Paramaters contain @Disbursement2", SqlParameters.Contains(disbursement2));
			Assert("Paramaters contain @Disbursement3", SqlParameters.Contains(disbursement3));
			Assert("Paramaters contain @Disbursement4", SqlParameters.Contains(disbursement4));
		}

		public void TestGetNotIncludedBatchTypeCodes()
		{
			var registry = AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.Value;

			registry.RemoveAll();
			registry.Add("STD", (NoResString)"Standard Batch", true);
			registry.Add("ST1", (NoResString)"ST1 Desc", false);
			registry.Add("ST2", (NoResString)"ST2 Desc", true);
			registry.Add("ST3", (NoResString)"ST3 Desc", false);
			registry.Add("ST4", (NoResString)"ST4 Desc", false);
			AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);

			AssertCollectionContains("ST1", Statement.GetNotIncludedBatchTypeCodes_ForTestOnly());
			AssertCollectionContains("ST3", Statement.GetNotIncludedBatchTypeCodes_ForTestOnly());
			AssertCollectionContains("ST4", Statement.GetNotIncludedBatchTypeCodes_ForTestOnly());
		}

		public void TestIncludeTransactionsInActiveBatch_False()
		{
			var testCollection = new DynamicBusinessObjectCollection(Factory);
			Statement.IncludeTransactionsInActiveBatch = false;

			var invoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M) as ARInvoice;
			var invoice1 = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M) as ARInvoice;

			InvoiceBatchHeader header = Factory.New<InvoiceBatchHeader>();
			header.AH_OH = Creator.ZECTRA.PK;
			header.AH_InvoiceAmount = 100m;
			header.AH_OutstandingAmount = 100m;
			header.AH_OSTotal = 100M;
			header.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			Factory.Save();

			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals(2, testCollection.Count);

			var batch = Creator.CreateCollectionBatch(Creator.AUDBankAccount, GlbCompany.CurrentCompany, ZString.Empty, 100m, false);
			var order = Creator.CreateCollectionOrder(batch, ZDateTime.Today.Date, Creator.ABIGAS, ZString.Empty, 100m, false);
			var line = Creator.CreateCollectionOrderLine(order, invoice, false);

			Factory.Save();

			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals(1, testCollection.Count);
			AssertEquals(Creator.AALSHI.PK, testCollection.First()["OH_PK"]);
		}

		public void TestIncludeTransactionsInActiveBatch_True()
		{
			var testCollection = new DynamicBusinessObjectCollection(Factory);
			Statement.IncludeTransactionsInActiveBatch = true;

			var invoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M) as ARInvoice;
			var invoice1 = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M) as ARInvoice;

			InvoiceBatchHeader header = Factory.New<InvoiceBatchHeader>();
			header.AH_OH = Creator.ZECTRA.PK;
			header.AH_InvoiceAmount = 100m;
			header.AH_OutstandingAmount = 100m;
			header.AH_OSTotal = 100M;
			header.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			Factory.Save();

			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals(2, testCollection.Count);

			var registry = AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.Value;

			registry.RemoveAll();
			AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);

			var batch = Creator.CreateCollectionBatch(Creator.AUDBankAccount, GlbCompany.CurrentCompany, ZString.Empty, 100m, false);
			batch.ACB_Type = "ST1";
			var order = Creator.CreateCollectionOrder(batch, ZDateTime.Today.Date, Creator.ABIGAS, ZString.Empty, 100m, false);
			var line = Creator.CreateCollectionOrderLine(order, invoice, false);

			var batch1 = Creator.CreateCollectionBatch(Creator.AUDBankAccount, GlbCompany.CurrentCompany, ZString.Empty, 100m, false);
			batch1.ACB_Type = "STD";
			var order1 = Creator.CreateCollectionOrder(batch1, ZDateTime.Today.Date, Creator.AALSHI, ZString.Empty, 100m, false);
			var line1 = Creator.CreateCollectionOrderLine(order1, invoice1, false);

			Factory.Save();

			Statement.BatchTypeParam_ForTestOnly = null;
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals(2, testCollection.Count);

			registry.Add("STD", (NoResString)"Standard Batch", true);
			AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);
			Factory.Save();

			Statement.BatchTypeParam_ForTestOnly = null;
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals(2, testCollection.Count);

			registry.RemoveAll();
			registry.Add("STD", (NoResString)"Standard Batch", false);
			AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);
			Factory.Save();

			Statement.BatchTypeParam_ForTestOnly = null;
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals(1, testCollection.Count);

			registry.RemoveAll();
			registry.Add("STD", (NoResString)"Standard Batch", true);
			registry.Add("ST1", (NoResString)"ST1 Desc", false);
			AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);
			Factory.Save();

			Statement.BatchTypeParam_ForTestOnly = null;
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals(1, testCollection.Count);
			AssertEquals(Creator.AALSHI.PK, testCollection.First()["OH_PK"]);

			registry.Add("ST2", (NoResString)"ST2 Desc", true);
			AccountingMasterFilesRegistry.Instance.CollectionBatchTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);
			Factory.Save();

			Statement.BatchTypeParam_ForTestOnly = null;
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals(1, testCollection.Count);
		}

		public void TestAddGroupByStatement()
		{
			ZString expectedText = "GROUP BY " + Statement.OrganisationHeader_ForTestOnly + "." + OrgHeaderSchema.Constants.PK + ", ";
			expectedText += AccTransactionHeaderSchema.Constants.AH_RX_NKTransactionCurrency + ", ";
			expectedText += Statement.OrganisationHeader_ForTestOnly + "." + OrgHeaderSchema.Constants.OH_Code + " ";

			Statement.IssueByTransactionBranch = false;
			Statement.TransactionBranch_PK = ZGuid.Empty;
			Statement.AddGroupByStatement_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			SQLStringBuilder = new ZStringBuilder();
			Statement.IssueByTransactionBranch = false;
			Statement.TransactionBranch_PK = ZGuid.NewZGuid();
			Statement.AddGroupByStatement_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			expectedText = "GROUP BY " + Statement.OrganisationHeader_ForTestOnly + "." + OrgHeaderSchema.Constants.PK + ", ";
			expectedText += AccTransactionHeaderSchema.Constants.AH_RX_NKTransactionCurrency + ", ";
			expectedText += Constants.AccTransactionHeader + "." + Constants.AH_GB + ", ";
			expectedText += Statement.OrganisationHeader_ForTestOnly + "." + OrgHeaderSchema.Constants.OH_Code + " ";

			SQLStringBuilder = new ZStringBuilder();
			Statement.IssueByTransactionBranch = true;
			Statement.TransactionBranch_PK = ZGuid.NewZGuid();
			Statement.AddGroupByStatement_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			SQLStringBuilder = new ZStringBuilder();
			Statement.IssueByTransactionBranch = true;
			Statement.TransactionBranch_PK = ZGuid.Empty;
			Statement.AddGroupByStatement_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			expectedText = "GROUP BY " + Statement.OrganisationHeader_ForTestOnly + "." + OrgHeaderSchema.Constants.PK + ", ";
			expectedText += AccTransactionHeaderSchema.Constants.AH_RX_NKTransactionCurrency + ", ";
			expectedText += Constants.AccTransactionHeader + "." + Constants.AH_GB + ", ";
			expectedText += Constants.AccTransactionHeader + "." + Constants.AH_GE + ", ";
			expectedText += Statement.OrganisationHeader_ForTestOnly + "." + OrgHeaderSchema.Constants.OH_Code + " ";

			SQLStringBuilder = new ZStringBuilder();
			Statement.IssueByTransactionDepartment = true;
			Statement.TransactionDepartment_PK = ZGuid.NewZGuid();
			Statement.AddGroupByStatement_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());

			SQLStringBuilder = new ZStringBuilder();
			Statement.IssueByTransactionDepartment = true;
			Statement.TransactionDepartment_PK = ZGuid.Empty;
			Statement.AddGroupByStatement_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", expectedText, SQLStringBuilder.ToString());
		}

		public void TestAddHavingFilterForCreditStatements_ExcludeStatementsInCredit()
		{
			Statement.CreditStatements = CreditOptions.ExcludeDocumentsInCredit;
			Statement.AddHavingFilter_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedString = "HAVING SUM(" + Constants.AH_OutstandingAmount + ") > 0 ";
			AssertEquals("StringBuilder Contents", expectedString, SQLStringBuilder.ToString());

			SQLStringBuilder = new ZStringBuilder();
			SqlParameters = new ZSqlParameterCollection();

			int currentPeriod = Statement.PeriodCalculator_ForTestOnly.GetPeriodFromDate(ZDateTime.Today);

			Statement.CutOffPeriod = Statement.PeriodCalculator_ForTestOnly.GetPreviousPeriod(currentPeriod);
			Statement.AddHavingFilter_ForTestOnly(SQLStringBuilder, SqlParameters);

			expectedString = "HAVING SUM(" + Constants.AH_LocalTotal + ") - SUM(ISNULL(Matches.SumOfMatches, 0)) > 0 ";
			AssertEquals("StringBuilder Contents", expectedString, SQLStringBuilder.ToString());
		}

		public void TestAddHavingFilterForCreditStatements_OnlyStatementsInCredit()
		{
			Statement.CreditStatements = CreditOptions.OnlyDocumentsInCredit;
			Statement.AddHavingFilter_ForTestOnly(SQLStringBuilder, SqlParameters);

			ZString expectedString = "HAVING SUM(" + Constants.AH_OutstandingAmount + ") <= 0 ";
			AssertEquals("StringBuilder Contents", expectedString, SQLStringBuilder.ToString());

			SQLStringBuilder = new ZStringBuilder();
			SqlParameters = new ZSqlParameterCollection();

			int currentPeriod = Statement.PeriodCalculator_ForTestOnly.GetPeriodFromDate(ZDateTime.Today);

			Statement.CutOffPeriod = Statement.PeriodCalculator_ForTestOnly.GetPreviousPeriod(currentPeriod);
			Statement.AddHavingFilter_ForTestOnly(SQLStringBuilder, SqlParameters);

			expectedString = "HAVING SUM(" + Constants.AH_LocalTotal + ") - SUM(ISNULL(Matches.SumOfMatches, 0)) <= 0 ";
			AssertEquals("StringBuilder Contents", expectedString, SQLStringBuilder.ToString());
		}

		public void TestAddHavingFilterForCreditStatements_AllStatements()
		{
			Statement.CreditStatements = CreditOptions.AllDocuments;
			Statement.AddHavingFilter_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", ZString.Empty, SQLStringBuilder.ToString());
		}

		public void TestAddOrderByStatement()
		{
			Statement.AddOrderByStatement_ForTestOnly(SQLStringBuilder, SqlParameters);
			AssertEquals("StringBuilder Contents", "ORDER BY " + Constants.TransactionOrg + "." + Constants.OH_Code, SQLStringBuilder.ToString());
		}

		#endregion

		#region Test BusinessObjectCollection

		#region OrganisationFilter

		public void TestFilterWhenOrgIsNotDebtorAndNoTransactions()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.CompanyData.OB_IsDebtor = false;
			Factory.Save();
			CreateARTransaction(null, 100M, "00001000", GlbBranch.CurrentBranch);
			CreateARTransaction(null, 100M, "00001001", GlbBranch.CurrentBranch);
			Factory.Save();

			Statement.Organisation_List.LoadWithMoreFiltering(new ZQuery(OrgHeaderSchema.PK, header.PK));
			AssertEquals("Collection should not contain TestOrgHeader", false, Statement.Organisation_List.Contains(header.PK));
		}

		public void TestFilterWhenOrgIsNotDebtorAndHasTransactionsWithBalanceOfZero()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.CompanyData.OB_IsDebtor = false;
			Factory.Save();
			CreateARTransaction(header, 0M, "00001000", GlbBranch.CurrentBranch);
			CreateARTransaction(null, 0M, "00001001", GlbBranch.CurrentBranch);
			Factory.Save();

			Statement.Organisation_List.LoadWithMoreFiltering(new ZQuery(OrgHeaderSchema.PK, header.PK));
			AssertEquals("Collection should not contain TestOrgHeader", false, Statement.Organisation_List.Contains(header.PK));
		}

		public void TestFilterWhenOrgIsNotDebtorAndHasTransactionsWithBalanceOutstanding()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_IsDebtor = false;
			Factory.Save();
			CreateARTransaction(header, 100M, "00001000", GlbBranch.CurrentBranch);
			CreateARTransaction(null, 100M, "00001001", GlbBranch.CurrentBranch);
			Factory.Save();

			Statement.Organisation_List.LoadWithMoreFiltering(new ZQuery(OrgHeaderSchema.PK, header.PK));
			AssertEquals("Collection should contain TestOrgHeader", true, Statement.Organisation_List.Contains(header.PK));
		}

		public void TestFilterWhenOrgIsDebtorAndNoTransactions()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.CompanyData.OB_IsDebtor = true;
			Factory.Save();
			CreateARTransaction(null, 100M, "00001000", GlbBranch.CurrentBranch);
			CreateARTransaction(null, 100M, "00001001", GlbBranch.CurrentBranch);
			Factory.Save();

			Statement.Organisation_List.LoadWithMoreFiltering(new ZQuery(OrgHeaderSchema.PK, header.PK));
			AssertEquals("Collection should contain TestOrgHeader", true, Statement.Organisation_List.Contains(header.PK));
		}

		public void TestFilterWhenOrgIsDebtorAndHasTransactionsWithBalanceOfZero()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.CompanyData.OB_IsDebtor = true;
			Factory.Save();
			CreateARTransaction(header, 0M, "00001000", GlbBranch.CurrentBranch);
			CreateARTransaction(header, 0M, "00001001", GlbBranch.CurrentBranch);
			Factory.Save();

			Statement.Organisation_List.LoadWithMoreFiltering(new ZQuery(OrgHeaderSchema.PK, header.PK));
			AssertEquals("Collection should contain TestOrgHeader", true, Statement.Organisation_List.Contains(header.PK));
		}

		public void TestFilterWhenOrgIsDebtorAndHasTransactionsWithBalanceOutstanding()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.CompanyData.OB_IsDebtor = true;
			Factory.Save();
			CreateARTransaction(header, 100M, "00001000", GlbBranch.CurrentBranch);
			CreateARTransaction(header, -100M, "00001001", GlbBranch.CurrentBranch);
			Factory.Save();

			Statement.Organisation_List.LoadWithMoreFiltering(new ZQuery(OrgHeaderSchema.PK, header.PK));
			AssertEquals("Collection should TestOrgHeader", true, Statement.Organisation_List.Contains(header.PK));
		}

		public void TestFilterWhenOrgIsNotDebtorAndHasTransactionsInOtherCompany()
		{
			OrgHeader header1 = Factory.NewWithValidTestData<OrgHeader>();
			header1.OH_Code = "ABC";
			header1.CompanyData.OB_IsDebtor = false;

			OrgHeader header2 = Factory.NewWithValidTestData<OrgHeader>();
			header2.OH_Code = "XYZ";
			header2.CompanyData.OB_IsDebtor = false;

			Factory.Save();

			CreateARTransaction(header1, 100M, "00001000", GetBranchFromOtherCompany());
			CreateARTransaction(header2, 100M, "00001001", GlbBranch.CurrentBranch);
			Factory.Save();

			ZQuery query = new ZQuery(OrgHeaderSchema.PK, header1.PK);
			query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.PK, SQLComparisonOperator.Equal, header2.PK);
			Statement.Organisation_List.LoadWithMoreFiltering(query);

			AssertEquals("Collection should not contain TestOrgHeader 1", false, Statement.Organisation_List.Contains(header1.PK));
			AssertEquals("Collection should contain TestOrgHeader 2", true, Statement.Organisation_List.Contains(header2.PK));
		}

		public void TestFilterWhenOrgIsNotDebtorAndHasTransactionsInAPLedger()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.CompanyData.OB_IsDebtor = false;
			Factory.Save();
			CreateAPTransaction(header, 100M, "00001000", GlbBranch.CurrentBranch);
			CreateARTransaction(null, 100M, "00001001", GlbBranch.CurrentBranch);
			Factory.Save();

			AssertEquals("Collection should not contain TestOrgHeader 1", false, Statement.Organisation_List.Contains(header.PK));
		}

		GlbBranch GetBranchFromOtherCompany()
		{
			ZQuery findCompanyThatIsNotThisCompanyFilter = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			GlbCompany otherCompany = Factory.LoadTop1<GlbCompany>(findCompanyThatIsNotThisCompanyFilter);
			return otherCompany.Branches[0];
		}

		void CreateARTransaction(OrgHeader orgHeader, ZDecimal outstandingAmount, ZString transactionNum, GlbBranch branch)
		{
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, orgHeader, outstandingAmount, transactionNum, branch);
		}

		void CreateAPTransaction(OrgHeader orgHeader, ZDecimal outstandingAmount, ZString transactionNum, GlbBranch branch)
		{
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, orgHeader, outstandingAmount, transactionNum, branch);
		}

		void CreateTransaction(ZString ledger, OrgHeader orgHeader, ZDecimal outstandingAmount, ZString transactionNum, GlbBranch branch)
		{
			AccTransactionHeader transactionHeader = Factory.New<AccTransactionHeader>();
			transactionHeader.AH_Ledger = ledger;
			transactionHeader.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			transactionHeader.AH_GB = branch.PK;
			transactionHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
			transactionHeader.AH_OH = (orgHeader != null) ? orgHeader.PK : ZGuid.Empty;
			transactionHeader.AH_OutstandingAmount = outstandingAmount;
			transactionHeader.AH_OSTotal = outstandingAmount;
			transactionHeader.AH_InvoiceAmount = outstandingAmount;

			if (outstandingAmount == 0M)
			{
				transactionHeader.AH_FullyPaidDate = ZDateTime.Now.AddDays(-5);
			}

			transactionHeader.AH_InvoiceDate = ZDateTime.Now;
			transactionHeader.AH_PostDate = ZDateTime.Now;
			transactionHeader.AH_DueDate = ZDateTime.Now;
			transactionHeader.AH_TransactionNum = transactionNum;
		}

		#endregion

		#region Old Tests

		[ExpectNoExceptions]
		public void TestBizObjCollectionForAllOrgansiationsWithNoCurrency()
		{
			DynamicBusinessObjectCollection testCollection = new DynamicBusinessObjectCollection(Factory);

			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-5);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.ExcludeDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-10);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.OnlyDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-2);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());
		}

		[ExpectNoExceptions]
		public void TestBizObjCollectionForAllOrgansiationsWithCurrency()
		{
			DynamicBusinessObjectCollection testCollection = new DynamicBusinessObjectCollection(Factory);

			Statement.RX_PK = Factory.LoadTop1(typeof(RefCurrency), new ZQuery()).PK;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-5);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.ExcludeDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-10);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.OnlyDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-2);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());
		}

		[ExpectNoExceptions]
		public void TestBizObjCollectionForOneOrganisationWithNoCurrency()
		{
			DynamicBusinessObjectCollection testCollection = new DynamicBusinessObjectCollection(Factory);
			Statement.OH_PK = (Factory.LoadTop1<OrgCompanyData>(new ZQuery(OrgCompanyDataSchema.OB_IsDebtor, SQLComparisonOperator.Equal, "Y"))).OB_OH;

			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-5);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.ExcludeDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-10);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.OnlyDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-2);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());
		}

		[ExpectNoExceptions]
		public void TestBizObjCollectionWithBranchFilterWithNoCurrency()
		{
			DynamicBusinessObjectCollection testCollection = new DynamicBusinessObjectCollection(Factory);
			Statement.GB_PK = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;

			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-5);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.ExcludeDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-10);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.OnlyDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-2);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());
		}

		[ExpectNoExceptions]
		public void TestBizObjCollectionWithBranchFilterWithCurrency()
		{
			DynamicBusinessObjectCollection testCollection = new DynamicBusinessObjectCollection(Factory);
			Statement.GB_PK = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;

			Statement.RX_PK = Factory.LoadTop1(typeof(RefCurrency), new ZQuery()).PK;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-5);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.ExcludeDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-10);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.OnlyDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-2);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());
		}

		[ExpectNoExceptions]
		public void TestBizObjCollectionWithARGroupFilterWithNoCurrency()
		{
			DynamicBusinessObjectCollection testCollection = new DynamicBusinessObjectCollection(Factory);
			Statement.OJ_PK = Factory.LoadTop1(typeof(OrgDebtorGroup), new ZQuery()).PK;

			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-5);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.ExcludeDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-10);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.OnlyDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-2);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());
		}

		[ExpectNoExceptions]
		public void TestBizObjCollectionWithARGroupFilterWithCurrency()
		{
			DynamicBusinessObjectCollection testCollection = new DynamicBusinessObjectCollection(Factory);
			Statement.OJ_PK = Factory.LoadTop1(typeof(OrgDebtorGroup), new ZQuery()).PK;

			Statement.RX_PK = Factory.LoadTop1(typeof(RefCurrency), new ZQuery()).PK;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-5);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.ExcludeDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-10);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.OnlyDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-2);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());
		}

		[ExpectNoExceptions]
		public void TestBizObjCollectionWithSalesRepFilterWithNoCurrency()
		{
			DynamicBusinessObjectCollection testCollection = new DynamicBusinessObjectCollection(Factory);
			Statement.SalesRep_PK = Factory.LoadTop1(typeof(GlbStaff), new ZQuery()).PK;

			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-5);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.ExcludeDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-10);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.OnlyDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-2);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());
		}

		[ExpectNoExceptions]
		public void TestBizObjCollectionWithSalesRepFilterWithCurrency()
		{
			DynamicBusinessObjectCollection testCollection = new DynamicBusinessObjectCollection(Factory);
			Statement.SalesRep_PK = Factory.LoadTop1(typeof(GlbStaff), new ZQuery()).PK;

			Statement.RX_PK = Factory.LoadTop1(typeof(RefCurrency), new ZQuery()).PK;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-5);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.ExcludeDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-10);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Empty;
			Statement.CreditStatements = CreditOptions.OnlyDocumentsInCredit;
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());

			Statement.CutOffDate = ZDateTime.Today.AddDays(-2);
			testCollection = new DynamicBusinessObjectCollection(Factory);
			testCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Is a collection", typeof(DynamicBusinessObjectCollection), testCollection.GetType());
		}

		#endregion

		public void TestBizObjCollectionWithNoFilters()
		{
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			Factory.Save();

			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();

			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
		}

		public void TestBizObjCollectionWithInvoiceBatch()
		{
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);

			InvoiceBatchHeader header = Factory.New<InvoiceBatchHeader>();
			header.AH_OH = Creator.ZECTRA.PK;
			header.AH_InvoiceAmount = 100m;
			header.AH_OutstandingAmount = 100m;
			header.AH_OSTotal = 100M;
			header.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			Factory.Save();

			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();

			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);
		}

		public void TestBizObjCollectionWithOrganisationFilter()
		{
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ZECTRA, 100M, 10M);
			Factory.Save();

			Statement.OH_PK = Creator.ABIGAS.PK;
			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();

			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);
		}

		public void TestBizObjCollectionWithDebtorGroupFilter()
		{
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ZECTRA, 100M, 10M);

			Statement.DebtorGroup_List.Load();
			ZGuid debtorGroup1 = Statement.DebtorGroup_List[0].PK;
			ZGuid debtorGroup2 = Statement.DebtorGroup_List[1].PK;

			Statement.OJ_PK = debtorGroup1;
			Creator.ABIGAS.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup1;
			Creator.AALSHI.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup2;
			Creator.ZECTRA.CompanyData.OB_OJ_ARDebtorGroup = ZGuid.Empty;
			Factory.Save();

			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();

			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);
		}

		public void TestBizObjCollectionWithBranchFilter()
		{
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ZECTRA, 100M, 10M);

			Statement.Branch_List.Load();
			ZGuid branch1 = Statement.Branch_List[0].PK;
			ZGuid branch2 = Statement.Branch_List[1].PK;

			Creator.ABIGAS.CompanyData.OB_GB_ControllingBranch = branch1;
			Creator.AALSHI.CompanyData.OB_GB_ControllingBranch = branch2;
			Creator.ZECTRA.CompanyData.OB_GB_ControllingBranch = ZGuid.Empty;
			Factory.Save();

			Statement.GB_PK = branch1;
			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();

			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);
		}

		public void TestBizObjCollectionWithAccountsRelationshipFilter()
		{
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ZECTRA, 100M, 10M);

			Statement.AccountsRelationShip = Statement.AccountsRelationShip_List[0].Code;
			Creator.ABIGAS.CompanyData.OB_ARCategory = Statement.AccountsRelationShip_List[0].Code;
			Creator.AALSHI.CompanyData.OB_ARCategory = Statement.AccountsRelationShip_List[1].Code;
			Creator.ZECTRA.CompanyData.OB_ARCategory = ZString.Empty;
			Factory.Save();

			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();

			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);
		}

		public void TestBizObjCollectionWithConsolidationCategoryFilter()
		{
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ZECTRA, 100M, 10M);

			Statement.ConsolidationCategory = Statement.ConsolidationCategory_List[0].Code;
			Creator.ABIGAS.CompanyData.OB_ARConsolidatedAccountingCategory = Statement.ConsolidationCategory_List[0].Code;
			Creator.AALSHI.CompanyData.OB_ARConsolidatedAccountingCategory = Statement.ConsolidationCategory_List[1].Code;
			Creator.ZECTRA.CompanyData.OB_ARConsolidatedAccountingCategory = ZString.Empty;
			Factory.Save();

			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();

			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);
		}

		public void TestBizObjCollectionWithCreditRatingFilter()
		{
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ZECTRA, 100M, 10M);

			Statement.CreditRating = Statement.CreditRating_List[0].Code;
			Creator.ABIGAS.CompanyData.OB_ARCreditRating = Statement.CreditRating_List[0].Code;
			Creator.AALSHI.CompanyData.OB_ARCreditRating = Statement.CreditRating_List[1].Code;
			Creator.ZECTRA.CompanyData.OB_ARCreditRating = ZString.Empty;
			Factory.Save();

			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();

			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);
		}

		public void TestBizObjCollectionWithSalesRepFilter()
		{
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ZECTRA, 100M, 10M);

			GlbStaff aBIGASSalesRep = Statement.SalesRep_List[0];
			GlbStaff aALSHISalesRep = Statement.SalesRep_List[1];

			CreateOrgStaffAssignment(Creator.ABIGAS, aBIGASSalesRep, StaffAssignmentRoles.Codes.SalesRep);
			CreateOrgStaffAssignment(Creator.AALSHI, aALSHISalesRep, StaffAssignmentRoles.Codes.SalesRep);

			Factory.Save();

			Statement.SalesRep_PK = aBIGASSalesRep.PK;
			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();

			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);
		}

		public void TestBizObjCollectionWithCustomerServiceRepFilter()
		{
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ZECTRA, 100M, 10M);

			GlbStaff aBIGASCustomerServiceRep = Statement.CustomerServiceRep_List[0];
			GlbStaff aALSHICustomerServiceRep = Statement.CustomerServiceRep_List[1];

			CreateOrgStaffAssignment(Creator.ABIGAS, aBIGASCustomerServiceRep, StaffAssignmentRoles.Codes.CustomerServiceRep);
			CreateOrgStaffAssignment(Creator.AALSHI, aALSHICustomerServiceRep, StaffAssignmentRoles.Codes.CustomerServiceRep);

			Factory.Save();

			Statement.CustomerServiceRep_PK = aBIGASCustomerServiceRep.PK;
			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();

			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);
		}

		public void TestBizObjCollectionWithCreditControllerFilter()
		{
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ZECTRA, 100M, 10M);

			GlbStaff aBIGASCreditController = Statement.CreditController_List[0];
			GlbStaff aALSHICreditController = Statement.CreditController_List[1];

			CreateOrgStaffAssignment(Creator.ABIGAS, aBIGASCreditController, StaffAssignmentRoles.Codes.CreditController);
			CreateOrgStaffAssignment(Creator.AALSHI, aALSHICreditController, StaffAssignmentRoles.Codes.CreditController);

			Factory.Save();

			Statement.CreditController_PK = aBIGASCreditController.PK;
			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();

			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);
		}

		public void TestBizObjCollectionWithCreditStatementFilter()
		{
			AccTransactionHeader aBIGASInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			TransactionMatchLinkGroup matchGroup = new TransactionMatchLinkGroup(Factory);
			SetUpMatchLinkForTransaction(matchGroup, aBIGASInvoice, 50M, ZDateTime.Now);
			AccTransactionHeader aBIGASReceipt = CreateReceiptForOrgHeader(Creator.ABIGAS, 1000M);
			SetUpMatchLinkForTransaction(matchGroup, aBIGASReceipt, -990M, ZDateTime.Now);

			AccTransactionHeader aALSHIInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, -100M, -10M);
			SetUpMatchLinkForTransaction(matchGroup, aBIGASInvoice, -50M, ZDateTime.Now);

			AccTransactionHeader zECTRAInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ZECTRA, -100M, -10M);
			SetUpMatchLinkForTransaction(matchGroup, zECTRAInvoice, -100M, ZDateTime.Now);

			aBIGASReceipt.AH_OutstandingAmount = -10M;
			zECTRAInvoice.AH_OutstandingAmount = -10M;

			BalanceMatchGroup(matchGroup);

			Factory.Save();

			Statement.CreditStatements = CreditOptions.AllDocuments;
			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();

			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, true);

			Statement.CreditStatements = CreditOptions.ExcludeDocumentsInCredit;
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);

			Statement.CreditStatements = CreditOptions.OnlyDocumentsInCredit;
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();

			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, true);
		}

		public void TestBizObjCollectionWithCurrencySpecified()
		{
			RefCurrency uSD = Creator.USD;
			RefCurrency aUD = Creator.AUD;

			ZDateTime now = ZDateTime.Now;
			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			ZQuery aBIGASFilter = new ZQuery(AccTransactionHeaderSchema.AH_OH, Creator.ABIGAS.PK);
			AccTransactionHeader aBIGASInvoice = Factory.LoadTop1<AccTransactionHeader>(aBIGASFilter);
			aBIGASInvoice.AH_RX_NKTransactionCurrency = uSD.RX_Code;

			SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			ZQuery aALSHIFilter = new ZQuery(AccTransactionHeaderSchema.AH_OH, Creator.AALSHI.PK);
			AccTransactionHeader aALSHIInvoice = Factory.LoadTop1<AccTransactionHeader>(aALSHIFilter);
			aALSHIInvoice.AH_RX_NKTransactionCurrency = aUD.RX_Code;

			Factory.Save();

			Statement.RX_PK = ZGuid.Empty;
			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);

			Statement.RX_PK = aUD.PK;
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
		}

		public void TestBizObjCollectionWithTransactionBranchSpecified()
		{
			GlbBranch currentBranch = GlbBranch.CurrentBranch;
			GlbBranch branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = currentBranch.GB_GC;

			Factory.Save();

			AccTransactionHeader aBIGASInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);

			AccTransactionHeader aALSHIInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			aALSHIInvoice.AH_GB = branch1.PK;

			Factory.Save();

			Statement.TransactionBranch_PK = ZGuid.Empty;
			Statement.IssueByTransactionBranch = false;
			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);

			Statement.TransactionBranch_PK = branch1.PK;
			Statement.IssueByTransactionBranch = false;
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);

			Statement.TransactionBranch_PK = ZGuid.Empty;
			Statement.IssueByTransactionBranch = true;
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);

			Statement.TransactionBranch_PK = branch1.PK;
			Statement.IssueByTransactionBranch = true;
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
		}

		public void TestBizObjCollectionWithTransactionDepartmentSpecified()
		{
			GlbDepartment currentDepartment = GlbDepartment.CurrentDepartment;
			GlbDepartment department1 = Factory.New<GlbDepartment>();

			Factory.Save();

			AccTransactionHeader aBIGASInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);

			AccTransactionHeader aALSHIInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			aALSHIInvoice.AH_GE = department1.PK;

			Factory.Save();

			Statement.TransactionDepartment_PK = ZGuid.Empty;
			Statement.IssueByTransactionDepartment = false;
			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);

			Statement.TransactionDepartment_PK = department1.PK;
			Statement.IssueByTransactionDepartment = false;
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);

			Statement.TransactionDepartment_PK = ZGuid.Empty;
			Statement.IssueByTransactionDepartment = true;
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);

			Statement.TransactionDepartment_PK = department1.PK;
			Statement.IssueByTransactionDepartment = true;
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
		}

		public void TestBizObjCollectionWithInvoiceDateSpecified()
		{
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
			// Invoice Dates On Or Before

			ZDateTime now = ZDateTime.Now;

			AccTransactionHeader aBIGASInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			aBIGASInvoice.AH_InvoiceDate = ZDateTime.Now;

			AccTransactionHeader aALSHIInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			aALSHIInvoice.AH_InvoiceDate = ZDateTime.Now.AddDays(-10);

			AccTransactionHeader zECTRAInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ZECTRA, 100M, 10M);
			zECTRAInvoice.AH_InvoiceDate = ZDateTime.Now.AddDays(-20);
			Factory.Save();

			Statement.CutOffDate = ZDateTime.Empty;
			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, true);

			Statement.CutOffDate = now;
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, true);

			Statement.CutOffDate = now.AddDays(-5);
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, true);

			Statement.CutOffDate = now.AddDays(-10);
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, true);

			Statement.CutOffDate = now.AddDays(-15);
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, true);

			Statement.CutOffDate = now.AddDays(-20);
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, true);

			Statement.CutOffDate = now.AddDays(-25);
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);
		}

		public void TestBizObjCollectionWithDueDateSpecified()
		{
			ZDateTime now = ZDateTime.Now;
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.FirstReminder;
			// Due Dates On Or Before

			AccTransactionHeader aBIGASInvoice;
			aBIGASInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			aBIGASInvoice.AH_DueDate = now.AddDays(-20);

			AccTransactionHeader aALSHIInvoice;
			aALSHIInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			aALSHIInvoice.AH_DueDate = now.AddDays(-10);

			AccTransactionHeader zECTRAInvoice;
			zECTRAInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ZECTRA, 100M, 10M);
			zECTRAInvoice.AH_DueDate = now;
			Factory.Save();

			Statement.CutOffDate = ZDateTime.Empty;
			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, true);

			Statement.CutOffDate = now;
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, true);

			Statement.CutOffDate = now.AddDays(-10);
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);

			Statement.CutOffDate = now.AddDays(-15);
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);

			Statement.CutOffDate = now.AddDays(-20);
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);

			Statement.CutOffDate = now.AddDays(-25);
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);
		}

		public void TestBizObjCollectionWithAmountGreaterThanSpecified()
		{
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.FirstReminder;
			Statement.CreditStatements = CreditOptions.AllDocuments;

			AccTransactionHeader aBIGASInvoice;
			aBIGASInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 300M, 30M);
			TransactionMatchLinkGroup matchGroup = new TransactionMatchLinkGroup(Factory);
			SetUpMatchLinkForTransaction(matchGroup, aBIGASInvoice, 100, ZDateTime.Today);
			aBIGASInvoice.AH_OutstandingAmount = 230M;

			AccTransactionHeader aALSHIInvoice;
			aALSHIInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			SetUpMatchLinkForTransaction(matchGroup, aALSHIInvoice, 10, ZDateTime.Today);
			aALSHIInvoice.AH_OutstandingAmount = 100M;

			AccTransactionHeader zECTRAInvoice;
			zECTRAInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ZECTRA, 100M, 10M);
			SetUpMatchLinkForTransaction(matchGroup, zECTRAInvoice, 100, ZDateTime.Today);
			zECTRAInvoice.AH_OutstandingAmount = 10M;
			BalanceMatchGroup(matchGroup);

			Factory.Save();

			Statement.OutstandingAmountGreaterThan = 0M;
			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, true);

			Statement.OutstandingAmountGreaterThan = 50M;
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);

			Statement.OutstandingAmountGreaterThan = 200M;
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);

			Statement.OutstandingAmountGreaterThan = 500M;
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, false);
		}

		public void TestBizObjCollectionWithIsDisbursementSpecified()
		{
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.FirstReminder;

			AssertEquals("Precondition: there are 4 disbursement invoice types.", 4, InvoiceTypeCalculationProvider.DisbursementInvoiceTypes.Length);

			ZDateTime now = ZDateTime.Now;
			AccTransactionHeader aBIGASInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 10M);
			aBIGASInvoice.AH_TransactionCategory = InvoiceTypeCalculationProvider.DisbursementInvoiceTypes[0];
			AccTransactionHeader aALSHIInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 10M);
			aALSHIInvoice.AH_TransactionCategory = InvoiceTypeCalculationProvider.DisbursementInvoiceTypes[1];
			AccTransactionHeader zECTRAInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ZECTRA, 100M, 10M);
			zECTRAInvoice.AH_TransactionCategory = InvoiceTypeCalculationProvider.DisbursementInvoiceTypes[2];
			AccTransactionHeader agentInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.Agent, 100M, 10M);
			agentInvoice.AH_TransactionCategory = InvoiceTypeCalculationProvider.DisbursementInvoiceTypes[3];
			AccTransactionHeader localClientInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.LocalClient, 100M, 10M);
			localClientInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();

			Statement.DisbursementInvoicesOnly = false;
			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.Agent, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.LocalClient, businessObjCollection, true);

			Statement.DisbursementInvoicesOnly = true;
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.Agent, businessObjCollection, true);
			AssertCollectionContainsHeader(Creator.LocalClient, businessObjCollection, false);
		}

		public void TestBizObjCollectionWithEndOfPeriod()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupPeriods();

			AccTransactionHeader aBIGASInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ABIGAS, 100M, 0M);
			aBIGASInvoice.AH_PostDate = testHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(2);
			TransactionMatchLinkGroup matchGroup = new TransactionMatchLinkGroup(Factory);
			SetUpMatchLinkForTransaction(matchGroup, aBIGASInvoice, 50M, testHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(2));
			SetUpMatchLinkForTransaction(matchGroup, aBIGASInvoice, 50M, testHelper.CurrentPeriod.AM_StartDate.AddDays(2));

			AccTransactionHeader aALSHIInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.AALSHI, 100M, 0M);
			aALSHIInvoice.AH_PostDate = testHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(2);
			SetUpMatchLinkForTransaction(matchGroup, aALSHIInvoice, 100M, testHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(2));

			AccTransactionHeader xLINDUInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.XLINDU, 100M, 0M);
			xLINDUInvoice.AH_PostDate = testHelper.PreviousOpenPeriod.AM_StartDate.AddDays(2);
			SetUpMatchLinkForTransaction(matchGroup, xLINDUInvoice, 40M, testHelper.PreviousOpenPeriod.AM_StartDate.AddDays(2));

			AccTransactionHeader zECTRAInvoice = SetUpOrgAsDebtorAndAddOutstandingTransaction(Creator.ZECTRA, 100M, 0M);
			zECTRAInvoice.AH_PostDate = testHelper.CurrentPeriod.AM_StartDate.AddDays(2);

			aBIGASInvoice.AH_OutstandingAmount = 0;
			aALSHIInvoice.AH_OutstandingAmount = 0;
			xLINDUInvoice.AH_OutstandingAmount = 60M;

			BalanceMatchGroup(matchGroup);

			Factory.Save();

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;

			Statement.CutOffPeriod = 0;
			DynamicBusinessObjectCollection collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, collection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, collection, false);
			AssertCollectionContainsHeader(Creator.XLINDU, collection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, collection, true);

			Statement.CutOffPeriod = testHelper.CurrentPeriodInt;
			collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, collection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, collection, false);
			AssertCollectionContainsHeader(Creator.XLINDU, collection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, collection, true);

			Statement.CutOffPeriod = testHelper.PreviousOpenPeriodInt;
			collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, collection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, collection, false);
			AssertCollectionContainsHeader(Creator.XLINDU, collection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, collection, false);

			Statement.CutOffPeriod = testHelper.PreviousGLClosedPeriodInt;
			collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, collection, true);
			AssertCollectionContainsHeader(Creator.AALSHI, collection, false);
			AssertCollectionContainsHeader(Creator.XLINDU, collection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, collection, false);

			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.FirstReminder;
			Statement.CreditStatements = CreditOptions.AllDocuments;

			Statement.CutOffPeriod = testHelper.CurrentPeriodInt;
			Statement.OutstandingAmountGreaterThan = 61M;
			collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, collection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, collection, false);
			AssertCollectionContainsHeader(Creator.XLINDU, collection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, collection, true);

			Statement.CutOffPeriod = testHelper.CurrentPeriodInt;
			Statement.OutstandingAmountGreaterThan = 60M;
			collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, collection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, collection, false);
			AssertCollectionContainsHeader(Creator.XLINDU, collection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, collection, true);

			Statement.CutOffPeriod = testHelper.PreviousOpenPeriodInt;
			Statement.OutstandingAmountGreaterThan = 61M;
			collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, collection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, collection, false);
			AssertCollectionContainsHeader(Creator.XLINDU, collection, false);
			AssertCollectionContainsHeader(Creator.ZECTRA, collection, true);

			Statement.CutOffPeriod = testHelper.PreviousOpenPeriodInt;
			Statement.OutstandingAmountGreaterThan = 60M;
			collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(Creator.ABIGAS, collection, false);
			AssertCollectionContainsHeader(Creator.AALSHI, collection, false);
			AssertCollectionContainsHeader(Creator.XLINDU, collection, true);
			AssertCollectionContainsHeader(Creator.ZECTRA, collection, true);
		}

		public void TestStatementWithSettlementGroupIsCompanySpecific()
		{
			OrgHeader settlementGroup = CreateNewDebtor("GroupA");
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = true;
			org.OH_Code = "Org";

			ZQuery companyFilter = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			GlbCompany anotherCompany = Factory.LoadTop1<GlbCompany>(companyFilter);

			OrgRelatedParty decoySettlementGroup = Factory.New<OrgRelatedParty>();
			decoySettlementGroup.PR_OH_Parent = settlementGroup.PK;
			decoySettlementGroup.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			decoySettlementGroup.PR_GC = anotherCompany.PK;
			decoySettlementGroup.PR_FreightDirection = RelatedPartyDirectionList.Codes.AR;
			decoySettlementGroup.PR_OH_RelatedParty = org.PK;

			CreateARTransaction(settlementGroup, 100M, "00001000", GlbBranch.CurrentBranch);

			Factory.Save();

			Statement.IssueBySettlementGroup = true;
			DynamicBusinessObjectCollection collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(settlementGroup, collection, true);
			AssertCollectionContainsHeader(org, collection, false);
		}

		public void TestBizObjCollection_SettlementGroupHasOtherOrgsReferencing()
		{
			OrgHeader settlementGroupA = CreateNewDebtor("GroupA");
			settlementGroupA.ARSettlementGroupPK = settlementGroupA.PK;
			OrgHeader organisationA1 = CreateNewDebtorAndAssignSettlementGroup("OrgA1", settlementGroupA);
			OrgHeader organisationA2 = CreateNewDebtorAndAssignSettlementGroup("OrgA2", settlementGroupA);

			OrgHeader settlementGroupB = CreateNewDebtor("GroupB");
			settlementGroupB.ARSettlementGroupPK = ZGuid.Empty;
			OrgHeader organisationB1 = CreateNewDebtorAndAssignSettlementGroup("OrgB1", settlementGroupA);
			OrgHeader organisationB2 = CreateNewDebtorAndAssignSettlementGroup("OrgB2", settlementGroupA);

			CreateARTransaction(settlementGroupA, 100M, "00001000", GlbBranch.CurrentBranch);
			CreateARTransaction(organisationA1, 100M, "00001001", GlbBranch.CurrentBranch);
			CreateARTransaction(organisationA2, 100M, "00001002", GlbBranch.CurrentBranch);

			CreateARTransaction(settlementGroupB, 100M, "00001003", GlbBranch.CurrentBranch);
			CreateARTransaction(organisationB1, 100M, "00001004", GlbBranch.CurrentBranch);
			CreateARTransaction(organisationB2, 100M, "00001005", GlbBranch.CurrentBranch);

			OrgMiscServ companyDataA = settlementGroupA.MiscServ;
			OrgMiscServ companyDataA1 = organisationA1.MiscServ;
			OrgMiscServ companyDataA2 = organisationA2.MiscServ;
			OrgMiscServ companyDataB = settlementGroupB.MiscServ;
			OrgMiscServ companyDataB1 = organisationB1.MiscServ;
			OrgMiscServ companyDataB2 = organisationB2.MiscServ;

			Factory.Save();

			Statement.IssueBySettlementGroup = false;
			DynamicBusinessObjectCollection collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(settlementGroupA, collection, true);
			AssertCollectionContainsHeader(organisationA1, collection, true);
			AssertCollectionContainsHeader(organisationA2, collection, true);
			AssertCollectionContainsHeader(settlementGroupB, collection, true);
			AssertCollectionContainsHeader(organisationB1, collection, true);
			AssertCollectionContainsHeader(organisationB2, collection, true);

			Statement.IssueBySettlementGroup = true;
			collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(settlementGroupA, collection, true);
			AssertCollectionContainsHeader(organisationA1, collection, false);
			AssertCollectionContainsHeader(organisationA2, collection, false);
			AssertCollectionContainsHeader(settlementGroupB, collection, true);
			AssertCollectionContainsHeader(organisationB1, collection, false);
			AssertCollectionContainsHeader(organisationB2, collection, false);
		}

		public void TestBizObjCollection_SettlementGroupHasNoTransactions()
		{
			OrgHeader settlementGroupA = CreateNewDebtor("GroupA");
			settlementGroupA.ARSettlementGroupPK = settlementGroupA.PK;
			OrgHeader organisationA1 = CreateNewDebtorAndAssignSettlementGroup("OrgA1", settlementGroupA);
			OrgHeader organisationA2 = CreateNewDebtorAndAssignSettlementGroup("OrgA2", settlementGroupA);

			CreateARTransaction(organisationA1, 100M, "00001001", GlbBranch.CurrentBranch);
			CreateARTransaction(organisationA2, 100M, "00001002", GlbBranch.CurrentBranch);

			OrgMiscServ companyDataA = settlementGroupA.MiscServ;
			OrgMiscServ companyDataB = organisationA1.MiscServ;
			OrgMiscServ companyDataC = organisationA2.MiscServ;

			Factory.Save();

			Statement.IssueBySettlementGroup = false;
			DynamicBusinessObjectCollection collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(settlementGroupA, collection, false);
			AssertCollectionContainsHeader(organisationA1, collection, true);
			AssertCollectionContainsHeader(organisationA2, collection, true);

			Statement.IssueBySettlementGroup = true;
			collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(settlementGroupA, collection, true);
			AssertCollectionContainsHeader(organisationA1, collection, false);
			AssertCollectionContainsHeader(organisationA2, collection, false);
		}

		public void TestBizObjCollection_NoOrganisationsReferencingSettlementGroup()
		{
			OrgHeader settlementGroupA = CreateNewDebtor("GroupA");
			settlementGroupA.ARSettlementGroupPK = settlementGroupA.PK;
			CreateARTransaction(settlementGroupA, 100M, "00001001", GlbBranch.CurrentBranch);

			OrgHeader settlementGroupB = CreateNewDebtor("GroupB");
			settlementGroupB.ARSettlementGroupPK = ZGuid.Empty;
			CreateARTransaction(settlementGroupB, 100M, "00001002", GlbBranch.CurrentBranch);

			OrgMiscServ companyDataA = settlementGroupA.MiscServ;
			OrgMiscServ companyDataB = settlementGroupB.MiscServ;

			Factory.Save();

			Statement.IssueBySettlementGroup = false;
			DynamicBusinessObjectCollection collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(settlementGroupA, collection, true);
			AssertCollectionContainsHeader(settlementGroupB, collection, true);

			Statement.IssueBySettlementGroup = true;
			collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertCollectionContainsHeader(settlementGroupA, collection, true);
			AssertCollectionContainsHeader(settlementGroupB, collection, true);
		}

		public void TestDoesntProduceStatementsWithNoTransactions()
		{
			OrgHeader debtor = CreateNewDebtor("NEWDEBTOR");
			CreateARTransaction(debtor, 0, "00001000", GlbBranch.CurrentBranch);
			Factory.Save();
			Statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementOnly;
			Statement.OH_PK = debtor.PK;
			Statement.CreditStatements = CreditOptions.AllDocuments;
			DynamicBusinessObjectCollection collection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Shouldn't include transactions with zero amount", 0, collection.Count);
		}

		public void TestBizObjCollectionWhenPrintAccountMovementSOAisTrue()
		{
			ZGuid anotherGLBCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;
			ZQuery filter = new ZQuery(OrgCompanyDataSchema.OB_OH, Creator.AALSHI.PK);
			filter.AddToFilter(OrgCompanyDataSchema.OB_GC, anotherGLBCompany);
			OrgCompanyData oldCompanyData = Factory.LoadTop1<OrgCompanyData>(filter);
			oldCompanyData.Delete();
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = anotherGLBCompany;
			Creator.AALSHI.CompanyData.OB_GC = anotherGLBCompany;
			Factory.Save();

			var transaction1 = CreateCRDTransaction(GlbBranch.CurrentBranch, Creator.ABIGAS, Creator.USD, "CRD0001000", 200.0M, 0.75M, new ZDateTime(2015, 02, 12, 0, 0, 0));

			var transaction2 = Creator.CreateInvoice(typeof(ARInvoice), Creator.USD, 0.75m, Creator.ABIGAS, new ZDateTime(2015, 02, 13, 0, 0, 0));
			transaction2.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction2.AH_TransactionType = TransactionTypes.InvoiceBatch;

			var transaction3 = CreateINVTransaction(GlbBranch.CurrentBranch, Creator.ABIGAS, Creator.USD, "INV0001001", 150M, 0.75M, new ZDateTime(2015, 02, 14, 15, 25, 0));
			var transaction4 = CreateINVTransaction(GlbBranch.CurrentBranch, Creator.ABIGAS, Creator.USD, "INV00001002", 110M, 0.75M, new ZDateTime(2015, 02, 15, 0, 0, 0));
			var transaction5 = CreateINVTransaction(GlbBranch.CurrentBranch, Creator.AALSHI, Creator.USD, "INV00001003", 110M, 0.75M, new ZDateTime(2015, 02, 15, 0, 0, 0));
			var transaction6 = CreateINVTransaction(GlbBranch.CurrentBranch, Creator.AALSHI, Creator.USD, "INV00001004", 120M, 0.75M, new ZDateTime(2015, 02, 16, 0, 0, 0));
			var transaction7 = CreateINVTransaction(GlbBranch.CurrentBranch, Creator.AALSHI, Creator.USD, "INV00001005", 120M, 0.75M, new ZDateTime(2015, 02, 16, 0, 0, 0));
			var transaction8 = CreateRECTransaction(GlbBranch.CurrentBranch, Creator.ABIGAS, Creator.AUD, "REC00001006", 120M, new ZDateTime(2015, 02, 17, 0, 0, 0));
			var transaction9 = CreateRECTransaction(GlbBranch.CurrentBranch, Creator.AALSHI, Creator.AUD, "REC00001007", 120M, new ZDateTime(2015, 02, 17, 0, 0, 0));

			Factory.Save();

			Statement.PrintAccountMovementSOA = true;
			Statement.PrintAccountMovementFromDate = new ZDateTime(2015, 02, 12, 0, 0, 0);
			Statement.PrintAccountMovementToDate = new ZDateTime(2015, 02, 14, 0, 0, 0);
			Statement.GroupByAccountMovementSOALine = "NON";
			Statement.CreditStatements = CreditOptions.AllDocuments;
			DynamicBusinessObjectCollection businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertEquals("Number of Transactions", 1, businessObjCollection.Count);
			AssertBizOForAccountMovementSOA(businessObjCollection, Creator.ABIGAS, "USD", 0, false);

			Statement.PrintAccountMovementFromDate = new ZDateTime(2015, 02, 14, 0, 0, 0);
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertBizOForAccountMovementSOA(businessObjCollection, Creator.ABIGAS, "USD", -200, false);

			Statement.PrintAccountMovementToDate = new ZDateTime(2015, 02, 17, 0, 0, 0);
			businessObjCollection = Statement.GetBusinessObjCollection_ForTestOnly();
			AssertBizOForAccountMovementSOA(businessObjCollection, Creator.ABIGAS, "AUD", -266.67, true);
			AssertBizOForAccountMovementSOA(businessObjCollection, Creator.AALSHI, "AUD", 0, true);
		}
		OrgHeader CreateNewDebtor(string oH_Code)
		{
			OrgHeader newDebtor = Factory.NewWithValidTestData<OrgHeader>();
			newDebtor.OH_IsDebtor = true;
			newDebtor.OH_Code = oH_Code;
			return newDebtor;
		}

		OrgHeader CreateNewDebtorAndAssignSettlementGroup(string oH_Code, OrgHeader settlementGroupTableName)
		{
			OrgHeader newDebtor = CreateNewDebtor(oH_Code);
			newDebtor.ARSettlementGroupPK = settlementGroupTableName.PK;
			return newDebtor;
		}

		#endregion

		#region Test Print Task

		[ExpectNoExceptions]
		public void TestNoExceptionIsThrownWhenNoPublishedARInvoiceDocumentIsFound_LegacyInvoices()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			TestNoExceptionIsThrownWhenNoPublishedARInvoiceDocumentIsFound_Core("Invoice");
		}

		[ExpectNoExceptions]
		public void TestNoExceptionIsThrownWhenNoPublishedARInvoiceDocumentIsFound_DocBuilderInvoices()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestNoExceptionIsThrownWhenNoPublishedARInvoiceDocumentIsFound_Core("DocBuilder Invoice");
		}

		void TestNoExceptionIsThrownWhenNoPublishedARInvoiceDocumentIsFound_Core(string sU_MenuName)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(StmMenuItemSchema.SU_MenuName, sU_MenuName);
			query.AddToFilter(StmMenuItemSchema.SU_IsPublished, true);
			StmMenuItem[] invoiceMenuItems = Factory.Load<StmMenuItem>(query);
			foreach (StmMenuItem invoiceMenuItem in invoiceMenuItems)
			{
				invoiceMenuItem.SU_IsPublished = false;
			}

			OrgHeader header = Creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			Invoice invoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			invoice.AH_OH = header.PK;
			Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 100);
			Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 200);

			Factory.Save();

			Statement statement = (Statement)GetNewBusinessObject();
			statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementAndInvoices;
			statement.GetPrintTaskForTest_ForTestOnly();
		}

		[ExpectException(typeof(UnableToFindInvoiceDocumentCommandException))]
		public void TestCorrectExceptionIsThrownWhenNoInvoiceDocumentIsFound_LegacyInvoices()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			TestExceptionIsThrownWhenARInvoiceDocumentIsNotFound_Core("Invoice");
		}

		[ExpectException(typeof(UnableToFindInvoiceDocumentCommandException))]
		public void TestCorrectExceptionIsThrownWhenNoInvoiceDocumentIsFound_DocBuilderInvoices()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ZQuery query = new ZQuery();
			query.AddToFilter(StmMenuItemSchema.SU_MenuName, "DocBuilder Invoice");
			StmMenuItem invoiceMenuItem = Factory.LoadTop1<StmMenuItem>(query);
			AccountingConfigurationRegistry.Instance.ARInvoiceMenuItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, invoiceMenuItem.PK.ToGuid());

			TestExceptionIsThrownWhenARInvoiceDocumentIsNotFound_Core("DocBuilder Invoice");
		}

		public virtual void TestGetStatementDocumentCommandLoadCorrectDocument()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderStatementDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var testCommand = Factory.NewWithValidTestData<DocumentCommand>();
			testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testCommand.SU_IsPublished = true;
			testCommand.SU_IsSystemDefined = false;
			testCommand.SU_MenuName = Statement.StandardStatementAndCollectionLetterTemplateName_ForTestOnly;
			testCommand.SU_EmailSubjectLine = string.Empty;
			testCommand.SU_MenuPath = string.Empty;

			var header = Creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			var invoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			invoice.AH_OH = header.PK;
			Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 100);
			Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 200);

			Factory.Save();

			var statement = (Statement)GetNewBusinessObject();
			statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementOnly;
			if (statement.UseNewPrintStreaming_ForTestOnly)
			{
				statement.GetPrintTaskIEnum();
				foreach (var pack in statement.GetPacksIEnum_ForTestOnly())
				{
					AssertNotNull("PreCondition", pack);
					AssertEquals("Should found the document", 1, pack.DeliveryInstructions.DeliverablesToBePrinted.Count);
				}
			}
			else
			{
				var documentPrintSet = statement.GetPrintTask();
				var pack = documentPrintSet.GetFirstDocumentPack();
				AssertNotNull("PreCondition", pack);
				AssertEquals("Should found the document", 1, pack.DeliveryInstructions.DeliverablesToBePrinted.Count);
			}
		}

		void TestExceptionIsThrownWhenARInvoiceDocumentIsNotFound_Core(string sU_MenuName)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(StmMenuItemSchema.SU_MenuName, sU_MenuName);
			StmMenuItem[] invoiceMenuItems = Factory.Load<StmMenuItem>(query);
			foreach (StmMenuItem invoiceMenuItem in invoiceMenuItems)
			{
				invoiceMenuItem.SU_MenuPath = "backupPath";
			}

			OrgHeader header = Creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			Invoice invoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			invoice.AH_OH = header.PK;
			Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 100);
			Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 200);

			Factory.Save();

			Statement statement = (Statement)GetNewBusinessObject();
			statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementAndInvoices;
			if (statement.UseNewPrintStreaming_ForTestOnly)
			{
				statement.GetPrintTaskIEnum();
				foreach (var pack in statement.GetPacksIEnum_ForTestOnly())
				{
					AssertNotNull(pack);
				}
			}
			else
			{
				statement.GetPrintTask();
			}
		}

		public void TestStatementSetDocBuilderInvoiceBusinessContextFromParentJobIfExists()
		{
			Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			OrgHeader header = Creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			Invoice invoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			invoice.AH_OH = header.PK;

			JobHeader header1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			header1.JH_GC = GlbCompany.CurrentCompany.PK;
			header1.JH_GB = GlbBranch.CurrentBranch.PK;
			header1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Freight.Forwarding.Business.ForwardingShipment jobShipment = Factory.NewWithValidTestData<Freight.Forwarding.Business.ForwardingShipment>();
			header1.JH_ParentID = jobShipment.PK;
			header1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			invoice.AH_JH = header1.PK;

			Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 100);
			Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 200);
			Factory.Save();

			Statement statement = (Statement)GetNewBusinessObject();
			statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementAndInvoices;
			PrintTask task = statement.GetPrintTaskForTest_ForTestOnly();

			foreach (DocumentPack pack in task.GetDocumentPacks())
			{
				AssertEquals("Statement business context should be Statement", "Statement", pack[0].MenuItem.SU_BusinessContext);
				AssertEquals("DocBuilder Invoice business context should be Shipment", "Shipment", pack[1].MenuItem.SU_BusinessContext);
			}
		}

		public void TestStatementDisplayDate()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupPeriods();
			Factory.Save();

			AccPeriodManagement lastPeriod = testHelper.PreviousOpenPeriod;
			AccPeriodManagement currentPeriod = testHelper.CurrentPeriod;
			AccPeriodManagement nextPeriod = testHelper.FuturePeriod;

			AssertNotNull("Precondition: Last Period should not be null", lastPeriod);
			AssertNotNull("Precondition: Current Period should not be null", currentPeriod);
			AssertNotNull("Precondition: Next Period should not be null", nextPeriod);

			PrintStatement printStatement = new PrintStatement(Factory, GlbBranch.CurrentBranch);

			AssertStatementDisplayDate(printStatement, ZDateTime.Empty, 0, ZDateTime.Today);

			AssertStatementDisplayDate(printStatement, ZDateTime.Today.AddDays(1), 0, ZDateTime.Today);
			AssertStatementDisplayDate(printStatement, ZDateTime.Empty, nextPeriod.AM_Period, ZDateTime.Today);
			AssertStatementDisplayDate(printStatement, ZDateTime.Today.AddDays(1), nextPeriod.AM_Period, ZDateTime.Today);

			AssertStatementDisplayDate(printStatement, ZDateTime.Today.AddDays(-1), 0, ZDateTime.Today.AddDays(-1));
			AssertStatementDisplayDate(printStatement, ZDateTime.Empty, lastPeriod.AM_Period, lastPeriod.AM_EndDate.Date);
			AssertStatementDisplayDate(printStatement, ZDateTime.Today.AddDays(-1), lastPeriod.AM_Period, lastPeriod.AM_EndDate.Date);
		}

		void AssertStatementDisplayDate(PrintStatement printStatement, ZDateTime cutoffDate, ZInt cutoffPeriod, ZDateTime expected)
		{
			Statement.CutOffDate = cutoffDate;
			Statement.CutOffPeriod = cutoffPeriod;
			Statement.SetStatementDisplayDateForStatement_ForTestOnly(printStatement);
			AssertEquals("Statement Display Date", expected, printStatement.StatementDisplayDate);
		}

		public void TestStatementOrdering()
		{
			OrgHeader header1 = Creator.CreateOrgHeader("ABC123", false, true, true, false, true, false);
			OrgHeader header2 = Creator.CreateOrgHeader("XYZ987", false, true, true, false, true, false);
			OrgHeader header3 = Creator.CreateOrgHeader("IMR111", false, true, true, false, true, false);
			Factory.Save();

			Invoice inv1 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv1.AH_OH = header1.PK;
			Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 100);
			Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 200);

			Factory.Save();

			Invoice inv2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv2.AH_OH = header2.PK;
			Creator.CreateInvoiceLine(inv2, Creator.AUD, 1m, 300);
			Creator.CreateInvoiceLine(inv2, Creator.AUD, 1m, 400);

			Factory.Save();

			Invoice inv3 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv3.AH_OH = header3.PK;
			Creator.CreateInvoiceLine(inv3, Creator.AUD, 1m, 500);
			Creator.CreateInvoiceLine(inv3, Creator.AUD, 1m, 600);

			Factory.Save();

			Statement testStatement = (Statement)GetNewBusinessObject();
			List<DocumentPack> documentPacks = testStatement.GetPrintTaskForTest_ForTestOnly().GetDocumentPacks().ToList();
			AssertEquals("First pack should be for ABC123", header1.OH_Code, documentPacks[0].Organisation.OH_Code);
			AssertEquals("Second pack should be for IMR111", header3.OH_Code, documentPacks[1].Organisation.OH_Code);
			AssertEquals("First pack should be for XYZ987", header2.OH_Code, documentPacks[2].Organisation.OH_Code);
		}

		public void TestCreatePrintTask()
		{
			OrgHeader header1 = Creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			Invoice inv1 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv1.AH_OH = header1.PK;
			Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 100);
			Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 200);

			OrgHeader header2 = Creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			Invoice inv2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv2.AH_OH = header2.PK;
			Creator.CreateInvoiceLine(inv2, Creator.AUD, 1m, 300);
			Creator.CreateInvoiceLine(inv2, Creator.AUD, 1m, 400);

			Invoice inv3 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv3.AH_OH = header2.PK;
			Creator.CreateInvoiceLine(inv3, Creator.AUD, 1m, 500);
			Creator.CreateInvoiceLine(inv3, Creator.AUD, 1m, 600);

			Factory.Save();

			Statement testStatement = (Statement)GetNewBusinessObject();
			PrintTask task = testStatement.GetPrintTaskForTest_ForTestOnly();
			List<DocumentPack> documentPacks = task.GetDocumentPacks().ToList();
			AssertEquals("should be two print tasks", 2, documentPacks.Count);
			AssertEquals("Task delivery instructions PK", testStatement.MenuPKForStatementOfAccount_ForTestOnly, task.DeliveryInstructionsDefaultPK);
		}

		[ExpectNoExceptions()]
		public void TestGetPrintTaskFindCorrectStatementSummaryMenuToUse()
		{
			Creator.ABIGAS.CompanyData.OB_IsDebtor = true;
			Creator.ABIGAS.OH_Code = "AAA";
			Creator.AALSHI.CompanyData.OB_IsDebtor = true;
			Creator.AALSHI.OH_Code = "BBB";

			Invoice aUDInvoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice, Creator.ABIGAS.PK, Creator.AUD.RX_Code, "00001000", 100M, 10M);

			Factory.Save();

			ARCreditNote uSDCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			SetUpOutstandingTransaction(uSDCreditNote, Creator.ABIGAS.PK, Creator.USD.RX_Code, "00001002", -100M, -10M);

			ARReceipt uSDReceipt = Factory.NewWithValidTestData<ARReceipt>();
			SetUpOutstandingTransaction(uSDReceipt, Creator.ABIGAS.PK, Creator.USD.RX_Code, "00001004", 100M, 10M);
			uSDReceipt.AH_OutstandingAmount = 110M;

			Factory.Save();

			Statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementOnly;
			Statement.IncludeDebtorSummaryPage = true;

			DocumentsDataRegistry.Instance.UseNewDocBuilderStatementDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			PrintTask task = Statement.GetPrintTaskForTest_ForTestOnly();
			List<DocumentPack> documentPacks = task.GetDocumentPacks().ToList();
			AssertEquals("There should be 1 tasks in the Print Task", 1, documentPacks.Count);
			AssertEquals("First pack should contain 3 documents", 3, documentPacks[0].Count);
			AssertEquals("Menu path should be 'Legacy Document'", "Legacy Documents", documentPacks[0][0].MenuItem.SU_MenuPath);

			DocumentsDataRegistry.Instance.UseNewDocBuilderStatementDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			task = Statement.GetPrintTaskForTest_ForTestOnly();
			documentPacks = task.GetDocumentPacks().ToList();
			AssertEquals("There should be 1 document pack in the Print Task", 1, documentPacks.Count);
			AssertEquals("First pack should contain 3 documents", 3, documentPacks[0].Count);
			AssertEquals("Menu path should be empty", ZString.Empty, documentPacks[0][0].MenuItem.SU_MenuPath);
		}

		[ExpectNoExceptions()]
		public void TestGetPrintTaskUseSameStatementOfAccountMenuToUse()
		{
			OrgHeader header1 = Creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			Invoice inv1 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv1.AH_OH = header1.PK;
			Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 100);
			Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 200);

			OrgHeader header2 = Creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			Invoice inv2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv2.AH_OH = header2.PK;
			Creator.CreateInvoiceLine(inv2, Creator.AUD, 1m, 300);
			Creator.CreateInvoiceLine(inv2, Creator.AUD, 1m, 400);

			Factory.Save();

			Statement testStatement = (Statement)GetNewBusinessObject();
			DocumentsDataRegistry.Instance.UseNewDocBuilderStatementDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			PrintTask task = testStatement.GetPrintTaskForTest_ForTestOnly();
			List<DocumentPack> documentPacks1 = task.GetDocumentPacks().ToList();
			AssertEquals("should be two print tasks", 2, documentPacks1.Count);
			foreach (DocumentPack docPack in documentPacks1)
			{
				AssertEquals("Menu path should be empty", "", docPack.StmMenuCommand.SU_MenuPath);
				AssertEquals("SU_IsClientSpecific should be false for DocBuilder Statement", false, docPack.StmMenuCommand.SU_IsClientSpecific);
				AssertEquals("SU_FilterList should hide menu", "\"<CurrentCompany.Country.Code>\" == \"HideThisDocument\"", docPack.StmMenuCommand.SU_FilterList);
			}
			AssertEquals("Task delivery instructions PK", Statement.MenuPKForStatementOfAccount_ForTestOnly, task.DeliveryInstructionsDefaultPK);

			Statement testStatement2 = (Statement)GetNewBusinessObject();
			DocumentsDataRegistry.Instance.UseNewDocBuilderStatementDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			PrintTask task2 = testStatement2.GetPrintTaskForTest_ForTestOnly();
			List<DocumentPack> documentPacks2 = task2.GetDocumentPacks().ToList();
			AssertEquals("should be two print tasks", 2, documentPacks2.Count);
			foreach (DocumentPack docPack in documentPacks2)
			{
				AssertEquals("Menu path should be 'Legacy Document'", "Legacy Documents", docPack.StmMenuCommand.SU_MenuPath);
				AssertEquals("SU_IsClientSpecific should equal to ExpectedIsClientSpecific for Non-DocBuilder Statement", ExpectedIsClientSpecific, docPack.StmMenuCommand.SU_IsClientSpecific);
				if (!ExpectedSpecificCountryCode.IsEmpty)
				{
					AssertEquals("SU_IsClientSpecific should be true", true, docPack.StmMenuCommand.SU_IsClientSpecific);
					Assert("SU_FilterList should contain ExpectedSpecificCountryCode for Non-DocBuilder Client Statement", docPack.StmMenuCommand.SU_FilterList.Contains(ExpectedSpecificCountryCode));
				}
			}
			AssertEquals("Task delivery instructions PK", Statement.MenuPKForStatementOfAccount_ForTestOnly, task2.DeliveryInstructionsDefaultPK);
		}

		public void TestCreatePrintTaskForOneCompany()
		{
			OrgHeader header1 = Creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			Invoice inv1 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv1.AH_OH = header1.PK;
			Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 100);
			Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 200);

			OrgHeader header2 = Creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			Invoice inv2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv2.AH_OH = header2.PK;
			Creator.CreateInvoiceLine(inv2, Creator.AUD, 1m, 300);
			Creator.CreateInvoiceLine(inv2, Creator.AUD, 1m, 400);

			Invoice inv3 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv3.AH_OH = header2.PK;
			Creator.CreateInvoiceLine(inv3, Creator.AUD, 1m, 500);
			Creator.CreateInvoiceLine(inv3, Creator.AUD, 1m, 600);

			Factory.Save();

			Statement testStatement = (Statement)GetNewBusinessObject();
			testStatement.OH_PK = header1.PK;
			List<DocumentPack> documentPacks = testStatement.GetPrintTaskForTest_ForTestOnly().GetDocumentPacks().ToList();
			AssertEquals("should be one documents, because we filter by one company", 1, documentPacks.Count);
		}

		public void TestCreatePrintTaskForAnotherGlbCompany()
		{
			ZQuery filter = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			GlbCompany anotherCompany = Factory.LoadTop1<GlbCompany>(filter);
			GlbBranch anotherCompanyBranch = anotherCompany.Branches[0];

			OrgHeader header1 = Creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			Invoice inv1 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv1.AH_GB = anotherCompanyBranch.PK;
			inv1.AH_OH = header1.PK;
			InvoiceLine line1 = (InvoiceLine)Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 100);
			line1.AL_GB = inv1.AH_GB = anotherCompanyBranch.PK;
			InvoiceLine line2 = (InvoiceLine)Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 200);
			line2.AL_GB = inv1.AH_GB = anotherCompanyBranch.PK;

			// Having OrgCompanyData is essential, otherwise the query won't work
			OrgCompanyData header1CompanyData = Factory.New<OrgCompanyData>();
			header1CompanyData.OB_GC = anotherCompany.PK;
			header1CompanyData.OB_OH = header1.PK;

			Factory.Save();

			var testStatement = Statement.New(anotherCompanyBranch);
			testStatement.OH_PK = header1.PK;
			List<DocumentPack> documentPacks = testStatement.GetPrintTaskForTest_ForTestOnly().GetDocumentPacks().ToList();
			AssertEquals("should be one document, because we filter by one company", 1, documentPacks.Count);
		}

		public void TestCreatePrintTaskByTranzactionBranches()
		{
			GlbBranch currentBranch = GlbBranch.CurrentBranch;
			GlbBranch branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = currentBranch.GB_GC;

			Factory.Save();

			OrgHeader header1 = Creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			Invoice inv1 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv1.AH_OH = header1.PK;
			inv1.AH_GB = branch1.PK;
			Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 100);
			Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 200);

			Invoice inv2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv2.AH_OH = header1.PK;
			Creator.CreateInvoiceLine(inv2, Creator.AUD, 1m, 300);
			Creator.CreateInvoiceLine(inv2, Creator.AUD, 1m, 400);

			Invoice inv3 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv3.AH_OH = header1.PK;
			inv3.AH_GB = branch1.PK;
			Creator.CreateInvoiceLine(inv3, Creator.AUD, 1m, 500);
			Creator.CreateInvoiceLine(inv3, Creator.AUD, 1m, 600);

			Factory.Save();

			Statement testStatement = (Statement)GetNewBusinessObject();
			testStatement.IssueByTransactionBranch = true;
			testStatement.TransactionBranch_PK = ZGuid.Empty;
			PrintTask printTask = testStatement.GetPrintTaskForTest_ForTestOnly();
			List<DocumentPack> documentPacks = printTask.GetDocumentPacks().ToList();
			AssertEquals("Print Task Should Contain One Document Pack", 1, documentPacks.Count);
			AssertEquals("Document Pack Should Contain 2 documents", 2, documentPacks[0].Count);

			testStatement = (Statement)GetNewBusinessObject();
			testStatement.IssueByTransactionBranch = true;
			testStatement.TransactionBranch_PK = branch1.PK;
			printTask = testStatement.GetPrintTaskForTest_ForTestOnly();
			documentPacks = printTask.GetDocumentPacks().ToList();
			AssertEquals("Print Task Should Contain One Document Pack", 1, documentPacks.Count);
			AssertEquals("Document Pack Should Contain 1 document", 1, documentPacks[0].Count);

			PrintStatementDocumentSupporter docSupporter = documentPacks[0].DocumentSupporter as PrintStatementDocumentSupporter;
			AssertNotNull(docSupporter);
			AssertEquals("Transactions should have only invoces for one branch", 2, docSupporter.PrintStatement.Transactions.Count);
			AssertContainsInvoice(inv1, docSupporter.PrintStatement.Transactions);
			AssertContainsInvoice(inv3, docSupporter.PrintStatement.Transactions);

			testStatement = (Statement)GetNewBusinessObject();
			testStatement.IssueByTransactionBranch = false;
			testStatement.TransactionBranch_PK = ZGuid.Empty;
			printTask = testStatement.GetPrintTaskForTest_ForTestOnly();
			documentPacks = printTask.GetDocumentPacks().ToList();
			AssertEquals("Print Task Should Contain One Document Pack", 1, documentPacks.Count);
			AssertEquals("Document Pack Should Contain 1 document", 1, documentPacks[0].Count);

			docSupporter = documentPacks[0].DocumentSupporter as PrintStatementDocumentSupporter;
			AssertNotNull(docSupporter);
			AssertEquals("Transactions should have all invoces", 3, docSupporter.PrintStatement.Transactions.Count);
			AssertContainsInvoice(inv1, docSupporter.PrintStatement.Transactions);
			AssertContainsInvoice(inv2, docSupporter.PrintStatement.Transactions);
			AssertContainsInvoice(inv3, docSupporter.PrintStatement.Transactions);

			testStatement = (Statement)GetNewBusinessObject();
			testStatement.IssueByTransactionBranch = false;
			testStatement.TransactionBranch_PK = branch1.PK;
			printTask = testStatement.GetPrintTaskForTest_ForTestOnly();
			documentPacks = printTask.GetDocumentPacks().ToList();
			AssertEquals("Print Task Should Contain One Document Pack", 1, documentPacks.Count);
			AssertEquals("Document Pack Should Contain 1 document", 1, documentPacks[0].Count);

			docSupporter = documentPacks[0].DocumentSupporter as PrintStatementDocumentSupporter;
			AssertNotNull(docSupporter);
			AssertEquals("Transactions should have all invoces", 3, docSupporter.PrintStatement.Transactions.Count);
			AssertContainsInvoice(inv1, docSupporter.PrintStatement.Transactions);
			AssertContainsInvoice(inv2, docSupporter.PrintStatement.Transactions);
			AssertContainsInvoice(inv3, docSupporter.PrintStatement.Transactions);
		}

		public void TestCreatePrintTaskByTranzactionDepartments()
		{
			GlbDepartment currentDepartment = GlbDepartment.CurrentDepartment;
			GlbDepartment department1 = Factory.New<GlbDepartment>();

			Factory.Save();

			OrgHeader header1 = Creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			Invoice inv1 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv1.AH_OH = header1.PK;
			inv1.AH_GE = department1.PK;
			Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 100);
			Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 200);

			Invoice inv2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv2.AH_OH = header1.PK;
			Creator.CreateInvoiceLine(inv2, Creator.AUD, 1m, 300);
			Creator.CreateInvoiceLine(inv2, Creator.AUD, 1m, 400);

			Invoice inv3 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv3.AH_OH = header1.PK;
			inv3.AH_GE = department1.PK;
			Creator.CreateInvoiceLine(inv3, Creator.AUD, 1m, 500);
			Creator.CreateInvoiceLine(inv3, Creator.AUD, 1m, 600);

			Factory.Save();

			Statement testStatement = (Statement)GetNewBusinessObject();
			testStatement.IssueByTransactionDepartment = true;
			testStatement.TransactionDepartment_PK = ZGuid.Empty;
			PrintTask printTask = testStatement.GetPrintTaskForTest_ForTestOnly();
			List<DocumentPack> documentPacks = printTask.GetDocumentPacks().ToList();
			AssertEquals("Print Task Should Contain One Document Pack", 1, documentPacks.Count);
			AssertEquals("Document Pack Should Contain 2 documents", 2, documentPacks[0].Count);

			testStatement = (Statement)GetNewBusinessObject();
			testStatement.IssueByTransactionDepartment = true;
			testStatement.TransactionDepartment_PK = department1.PK;
			printTask = testStatement.GetPrintTaskForTest_ForTestOnly();
			documentPacks = printTask.GetDocumentPacks().ToList();
			AssertEquals("Print Task Should Contain One Document Pack", 1, documentPacks.Count);
			AssertEquals("Document Pack Should Contain 1 document, because we filter by one Department", 1, documentPacks[0].Count);

			PrintStatementDocumentSupporter docSupporter = documentPacks[0].DocumentSupporter as PrintStatementDocumentSupporter;
			AssertNotNull(docSupporter);
			AssertEquals("Transactions should have only invoces for one Department", 2, docSupporter.PrintStatement.Transactions.Count);
			AssertContainsInvoice(inv1, docSupporter.PrintStatement.Transactions);
			AssertContainsInvoice(inv3, docSupporter.PrintStatement.Transactions);

			testStatement = (Statement)GetNewBusinessObject();
			testStatement.IssueByTransactionDepartment = false;
			testStatement.TransactionDepartment_PK = ZGuid.Empty;
			printTask = testStatement.GetPrintTaskForTest_ForTestOnly();
			documentPacks = printTask.GetDocumentPacks().ToList();
			AssertEquals("Print Task Should Contain One Document Pack", 1, documentPacks.Count);
			AssertEquals("Document Pack Should Contain 1 document", 1, documentPacks[0].Count);

			docSupporter = documentPacks[0].DocumentSupporter as PrintStatementDocumentSupporter;
			AssertNotNull(docSupporter);
			AssertEquals("Transactions should have all invoces", 3, docSupporter.PrintStatement.Transactions.Count);
			AssertContainsInvoice(inv1, docSupporter.PrintStatement.Transactions);
			AssertContainsInvoice(inv2, docSupporter.PrintStatement.Transactions);
			AssertContainsInvoice(inv3, docSupporter.PrintStatement.Transactions);

			testStatement = (Statement)GetNewBusinessObject();
			testStatement.IssueByTransactionDepartment = false;
			testStatement.TransactionDepartment_PK = department1.PK;
			printTask = testStatement.GetPrintTaskForTest_ForTestOnly();
			documentPacks = printTask.GetDocumentPacks().ToList();
			AssertEquals("Print Task Should Contain One Document Pack", 1, documentPacks.Count);
			AssertEquals("Document Pack Should Contain 1 document", 1, documentPacks[0].Count);

			docSupporter = documentPacks[0].DocumentSupporter as PrintStatementDocumentSupporter;
			AssertNotNull(docSupporter);
			AssertEquals("Transactions should have all invoces", 3, docSupporter.PrintStatement.Transactions.Count);
			AssertContainsInvoice(inv1, docSupporter.PrintStatement.Transactions);
			AssertContainsInvoice(inv2, docSupporter.PrintStatement.Transactions);
			AssertContainsInvoice(inv3, docSupporter.PrintStatement.Transactions);
		}

		protected void AssertContainsInvoice(TransactionHeader invoice, TransactionHeaderCollection transactions)
		{
			AssertNotNull(invoice);
			AssertNotNull(transactions);
			bool isInvoiceContains = false;
			foreach (TransactionHeader transaction in transactions)
			{
				if (invoice.PK.Equals(transaction.PK))
				{
					isInvoiceContains = true;
					break;
				}
			}
			Assert("The invoice should be in transactions", isInvoiceContains);
		}

		public void TestNewResultAlwaysReturned()
		{
			OrgHeader header1 = Creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			Invoice inv1 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv1.AH_OH = header1.PK;
			Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 100);
			Creator.CreateInvoiceLine(inv1, Creator.AUD, 1m, 200);

			OrgHeader header2 = Creator.CreateOrgHeader(TestObjectCreator.GetRandomString(6), false, true, true, false, true, false);
			Invoice inv2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv2.AH_OH = header2.PK;
			Creator.CreateInvoiceLine(inv2, Creator.AUD, 1m, 300);
			Creator.CreateInvoiceLine(inv2, Creator.AUD, 1m, 400);

			Invoice inv3 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			inv3.AH_OH = header2.PK;
			Creator.CreateInvoiceLine(inv3, Creator.AUD, 1m, 500);
			Creator.CreateInvoiceLine(inv3, Creator.AUD, 1m, 600);

			Factory.Save();

			Statement testStatement = (Statement)GetNewBusinessObject();
			PrintTask task = testStatement.GetPrintTaskForTest_ForTestOnly();
			List<DocumentPack> documentPacks = task.GetDocumentPacks().ToList();
			AssertEquals("should be two print tasks", 2, documentPacks.Count);

			testStatement.OH_PK = header1.PK;
			PrintTask docs = testStatement.GetPrintTaskForTest_ForTestOnly();
			List<DocumentPack> documentPacks2 = docs.GetDocumentPacks().ToList();
			AssertEquals("should be one documents, because we filter by one company", 1, documentPacks2.Count);
		}

		public void TestGetPrintTask_Default()
		{
			Creator.AALSHI.OH_Code = "ABC123";
			Creator.AALSHI.CompanyData.OB_ARCombinedStatementInvoice = ZBool.True;

			Creator.ABIGAS.OH_Code = "XYZ987";
			Creator.ABIGAS.CompanyData.OB_ARCombinedStatementInvoice = ZBool.False;
			Factory.Save();

			Invoice inv1 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(inv1, Creator.AALSHI.PK, Creator.AUD.RX_Code, "00001000", 100M, 10M);

			ARCreditNote crd1 = Factory.NewWithValidTestData<ARCreditNote>();
			SetUpOutstandingTransaction(crd1, Creator.AALSHI.PK, Creator.USD.RX_Code, "00001001", -100M, -10M);

			Invoice inv2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.USD, 1.0M);
			SetUpOutstandingTransaction(inv2, Creator.ABIGAS.PK, Creator.USD.RX_Code, "00001002", 100M, 10M);

			Factory.Save();

			Statement statementObject = (Statement)GetNewBusinessObject();
			statementObject.IssueStatementPack = AccountingConstants.IssueStatementPackType.Default;
			PrintTask task = statementObject.GetPrintTaskForTest_ForTestOnly();
			List<DocumentPack> documentPacks = task.GetDocumentPacks().ToList();

			AssertEquals("There should be 2 tasks in the Print Task", 2, documentPacks.Count);
			AssertEquals("First pack should be for org ABC123", Creator.AALSHI.OH_Code, documentPacks[0].Organisation.OH_Code);
			AssertEquals("First pack should contain 4 documents", 4, documentPacks[0].Count);
			AssertEquals("Second pack should be for org XYZ987", Creator.ABIGAS.OH_Code, documentPacks[1].Organisation.OH_Code);
			AssertEquals("Second pack should contain 1 document", 1, documentPacks[1].Count);
			AssertEquals("IsReportPrintSet should be false", false, task.IsReportPrintSet);
		}

		public void TestGetPrintTaskSetMenuCommandCorrectlyForDocBuilderStatement()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderStatementDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			DocumentCommand docBuilderStatementOfAccountCommand = Factory.LoadTop1<DocumentCommand>(new ZQuery(new ZQuery(StmMenuItemSchema.SU_MenuName, "Statement Of Account"), new ZQuery(StmMenuItemSchema.SU_MenuPath, "")));
			DocumentCommand docBuilderStatementSummaryCommand = Factory.LoadTop1<DocumentCommand>(new ZQuery(new ZQuery(StmMenuItemSchema.SU_MenuName, "Statement Summary"), new ZQuery(StmMenuItemSchema.SU_MenuPath, "")));

			Creator.ABIGAS.CompanyData.OB_IsDebtor = true;
			Creator.ABIGAS.OH_Code = "AAA";
			Creator.AALSHI.CompanyData.OB_IsDebtor = true;
			Creator.AALSHI.OH_Code = "BBB";

			Invoice aUDInvoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice, Creator.ABIGAS.PK, Creator.AUD.RX_Code, "00001000", 100M, 10M);

			Invoice aUDInvoice2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice2, Creator.AALSHI.PK, Creator.AUD.RX_Code, "00001001", 100M, 10M);

			ARCreditNote uSDCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			SetUpOutstandingTransaction(uSDCreditNote, Creator.ABIGAS.PK, Creator.USD.RX_Code, "00001002", -100M, -10M);

			ARReceipt uSDReceipt = Factory.NewWithValidTestData<ARReceipt>();
			SetUpOutstandingTransaction(uSDReceipt, Creator.ABIGAS.PK, Creator.USD.RX_Code, "00001004", 100M, 10M);
			uSDReceipt.AH_OutstandingAmount = 110M;

			Factory.Save();

			Statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementOnly;
			Statement.IncludeDebtorSummaryPage = false;
			PrintTask task = Statement.GetPrintTaskForTest_ForTestOnly();
			AssertEquals("task should use the docBuilder statement of account menu", docBuilderStatementOfAccountCommand.PK, task.ParentMenuCommand.PK);

			Statement.IncludeDebtorSummaryPage = true;
			task = Statement.GetPrintTaskForTest_ForTestOnly();
			AssertEquals("task should use the docBuilder statement summary menu", docBuilderStatementSummaryCommand.PK, task.ParentMenuCommand.PK);
		}

		public void TestGetPrintTaskSetMenuCommandCorrectlyForLegacyStatement()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderStatementDocument.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			DocumentCommand legacyStatementSummaryCommand = Factory.LoadTop1<DocumentCommand>(new ZQuery(new ZQuery(StmMenuItemSchema.SU_MenuName, "Statement Summary"), new ZQuery(StmMenuItemSchema.SU_MenuPath, "Legacy Documents")));
			DocumentCommand legacyStatementOfAccountCommand = null;
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Statement Of Account");
			filter.AddToFilter(new ZQuery(StmMenuItemSchema.SU_MenuPath, "Legacy Documents"));

			if (!ExpectedIsClientSpecific)
			{
				filter.AddToFilter(new ZQuery(StmMenuItemSchema.SU_IsClientSpecific, "N"));
			}
			else
			{
				filter.AddToFilter(new ZQuery(StmMenuItemSchema.SU_IsClientSpecific, "Y"));
				if (!ExpectedSpecificCountryCode.IsEmpty)
				{
					filter.AddToFilter(new ZQuery(StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Contains, ExpectedSpecificCountryCode));
				}
			}
			legacyStatementOfAccountCommand = Factory.LoadTop1<DocumentCommand>(filter);

			Creator.ABIGAS.CompanyData.OB_IsDebtor = true;
			Creator.ABIGAS.OH_Code = "AAA";
			Creator.AALSHI.CompanyData.OB_IsDebtor = true;
			Creator.AALSHI.OH_Code = "BBB";

			Invoice aUDInvoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice, Creator.ABIGAS.PK, Creator.AUD.RX_Code, "00001000", 100M, 10M);

			Invoice aUDInvoice2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice2, Creator.AALSHI.PK, Creator.AUD.RX_Code, "00001001", 100M, 10M);

			ARCreditNote uSDCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			SetUpOutstandingTransaction(uSDCreditNote, Creator.ABIGAS.PK, Creator.USD.RX_Code, "00001002", -100M, -10M);

			ARReceipt uSDReceipt = Factory.NewWithValidTestData<ARReceipt>();
			SetUpOutstandingTransaction(uSDReceipt, Creator.ABIGAS.PK, Creator.USD.RX_Code, "00001004", 100M, 10M);
			uSDReceipt.AH_OutstandingAmount = 110M;

			Factory.Save();

			Statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementOnly;
			Statement.IncludeDebtorSummaryPage = false;
			PrintTask task = Statement.GetPrintTaskForTest_ForTestOnly();
			AssertEquals("task should use the legacy statement of account menu", legacyStatementOfAccountCommand.PK, task.ParentMenuCommand.PK);

			Statement.IncludeDebtorSummaryPage = true;
			task = Statement.GetPrintTaskForTest_ForTestOnly();
			AssertEquals("task should use the legacy statement summary menu", legacyStatementSummaryCommand.PK, task.ParentMenuCommand.PK);
		}

		public void TestGetPrintTask_StatementOnly()
		{
			Creator.AALSHI.OH_Code = "ABC123";
			Creator.AALSHI.CompanyData.OB_ARCombinedStatementInvoice = ZBool.True;

			Creator.ABIGAS.OH_Code = "XYZ987";
			Creator.ABIGAS.CompanyData.OB_ARCombinedStatementInvoice = ZBool.True;
			Factory.Save();

			Invoice inv1 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(inv1, Creator.AALSHI.PK, Creator.AUD.RX_Code, "00001000", 100M, 10M);

			ARCreditNote crd1 = Factory.NewWithValidTestData<ARCreditNote>();
			SetUpOutstandingTransaction(crd1, Creator.AALSHI.PK, Creator.USD.RX_Code, "00001001", -100M, -10M);

			Invoice inv2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.USD, 1.0M);
			SetUpOutstandingTransaction(inv2, Creator.ABIGAS.PK, Creator.USD.RX_Code, "00001002", 100M, 10M);

			Factory.Save();

			Statement statementObject = (Statement)GetNewBusinessObject();
			statementObject.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementOnly;
			PrintTask task = statementObject.GetPrintTaskForTest_ForTestOnly();
			List<DocumentPack> documentPacks = task.GetDocumentPacks().ToList();

			AssertEquals("There should be 2 tasks in the Print Task", 2, documentPacks.Count);
			AssertEquals("First pack should be for org ABC123", Creator.AALSHI.OH_Code, documentPacks[0].Organisation.OH_Code);
			AssertEquals("First pack should contain 2 documents", 2, documentPacks[0].Count);
			AssertEquals("Second pack should be for org XYZ987", Creator.ABIGAS.OH_Code, documentPacks[1].Organisation.OH_Code);
			AssertEquals("Second pack should contain 1 document", 1, documentPacks[1].Count);
		}

		public void TestGetPrintTask_StatementAndInvoices_LegacyInvoices()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			string sU_MenuName = "Invoice";

			TestGetPrintTask_StatementAndInvoices_Core(sU_MenuName);
		}

		public void TestGetPrintTask_StatementAndInvoices_DocBuilderInvoices()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string sU_MenuName = "DocBuilder Invoice";

			TestGetPrintTask_StatementAndInvoices_Core(sU_MenuName);
		}

		void TestGetPrintTask_StatementAndInvoices_Core(string sU_MenuName)
		{
			Creator.AALSHI.OH_Code = "ABC123";
			Creator.AALSHI.CompanyData.OB_ARCombinedStatementInvoice = ZBool.False;

			Creator.ABIGAS.OH_Code = "XYZ987";
			Creator.ABIGAS.CompanyData.OB_ARCombinedStatementInvoice = ZBool.False;
			Factory.Save();

			Invoice inv1 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(inv1, Creator.AALSHI.PK, Creator.AUD.RX_Code, "00001000", 100M, 10M);

			ARCreditNote crd1 = Factory.NewWithValidTestData<ARCreditNote>();
			SetUpOutstandingTransaction(crd1, Creator.AALSHI.PK, Creator.USD.RX_Code, "00001001", -100M, -10M);

			Invoice inv2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.USD, 1.0M);
			SetUpOutstandingTransaction(inv2, Creator.ABIGAS.PK, Creator.USD.RX_Code, "00001002", 100M, 10M);

			Factory.Save();

			Statement statementObject = (Statement)GetNewBusinessObject();
			statementObject.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementAndInvoices;
			statementObject.IssueByTransactionBranch = false;
			PrintTask task = statementObject.GetPrintTaskForTest_ForTestOnly();
			List<DocumentPack> documentPacks = task.GetDocumentPacks().ToList();

			AssertEquals("There should be 2 tasks in the Print Task", 2, documentPacks.Count);
			AssertEquals("First pack should be for org ABC123", Creator.AALSHI.OH_Code, documentPacks[0].Organisation.OH_Code);
			AssertEquals("First pack should contain 4 documents", 4, documentPacks[0].Count);
			AssertEquals("Task1 Invoice1 Default Menu Command", sU_MenuName, documentPacks[0][2].MenuItem.SU_MenuName);
			AssertEquals("Task1 Invoice2 Default Menu Command", sU_MenuName, documentPacks[0][3].MenuItem.SU_MenuName);
			AssertEquals("Second pack should be for org XYZ987", Creator.ABIGAS.OH_Code, documentPacks[1].Organisation.OH_Code);
			AssertEquals("Second pack should contain 2 documents", 2, documentPacks[1].Count);
			AssertEquals("Task2 Invoice Default Menu Command", sU_MenuName, documentPacks[1][1].MenuItem.SU_MenuName);

			ZQuery menuFilter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Periodic Invoice");
			menuFilter.AddToFilter(StmMenuItemSchema.SU_ContactType, "A/R");
			menuFilter.AddToFilter(StmMenuItemSchema.SU_IsPublished, ZBool.True);
			StmMenuItem item = Factory.LoadTop1<StmMenuItem>(menuFilter);
			AssertNotNull("'Periodic Invoice' menu item", item);
			Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.ARInvoiceMenuItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, item.PK.ToGuid());

			statementObject = (Statement)GetNewBusinessObject();
			statementObject.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementAndInvoices;
			statementObject.IssueByTransactionBranch = true;
			task = statementObject.GetPrintTaskForTest_ForTestOnly();
			documentPacks = task.GetDocumentPacks().ToList();

			AssertEquals("There should be 2 tasks in the Print Task", 2, documentPacks.Count);
			AssertEquals("First pack should be for org ABC123", Creator.AALSHI.OH_Code, documentPacks[0].Organisation.OH_Code);
			AssertEquals("First pack should contain 4 documents", 4, documentPacks[0].Count);
			AssertEquals("Task1 Invoice1 Menu Command", item.PK, documentPacks[0][2].MenuItem.PK);
			AssertEquals("Task1 Invoice2 Menu Command", item.PK, documentPacks[0][3].MenuItem.PK);
			AssertEquals("Second pack should be for org XYZ987", Creator.ABIGAS.OH_Code, documentPacks[1].Organisation.OH_Code);
			AssertEquals("Second pack should contain 2 documents", 2, documentPacks[1].Count);
			AssertEquals("Task2 Invoice Menu Command", item.PK, documentPacks[1][1].MenuItem.PK);
		}

		#endregion

		#region Test Event Raised On No Statements To Print

		public void TestEventRaisedOnNoStatementsToPrint()
		{
			Statement.NoDocumentsToPrint += new NoDocumentsToPrintEventHandler(Statement_NoStatementsToPrintEvent);
			AssertEquals("Precondition: Event should not be raised.", 0, Count);
			Statement.PrintStatements();
			AssertEquals("Event Should be raised once and only once.", 1, Count);
		}

		void Statement_NoStatementsToPrintEvent(object sender, NoDocumentsToPrintEventArgs e)
		{
			AssertEquals("No statements to Print", e.Caption);
			AssertEquals("Based on the given criteria, there are currently no statements to print.", e.Message);
			Count++;
		}

		int Count;

		#endregion

		#region Test ShouldAttachInvoices

		public void TestShouldAttachInvoices()
		{
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.MiscServ.OM_ARCombinedStatementInvoice = ZBool.True;
			PrintStatement statementBisObj = new PrintStatement(Factory, GlbBranch.CurrentBranch);
			statementBisObj.OrganisationPK = newOrganisation.PK;

			Statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementOnly;
			Assert("Function should return false", !Statement.ShouldAttachInvoices_ForTestOnly(statementBisObj));
			Statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementAndInvoices;
			Assert("Function should return true", Statement.ShouldAttachInvoices_ForTestOnly(statementBisObj));
			Statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.Default;
			Assert("Function should return true", Statement.ShouldAttachInvoices_ForTestOnly(statementBisObj));

			newOrganisation.MiscServ.OM_ARCombinedStatementInvoice = ZBool.False;
			Assert("Function should return false", !Statement.ShouldAttachInvoices_ForTestOnly(statementBisObj));
		}

		#endregion

		#region Test AppendTransactionsToDocumentPack

		public void TestAppendSummaryToDocumentPack()
		{
			Creator.ABIGAS.CompanyData.OB_IsDebtor = true;
			Creator.ABIGAS.OH_Code = "AAA";
			Creator.AALSHI.CompanyData.OB_IsDebtor = true;
			Creator.AALSHI.OH_Code = "BBB";

			Invoice aUDInvoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice, Creator.ABIGAS.PK, Creator.AUD.RX_Code, "00001000", 100M, 10M);

			Invoice aUDInvoice2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice2, Creator.AALSHI.PK, Creator.AUD.RX_Code, "00001001", 100M, 10M);

			Factory.Save();

			ARCreditNote uSDCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			SetUpOutstandingTransaction(uSDCreditNote, Creator.ABIGAS.PK, Creator.USD.RX_Code, "00001002", -100M, -10M);

			ARReceipt uSDReceipt = Factory.NewWithValidTestData<ARReceipt>();
			SetUpOutstandingTransaction(uSDReceipt, Creator.ABIGAS.PK, Creator.USD.RX_Code, "00001004", 100M, 10M);
			uSDReceipt.AH_OutstandingAmount = 110M;

			Factory.Save();

			Statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementOnly;
			Statement.IssueByTransactionBranch = false;
			PrintTask task = Statement.GetPrintTaskForTest_ForTestOnly();
			List<DocumentPack> documentPacks = task.GetDocumentPacks().ToList();
			AssertEquals("There should be 2 tasks in the Print Task", 2, documentPacks.Count);

			AssertEquals("First pack should be for org AAA", "AAA", documentPacks[0].Organisation.OH_Code);
			AssertEquals("First pack should contain 2 documents", 2, documentPacks[0].Count);
			AssertEquals("Second pack should be for org BBB", "BBB", documentPacks[1].Organisation.OH_Code);
			AssertEquals("Second pack should contain 1 documents", 1, documentPacks[1].Count);

			Statement.IncludeDebtorSummaryPage = true;
			task = Statement.GetPrintTaskForTest_ForTestOnly();
			documentPacks = task.GetDocumentPacks().ToList();

			AssertEquals("There should be 2 tasks in the Print Task", 2, documentPacks.Count);
			AssertEquals("First pack should be for org AAA", "AAA", documentPacks[0].Organisation.OH_Code);
			AssertEquals("First pack should contain 3 documents", 3, documentPacks[0].Count);
			AssertEquals("Second pack should be for org BBB", "BBB", documentPacks[1].Organisation.OH_Code);
			AssertEquals("Second pack should contain 1 documents", 1, documentPacks[1].Count);
		}

		public void TestSummaryDocumentPackHasSameParentWithStatementDocuments()
		{
			Creator.ABIGAS.CompanyData.OB_IsDebtor = true;
			Creator.ABIGAS.OH_Code = "AAA";

			Invoice aUDInvoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice, Creator.ABIGAS.PK, Creator.AUD.RX_Code, "00001000", 100M, 10M);

			ARCreditNote uSDCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			SetUpOutstandingTransaction(uSDCreditNote, Creator.ABIGAS.PK, Creator.USD.RX_Code, "00001002", -100M, -10M);

			Factory.Save();

			Statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementOnly;
			Statement.IssueByTransactionBranch = false;
			Statement.IncludeDebtorSummaryPage = true;

			Action assertSummaryDocumentPack = () =>
			{
				var task = Statement.GetPrintTaskForTest_ForTestOnly();
				List<DocumentPack> documentPacks = task.GetDocumentPacks().ToList();
				AssertEquals("There should be 1 tasks in the Print Task", 1, documentPacks.Count);
				AssertEquals("Document pack should be for org AAA", "AAA", documentPacks[0].Organisation.OH_Code);
				AssertEquals("Document pack should contain 3 documents", 3, documentPacks[0].Count);
				AssertEquals("Document pack should contain documents with same parent", 1, documentPacks[0].Cast<Report>().Select(x => x.Parent).Distinct().Count());
			};

			using (AccountingMasterFilesRegistry.Instance.StatementUsePrintStreaming.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				assertSummaryDocumentPack();
			}

			using (AccountingMasterFilesRegistry.Instance.StatementUsePrintStreaming.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				assertSummaryDocumentPack();
			}
		}

		public void TestDocumentPackWithMultipleInvoiceUseStreamPrinting()
		{
			Creator.ABIGAS.CompanyData.OB_IsDebtor = true;
			Creator.ABIGAS.OH_Code = "AAA";
			Creator.AALSHI.CompanyData.OB_IsDebtor = true;
			Creator.AALSHI.OH_Code = "BBB";

			var totalInvoice = PrintTask.MaxPreviewCount;
			for (var i = 0; i < totalInvoice; i++)
			{
				var transactionNumber = "0000100" + i.ToString();
				var invoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
				SetUpOutstandingTransaction(invoice, Creator.ABIGAS.PK, Creator.AUD.RX_Code, transactionNumber, 100M, 10M);
			}

			var invoice2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(invoice2, Creator.AALSHI.PK, Creator.AUD.RX_Code, "000111001", 100M, 10M);

			Factory.Save();

			Statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementAndInvoices;
			Statement.IssueByTransactionBranch = false;
			var task = Statement.GetPrintTaskForTest_ForTestOnly();

			AssertEquals("Print task should be StreamPrinting when StreamPrinting is set", Statement.UseNewPrintStreaming_ForTestOnly, task is DocumentPrintSetWithStreaming);

			List<DocumentPack> documentPacks = task.GetDocumentPacks().ToList();
			AssertEquals("There should be 2 tasks in the Print Task", 2, documentPacks.Count);
			AssertEquals("First pack should be for org AAA", "AAA", documentPacks[0].Organisation.OH_Code);
			AssertEquals("First pack should contain 51 documents", PrintTask.MaxPreviewCount + 1, documentPacks[0].Count);
			AssertEquals("Second pack should be for org BBB", "BBB", documentPacks[1].Organisation.OH_Code);
			AssertEquals("Second pack should contain 2 documents", 2, documentPacks[1].Count);
		}

		public void TestAppendTransactionsToDocumentPack()
		{
			Creator.ABIGAS.CompanyData.OB_IsDebtor = true;
			Creator.ABIGAS.OH_Code = "AAA";
			Creator.AALSHI.CompanyData.OB_IsDebtor = true;
			Creator.AALSHI.OH_Code = "BBB";

			Invoice aUDInvoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice, Creator.ABIGAS.PK, Creator.AUD.RX_Code, "00001000", 100M, 10M);

			Invoice aUDInvoice2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice2, Creator.AALSHI.PK, Creator.AUD.RX_Code, "00001001", 100M, 10M);

			Factory.Save();

			ARCreditNote uSDCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			SetUpOutstandingTransaction(uSDCreditNote, Creator.ABIGAS.PK, Creator.USD.RX_Code, "00001002", -100M, -10M);

			ARAdjustmentNote uSDAdjustmentNote = Factory.NewWithValidTestData<ARAdjustmentNote>();
			SetUpOutstandingTransaction(uSDAdjustmentNote, Creator.AALSHI.PK, Creator.USD.RX_Code, "00001003", 100M, 10M);

			ARReceipt uSDReceipt = Factory.NewWithValidTestData<ARReceipt>();
			SetUpOutstandingTransaction(uSDReceipt, Creator.ABIGAS.PK, Creator.USD.RX_Code, "00001004", 100M, 10M);
			uSDReceipt.AH_OutstandingAmount = 110M;

			Factory.Save();

			Statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementAndInvoices;
			Statement.IssueByTransactionBranch = false;
			PrintTask task = Statement.GetPrintTaskForTest_ForTestOnly();
			List<DocumentPack> documentPacks = task.GetDocumentPacks().ToList();

			AssertEquals("There should be 2 tasks in the Print Task", 2, documentPacks.Count);
			AssertEquals("First pack should be for org AAA", "AAA", documentPacks[0].Organisation.OH_Code);
			AssertEquals("First pack should contain 4 documents", 4, documentPacks[0].Count);
			AssertEquals("Second pack should be for org BBB", "BBB", documentPacks[1].Organisation.OH_Code);
			AssertEquals("Second pack should contain 4 documents", 4, documentPacks[1].Count);

			Statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementAndInvoices;
			Statement.IssueByTransactionBranch = true;
			task = Statement.GetPrintTaskForTest_ForTestOnly();
			documentPacks = task.GetDocumentPacks().ToList();

			AssertEquals("There should be 2 tasks in the Print Task", 2, documentPacks.Count);
			AssertEquals("First pack should be for org AAA", "AAA", documentPacks[0].Organisation.OH_Code);
			AssertEquals("First pack should contain 4 documents", 4, documentPacks[0].Count);
			AssertEquals("Second pack should be for org BBB", "BBB", documentPacks[1].Organisation.OH_Code);
			AssertEquals("Second pack should contain 4 documents", 4, documentPacks[1].Count);
		}

		public void TestAppendInvoiceBatchesToDocumentPack()
		{
			Creator.ABIGAS.CompanyData.OB_IsDebtor = true;
			Creator.ABIGAS.OH_Code = "AAA";

			Invoice aUDInvoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice, Creator.ABIGAS.PK, Creator.AUD.RX_Code, "00001000", 100M, 10M);

			Invoice aUDInvoice2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice2, Creator.ABIGAS.PK, Creator.AUD.RX_Code, "00001005", 100M, 10M);

			Invoice aUDInvoice3 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice3, Creator.ABIGAS.PK, Creator.AUD.RX_Code, "00001006", 100M, 10M);

			Factory.Save();

			Statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementAndInvoices;
			Statement.IssueByTransactionBranch = true;
			PrintTask task = Statement.GetPrintTaskForTest_ForTestOnly();
			List<DocumentPack> documentPacks = task.GetDocumentPacks().ToList();

			AssertEquals("here should be 1 task in the Print Task", 1, documentPacks.Count);
			AssertEquals("Pack should be for org AAA", "AAA", documentPacks[0].Organisation.OH_Code);
			AssertEquals("Pack should contain 4 documents", 4, documentPacks[0].Count);

			InvoiceBatchHeader invoiceBatchHeader = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			invoiceBatchHeader.AH_OH = Creator.ABIGAS.PK;
			invoiceBatchHeader.AH_Desc = "Test Batch";
			invoiceBatchHeader.AH_RX_NKTransactionCurrency = "AUD";
			invoiceBatchHeader.AH_InvoiceAmount = 200M;
			invoiceBatchHeader.AH_OutstandingAmount = 200M;
			invoiceBatchHeader.AH_OSTotal = 200M;
			aUDInvoice2.AH_AH_InvoiceStatement = invoiceBatchHeader.PK;
			aUDInvoice3.AH_AH_InvoiceStatement = invoiceBatchHeader.PK;

			Factory.Save();

			Statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementAndInvoices;

			task = Statement.GetPrintTaskForTest_ForTestOnly();
			documentPacks = task.GetDocumentPacks().ToList();

			AssertEquals("There should be 1 task in the Print Task", 1, documentPacks.Count);
			AssertEquals("Pack should be for org AAA", "AAA", documentPacks[0].Organisation.OH_Code);
			AssertEquals("Pack should contain 3 documents", 3, documentPacks[0].Count);
		}

		public void TestNoCastExceptionWhenGetPrintTaskWithAccountMovement()
		{
			Creator.ABIGAS.CompanyData.OB_IsDebtor = true;
			Creator.ABIGAS.OH_Code = "AAA";

			var aUDInvoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice, Creator.ABIGAS.PK, Creator.AUD.RX_Code, "00001000", 100M, 10M);

			var aUDInvoice2 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice2, Creator.ABIGAS.PK, Creator.AUD.RX_Code, "00001005", 100M, 10M);

			var aUDInvoice3 = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1.0M);
			SetUpOutstandingTransaction(aUDInvoice3, Creator.ABIGAS.PK, Creator.AUD.RX_Code, "00001006", 100M, 10M);

			Factory.Save();

			Statement.IssueStatementPack = AccountingConstants.IssueStatementPackType.StatementAndInvoices;
			Statement.IssueByTransactionBranch = true;

			Statement.PrintAccountMovementSOA = true;
			Statement.PrintAccountMovementFromDate = ZDateTime.Today;
			Statement.PrintAccountMovementToDate = ZDateTime.Today.AddDays(2);

			var task = Statement.GetPrintTaskForTest_ForTestOnly();
			var documentPacks = task.GetDocumentPacks().ToList();

			AssertEquals("here should be 1 task in the Print Task", 1, documentPacks.Count);
			AssertEquals("Pack should be for org AAA", "AAA", documentPacks[0].Organisation.OH_Code);
			AssertEquals("Pack should contain 4 documents", 4, documentPacks[0].Count);
		}

		#endregion

		#region Test Account Fee Generation

		#region Debtor Query SQL Generation

		public void TestSelectClause_AccountFee()
		{
			Statement.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;

			SqlParameters = new ZSqlParameterCollection();
			ZSqlParameter company = ZSqlParameter.New("@CurrentCompany", Statement.Company.PK, GlbCompanySchema.PK);
			SqlParameters.Add(company);
			Assert("Paramaters contains @CurrentCompany", SqlParameters.Contains(company));
			string sqlString = Statement.GetOrgPKQueryForAccountFee(SqlParameters);
			AssertEquals("Paramaters.Count", 1, SqlParameters.Count);
			AssertEquals("Query", string.Empty, sqlString.Trim());

			Statement.OH_PK = Guid.NewGuid();
			AddAndAssertQuerySegment("OH_PK", new List<string>() { "@Organisation" }, 2, "SELECT TransactionOrg.OH_PK FROM dbo.OrgHeader TransactionOrg INNER JOIN dbo.OrgCompanyData TransactionCompanyData ON TransactionCompanyData.OB_OH = OH_PK INNER JOIN dbo.GlbCompany ON TransactionCompanyData.OB_GC = GC_PK WHERE GC_PK = @Company AND TransactionOrg.OH_PK = @Organisation");

			Statement.OJ_PK = Guid.NewGuid();
			AddAndAssertQuerySegment("OJ_PK", new List<string>() { "@ARGroup" }, 3, "SELECT TransactionOrg.OH_PK FROM dbo.OrgHeader TransactionOrg INNER JOIN dbo.OrgCompanyData TransactionCompanyData ON TransactionCompanyData.OB_OH = OH_PK INNER JOIN dbo.GlbCompany ON TransactionCompanyData.OB_GC = GC_PK WHERE GC_PK = @Company AND TransactionOrg.OH_PK = @Organisation AND TransactionCompanyData.OB_OJ_ARDebtorGroup = @ARGroup");

			Statement.GB_PK = Guid.NewGuid();
			AddAndAssertQuerySegment("GB_PK", new List<string>() { "@Branch" }, 4, "SELECT TransactionOrg.OH_PK FROM dbo.OrgHeader TransactionOrg INNER JOIN dbo.OrgCompanyData TransactionCompanyData ON TransactionCompanyData.OB_OH = OH_PK INNER JOIN dbo.GlbCompany ON TransactionCompanyData.OB_GC = GC_PK INNER JOIN dbo.GlbBranch ON OB_GB_ControllingBranch = GB_PK WHERE GC_PK = @Company AND TransactionOrg.OH_PK = @Organisation AND TransactionCompanyData.OB_OJ_ARDebtorGroup = @ARGroup AND TransactionCompanyData.OB_GB_ControllingBranch = @Branch");

			Statement.AccountsRelationShip = "ACR";
			AddAndAssertQuerySegment("OB_ARCategory", new List<string>() { "@AccountsRelationship" }, 5, "SELECT TransactionOrg.OH_PK FROM dbo.OrgHeader TransactionOrg INNER JOIN dbo.OrgCompanyData TransactionCompanyData ON TransactionCompanyData.OB_OH = OH_PK INNER JOIN dbo.GlbCompany ON TransactionCompanyData.OB_GC = GC_PK INNER JOIN dbo.GlbBranch ON OB_GB_ControllingBranch = GB_PK WHERE GC_PK = @Company AND TransactionOrg.OH_PK = @Organisation AND TransactionCompanyData.OB_OJ_ARDebtorGroup = @ARGroup AND TransactionCompanyData.OB_GB_ControllingBranch = @Branch AND TransactionCompanyData.OB_ARCategory = @AccountsRelationship");

			Statement.ConsolidationCategory = "CNS";
			AddAndAssertQuerySegment("OB_ARConsolidatedAccountingCategory", new List<string>() { "@ConsolidationCategory" }, 6, "SELECT TransactionOrg.OH_PK FROM dbo.OrgHeader TransactionOrg INNER JOIN dbo.OrgCompanyData TransactionCompanyData ON TransactionCompanyData.OB_OH = OH_PK INNER JOIN dbo.GlbCompany ON TransactionCompanyData.OB_GC = GC_PK INNER JOIN dbo.GlbBranch ON OB_GB_ControllingBranch = GB_PK WHERE GC_PK = @Company AND TransactionOrg.OH_PK = @Organisation AND TransactionCompanyData.OB_OJ_ARDebtorGroup = @ARGroup AND TransactionCompanyData.OB_GB_ControllingBranch = @Branch AND TransactionCompanyData.OB_ARCategory = @AccountsRelationship AND TransactionCompanyData.OB_ARConsolidatedAccountingCategory = @ConsolidationCategory");

			Statement.CreditRating = "CRD";
			AddAndAssertQuerySegment("OB_ARCreditRating", new List<string>() { "@CreditRating" }, 7, "SELECT TransactionOrg.OH_PK FROM dbo.OrgHeader TransactionOrg INNER JOIN dbo.OrgCompanyData TransactionCompanyData ON TransactionCompanyData.OB_OH = OH_PK INNER JOIN dbo.GlbCompany ON TransactionCompanyData.OB_GC = GC_PK INNER JOIN dbo.GlbBranch ON OB_GB_ControllingBranch = GB_PK WHERE GC_PK = @Company AND TransactionOrg.OH_PK = @Organisation AND TransactionCompanyData.OB_OJ_ARDebtorGroup = @ARGroup AND TransactionCompanyData.OB_GB_ControllingBranch = @Branch AND TransactionCompanyData.OB_ARCategory = @AccountsRelationship AND TransactionCompanyData.OB_ARConsolidatedAccountingCategory = @ConsolidationCategory AND TransactionCompanyData.OB_ARCreditRating = @CreditRating");

			Statement.SalesRep_PK = GlbStaff.CurrentUser.PK;
			AddAndAssertQuerySegment("Sales Rep", new List<string>() { "@SalesRep", "@SalesRepRole", "@SalesRepDepartment", "@SalesRepCompany" }, 11, "SELECT TransactionOrg.OH_PK FROM dbo.OrgHeader TransactionOrg INNER JOIN dbo.OrgCompanyData TransactionCompanyData ON TransactionCompanyData.OB_OH = OH_PK INNER JOIN dbo.GlbCompany ON TransactionCompanyData.OB_GC = GC_PK INNER JOIN dbo.GlbBranch ON OB_GB_ControllingBranch = GB_PK WHERE GC_PK = @Company AND TransactionOrg.OH_PK = @Organisation AND TransactionCompanyData.OB_OJ_ARDebtorGroup = @ARGroup AND TransactionCompanyData.OB_GB_ControllingBranch = @Branch AND TransactionCompanyData.OB_ARCategory = @AccountsRelationship AND TransactionCompanyData.OB_ARConsolidatedAccountingCategory = @ConsolidationCategory AND TransactionCompanyData.OB_ARCreditRating = @CreditRating AND TransactionOrg.OH_PK IN (SELECT O8_OH FROM dbo.OrgStaffAssignments WHERE O8_GS_NKPersonResponsible = @SalesRep AND O8_Role = @SalesRepRole AND O8_Department = @SalesRepDepartment AND O8_GC = @SalesRepCompany )");

			Statement.CustomerServiceRep_PK = GlbStaff.CurrentUser.PK;
			AddAndAssertQuerySegment("Customer Service Rep", new List<string>() { "@CustomerServiceRep", "@CustomerServiceRepRole", "@CustomerServiceRepDepartment", "@CustomerServiceRepCompany" }, 15, "SELECT TransactionOrg.OH_PK FROM dbo.OrgHeader TransactionOrg INNER JOIN dbo.OrgCompanyData TransactionCompanyData ON TransactionCompanyData.OB_OH = OH_PK INNER JOIN dbo.GlbCompany ON TransactionCompanyData.OB_GC = GC_PK INNER JOIN dbo.GlbBranch ON OB_GB_ControllingBranch = GB_PK WHERE GC_PK = @Company AND TransactionOrg.OH_PK = @Organisation AND TransactionCompanyData.OB_OJ_ARDebtorGroup = @ARGroup AND TransactionCompanyData.OB_GB_ControllingBranch = @Branch AND TransactionCompanyData.OB_ARCategory = @AccountsRelationship AND TransactionCompanyData.OB_ARConsolidatedAccountingCategory = @ConsolidationCategory AND TransactionCompanyData.OB_ARCreditRating = @CreditRating AND TransactionOrg.OH_PK IN (SELECT O8_OH FROM dbo.OrgStaffAssignments WHERE O8_GS_NKPersonResponsible = @SalesRep AND O8_Role = @SalesRepRole AND O8_Department = @SalesRepDepartment AND O8_GC = @SalesRepCompany )AND TransactionOrg.OH_PK IN (SELECT O8_OH FROM dbo.OrgStaffAssignments WHERE O8_GS_NKPersonResponsible = @CustomerServiceRep AND O8_Role = @CustomerServiceRepRole AND O8_Department = @CustomerServiceRepDepartment AND O8_GC = @CustomerServiceRepCompany )");

			Statement.CreditController_PK = GlbStaff.CurrentUser.PK;
			AddAndAssertQuerySegment("Credit Controller", new List<string>() { "@CreditController", "@CreditControllerRole", "@CreditControllerDepartment", "@CreditControllerCompany" }, 19, "SELECT TransactionOrg.OH_PK FROM dbo.OrgHeader TransactionOrg INNER JOIN dbo.OrgCompanyData TransactionCompanyData ON TransactionCompanyData.OB_OH = OH_PK INNER JOIN dbo.GlbCompany ON TransactionCompanyData.OB_GC = GC_PK INNER JOIN dbo.GlbBranch ON OB_GB_ControllingBranch = GB_PK WHERE GC_PK = @Company AND TransactionOrg.OH_PK = @Organisation AND TransactionCompanyData.OB_OJ_ARDebtorGroup = @ARGroup AND TransactionCompanyData.OB_GB_ControllingBranch = @Branch AND TransactionCompanyData.OB_ARCategory = @AccountsRelationship AND TransactionCompanyData.OB_ARConsolidatedAccountingCategory = @ConsolidationCategory AND TransactionCompanyData.OB_ARCreditRating = @CreditRating AND TransactionOrg.OH_PK IN (SELECT O8_OH FROM dbo.OrgStaffAssignments WHERE O8_GS_NKPersonResponsible = @SalesRep AND O8_Role = @SalesRepRole AND O8_Department = @SalesRepDepartment AND O8_GC = @SalesRepCompany )AND TransactionOrg.OH_PK IN (SELECT O8_OH FROM dbo.OrgStaffAssignments WHERE O8_GS_NKPersonResponsible = @CustomerServiceRep AND O8_Role = @CustomerServiceRepRole AND O8_Department = @CustomerServiceRepDepartment AND O8_GC = @CustomerServiceRepCompany )AND TransactionOrg.OH_PK IN (SELECT O8_OH FROM dbo.OrgStaffAssignments WHERE O8_GS_NKPersonResponsible = @CreditController AND O8_Role = @CreditControllerRole AND O8_Department = @CreditControllerDepartment AND O8_GC = @CreditControllerCompany )");
		}

		void AddAndAssertQuerySegment(string fieldName, List<string> expectedParameter, int expectedCountOfParameter, string expectedSQL)
		{
			var sqlString = Statement.GetOrgPKQueryForAccountFee(SqlParameters);
			AssertEquals("Paramaters.Count", expectedCountOfParameter, SqlParameters.Count);
			expectedParameter.ForEach(x => AssertNotNull("Paramaters contains " + x, SqlParameters[x]));
			AssertEquals("Query with " + fieldName, expectedSQL, sqlString.Trim());
		}

		#endregion

		#region Test Debtor Query

		public void TestAccountFeeSettings_OH_PK()
		{
			AssertSettingsWithEmptyDebtorParameters();

			Statement.OH_PK = org1.PK;
			var settings = Statement.accountFeeInvoiceCreator_ForTestOnly.GetAccountFeeSettingsFromDatabase();
			Assert("As there are Transactions within the selected Date Range, there should be settings", settings != null && settings.Count == 1);
			Assert("Org1 should be available", settings != null && settings.Any(x => new ZGuid(x["OrgPK"]) == org1.PK));
			Assert("Org2 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org2.PK));
			Assert("Org3 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org3.PK));
		}

		public void TestAccountFeeSettings_OJ_PK()
		{
			AssertSettingsWithEmptyDebtorParameters();

			Statement.OJ_PK = debtorGroup.PK;
			var settings = Statement.accountFeeInvoiceCreator_ForTestOnly.GetAccountFeeSettingsFromDatabase();
			Assert("As there are Transactions within the selected Date Range, there should be settings", settings != null && settings.Count == 1);
			Assert("Org1 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org1.PK));
			Assert("Org2 should be available", settings != null && settings.Any(x => new ZGuid(x["OrgPK"]) == org2.PK));
			Assert("Org3 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org3.PK));
		}

		public void TestAccountFeeSettings_GB_PK()
		{
			AssertSettingsWithEmptyDebtorParameters();

			org1.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			Statement.GB_PK = GlbBranch.CurrentBranch.PK;
			var settings = Statement.accountFeeInvoiceCreator_ForTestOnly.GetAccountFeeSettingsFromDatabase();
			Assert("As there are Transactions within the selected Date Range, there should be settings", settings != null && settings.Count == 1);
			Assert("Org1 should be available", settings != null && settings.Any(x => new ZGuid(x["OrgPK"]) == org1.PK));
			Assert("Org2 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org2.PK));
			Assert("Org3 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org3.PK));
		}

		public void TestAccountFeeSettings_AccountsRelationShip()
		{
			AssertSettingsWithEmptyDebtorParameters();

			org1.CompanyData.OB_ARCategory = "STD";
			Factory.Save();

			Statement.AccountsRelationShip = "STD";
			var settings = Statement.accountFeeInvoiceCreator_ForTestOnly.GetAccountFeeSettingsFromDatabase();
			Assert("As there are Transactions within the selected Date Range, there should be settings", settings != null && settings.Count == 1);
			Assert("Org1 should be available", settings != null && settings.Any(x => new ZGuid(x["OrgPK"]) == org1.PK));
			Assert("Org2 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org2.PK));
			Assert("Org3 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org3.PK));
		}

		public void TestAccountFeeSettings_ConsolidationCategory()
		{
			AssertSettingsWithEmptyDebtorParameters();

			org1.CompanyData.OB_ARConsolidatedAccountingCategory = "STD";
			Factory.Save();

			Statement.ConsolidationCategory = "STD";
			var settings = Statement.accountFeeInvoiceCreator_ForTestOnly.GetAccountFeeSettingsFromDatabase();
			Assert("As there are Transactions within the selected Date Range, there should be settings", settings != null && settings.Count == 1);
			Assert("Org1 should be available", settings != null && settings.Any(x => new ZGuid(x["OrgPK"]) == org1.PK));
			Assert("Org2 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org2.PK));
			Assert("Org3 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org3.PK));
		}

		public void TestAccountFeeSettings_CreditRating()
		{
			AssertSettingsWithEmptyDebtorParameters();

			org1.CompanyData.OB_ARCreditRating = OrgConstants.ARCreditRating.Code.LowRisk;
			Factory.Save();

			Statement.CreditRating = OrgConstants.ARCreditRating.Code.LowRisk;
			var settings = Statement.accountFeeInvoiceCreator_ForTestOnly.GetAccountFeeSettingsFromDatabase();
			Assert("As there are Transactions within the selected Date Range, there should be settings", settings != null && settings.Count == 1);
			Assert("Org1 should be available", settings != null && settings.Any(x => new ZGuid(x["OrgPK"]) == org1.PK));
			Assert("Org2 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org2.PK));
			Assert("Org3 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org3.PK));
		}

		public void TestAccountFeeSettings_SalesRep()
		{
			AssertSettingsWithEmptyDebtorParameters();

			Creator.CreateStaffAssignment(org1.PK, GlbCompany.CurrentCompany.PK, "ALL", StaffAssignmentRoles.Codes.SalesRep, GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			Statement.SalesRep_PK = GlbStaff.CurrentUser.PK;
			var settings = Statement.accountFeeInvoiceCreator_ForTestOnly.GetAccountFeeSettingsFromDatabase();
			Assert("As there are Transactions within the selected Date Range, there should be settings", settings != null && settings.Count == 1);
			Assert("Org1 should be available", settings != null && settings.Any(x => new ZGuid(x["OrgPK"]) == org1.PK));
			Assert("Org2 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org2.PK));
			Assert("Org3 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org3.PK));
		}

		public void TestAccountFeeSettings_CustomerServiceRep()
		{
			AssertSettingsWithEmptyDebtorParameters();

			Creator.CreateStaffAssignment(org1.PK, GlbCompany.CurrentCompany.PK, "ALL", StaffAssignmentRoles.Codes.CustomerServiceRep, GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			Statement.CustomerServiceRep_PK = GlbStaff.CurrentUser.PK;
			var settings = Statement.accountFeeInvoiceCreator_ForTestOnly.GetAccountFeeSettingsFromDatabase();
			Assert("As there are Transactions within the selected Date Range, there should be settings", settings != null && settings.Count == 1);
			Assert("Org1 should be available", settings != null && settings.Any(x => new ZGuid(x["OrgPK"]) == org1.PK));
			Assert("Org2 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org2.PK));
			Assert("Org3 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org3.PK));
		}

		public void TestAccountFeeSettings_CreditController()
		{
			AssertSettingsWithEmptyDebtorParameters();

			Creator.CreateStaffAssignment(org1.PK, GlbCompany.CurrentCompany.PK, "ALL", StaffAssignmentRoles.Codes.CreditController, GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			Statement.CreditController_PK = GlbStaff.CurrentUser.PK;
			var settings = Statement.accountFeeInvoiceCreator_ForTestOnly.GetAccountFeeSettingsFromDatabase();
			Assert("As there are Transactions within the selected Date Range, there should be settings", settings != null && settings.Count == 1);
			Assert("Org1 should be available", settings != null && settings.Any(x => new ZGuid(x["OrgPK"]) == org1.PK));
			Assert("Org2 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org2.PK));
			Assert("Org3 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org3.PK));
		}

		public void TestAccountFeeSettings_IssueBySettlementGroup()
		{
			AssertSettingsWithEmptyDebtorParameters();

			Creator.CreateARSettlementGroup(Creator.AALSHI, org1);
			Factory.Save();

			Statement.OH_PK = org1.PK;
			Statement.IssueBySettlementGroup = true;
			var settings = Statement.accountFeeInvoiceCreator_ForTestOnly.GetAccountFeeSettingsFromDatabase();
			Assert("As there are Transactions within the selected Date Range, there should be settings", settings != null && settings.Count == 1);
			Assert("Org1 should be available", settings != null && settings.Any(x => new ZGuid(x["OrgPK"]) == org1.PK));
			Assert("Org2 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org2.PK));
			Assert("Org3 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org3.PK));
		}

		public void TestAccountFeeSettings_Combined()
		{
			AssertSettingsWithEmptyDebtorParameters();

			Statement.OH_PK = org1.PK;
			org1.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			org1.CompanyData.OB_ARCategory = "STD";
			Creator.CreateStaffAssignment(org1.PK, GlbCompany.CurrentCompany.PK, "ALL", StaffAssignmentRoles.Codes.SalesRep, GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			Statement.OH_PK = org1.PK;
			Statement.AccountsRelationShip = "STD";
			Statement.GB_PK = GlbBranch.CurrentBranch.PK;
			Statement.SalesRep_PK = GlbStaff.CurrentUser.PK;
			var settings = Statement.accountFeeInvoiceCreator_ForTestOnly.GetAccountFeeSettingsFromDatabase();
			Assert("As there are Transactions within the selected Date Range, there should be settings", settings != null && settings.Count == 1);
			Assert("Org1 should be available", settings != null && settings.Any(x => new ZGuid(x["OrgPK"]) == org1.PK));
			Assert("Org2 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org2.PK));
			Assert("Org3 shouldn't be available", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org3.PK));
		}

		void AssertSettingsWithEmptyDebtorParameters()
		{
			org1 = Creator.CreateOrgHeader("ORG_" + AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, true, true);

			debtorGroup = Creator.CreateDebtorGroup();
			org2 = Creator.CreateOrgHeader("ORG2_" + AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, true, true);
			org2.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;

			org3 = Creator.CreateOrgHeader("ORG3_", true, true);
			newCurrency = Factory.New<RefCurrency>();
			newCurrency.RX_Code = "TST";
			newCurrency.RX_Desc = "TEST CURRENCY";
			newCurrency.RX_SubUnitRatio = 2;

			SetAccountFeeSettings(org1.CompanyData.PK, AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, 250m, "AUD", Creator.GLHeader1.PK, GlbCompany.CurrentCompany.PK, true, false);
			SetAccountFeeSettings(debtorGroup.PK, AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, 350m, "USD", Creator.GLHeader1.PK, GlbCompany.CurrentCompany.PK, false, true);

			var rate = Creator.USD.ExchangeRates.AddNew();
			rate.RE_ExRateType = "SEL";
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(30);
			rate.RE_GC = GlbCompany.CurrentCompany.PK;
			rate.RE_RX_NKExCurrency = "USD";
			rate.RE_SellRate = 0.75m;
			rate.RE_StartDate = ZDateTime.Today.AddDays(-7);

			Factory.Save();
			GlbCompany.CurrentCompany.Factory.Save();

			Statement.AccountFeeInvoiceCreator.AccFeeFromDate = new ZDateTime(2015, 07, 01, 0, 0, 0);
			Statement.AccountFeeInvoiceCreator.AccFeeToDate = new ZDateTime(2015, 07, 15, 0, 0, 0);

			var settings = Statement.accountFeeInvoiceCreator_ForTestOnly.GetAccountFeeSettingsFromDatabase();
			Assert("As there is no Transaction within the selected Date Range, No settings should be returned from DB", settings == null || settings.Count == 0);

			CreateINVTransaction(GlbBranch.CurrentBranch, org3, Creator.USD, "INV 001", 200M, 2.0M, new ZDateTime(2015, 07, 05, 0, 0, 0));
			CreateINVTransaction(GlbBranch.CurrentBranch, org2, Creator.USD, "INV 002", 250M, 2.0M, new ZDateTime(2015, 07, 06, 0, 0, 0));
			CreateINVTransaction(GlbBranch.CurrentBranch, org1, Creator.USD, "INV 003", 350M, 2.0M, new ZDateTime(2015, 07, 07, 0, 0, 0));
			Factory.Save();

			settings = Statement.accountFeeInvoiceCreator_ForTestOnly.GetAccountFeeSettingsFromDatabase();
			Assert("As there are Transactions within the selected Date Range, there should be settings", settings != null && settings.Count == 2);
			Assert("Org1 should be available", settings != null && settings.Any(x => new ZGuid(x["OrgPK"]) == org1.PK));
			Assert("Org2 should be available", settings != null && settings.Any(x => new ZGuid(x["OrgPK"]) == org2.PK));
			Assert("Org3 shouldn't be available as there is no settings for org 3", settings != null && !settings.Any(x => new ZGuid(x["OrgPK"]) == org3.PK));
		}

		void SetAccountFeeSettings(ZGuid parentObjectPK, ZString ruleType, ZDecimal amount, ZString currencyNK, ZGuid gLPK, ZGuid? companyPK, bool isOrgLevel, bool isDebtorLevel)
		{
			var accountFee = Factory.New<AccAccountFee>();
			accountFee.AAF_AG_GLAccount = gLPK;
			accountFee.AAF_FeeAmount = amount;
			accountFee.AAF_RX_NKFeeCurrency = currencyNK;
			accountFee.AAF_GC_Company = companyPK ?? new ZGuid(Env.CurrentCompany.PK);
			if (isOrgLevel)
			{
				accountFee.AAF_OB_CompanyData = parentObjectPK;
			}
			else if (isDebtorLevel)
			{
				accountFee.AAF_OJ_DebtorGroup = parentObjectPK;
			}
			accountFee.AAF_Rule = ruleType;

			Factory.Save();
		}

		OrgHeader org1, org2, org3;
		OrgDebtorGroup debtorGroup;
		RefCurrency newCurrency;
		#endregion

		public void TestDefaultValueOfDoAccountFeeTransaction()
		{
			var statement = Statement.New(Creator.NonCurrentCompanyBranch);
			AssertEquals("No Account Fee Settings should be available", false, AccountFeeSettings.HasAccountFeeSettings(Creator.NonCurrentCompanyBranch.Company.PK));
			AssertEquals("Do Account Fee Transaction", false, statement.DoAccountFeeTransaction);

			statement.Company.AccountFeeSettings.OverrideSettings = true;
			statement.Company.AccountFeeSettings.AAF_AG_GLAccount = Creator.GLHeader1.PK;
			statement.Company.AccountFeeSettings.AAF_FeeAmount = 250m;
			statement.Company.AccountFeeSettings.AAF_RX_NKFeeCurrency = "AUD";
			statement.Company.AccountFeeSettings.AAF_Rule = "OSB";
			Factory.Save();

			statement = Statement.New(Creator.NonCurrentCompanyBranch);
			AssertEquals("Account Fee Settings should be available", true, AccountFeeSettings.HasAccountFeeSettings(Creator.NonCurrentCompanyBranch.Company.PK));
			AssertEquals("Do Account Fee Transaction", true, statement.DoAccountFeeTransaction);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Statement = (Statement)GetNewBusinessObject();
			SQLStringBuilder = new ZStringBuilder();
			SqlParameters = new ZSqlParameterCollection();

			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Now.Year - 1, GlbCompany.CurrentCompany.PK);
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Now.Year, GlbCompany.CurrentCompany.PK);
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Now.Year + 1, GlbCompany.CurrentCompany.PK);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Statement.New(GlbBranch.CurrentBranch);
		}

		protected Statement Statement;
		protected ZStringBuilder SQLStringBuilder;
		protected ZSqlParameterCollection SqlParameters;

		protected TestObjectCreator Creator
		{
			get
			{
				if (fCreator == null)
				{
					fCreator = new TestObjectCreator(Factory);
				}

				return fCreator;
			}
		}

		TestObjectCreator fCreator;

		void CreateOrgStaffAssignment(OrgHeader organisation, GlbStaff responsiblePerson, ZString role)
		{
			OrgStaffAssignments staffAssignment = Factory.New<OrgStaffAssignments>();
			staffAssignment.O8_OH = organisation.PK;
			staffAssignment.O8_GS_NKPersonResponsible = responsiblePerson.GS_Code;
			staffAssignment.O8_Department = "ALL";
			staffAssignment.O8_Role = role;
			staffAssignment.O8_GC = GlbCompany.CurrentCompany.PK;
		}

		protected AccTransactionHeader SetUpOrgAsDebtorAndAddOutstandingTransaction(OrgHeader orgToSetup, ZDecimal invoiceAmount, ZDecimal gSTAmount)
		{
			orgToSetup.CompanyData.OB_IsDebtor = true;
			orgToSetup.CompanyData.SetARTaxApplicable(false);

			Invoice invoice = (Invoice)Creator.CreateInvoice(typeof(ARInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1.0M);
			invoice.AH_InvoiceDate = ZDateTime.Now;
			invoice.AH_PostDate = ZDateTime.Now;
			invoice.AH_DueDate = ZDateTime.Now;
			invoice.AH_OH = orgToSetup.PK;

			var line = Creator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, 1.0M, invoiceAmount);
			line.AL_LocalExTaxAmount = invoiceAmount;
			line.AL_LocalTaxAmount = gSTAmount;

			return invoice;
		}

		protected void SetUpOutstandingTransaction(AccTransactionHeader transaction, ZGuid organisationPK, ZString currencyNK, ZString transactionNumber, ZDecimal invoiceAmount, ZDecimal gSTAmount)
		{
			transaction.AH_OH = organisationPK;
			transaction.AH_RX_NKTransactionCurrency = currencyNK;
			if (transaction.AH_ExchangeRate == 0)
			{
				transaction.AH_ExchangeRate = 1m;
			}
			transaction.AH_FullyPaidDate = ZDateTime.Empty;
			transaction.AH_TransactionNum = transactionNumber;
			transaction.AH_InvoiceAmount = invoiceAmount;
			transaction.AH_GSTAmount = gSTAmount;
			transaction.AH_InvoiceDate = ZDateTime.Now;
			transaction.AH_OSTotal = invoiceAmount + gSTAmount;
			transaction.AH_PostDate = ZDateTime.Now;
			transaction.AH_DueDate = ZDateTime.Now;

			if (transaction is InvoicingBase)
			{
				Creator.CreateInvoiceLine(transaction as InvoicingBase, transaction.TransactionCurrency, transaction.AH_ExchangeRate, invoiceAmount, gSTAmount, 0m);
			}
		}

		AccTransactionHeader CreateReceiptForOrgHeader(OrgHeader organisation, ZDecimal receiptAmount)
		{
			ARReceipt receipt = Factory.NewWithValidTestData<ARReceipt>();
			receipt.AH_OH = organisation.PK;
			receipt.AH_InvoiceAmount = -receiptAmount;
			receipt.AH_OSTotal = -receiptAmount;
			return receipt;
		}

		protected AccTransactionMatchLink SetUpMatchLinkForTransaction(TransactionMatchLinkGroup matchGroup, AccTransactionHeader header, ZDecimal matchedAmount, ZDateTime matchedDate)
		{
			AccTransactionMatchLink matchLink = matchGroup.AddNew();

			matchLink.AP_AH = header.PK;
			matchLink.AP_Amount = matchedAmount;
			matchLink.AP_MatchDate = matchedDate;
			matchLink.AP_MatchGroupNum = "M000010001";

			return matchLink;
		}

		protected void BalanceMatchGroup(TransactionMatchLinkGroup matchGroup)
		{
			ZDecimal total = ZDecimal.Zero;
			foreach (TransactionMatchLink link in matchGroup)
			{
				total += link.AP_Amount;
			}
			if (total != ZDecimal.Zero)
			{
				AccTransactionHeader header = matchGroup.Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_InvoiceAmount = -total;

				TransactionMatchLink balanceLink = matchGroup.AddNew();
				balanceLink.AP_AH = header.PK;
				balanceLink.AP_Amount = -total;
				TestObjectCreator.SetupMatchLinkMatchDate(balanceLink);
			}
		}

		InvoicingBase CreateINVTransaction(GlbBranch branch, OrgHeader debtor, RefCurrency currency, ZString invoiceNumber, ZDecimal invoiceAmount, ZDecimal exchangeRate, ZDateTime postDate)
		{
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.SetARTaxApplicable(false);
			var newJob = Creator.CreateJob(debtor, 0.0M, Creator.Agent, 0.0M);
			var transaction = Creator.CreateInvoiceWithLine(typeof(ARInvoice), invoiceNumber, currency, exchangeRate, invoiceAmount * exchangeRate, 0M, invoiceAmount, 0M, debtor, Creator.FRT.PK, postDate, postDate.AddDays(-2), postDate.AddDays(-5), false);
			var jobCharge = Creator.CreateJobCharge(transaction.Lines[0], newJob, Creator.FRT, currency);

			transaction.Lines[0].AL_JH = transaction.AH_JH = newJob.PK;
			newJob.JH_GC = transaction.AH_GC = transaction.Lines[0].AL_GC = branch.GB_GC;
			newJob.JH_GB = jobCharge.JR_GB = transaction.AH_GB = transaction.Lines[0].AL_GB = branch.PK;
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_PostDate = postDate;
			transaction.AH_TransactionNum = invoiceNumber;

			return transaction;
		}

		ARCreditNote CreateCRDTransaction(GlbBranch branch, OrgHeader debtor, RefCurrency currency, ZString cRDNumber, ZDecimal invoiceAmount, ZDecimal exchangeRate, ZDateTime postDate)
		{
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.SetARTaxApplicable(false);
			var newJob = Creator.CreateJob(debtor, 0.0M, Creator.Agent, 0.0M);
			var transaction = Creator.CreateARCreditNoteWithLine(cRDNumber, debtor, Creator.USD, exchangeRate, "Credit Note Line 1", newJob, Creator.FRT, invoiceAmount, new ZDateTime(2015, 02, 12, 0, 0, 0), false);
			var jobCharge = Creator.CreateJobCharge(transaction.Lines[0], newJob, Creator.FRT, currency);

			transaction.Lines[0].AL_JH = transaction.AH_JH = newJob.PK;
			newJob.JH_GC = transaction.AH_GC = transaction.Lines[0].AL_GC = branch.GB_GC;
			newJob.JH_GB = jobCharge.JR_GB = transaction.AH_GB = transaction.Lines[0].AL_GB = branch.PK;

			transaction.AH_RX_NKTransactionCurrency = currency.RX_Code;
			transaction.AH_FullyPaidDate = ZDateTime.Empty;
			transaction.AH_TransactionNum = cRDNumber;
			transaction.AH_PostDate = postDate;

			return transaction;
		}

		ReceiptPaymentBase CreateRECTransaction(GlbBranch branch, OrgHeader debtor, RefCurrency currency, ZString recNumber, ZDecimal recAmount, ZDateTime postDate)
		{
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.SetARTaxApplicable(false);

			var transaction = Creator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, recAmount, Creator.AUDBankAccount.PK);
			transaction.AH_OH = debtor.PK;
			transaction.AH_PostDate = postDate;
			transaction.AH_TransactionNum = recNumber;
			transaction.AH_GC = branch.GB_GC;
			transaction.AH_GB = branch.PK;

			return transaction;
		}

		protected void AssertCollectionContainsHeader(OrgHeader orgToFind, DynamicBusinessObjectCollection businessObjCollection, bool expected)
		{
			bool organisationFound = false;

			foreach (BusinessObject currentBisObj in businessObjCollection)
			{
				ZGuid organisationPK = new ZGuid(currentBisObj[OrgHeaderSchema.Constants.PK]);
				ZString currencyNK = new ZString(currentBisObj[AccTransactionHeaderSchema.Constants.AH_RX_NKTransactionCurrency]);
				ZString organisationCode = (ZString)currentBisObj[OrgHeaderSchema.Constants.OH_Code];

				if (organisationPK == orgToFind.PK)
				{
					organisationFound = true;
					break;
				}
			}

			ZString stringToDisplay = "Organisation " + orgToFind.OH_Code + " is in DynamicBusinessObjectCollection";

			AssertEquals(stringToDisplay, expected, organisationFound);
		}

		protected void AssertBizOForAccountMovementSOA(DynamicBusinessObjectCollection businessObjCollection, OrgHeader orgToFind, ZString expectedCurrency, ZDecimal expectedOpeningBalance, ZBool expectedIsMultipleCurrency)
		{
			bool organisationFound = false;

			foreach (BusinessObject currentBisObj in businessObjCollection)
			{
				ZGuid organisationPK = new ZGuid(currentBisObj[OrgHeaderSchema.Constants.PK]);
				ZString currencyNK = new ZString(currentBisObj[AccTransactionHeaderSchema.Constants.AH_RX_NKTransactionCurrency]);
				ZString organisationCode = (ZString)currentBisObj[OrgHeaderSchema.Constants.OH_Code];
				ZDecimal openingBalance = (ZDecimal)currentBisObj[Statement.Constants.OpeningBalance];
				ZBool isMultipleCurrency = ((ZInt)currentBisObj[Statement.Constants.isMultipleCurrency] == 1);

				if (organisationPK == orgToFind.PK)
				{
					organisationFound = true;
					AssertEquals("CurrencyNK", currencyNK, expectedCurrency);
					AssertEquals("OrganisationCode", orgToFind.OH_Code, organisationCode);
					AssertEquals("OpeningBalance", expectedOpeningBalance, openingBalance);
					AssertEquals("IsMultipleCurrency", expectedIsMultipleCurrency, isMultipleCurrency);
					break;
				}
			}

			ZString stringToDisplay = "Organisation " + orgToFind.OH_Code + " is in DynamicBusinessObjectCollection";

			AssertEquals(stringToDisplay, true, organisationFound);
		}

		#endregion
	}
}
