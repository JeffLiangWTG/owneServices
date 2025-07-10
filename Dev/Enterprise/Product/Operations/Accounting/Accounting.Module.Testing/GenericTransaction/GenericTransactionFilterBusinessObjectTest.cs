using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GenericTransaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GenericTransactionFilterBusinessObject))]
	public class GenericTransactionFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		#region Filter Tests

		public void TestLedgerFilter()
		{
			Transaction1.AH_Ledger = LedgerTypes.AccountsPayable;
			Transaction2.AH_Ledger = LedgerTypes.AccountsReceivable;

			Factory.Save();

			DependentListFilter filter = (DependentListFilter)FilterBO["Ledger/Transaction Type"];

			filter.Property1 = LedgerTypes.AccountsPayable;
			filter.Property2 = String.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));

			filter.Property1 = LedgerTypes.AccountsReceivable;
			filter.Property2 = String.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expecting collection to contain Transaction2", CollectionContains(Transaction2.PK));

			filter.Property1 = LedgerTypes.General;
			filter.Property2 = String.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
		}

		public void TestTransactionTypeFilter()
		{
			Factory.Save();

			DependentListFilter filter = (DependentListFilter)FilterBO["Ledger/Transaction Type"];

			filter.Property1 = LedgerTypes.AccountsReceivable;
			filter.Property2 = TransactionTypes.Invoice;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expecting collection to contain Transaction2", CollectionContains(Transaction2.PK));
			Assert("Expecting collection to contain Transaction3", CollectionContains(Transaction3.PK));

			filter.Property1 = LedgerTypes.AccountsPayable;
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
			Assert("Expecting collection not to contain Transaction3", !CollectionContains(Transaction3.PK));

			filter.Property1 = ZString.Empty;
			filter.Property2 = TransactionTypes.Invoice;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expecting collection to contain Transaction2", CollectionContains(Transaction2.PK));
			Assert("Expecting collection to contain Transaction3", CollectionContains(Transaction3.PK));

			filter.Property1 = ZString.Empty;
			filter.Property2 = TransactionTypes.Payment;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
			Assert("Expecting collection not to contain Transaction3", !CollectionContains(Transaction3.PK));
		}

		public void TestSubAccountFiltersWithMultipleSubAccounts()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var orgHeader = testObjectCreator.CreateOrgHeader("TSTORG1", false, false);
			var staffGroup = testObjectCreator.CreateStaffGroup("TST");
			var glAccount = testObjectCreator.CreateGLHeader();
			testObjectCreator.CreateGLHeaderSubAccount(glAccount, OrgHeaderSchema.Constants.Prefix, true);
			testObjectCreator.CreateGLHeaderSubAccount(glAccount, AccGroupsSchema.Constants.Prefix, false);
			testObjectCreator.CreateGLHeaderSubAccount(glAccount, GlbStaffSchema.Constants.Prefix, false);
			testObjectCreator.CreateGLHeaderSubAccount(glAccount, GlbGroupSchema.Constants.Prefix, true);
			var arInvoice1 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AA1", testObjectCreator.AUD, 1M, 50M, 50M, 50M, 50M);

			AssertEquals("Precondition", 1, arInvoice1.Lines.Count);
			var arLine1 = arInvoice1.Lines[0];
			arLine1.AL_AG = glAccount.PK;
			testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(arLine1.PK, Core.Constants.SubAccountType.Organization, orgHeader.PK);
			testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(arLine1.PK, Core.Constants.SubAccountType.StaffGroup, staffGroup.PK);

			var arInvoice2 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AA2", testObjectCreator.AUD, 1M, 50M, 50M, 50M, 50M);
			AssertEquals("Precondition", 1, arInvoice2.Lines.Count);
			var arLine2 = arInvoice2.Lines[0];
			arLine2.AL_AG = glAccount.PK;
			Factory.Save();

			var glAccount2 = testObjectCreator.CreateGLHeader();
			var arInvoice3 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AA3", testObjectCreator.AUD, 1M, 50M, 50M, 50M, 50M);
			AssertEquals("Precondition", 1, arInvoice3.Lines.Count);
			var arLine3 = arInvoice3.Lines[0];
			arLine3.AL_AG = glAccount2.PK;
			Factory.Save();

			var filter = (SubAccountFilter)FilterBO["Sub Account + Value"];
			filter.SubAccountType = Core.Constants.SubAccountType.Organization;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Filter result collection count", 2, FilterCollection.Count);
			Assert("Expecting collection to contain invoice1", CollectionContains(arInvoice1.PK));
			Assert("Expecting collection to contain invoice2", CollectionContains(arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", !CollectionContains(arInvoice3.PK));

			filter = (SubAccountFilter)FilterBO["Sub Account + Value"];
			filter.SubAccountType = Core.Constants.SubAccountType.StaffGroup;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Filter result collection count", 2, FilterCollection.Count);
			Assert("Expecting collection to contain invoice1", CollectionContains(arInvoice1.PK));
			Assert("Expecting collection to contain invoice2", CollectionContains(arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", !CollectionContains(arInvoice3.PK));

			filter = (SubAccountFilter)FilterBO["Sub Account + Value"];
			filter.SubAccountType = Core.Constants.SubAccountType.Organization;
			filter.SubAccount = orgHeader.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Filter result collection count", 1, FilterCollection.Count);
			Assert("Expecting collection to contain invoice1", CollectionContains(arInvoice1.PK));
			Assert("Expecting collection not to contain invoice2", !CollectionContains(arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", !CollectionContains(arInvoice3.PK));

			filter = (SubAccountFilter)FilterBO["Sub Account + Value"];
			filter.SubAccountType = Core.Constants.SubAccountType.StaffGroup;
			filter.SubAccount = staffGroup.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Filter result collection count", 1, FilterCollection.Count);
			Assert("Expecting collection to contain invoice1", CollectionContains(arInvoice1.PK));
			Assert("Expecting collection not to contain invoice2", !CollectionContains(arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", !CollectionContains(arInvoice3.PK));

			filter = (SubAccountFilter)FilterBO["Sub Account + Value"];
			filter.SubAccountType = string.Empty;
			filter.SubAccount = orgHeader.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Filter result collection count", 1, FilterCollection.Count);
			Assert("Expecting collection to contain invoice1", CollectionContains(arInvoice1.PK));
			Assert("Expecting collection not to contain invoice2", !CollectionContains(arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", !CollectionContains(arInvoice3.PK));

			filter = (SubAccountFilter)FilterBO["Sub Account + Value"];
			filter.SubAccountType = string.Empty;
			filter.SubAccount = staffGroup.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Filter result collection count", 1, FilterCollection.Count);
			Assert("Expecting collection to contain invoice1", CollectionContains(arInvoice1.PK));
			Assert("Expecting collection not to contain invoice2", !CollectionContains(arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", !CollectionContains(arInvoice3.PK));

			filter.IsActive = false;

			var subAccountNoValueFilter = (ModuleTextFilter)FilterBO["Sub Account + No Value"];
			subAccountNoValueFilter.Property = Core.Constants.SubAccountType.Organization;
			subAccountNoValueFilter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Filter result collection count", 1, FilterCollection.Count);
			Assert("Expecting collection not to contain invoice1", !CollectionContains(arInvoice1.PK));
			Assert("Expecting collection to contain invoice2", CollectionContains(arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", !CollectionContains(arInvoice3.PK));

			subAccountNoValueFilter = (ModuleTextFilter)FilterBO["Sub Account + No Value"];
			subAccountNoValueFilter.Property = Core.Constants.SubAccountType.SalesGroup;
			subAccountNoValueFilter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Filter result collection count", 2, FilterCollection.Count);
			Assert("Expecting collection to contain invoice1", CollectionContains(arInvoice1.PK));
			Assert("Expecting collection to contain invoice2", CollectionContains(arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", !CollectionContains(arInvoice3.PK));

			subAccountNoValueFilter = (ModuleTextFilter)FilterBO["Sub Account + No Value"];
			subAccountNoValueFilter.Property = Core.Constants.SubAccountType.StaffAndResources;
			subAccountNoValueFilter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Filter result collection count", 2, FilterCollection.Count);
			Assert("Expecting collection to contain invoice1", CollectionContains(arInvoice1.PK));
			Assert("Expecting collection to contain invoice2", CollectionContains(arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", !CollectionContains(arInvoice3.PK));

			subAccountNoValueFilter = (ModuleTextFilter)FilterBO["Sub Account + No Value"];
			subAccountNoValueFilter.Property = Core.Constants.SubAccountType.StaffGroup;
			subAccountNoValueFilter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Filter result collection count", 1, FilterCollection.Count);
			Assert("Expecting collection not to contain invoice1", !CollectionContains(arInvoice1.PK));
			Assert("Expecting collection to contain invoice2", CollectionContains(arInvoice2.PK));
			Assert("Expecting collection not to contain invoice3", !CollectionContains(arInvoice3.PK));
		}

		public void TestAccountingPeriodFilter()
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			ZDateTime startDateOfFinancialYear = new ZDateTime(ZDateTime.Now.Year, 1, 1);
			PeriodManager periodManager = TestObjectCreator.CreateTestPeriods(startDateOfFinancialYear);

			Assert("Expecting collection to contain twelve items", periodManager.Periods.Count == 12);

			Transaction1.AH_PostDate = new ZDateTime(startDateOfFinancialYear.Year, 1, 1);
			Transaction2.AH_PostDate = new ZDateTime(startDateOfFinancialYear.Year, 2, 2);
			Transaction3.AH_PostDate = new ZDateTime(startDateOfFinancialYear.Year, 3, 3);

			Factory.Save();

			ModuleTextRangeFilter filter = (ModuleTextRangeFilter)FilterBO["Accounting Period"];

			filter.Property1 = String.Empty;
			filter.Property2 = String.Format("{0}01", startDateOfFinancialYear.Year);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
			Assert("Expecting collection not to contain Transaction3", !CollectionContains(Transaction3.PK));

			filter.Property1 = String.Format("{0}03", startDateOfFinancialYear.Year);
			filter.Property2 = String.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
			Assert("Expecting collection to contain Transaction3", CollectionContains(Transaction3.PK));

			filter.Property1 = String.Format("{0}02", startDateOfFinancialYear.Year);
			filter.Property2 = String.Format("{0}02", startDateOfFinancialYear.Year);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expecting collection to contain Transaction2", CollectionContains(Transaction2.PK));
			Assert("Expecting collection not to contain Transaction3", !CollectionContains(Transaction3.PK));

			filter.Property1 = String.Format("{0}04", startDateOfFinancialYear.Year);
			filter.Property2 = String.Format("{0}05", startDateOfFinancialYear.Year);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
			Assert("Expecting collection not to contain Transaction3", !CollectionContains(Transaction3.PK));
		}

		public void TestAccountingPeriodFilterContainPostDate()
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			ZDateTime startDateOfFinancialYear = new ZDateTime(ZDateTime.Now.Year, 1, 1);
			PeriodManager periodManager = TestObjectCreator.CreateTestPeriods(startDateOfFinancialYear);

			Factory.Save();

			Assert("Expecting collection to contain twelve items", periodManager.Periods.Count == 12);

			ModuleTextRangeFilter filter = (ModuleTextRangeFilter)FilterBO["Accounting Period"];

			filter.Property1 = String.Format("{0}01", startDateOfFinancialYear.Year);
			filter.Property2 = String.Format("{0}02", startDateOfFinancialYear.Year);
			filter.IsActive = true;

			Assert(FilterBO.Filter.HasParameters);
			AssertContains("VT_PostDate", FilterBO.Filter.GetAsWhereClause(false));
			AssertNotContains("VT_Period", FilterBO.Filter.GetAsWhereClause(false));
		}

		public void TestAccountingPeriodValidation()
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			ZDateTime startDateOfFinancialYear = new ZDateTime(ZDateTime.Now.Year, 1, 1);
			PeriodManager periodManager = TestObjectCreator.CreateTestPeriods(startDateOfFinancialYear);

			Factory.Save();

			Assert("Expecting collection to contain twelve items", periodManager.Periods.Count == 12);

			ModuleTextRangeFilter filter = (ModuleTextRangeFilter)FilterBO["Accounting Period"];

			filter.Property1 = String.Empty;
			filter.Property2 = String.Format("{0}13", startDateOfFinancialYear.Year);
			filter.IsActive = true;

			filter.Validation.ValidateAll();

			AssertNoErrors(filter.Property1Info);
			AssertHasErrors(filter.Property2Info);

			filter.Property1 = String.Format("{0}01", startDateOfFinancialYear.Year);
			filter.Property2 = String.Format("{0}02", startDateOfFinancialYear.Year);
			filter.IsActive = true;

			filter.Validation.ValidateAll();

			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);
		}

		public void TestOrganisationFilter()
		{
			OrgHeader organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation3 = Factory.NewWithValidTestData<OrgHeader>();

			Transaction1.AH_OH = organisation1.PK;
			Transaction2.AH_OH = organisation2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Organisation"];

			AssertEquals("OrganisationFilter should be defaulted to Exact", SQLComparisonOperator.Equal, filter.SqlComparisonOperator);
			Assert("OrganisationFilter SqlComparisonOperator should be ReadOnly", (filter.ComparisonOperatorInfo.ReadOnly));

			filter.Property = organisation1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));

			filter.Property = organisation3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
		}

		public void TestPostingDateFilter()
		{
			Transaction1.AH_PostDate = new ZDateTime(2000, 1, 1, 11, 0, 0);   // 2 Jan 11:00
			Transaction2.AH_PostDate = new ZDateTime(2000, 2, 2, 22, 0, 0);   // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[AccountingConstants.DateFilterTypes.PostingDate];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expecting collection to contain Transaction2", CollectionContains(Transaction2.PK));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expecting collection to contain Transaction2", CollectionContains(Transaction2.PK));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
		}

		public void TestDocumentDateFilter()
		{
			Transaction1.AH_InvoiceDate = new ZDateTime(2000, 1, 1, 11, 0, 0);    // 2 Jan 11:00
			Transaction2.AH_InvoiceDate = new ZDateTime(2000, 2, 2, 22, 0, 0);    // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[AccountingConstants.DateFilterTypes.DocumentDate];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expecting collection to contain Transaction2", CollectionContains(Transaction2.PK));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expecting collection to contain Transaction2", CollectionContains(Transaction2.PK));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
		}

		[TestDate(2000, 1, 1, 11, 0, 0)]
		public void TestPostDateFilterForToday()
		{
			Transaction1.AH_PostDate = ZDateTime.Now;   // 2 Jan 11:00
			Transaction2.AH_PostDate = new ZDateTime(2000, 2, 2, 22, 0, 0);   // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[AccountingConstants.DateFilterTypes.PostingDate];

			filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
		}

		[TestDate(2000, 1, 1, 11, 0, 0)]
		public void TestDocumentDateFilterForToday()
		{
			Transaction1.AH_InvoiceDate = ZDateTime.Now;    // 2 Jan 11:00
			Transaction2.AH_InvoiceDate = new ZDateTime(2000, 2, 2, 22, 0, 0);    // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[AccountingConstants.DateFilterTypes.DocumentDate];

			filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
		}

		public void TestExTaxAmountFilter()
		{
			TestObjectCreator.CreateInvoiceLine(Transaction1, Transaction1.TransactionCurrency, Transaction1.AH_ExchangeRate, -10m, 0m, 0m);
			TestObjectCreator.CreateInvoiceLine(Transaction2, Transaction2.TransactionCurrency, Transaction2.AH_ExchangeRate, 100m, 0m, 0m);

			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterBO[AccountingConstants.AmountFilterTypes.ExTaxAmount];

			filter.Property1 = 0.0;
			filter.Property2 = 10.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expected collection not to contain Transaction2", !CollectionContains(Transaction2.PK));

			filter.Property1 = 10.0;
			filter.Property2 = 100.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expected collection to contain Transaction2", CollectionContains(Transaction2.PK));

			filter.Property1 = 20.0;
			filter.Property2 = 90.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expected collection not to contain Transaction2", !CollectionContains(Transaction2.PK));

			filter.Property1 = 200.0;
			filter.Property2 = 300.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expected collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
		}

		public void TestTaxAmountFilter()
		{
			TestObjectCreator.CreateInvoiceLine(Transaction1, Transaction1.TransactionCurrency, Transaction1.AH_ExchangeRate, 0m, -10m, 0m);
			TestObjectCreator.CreateInvoiceLine(Transaction2, Transaction2.TransactionCurrency, Transaction2.AH_ExchangeRate, 0m, 100m, 0m);

			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterBO[AccountingConstants.AmountFilterTypes.TaxAmount];

			filter.Property1 = 0.0;
			filter.Property2 = 10.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expected collection not to contain Transaction2", !CollectionContains(Transaction2.PK));

			filter.Property1 = 10.0;
			filter.Property2 = 100.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expected collection to contain Transaction2", CollectionContains(Transaction2.PK));

			filter.Property1 = 20.0;
			filter.Property2 = 90.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expected collection not to contain Transaction2", !CollectionContains(Transaction2.PK));

			filter.Property1 = 200.0;
			filter.Property2 = 300.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expected collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
		}

		public void TestTotalAmountFilter()
		{
			TestObjectCreator.CreateInvoiceLine(Transaction1, Transaction1.TransactionCurrency, Transaction1.AH_ExchangeRate, -5m, -5m, 0m);
			TestObjectCreator.CreateInvoiceLine(Transaction2, Transaction2.TransactionCurrency, Transaction2.AH_ExchangeRate, 50m, 50m, 0m);

			Factory.Save();

			ModuleNumberRangeFilter filter = (ModuleNumberRangeFilter)FilterBO[AccountingConstants.AmountFilterTypes.TotalAmount];

			filter.Property1 = 0.0;
			filter.Property2 = 10.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expected collection not to contain Transaction2", !CollectionContains(Transaction2.PK));

			filter.Property1 = 10.0;
			filter.Property2 = 100.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expected collection to contain Transaction2", CollectionContains(Transaction2.PK));

			filter.Property1 = 20.0;
			filter.Property2 = 90.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expected collection not to contain Transaction2", !CollectionContains(Transaction2.PK));

			filter.Property1 = 200.0;
			filter.Property2 = 300.0;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expected collection not to contain Transaction1", !CollectionContains(Transaction1.PK));
			Assert("Expected collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
		}

		public void TestGLAccountFilter()
		{
			Account1.AG_AccountNum = "1111.11.11";
			Account2.AG_AccountNum = "2222.22.22";

			Factory.Save();

			AccGLHeaderRangeFilter filter = (AccGLHeaderRangeFilter)FilterBO["GL Account"];

			filter.Property1 = Account1.AG_AccountNum;
			filter.Property2 = Account1.AG_AccountNum;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
		}

		public void TestGLAccountFilterForLinesWithChargeCode()
		{
			Account1.AG_AccountNum = "1111.11.11";
			Account2.AG_AccountNum = "2222.22.22";

			AssertEquals("Precondition: InvoiceLine1.AL_AG must be set to Account1.", Account1.PK, InvoiceLine1.AL_AG);
			InvoiceLine1.AL_AC = TestObjectCreator.CC1.PK;
			AssertNotEquals("Precondition: InvoiceLine1.AL_AG must be changed to GLAcount from charge code.", Account1.PK, InvoiceLine1.AL_AG);
			TestObjectCreator.AttachJobToAPLine(InvoiceLine1);
			InvoiceLine1.AL_AG = Account1.PK;
			AssertEquals("Precondition: InvoiceLine1.AL_AC must be still set to CC1 charge code.", TestObjectCreator.CC1.PK, InvoiceLine1.AL_AC);
			TestObjectCreator.AttachChargeToAPLine(InvoiceLine1);
			Factory.Save();

			AccGLHeaderRangeFilter filter = (AccGLHeaderRangeFilter)FilterBO["GL Account"];

			filter.Property1 = Account1.AG_AccountNum;
			filter.Property2 = Account1.AG_AccountNum;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Transaction1", CollectionContains(Transaction1.PK));
			Assert("Expecting collection not to contain Transaction2", !CollectionContains(Transaction2.PK));
		}

		public void TestGLAccountFilterForLinesWithChargeCode_CFXJournals()
		{
			JCJournalHeader cfxJournal1 = Factory.NewWithValidTestData<JCJournalHeader>();
			JCJournalHeader cfxJournal2 = Factory.NewWithValidTestData<JCJournalHeader>();

			JCJournalLine line1 = cfxJournal1.Lines.AddNew();
			line1.FillWithValidTestData();
			JCJournalLine line2 = cfxJournal2.Lines.AddNew();
			line2.FillWithValidTestData();

			Account1 = Factory.NewWithValidTestData<AccGLHeader>();
			Account2 = Factory.NewWithValidTestData<AccGLHeader>();

			line1.AL_AG = Account1.PK;
			line2.AL_AG = Account2.PK;

			Account1.AG_AccountNum = "1111.11.11";
			Account2.AG_AccountNum = "2222.22.22";

			AssertEquals("Precondition: line1.AL_AG must be set to Account1.", Account1.PK, line1.AL_AG);
			line1.AL_AC = TestObjectCreator.CC1.PK;
			AssertNotEquals("Precondition: line1.AL_AG must be changed to GLAcount from charge code.", Account1.PK, line1.AL_AG);
			line1.AL_AG = Account1.PK;
			AssertEquals("Precondition: line1.AL_AC must be still set to CC1 charge code.", TestObjectCreator.CC1.PK, line1.AL_AC);

			Factory.Save();

			AccGLHeaderRangeFilter filter = (AccGLHeaderRangeFilter)FilterBO["GL Account"];

			filter.Property1 = Account1.AG_AccountNum;
			filter.Property2 = Account1.AG_AccountNum;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain cfxJournal1", CollectionContains(cfxJournal1.PK));
			Assert("Expecting collection not to contain cfxJournal2", !CollectionContains(cfxJournal2.PK));
		}

		public void TestGLAccountFilterForCFXJournalsWithCFXAccountFallback()
		{
			JCJournalHeader cfxJournal1 = Factory.NewWithValidTestData<JCJournalHeader>();
			JCJournalHeader cfxJournal2 = Factory.NewWithValidTestData<JCJournalHeader>();

			JCJournalLine line1 = cfxJournal1.Lines.AddNew();
			line1.FillWithValidTestData();
			JCJournalLine line2 = cfxJournal2.Lines.AddNew();
			line2.FillWithValidTestData();
			line2.AL_GE = TestObjectCreator.NonCurrentDepartment.PK;

			Account1 = Factory.NewWithValidTestData<AccGLHeader>();
			Account2 = Factory.NewWithValidTestData<AccGLHeader>();
			Account3 = Factory.NewWithValidTestData<AccGLHeader>();

			Account1.AG_AccountNum = "1111.11.11";
			Account2.AG_AccountNum = "2222.22.22";
			Account3.AG_AccountNum = "3333.33.33";

			line1.AL_AG = Account3.PK;
			line2.AL_AG = Account3.PK;

			Factory.Save();

			AccGLHeaderRangeFilter filter = (AccGLHeaderRangeFilter)FilterBO["GL Account"];

			filter.Property1 = Account1.AG_AccountNum;
			filter.Property2 = Account2.AG_AccountNum;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain cfxJournal1", !CollectionContains(cfxJournal1.PK));
			Assert("Expecting collection not to contain cfxJournal2", !CollectionContains(cfxJournal2.PK));

			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Account1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Env.CurrentCompany.PK, Guid.Empty, TestObjectCreator.NonCurrentDepartment.PK.ToGuid(), Account2.PK.ToGuid());
			//Factory.Save();
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain cfxJournal1", CollectionContains(cfxJournal1.PK));
			Assert("Expecting collection to contain cfxJournal2", CollectionContains(cfxJournal2.PK));

			filter.Property1 = Account1.AG_AccountNum;
			filter.Property2 = Account1.AG_AccountNum;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain cfxJournal1", CollectionContains(cfxJournal1.PK));
			Assert("Expecting collection not to contain cfxJournal2", !CollectionContains(cfxJournal2.PK));

			filter.Property1 = Account2.AG_AccountNum;
			filter.Property2 = Account2.AG_AccountNum;
			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain cfxJournal1", !CollectionContains(cfxJournal1.PK));
			Assert("Expecting collection to contain cfxJournal2", CollectionContains(cfxJournal2.PK));
		}

		public void TestOverpaymentGLAccount()
		{
			Account1.AG_AccountNum = "1111.11.11";
			Account2.AG_AccountNum = "2222.22.22";
			Account3.AG_AccountNum = "3333.33.33";

			AccountingConfigurationRegistry.Instance.OverpaymentsAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Account1.PK.ToGuid());
			APOverpayment apOverpayment = Factory.NewWithValidTestData<APOverpayment>();

			AccountingConfigurationRegistry.Instance.OverpaymentsAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Account2.PK.ToGuid());
			AROverpayment arOverpayment = Factory.NewWithValidTestData<AROverpayment>();

			AccountingConfigurationRegistry.Instance.OverpaymentsAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Account3.PK.ToGuid());
			Factory.Save();

			AccGLHeaderRangeFilter filter = (AccGLHeaderRangeFilter)FilterBO["GL Account"];

			filter.Property1 = Account1.AG_AccountNum;
			filter.Property2 = Account1.AG_AccountNum;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain APOverpayment", CollectionContains(apOverpayment.PK));
			Assert("Expecting collection not to contain AROverpayment", !CollectionContains(arOverpayment.PK));

			filter.Property1 = Account2.AG_AccountNum;
			filter.Property2 = Account2.AG_AccountNum;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain APOverpayment", !CollectionContains(apOverpayment.PK));
			Assert("Expecting collection to contain AROverpayment", CollectionContains(arOverpayment.PK));

			filter.Property1 = Account3.AG_AccountNum;
			filter.Property2 = Account3.AG_AccountNum;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain APOverpayment", !CollectionContains(apOverpayment.PK));
			Assert("Expecting collection not to contain AROverpayment", !CollectionContains(arOverpayment.PK));
		}

		public void TestExchangeDifferenceGLAccount()
		{
			Account1.AG_AccountNum = "1111.11.11";
			Account2.AG_AccountNum = "2222.22.22";
			Account3.AG_AccountNum = "3333.33.33";

			APExchangeDifference apExchangeDifference = Factory.NewWithValidTestData<APExchangeDifference>();
			apExchangeDifference.AH_AG = Account1.PK;

			ARExchangeDifference arExchangeDifference = Factory.NewWithValidTestData<ARExchangeDifference>();
			arExchangeDifference.AH_AG = Account2.PK;

			Factory.Save();

			AccGLHeaderRangeFilter filter = (AccGLHeaderRangeFilter)FilterBO["GL Account"];

			filter.Property1 = Account1.AG_AccountNum;
			filter.Property2 = Account1.AG_AccountNum;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain APExchangeDifference", CollectionContains(apExchangeDifference.PK));
			Assert("Expecting collection not to contain ARExchangeDifference", !CollectionContains(arExchangeDifference.PK));

			filter.Property1 = Account2.AG_AccountNum;
			filter.Property2 = Account2.AG_AccountNum;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain APExchangeDifference", !CollectionContains(apExchangeDifference.PK));
			Assert("Expecting collection to contain ARExchangeDifference", CollectionContains(arExchangeDifference.PK));

			filter.Property1 = Account3.AG_AccountNum;
			filter.Property2 = Account3.AG_AccountNum;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain APExchangeDifference", !CollectionContains(apExchangeDifference.PK));
			Assert("Expecting collection not to contain ARExchangeDifference", !CollectionContains(arExchangeDifference.PK));
		}

		public void TestDiscountGLAccount()
		{
			Account1.AG_AccountNum = "1111.11.11";
			Account2.AG_AccountNum = "2222.22.22";
			Account3.AG_AccountNum = "3333.33.33";

			using (AccountingConfigurationRegistry.Instance.APDiscountAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Account1.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.ARDiscountAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Account2.PK.ToGuid()))
			{
				APDiscount apDiscount = Factory.NewWithValidTestData<APDiscount>();
				ARDiscount arDiscount = Factory.NewWithValidTestData<ARDiscount>();

				Factory.Save();

				AccGLHeaderRangeFilter filter = (AccGLHeaderRangeFilter)FilterBO["GL Account"];

				filter.Property1 = Account1.AG_AccountNum;
				filter.Property2 = Account1.AG_AccountNum;
				filter.IsActive = true;

				FilterCollection.Load(FilterBO.Filter);
				Assert("Expecting collection to contain APDiscount", CollectionContains(apDiscount.PK));
				Assert("Expecting collection not to contain ARDiscount", !CollectionContains(arDiscount.PK));

				filter.Property1 = Account2.AG_AccountNum;
				filter.Property2 = Account2.AG_AccountNum;
				filter.IsActive = true;

				FilterCollection.Load(FilterBO.Filter);
				Assert("Expecting collection not to contain APDiscount", !CollectionContains(apDiscount.PK));
				Assert("Expecting collection to contain ARDiscount", CollectionContains(arDiscount.PK));

				filter.Property1 = Account3.AG_AccountNum;
				filter.Property2 = Account3.AG_AccountNum;
				filter.IsActive = true;

				FilterCollection.Load(FilterBO.Filter);
				Assert("Expecting collection not to contain APDiscount", !CollectionContains(apDiscount.PK));
				Assert("Expecting collection not to contain ARDiscount", !CollectionContains(arDiscount.PK));
			}
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GenericTransactionFilterBusinessObject();
		}

		APInvoice Transaction1;
		ARInvoice Transaction2;
		ARInvoice Transaction3;

		APInvoiceLine InvoiceLine1;
		ARInvoiceLine InvoiceLine2;
		ARInvoiceLine InvoiceLine3;

		AccGLHeader Account1;
		AccGLHeader Account2;
		AccGLHeader Account3;

		GenericTransactionCollection FilterCollection;
		GenericTransactionFilterBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			Transaction1 = Factory.NewWithValidTestData<APInvoice>();
			Transaction2 = Factory.NewWithValidTestData<ARInvoice>();
			Transaction3 = Factory.NewWithValidTestData<ARInvoice>();

			InvoiceLine1 = Factory.NewWithValidTestData<APInvoiceLine>();
			InvoiceLine2 = Factory.NewWithValidTestData<ARInvoiceLine>();
			InvoiceLine3 = Factory.NewWithValidTestData<ARInvoiceLine>();

			Account1 = Factory.NewWithValidTestData<AccGLHeader>();
			Account2 = Factory.NewWithValidTestData<AccGLHeader>();
			Account3 = Factory.NewWithValidTestData<AccGLHeader>();

			InvoiceLine1.AL_AG = Account1.PK;
			InvoiceLine2.AL_AG = Account2.PK;
			InvoiceLine3.AL_AG = Account3.PK;

			InvoiceLine1.AL_AH = Transaction1.PK;
			InvoiceLine2.AL_AH = Transaction2.PK;
			InvoiceLine3.AL_AH = Transaction3.PK;

			Transaction1.AH_AG = Account1.PK;
			Transaction2.AH_AG = Account2.PK;
			Transaction3.AH_AG = Account3.PK;

			FilterCollection = new GenericTransactionCollection(Factory);
			FilterBO = (GenericTransactionFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		bool CollectionContains(ZGuid itemPK)
		{
			foreach (GenericTransaction transaction in FilterCollection)
			{
				if (transaction.VT_FK == itemPK)
				{
					return true;
				}
			}

			return false;
		}

		#endregion
	}
}
