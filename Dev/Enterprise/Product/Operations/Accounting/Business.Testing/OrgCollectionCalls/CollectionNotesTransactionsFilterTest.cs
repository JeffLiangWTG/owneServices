using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.OrgCollectionCalls.Testing
{
	using Enterprise.Accounting.Business.ARAP;
	using Enterprise.Accounting.Business.ARAP.Journal;
	using Enterprise.Accounting.Business.ARAP.Overpayment;
	using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
	using NUnit.Framework;

	[TestedType(typeof(CollectionNotesTransactionsFilter))]
	public class CollectionNotesTransactionsFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CollectionNotesTransactionsFilter(Factory.NewWithValidTestData<OrgHeader>(), GlbBranch.CurrentBranch);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesCollectionNotesTransactionsFilter()
		{
			TransactionsFilter = new CollectionNotesTransactionsFilter(Organisation, GlbBranch.CurrentBranch);

			var localList = new List<string>
			{
				nameof(TransactionsFilter.StandardNotYetDue),
				nameof(TransactionsFilter.StandardDueToday),
				nameof(TransactionsFilter.StandardOneTermPastDue),
				nameof(TransactionsFilter.StandardTwoTermsPastDue),
				nameof(TransactionsFilter.StandardOverTwoTermsPastDue),
				nameof(TransactionsFilter.StandardOutstanding),
				nameof(TransactionsFilter.DisbursementNotYetDue),
				nameof(TransactionsFilter.DisbursementDueToday),
				nameof(TransactionsFilter.DisbursementOneTermPastDue),
				nameof(TransactionsFilter.DisbursementTwoTermsPastDue),
				nameof(TransactionsFilter.DisbursementOverTwoTermsPastDue),
				nameof(TransactionsFilter.DisbursementOutstanding),
				nameof(TransactionsFilter.TotalNotYetDue),
				nameof(TransactionsFilter.TotalDueToday),
				nameof(TransactionsFilter.TotalOneTermPastDue),
				nameof(TransactionsFilter.TotalTwoTermsPastDue),
				nameof(TransactionsFilter.TotalOverTwoTermsPastDue),
			};

			var tester = new DecimalPlacesAttributeTester(TransactionsFilter);
			tester.CheckLocalCurrency(localList, nameof(TransactionsFilter.Decimals));
		}

		public void TestFilterDoesNotIncludeInvoiceBatches()
		{
			ARInvoice invoiceOne = Factory.NewWithValidTestData<ARInvoice>();
			invoiceOne.AH_OH = Organisation.PK;
			ARInvoiceLine invoiceLineOne = Factory.NewWithValidTestData<ARInvoiceLine>();
			invoiceLineOne.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoiceLineOne.AL_OSExTaxAmount = 10m;
			invoiceOne.Lines.Add(invoiceLineOne);
			ARInvoice invoiceTwo = Factory.NewWithValidTestData<ARInvoice>();
			invoiceTwo.AH_OH = Organisation.PK;
			ARInvoiceLine invoiceLineTwo = Factory.NewWithValidTestData<ARInvoiceLine>();
			invoiceLineTwo.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoiceLineTwo.AL_OSExTaxAmount = 20m;
			invoiceTwo.Lines.Add(invoiceLineTwo);
			Factory.Save();
			InvoiceBatchHeader invoiceBatchHeader = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			invoiceBatchHeader.AH_OH = Organisation.PK;
			invoiceBatchHeader.Line.Add(invoiceOne);
			invoiceBatchHeader.Line.Add(invoiceTwo);
			Factory.Save();
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Collection should contain only 2 AR Invoices", 2, TransactionsFilter.Transactions.Count);
			Assert("Collection should contain ARInv1", TransactionsFilter.Transactions.Contains(invoiceOne.PK));
			Assert("Collection should contain ARInv1", TransactionsFilter.Transactions.Contains(invoiceTwo.PK));
			Assert("Collection should NOT contain Invoice Batch", !TransactionsFilter.Transactions.Contains(invoiceBatchHeader.PK));
		}

		public void TestDateFilter()
		{
			ARJournal testJournal1 = Factory.New<ARJournal>();
			testJournal1.AH_PostDate = new ZDateTime(2004, 2, 3);
			testJournal1.AH_OH = Organisation.PK;
			ARJournal testJournal2 = Factory.New<ARJournal>();
			testJournal2.AH_InvoiceDate = new ZDateTime(2004, 4, 5);
			testJournal2.AH_OH = Organisation.PK;
			ARReceipt testReceipt1 = Factory.New<ARReceipt>();
			testReceipt1.AH_DueDate = new ZDateTime(2004, 3, 5);
			testReceipt1.AH_OH = Organisation.PK;
			ARReceipt testReceipt2 = Factory.New<ARReceipt>();
			testReceipt2.AH_DueDate = new ZDateTime(2004, 4, 6);
			testReceipt2.AH_PostDate = new ZDateTime(2004, 1, 6);
			testReceipt2.AH_OH = Organisation.PK;
			Factory.Save();
			TransactionsFilter.AH_FromDate = new ZDateTime(2004, 2, 3);
			TransactionsFilter.AH_ToDate = new ZDateTime(2004, 4, 5);
			TransactionsFilter.AH_DateFilter = AccountingUtils.DateFilterTypes.PostDate;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Collection should contain only TestJournal1", 1, TransactionsFilter.Transactions.Count);
			Assert("Collection should contain only TestJournal1", TransactionsFilter.Transactions.Contains(testJournal1.PK));
			TransactionsFilter.AH_DateFilter = AccountingUtils.DateFilterTypes.TransactionDate;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Collection should contain only TestJournal2", 1, TransactionsFilter.Transactions.Count);
			Assert("Collection should contain only TestJournal2", TransactionsFilter.Transactions.Contains(testJournal2.PK));
			TransactionsFilter.AH_DateFilter = AccountingUtils.DateFilterTypes.DueDate;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Collection should contain only TestReceipt1", 1, TransactionsFilter.Transactions.Count);
			Assert("Collection should contain only TestReceipt1", TransactionsFilter.Transactions.Contains(testReceipt1.PK));
			TransactionsFilter.AH_DateFilter = AccountingUtils.DateFilterTypes.All;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Collection should contain 3 transactions", 3, TransactionsFilter.Transactions.Count);
			Assert("Collection should contain TestJournal1", TransactionsFilter.Transactions.Contains(testJournal1.PK));
			Assert("Collection should contain TestJournal2", TransactionsFilter.Transactions.Contains(testJournal2.PK));
			Assert("Collection should contain TestReceipt1", TransactionsFilter.Transactions.Contains(testReceipt1.PK));
			TransactionsFilter.AH_DateFilter = AccountingUtils.DateFilterTypes.None;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Collection should contain 4 transactions", 4, TransactionsFilter.Transactions.Count);
		}

		public void TestPaymentStatusPanelFilter()
		{
			ARJournal testARJournal = Factory.NewWithValidTestData<ARJournal>();
			ARJournal testARJournal2 = Factory.NewWithValidTestData<ARJournal>();
			testARJournal.AH_FullyPaidDate = ZDateTime.Empty;
			testARJournal2.AH_FullyPaidDate = ZDateTime.Now;
			testARJournal.AH_OH = Organisation.PK;
			testARJournal2.AH_OH = Organisation.PK;
			testARJournal.AH_OutstandingAmount = 100m;
			testARJournal.AH_InvoiceAmount = 100m;
			testARJournal.AH_OSTotal = 100m;
			testARJournal2.AH_OutstandingAmount = 100m;
			testARJournal2.AH_InvoiceAmount = 100m;
			testARJournal2.AH_OSTotal = 100m;
			Factory.Save();
			TransactionsFilter.PaymentStatus = "ALL";
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Should capture all the transactions", 2, TransactionsFilter.Transactions.Count);
			Assert("Should contain TestARJournal", TransactionsFilter.Transactions.Contains(testARJournal.PK));
			Assert("SHould contain TestARJournal2", TransactionsFilter.Transactions.Contains(testARJournal2.PK));
			TransactionsFilter.PaymentStatus = AccountingUtils.PaymentStatusTypes.Unpaid;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Should contain TestARJournal only", 1, TransactionsFilter.Transactions.Count);
			Assert("Should contain TestARJournal only", TransactionsFilter.Transactions.Contains(testARJournal.PK));
		}

		public void TestTransactionNumberFiltering()
		{
			ARInvoice aRInv1 = Factory.NewWithValidTestData<ARInvoice>();
			aRInv1.AH_OH = Organisation.PK;
			ARCreditNote aRCrd1 = Factory.NewWithValidTestData<ARCreditNote>();
			aRCrd1.AH_OH = Organisation.PK;
			ARAdjustmentNote aRAdj = Factory.NewWithValidTestData<ARAdjustmentNote>();
			aRAdj.AH_OH = Organisation.PK;
			aRInv1.AH_TransactionNum = "00001848";
			aRInv1.IsManuallySetTransactionNumber_ForTestOnly = true;
			aRCrd1.AH_TransactionNum = "00003185";
			aRCrd1.IsManuallySetTransactionNumber_ForTestOnly = true;
			aRAdj.AH_TransactionNum = "00185000";
			aRAdj.IsManuallySetTransactionNumber_ForTestOnly = true;
			Factory.Save();
			TransactionsFilter.AH_Number = "185";
			TransactionsFilter.AH_NumberFilter = AccountingUtils.NumberFilterTypes.TransactionNumber;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 2 transactions in the collection", 2, TransactionsFilter.Transactions.Count);
			Assert("Collection should contain ARCrd1", TransactionsFilter.Transactions.Contains(aRCrd1));
			Assert("Collection should contain ARAdj1", TransactionsFilter.Transactions.Contains(aRAdj));
			TransactionsFilter.AH_Number = "0000";
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 2 transactions in the collection", 2, TransactionsFilter.Transactions.Count);
			Assert("Collection should contain ARInv", TransactionsFilter.Transactions.Contains(aRInv1));
			Assert("Collection should contain ARCrd", TransactionsFilter.Transactions.Contains(aRCrd1));
			TransactionsFilter.AH_Number = "000019";
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Collection should be empty", 0, TransactionsFilter.Transactions.Count);
			TransactionsFilter.AH_Number = "";
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 3 transactions in the collection", 3, TransactionsFilter.Transactions.Count);
		}

		public void TestJobNumberFiltering()
		{
			JobHeader job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_JobNum = "S00001890";
			JobHeader job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_JobNum = "S00010189";
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_OH = Organisation.PK;
			aRInv.AH_JH = job1.PK;
			ARInvoiceLine aRInvLine1 = (ARInvoiceLine)aRInv.Lines.AddNew();
			aRInvLine1.AL_JH = job1.PK;
			aRInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CreateJobCharge(aRInvLine1, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
			ARCreditNote aRCrd = Factory.NewWithValidTestData<ARCreditNote>();
			aRCrd.AH_OH = Organisation.PK;
			aRCrd.AH_JH = job2.PK;
			ARCreditNoteLine aRCrdLine1 = (ARCreditNoteLine)aRCrd.Lines.AddNew();
			aRCrdLine1.AL_JH = job2.PK;
			aRCrdLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CreateJobCharge(aRCrdLine1, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);
			ARAdjustmentNote aRAdj = Factory.NewWithValidTestData<ARAdjustmentNote>();
			aRAdj.AH_OH = Organisation.PK;
			Factory.Save();
			TransactionsFilter.AH_Number = "189";
			TransactionsFilter.AH_NumberFilter = AccountingUtils.NumberFilterTypes.JobNumber;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 2 matching transactions", 2, TransactionsFilter.Transactions.Count);
			Assert("ARInv should be included", TransactionsFilter.Transactions.Contains(aRInv));
			Assert("ARCrd should be included", TransactionsFilter.Transactions.Contains(aRCrd));
			TransactionsFilter.AH_Number = "S0001";
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 1 matching transaction", 1, TransactionsFilter.Transactions.Count);
			Assert("ARCrd should be included", TransactionsFilter.Transactions.Contains(aRCrd));
			TransactionsFilter.AH_Number = "000123";
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Collection should be empty", 0, TransactionsFilter.Transactions.Count);
			TransactionsFilter.AH_Number = "";
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 3 matching transactions", 3, TransactionsFilter.Transactions.Count);
		}

		public void TestGovtTaxInvoiceNumberFilter()
		{
			GlbBranch newBranch = SetupNewChineseCompanyAndBranch();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				TransactionsFilter = new CollectionNotesTransactionsFilter(Organisation, GlbBranch.CurrentBranch);
				TransactionsFilter.PaymentStatus = "ALL";
				ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
				aRInv.AH_TransactionReference = "S00010189";
				aRInv.AH_OH = Organisation.PK;
				ARCreditNote aRCrd = Factory.NewWithValidTestData<ARCreditNote>();
				aRCrd.AH_TransactionReference = "S01018900";
				aRCrd.AH_OH = Organisation.PK;
				TransactionsFilter.AH_Number = "10189";
				TransactionsFilter.AH_NumberFilter = AccountingUtils.NumberFilterTypes.GovtComplianceNumber;
				TransactionsFilter.RefreshInvoiceList();
				AssertEquals("There should be 2 matching transactions", 2, TransactionsFilter.Transactions.Count);
				Assert("ARCrd should match", TransactionsFilter.Transactions.Contains(aRCrd));
				Assert("ARInv should match", TransactionsFilter.Transactions.Contains(aRInv));
				TransactionsFilter.AH_Number = "S010";
				TransactionsFilter.RefreshInvoiceList();
				AssertEquals("There should be 1 matching transaction", 1, TransactionsFilter.Transactions.Count);
				Assert("ARCrd should match", TransactionsFilter.Transactions.Contains(aRCrd));
				TransactionsFilter.AH_Number = "55";
				TransactionsFilter.RefreshInvoiceList();
				AssertEquals("The collection should be empty", 0, TransactionsFilter.Transactions.Count);
				TransactionsFilter.AH_Number = "";
				TransactionsFilter.RefreshInvoiceList();
				AssertEquals("There should be 2 matching transactions", 2, TransactionsFilter.Transactions.Count);
			}
		}

		public void TestJobInvNumberFilter()
		{
			ARInvoice aRInv1 = Factory.NewWithValidTestData<ARInvoice>();
			aRInv1.AH_ConsolidatedInvoiceRef = "00001848";
			aRInv1.AH_OH = Organisation.PK;
			ARCreditNote aRCrd1 = Factory.NewWithValidTestData<ARCreditNote>();
			aRCrd1.AH_ConsolidatedInvoiceRef = "00003185";
			aRCrd1.AH_OH = Organisation.PK;
			ARAdjustmentNote aRAdj = Factory.NewWithValidTestData<ARAdjustmentNote>();
			aRAdj.AH_ConsolidatedInvoiceRef = "00185000";
			aRAdj.AH_OH = Organisation.PK;
			Factory.Save();
			TransactionsFilter.AH_Number = "185";
			TransactionsFilter.AH_NumberFilter = AccountingUtils.NumberFilterTypes.ConsolidationNumber;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 2 transactions in the collection", 2, TransactionsFilter.Transactions.Count);
			Assert("Collection should contain ARCrd1", TransactionsFilter.Transactions.Contains(aRCrd1));
			Assert("Collection should contain ARAdj1", TransactionsFilter.Transactions.Contains(aRAdj));
			TransactionsFilter.AH_Number = "0000";
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 2 transactions in the collection", 2, TransactionsFilter.Transactions.Count);
			Assert("Collection should contain ARInv", TransactionsFilter.Transactions.Contains(aRInv1));
			Assert("Collection should contain ARCrd", TransactionsFilter.Transactions.Contains(aRCrd1));
			TransactionsFilter.AH_Number = "000019";
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Collection should be empty", 0, TransactionsFilter.Transactions.Count);
			TransactionsFilter.AH_Number = "";
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 3 transactions in the collection", 3, TransactionsFilter.Transactions.Count);
		}

		public void TestTransactionTypeFilter()
		{
			ARAdjustmentNote testAdjustmentNote = Factory.NewWithValidTestData<ARAdjustmentNote>();
			testAdjustmentNote.AH_OH = Organisation.PK;
			Contra testContra = Contra.New(Factory);
			testContra.ARRow.AH_OH = Organisation.PK;
			ARCreditNote testCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			testCreditNote.AH_OH = Organisation.PK;
			ARDiscount testDiscount = Factory.NewWithValidTestData<ARDiscount>();
			testDiscount.AH_OH = Organisation.PK;
			ARExchangeDifference testExchDifference = Factory.NewWithValidTestData<ARExchangeDifference>();
			testExchDifference.AH_OH = Organisation.PK;
			ARInvoice testInvoice = Factory.NewWithValidTestData<ARInvoice>();
			testInvoice.AH_OH = Organisation.PK;
			ARJournal testJournal = Factory.NewWithValidTestData<ARJournal>();
			testJournal.AH_OH = Organisation.PK;
			AROverpayment overpayment = Factory.NewWithValidTestData<AROverpayment>();
			overpayment.AH_OH = Organisation.PK;
			ARPayment testPayment = Factory.NewWithValidTestData<ARPayment>();
			testPayment.AH_OH = Organisation.PK;
			ARReceipt testReceipt = Factory.NewWithValidTestData<ARReceipt>();
			testReceipt.AH_OH = Organisation.PK;
			APReceipt testReceipt2 = Factory.NewWithValidTestData<APReceipt>();
			testReceipt2.AH_OH = Organisation.PK;
			ARTransfer testTransfer = (ARTransfer)ARTransfer.New(typeof(ARTransfer), Factory);
			testTransfer.TransferFrom.AH_OH = Organisation.PK;
			Factory.Save();
			TransactionsFilter.AH_TransactionType = "ALL";
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Collection should contain all 11 transactions", 11, TransactionsFilter.Transactions.Count);
			TestLoadTransactionsWithAH_TransactionTypeSpecified("ADJ", "TestAdjustmentNote", testAdjustmentNote.PK);
			TestLoadTransactionsWithAH_TransactionTypeSpecified("CTR", "TestContra", testContra.ARRow.PK);
			TestLoadTransactionsWithAH_TransactionTypeSpecified("CRD", "TestCreditNote", testCreditNote.PK);
			TestLoadTransactionsWithAH_TransactionTypeSpecified("DSC", "TestDiscount", testDiscount.PK);
			TestLoadTransactionsWithAH_TransactionTypeSpecified("EXX", "TestExchDifference", testExchDifference.PK);
			TestLoadTransactionsWithAH_TransactionTypeSpecified("INV", "TestInvoice", testInvoice.PK);
			TestLoadTransactionsWithAH_TransactionTypeSpecified("JNL", "TestJournal", testJournal.PK);
			TestLoadTransactionsWithAH_TransactionTypeSpecified("OVP", "Overpayment", overpayment.PK);
			TestLoadTransactionsWithAH_TransactionTypeSpecified("PAY", "TestPayment", testPayment.PK);
			TestLoadTransactionsWithAH_TransactionTypeSpecified("REC", "TestReceipt", testReceipt.PK);
			TestLoadTransactionsWithAH_TransactionTypeSpecified("TRF", "ARTrRow", testTransfer.TransferFrom.PK);
		}

		void TestLoadTransactionsWithAH_TransactionTypeSpecified(ZString transactionType, ZString objectToTestName, ZGuid pKForCheck)
		{
			TransactionsFilter.AH_TransactionType = transactionType;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Collection should contain only " + objectToTestName, 1, TransactionsFilter.Transactions.Count);
			Assert("Collection should contain only " + objectToTestName, TransactionsFilter.Transactions.Contains(pKForCheck));
		}

		public void TestCurrencyFilter()
		{
			ARJournal testJournal = Factory.NewWithValidTestData<ARJournal>();
			testJournal.AH_OH = Organisation.PK;
			testJournal.AH_RX_NKTransactionCurrency = TestHelper.AUD.RX_Code;
			testJournal.AH_ExchangeRate = 1m;
			ARReceipt testReceipt = Factory.NewWithValidTestData<ARReceipt>();
			testReceipt.AH_OH = Organisation.PK;
			testReceipt.AH_RX_NKTransactionCurrency = TestHelper.USD.RX_Code;
			testReceipt.AH_ExchangeRate = 1m;
			Factory.Save();
			TransactionsFilter.AH_RX_NKTransactionCurrency = TestHelper.AUD.RX_Code;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 1 transaction in the collection", 1, TransactionsFilter.Transactions.Count);
			Assert("The collection should contain the TestJournal", TransactionsFilter.Transactions.Contains(testJournal));
			RefCurrency rUR = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "RUR");
			TransactionsFilter.AH_RX_NKTransactionCurrency = rUR.RX_Code;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There collection should be empty", 0, TransactionsFilter.Transactions.Count);
			TransactionsFilter.AH_RX_NKTransactionCurrency = ZString.Empty;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 2 transactions in the collection", 2, TransactionsFilter.Transactions.Count);
			Assert("The collection should contain the TestJournal", TransactionsFilter.Transactions.Contains(testJournal));
			Assert("The collection should contain the TestReceipt", TransactionsFilter.Transactions.Contains(testReceipt));
		}

		public void TestBranchFilter()
		{
			ARJournal testJournal = Factory.NewWithValidTestData<ARJournal>();
			testJournal.AH_OH = Organisation.PK;
			testJournal.AH_GB = GlbBranch.CurrentBranch.PK;
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			ARReceipt testReceipt = Factory.NewWithValidTestData<ARReceipt>();
			testReceipt.AH_OH = Organisation.PK;
			testReceipt.AH_GB = newBranch.PK;
			Factory.Save();
			TransactionsFilter.AH_GB = GlbBranch.CurrentBranch.PK;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 1 transaction in the collection", 1, TransactionsFilter.Transactions.Count);
			Assert("The collection should contain the TestJournal", TransactionsFilter.Transactions.Contains(testJournal));
			GlbCompany newCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch newBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch2.GB_GC = newCompany.PK;
			Factory.Save();
			TransactionsFilter.AH_GB = newBranch2.PK;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There collection should be empty", 0, TransactionsFilter.Transactions.Count);
			TransactionsFilter.AH_GB = ZGuid.Empty;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 2 transactions in the collection", 2, TransactionsFilter.Transactions.Count);
			Assert("The collection should contain the TestJournal", TransactionsFilter.Transactions.Contains(testJournal));
			Assert("The collection should contain the TestReceipt", TransactionsFilter.Transactions.Contains(testReceipt));
		}

		public void TestDepartmentFilter()
		{
			ARJournal testJournal = Factory.NewWithValidTestData<ARJournal>();
			testJournal.AH_OH = Organisation.PK;
			testJournal.AH_GE = GlbDepartment.CurrentDepartment.PK;
			GlbDepartment newDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			ARReceipt testReceipt = Factory.NewWithValidTestData<ARReceipt>();
			testReceipt.AH_OH = Organisation.PK;
			testReceipt.AH_GE = newDepartment.PK;
			GlbDepartment newDepartment2 = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();
			TransactionsFilter.AH_GE = GlbDepartment.CurrentDepartment.PK;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 1 transaction in the collection", 1, TransactionsFilter.Transactions.Count);
			Assert("The collection should contain the TestJournal", TransactionsFilter.Transactions.Contains(testJournal));
			TransactionsFilter.AH_GE = newDepartment2.PK;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There collection should be empty", 0, TransactionsFilter.Transactions.Count);
			TransactionsFilter.AH_GE = ZGuid.Empty;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 2 transactions in the collection", 2, TransactionsFilter.Transactions.Count);
			Assert("The collection should contain the TestJournal", TransactionsFilter.Transactions.Contains(testJournal));
			Assert("The collection should contain the TestReceipt", TransactionsFilter.Transactions.Contains(testReceipt));
		}

		public void TestDisbursementOnlyFilter()
		{
			ARJournal journalDisbursementInvoice = Factory.NewWithValidTestData<ARJournal>();
			journalDisbursementInvoice.AH_OH = Organisation.PK;
			journalDisbursementInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			ARJournal journalDisbursementInForeignCurrency = Factory.NewWithValidTestData<ARJournal>();
			journalDisbursementInForeignCurrency.AH_OH = Organisation.PK;
			journalDisbursementInForeignCurrency.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency;
			ARJournal journalDisbursementInForeignCurrency_Batching = Factory.NewWithValidTestData<ARJournal>();
			journalDisbursementInForeignCurrency_Batching.AH_OH = Organisation.PK;
			journalDisbursementInForeignCurrency_Batching.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching;
			ARJournal journalDisbursementInvoice_Batching = Factory.NewWithValidTestData<ARJournal>();
			journalDisbursementInvoice_Batching.AH_OH = Organisation.PK;
			journalDisbursementInvoice_Batching.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice_Batching;
			ARReceipt receipt = Factory.NewWithValidTestData<ARReceipt>();
			receipt.AH_OH = Organisation.PK;
			receipt.AH_TransactionCategory = "";
			Factory.Save();
			TransactionsFilter.AH_IsDisbursement = ZBool.True;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 4 transactions in the collection", 4, TransactionsFilter.Transactions.Count);
			Assert("The collection should contain the journalDisbursementInvoice", TransactionsFilter.Transactions.Contains(journalDisbursementInvoice));
			Assert("The collection should contain the journalDisbursementInForeignCurrency", TransactionsFilter.Transactions.Contains(journalDisbursementInForeignCurrency));
			Assert("The collection should contain the journalDisbursementInForeignCurrency_Batching", TransactionsFilter.Transactions.Contains(journalDisbursementInForeignCurrency_Batching));
			Assert("The collection should contain the journalDisbursementInvoice_Batching", TransactionsFilter.Transactions.Contains(journalDisbursementInvoice_Batching));
			TransactionsFilter.AH_IsDisbursement = ZBool.False;
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("There should be 5 transactions in the collection", 5, TransactionsFilter.Transactions.Count);
			Assert("The collection should contain the journalDisbursementInvoice", TransactionsFilter.Transactions.Contains(journalDisbursementInvoice));
			Assert("The collection should contain the journalDisbursementInForeignCurrency", TransactionsFilter.Transactions.Contains(journalDisbursementInForeignCurrency));
			Assert("The collection should contain the journalDisbursementInForeignCurrency_Batching", TransactionsFilter.Transactions.Contains(journalDisbursementInForeignCurrency_Batching));
			Assert("The collection should contain the journalDisbursementInvoice_Batching", TransactionsFilter.Transactions.Contains(journalDisbursementInvoice_Batching));
			Assert("The collection should contain the TestReceipt", TransactionsFilter.Transactions.Contains(receipt));
		}

		public void TestDecimals()
		{
			AssertEquals("LocalCurrency Decimal Places", GlbCompany.CurrentCompany.LocalCurrency.Decimals, TransactionsFilter.Decimals);
			RefCurrency localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			int saveDecimals = localCurrency.RX_SubUnitRatio;
			try
			{
				localCurrency.RX_SubUnitRatio = 10;
				AssertEquals("One Decimal Place", 1, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
				AssertEquals("One Decimal Place", 1, TransactionsFilter.Decimals);
			}
			finally
			{
				localCurrency.RX_SubUnitRatio = saveDecimals;
			}
		}

		public void TestSummaryProperties()
		{
			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 7;
			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "INV";
			ZDateTime today = ZDateTime.Today;
			ZDateTime postDate = today.AddDays(-100);
			ZDateTime overThreeTerms = today.AddDays(-60);
			ZDateTime threeTerms = today.AddDays(-21);
			ZDateTime twoTerms = today.AddDays(-14);
			ZDateTime oneTerm = today.AddDays(-7);
			ZDateTime notYetDue = today.AddDays(8);
			ARInvoice invoice01 = CreateTransactionForTest("001", Organisation, postDate, overThreeTerms, 120m, true);
			ARInvoice invoice02 = CreateTransactionForTest("002", Organisation, postDate, threeTerms, 110m, true);
			ARInvoice invoice03 = CreateTransactionForTest("003", Organisation, postDate, twoTerms, 100m, true);
			ARInvoice invoice04 = CreateTransactionForTest("004", Organisation, postDate, oneTerm, 90m, true);
			ARInvoice invoice05 = CreateTransactionForTest("005", Organisation, postDate, today, 80m, true);
			ARInvoice invoice06 = CreateTransactionForTest("006", Organisation, postDate, notYetDue, 70m, true);
			ARInvoice invoice07 = CreateTransactionForTest("007", Organisation, postDate, overThreeTerms, 60m, false);
			ARInvoice invoice08 = CreateTransactionForTest("008", Organisation, postDate, threeTerms, 50m, false);
			ARInvoice invoice09 = CreateTransactionForTest("009", Organisation, postDate, twoTerms, 40m, false);
			ARInvoice invoice10 = CreateTransactionForTest("0010", Organisation, postDate, oneTerm, 30m, false);
			ARInvoice invoice11 = CreateTransactionForTest("0011", Organisation, postDate, today, 20m, false);
			ARInvoice invoice12 = CreateTransactionForTest("0012", Organisation, postDate, notYetDue, 10m, false);
			Factory.Save();
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Transactions in Collection", 12, TransactionsFilter.Transactions.Count);
			AssertStandardSummaryAmounts(10m, 20m, 30m, 40m, 110m, 210m);
			AssertDisbursementSummaryAmounts(70m, 80m, 90m, 100m, 230m, 570m);
			AssertTotalSummaryAmounts(80m, 100m, 120m, 140m, 340m, 780m, 700m, "90%");
			Organisation.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceDays = 0;
			Organisation.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = "COD";
			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 7;
			Organisation.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "INV";
			TransactionsFilter.RefreshInvoiceList();
			AssertStandardSummaryAmounts(10m, 20m, 30m, 40m, 110m, 210m);
			AssertDisbursementSummaryAmounts(70m, 80m, 0m, 0m, 420m, 570m);
			AssertTotalSummaryAmounts(80m, 100m, 30m, 40m, 530m, 780m, 700m, "90%");
		}

		void AssertStandardSummaryAmounts(decimal notYetDue, decimal today, decimal oneTerm, decimal twoTerms, decimal overTwoTerms, decimal totalOutstanding)
		{
			AssertEquals("StandardNotYetDue", notYetDue, TransactionsFilter.StandardNotYetDue);
			AssertEquals("StandardDueToday", today, TransactionsFilter.StandardDueToday);
			AssertEquals("StandardOneTermPastDue", oneTerm, TransactionsFilter.StandardOneTermPastDue);
			AssertEquals("StandardTwoTermsPastDue", twoTerms, TransactionsFilter.StandardTwoTermsPastDue);
			AssertEquals("StandardOverTwoTermsPastDue", overTwoTerms, TransactionsFilter.StandardOverTwoTermsPastDue);
			AssertEquals("StandardOutstanding", totalOutstanding, TransactionsFilter.StandardOutstanding);
		}

		void AssertDisbursementSummaryAmounts(decimal notYetDue, decimal today, decimal oneTerm, decimal twoTerms, decimal overTwoTerms, decimal totalOutstanding)
		{
			AssertEquals("DisbursementNotYetDue", notYetDue, TransactionsFilter.DisbursementNotYetDue);
			AssertEquals("DisbursementDueToday", today, TransactionsFilter.DisbursementDueToday);
			AssertEquals("DisbursementOneTermPastDue", oneTerm, TransactionsFilter.DisbursementOneTermPastDue);
			AssertEquals("DisbursementTwoTermsPastDue", twoTerms, TransactionsFilter.DisbursementTwoTermsPastDue);
			AssertEquals("DisbursementOverTwoTermsPastDue", overTwoTerms, TransactionsFilter.DisbursementOverTwoTermsPastDue);
			AssertEquals("DisbursementOutstanding", totalOutstanding, TransactionsFilter.DisbursementOutstanding);
		}

		void AssertTotalSummaryAmounts(ZDecimal notYetDue, ZDecimal today, ZDecimal oneTerm, ZDecimal twoTerms, ZDecimal overTwoTerms, ZDecimal totalOutstanding, ZDecimal totalPastDue, ZString percentagePastDue)
		{
			AssertEquals("TotalNotYetDue", notYetDue, TransactionsFilter.TotalNotYetDue);
			AssertEquals("TotalDueToday", today, TransactionsFilter.TotalDueToday);
			AssertEquals("TotalOneTermPastDue", oneTerm, TransactionsFilter.TotalOneTermPastDue);
			AssertEquals("TotalTwoTermsPastDue", twoTerms, TransactionsFilter.TotalTwoTermsPastDue);
			AssertEquals("TotalOverTwoTermsPastDue", overTwoTerms, TransactionsFilter.TotalOverTwoTermsPastDue);
			AssertEquals("TotalOutstanding", totalOutstanding.ToString(2), TransactionsFilter.TotalOutstanding);
			AssertEquals("TotalPastDue", totalPastDue.ToString(2), TransactionsFilter.TotalPastDue);
			AssertEquals("TotalPercentagePastDue", percentagePastDue, TransactionsFilter.TotalPercentagePastDue);
		}

		ARInvoice CreateTransactionForTest(ZString transactionNum, OrgHeader org, ZDateTime postDate, ZDateTime dueDate, ZDecimal outstandingAmount, bool disbursement)
		{
			var newInvoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), transactionNum, TestObjectCreator.AUD, 1m, outstandingAmount + 1, 0m, outstandingAmount + 1, 0m);
			newInvoice.AH_PostDate = postDate;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_OutstandingAmount = outstandingAmount;
			newInvoice.AH_TransactionCategory = disbursement ? InvoiceTypesList.Codes.DisbursementInvoice : "";
			newInvoice.AH_DueDate = dueDate;
			AccTransactionMatchLink matchLink = ((IMatching)newInvoice).CurrentMatchGroup.AddNew();
			matchLink.AP_AH = newInvoice.PK;
			matchLink.AP_Amount = 1;
			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -1;
			AccTransactionMatchLink matchingLink = ((IMatching)newInvoice).CurrentMatchGroup.AddNew();
			matchingLink.AP_AH = headerToMatch.PK;
			matchingLink.AP_Amount = -1;
			TestObjectCreator.SetupMatchLinkMatchDate(newInvoice);
			return newInvoice;
		}

		public void TestValidateTransactionType()
		{
			Assert("Precondition: there should not be any errors for TransactionType", !TransactionsFilter.AH_TransactionTypeInfo.HasErrors());
			TransactionsFilter.AH_TransactionType = "#$%";
			Assert("Invalid transaction type - should cause errors", TransactionsFilter.AH_TransactionTypeInfo.HasErrors());
			TransactionsFilter.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			Assert("Invoice is a valid transaction type - should not be any errors", !TransactionsFilter.AH_TransactionTypeInfo.HasErrors());
		}

		public void TestValidatePaymentStatus()
		{
			Assert("Precondition: there should not be any errors for Payment Status", !TransactionsFilter.PaymentStatusInfo.HasErrors());
			TransactionsFilter.PaymentStatus = "@#%^";
			Assert("Invalid payment status - should cause errors", TransactionsFilter.PaymentStatusInfo.HasErrors());
			TransactionsFilter.PaymentStatus = AccountingUtils.PaymentStatusTypes.All;
			Assert("This is a valid payment status - should not be any errors", !TransactionsFilter.PaymentStatusInfo.HasErrors());
		}

		public void TestResetInvoiceList()
		{
			TransactionsFilter = new CollectionNotesTransactionsFilter(Organisation, GlbBranch.CurrentBranch);
			var newInvoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "0001", TestObjectCreator.USD, 1m, 100m, 0m, 100m, 0m);
			newInvoice.AH_OH = Organisation.PK;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_FullyPaidDate = ZDateTime.Empty;
			newInvoice.AH_OutstandingAmount = 10m;
			AccTransactionMatchLink matchLink = ((IMatching)newInvoice).CurrentMatchGroup.AddNew();
			matchLink.AP_Amount = 90m;
			matchLink.AP_AH = newInvoice.PK;
			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -90m;
			AccTransactionMatchLink matchingLink = ((IMatching)newInvoice).CurrentMatchGroup.AddNew();
			matchingLink.AP_AH = headerToMatch.PK;
			matchingLink.AP_Amount = -90m;
			TestObjectCreator.SetupMatchLinkMatchDate(newInvoice);
			Factory.Save();
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Should be 1 transaction in the collection", 1, TransactionsFilter.Transactions.Count);
			Assert("Collection should contain NewInvoice", TransactionsFilter.Transactions.Contains(newInvoice));
			TransactionsFilter.AH_TransactionType = "ADJ";
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Invoice Count", 0, TransactionsFilter.Transactions.Count);
			TransactionsFilter.ResetInvoiceList();
			AssertDefaultValuesAreSetCorrectly();
			AssertEquals("Should be 1 transaction in the collection", 1, TransactionsFilter.Transactions.Count);
			Assert("Collection should contain NewInvoice", TransactionsFilter.Transactions.Contains(newInvoice));
		}

		public void TestDateFilterList()
		{
			AssertEquals("Expected List to contain 4 elements", 4, TransactionsFilter.AH_DateFilter_List.Count);
			Assert("Expected List to contain ALL option", TransactionsFilter.AH_DateFilter_List.ContainsCode(AccountingUtils.DateFilterTypes.All));
			Assert("Expected List to contain Post Date option", TransactionsFilter.AH_DateFilter_List.ContainsCode(AccountingUtils.DateFilterTypes.PostDate));
			Assert("Expected List to contain Transaction Date option", TransactionsFilter.AH_DateFilter_List.ContainsCode(AccountingUtils.DateFilterTypes.TransactionDate));
			Assert("Expected List to contain Due Date option", TransactionsFilter.AH_DateFilter_List.ContainsCode(AccountingUtils.DateFilterTypes.DueDate));
		}

		public void TestAH_NumberFilter_List()
		{
			TransactionsFilter = new CollectionNotesTransactionsFilter(Organisation, GlbBranch.CurrentBranch);
			Assert("The Number types list should contain 'All'", TransactionsFilter.AH_NumberFilterList.ContainsCode(AccountingUtils.NumberFilterTypes.All));
			Assert("The Number types list should contain JobNumber", TransactionsFilter.AH_NumberFilterList.ContainsCode(AccountingUtils.NumberFilterTypes.JobNumber));
			Assert("The Number types list should contain TransactionNumber", TransactionsFilter.AH_NumberFilterList.ContainsCode(AccountingUtils.NumberFilterTypes.TransactionNumber));
			Assert("The Number types list should contain JobInvoiceNumber", TransactionsFilter.AH_NumberFilterList.ContainsCode(AccountingUtils.NumberFilterTypes.ConsolidationNumber));
			AssertEquals("items in number filter list", 4, TransactionsFilter.AH_NumberFilterList.Count);
			GlbBranch newBranch = SetupNewChineseCompanyAndBranch();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				TransactionsFilter = new CollectionNotesTransactionsFilter(Organisation, GlbBranch.CurrentBranch);
				Assert("The Number types list should contain Govt Compliance Number", TransactionsFilter.AH_NumberFilterList.ContainsCode(AccountingUtils.NumberFilterTypes.GovtComplianceNumber));
				AssertEquals("There should be 5 items in the number filter list", 5, TransactionsFilter.AH_NumberFilterList.Count);
			}
		}

		public void TestSetDefaultValues()
		{
			TransactionsFilter = new CollectionNotesTransactionsFilter(Organisation, GlbBranch.CurrentBranch);
			AssertDefaultValuesAreSetCorrectly();
		}

		void AssertDefaultValuesAreSetCorrectly()
		{
			AssertEquals("AH_DateFilter should be defaulted", AccountingUtils.DateFilterTypes.All, TransactionsFilter.AH_DateFilter);
			AssertEquals("AH_NumberFilter should be defaulted", AccountingUtils.NumberFilterTypes.All, TransactionsFilter.AH_NumberFilter);
			AssertEquals("AH_TransactionType should be defaulted", "ALL", TransactionsFilter.AH_TransactionType);
			AssertEquals("AH_FromDate should be defaulted", ZDateTime.Empty, TransactionsFilter.AH_FromDate);
			AssertEquals("AH_ToDate should be defaulted", ZDateTime.Empty, TransactionsFilter.AH_ToDate);
			AssertEquals("AH_GB should be defaulted", ZGuid.Empty, TransactionsFilter.AH_GB);
			AssertEquals("AH_GE should be defaulted", ZGuid.Empty, TransactionsFilter.AH_GE);
			AssertEquals("AH_Number should be defaulted", ZString.Empty, TransactionsFilter.AH_Number);
			AssertEquals("AH_RX_NKTransactionCurrency should be defaulted", ZString.Empty, TransactionsFilter.AH_RX_NKTransactionCurrency);
			AssertEquals("PaymentStatus should be defaulted", AccountingUtils.PaymentStatusTypes.Unpaid, TransactionsFilter.PaymentStatus);
			AssertEquals("AH_IsDisbursement should be defaulted", ZBool.False, TransactionsFilter.AH_IsDisbursement);
		}

		public void TestTransactionsIsReadOnly()
		{
			ARInvoice newInvoice = (ARInvoice)TestHelper.CreateInvoice(typeof(ARInvoice), TestHelper.USD, 1m);
			newInvoice.AH_OH = Organisation.PK;
			Factory.Save();
			TransactionsFilter.RefreshInvoiceList();
			AssertEquals("Should be 1 transaction in the collection", 1, TransactionsFilter.Transactions.Count);
			Assert("Collection should contain NewInvoice", TransactionsFilter.Transactions.Contains(newInvoice));
			Assert("Transactions should be read only", TransactionsFilter.Transactions.ReadOnly);
		}

		CollectionNotesTransactionsFilter TransactionsFilter;
		OrgHeader Organisation;
		TestObjectCreator TestHelper;
		GlbBranch SetupNewChineseCompanyAndBranch()
		{
			RefCountry china = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.China));
			GlbCompany chineseCompany = Factory.NewWithValidTestData<GlbCompany>();
			chineseCompany.GC_RN_NKCountryCode = china.Code;
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = chineseCompany.PK;
			Factory.Save();
			return newBranch;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestHelper = new TestObjectCreator(Factory);
			Organisation = TestHelper.LocalClient;
			TransactionsFilter = new CollectionNotesTransactionsFilter(Organisation, GlbBranch.CurrentBranch);
			TransactionsFilter.PaymentStatus = "ALL";
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
			}
		}
	}
}
