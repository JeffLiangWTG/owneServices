using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class TransactionFilterStripBusinessObjectTest : AccTransactionFilterStripBusinessObjectTest
	{
		#region Filters

		#region TestOrganisatonWithAddressFilter

		public void TestOrganisatonWithAddressFilter()
		{
			OrgAddress testAddress = Factory.NewWithValidTestData<OrgAddress>();
			testAddress.OA_Code = "XYZ";
			testAddress.OA_OH = TestOrg.PK;

			Invoice testInvoice1 = CreateNewInvoice(Factory);
			testInvoice1.AH_OH = TestOrg.PK;
			testInvoice1.AH_OA_InvoiceAddressOverride = ZGuid.Empty;
			testInvoice1.AH_TransactionNum = "00002544";

			Invoice testInvoice2 = CreateNewInvoice(Factory);
			testInvoice2.AH_OH = TestOrg.PK;
			testInvoice2.AH_OA_InvoiceAddressOverride = testAddress.PK;
			testInvoice2.AH_ConsolidatedInvoiceRef = "S0002544/A";

			Factory.Save();

			OrgWithAddressFilter orgWithAddressFilter;
			if (testInvoice1 is InvoicingBase)
			{
				if (testInvoice1.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					orgWithAddressFilter = ((OrgWithAddressFilter)TestFilterBizO["Debtor and Address"]);
				}
				else if (testInvoice1.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					orgWithAddressFilter = ((OrgWithAddressFilter)TestFilterBizO["Creditor and Address"]);
				}
				else
				{
					Assert(true);
					return;
				}
				orgWithAddressFilter.Organization = TestOrg.PK;
				orgWithAddressFilter.Address = testAddress.PK;
				orgWithAddressFilter.IsActive = true;

				TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Collection should contain 1 invoice", 1, testTransactions.Count);
				Assert("Collection contains invoice 2", testTransactions.Contains(testInvoice2.PK));

				orgWithAddressFilter.Address = ZGuid.Empty;
				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Collection should contain 2 invoice", 2, testTransactions.Count);
				Assert("Collection contains invoice 1", testTransactions.Contains(testInvoice1.PK));
				Assert("Collection contains invoice 2", testTransactions.Contains(testInvoice2.PK));

				orgWithAddressFilter.Address = TestOrg.AddressForSendingARDocuments.PK;
				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Collection should contain 1 invoice", 1, testTransactions.Count);
				Assert("Collection contains invoice 1", testTransactions.Contains(testInvoice1.PK));
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestDateFilter

		[TestDate(2015, 03, 22, 10, 10, 10)]
		public void TestDateFilter()
		{
			Journal testJournal1 = CreateNewJournal(Factory);
			testJournal1.AH_PostDate = ZDateTime.Now;
			Journal testJournal2 = CreateNewJournal(Factory);
			testJournal2.AH_InvoiceDate = ZDateTime.Now;
			testJournal2.AH_PostDate = ZDateTime.Now.AddDays(-4);

			Factory.Save();

			ModuleDateFilter postDateFilter = ((ModuleDateFilter)TestFilterBizO[Business.AccountingUtils.DateFilterTypes.PostDate]);
			postDateFilter.Property1 = ZDateTime.Now.AddDays(-2);
			postDateFilter.Property2 = ZDateTime.Now.AddDays(2);
			postDateFilter.IsActive = true;
			postDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain only TestJournal1", 1, testTransactions.Count);
			Assert("Collection should contain only TestJournal1", testTransactions.Contains(testJournal1.PK));

			postDateFilter.IsActive = false;
			ModuleDateFilter allDateFilter = ((ModuleDateFilter)TestFilterBizO[Business.AccountingUtils.DateFilterTypes.All]);
			allDateFilter.Property1 = ZDateTime.Now.AddDays(-4);
			allDateFilter.Property2 = ZDateTime.Now.AddDays(2);
			allDateFilter.IsActive = true;
			allDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("Collection should contain both TestJournals", 2, testTransactions.Count);
			Assert("Collection should contain TestJournal2", testTransactions.Contains(testJournal2.PK));
			allDateFilter.IsActive = false;

			postDateFilter.Property1 = ZDateTime.Now;
			postDateFilter.Property2 = ZDateTime.Now;
			postDateFilter.IsActive = true;
			postDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("Collection should one journal", 1, testTransactions.Count);
			Assert("Collection should contain TestJournal1 because its post date is today", testTransactions.Contains(testJournal1.PK));
		}

		#endregion

		#region TestPaymentStatusFilter

		public void TestPaymentStatusFilter()
		{
			Journal testJournal = CreateNewJournal(Factory);
			Journal testJournal2 = CreateNewJournal(Factory);
			testJournal.AH_FullyPaidDate = ZDateTime.Empty;
			testJournal2.AH_FullyPaidDate = ZDateTime.Now;

			Factory.Save();

			ModuleTextFilter paymentStatusFilter = ((ModuleTextFilter)TestFilterBizO["Payment Status"]);
			paymentStatusFilter.Property = "ALL";
			paymentStatusFilter.IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Should capture all the transactions", 2, testTransactions.Count);
			Assert("Should contain TestJournal", testTransactions.Contains(testJournal.PK));
			Assert("SHould contain TestJournal2", testTransactions.Contains(testJournal2.PK));

			paymentStatusFilter.Property = AccountingUtils.PaymentStatusTypes.Unpaid;
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("Should contain TestJournal only", 1, testTransactions.Count);
			Assert("Should contain TestJournal only", testTransactions.Contains(testJournal.PK));
		}

		#endregion

		#region TestPrintedFilter

		public void TestPrintedFilter()
		{
			Journal testJournal = CreateNewJournal(Factory);
			Journal testJournal2 = CreateNewJournal(Factory);
			testJournal.AH_InvoicePrinted = true;
			testJournal2.AH_InvoicePrinted = false;

			Factory.Save();

			ModuleTextFilter printedFilter = ((ModuleTextFilter)TestFilterBizO["Printed"]);
			printedFilter.Property = "ALL";
			printedFilter.IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Should capture all the transactions", 2, testTransactions.Count);
			Assert("Should contain TestJournal", testTransactions.Contains(testJournal.PK));
			Assert("SHould contain TestJournal2", testTransactions.Contains(testJournal2.PK));

			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("Should capture all the transactions", 2, testTransactions.Count);
			Assert("Should contain TestJournal", testTransactions.Contains(testJournal.PK));
			Assert("SHould contain TestJournal2", testTransactions.Contains(testJournal2.PK));

			printedFilter.Property = "PRN";
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("Should contain TestJournal only", 1, testTransactions.Count);
			Assert("Should contain TestJournal only", testTransactions.Contains(testJournal.PK));

			printedFilter.Property = "NPR";
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("Should contain TestJournal2 only", 1, testTransactions.Count);
			Assert("Should contain TestJournal2 only", testTransactions.Contains(testJournal2.PK));
		}

		#endregion

		#region TestComplianceSubTypeFilter

		public void TestComplianceSubTypeFilterExistence()
		{
			foreach (string country in TestObjectCreator.CountriesSupportComplianceSubtype)
			{
				fTestFilterBizO = null;
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					AssertNotNull(TestFilterBizO["Compliance SubType"]);
				}
			}

			fTestFilterBizO = null;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			AssertNull(TestFilterBizO["Compliance SubType"]);
		}

		public void TestComplianceSubTypeFilter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Peru))
			{
				AssertNotNull(TestFilterBizO["Compliance SubType"]);

				Invoice testInv1 = CreateNewInvoice(Factory);
				testInv1.AH_ComplianceSubType = "TXI";
				Invoice testInv2 = CreateNewInvoice(Factory);
				testInv2.AH_ComplianceSubType = "TCR";
				Factory.Save();

				((ModuleTextFilter)TestFilterBizO["Compliance SubType"]).Property = "TXI";
				((ModuleTextFilter)TestFilterBizO["Compliance SubType"]).IsActive = true;

				TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv1));

				((ModuleTextFilter)TestFilterBizO["Compliance SubType"]).Property = "TCR";
				((ModuleTextFilter)TestFilterBizO["Compliance SubType"]).IsActive = true;

				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Should contain 1 invoices", 1, testTransactions.Count);
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv2));

				((ModuleTextFilter)TestFilterBizO["Compliance SubType"]).Property = "";
				((ModuleTextFilter)TestFilterBizO["Compliance SubType"]).IsActive = true;

				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv1));
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv2));

				((ModuleTextFilter)TestFilterBizO["Compliance SubType"]).IsActive = false;

				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv1));
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv2));

				var vat3 = TestObjectCreator.CreateTaxRate("VAT3", "VAT3", 3);
				vat3.AT_PostingGroupId = 0;

				var ac1 = TestObjectCreator.CreateChargeCode("AC1");
				ac1.AC_AT_GSTRate = vat3.PK;
				ac1.AC_Desc = "desc";

				Invoice testInv3 = CreateNewInvoice(Factory);
				var invoiceLine = (InvoiceLine)testInv3.Lines.AddNew();
				invoiceLine.AL_JH = TestObjectCreator.Job1.PK;
				invoiceLine.AL_AC = ac1.PK;
				invoiceLine.AL_AT = vat3.PK;

				var charge = TestObjectCreator.CreateJobCharge(invoiceLine, TestObjectCreator.Job1, ac1);

				var complianceHeader = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(testInv3.AH_Ledger, "desc", "001", "TXI", "lineDesc", invoiceLine);

				Factory.Save();

				((ModuleTextFilter)TestFilterBizO["Compliance SubType"]).Property = "TXI";
				((ModuleTextFilter)TestFilterBizO["Compliance SubType"]).IsActive = true;

				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Should contain 2 invoices", 2, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv1));
				Assert("Should contain 3rd invoice", testTransactions.Contains(testInv3));
			}
		}

		#endregion

		#region TestAllNumbersFiltering

		public virtual void TestAllNumbersFiltering()
		{
			Job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.JobReadyForCostPosting.Code);
			Invoice invoiceJobNum = CreateNewInvoice(Factory);
			InvoicingLineBase invoicingLine = (InvoicingLineBase)invoiceJobNum.Lines.AddNew();
			invoicingLine.AL_JH = Job.PK;
			invoicingLine.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			TestObjectCreator.CreateJobCharge(invoicingLine, Job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			Factory.Save();
			ZString filterValueForTest = Job.JH_JobNum; //Can not set custom JH_JobNum, Will use this value for other properties as well.

			Journal testJournalChequeNumber = CreateNewJournal(Factory);
			testJournalChequeNumber.AH_ChequeOrReference = filterValueForTest;

			Journal testJournalTransNum = CreateNewJournal(Factory);
			testJournalTransNum.AH_TransactionNum = filterValueForTest;
			testJournalTransNum.IsManuallySetTransactionNumber_ForTestOnly = true;

			Receipt testReceiptDepositBatchNumber = CreateNewReceipt(Factory);
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testReceiptDepositBatchNumber.AH_AB = testBank.PK;
			testReceiptDepositBatchNumber.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			testReceiptDepositBatchNumber.AH_OSExTaxAmount = 90M;
			testReceiptDepositBatchNumber.AH_ExchangeRate = 1M;
			testReceiptDepositBatchNumber.AH_TransactionReference = "abc";
			testReceiptDepositBatchNumber.AH_Desc = "test receipt";
			testReceiptDepositBatchNumber.AH_ChequeOrReference = "00123";
			testReceiptDepositBatchNumber.AH_ChequeDrawer = "bbb";

			Payment testPaymentDDRBatchNumber = CreateNewPayment(Factory);
			testPaymentDDRBatchNumber.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			testPaymentDDRBatchNumber.AH_ReceiptBatchNo = filterValueForTest;

			Invoice testInvoiceJobInvoiceNumber = CreateNewInvoice(Factory);
			testInvoiceJobInvoiceNumber.AH_ConsolidatedInvoiceRef = filterValueForTest;
			Factory.Save();

			ModuleTextFilter allNumbersFilter = ((ModuleTextFilter)TestFilterBizO["All Numbers"]);
			allNumbersFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			allNumbersFilter.Property = filterValueForTest;
			allNumbersFilter.IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain all transactions, except TestInvoiceJobInvoiceNumber and TestReceiptDepositBatchNumber", 4, testTransactions.Count);
			Assert("Collection contains InvoiceJobNum", testTransactions.Contains(invoiceJobNum.PK));
			Assert("Collection contains TestJournalChequeNumber", testTransactions.Contains(testJournalChequeNumber.PK));
			Assert("Collection contains TestJournalTransNum", testTransactions.Contains(testJournalTransNum.PK));
			Assert("Collection contains TestPaymentDDRBatchNumber", testTransactions.Contains(testPaymentDDRBatchNumber.PK));

			allNumbersFilter.Property = filterValueForTest.Replace("S", "");
			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain all transactions, except TestInvoiceJobInvoiceNumber and InvoiceJobNum", 5, testTransactions.Count);
			Assert("Collection contains TestReceiptDepositBatchNumber", testTransactions.Contains(invoiceJobNum.PK));
			Assert("Collection contains TestReceiptDepositBatchNumber", testTransactions.Contains(testReceiptDepositBatchNumber.PK));
			Assert("Collection contains TestJournalChequeNumber", testTransactions.Contains(testJournalChequeNumber.PK));
			Assert("Collection contains TestJournalTransNum", testTransactions.Contains(testJournalTransNum.PK));
			Assert("Collection contains TestPaymentDDRBatchNumber", testTransactions.Contains(testPaymentDDRBatchNumber.PK));
		}

		public virtual void TestAllNumbersFilter_JobNumber()
		{
			Job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.JobReadyForCostPosting.Code);
			var invoice = CreateNewInvoice(Factory);
			var invoicingLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoicingLine.AL_JH = Job.PK;
			invoicingLine.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			TestObjectCreator.CreateJobCharge(invoicingLine, Job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			Factory.Save();

			var allNumbersFilter = ((ModuleTextFilter)TestFilterBizO["All Numbers"]);
			var transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);

			allNumbersFilter.IsActive = false;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("Total Number of Transactions should be", 1, transactions.Count);

			allNumbersFilter.Property = "SS5346";
			allNumbersFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			allNumbersFilter.IsActive = true;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("Collection should be empty", 0, transactions.Count);

			allNumbersFilter.Property = Job.JH_JobNum;
			allNumbersFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			transactions.Load(TestFilterBizO.Filter);
			Assert("Collection should contain Job", transactions.Contains(invoice.PK));

			var jobNumberMinLength = 4;
			Assert("Job Number should be a valid string", Job.JH_JobNum.Length >= jobNumberMinLength);
			allNumbersFilter.Property = Job.JH_JobNum.Substring(0, jobNumberMinLength);
			allNumbersFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			transactions.Load(TestFilterBizO.Filter);
			Assert("Collection should contain Job", transactions.Contains(invoice.PK));

			allNumbersFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			transactions.Load(TestFilterBizO.Filter);
			Assert("Collection should contain Job", transactions.Contains(invoice.PK));
		}

		#endregion

		#region TestTransactionNumberFilteringUsingContainsAndStartsWith

		public void TestTransactionNumberFilteringUsingContainsAndStartsWith()
		{
			Invoice inv = CreateNewInvoice(Factory);
			inv.AH_OH = TestOrg.PK;

			Invoice inv2 = CreateNewInvoice(Factory);
			inv2.AH_OH = TestOrg.PK;

			Journal jnl = CreateNewJournal(Factory);
			jnl.AH_OH = TestOrg.PK;

			Factory.Save();

			inv.AH_TransactionNum = "00035262";
			inv2.AH_TransactionNum = "00001626";
			jnl.AH_TransactionNum = "00001583";

			ModuleTextFilter transactionNumberFilter = ((ModuleTextFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.TransactionNumber]);
			transactionNumberFilter.Property = "00001626";
			transactionNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			transactionNumberFilter.IsActive = true;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 1 transaction in the collection", 1, transactions.Count);
			Assert("Collection should contain Inv2 ", transactions.Contains(inv2));

			transactionNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			transactionNumberFilter.Property = "26";

			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 2 transactions in the collection", 2, transactions.Count);
			Assert("Collection should contain Inv", transactions.Contains(inv));
			Assert("Collection should contain Inv2 ", transactions.Contains(inv2));
		}

		#endregion

		#region TestJobNumberFiltering

		public void TestJobNumberFiltering()
		{
			PrepareForJobNumberFilteringTest();
			Factory.Save();

			ModuleTextFilter jobNumberFilter = ((ModuleTextFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.JobNumber]);
			AssertEquals("JobNumberFilter should be defaulted to Exact", SQLComparisonOperator.Equal, jobNumberFilter.SqlComparisonOperator);
			jobNumberFilter.Property = "S0000";
			jobNumberFilter.IsActive = true;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("Collection should be empty", 0, transactions.Count);

			jobNumberFilter.Property = Job2.JH_JobNum;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 2 matching transactions", 2, transactions.Count);
			Assert("Collection should contain Invoice", transactions.Contains(Invoice));
			Assert("Collection should contain Crd", transactions.Contains(Crd));

			jobNumberFilter.Property = Job3.JH_JobNum;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 1 matching transaction", 1, transactions.Count);
			Assert("Collection should contain Adj", transactions.Contains(Adj));

			jobNumberFilter.IsActive = false;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 3 matching transactions", 3, transactions.Count);
		}

		#endregion

		#region TestCustomerEntryNumberFilters

		public void TestCustomsEntryNumberFilters()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			CusEntryNumber number1 = shipment1.Numbers.AddNew();
			number1.CE_EntryType = "COC";
			number1.CE_EntryNum = "111";
			number1.CE_Category = "CUS";
			number1.CE_IssueDate = new ZDateTime(2007, 1, 1);

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			CusEntryNumber number2 = shipment2.Numbers.AddNew();
			number2.CE_EntryType = "DLV";
			number2.CE_EntryNum = "122";
			number2.CE_Category = "CUS";
			number2.CE_IssueDate = new ZDateTime(2007, 1, 1);

			Job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job2 = Factory.NewJobWithValidTestDataForTesting<Job>();

			Job.JH_ParentID = shipment1.PK;
			Job2.JH_ParentID = shipment2.PK;

			Invoice = CreateNewInvoice(Factory);
			Invoice.AH_JH = Job.PK;
			Invoice2 = CreateNewInvoice(Factory);
			Invoice2.AH_JH = Job2.PK;

			InvoiceLine line1 = (InvoiceLine)Invoice.Lines.AddNew();
			InvoiceLine line2 = (InvoiceLine)Invoice2.Lines.AddNew();

			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			line1.AL_JH = Job.PK;
			line1.AL_OSExTaxAmount = line1.AL_LineAmount = 100M;
			line2.AL_JH = Job2.PK;
			line2.AL_OSExTaxAmount = line2.AL_LineAmount = 50m;

			TestObjectCreator.CreateJobCharge(line1, Job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TestObjectCreator.CreateJobCharge(line2, Job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Factory.Save();

			ModuleNumberFilter customsEntryNoFilter = (ModuleNumberFilter)TestFilterBizO["Customs Entry #"];
			customsEntryNoFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			customsEntryNoFilter.Property = "";
			customsEntryNoFilter.IsActive = true;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 2 Payments in the collection", 2, transactions.Count);

			customsEntryNoFilter.Property = "1";
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 2 Payments in the collection", 2, transactions.Count);

			customsEntryNoFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			customsEntryNoFilter.Property = "122";
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);

			BusinessObject dec1 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			BusinessObject entryHeader1 = ((BusinessObjectCollection)dec1["CustomsEntryHeaders"]).AddNew();

			BusinessObject dec2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			BusinessObject entryHeader2 = ((BusinessObjectCollection)dec2["CustomsEntryHeaders"]).AddNew();

			number1.CE_ParentID = entryHeader1.PK;
			number2.CE_ParentID = entryHeader2.PK;
			Job.JH_ParentID = dec1.PK;
			Job2.JH_ParentID = dec2.PK;
			Factory.Save();

			customsEntryNoFilter.Property = "1";
			customsEntryNoFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 2 Payments in the collection", 2, transactions.Count);
		}

		#endregion

		#region Test Compliance Number

		public void TestComplianceNumberFilter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var vat3 = TestObjectCreator.CreateTaxRate("VAT3", "", 3);
				vat3.AT_PostingGroupId = 0;

				var ac1 = TestObjectCreator.CreateChargeCode("AC1");
				ac1.AC_AT_GSTRate = vat3.PK;
				ac1.AC_Desc = "desc";

				var testInv = new Invoice[4];
				for (int i = 0; i < testInv.Length; i++)
				{
					testInv[i] = CreateNewInvoice(Factory);
					var line = (InvoiceLine)testInv[i].Lines.AddNew();
					line.AL_JH = TestObjectCreator.Job1.PK;
					line.AL_AC = ac1.PK;
					line.AL_AT = vat3.PK;
					var charge = TestObjectCreator.CreateJobCharge(line, TestObjectCreator.Job1, ac1);
				}
				testInv[1].AH_TransactionReference = "00001014";
				var complianceHeader3 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(testInv[2].AH_Ledger, "desc", "", "TXI", "lineDesc", testInv[2].Lines[0]);
				var complianceHeader4 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(testInv[3].AH_Ledger, "desc", "00001015", "TXI", "lineDesc", testInv[3].Lines[0]);
				Factory.Save();

				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				var filter = ((ModuleNumberFilter)TestFilterBizO["Compliance #"]);
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "00001014";
				filter.IsActive = true;

				var testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("There should be 1 Invoice in the collection", 1, testTransactions.Count);
				Assert("Should not contain 1st invoice", !testTransactions.Contains(testInv[0]));
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv[1]));
				Assert("Should not contain 3rd invoice", !testTransactions.Contains(testInv[2]));
				Assert("Should not contain 4th invoice", !testTransactions.Contains(testInv[3]));

				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 3 Invoices in the collection", 3, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv[0]));
				Assert("Should not contain 2nd invoice", !testTransactions.Contains(testInv[1]));
				Assert("Should contain 3rd invoice", testTransactions.Contains(testInv[2]));
				Assert("Should contain 4th invoice", testTransactions.Contains(testInv[3]));

				filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 1 Invoice in the collection", 1, testTransactions.Count);
				Assert("Should not contain 1st invoice", !testTransactions.Contains(testInv[0]));
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv[1]));
				Assert("Should not contain 3rd invoice", !testTransactions.Contains(testInv[2]));
				Assert("Should not contain 4th invoice", !testTransactions.Contains(testInv[3]));

				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				fTestFilterBizO = null;
				filter = ((ModuleNumberFilter)TestFilterBizO["Compliance #"]);
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "00001014";
				filter.IsActive = true;
				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 1 Invoice in the collection", 1, testTransactions.Count);
				Assert("Should not contain 1st invoice", !testTransactions.Contains(testInv[0]));
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv[1]));
				Assert("Should not contain 3rd invoice", !testTransactions.Contains(testInv[2]));
				Assert("Should not contain 4th invoice", !testTransactions.Contains(testInv[3]));

				filter.Property = "00001015";
				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("There should be 1 Invoice in the collection", 1, testTransactions.Count);
				Assert("Should not contain 1st invoice", !testTransactions.Contains(testInv[0]));
				Assert("Should not contain 2nd invoice", !testTransactions.Contains(testInv[1]));
				Assert("Should not contain 3rd invoice", !testTransactions.Contains(testInv[2]));
				Assert("Should contain 4th invoice", testTransactions.Contains(testInv[3]));

				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				filter.Property = "00001015";
				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 3 Invoices in the collection", 3, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv[0]));
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv[1]));
				Assert("Should contain 3rd invoice", testTransactions.Contains(testInv[2]));
				Assert("Should not contain 4th invoice", !testTransactions.Contains(testInv[3]));

				filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
				filter.Property = "0000101";
				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 2 Invoices in the collection", 2, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv[0]));
				Assert("Should not contain 2nd invoice", !testTransactions.Contains(testInv[1]));
				Assert("Should contain 3rd invoice", testTransactions.Contains(testInv[2]));
				Assert("Should not contain 4th invoice", !testTransactions.Contains(testInv[3]));

				filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
				filter.Property = "0000101";
				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 2 Invoices in the collection", 2, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv[0]));
				Assert("Should not contain 2nd invoice", !testTransactions.Contains(testInv[1]));
				Assert("Should contain 3rd invoice", testTransactions.Contains(testInv[2]));
				Assert("Should not contain 4th invoice", !testTransactions.Contains(testInv[3]));

				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 2 Invoices in the collection", 2, testTransactions.Count);
				Assert("Should contain 1st invoice", testTransactions.Contains(testInv[0]));
				Assert("Should not contain 2nd invoice", !testTransactions.Contains(testInv[1]));
				Assert("Should contain 3rd invoice", testTransactions.Contains(testInv[2]));
				Assert("Should not contain 4th invoice", !testTransactions.Contains(testInv[3]));

				filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();

				AssertEquals("There should be 2 Invoices in the collection", 2, testTransactions.Count);
				Assert("Should not contain 1st invoice", !testTransactions.Contains(testInv[0]));
				Assert("Should contain 2nd invoice", testTransactions.Contains(testInv[1]));
				Assert("Should not contain 3rd invoice", !testTransactions.Contains(testInv[2]));
				Assert("Should contain 4th invoice", testTransactions.Contains(testInv[3]));
			}
		}

		#endregion

		#region Test Master Bill Or Ocean Bill Number

		public void TestMasterBillOrOceanBillNumber()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment.JS_HouseBill = "D12345";
			shipment2.JS_HouseBill = "C98765";
			shipment3.JS_HouseBill = "B4455";

			shipment.JS_IsShipping = false;
			shipment2.JS_IsShipping = false;
			shipment3.JS_IsShipping = true;

			Job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job.JH_ParentID = shipment.PK;
			Job2.JH_ParentID = shipment2.PK;
			Job3.JH_ParentID = shipment3.PK;

			Factory.Save();

			Invoice = CreateNewInvoice(Factory);
			Invoice.AH_JH = Job.PK;
			Invoice2 = CreateNewInvoice(Factory);
			Invoice2.AH_JH = Job2.PK;
			Invoice3 = CreateNewInvoice(Factory);
			Invoice3.AH_JH = Job3.PK;

			InvoiceLine line1 = (InvoiceLine)Invoice.Lines.AddNew();
			InvoiceLine line2 = (InvoiceLine)Invoice2.Lines.AddNew();
			InvoiceLine line3 = (InvoiceLine)Invoice3.Lines.AddNew();

			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			line3.AL_AG = TestObjectCreator.GLHeader1.PK;
			line1.AL_JH = Job.PK;
			line1.AL_OSExTaxAmount = line1.AL_OSAmount = line1.AL_LineAmount = 100M;
			line2.AL_JH = Job2.PK;
			line2.AL_OSExTaxAmount = line2.AL_LineAmount = 50m;
			line3.AL_JH = Job3.PK;
			line3.AL_OSExTaxAmount = line3.AL_LineAmount = 150m;

			TestObjectCreator.CreateJobCharge(line1, Job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TestObjectCreator.CreateJobCharge(line2, Job2, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TestObjectCreator.CreateJobCharge(line3, Job3, TestObjectCreator.CC1, TestObjectCreator.AUD);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_MasterBillNum = "A12345";
			consol2.JK_MasterBillNum = "B98765";

			JobConShipLink jobConShipLink = Factory.NewWithValidTestData<JobConShipLink>();
			JobConShipLink jobConShipLink2 = Factory.NewWithValidTestData<JobConShipLink>();

			jobConShipLink.JN_JK = consol.PK;
			jobConShipLink2.JN_JK = consol2.PK;

			jobConShipLink.JN_JS = shipment.PK;
			jobConShipLink2.JN_JS = shipment2.PK;

			Factory.Save();

			ModuleNumberFilter filter = ((ModuleNumberFilter)TestFilterBizO["Master Bill #/Ocean Bill #"]);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "A";
			filter.IsActive = true;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);

			filter.Property = "B";
			filter.IsActive = true;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 2 Payments in the collection", 2, transactions.Count);

			filter.Property = "C";
			filter.IsActive = true;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be no Payments in the collection", 0, transactions.Count);

			shipment.JS_IsShipping = true;
			shipment.JS_HouseBill = "AA0987";
			Factory.Save();
			filter.Property = "A";
			filter.IsActive = true;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);
		}

		#endregion

		#region Test Voyage Vessel Filter

		public void TestVoyageVesselFilter()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "tvfvf1";
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "SGSIN";

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";

			Transport transport1 = consol1.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUMEL";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_Vessel = vessel.RV_FK;
			transport1.JW_VoyageFlight = "2222";

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "tvfvf2";
			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "NZAKL";

			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_LloydsNumber = "7654321";

			Transport transport2 = consol2.Transports[0];
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_Vessel = vessel2.RV_FK;
			transport2.JW_VoyageFlight = "2233";

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.JS_IsShipping = false;
			shipment2.JS_IsShipping = false;

			JobConShipLink jobConShipLink1 = Factory.NewWithValidTestData<JobConShipLink>();
			JobConShipLink jobConShipLink2 = Factory.NewWithValidTestData<JobConShipLink>();

			Job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job2 = Factory.NewJobWithValidTestDataForTesting<Job>();

			jobConShipLink1.JN_JK = consol1.PK;
			jobConShipLink2.JN_JK = consol2.PK;

			jobConShipLink1.JN_JS = shipment1.PK;
			jobConShipLink2.JN_JS = shipment2.PK;

			Job.JH_ParentID = shipment1.PK;
			Job2.JH_ParentID = shipment2.PK;

			Invoice = CreateNewInvoice(Factory);
			Invoice.AH_JH = Job.PK;
			Invoice2 = CreateNewInvoice(Factory);
			Invoice2.AH_JH = Job2.PK;

			InvoiceLine line1 = (InvoiceLine)Invoice.Lines.AddNew();
			InvoiceLine line2 = (InvoiceLine)Invoice2.Lines.AddNew();
			InvoiceLine line3 = (InvoiceLine)Invoice2.Lines.AddNew();

			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			line3.AL_AG = TestObjectCreator.GLHeader1.PK;
			line1.AL_JH = Job.PK;
			line1.AL_OSExTaxAmount = line1.AL_LineAmount = 100M;
			line2.AL_JH = Job2.PK;
			line2.AL_OSExTaxAmount = line2.AL_LineAmount = 50m;

			TestObjectCreator.CreateJobCharge(line1, Job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TestObjectCreator.CreateJobCharge(line2, Job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Factory.Save();

			ModuleTextAndNkFilter voyageVesselFilter = (ModuleTextAndNkFilter)TestFilterBizO["Flight/Voyage # and Vessel"];

			voyageVesselFilter.Property = "";
			voyageVesselFilter.IsActive = true;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 2 Payments in the collection", 2, transactions.Count);

			voyageVesselFilter.Property = "22";
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 2 Payments in the collection", 2, transactions.Count);

			voyageVesselFilter.Property = "2233";
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);

			voyageVesselFilter.Property = "";
			voyageVesselFilter.NkProperty = vessel.RV_Name;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);

			voyageVesselFilter.NkProperty = vessel2.RV_Name;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);

			voyageVesselFilter.NkProperty = "Test";
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be no Payments in the collection", 0, transactions.Count);

			shipment1.JS_IsShipping = true;
			shipment2.JS_IsShipping = true;
			Factory.Save();
			voyageVesselFilter.Property = "22";
			voyageVesselFilter.NkProperty = "";
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be no Payments in the collection", 0, transactions.Count);

			JobSailing jobSailing1 = Factory.NewWithValidTestData<JobSailing>();
			JobVoyage jobVoyage1 = Factory.NewWithValidTestData<JobVoyage>();
			VoyageDestination jobVoyDestination1 = Factory.NewWithValidTestData<VoyageDestination>();

			shipment1.JS_JX = jobSailing1.PK;
			jobSailing1.JX_JB = jobVoyDestination1.PK;
			jobVoyDestination1.JB_JV = jobVoyage1.PK;
			jobVoyage1.JV_VoyageFlight = "2222";

			Factory.Save();
			voyageVesselFilter.Property = "22";

			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);
		}

		#endregion

		#region Test Job Local Reference Filter

		public void TestLocalJobReferenceFilter()
		{
			Job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job2 = Factory.NewJobWithValidTestDataForTesting<Job>();

			Invoice = CreateNewInvoice(Factory);
			Invoice2 = CreateNewInvoice(Factory);

			InvoiceLine line1 = (InvoiceLine)Invoice.Lines.AddNew();
			InvoiceLine line2 = (InvoiceLine)Invoice2.Lines.AddNew();

			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			line1.AL_JH = Job.PK;
			line1.AL_OSExTaxAmount = 100M;
			line1.AL_LocalExTaxAmount = 100M;
			line2.AL_JH = Job2.PK;
			line2.AL_OSAmount = 50m;
			line2.AL_LineAmount = 50m;

			TestObjectCreator.CreateJobCharge(line1, Job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TestObjectCreator.CreateJobCharge(line2, Job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Factory.Save();

			ModuleNumberFilter filter = ((ModuleNumberFilter)TestFilterBizO["Job Local Reference"]);
			filter.Property = Job.JH_JobLocalReference;
			filter.IsActive = true;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);
			AssertCollectionContains(Invoice, transactions);

			filter.Property = "Denys";
			filter.IsActive = true;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 0 Payment in the collection", 0, transactions.Count);
		}

		#endregion

		#region Test House Bill Number Filter

		public void TestHouseBillNumberFilter_QueryData()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.JS_HouseBill = "A12345";
			shipment2.JS_HouseBill = "B98765";

			Job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job.JH_ParentID = shipment1.PK;
			Job2.JH_ParentID = shipment2.PK;

			BaseJobDeclaration declaration1 = Factory.New<BaseJobDeclaration>();
			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_HouseBill = "A56789";
			declaration2.JE_HouseBill = "B12345";

			Job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job4 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job3.JH_ParentID = declaration1.PK;
			Job4.JH_ParentID = declaration2.PK;

			Factory.Save();

			Invoice = CreateNewInvoice(Factory);
			Invoice.AH_JH = Job.PK;
			Invoice2 = CreateNewInvoice(Factory);
			Invoice2.AH_JH = Job2.PK;
			Invoice3 = CreateNewInvoice(Factory);
			Invoice3.AH_JH = Job3.PK;
			Invoice4 = CreateNewInvoice(Factory);
			Invoice4.AH_JH = Job4.PK;

			InvoiceLine line1 = (InvoiceLine)Invoice.Lines.AddNew();
			InvoiceLine line2 = (InvoiceLine)Invoice2.Lines.AddNew();
			InvoiceLine line3 = (InvoiceLine)Invoice3.Lines.AddNew();
			InvoiceLine line4 = (InvoiceLine)Invoice4.Lines.AddNew();
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			line3.AL_AG = TestObjectCreator.GLHeader1.PK;
			line4.AL_AG = TestObjectCreator.GLHeader1.PK;

			line1.AL_JH = Job.PK;
			line1.AL_OSExTaxAmount = 100M;
			line1.AL_LocalExTaxAmount = 100M;

			line2.AL_JH = Job2.PK;
			line2.AL_OSAmount = 50m;
			line2.AL_LineAmount = 50m;

			line3.AL_JH = Job3.PK;
			line3.AL_OSAmount = 100m;
			line3.AL_LineAmount = 100m;

			line4.AL_JH = Job4.PK;
			line4.AL_OSAmount = 50m;
			line4.AL_LineAmount = 50m;

			TestObjectCreator.CreateJobCharge(line1, Job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TestObjectCreator.CreateJobCharge(line2, Job2, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TestObjectCreator.CreateJobCharge(line3, Job3, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TestObjectCreator.CreateJobCharge(line4, Job4, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Factory.Save();

			ModuleNumberFilter filter = ((ModuleNumberFilter)TestFilterBizO["House Bill #"]);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "A";
			filter.IsActive = true;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 2 Payments in the collection", 2, transactions.Count);
			Assert("Collection should contain Invoice", transactions.Contains(Invoice.PK));
			Assert("Collection should contain Invoice3", transactions.Contains(Invoice3.PK));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "B98765";
			filter.IsActive = true;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);
			Assert("Collection should contain Invoice2", transactions.Contains(Invoice2.PK));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "B12345";
			filter.IsActive = true;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);
			Assert("Collection should contain Invoice4", transactions.Contains(Invoice4.PK));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "C";
			filter.IsActive = true;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 0 Payment in the collection", 0, transactions.Count);
		}

		public void TestHouseBillNumberFilter_WhenSearchLengthGreaterThanMaxLength()
		{
			var maximumMaxLength = Math.Max(JobShipmentSchema.JS_HouseBill.MaxLength, JobDeclarationSchema.JE_HouseBill.MaxLength);
			var minimumMaxLength = Math.Min(JobShipmentSchema.JS_HouseBill.MaxLength, JobDeclarationSchema.JE_HouseBill.MaxLength);

			var filter = ((ModuleNumberFilter)TestFilterBizO["House Bill #"]);
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(maximumMaxLength), filter.MaxLength);

			filter.Property = TestObjectCreator.GetRandomString(maximumMaxLength);
			AssertEquals("is-no-result query should be FALSE when search value is an acceptable size.", false, filter.Query.IsNoResultQuery);

			filter.Property = TestObjectCreator.GetRandomString(maximumMaxLength + 1);
			AssertEquals("is-no-result query should be TRUE when search value is too long.", true, filter.Query.IsNoResultQuery);

			filter.Property = TestObjectCreator.GetRandomString(JobShipmentSchema.JS_HouseBill.MaxLength);
			AssertContains(JobShipmentSchema.JS_HouseBill.Name, filter.Query.GetAsWhereClause(false));

			if (minimumMaxLength == maximumMaxLength || minimumMaxLength == JobShipmentSchema.JS_HouseBill.MaxLength)
			{
				AssertContains(JobDeclarationSchema.JE_HouseBill.Name, filter.Query.GetAsWhereClause(false));
			}
			else
			{
				AssertNotContains(JobDeclarationSchema.JE_HouseBill.Name, filter.Query.GetAsWhereClause(false));
			}

			filter.Property = TestObjectCreator.GetRandomString(JobShipmentSchema.JS_HouseBill.MaxLength + 1);
			AssertNotContains(JobShipmentSchema.JS_HouseBill.Name, filter.Query.GetAsWhereClause(false));

			filter.Property = TestObjectCreator.GetRandomString(JobDeclarationSchema.JE_HouseBill.MaxLength);
			AssertContains(JobDeclarationSchema.JE_HouseBill.Name, filter.Query.GetAsWhereClause(false));

			if (minimumMaxLength == maximumMaxLength || minimumMaxLength == JobDeclarationSchema.JE_HouseBill.MaxLength)
			{
				AssertContains(JobShipmentSchema.JS_HouseBill.Name, filter.Query.GetAsWhereClause(false));
			}
			else
			{
				AssertNotContains(JobShipmentSchema.JS_HouseBill.Name, filter.Query.GetAsWhereClause(false));
			}

			filter.Property = TestObjectCreator.GetRandomString(JobDeclarationSchema.JE_HouseBill.MaxLength + 1);
			AssertNotContains(JobDeclarationSchema.JE_HouseBill.Name, filter.Query.GetAsWhereClause(false));
		}

		#endregion

		#region Test Order Number Filter

		public void TestOrderNumberFilter()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			Order order1 = Factory.NewWithValidTestData<Order>();
			Order order2 = Factory.NewWithValidTestData<Order>();

			order1.JD_JS = shipment1.PK;
			order2.JD_JS = shipment2.PK;
			order1.JD_OrderNumber = "909090";
			order2.JD_OrderNumber = "909091";

			Job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job2 = Factory.NewJobWithValidTestDataForTesting<Job>();

			Job.JH_ParentID = shipment1.PK;
			Job2.JH_ParentID = shipment2.PK;

			Invoice = CreateNewInvoice(Factory);
			Invoice.AH_JH = Job.PK;

			Invoice2 = CreateNewInvoice(Factory);
			Invoice2.AH_JH = Job2.PK;

			InvoiceLine line1 = (InvoiceLine)Invoice.Lines.AddNew();
			InvoiceLine line2 = (InvoiceLine)Invoice2.Lines.AddNew();

			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			line1.AL_JH = Job.PK;
			line1.AL_OSExTaxAmount = line1.AL_LineAmount = 100M;
			line2.AL_JH = Job2.PK;
			line2.AL_OSExTaxAmount = line2.AL_LineAmount = 50m;

			TestObjectCreator.CreateJobCharge(line1, Job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TestObjectCreator.CreateJobCharge(line2, Job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Factory.Save();

			const string Reference = "Reference10";
			var oi = shipment1.DocsAndCartage.OrderItems.AddNew();
			oi.JT_OrderReference = shipment1.JS_UniqueConsignRef = Reference;

			Factory.Save();

			var filterName = "Order #";
			ModuleNumberFilter filter = (ModuleNumberFilter)TestFilterBizO[filterName];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "";
			filter.IsActive = true;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 2 Payments in the collection", 2, transactions.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "909091";
			filter.IsActive = true;

			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "C";
			filter.IsActive = true;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be no Payments in the collection", 0, transactions.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = Reference;
			filter.IsActive = true;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);
			AssertEquals($"Wrong Payment is found using '{filterName}' filter", shipment1.PK, transactions[0].Job.Parent.PK);
		}

		#endregion

		#region Test Carrier Filter

		public void TestCarrier()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.JS_IsShipping = true;
			shipment1.JS_IsShipping = false;

			Job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job2 = Factory.NewJobWithValidTestDataForTesting<Job>();

			Job.JH_ParentID = shipment1.PK;
			Job2.JH_ParentID = shipment2.PK;

			Factory.Save();

			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			consol1.SetDefaultShippingLineAddress(org1);

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			consol2.SetDefaultShippingLineAddress(org2);

			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();

			JobConShipLink jobConShipLink1 = Factory.NewWithValidTestData<JobConShipLink>();
			JobConShipLink jobConShipLink2 = Factory.NewWithValidTestData<JobConShipLink>();

			jobConShipLink1.JN_JK = consol1.PK;
			jobConShipLink2.JN_JK = consol2.PK;

			jobConShipLink1.JN_JS = shipment1.PK;
			jobConShipLink2.JN_JS = shipment2.PK;

			Invoice = CreateNewInvoice(Factory);
			Invoice.AH_JH = Job.PK;
			Invoice2 = CreateNewInvoice(Factory);
			Invoice2.AH_JH = Job2.PK;

			InvoiceLine line1 = (InvoiceLine)Invoice.Lines.AddNew();
			InvoiceLine line2 = (InvoiceLine)Invoice2.Lines.AddNew();

			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			line1.AL_JH = Job.PK;
			line1.AL_OSExTaxAmount = line1.AL_LineAmount = 100M;
			line2.AL_JH = Job2.PK;
			line2.AL_OSExTaxAmount = line2.AL_LineAmount = 50m;

			TestObjectCreator.CreateJobCharge(line1, Job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			TestObjectCreator.CreateJobCharge(line2, Job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)TestFilterBizO["Carrier"];
			filter.Property = org1.PK;
			filter.IsActive = true;
			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);

			filter.Property = org2.PK;
			filter.IsActive = true;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);

			filter.Property = org3.PK;
			filter.IsActive = true;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be no Payments in the collection", 0, transactions.Count);

			Job2.JH_ParentID = consol2.PK;
			consol2.SetDefaultShippingLineAddress(org1);
			Factory.Save();
			filter.Property = org1.PK;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 2 Payments in the collection", 2, transactions.Count);
		}

		#endregion

		#region DepositBatchNumberFilter Test

		public void TestDepositBatchNumberFilter()
		{
			Receipt rec = CreateNewReceipt(Factory);
			rec.AH_ReceiptBatchNo = "00003495";
			rec.AH_OH = TestOrg.PK;
			rec.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;

			Journal jnl = CreateNewJournal(Factory);
			jnl.AH_ReceiptBatchNo = "00003495";
			jnl.AH_OH = TestOrg.PK;

			Receipt rec2 = CreateNewReceipt(Factory);
			rec2.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			rec2.AH_ReceiptBatchNo = "00001498";
			rec2.AH_OH = TestOrg.PK;

			Factory.Save();

			((ModuleGuidFilter)TestFilterBizO[TestFilterBizO.CreditorDebtorText]).Property = TestOrg.PK;
			((ModuleGuidFilter)TestFilterBizO[TestFilterBizO.CreditorDebtorText]).IsActive = true;

			ModuleTextFilter depositBatchNumber = ((ModuleTextFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.DepositBatchNumber]);
			depositBatchNumber.Property = "00003495";
			depositBatchNumber.IsActive = true;
			depositBatchNumber.SqlComparisonOperator = SQLComparisonOperator.Equal;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 1 Receipt in the collection", 1, transactions.Count);
			AssertEquals("The Receipt should be Rec", rec.PK, transactions[0].PK);

			((ModuleTextFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.DepositBatchNumber]).Property = "49";
			((ModuleTextFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.DepositBatchNumber]).SqlComparisonOperator = SQLComparisonOperator.Contains;
			transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 2 Receipts in the collection", 2, transactions.Count);
			Assert("Rec2 should be in the collection", transactions.Contains(rec2));
			Assert("Rec should be in the collection", transactions.Contains(rec));
		}

		#endregion

		#region DDRBatchNumberFilter Test

		public void TestDDRBatchNumberFilter()
		{
			Payment pay = CreateNewPayment(Factory);
			pay.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			pay.AH_ReceiptBatchNo = "00005839";
			pay.AH_OH = TestOrg.PK;

			Payment pay2 = CreateNewPayment(Factory);
			pay2.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			pay2.AH_ReceiptBatchNo = "00004582";
			pay2.AH_OH = TestOrg.PK;

			Journal jnl = CreateNewJournal(Factory);
			jnl.AH_ReceiptBatchNo = "00005839";
			jnl.AH_OH = TestOrg.PK;

			Factory.Save();

			((ModuleGuidFilter)TestFilterBizO[TestFilterBizO.CreditorDebtorText]).Property = TestOrg.PK;
			((ModuleGuidFilter)TestFilterBizO[TestFilterBizO.CreditorDebtorText]).IsActive = true;

			ModuleTextFilter dDRBatchNumber = ((ModuleTextFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.DDRBatchNumber]);
			dDRBatchNumber.Property = "00005839";
			dDRBatchNumber.IsActive = true;
			dDRBatchNumber.SqlComparisonOperator = SQLComparisonOperator.Equal;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);
			Assert("Pay should be in the collection", transactions.Contains(pay));

			dDRBatchNumber.Property = "45";
			dDRBatchNumber.SqlComparisonOperator = SQLComparisonOperator.Contains;

			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);
			Assert("Pay2 should be in the collection", transactions.Contains(pay2));
		}

		#endregion

		#region AddDDRBatchNumberToFilter Test

		public void TestAddDDRBatchNumberToFilter()
		{
			Payment pay = CreateNewPayment(Factory);
			pay.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			pay.AH_ReceiptBatchNo = "00001004";

			Payment pay2 = CreateNewPayment(Factory);
			pay2.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			pay2.AH_ReceiptBatchNo = "00001045";

			Journal jnl = CreateNewJournal(Factory);
			jnl.AH_ReceiptBatchNo = "00001004";

			Factory.Save();

			ModuleTextFilter dDRBatchNumber = ((ModuleTextFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.DDRBatchNumber]);
			dDRBatchNumber.Property = "00001004";
			dDRBatchNumber.IsActive = true;
			dDRBatchNumber.SqlComparisonOperator = SQLComparisonOperator.Equal;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);
			Assert("The payment should be Pay", transactions.Contains(pay));

			dDRBatchNumber.Property = "104";
			dDRBatchNumber.SqlComparisonOperator = SQLComparisonOperator.Contains;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 1 Payment in the collection", 1, transactions.Count);
			Assert("The payment should be Pay2", transactions.Contains(pay2));
		}

		#endregion

		#region AddDepositBatchNumberToFilter Test

		public void TestAddDepositBatchNumberToFilter()
		{
			Receipt rec = CreateNewReceipt(Factory);
			rec.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			rec.AH_ReceiptBatchNo = "00008339";

			Receipt rec2 = CreateNewReceipt(Factory);
			rec2.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			rec2.AH_ReceiptBatchNo = "00002748";

			Payment pay = CreateNewPayment(Factory);
			pay.AH_ReceiptBatchNo = "00003390";

			Factory.Save();

			ModuleTextFilter depositBatchNumber = ((ModuleTextFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.DepositBatchNumber]);
			depositBatchNumber.Property = "339";
			depositBatchNumber.IsActive = true;
			depositBatchNumber.SqlComparisonOperator = SQLComparisonOperator.Contains;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be one receipt in the collection", 1, transactions.Count);
			Assert("Rec should be in the collection", transactions.Contains(rec));

			depositBatchNumber.Property = "8";
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 2 receipts in the collection", 2, transactions.Count);
			Assert("Rec should be in the collection", transactions.Contains(rec));
			Assert("Rec2 should be in the collection", transactions.Contains(rec2));
		}

		#endregion

		#region TestChequeNumberFilteringUsingContainsAndStartsWith

		public void TestChequeNumberFilteringUsingContainsAndStartsWith()
		{
			Payment pay = CreateNewPayment(Factory);
			pay.AH_OH = TestOrg.PK;
			pay.AH_ChequeOrReference = "26798";

			Receipt rec = CreateNewReceipt(Factory);
			rec.AH_OH = TestOrg.PK;
			rec.AH_ChequeOrReference = "24579";

			Factory.Save();

			ModuleTextFilter chequeReferenceNumber = ((ModuleTextFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.ChequeReferenceNumber]);
			chequeReferenceNumber.Property = "26";
			chequeReferenceNumber.IsActive = true;
			chequeReferenceNumber.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();

			AssertEquals("There should be 1 matching transaction", 1, transactions.Count);
			Assert("Collection should contain Pay", transactions.Contains(pay));

			chequeReferenceNumber.Property = "79";
			chequeReferenceNumber.IsActive = true;
			chequeReferenceNumber.SqlComparisonOperator = SQLComparisonOperator.Contains;

			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 2 matching transactions", 2, transactions.Count);
		}

		#endregion

		#region TestInvoicePaymentReferenceNumberFiltering

		public void TestInvoicePaymentReferenceNumberFiltering()
		{
			var invoice1 = CreateNewInvoice(Factory);
			var invoice2 = CreateNewInvoice(Factory);
			var invoice3 = CreateNewInvoice(Factory);

			var headerReference1 = Factory.New<AccTransactionHeaderReference>();
			headerReference1.AH1_Reference = "00001000";
			headerReference1.AH1_AH = invoice1.PK;
			headerReference1.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRR;
			var headerReference2 = Factory.New<AccTransactionHeaderReference>();
			headerReference2.AH1_Reference = "00001001";
			headerReference2.AH1_AH = invoice2.PK;
			headerReference2.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRR;

			Factory.Save();

			ModuleTextFilter invoicePaymentReferenceNumberFilter = ((ModuleTextFilter)TestFilterBizO["Invoice Remittance Reference"]);
			invoicePaymentReferenceNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			invoicePaymentReferenceNumberFilter.Property = "00001000";
			invoicePaymentReferenceNumberFilter.IsActive = true;

			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(60), invoicePaymentReferenceNumberFilter.MaxLength);

			var testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain one invoice", 1, testTransactions.Count);
			Assert("Collection contains invoice1", testTransactions.Contains(invoice1.PK));
			Assert("Collection doesn't contains invoice2", !testTransactions.Contains(invoice2.PK));
			Assert("Collection doesn't contains invoice3", !testTransactions.Contains(invoice3.PK));

			invoicePaymentReferenceNumberFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			invoicePaymentReferenceNumberFilter.Property = "";

			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain one invoice", 1, testTransactions.Count);
			Assert("Collection contains invoice3", testTransactions.Contains(invoice3.PK));
			Assert("Collection doesn't contains invoice1", !testTransactions.Contains(invoice1.PK));
			Assert("Collection doesn't contains invoice2", !testTransactions.Contains(invoice2.PK));

			invoicePaymentReferenceNumberFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			invoicePaymentReferenceNumberFilter.Property = "";

			testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load();
			AssertEquals("Collection should contain 2 invoices", 2, testTransactions.Count);
			Assert("Collection contains invoice1", testTransactions.Contains(invoice1.PK));
			Assert("Collection contains invoice2", testTransactions.Contains(invoice2.PK));
			Assert("Collection doesn't contains invoice3", !testTransactions.Contains(invoice3.PK));
		}

		#endregion

		#region TestFilterbyCurrentCompany

		public void TestFilterbyCurrentCompany()
		{
			PrepareForJobNumberFilteringTest();
			Crd.AH_GB = new TestObjectCreator(Factory).NonCurrentCompanyBranch.PK;
			Crd.Lines[0].AL_GB = Crd.Company.Branches[0].PK;
			Crd.Lines[1].AL_GB = Crd.Company.Branches[0].PK;
			Factory.Save();

			ModuleTextFilter jobNumberFilter = ((ModuleTextFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.JobNumber]);
			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			jobNumberFilter.Property = Job.JH_JobNum;
			jobNumberFilter.IsActive = true;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be only 1 matching transaction as Crd saved to foreign company branch", 1, transactions.Count);
			Assert("Collection should contain Invoice", transactions.Contains(Invoice));

			jobNumberFilter.Property = Job3.JH_JobNum;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 1 matching transaction", 1, transactions.Count);
			Assert("Collection should contain Adj", transactions.Contains(Adj));

			jobNumberFilter.IsActive = false;
			transactions.Load(TestFilterBizO.Filter);
			AssertEquals("There should be 2 matching transactions", 2, transactions.Count);
		}

		#endregion

		#region TestFilterBySettlementGroup

		public void TestFilterBySettlementGroup()
		{
			Factory.Save();

			OrgHeader newHeaderForGrouping = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgHeaderInGroup = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgHeaderInGroup2 = Factory.NewWithValidTestData<OrgHeader>();
			if (TestFilterBizO.UseDebtor_ForTestOnly)
			{
				orgHeaderInGroup.ARSettlementGroupPK = newHeaderForGrouping.PK;
				orgHeaderInGroup2.ARSettlementGroupPK = newHeaderForGrouping.PK;
			}
			else
			{
				orgHeaderInGroup.APSettlementGroupPK = newHeaderForGrouping.PK;
				orgHeaderInGroup2.APSettlementGroupPK = newHeaderForGrouping.PK;
			}

			OrgHeader orgHeaderOurOfGroup = Factory.NewWithValidTestData<OrgHeader>();

			Invoice inv = CreateNewInvoice(Factory);
			inv.AH_OH = orgHeaderInGroup.PK;

			CreditNote crd = CreateNewCreditNote(Factory);
			crd.AH_OH = orgHeaderInGroup2.PK;

			Invoice inv1 = CreateNewInvoice(Factory);
			inv1.AH_OH = newHeaderForGrouping.PK;

			Invoice inv2 = CreateNewInvoice(Factory);
			inv2.AH_OH = orgHeaderOurOfGroup.PK;

			Factory.Save();

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 4 transactions in the collection", 4, transactions.Count);

			ModuleGuidFilter settlementGroupFilter = ((ModuleGuidFilter)TestFilterBizO["Settlement Group"]);
			settlementGroupFilter.Property = newHeaderForGrouping.PK;
			settlementGroupFilter.IsActive = true;

			transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("There should be 3 transactions in the collection", 3, transactions.Count);
			Assert("Collection should contain ARInv", transactions.Contains(inv));
			Assert("Collection should contain ARCrd", transactions.Contains(crd));
			Assert("Collection should contain Inv1", transactions.Contains(inv1));

			settlementGroupFilter.Property = orgHeaderInGroup.PK;
			transactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			transactions.Load();
			AssertEquals("The collection should be empty", 0, transactions.Count);
		}

		#endregion

		#region TestTransactionAmountFiltering

		public void TestTransactionAmountFiltering()
		{
			Factory.Save();
			Invoice testOtherLedgerInv = CreateNewInvoiceOfOtherLedger(Factory);
			TestObjectCreator.CreateInvoiceLine(testOtherLedgerInv, testOtherLedgerInv.TransactionCurrency, testOtherLedgerInv.AH_ExchangeRate, 998m, 0m, 0m);
			testOtherLedgerInv.AH_OutstandingAmount = 100m;
			testOtherLedgerInv.AH_OH = TestOrg.PK;
			Invoice testInv = CreateNewInvoice(Factory);
			TestObjectCreator.CreateInvoiceLine(testInv, testInv.TransactionCurrency, testInv.AH_ExchangeRate, 998m * (testInv.AH_Ledger == LedgerTypes.AccountsPayable ? -1 : 1), 0m, 0m);
			testInv.AH_OutstandingAmount = 100m;
			testInv.AH_OH = TestOrg.PK;
			CreditNote testCrd = CreateNewCreditNote(Factory);
			TestObjectCreator.CreateInvoiceLine(testCrd, testCrd.TransactionCurrency, testCrd.AH_ExchangeRate, 600m * (testInv.AH_Ledger == LedgerTypes.AccountsPayable ? -1 : 1), 0m, 0m);
			testCrd.AH_OutstandingAmount = 100m;
			testCrd.AH_OH = TestOrg.PK;
			Receipt testRec = CreateNewReceipt(Factory);
			testRec.AH_OSTotal = -500m;
			testRec.AH_OutstandingAmount = 100m;
			testRec.AH_OH = TestOrg.PK;
			Invoice testInv2 = CreateNewInvoice(Factory);
			TestObjectCreator.CreateInvoiceLine(testInv2, testInv2.TransactionCurrency, testInv2.AH_ExchangeRate, 2500m, 0m, 0m);
			testInv2.AH_OutstandingAmount = 100m;
			testInv2.AH_OH = TestOrg.PK;
			Journal testJrn = CreateNewJournal(Factory);
			testJrn.AH_OSTotal = -250m;
			testJrn.AH_OutstandingAmount = 100m;
			testJrn.AH_OH = TestOrg.PK;

			ModuleNumberRangeFilter transactionAmountFilter = ((ModuleNumberRangeFilter)TestFilterBizO["Transaction Amount"]);
			transactionAmountFilter.Property1 = 0m;
			transactionAmountFilter.Property2 = 1000m;
			transactionAmountFilter.IsActive = true;

			TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory);
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("Collection should contain only TestInv", 1, testTransactions.Count);
			Assert("TestInv should be in collection", testTransactions.Contains(testInv));

			transactionAmountFilter.Property1 = -1000m;
			transactionAmountFilter.Property2 = 0m;
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("Collection should contain 3 transactions", 3, testTransactions.Count);
			Assert("TestCrd should be in collection", testTransactions.Contains(testCrd));
			Assert("TestRec should be in collection", testTransactions.Contains(testRec));
			Assert("TestJrn should be in collection", testTransactions.Contains(testJrn));

			transactionAmountFilter.Property1 = -500m;
			transactionAmountFilter.Property2 = 1000m;
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("Collection should contain 3 transactions", 3, testTransactions.Count);
			Assert("TestInv should be in collection", testTransactions.Contains(testInv));
			Assert("TestRec should be in collection", testTransactions.Contains(testRec));
			Assert("TestJrn should be in collection", testTransactions.Contains(testJrn));

			transactionAmountFilter.Property1 = 0m;
			transactionAmountFilter.Property2 = 0m;
			testTransactions.Load(TestFilterBizO.Filter);
			//AssertEquals("Collection should contain all 5 transactions", 5, TestTransactions.Count);
			AssertEquals("Collection should be empty", 0, testTransactions.Count);
		}

		#endregion

		#region TestDisbursementInvoiceFilter

		public void TestDisbursementInvoiceFilter()
		{
			if (!TestFilterBizO.IsPayableModule)
			{
				Journal testJournal1 = CreateNewJournal(Factory);
				Journal testJournal2 = CreateNewJournal(Factory);
				Journal testJournal3 = CreateNewJournal(Factory);
				Journal testJournal4 = CreateNewJournal(Factory);
				Journal testJournal5 = CreateNewJournal(Factory);
				testJournal1.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
				testJournal2.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
				testJournal3.AH_TransactionCategory = "";
				testJournal4.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice_Batching;
				testJournal5.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching;

				Factory.Save();

				ModuleFlagsFilter isDisbursementFilter = ((ModuleFlagsFilter)TestFilterBizO["Disbursement Invoice"]);
				isDisbursementFilter.IsActive = true;
				isDisbursementFilter.Property0 = ZBool.False;

				TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
				testTransactions.Load();
				AssertEquals("Should capture all the transactions", 5, testTransactions.Count);
				Assert("Should contain TestJournal1", testTransactions.Contains(testJournal1.PK));
				Assert("Should contain TestJournal2", testTransactions.Contains(testJournal2.PK));
				Assert("Should contain TestJournal3", testTransactions.Contains(testJournal3.PK));
				Assert("Should contain TestJournal4", testTransactions.Contains(testJournal4.PK));
				Assert("Should contain TestJournal5", testTransactions.Contains(testJournal5.PK));

				isDisbursementFilter.Property0 = ZBool.True;
				testTransactions.Load(TestFilterBizO.Filter);
				AssertEquals("Should capture only Disbursement Invoice transactions", 4, testTransactions.Count);
				Assert("Should contain TestJournal1", testTransactions.Contains(testJournal1.PK));
				Assert("Should contain TestJournal2", testTransactions.Contains(testJournal2.PK));
				Assert("Should NOT contain TestJournal3", !testTransactions.Contains(testJournal3.PK));
				Assert("Should contain TestJournal4", testTransactions.Contains(testJournal4.PK));
				Assert("Should contain TestJournal5", testTransactions.Contains(testJournal5.PK));
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestRelatedTransactionsNotPaidFilter

		public void TestRelatedTransactionsNotPaidFilter()
		{
			if (TestFilterBizO.ShouldAddRelatedTransactionsNotPaidFilter_ForTestOnly)
			{
				// Primary transactions
				Invoice inv1_HasRelatedFullyPaid = CreateNewInvoice(Factory);
				InvoicingLineBase line1 = TestObjectCreator.CreateInvoiceLine(inv1_HasRelatedFullyPaid, inv1_HasRelatedFullyPaid.TransactionCurrency, inv1_HasRelatedFullyPaid.AH_ExchangeRate, 10m);
				line1.AL_AC = TestObjectCreator.CC1.PK;
				line1.AL_JH = TestObjectCreator.Job1.PK;
				line1.AL_GB = GlbBranch.CurrentBranch.PK;
				line1.AL_GE = GlbDepartment.CurrentDepartment.PK;
				TestObjectCreator.CreateJobCharge(line1, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

				Invoice inv2_HasRelatedNotFullyPaid = CreateNewInvoice(Factory);
				InvoicingLineBase line2 = TestObjectCreator.CreateInvoiceLine(inv2_HasRelatedNotFullyPaid, inv2_HasRelatedNotFullyPaid.TransactionCurrency, inv2_HasRelatedNotFullyPaid.AH_ExchangeRate, 10m);
				line2.AL_AC = TestObjectCreator.CC2.PK;
				line2.AL_JH = TestObjectCreator.Job1.PK;
				line2.AL_GB = GlbBranch.CurrentBranch.PK;
				line2.AL_GE = GlbDepartment.CurrentDepartment.PK;
				TestObjectCreator.CreateJobCharge(line2, TestObjectCreator.Job1, TestObjectCreator.CC2, TestObjectCreator.AUD);

				Invoice inv3_HasPartiallyRelated = CreateNewInvoice(Factory);
				InvoicingLineBase line3 = TestObjectCreator.CreateInvoiceLine(inv3_HasPartiallyRelated, inv3_HasPartiallyRelated.TransactionCurrency, inv3_HasPartiallyRelated.AH_ExchangeRate, 10m);
				line3.AL_AC = TestObjectCreator.CC3.PK;
				line3.AL_JH = TestObjectCreator.Job1.PK;
				line3.AL_GB = GlbBranch.CurrentBranch.PK;
				line3.AL_GE = GlbDepartment.CurrentDepartment.PK;
				TestObjectCreator.CreateJobCharge(line3, TestObjectCreator.Job1, TestObjectCreator.CC3, TestObjectCreator.AUD);

				Invoice inv4_Unrelated = CreateNewInvoice(Factory);
				InvoicingLineBase line4 = TestObjectCreator.CreateInvoiceLine(inv4_Unrelated, inv4_Unrelated.TransactionCurrency, inv4_Unrelated.AH_ExchangeRate, 10m);
				line4.AL_AC = TestObjectCreator.CC4.PK;
				line4.AL_JH = TestObjectCreator.Job1.PK;
				line4.AL_GB = GlbBranch.CurrentBranch.PK;
				line4.AL_GE = GlbDepartment.CurrentDepartment.PK;
				TestObjectCreator.CreateJobCharge(line4, TestObjectCreator.Job1, TestObjectCreator.CC4, TestObjectCreator.AUD);

				Invoice inv5_HasRelatedDSBFullyPaid = null;
				Invoice inv6_HasRelatedDSBNotFullyPaid = null;

				if (TestFilterBizO.IsPayableModule)
				{
					inv5_HasRelatedDSBFullyPaid = CreateNewInvoice(Factory);
					InvoicingLineBase line5 = TestObjectCreator.CreateInvoiceLine(inv5_HasRelatedDSBFullyPaid, inv5_HasRelatedDSBFullyPaid.TransactionCurrency, inv5_HasRelatedDSBFullyPaid.AH_ExchangeRate, 10m);
					line5.AL_AC = TestObjectCreator.CC5.PK;
					line5.AL_JH = TestObjectCreator.Job1.PK;
					line5.AL_GB = GlbBranch.CurrentBranch.PK;
					line5.AL_GE = GlbDepartment.CurrentDepartment.PK;
					TestObjectCreator.CreateJobCharge(line5, TestObjectCreator.Job1, TestObjectCreator.CC5, TestObjectCreator.AUD);

					inv6_HasRelatedDSBNotFullyPaid = CreateNewInvoice(Factory);
					InvoicingLineBase line6 = TestObjectCreator.CreateInvoiceLine(inv6_HasRelatedDSBNotFullyPaid, inv6_HasRelatedDSBNotFullyPaid.TransactionCurrency, inv6_HasRelatedDSBNotFullyPaid.AH_ExchangeRate, 10m);
					line6.AL_AC = TestObjectCreator.CC6.PK;
					line6.AL_JH = TestObjectCreator.Job1.PK;
					line6.AL_GB = GlbBranch.CurrentBranch.PK;
					line6.AL_GE = GlbDepartment.CurrentDepartment.PK;
					TestObjectCreator.CreateJobCharge(line6, TestObjectCreator.Job1, TestObjectCreator.CC6, TestObjectCreator.AUD);
				}

				// Related Transactions
				Invoice inv1 = CreateNewInvoiceOfOtherLedger(Factory);
				InvoicingLineBase lineR1 = TestObjectCreator.CreateInvoiceLine(inv1, inv1.TransactionCurrency, inv1.AH_ExchangeRate, 10m);
				lineR1.AL_AC = TestObjectCreator.CC1.PK;
				lineR1.AL_JH = TestObjectCreator.Job1.PK;
				lineR1.AL_GB = GlbBranch.CurrentBranch.PK;
				lineR1.AL_GE = GlbDepartment.CurrentDepartment.PK;
				TestObjectCreator.CreateJobCharge(lineR1, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				TransactionMatchLink matchlink = ((IMatching)inv1).CurrentMatchGroup.AddNew();
				matchlink.AP_AH = inv1.PK;
				matchlink.AP_Amount = inv1.AH_OutstandingAmount;
				inv1.AH_OutstandingAmount = 0M;
				inv1.AH_FullyPaidDate = ZDateTime.Today;

				AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
				headerToMatch.AH_InvoiceAmount = -matchlink.AP_Amount;

				AccTransactionMatchLink linkToMatch = ((IMatching)inv1).CurrentMatchGroup.AddNew();
				linkToMatch.AP_AH = headerToMatch.PK;
				linkToMatch.AP_Amount = -matchlink.AP_Amount;
				TestObjectCreator.SetupMatchLinkMatchDate(inv1);

				Invoice inv2 = CreateNewInvoiceOfOtherLedger(Factory);
				InvoicingLineBase lineR2 = TestObjectCreator.CreateInvoiceLine(inv2, inv2.TransactionCurrency, inv2.AH_ExchangeRate, 10m);
				lineR2.AL_AC = TestObjectCreator.CC2.PK;
				lineR2.AL_JH = TestObjectCreator.Job1.PK;
				lineR2.AL_GB = GlbBranch.CurrentBranch.PK;
				lineR2.AL_GE = GlbDepartment.CurrentDepartment.PK;
				TestObjectCreator.CreateJobCharge(lineR2, TestObjectCreator.Job1, TestObjectCreator.CC2, TestObjectCreator.AUD);
				inv2.AH_FullyPaidDate = ZDateTime.Empty;

				Invoice inv3 = CreateNewInvoiceOfOtherLedger(Factory);
				InvoicingLineBase lineR3 = TestObjectCreator.CreateInvoiceLine(inv3, inv3.TransactionCurrency, inv3.AH_ExchangeRate, 10m);
				lineR3.AL_AC = TestObjectCreator.CC3.PK;
				lineR3.AL_JH = TestObjectCreator.Job2.PK;
				lineR3.AL_GB = GlbBranch.CurrentBranch.PK;
				lineR3.AL_GE = GlbDepartment.CurrentDepartment.PK;
				TestObjectCreator.CreateJobCharge(lineR3, TestObjectCreator.Job2, TestObjectCreator.CC3, TestObjectCreator.AUD);

				if (TestFilterBizO.IsPayableModule)
				{
					Invoice inv5 = CreateNewInvoiceOfOtherLedger(Factory);
					InvoicingLineBase lineR5 = TestObjectCreator.CreateInvoiceLine(inv5, inv5.TransactionCurrency, inv5.AH_ExchangeRate, 10m);
					lineR5.AL_AC = TestObjectCreator.CC5.PK;
					lineR5.AL_JH = TestObjectCreator.Job1.PK;
					lineR5.AL_GB = GlbBranch.CurrentBranch.PK;
					lineR5.AL_GE = GlbDepartment.CurrentDepartment.PK;
					TestObjectCreator.CreateJobCharge(lineR5, TestObjectCreator.Job1, TestObjectCreator.CC5, TestObjectCreator.AUD);
					matchlink = ((IMatching)inv5).CurrentMatchGroup.AddNew();
					matchlink.AP_AH = inv5.PK;
					matchlink.AP_Amount = inv5.AH_OutstandingAmount;
					inv5.AH_OutstandingAmount = 0M;
					inv5.AH_FullyPaidDate = ZDateTime.Today;
					inv5.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;

					AccTransactionHeader headerToMatch2 = Factory.NewWithValidTestData<AccTransactionHeader>();
					headerToMatch2.AH_InvoiceAmount = -matchlink.AP_Amount;

					linkToMatch = ((IMatching)inv5).CurrentMatchGroup.AddNew();
					linkToMatch.AP_AH = headerToMatch2.PK;
					linkToMatch.AP_Amount = -matchlink.AP_Amount;
					TestObjectCreator.SetupMatchLinkMatchDate(inv5);

					Invoice inv6 = CreateNewInvoiceOfOtherLedger(Factory);
					InvoicingLineBase lineR6 = TestObjectCreator.CreateInvoiceLine(inv6, inv6.TransactionCurrency, inv6.AH_ExchangeRate, 10m);
					lineR6.AL_AC = TestObjectCreator.CC6.PK;
					lineR6.AL_JH = TestObjectCreator.Job1.PK;
					lineR6.AL_GB = GlbBranch.CurrentBranch.PK;
					lineR6.AL_GE = GlbDepartment.CurrentDepartment.PK;
					TestObjectCreator.CreateJobCharge(lineR6, TestObjectCreator.Job1, TestObjectCreator.CC6, TestObjectCreator.AUD);
					inv6.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice_Batching;
				}

				Factory.Save();

				ModuleTextFilter relatedTransactionsNotPaidFilter = ((ModuleTextFilter)TestFilterBizO["Related Transactions Not Paid"]);
				relatedTransactionsNotPaidFilter.IsActive = true;

				if (TestFilterBizO.IsPayableModule)
				{
					relatedTransactionsNotPaidFilter.Property = string.Empty;
					TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
					testTransactions.Load();
					AssertEquals("All transactions: Count", 6, testTransactions.Count);
					Assert("All transactions: Should contain inv1_HasRelatedFullyPaid", testTransactions.Contains(inv1_HasRelatedFullyPaid.PK));
					Assert("All transactions: Should contain inv2_HasRelatedNotFullyPaid", testTransactions.Contains(inv2_HasRelatedNotFullyPaid.PK));
					Assert("All transactions: Should contain inv3_HasPartiallyRelated", testTransactions.Contains(inv3_HasPartiallyRelated.PK));
					Assert("All transactions: Should contain inv4_Unrelated", testTransactions.Contains(inv4_Unrelated.PK));
					Assert("All transactions: Should contain inv5_HasRelatedDSBFullyPaid", testTransactions.Contains(inv5_HasRelatedDSBFullyPaid.PK));
					Assert("All transactions: Should contain inv6_HasRelatedDSBNotFullyPaid", testTransactions.Contains(inv6_HasRelatedDSBNotFullyPaid.PK));

					relatedTransactionsNotPaidFilter.Property = RelatedARTransactionsFullyPaid;
					testTransactions.Load(TestFilterBizO.Filter);
					AssertEquals("RelatedARTransactionsFullyPaid: Count", 2, testTransactions.Count);
					Assert("RelatedARTransactionsFullyPaid: Should contain inv1_HasRelatedFullyPaid", testTransactions.Contains(inv1_HasRelatedFullyPaid.PK));
					Assert("RelatedARTransactionsFullyPaid: Should contain inv5_HasRelatedDSBFullyPaid", testTransactions.Contains(inv5_HasRelatedDSBFullyPaid.PK));

					relatedTransactionsNotPaidFilter.Property = RelatedARTransactionsNotFullyPaid;
					testTransactions.Load(TestFilterBizO.Filter);
					AssertEquals("RelatedARTransactionsFullyPaid: Count", 2, testTransactions.Count);
					Assert("RelatedARTransactionsFullyPaid: Should contain inv2_HasRelatedNotFullyPaid", testTransactions.Contains(inv2_HasRelatedNotFullyPaid.PK));
					Assert("RelatedARTransactionsFullyPaid: Should contain inv6_HasRelatedDSBNotFullyPaid", testTransactions.Contains(inv6_HasRelatedDSBNotFullyPaid.PK));

					relatedTransactionsNotPaidFilter.Property = RelatedARDSBTransactionsFullyPaid;
					testTransactions.Load(TestFilterBizO.Filter);
					AssertEquals("RelatedARTransactionsFullyPaid: Count", 1, testTransactions.Count);
					Assert("RelatedARTransactionsFullyPaid: Should contain inv5_HasRelatedDSBFullyPaid", testTransactions.Contains(inv5_HasRelatedDSBFullyPaid.PK));

					relatedTransactionsNotPaidFilter.Property = RelatedARDSBTransactionsNotFullyPaid;
					testTransactions.Load(TestFilterBizO.Filter);
					AssertEquals("RelatedARTransactionsFullyPaid: Count", 1, testTransactions.Count);
					Assert("RelatedARTransactionsFullyPaid: Should contain inv6_HasRelatedDSBNotFullyPaid", testTransactions.Contains(inv6_HasRelatedDSBNotFullyPaid.PK));
				}
				else
				{
					relatedTransactionsNotPaidFilter.Property = string.Empty;
					TransactionHeaderCollection testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
					testTransactions.Load();
					AssertEquals("All transactions: Count", 5, testTransactions.Count);
					Assert("All transactions: Should contain inv1_HasRelatedFullyPaid", testTransactions.Contains(inv1_HasRelatedFullyPaid.PK));
					Assert("All transactions: Should contain inv2_HasRelatedNotFullyPaid", testTransactions.Contains(inv2_HasRelatedNotFullyPaid.PK));
					Assert("All transactions: Should contain inv3_HasPartiallyRelated", testTransactions.Contains(inv3_HasPartiallyRelated.PK));
					Assert("All transactions: Should contain inv4_Unrelated", testTransactions.Contains(inv4_Unrelated.PK));
					Assert("All transactions: Should contain headerToMatch", testTransactions.Contains(headerToMatch.PK));

					relatedTransactionsNotPaidFilter.Property = RelatedAPTransactionsFullyPaid;
					testTransactions.Load(TestFilterBizO.Filter);
					AssertEquals("RelatedARTransactionsFullyPaid: Count", 1, testTransactions.Count);
					Assert("RelatedARTransactionsFullyPaid: Should contain inv1_HasRelatedFullyPaid", testTransactions.Contains(inv1_HasRelatedFullyPaid.PK));

					relatedTransactionsNotPaidFilter.Property = RelatedAPTransactionsNotFullyPaid;
					testTransactions.Load(TestFilterBizO.Filter);
					AssertEquals("RelatedARTransactionsFullyPaid: Count", 1, testTransactions.Count);
					Assert("RelatedARTransactionsFullyPaid: Should contain inv2_HasRelatedNotFullyPaid", testTransactions.Contains(inv2_HasRelatedNotFullyPaid.PK));
				}
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestMatchStatusAndReasonFilter

		public void TestMatchStatusAndReasonFilter()
		{
			var testJournal = CreateNewJournal(Factory);
			var testJournal2 = CreateNewJournal(Factory);
			testJournal.AH_MatchStatus = ZString.Empty;
			testJournal.AH_MatchStatusReasonCode = "ADV";
			testJournal2.AH_MatchStatus = "UAC";
			testJournal2.AH_MatchStatusReasonCode = ZString.Empty;

			Factory.Save();

			var statusFilter = ((ModuleTextFilter)TestFilterBizO["Match Status"]);
			statusFilter.IsActive = true;
			statusFilter.Property = "UAC";
			var testTransactions = new TransactionHeaderCollection(Factory, TestFilterBizO.Filter);
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("Should contain TestJournal2 only", 1, testTransactions.Count);
			Assert("Should contain TestJournal2 only", testTransactions.Contains(testJournal2.PK));

			statusFilter.IsActive = false;
			var statusReasonFilter = ((ModuleTextFilter)TestFilterBizO["Match Status Reason"]);
			statusReasonFilter.IsActive = true;
			statusReasonFilter.Property = "ADV";
			testTransactions.Load(TestFilterBizO.Filter);
			AssertEquals("Should contain TestJournal only", 1, testTransactions.Count);
			Assert("Should contain TestJournal only", testTransactions.Contains(testJournal.PK));
		}

		#endregion

		#region TestTaxBranchFilter

		public void TestTaxBranchFilter()
		{
			var testCompany = TestObjectCreator.CreateNewCompany("ZZZ");
			var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("GHI", testCompany);
			var invoice1 = CreateNewInvoice(Factory);
			invoice1.AH_GB_TaxBranch = branch1.PK;
			var invoice2 = CreateNewInvoice(Factory);
			invoice2.AH_GB_TaxBranch = branch2.PK;

			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var testFilterBO = GetNewFilterStripBusinessObject();
			AssertNull("Should not have Tax Branch filter when the value of Registry item Enable Tax Branch Reporting is false.", (ModuleGuidFilter)testFilterBO["Tax Branch"]);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			testFilterBO = GetNewFilterStripBusinessObject();
			var taxBranchFilter = (ModuleGuidFilter)testFilterBO["Tax Branch"];
			AssertNotNull("Should have Tax Branch filter when the value of Registry item Enable Tax Branch Reporting is true.", taxBranchFilter);

			var branchList = taxBranchFilter.List as GlbBranchCollection;
			if (branchList != null)
			{
				branchList.Load();
				Assert("Branch list of Tax Branch filter should contain branch which belongs to current company", taxBranchFilter.List.Contains(branch1));
				Assert("Branch list of Tax Branch filter should contain branch which belongs to current company", taxBranchFilter.List.Contains(branch2));
				Assert("Branch list of Tax Branch filter should not contain branch which doesn't belong to current company", !(taxBranchFilter.List.Contains(branch3)));
			}

			taxBranchFilter.IsActive = true;
			taxBranchFilter.Property = branch1.PK;
			var testTransactions = new TransactionHeaderCollection(Factory, testFilterBO.Filter);
			testTransactions.Load();
			AssertEquals("Should only find one invoice", 1, testTransactions.Count);
			Assert("Should only find invoice1", testTransactions.Contains(invoice1.PK));

			taxBranchFilter.Property = branch2.PK;
			testTransactions.Load(testFilterBO.Filter);
			AssertEquals("Should only find one invoice", 1, testTransactions.Count);
			Assert("Should only find invoice2", testTransactions.Contains(invoice2.PK));
		}

		#endregion

		#endregion

		#region Validation

		#region TestValidateTransactionType

		public void TestValidateTransactionType()
		{
			ModuleTextFilter transactionTypeFilter = ((ModuleTextFilter)TestFilterBizO["Transaction Type"]);
			transactionTypeFilter.IsActive = true;
			Assert("Precondition: No errors for TransactionType", !transactionTypeFilter.HasNotifications());
			transactionTypeFilter.Property = ZArchitecture.Core.TransactionTypes.Contra;
			Assert("Valid TransactionType - should not cause errors", !transactionTypeFilter.HasNotifications());
		}

		#endregion

		#region TestValidatePaymentStatus

		public void TestValidatePaymentStatus()
		{
			ModuleTextFilter paymentStatusFilter = ((ModuleTextFilter)TestFilterBizO["Payment Status"]);
			paymentStatusFilter.IsActive = true;
			Assert("Precondition: No errors for PaymentStatus", !paymentStatusFilter.HasNotifications());
			paymentStatusFilter.Property = Business.AccountingUtils.PaymentStatusTypes.Unpaid;
			Assert("Valid PaymentStatus - should not cause errors", !paymentStatusFilter.HasNotifications());
		}

		#endregion

		#region TestJobNumberFilter_ComparisonOperator_List

		public void TestJobNumberFilter_ComparisonOperator_List()
		{
			ModuleNumberFilter jobNumberFilter = ((ModuleNumberFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.JobNumber]);
			AssertNotNull("JobNumber filter should exist", jobNumberFilter);
			AssertEquals("ComparisonOperator on JobNumberFilter should be defaulted to 'exact'", "exact", jobNumberFilter.ComparisonOperator);
			Assert("ComparisonOperator on JobNumberFilter should be read only", jobNumberFilter.ComparisonOperatorInfo.ReadOnly);
			jobNumberFilter.Clear();
			AssertEquals("A call to clear should not set default as 'starts with'", "exact", jobNumberFilter.ComparisonOperator);
		}

		#endregion

		#region TestNumberFiltersList

		public virtual void TestNumberFiltersList()
		{
			ModuleNumberFilter numberFilter = ((ModuleNumberFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.JobNumber]);
			AssertNotNull("JobNumber filter should exist", numberFilter);
			Assert("JobNumber filter not is common", !numberFilter.IsCommon);
			numberFilter = ((ModuleNumberFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.ChequeReferenceNumber]);
			AssertNotNull("ChequeReferenceNumber filter should exist", numberFilter);
			Assert("ChequeReferenceNumber filter not is common", !numberFilter.IsCommon);
			numberFilter = ((ModuleNumberFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.DepositBatchNumber]);
			AssertNotNull("DepositBatchNumber filter should exist", numberFilter);
			Assert("DepositBatchNumber filter not is common", !numberFilter.IsCommon);
			numberFilter = ((ModuleNumberFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.DDRBatchNumber]);
			AssertNotNull("DDRBatchNumber filter should exist", numberFilter);
			Assert("DDRBatchNumber filter not is common", !numberFilter.IsCommon);
			numberFilter = ((ModuleNumberFilter)TestFilterBizO["All Numbers"]);
			Assert("All Numbers filter not is common", !numberFilter.IsCommon);
			AssertNotNull("All Numbers filter should exist", numberFilter);
			numberFilter = ((ModuleNumberFilter)TestFilterBizO[Business.AccountingUtils.NumberFilterTypes.InvoiceRemittanceReference]);
			AssertNotNull("InvoiceRemittanceReference filter should exist", numberFilter);
			Assert("InvoiceRemittanceReference filter not is common", !numberFilter.IsCommon);
		}

		#endregion

		#endregion

		#region Lookups

		#region TestOrgSettlementGroup_List

		public void TestOrgSettlementGroup_List()
		{
			Assert("OrgSettlementGroup_List should be of type OrganisationsFindBoxCollection", TestFilterBizO.OrgSettlementGroup_List is OrganisationsFindBoxCollection);
		}

		#endregion

		#region TestInactiveOrganisationsIncludedInList

		public void TestInactiveOrganisationsIncludedInList()
		{
			OrgHeader inactiveOrg = Factory.NewWithValidTestData<OrgHeader>();
			inactiveOrg.OH_IsActive = false;
			if (TestFilterBizO.UseDebtor_ForTestOnly)
			{
				inactiveOrg.OH_IsDebtor = true;
			}
			else
			{
				inactiveOrg.OH_IsCreditor = true;
			}
			Factory.Save();

			OrgHeader activeOrg = Factory.NewWithValidTestData<OrgHeader>();
			activeOrg.OH_IsActive = true;
			if (TestFilterBizO.UseDebtor_ForTestOnly)
			{
				activeOrg.OH_IsDebtor = true;
			}
			else
			{
				activeOrg.OH_IsCreditor = true;
			}
			Factory.Save();

			TestFilterBizO.AH_OHList_ForTestOnly.Load();
			Assert("Org list should contain the Inactive Org", TestFilterBizO.AH_OHList_ForTestOnly.Contains(inactiveOrg));
			Assert("Org list should contain the Active Org", TestFilterBizO.AH_OHList_ForTestOnly.Contains(activeOrg));
		}

		#endregion

		#region RelatedTransactionsNotPaidFilterOptionsList

		public void TestRelatedTransactionsNotPaidFilterOptionsList()
		{
			string[] expectedRelatedTransactionsNotPaidFilterOptionsListMembers = TestFilterBizO.IsPayableModule ? new[] { RelatedARTransactionsFullyPaid, RelatedARTransactionsNotFullyPaid, RelatedARDSBTransactionsFullyPaid, RelatedARDSBTransactionsNotFullyPaid } : new[] { RelatedAPTransactionsFullyPaid, RelatedAPTransactionsNotFullyPaid };

			AssertEquals("Number of elements", expectedRelatedTransactionsNotPaidFilterOptionsListMembers.Length, TestFilterBizO.RelatedTransactionsNotPaidFilterOptionsList.Count);
			foreach (string element in expectedRelatedTransactionsNotPaidFilterOptionsListMembers)
			{
				Assert(string.Format("List should contain '{0}'", element), TestFilterBizO.RelatedTransactionsNotPaidFilterOptionsList.ContainsCode(element));
			}
		}

		#endregion

		#endregion

		#region Implementation

		protected abstract Invoice CreateNewInvoiceOfOtherLedger(BusinessObjectFactory factory);
		protected abstract Receipt CreateNewReceipt(BusinessObjectFactory factory);
		protected abstract AdjustmentNote CreateNewAdjustmentNote(BusinessObjectFactory factory);
		protected abstract Payment CreateNewPayment(BusinessObjectFactory factory);
		protected abstract CreditNote CreateNewCreditNote(BusinessObjectFactory factory);
		protected abstract Journal CreateNewJournal(BusinessObjectFactory factory);

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(JobCharge.Schema.TableName);
			TestCaseHelper.ClearTable(AccTransactionLines.Schema.TableName);
			TestCaseHelper.ClearTable(AccTransactionMatchLink.Schema.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeader.Schema.TableName);
		}

		Invoice Invoice;
		Invoice Invoice2;
		Invoice Invoice3;
		Invoice Invoice4;
		CreditNote Crd;
		AdjustmentNote Adj;
		protected Job Job;
		Job Job2;
		Job Job3;
		Job Job4;

		void PrepareForJobNumberFilteringTest()
		{
			Job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.JobReadyForCostPosting.Code);
			Job2 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.JobReadyForCostPosting.Code);
			Job3 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.JobReadyForCostPosting.Code);
			Factory.Save();

			Invoice = CreateNewInvoice(Factory);
			Invoice.AH_ConsolidatedInvoiceRef = Job3.JH_JobNum; //Should not affect the results as Job # filter doesn't filter by this field
			Invoice.AH_OH = TestOrg.PK;
			InvoicingLineBase invoicingLine = (InvoicingLineBase)Invoice.Lines.AddNew();
			invoicingLine.AL_JH = Job.PK;
			invoicingLine.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			TestObjectCreator.CreateJobCharge(invoicingLine, Job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			InvoicingLineBase invoicingLine2 = (InvoicingLineBase)Invoice.Lines.AddNew();
			invoicingLine2.AL_JH = Job2.PK;
			invoicingLine2.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			TestObjectCreator.CreateJobCharge(invoicingLine2, Job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Crd = CreateNewCreditNote(Factory);
			Crd.AH_ConsolidatedInvoiceRef = Job.JH_JobNum;
			Crd.AH_OH = TestOrg.PK;
			InvoicingLineBase crdLine = (InvoicingLineBase)Crd.Lines.AddNew();
			crdLine.AL_JH = Job2.PK;
			crdLine.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			TestObjectCreator.CreateJobCharge(crdLine, Job2, TestObjectCreator.CC1, TestObjectCreator.AUD);
			InvoicingLineBase crdLine2 = (InvoicingLineBase)Crd.Lines.AddNew();
			crdLine2.AL_JH = Job.PK;
			crdLine2.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			TestObjectCreator.CreateJobCharge(crdLine2, Job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			Adj = CreateNewAdjustmentNote(Factory);
			Adj.AH_ConsolidatedInvoiceRef = Job.JH_JobNum;
			Adj.AH_OH = TestOrg.PK;
			InvoicingLineBase adjLine = (InvoicingLineBase)Adj.Lines.AddNew();
			adjLine.AL_JH = Job3.PK;
			adjLine.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			TestObjectCreator.CreateJobCharge(adjLine, Job3, TestObjectCreator.CC1, TestObjectCreator.AUD);
		}

		protected OrgHeader TestOrg
		{
			get
			{
				if (fTestOrg == null)
				{
					fTestOrg = Factory.NewWithValidTestData<OrgHeader>();
				}
				return fTestOrg;
			}
		}
		protected OrgHeader fTestOrg;

		const string RelatedARTransactionsFullyPaid = "ARPAID";
		const string RelatedARTransactionsNotFullyPaid = "AROPEN";
		const string RelatedARDSBTransactionsFullyPaid = "DSBPAID";
		const string RelatedARDSBTransactionsNotFullyPaid = "DSB AR NOT";
		const string RelatedAPTransactionsFullyPaid = "APPAID";
		const string RelatedAPTransactionsNotFullyPaid = "APOPEN";

		#endregion

		#region Helper

		protected class GetOrganizationAndAddressFilterCommonScenario
		{
			public GetOrganizationAndAddressFilterCommonScenarioForLedgerType AR;
			public GetOrganizationAndAddressFilterCommonScenarioForLedgerType AP;
		}

		protected class GetOrganizationAndAddressFilterCommonScenarioForLedgerType
		{
			public OrgHeader org;
			public OrgAddress defaultAddress, otherAddress;
			public InvoicingBase invoiceNoAddr, invoiceDefaultAddr, invoiceOtherAddr, invoiceDefaultViaJobAddr, invoiceDefaultToOtherViaJobAddr;
		}

		protected GetOrganizationAndAddressFilterCommonScenario SetupGetOrganizationAndAddressFilterCommonScenario()
		{
			int uniqueId = 1;
			var result = new GetOrganizationAndAddressFilterCommonScenario();

			foreach (bool isAR in new[] { true, false })
			{
				var invoiceType = isAR ? typeof(ARInvoice) : typeof(APInvoice);

				var org = TestObjectCreator.AALSHI;
				var otherAddress = org.Addresses.AddNew();
				otherAddress.OA_Code = "OTHER";
				otherAddress.OA_Address1 = "3.14 Pi St";
				otherAddress.OA_City = "London";
				var defaultAddress = isAR ? TestObjectCreator.AALSHI.AddressForSendingARDocuments : TestObjectCreator.AALSHI.AddressForSendingAPDocuments;
				AssertNotEquals("precondition", otherAddress, defaultAddress);

				var oTHER_org = TestObjectCreator.ABIGAS;
				var oTHER_defaultAddress = defaultAddress;
				var oTHER_otherAddress = org.Addresses.AddNew();
				oTHER_otherAddress.OA_Code = "OTHER";
				oTHER_otherAddress.OA_Address1 = "3.14 Pi St";
				oTHER_otherAddress.OA_City = "London";
				AssertNotEquals("precondition", oTHER_otherAddress, oTHER_defaultAddress);

				var invoiceNoAddr = TestObjectCreator.CreateInvoiceWithLine(invoiceType, (uniqueId++).ToString(), TestObjectCreator.AUD, 1, 100, 0, 100, 0, org, ZGuid.Empty);
				invoiceNoAddr.AH_OA_InvoiceAddressOverride = ZGuid.Empty;

				var invoiceDefaultAddr = TestObjectCreator.CreateInvoiceWithLine(invoiceType, (uniqueId++).ToString(), TestObjectCreator.AUD, 1, 100, 0, 100, 0, org, ZGuid.Empty);
				invoiceDefaultAddr.AH_OA_InvoiceAddressOverride = defaultAddress.PK;

				var invoiceOtherAddr = TestObjectCreator.CreateInvoiceWithLine(invoiceType, (uniqueId++).ToString(), TestObjectCreator.AUD, 1, 100, 0, 100, 0, org, ZGuid.Empty);
				invoiceOtherAddr.AH_OA_InvoiceAddressOverride = otherAddress.PK;

				var invoiceDefaultViaJobAddr = TestObjectCreator.CreateInvoice(invoiceType, (uniqueId++).ToString(), TestObjectCreator.AUD, 1, org);
				var invoiceDefaultViaJobAddr_Line = TestObjectCreator.CreateShipmentChargeJobAndLine((uniqueId++).ToString(), invoiceDefaultViaJobAddr, org, isAR);
				invoiceDefaultViaJobAddr.AH_OA_InvoiceAddressOverride = ZGuid.Empty;
				invoiceDefaultViaJobAddr_Line.Job.JH_OA_LocalChargesAddr = defaultAddress.PK;

				var invoiceDefaultToOtherViaJobAddr = TestObjectCreator.CreateInvoice(invoiceType, (uniqueId++).ToString(), TestObjectCreator.AUD, 1, org);
				var invoiceDefaultToOtherViaJobAddr_Line = TestObjectCreator.CreateShipmentChargeJobAndLine((uniqueId++).ToString(), invoiceDefaultToOtherViaJobAddr, org, isAR);
				invoiceDefaultToOtherViaJobAddr.AH_OA_InvoiceAddressOverride = ZGuid.Empty;
				invoiceDefaultToOtherViaJobAddr_Line.Job.JH_OA_LocalChargesAddr = otherAddress.PK;

				// A mirror setup with another organisation, created to test that the queries ignore these
				var oTHER_invoiceNoAddr = TestObjectCreator.CreateInvoiceWithLine(invoiceType, (uniqueId++).ToString(), TestObjectCreator.AUD, 1, 100, 0, 100, 0, oTHER_org, ZGuid.Empty);
				oTHER_invoiceNoAddr.AH_OA_InvoiceAddressOverride = ZGuid.Empty;

				var oTHER_invoiceDefaultAddr = TestObjectCreator.CreateInvoiceWithLine(invoiceType, (uniqueId++).ToString(), TestObjectCreator.AUD, 1, 100, 0, 100, 0, oTHER_org, ZGuid.Empty);
				oTHER_invoiceDefaultAddr.AH_OA_InvoiceAddressOverride = oTHER_defaultAddress.PK;

				var oTHER_invoiceOtherAddr = TestObjectCreator.CreateInvoiceWithLine(invoiceType, (uniqueId++).ToString(), TestObjectCreator.AUD, 1, 100, 0, 100, 0, oTHER_org, ZGuid.Empty);
				oTHER_invoiceOtherAddr.AH_OA_InvoiceAddressOverride = oTHER_otherAddress.PK;

				var oTHER_invoiceDefaultViaJobAddr = TestObjectCreator.CreateInvoice(invoiceType, (uniqueId++).ToString(), TestObjectCreator.AUD, 1, oTHER_org);
				var oTHER_invoiceDefaultViaJobAddr_Line = TestObjectCreator.CreateShipmentChargeJobAndLine((uniqueId++).ToString(), oTHER_invoiceDefaultViaJobAddr, oTHER_org, isAR);
				oTHER_invoiceDefaultViaJobAddr.AH_OA_InvoiceAddressOverride = ZGuid.Empty;
				oTHER_invoiceDefaultViaJobAddr_Line.Job.JH_OA_LocalChargesAddr = oTHER_defaultAddress.PK;

				var oTHER_invoiceDefaultToOtherViaJobAddr = TestObjectCreator.CreateInvoice(invoiceType, (uniqueId++).ToString(), TestObjectCreator.AUD, 1, oTHER_org);
				var oTHER_invoiceDefaultToOtherViaJobAddr_Line = TestObjectCreator.CreateShipmentChargeJobAndLine((uniqueId++).ToString(), oTHER_invoiceDefaultToOtherViaJobAddr, oTHER_org, isAR);
				oTHER_invoiceDefaultToOtherViaJobAddr.AH_OA_InvoiceAddressOverride = ZGuid.Empty;
				oTHER_invoiceDefaultToOtherViaJobAddr_Line.Job.JH_OA_LocalChargesAddr = oTHER_otherAddress.PK;

				Factory.Save();

				AssertEquals("precondition", org.PK, invoiceDefaultViaJobAddr_Line.Job.LocalCharges.PK);
				AssertEquals("precondition", org.PK, invoiceDefaultToOtherViaJobAddr_Line.Job.LocalCharges.PK);

				var scenarioForLedger = new GetOrganizationAndAddressFilterCommonScenarioForLedgerType
				{
					defaultAddress = defaultAddress,
					invoiceDefaultAddr = invoiceDefaultAddr,
					invoiceDefaultToOtherViaJobAddr = invoiceDefaultToOtherViaJobAddr,
					invoiceDefaultViaJobAddr = invoiceDefaultViaJobAddr,
					invoiceNoAddr = invoiceNoAddr,
					invoiceOtherAddr = invoiceOtherAddr,
					org = org,
					otherAddress = otherAddress
				};

				if (isAR)
				{
					result.AR = scenarioForLedger;
				}
				else
				{
					result.AP = scenarioForLedger;
				}
			}
			return result;
		}

		#endregion
	}
}
