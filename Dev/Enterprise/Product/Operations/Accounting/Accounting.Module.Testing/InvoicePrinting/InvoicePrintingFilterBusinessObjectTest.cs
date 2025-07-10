using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ARInvoice = Enterprise.Accounting.Business.ARAP.Invoicing.ARInvoice;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(InvoicePrintingFilterBusinessObject))]
	public class InvoicePrintingFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestTransactionTypeFilter()
		{
			ARInvoice testInvNonEmpty = SetUpClassAInvoiceTest();
			testInvNonEmpty.AH_TransactionType = "INV";

			((ModuleTextFilter)InvFilterBO["Transaction Type"]).Property = "INV";
			((ModuleTextFilter)InvFilterBO["Transaction Type"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", "INV", testTransactions[0].AH_TransactionType);

			((ModuleTextFilter)InvFilterBO["Transaction Type"]).Property = "ADJ";
			((ModuleTextFilter)InvFilterBO["Transaction Type"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 0 invoices", 0, testTransactions.Count);
		}

		public void TestLocalJobReferenceFilter()
		{
			ARInvoice testInvNonEmpty = SetUpClassAInvoiceTest();

			((ModuleNumberFilter)InvFilterBO["Job Local Reference"]).Property = TestJob.JH_JobLocalReference;
			((ModuleNumberFilter)InvFilterBO["Job Local Reference"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);

			((ModuleNumberFilter)InvFilterBO["Job Local Reference"]).Property = "222222";
			((ModuleNumberFilter)InvFilterBO["Job Local Reference"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 0 invoices", 0, testTransactions.Count);

			((ModuleNumberFilter)InvFilterBO["Job Local Reference"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);
		}

		public void TestJobNumberFilter()
		{
			ARInvoice testInvNonEmpty = SetUpClassAInvoiceTest();

			((ModuleTextFilter)InvFilterBO["Job #"]).Property = "111122";
			((ModuleTextFilter)InvFilterBO["Job #"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);

			((ModuleTextFilter)InvFilterBO["Job #"]).Property = "222222";
			((ModuleTextFilter)InvFilterBO["Job #"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 0 invoices", 0, testTransactions.Count);

			((ModuleTextFilter)InvFilterBO["Job #"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);
		}

		public void TestTransactionNumberFilter()
		{
			ARInvoice testInvNonEmpty = SetUpClassAInvoiceTest(transactionNum: "0004500");

			((ModuleTextFilter)InvFilterBO["Transaction #"]).Property = "0004500";
			((ModuleTextFilter)InvFilterBO["Transaction #"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);

			((ModuleTextFilter)InvFilterBO["Transaction #"]).Property = "0009900";
			((ModuleTextFilter)InvFilterBO["Transaction #"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 0 invoices", 0, testTransactions.Count);

			((ModuleTextFilter)InvFilterBO["Transaction #"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);
		}

		public void TestConsolidationNumberFilter()
		{
			ARInvoice testInvNonEmpty = SetUpClassAInvoiceTest();

			((ModuleTextFilter)InvFilterBO["Job Invoice #"]).Property = "123321";
			((ModuleTextFilter)InvFilterBO["Job Invoice #"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);

			((ModuleTextFilter)InvFilterBO["Job Invoice #"]).Property = "999999";
			((ModuleTextFilter)InvFilterBO["Job Invoice #"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 0 invoices", 0, testTransactions.Count);

			((ModuleTextFilter)InvFilterBO["Job Invoice #"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);
		}

		public void TestGovTaxInvoiceNumberFilter()
		{
			ARInvoice testInvNonEmpty = SetUpClassAInvoiceTest();

			((ModuleTextFilter)InvFilterBO["Compliance #"]).Property = "00001990";
			((ModuleTextFilter)InvFilterBO["Compliance #"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);

			((ModuleTextFilter)InvFilterBO["Compliance #"]).Property = "99999999";
			((ModuleTextFilter)InvFilterBO["Compliance #"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 0 invoices", 0, testTransactions.Count);

			((ModuleTextFilter)InvFilterBO["Compliance #"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);
		}

		public void TestDebtorFilter()
		{
			ARInvoice testInvNonEmpty = SetUpClassAInvoiceTest();

			((ModuleGuidFilter)InvFilterBO["Debtor"]).Property = TestOrg.PK;
			((ModuleGuidFilter)InvFilterBO["Debtor"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);

			((ModuleGuidFilter)InvFilterBO["Debtor"]).Property = ZGuid.NewZGuid();
			((ModuleGuidFilter)InvFilterBO["Debtor"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 0 invoices", 0, testTransactions.Count);

			((ModuleGuidFilter)InvFilterBO["Debtor"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);
		}

		public void TestCurrencyFilter()
		{
			ARInvoice testInvNonEmpty = SetUpClassAInvoiceTest();

			((ModuleNkFilter)InvFilterBO["Currency"]).Property = TestCurrency.RX_Code;
			((ModuleNkFilter)InvFilterBO["Currency"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);

			((ModuleNkFilter)InvFilterBO["Currency"]).Property = "XXX";
			((ModuleNkFilter)InvFilterBO["Currency"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 0 invoices", 0, testTransactions.Count);

			((ModuleNkFilter)InvFilterBO["Currency"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);
		}

		public void TestBranchFilter()
		{
			ARInvoice testInvNonEmpty = SetUpClassAInvoiceTest();

			((ModuleGuidFilter)InvFilterBO["Branch"]).Property = TestBranch.PK;
			((ModuleGuidFilter)InvFilterBO["Branch"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);

			((ModuleGuidFilter)InvFilterBO["Branch"]).Property = ZGuid.NewZGuid();
			((ModuleGuidFilter)InvFilterBO["Branch"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 0 invoices", 0, testTransactions.Count);

			((ModuleGuidFilter)InvFilterBO["Branch"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);
		}

		public void TestDepartmentFilter()
		{
			ARInvoice testInvNonEmpty = SetUpClassAInvoiceTest();

			((ModuleGuidFilter)InvFilterBO["Department"]).Property = TestDepartment.PK;
			((ModuleGuidFilter)InvFilterBO["Department"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);

			((ModuleGuidFilter)InvFilterBO["Department"]).Property = ZGuid.NewZGuid();
			((ModuleGuidFilter)InvFilterBO["Department"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 0 invoices", 0, testTransactions.Count);

			((ModuleGuidFilter)InvFilterBO["Department"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);
		}

		public void TestDebtorGroupFilter()
		{
			ARInvoice testInvNonEmpty = SetUpClassAInvoiceTest();

			((ModuleGuidFilter)InvFilterBO["Debtor Group"]).Property = testDebtorGroup.PK;
			((ModuleGuidFilter)InvFilterBO["Debtor Group"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);

			((ModuleGuidFilter)InvFilterBO["Debtor Group"]).Property = ZGuid.NewZGuid();
			((ModuleGuidFilter)InvFilterBO["Debtor Group"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 0 invoices", 0, testTransactions.Count);

			((ModuleGuidFilter)InvFilterBO["Debtor Group"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", testInvNonEmpty.PK, testTransactions[0].PK);
		}

		public void TestDebtorGroupFilter_WithOtherCompanyTransactions()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			GlbCompany nonCurrentCompany = creator.NonCurrentCompany;
			OrgHeader orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgDebtorGroup orgDebtorGroup1 = Factory.NewWithValidTestData<OrgDebtorGroup>();
			OrgDebtorGroup orgDebtorGroup2 = Factory.NewWithValidTestData<OrgDebtorGroup>();
			OrgDebtorGroup orgDebtorGroup3 = Factory.NewWithValidTestData<OrgDebtorGroup>();
			orgHeader1.CompanyData.OB_OJ_ARDebtorGroup = orgDebtorGroup1.PK;
			orgHeader2.CompanyData.OB_OJ_ARDebtorGroup = orgDebtorGroup2.PK;
			orgHeader3.CompanyData.OB_OJ_ARDebtorGroup = orgDebtorGroup3.PK;
			OrgCompanyData companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_OJ_ARDebtorGroup = orgDebtorGroup1.PK;
			companyData.OB_OH = orgHeader1.PK;
			companyData.OB_GC = nonCurrentCompany.PK;
			Factory.Save();

			ARInvoice invoice1 = SetUpClassAInvoiceTest(transactionNum: "0004500", org: orgHeader1);
			ARInvoice invoice2 = SetUpClassAInvoiceTest(transactionNum: "0004501", org: orgHeader2);
			ARInvoice invoice3 = SetUpClassAInvoiceTest(transactionNum: "0004502", org: orgHeader1, branch: nonCurrentCompany.Branches[0]);
			ARInvoice invoice4 = SetUpClassAInvoiceTest(transactionNum: "0004503", org: orgHeader3, branch: nonCurrentCompany.Branches[0]);
			ARInvoice invoice5 = SetUpClassAInvoiceTest(transactionNum: "0004504", org: orgHeader3);

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);

			((ModuleGuidFilter)InvFilterBO["Debtor Group"]).Property = orgDebtorGroup1.PK;
			((ModuleGuidFilter)InvFilterBO["Debtor Group"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", invoice1.PK, testTransactions[0].PK);

			((ModuleGuidFilter)InvFilterBO["Debtor Group"]).Property = orgDebtorGroup2.PK;
			((ModuleGuidFilter)InvFilterBO["Debtor Group"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			AssertEquals("Should contain 1 invoices", invoice2.PK, testTransactions[0].PK);
		}

		public void TestPostDate()
		{
			ARInvoice testInvNonEmpty1 = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty1.AH_PostDate = new ZDateTime(2007, 10, 15);
			ARInvoice testInvNonEmpty2 = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty2.AH_PostDate = new ZDateTime(2007, 10, 17);

			((ModuleDateFilter)InvFilterBO["Post Date"]).Property1 = new ZDateTime(2007, 10, 14);
			((ModuleDateFilter)InvFilterBO["Post Date"]).Property2 = new ZDateTime(2007, 10, 18);
			((ModuleDateFilter)InvFilterBO["Post Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)InvFilterBO["Post Date"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1.PK));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2.PK));

			((ModuleDateFilter)InvFilterBO["Post Date"]).Property1 = new ZDateTime(2007, 10, 14);
			((ModuleDateFilter)InvFilterBO["Post Date"]).Property2 = new ZDateTime(2007, 10, 16);
			((ModuleDateFilter)InvFilterBO["Post Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)InvFilterBO["Post Date"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1.PK));

			((ModuleDateFilter)InvFilterBO["Post Date"]).Property1 = new ZDateTime(2007, 10, 28);
			((ModuleDateFilter)InvFilterBO["Post Date"]).Property2 = new ZDateTime(2007, 10, 29);
			((ModuleDateFilter)InvFilterBO["Post Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)InvFilterBO["Post Date"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 0 invoices", 0, testTransactions.Count);

			((ModuleDateFilter)InvFilterBO["Post Date"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1.PK));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2.PK));
		}

		public void TestTransactionDate()
		{
			ARInvoice testInvNonEmpty1 = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty1.AH_InvoiceDate = new ZDateTime(2007, 10, 15);
			ARInvoice testInvNonEmpty2 = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty2.AH_InvoiceDate = new ZDateTime(2007, 10, 17);

			((ModuleDateFilter)InvFilterBO["Transaction Date"]).Property1 = new ZDateTime(2007, 10, 14);
			((ModuleDateFilter)InvFilterBO["Transaction Date"]).Property2 = new ZDateTime(2007, 10, 18);
			((ModuleDateFilter)InvFilterBO["Transaction Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)InvFilterBO["Transaction Date"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1.PK));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2.PK));

			((ModuleDateFilter)InvFilterBO["Transaction Date"]).Property1 = new ZDateTime(2007, 10, 14);
			((ModuleDateFilter)InvFilterBO["Transaction Date"]).Property2 = new ZDateTime(2007, 10, 16);
			((ModuleDateFilter)InvFilterBO["Transaction Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)InvFilterBO["Transaction Date"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1.PK));

			((ModuleDateFilter)InvFilterBO["Transaction Date"]).Property1 = new ZDateTime(2007, 10, 28);
			((ModuleDateFilter)InvFilterBO["Transaction Date"]).Property2 = new ZDateTime(2007, 10, 29);
			((ModuleDateFilter)InvFilterBO["Transaction Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)InvFilterBO["Transaction Date"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 0 invoices", 0, testTransactions.Count);

			((ModuleDateFilter)InvFilterBO["Transaction Date"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1.PK));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2.PK));
		}

		public void TestDueDate()
		{
			ARInvoice testInvNonEmpty1 = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty1.AH_DueDate = new ZDateTime(2007, 10, 15);
			ARInvoice testInvNonEmpty2 = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty2.AH_DueDate = new ZDateTime(2007, 10, 17);

			((ModuleDateFilter)InvFilterBO["Due Date"]).Property1 = new ZDateTime(2007, 10, 14);
			((ModuleDateFilter)InvFilterBO["Due Date"]).Property2 = new ZDateTime(2007, 10, 18);
			((ModuleDateFilter)InvFilterBO["Due Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)InvFilterBO["Due Date"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1.PK));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2.PK));

			((ModuleDateFilter)InvFilterBO["Due Date"]).Property1 = new ZDateTime(2007, 10, 14);
			((ModuleDateFilter)InvFilterBO["Due Date"]).Property2 = new ZDateTime(2007, 10, 16);
			((ModuleDateFilter)InvFilterBO["Due Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)InvFilterBO["Due Date"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1.PK));

			((ModuleDateFilter)InvFilterBO["Due Date"]).Property1 = new ZDateTime(2007, 10, 28);
			((ModuleDateFilter)InvFilterBO["Due Date"]).Property2 = new ZDateTime(2007, 10, 29);
			((ModuleDateFilter)InvFilterBO["Due Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)InvFilterBO["Due Date"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 0 invoices", 0, testTransactions.Count);

			((ModuleDateFilter)InvFilterBO["Due Date"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1.PK));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2.PK));
		}

		public void TestPaymentStatusFilter()
		{
			ARInvoice testInvNonEmpty1 = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty1.AH_FullyPaidDate = new ZDateTime(2007, 10, 18);
			ARInvoice testInvNonEmpty2 = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty2.AH_FullyPaidDate = ZDateTime.Empty;

			((ModuleTextFilter)InvFilterBO["Payment Status"]).Property = AccountingUtils.PaymentStatusTypes.Unpaid;
			((ModuleTextFilter)InvFilterBO["Payment Status"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2));

			((ModuleTextFilter)InvFilterBO["Payment Status"]).Property = "ALL";
			((ModuleTextFilter)InvFilterBO["Payment Status"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2));

			((ModuleTextFilter)InvFilterBO["Payment Status"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2));
		}

		public void TestDisbursementInvoiceFilter()
		{
			ARInvoice testDisbursementInv1 = Factory.NewWithValidTestData<ARInvoice>();
			testDisbursementInv1.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			ARInvoice testDisbursementInv2 = Factory.NewWithValidTestData<ARInvoice>();
			testDisbursementInv2.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			ARInvoice testDisbursementInv3 = Factory.NewWithValidTestData<ARInvoice>();
			testDisbursementInv3.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice_Batching;
			ARInvoice testDisbursementInv4 = Factory.NewWithValidTestData<ARInvoice>();
			testDisbursementInv4.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching;
			ARInvoice testInvNonEmpty = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty.AH_TransactionCategory = InvoiceTypesList.Codes.DestinationChargesInvoice;

			((ModuleTextFilter)InvFilterBO["Disbursement Invoice"]).Property = "DSB";
			((ModuleTextFilter)InvFilterBO["Disbursement Invoice"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 4 invoices", 4, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testDisbursementInv1));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testDisbursementInv2));
			Assert("Should contain 3rd invoice", testTransactions.Contains(testDisbursementInv3));
			Assert("Should contain 4th invoice", testTransactions.Contains(testDisbursementInv4));

			((ModuleTextFilter)InvFilterBO["Disbursement Invoice"]).Property = "STD";
			((ModuleTextFilter)InvFilterBO["Disbursement Invoice"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoice", 1, testTransactions.Count);
			Assert("Should contain TestInvNonEmpty invoice", testTransactions.Contains(testInvNonEmpty));

			((ModuleTextFilter)InvFilterBO["Disbursement Invoice"]).Property = "ALL";
			((ModuleTextFilter)InvFilterBO["Disbursement Invoice"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 5 invoices", 5, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testDisbursementInv1));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testDisbursementInv2));
			Assert("Should contain 3rd invoice", testTransactions.Contains(testDisbursementInv3));
			Assert("Should contain 4th invoice", testTransactions.Contains(testDisbursementInv4));
			Assert("Should contain TestInvNonEmpty invoice", testTransactions.Contains(testInvNonEmpty));
		}

		public void TestPrintedFilter()
		{
			ARInvoice testInvNonEmpty1 = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty1.AH_InvoicePrinted = true;
			ARInvoice testInvNonEmpty2 = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty2.AH_InvoicePrinted = false;

			((ModuleTextFilter)InvFilterBO["Printed"]).Property = "PRN";
			((ModuleTextFilter)InvFilterBO["Printed"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1));

			((ModuleTextFilter)InvFilterBO["Printed"]).Property = "NPR";
			((ModuleTextFilter)InvFilterBO["Printed"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2));

			((ModuleTextFilter)InvFilterBO["Printed"]).Property = "ALL";
			((ModuleTextFilter)InvFilterBO["Printed"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2));

			((ModuleTextFilter)InvFilterBO["Printed"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2));
		}

		public void TestComplianceSubTypeFilterExistence()
		{
			foreach (string country in TestObjectCreator.CountriesSupportComplianceSubtype)
			{
				invFilterBO = null;
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					AssertNotNull(InvFilterBO["Compliance SubType"]);
				}
			}

			invFilterBO = null;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			AssertNull(InvFilterBO["Compliance SubType"]);
		}

		public void TestComplianceSubTypeFilter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Peru))
			{
				AssertNotNull(InvFilterBO["Compliance SubType"]);

				ARInvoice testInv1 = Factory.NewWithValidTestData<ARInvoice>();
				testInv1.AH_ComplianceSubType = "TXI";
				ARInvoice testInv2 = Factory.NewWithValidTestData<ARInvoice>();
				testInv2.AH_ComplianceSubType = "TCR";
				Factory.Save();

				((ModuleTextFilter)InvFilterBO["Compliance SubType"]).Property = "TXI";
				((ModuleTextFilter)InvFilterBO["Compliance SubType"]).IsActive = true;

				TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
				testTransactions.Load();
				AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv1));

				((ModuleTextFilter)InvFilterBO["Compliance SubType"]).Property = "TCR";
				((ModuleTextFilter)InvFilterBO["Compliance SubType"]).IsActive = true;

				testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
				testTransactions.Load();
				AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv2));

				((ModuleTextFilter)InvFilterBO["Compliance SubType"]).Property = "";
				((ModuleTextFilter)InvFilterBO["Compliance SubType"]).IsActive = true;

				testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
				testTransactions.Load();
				AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv1));
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv2));

				((ModuleTextFilter)InvFilterBO["Compliance SubType"]).IsActive = false;

				testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
				testTransactions.Load();
				AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv1));
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv2));
			}
		}

		public void TestGovtComplianceInvoiceFilter()
		{
			ARInvoice testInvNonEmpty1 = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty1.AH_TransactionReference = "blah blah";
			testInvNonEmpty1.AH_OH = TestOrg.PK;
			ARInvoice testInvNonEmpty2 = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty2.AH_TransactionReference = ZString.Empty;
			testInvNonEmpty2.AH_OH = TestOrg.PK;
			Factory.Save();

			((ModuleTextFilter)InvFilterBO["Government Compliance Invoice"]).Property = "UPD";
			((ModuleTextFilter)InvFilterBO["Government Compliance Invoice"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1));

			((ModuleTextFilter)InvFilterBO["Government Compliance Invoice"]).Property = "EMP";
			((ModuleTextFilter)InvFilterBO["Government Compliance Invoice"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2));

			((ModuleTextFilter)InvFilterBO["Government Compliance Invoice"]).Property = "ALL";
			((ModuleTextFilter)InvFilterBO["Government Compliance Invoice"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2));

			((ModuleTextFilter)InvFilterBO["Government Compliance Invoice"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2));
		}

		public void TestFullyPaidDateFilter()
		{
			ARInvoice testInvNonEmpty1 = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty1.AH_FullyPaidDate = new ZDateTime(2011, 11, 11);
			testInvNonEmpty1.AH_OH = TestOrg.PK;
			ARInvoice testInvNonEmpty2 = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty2.AH_FullyPaidDate = ZDateTime.Empty;
			testInvNonEmpty2.AH_OH = TestOrg.PK;
			Factory.Save();

			((ModuleDateFilter)InvFilterBO["Fully Paid Date"]).Property1 = new ZDateTime(2011, 11, 01);
			((ModuleDateFilter)InvFilterBO["Fully Paid Date"]).Property2 = new ZDateTime(2011, 11, 30);
			((ModuleDateFilter)InvFilterBO["Fully Paid Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)InvFilterBO["Fully Paid Date"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1));

			((ModuleDateFilter)InvFilterBO["Fully Paid Date"]).Property1 = new ZDateTime(2010, 11, 01);
			((ModuleDateFilter)InvFilterBO["Fully Paid Date"]).Property2 = new ZDateTime(2010, 11, 30);
			((ModuleDateFilter)InvFilterBO["Fully Paid Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)InvFilterBO["Fully Paid Date"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain no invoices", 0, testTransactions.Count);
			Assert("Should NOT contain 1st invoice", !testTransactions.Contains(testInvNonEmpty1));

			((ModuleDateFilter)InvFilterBO["Fully Paid Date"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1));
			Assert("Should contain 2nd invoice", testTransactions.Contains(testInvNonEmpty2));
		}

		public void TestAmendingReversalTransactionFilter()
		{
			ARInvoice testOriginalInv = Factory.NewWithValidTestData<ARInvoice>();
			testOriginalInv.AH_OH = TestOrg.PK;

			IAmending original = testOriginalInv;
			AssertNotNull("Should implement IAmending interface", original);

			IAmending amending = original.GenerateAmendingTransaction(testOriginalInv.AH_TransactionType);
			InvoicingBase amendingInvoice = amending as InvoicingBase;
			AssertNotNull("Should amend", amendingInvoice);

			testOriginalInv.GenerateReverseTransaction(true);
			testOriginalInv.SetCancellationFlag(true);
			InvoicingBase reverseInvoice = testOriginalInv.ReverseInvoice;
			AssertNotNull("Should reverse", reverseInvoice);
			reverseInvoice.SetCancellationFlag(true);
			testOriginalInv.GenerateMatchLinks();
			reverseInvoice.GenerateMatchLinks();
			TestObjectCreator.SetupMatchLinkMatchDate(testOriginalInv);
			TestObjectCreator.SetupMatchLinkMatchDate(reverseInvoice);

			ZGuid groupingGuid = ZGuid.NewZGuid();
			testOriginalInv.SetTransactionBelongsToGroupField(groupingGuid);

			Factory.Save();

			((ModuleTextFilter)InvFilterBO["Amending/Reversal Transaction"]).Property = "ALL";
			((ModuleTextFilter)InvFilterBO["Amending/Reversal Transaction"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 3 invoices", 3, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testOriginalInv));
			Assert("Should contain 2nd invoice", testTransactions.Contains(amendingInvoice));
			Assert("Should contain 3rd invoice", testTransactions.Contains(reverseInvoice));

			((ModuleTextFilter)InvFilterBO["Amending/Reversal Transaction"]).Property = "ORG";
			((ModuleTextFilter)InvFilterBO["Amending/Reversal Transaction"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testOriginalInv));

			((ModuleTextFilter)InvFilterBO["Amending/Reversal Transaction"]).Property = "AMD";
			((ModuleTextFilter)InvFilterBO["Amending/Reversal Transaction"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
			Assert("Should contain 2nd invoice", testTransactions.Contains(amendingInvoice));
			Assert("Should contain 3rd invoice", testTransactions.Contains(reverseInvoice));

			((ModuleTextFilter)InvFilterBO["Amending/Reversal Transaction"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 3 invoices", 3, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testOriginalInv));
			Assert("Should contain 2nd invoice", testTransactions.Contains(amendingInvoice));
			Assert("Should contain 3rd invoice", testTransactions.Contains(reverseInvoice));
		}

		public void TestHasTaxAmountFilter()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ARInvoice invoice1 = creator.CreateARInvoice<ARInvoice>("001", creator.AUD, 1.0m, creator.ABIGAS);
			ARInvoiceLine line1 = creator.CreateARInvoiceLine(invoice1, null, creator.CC1, creator.AUD, 1.0m, "Desc", 100.00m);
			line1.AL_AT = creator.GST1.PK;

			ARInvoice invoice2 = creator.CreateARInvoice<ARInvoice>("002", creator.AUD, 1.0m, creator.ABIGAS);
			ARInvoiceLine line2 = creator.CreateARInvoiceLine(invoice2, null, creator.CC1, creator.AUD, 1.0m, "Desc", 200.00m);

			Factory.Save();

			((ModuleFlagsFilter)InvFilterBO["Has Tax Amount"]).Property0 = true;
			((ModuleFlagsFilter)InvFilterBO["Has Tax Amount"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(invoice1));

			((ModuleFlagsFilter)InvFilterBO["Has Tax Amount"]).Property0 = false;
			((ModuleFlagsFilter)InvFilterBO["Has Tax Amount"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(invoice1));
			Assert("Should contain 2nd invoice", testTransactions.Contains(invoice2));

			((ModuleFlagsFilter)InvFilterBO["Has Tax Amount"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(invoice1));
			Assert("Should contain 2nd invoice", testTransactions.Contains(invoice2));
		}

		public void TestContainsTaxIDFilter()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ARInvoice invoice1 = creator.CreateARInvoice<ARInvoice>("001", creator.AUD, 1.0m, creator.ABIGAS);
			ARInvoiceLine line1 = creator.CreateARInvoiceLine(invoice1, null, creator.CC1, creator.AUD, 1.0m, "Desc", 100.00m);
			line1.AL_AT = creator.GST1.PK;

			ARInvoice invoice2 = creator.CreateARInvoice<ARInvoice>("002", creator.AUD, 1.0m, creator.ABIGAS);
			ARInvoiceLine line2 = creator.CreateARInvoiceLine(invoice2, null, creator.CC1, creator.AUD, 1.0m, "Desc", 200.00m);
			line2.AL_AT = creator.GSTFREE1.PK;

			Factory.Save();

			((ModuleNkFilter)InvFilterBO["Contains Tax ID"]).Property = creator.GST1.AT_Code;
			((ModuleNkFilter)InvFilterBO["Contains Tax ID"]).IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(invoice1));

			((ModuleNkFilter)InvFilterBO["Contains Tax ID"]).Property = creator.GSTFREE1.AT_Code;
			((ModuleNkFilter)InvFilterBO["Contains Tax ID"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			Assert("Should contain 2nd invoice", testTransactions.Contains(invoice2));

			((ModuleNkFilter)InvFilterBO["Contains Tax ID"]).Property = "NOTREPORT";
			((ModuleNkFilter)InvFilterBO["Contains Tax ID"]).IsActive = true;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 0 invoices", 0, testTransactions.Count);

			((ModuleNkFilter)InvFilterBO["Contains Tax ID"]).IsActive = false;

			testTransactions = new TransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(invoice1));
			Assert("Should contain 2nd invoice", testTransactions.Contains(invoice2));
		}

		public void TestContainsTaxIdTaxRateCollection()
		{
			var rate = Factory.NewWithValidTestData<AccTaxRate>();
			rate.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate.AT_Type = AccTaxRate.Types.NotReportable;
			rate.AT_TaxSystemCode = "Other";

			var rate2 = Factory.NewWithValidTestData<AccTaxRate>();
			rate2.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate2.AT_Code = "BBCXYZ";
			rate2.AT_Type = AccTaxRate.Types.NotReportable;
			rate2.AT_TaxSystemCode = "Other1";

			Factory.Save();

			var containsTaxIdModuleFilter = ((ModuleNkFilter)InvFilterBO["Contains Tax ID"]);
			containsTaxIdModuleFilter.IsActive = true;
			Assert("Filter should have VAT Tax System", containsTaxIdModuleFilter.List.Cast<AccTaxRate>().All(item => item.AT_TaxSystemCode.IsEmpty));
		}

		public void TestCompanyQuery()
		{
			ARInvoice testInvNonEmpty1 = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoice testInvNonEmpty2 = Factory.NewWithValidTestData<ARInvoice>();
			GlbCompany otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			otherBranch.GB_GC = otherCompany.PK;
			testInvNonEmpty2.AH_GB = otherBranch.PK;
			Factory.Save();

			AccTransactionHeaderCollection testTransactions = new AccTransactionHeaderCollection(Factory, InvFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
			Assert("Should contain 1st invoice", testTransactions.Contains(testInvNonEmpty1));
		}

		#endregion

		#region Lookups

		#region TestTransaction Types

		public void TestTransactionTypes()
		{
			Assert("Expected List to contains more than 0 elements", InvFilterBO.TransactionTypes.Count > 0);
		}

		#endregion

		#region Test Debtor List

		public void TestDebtorList()
		{
			AssertNotNull("Expected List to contains more than 0 elements", InvFilterBO.DebtorList);
		}

		#endregion

		#region Test Currency List

		public void TestCurrencyList()
		{
			AssertNotNull("Expected List to contains more than 0 elements", InvFilterBO.CurrencyList);
		}

		#endregion

		#region Test Department List

		public void TestDepartmentList()
		{
			AssertNotNull("Expected List to contains more than 0 elements", InvFilterBO.DepartmentList);
		}

		#endregion

		#region Test Branch List

		public void TestBranchList()
		{
			AssertNotNull("Expected List to contains more than 0 elements", InvFilterBO.BranchList);
		}

		#endregion

		#region Test PaymentStatus List

		public void TestPaymentStatusList()
		{
			AssertEquals("Expected List to contain 4 elements", 4, InvFilterBO.PaymentStatusList.Count);
			Assert("Expected List to contain ALL option", InvFilterBO.PaymentStatusList.ContainsCode(AccountingUtils.PaymentStatusTypes.All));
			Assert("Expected List to contain UNPAID option", InvFilterBO.PaymentStatusList.ContainsCode(AccountingUtils.PaymentStatusTypes.Unpaid));
			Assert("Expected List to contain PAID option", InvFilterBO.PaymentStatusList.ContainsCode(AccountingUtils.PaymentStatusTypes.Paid));
			Assert("Expected List to contain PARTPAID option", InvFilterBO.PaymentStatusList.ContainsCode(AccountingUtils.PaymentStatusTypes.PartPaid));
		}

		#endregion

		#region Test Printed List

		public void TestPrintedList()
		{
			Assert("Expected List to contains more than 0 elements", InvFilterBO.PrintedList.Count == 3);

			Assert("Expected List to contain ALL option", InvFilterBO.PrintedList.ContainsCode("ALL"));
			Assert("Expected List to contain PRN option", InvFilterBO.PrintedList.ContainsCode("PRN"));
			Assert("Expected List to contain NPR option", InvFilterBO.PrintedList.ContainsCode("NPR"));
		}

		#endregion

		#region Test GovtComplianceInvoice List

		public void TestGovtComplianceInvoiceList()
		{
			Assert("Expected List to contains more than 0 elements", InvFilterBO.GovtComplianceInvoiceList.Count == 3);

			Assert("Expected List to contain ALL option", InvFilterBO.GovtComplianceInvoiceList.ContainsCode("ALL"));
			Assert("Expected List to contain UPD option", InvFilterBO.GovtComplianceInvoiceList.ContainsCode("UPD"));
			Assert("Expected List to contain EMP option", InvFilterBO.GovtComplianceInvoiceList.ContainsCode("EMP"));
		}

		#endregion

		#region Test DebtorGroup Collection

		public void TestDebtorGroupCollection()
		{
			AssertNotNull("Expected List not null", InvFilterBO.DebtorGroupCollection);
		}

		#endregion

		#endregion

		#region Implementation

		#region InvoiceFilterBusinessObject For Test

		InvoicePrintingFilterBusinessObject InvFilterBO
		{
			get
			{
				if (invFilterBO == null)
				{
					invFilterBO = (InvoicePrintingFilterBusinessObject)GetNewFilterStripBusinessObject();
				}

				return invFilterBO;
			}
		}
		InvoicePrintingFilterBusinessObject invFilterBO;

		#endregion

		#region Test Organisation

		OrgHeader TestOrg
		{
			get
			{
				if (testOrg == null)
				{
					testOrg = Factory.NewWithValidTestData<OrgHeader>();
					testOrg.CompanyData.OB_OJ_ARDebtorGroup = TestDebtorGroup.PK;
					testOrg.OH_RL_NKClosestPort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

					Factory.Save();
				}
				return testOrg;
			}
		}
		OrgHeader testOrg;

		#endregion

		#region Test Job

		JobHeader TestJob
		{
			get
			{
				if (testJob == null)
				{
					testJob = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
					testJob.JH_JobNum = "111122";
					Factory.Save();
				}
				return testJob;
			}
		}
		JobHeader testJob;

		#endregion

		#region Test Currency

		RefCurrency TestCurrency => TestObjectCreator.GBP;

		#endregion

		#region Test Branch

		GlbBranch TestBranch => TestObjectCreator.NonCurrentBranch;

		#endregion

		#region Test Department

		GlbDepartment TestDepartment => TestObjectCreator.NonCurrentDepartment;

		#endregion

		#region Test DebtorGroup

		OrgDebtorGroup TestDebtorGroup
		{
			get
			{
				if (testDebtorGroup == null)
				{
					testDebtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
					Factory.Save();
				}
				return testDebtorGroup;
			}
		}
		OrgDebtorGroup testDebtorGroup;

		#endregion

		#region SetUp

		ARInvoice SetUpClassAInvoiceTest(string transactionNum = "", GlbBranch branch = null, OrgHeader org = null)
		{
			var testOrg = org ?? TestOrg;
			var testJob = TestJob;

			ARInvoice testInvNonEmpty = Factory.NewWithValidTestData<ARInvoice>();
			testInvNonEmpty.AH_InvoiceDate = new ZDateTime(2004, 6, 10);
			testInvNonEmpty.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			testInvNonEmpty.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			testInvNonEmpty.IsManuallySetTransactionNumber_ForTestOnly = !string.IsNullOrEmpty(transactionNum);
			testInvNonEmpty.AH_TransactionNum = transactionNum;
			testInvNonEmpty.AH_TransactionReference = "00001990";
			testInvNonEmpty.AH_ConsolidatedInvoiceRef = "123321";
			testInvNonEmpty.AH_PostDate = new ZDateTime(2007, 10, 15);
			testInvNonEmpty.AH_RX_NKTransactionCurrency = TestCurrency.RX_Code;
			testInvNonEmpty.AH_GB = branch?.PK ?? TestBranch.PK;
			testInvNonEmpty.AH_GE = TestDepartment.PK;
			testInvNonEmpty.AH_OH = testOrg.PK;
			testInvNonEmpty.AH_JH = testJob.PK;

			Factory.Save();

			return testInvNonEmpty;
		}

		#endregion

		#region Override

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new InvoicePrintingFilterBusinessObject();
		}

		#endregion

		#endregion
	}
}
