using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	[TestedType(typeof(AccCollectionBatch))]
	public class AccCollectionBatchTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<AccCollectionBatch>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAccCollectionBatch()
		{
			var batch = Factory.NewWithValidTestData<AccCollectionBatch>();

			var currencyList = new List<string>
			{
				nameof(batch.ACB_TotalAmount)
			};

			var tester = new DecimalPlacesAttributeTester(batch, batch.Company);
			tester.CheckNonLocalCurrency(currencyList, nameof(batch.Decimals), nameof(batch.ACB_RX_NKCurrency), batch);
		}

		public void TestUpdateCollectionFormat()
		{
			var currCompany = GlbCompany.CurrentCompany;

			using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var batch = Factory.NewWithValidTestData<AccCollectionBatch>();
				AssertEquals(CollectionFileFormatList.Codes.ribaFormat, batch.ACB_CollectionFileFormat);
			}

			using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				batch = Factory.NewWithValidTestData<AccCollectionBatch>();
				batch.ACB_RX_NKCurrency = TestObjectCreator.EUR.RX_Code;
				AssertEquals(CollectionFileFormatList.Codes.sepaFormat, batch.ACB_CollectionFileFormat);
				batch = Factory.NewWithValidTestData<AccCollectionBatch>();
				batch.ACB_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
				AssertEquals(CollectionFileFormatList.Codes.unknown, batch.ACB_CollectionFileFormat);
			}

			using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				batch = Factory.NewWithValidTestData<AccCollectionBatch>();
				AssertEquals(CollectionFileFormatList.Codes.deFormat, batch.ACB_CollectionFileFormat);
			}

			using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				batch = Factory.NewWithValidTestData<AccCollectionBatch>();
				AssertEquals(CollectionFileFormatList.Codes.itauBank, batch.ACB_CollectionFileFormat);
			}

			using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Argentina))
			{
				batch = Factory.NewWithValidTestData<AccCollectionBatch>();
				AssertEquals(CollectionFileFormatList.Codes.unknown, batch.ACB_CollectionFileFormat);
			}
		}

		public void TestDocManagerInfo()
		{
			AccCollectionBatch batch = (AccCollectionBatch)GetNewBusinessObject();
			AssertEquals("DocManagerInfo.GetType()", typeof(DocManagerInfo), ((IDocManagerSupport)batch).DocManagerInfo.GetType());
			AssertEquals("DocManagerInfo.DocManagerCode", Core.Constants.DocManagerCodes.CollectionBatch, ((IDocManagerSupport)batch).DocManagerInfo.DocManagerCode);
		}

		public void TestUniqueBatchID()
		{
			batch.ACB_BatchNumber = "00001000";
			AssertEquals("UniqueBatchID", "00001000", batch.UniqueBatchID);
		}

		[TestDate(2015, 02, 10)]
		public void TestCreateReceiptsAndDepositBatchHandleExceptionAndRollback()
		{
			ZString errorMessage;
			var runDate = ZDate.Today;
			Factory.Save();
			AssertEquals(60m, order1.ACO_Amount);
			AssertEquals(20m, invoice1.AH_OutstandingAmount);
			AssertEquals(30m, invoice2.AH_OutstandingAmount);
			AssertEquals(10m, journal.AH_OutstandingAmount);
			AssertEquals(50m, order2.ACO_Amount);
			AssertEquals(20m, invoice3.AH_OutstandingAmount);
			AssertEquals(30m, invoice4.AH_OutstandingAmount);
			order2.SimulateExceptionCondition = true;
			Assert("Expect CreateReceiptsAndDepositBatch failed and rollback", !batch.CreateReceiptsAndDepositBatch(out errorMessage, runDate, runDate, false));
			AssertEquals("Receipt matching process failed for the transactions in the order 00000002.", errorMessage);
			var aRReceiptFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
			aRReceiptFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			aRReceiptFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			var aRReceipts = Factory.Load<ARReceipt>(aRReceiptFilter);
			AssertEquals("should find 0 receipt due to rollback", 0, aRReceipts.Length);
		}

		[TestDate(2015, 02, 10)]
		public void TestCreateReceiptsAndOneDepositBatchForAllOrders()
		{
			TestCreateReceiptsAndDepositBatch(false);
		}

		[TestDate(2019, 07, 10)]
		public void TestPostDateAndInvoiceDateForCreateReceiptsAndOneDepositBatch()
		{
			var now = ZDateTime.Now;
			var oneMonthAgo = now.AddMonths(-1);
			var debtor = TestObjectCreator.ABIGAS;
			var refCurr = TestObjectCreator.AUD;
			var chargeCode = TestObjectCreator.CC1.PK;

			var invoice5 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV5", refCurr, 1M, 20, 0M, 20, 0M, debtor, chargeCode, now, ZDateTime.Empty, now, false);
			var invoice6 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV6", refCurr, 1M, 30, 0M, 30, 0M, debtor, chargeCode, now, ZDateTime.Empty, now, false);
			var order3 = TestObjectCreator.CreateCollectionOrder(batch, now.Date, debtor, "00000003", 50m, false);
			var line3_1 = TestObjectCreator.CreateCollectionOrderLine(order3, invoice5, false);
			var line3_2 = TestObjectCreator.CreateCollectionOrderLine(order3, invoice6, false);
			order3.IncludeInBatch = true;

			creator.CreateTestPeriodsForEntireYear(now.Year);
			order1.ACO_DepositedDate = oneMonthAgo.Date;
			order2.ACO_DepositedDate = now.Date;
			Factory.Save();

			batch.CreateReceiptsAndDepositBatch(out ZString errorMessage, now, now.AddDays(-1), false);
			var receiptQuery = new ZQuery(AccTransactionHeaderSchema.AH_AB, batch.ACB_AB);
			receiptQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
			var arReceipts = Factory.Load<ARReceipt>(receiptQuery);
			AssertEquals(3, arReceipts.Length);
			var expectedPost = new[] { oneMonthAgo, now, now };
			var expectedInvoiceDate = new[] { oneMonthAgo, now, now.AddDays(-1) };
			AssertContainsExactElementsInAnyOrder(expectedPost, arReceipts.Select(x => x.AH_PostDate));
			AssertContainsExactElementsInAnyOrder(expectedInvoiceDate, arReceipts.Select(x => x.AH_InvoiceDate));

			var depositBatchQuery = new ZQuery(AccTransactionHeaderSchema.AH_AB, batch.ACB_AB);
			depositBatchQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ReceiptBatch);
			var depositBatches = Factory.Load<DepositBatch>(depositBatchQuery);
			AssertEquals(3, depositBatches.Length);
			AssertContainsExactElementsInAnyOrder(expectedPost, depositBatches.Select(x => x.AH_PostDate));
			AssertContainsExactElementsInAnyOrder(expectedInvoiceDate, depositBatches.Select(x => x.AH_InvoiceDate));
		}

		[TestDate(2015, 02, 10)]
		public void TestCreateReceiptsAndDepositBatchPerOrder()
		{
			TestCreateReceiptsAndDepositBatch(true);
		}

		[TestDate(2015, 02, 10)]
		void TestCreateReceiptsAndDepositBatch(bool createDepositBatchPerOrder)
		{
			ZString errorMessage;
			var runDate = ZDate.Today;
			Factory.Save();
			AssertEquals(60m, order1.ACO_Amount);
			AssertEquals(20m, invoice1.AH_OutstandingAmount);
			AssertEquals(30m, invoice2.AH_OutstandingAmount);
			AssertEquals(10m, journal.AH_OutstandingAmount);
			AssertEquals(50m, order2.ACO_Amount);
			AssertEquals(20m, invoice3.AH_OutstandingAmount);
			AssertEquals(30m, invoice4.AH_OutstandingAmount);
			var logs = batch.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_Reference, $"Receipt and Deposit Batch created for Batch Number {batch.ACB_BatchNumber}."));
			AssertEquals(0, logs.Length);
			Assert(batch.CreateReceiptsAndDepositBatch(out errorMessage, runDate, runDate, createDepositBatchPerOrder));

			AssertEquals(0m, invoice1.AH_OutstandingAmount);
			AssertEquals(runDate, invoice1.AH_FullyPaidDate);
			AssertEquals(0m, invoice2.AH_OutstandingAmount);
			AssertEquals(runDate, invoice2.AH_FullyPaidDate);
			AssertEquals(0m, journal.AH_OutstandingAmount);
			AssertEquals(runDate, journal.AH_FullyPaidDate);
			AssertEquals(0m, invoice3.AH_OutstandingAmount);
			AssertEquals(runDate, invoice3.AH_FullyPaidDate);
			AssertEquals(0m, invoice4.AH_OutstandingAmount);
			AssertEquals(runDate, invoice4.AH_FullyPaidDate);
			AssertNullOrEmpty(errorMessage);
			var aRReceiptFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
			aRReceiptFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			aRReceiptFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			aRReceiptFilter.OrderBy = "AH_TransactionNum";
			var aRReceipts = Factory.Load<ARReceipt>(aRReceiptFilter);
			AssertEquals("should find 2 receipts", 2, aRReceipts.Length);
			var receipt1 = aRReceipts[0];
			var receipt2 = aRReceipts[1];
			AssertEquals(-60m, receipt1.AH_InvoiceAmount);
			AssertEquals(-50m, receipt2.AH_InvoiceAmount);
			AccCollectionOrderTest.AssertReceiptDetail(receipt1, order1.ACO_OH_Debtor, batch.ACB_AB, order1.ACO_RX_NKCurrency, 1m, $"{batch.ACB_BatchNumber}/{order1.ACO_OrderNumber}", 60m, 60m, -60m, 0m, runDate, runDate, runDate);
			AccCollectionOrderTest.AssertReceiptDetail(receipt2, order2.ACO_OH_Debtor, batch.ACB_AB, order2.ACO_RX_NKCurrency, 1m, $"{batch.ACB_BatchNumber}/{order2.ACO_OrderNumber}", 50m, 50m, -50m, 0m, runDate, runDate, runDate);
			AccCollectionOrderTest.AssertMatchLinks(Factory, receipt1.PK, -60m, invoice1.PK, 20m, invoice2.PK, 30m, journal.PK, 10m);
			AccCollectionOrderTest.AssertMatchLinks(Factory, receipt2.PK, -50m, invoice3.PK, 20m, invoice4.PK, 30m);
			AssertNotNull(receipt1.RelatedDepositBatch);
			AssertNotNull(receipt2.RelatedDepositBatch);
			if (createDepositBatchPerOrder)
			{
				AssertNotEquals(receipt1.RelatedDepositBatch, receipt2.RelatedDepositBatch);
				AssertEquals(60m, receipt1.RelatedDepositBatch.AH_InvoiceAmount);
				AssertEquals(60m, receipt1.RelatedDepositBatch.AH_OSTotal);
				AssertEquals(50m, receipt2.RelatedDepositBatch.AH_InvoiceAmount);
				AssertEquals(50m, receipt2.RelatedDepositBatch.AH_OSTotal);
			}
			else
			{
				AssertEquals(receipt1.RelatedDepositBatch, receipt2.RelatedDepositBatch);
				AssertEquals(110m, receipt1.RelatedDepositBatch.AH_InvoiceAmount);
				AssertEquals(110m, receipt1.RelatedDepositBatch.AH_OSTotal);
			}

			logs = batch.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_Reference, $"Receipt and Deposit Batch created for Batch Number {batch.ACB_BatchNumber}."));
			AssertEquals(1, logs.Length);
		}

		[TestDate(2019, 07, 10)]
		public void TestCreateReceiptsAndDeposit_EmptyDepositedDate()
		{
			var runDate = ZDate.Today;
			order1.ACO_DepositedDate = ZDate.Empty;
			order2.ACO_DepositedDate = ZDate.Empty;
			Factory.Save();

			ZString errorMessage;
			Assert(batch.CreateReceiptsAndDepositBatch(out errorMessage, runDate, runDate, true));
			AssertNullOrEmpty(errorMessage);

			AssertEquals(runDate, order1.ACO_DepositedDate);
			AssertEquals(runDate, order2.ACO_DepositedDate);
		}

		[TestDate(2019, 07, 10)]
		public void TestCreateReceiptsAndDeposit_WithFutureDepositedDate()
		{
			var runDate = ZDate.Today;
			order1.ACO_DepositedDate = runDate.AddDays(-1);
			order2.ACO_DepositedDate = runDate.AddDays(1);
			Factory.Save();

			ZString errorMessage;
			Assert(batch.CreateReceiptsAndDepositBatch(out errorMessage, runDate, runDate, true));
			AssertNullOrEmpty(errorMessage);

			AssertEquals("order with deposted date before today is not changed", runDate.AddDays(-1), order1.ACO_DepositedDate);
			AssertEquals("order with future deposted date is changed", runDate, order2.ACO_DepositedDate);
			var logs = order2.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_Reference, "Deposited Date of 11-JUL-19 was in the future and discarded."));
			AssertEquals(1, logs.Length);
		}

		[TestDate(2019, 07, 10)]
		public void TestCreateReceiptsAndDeposit_WithDepositedDateFallsClosedPeriod()
		{
			var runDate = ZDate.Today;
			creator.CreateTestPeriodsForEntireYear(runDate.Year);

			order1.ACO_DepositedDate = runDate.AddMonths(-1);
			order2.ACO_DepositedDate = runDate;

			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var period = periodCalculator.GetPeriodManagementFromDate(order1.ACO_DepositedDate);
			period.AM_IsSubLedgerClosed = true;
			Factory.Save();

			ZString errorMessage;
			Assert(batch.CreateReceiptsAndDepositBatch(out errorMessage, runDate, runDate, true));
			AssertNullOrEmpty(errorMessage);

			AssertEquals("order with deposted date falls in closed period is changed", runDate, order1.ACO_DepositedDate);
			AssertEquals("order with today deposted date is not changed", runDate, order2.ACO_DepositedDate);
			var logs = order1.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_Reference, "Deposited Date of 10-JUN-19 falls in closed sub ledger period and discarded."));
			AssertEquals(1, logs.Length);
		}

		public void TestPreventDelete()
		{
			var batch = Factory.NewWithValidTestData<AccCollectionBatch>();
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(AccCollectionBatch)));
		}

		public void TestACB_RX_NKCurrency()
		{
			batch.ACB_AB = ZGuid.Empty;
			AssertNullOrEmpty(batch.ACB_RX_NKCurrency);
			batch.ACB_AB = bankAccount.PK;
			var savedCurrency = batch.BankAccount.AB_RX_NKAccountCurrency;
			AssertEquals(savedCurrency, batch.ACB_RX_NKCurrency);
			batch.ACB_AB = ZGuid.Empty;
			AssertEquals(savedCurrency, batch.ACB_RX_NKCurrency);
		}

		public void TestCollectionOrders()
		{
			Factory.Save();
			AssertEquals(2, batch.CollectionOrders.Count);
			AssertEquals(true, batch.CollectionOrders[0].IncludeInBatch);
			AssertEquals(true, batch.CollectionOrders[1].IncludeInBatch);
		}

		public void TestOnSaveWillDeleteUnselectedOrderAndLines()
		{
			var orders = batch.CollectionOrders;
			var o1 = (AccCollectionOrder)orders.FindByPK(order1.PK);
			var o2 = (AccCollectionOrder)orders.FindByPK(order2.PK);
			o1.IncludeInBatch = true;
			o1.CollectionOrderLines.ApplySort(new InstantiationTimeComparer());
			o1.CollectionOrderLines[0].IncludeInOrder = true;
			o1.CollectionOrderLines[1].IncludeInOrder = false;
			o1.CollectionOrderLines[2].IncludeInOrder = true;
			o2.IncludeInBatch = false;
			o2.CollectionOrderLines[0].IncludeInOrder = true;
			o2.CollectionOrderLines[1].IncludeInOrder = true;
			Factory.Save();
			AssertEquals(1, batch.CollectionOrders.Count);
			AssertEquals(order1.PK, batch.CollectionOrders[0].PK);
			AssertEquals(2, batch.CollectionOrders[0].CollectionOrderLines.Count);
			AssertEquals(line1_1.PK, batch.CollectionOrders[0].CollectionOrderLines[0].PK);
		}

		public void TestOnSaveGeneratesBatchNumberAndOrderNumber()
		{
			AssertEquals(ZString.Empty, batch.ACB_BatchNumber);
			AssertEquals(ZString.Empty, order1.ACO_OrderNumber);
			AssertEquals(ZString.Empty, order2.ACO_OrderNumber);
			Factory.Save();
			AssertEquals("00001000", batch.ACB_BatchNumber);
			AssertEquals("00000001", order1.ACO_OrderNumber);
			AssertEquals("00000002", order2.ACO_OrderNumber);
			batch.ACB_SystemLastEditTimeUtc = batch.ACB_SystemLastEditTimeUtc.AddMinutes(1);
			// call save again should not regenerate batch and order numbers
			Factory.Save();
			AssertEquals("00001000", batch.ACB_BatchNumber);
			AssertEquals("00000001", order1.ACO_OrderNumber);
			AssertEquals("00000002", order2.ACO_OrderNumber);
		}

		[ExpectNoExceptions]
		public void TestAttachFileToEdoc_WhenGetHelperNull()
		{
			SetupMocks();
			collectionBatchFileGeneratorProviderMock.Setup(x => x.GetHelper()).Returns((ICollectionBatchFileGenerator)null);

			Factory.Save();
			collectionBatchFileGeneratorMock.Verify(x => x.AttachFileToEdoc(It.IsAny<string>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestAttachFileToEdoc_WhenGetHelperNotNull()
		{
			SetupMocks();
			collectionBatchFileGeneratorProviderMock.Setup(x => x.GetHelper()).Returns(collectionBatchFileGeneratorMock.Object);

			string data = "Test Data";
			collectionBatchFileGeneratorMock.Setup(x => x.GetFileData()).Returns(data);

			Factory.Save();
			collectionBatchFileGeneratorMock.Verify(x => x.AttachFileToEdoc(data), Times.Once);
		}

		void SetupMocks()
		{
			var accountingDependencyFactoryMock = new Mock<IAccountingDependencyFactory>();
			var branchLevelPostingHelperMock = new Mock<IBranchLevelPostingHelper>();
			collectionBatchFileGeneratorProviderMock = new Mock<ICollectionBatchFileGeneratorProvider>();
			collectionBatchFileGeneratorMock = new Mock<ICollectionBatchFileGenerator>();
			ObjectFactory.Substitute(accountingDependencyFactoryMock.Object);

			accountingDependencyFactoryMock.Setup(x => x.GetBranchLevelPostingHelper()).Returns(branchLevelPostingHelperMock.Object);
			accountingDependencyFactoryMock.Setup(x => x.GetCollectionBatchFileGeneratorProvider(batch, Env.Instance)).Returns(collectionBatchFileGeneratorProviderMock.Object);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Shouldn't be fired on this business object", true);
		}

		public void TestWorkflowType()
		{
			AssertEquals(WorkflowDescriptors.CollectionBatchCode, ((IWorkflowProviderCore)batch).WorkflowType);
		}

		AccCollectionBatch batch;
		AccCollectionOrder order1;
		AccCollectionOrder order2;
		AccCollectionOrderLine line1_1;
		TestObjectCreator creator;
		AccBankAccount bankAccount;
		ARInvoice invoice1;
		ARInvoice invoice2;
		ARInvoice invoice3;
		ARInvoice invoice4;
		ARJournal journal;
		Mock<ICollectionBatchFileGeneratorProvider> collectionBatchFileGeneratorProviderMock;
		Mock<ICollectionBatchFileGenerator> collectionBatchFileGeneratorMock;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<AccCollectionBatch>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			creator = new TestObjectCreator(Factory);
			var now = ZDateTime.Now;
			var debtor = TestObjectCreator.ABIGAS;
			var refCurr = TestObjectCreator.AUD;
			var chargeCode = TestObjectCreator.CC1.PK;

			batch = Factory.NewWithValidTestData<AccCollectionBatch>();
			bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			//batch.ACB_AB = bankAccount.PK;
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.IsCancelled = false;
			batch.ACB_BatchNumber = ZString.Empty;
			batch.ACB_TotalAmount = 110m;

			invoice1 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", refCurr, 1M, 20, 0M, 20, 0M, debtor, chargeCode, now, ZDateTime.Empty, now, false);
			invoice2 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", refCurr, 1M, 30, 0M, 30, 0M, debtor, chargeCode, now, ZDateTime.Empty, now, false);
			invoice3 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", refCurr, 1M, 20, 0M, 20, 0M, debtor, chargeCode, now, ZDateTime.Empty, now, false);
			invoice4 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV4", refCurr, 1M, 30, 0M, 30, 0M, debtor, chargeCode, now, ZDateTime.Empty, now, false);
			journal = TestObjectCreator.CreateJournal<ARJournal>(10m, now, debtor.PK);

			order1 = TestObjectCreator.CreateCollectionOrder(batch, now.Date, debtor, ZString.Empty, 60m, false);
			line1_1 = TestObjectCreator.CreateCollectionOrderLine(order1, invoice1, false);
			TestObjectCreator.CreateCollectionOrderLine(order1, invoice2, false);
			TestObjectCreator.CreateCollectionOrderLine(order1, journal, false);

			order2 = TestObjectCreator.CreateCollectionOrder(batch, now.Date.AddDays(1), debtor, ZString.Empty, 50m, false);
			TestObjectCreator.CreateCollectionOrderLine(order2, invoice3, false);
			TestObjectCreator.CreateCollectionOrderLine(order2, invoice4, false);

			batch.CollectionOrders.ApplySort(new InstantiationTimeComparer());
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
	}

	[TestedType(typeof(AccCollectionBatch))]
	class AccCollectionBatchWorkflowProviderTest : WorkflowProviderTest<AccCollectionBatch, AccCollectionBatchProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => new AccCollectionBatchWorkflowDescriptor().Code;
	}
}
