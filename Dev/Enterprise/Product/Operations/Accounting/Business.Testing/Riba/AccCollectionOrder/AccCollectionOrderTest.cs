using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.Riba.AccCollectionOrder;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	[TestedType(typeof(AccCollectionOrder))]
	public class AccCollectionOrderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<AccCollectionOrder>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<AccCollectionOrder>();
		}

		public void TestBankCountryIsEmptyWhenCountryCodeNotPopulated()
		{
			var accountDetail = TestObjectCreator.Debtor.CompanyData.ARAccountDetailsCollection.AddNew();
			accountDetail.A1_PaymentMethod = "CRQ";
			accountDetail.A1_RX_NKAccountCurrency = "AUD";
			accountDetail.A1_IsDefaultAccount = true;

			var order = TestObjectCreator.CreateCollectionOrder(batch, ZDateTime.Today.Date, TestObjectCreator.Debtor, "0000001", 50m, false);
			AssertNotNull(order.CollectionRequestBankCountry);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAccCollectionOrder()
		{
			var osList = new List<string>
			{
				nameof(order1.ACO_Amount)
			};

			var tester = new DecimalPlacesAttributeTester(order1);
			tester.CheckNonLocalCurrency(osList, nameof(order1.CurrencyDecimals), nameof(order1.CollectionBatch.ACB_RX_NKCurrency), order1.CollectionBatch);
		}

		[TestDate(2015, 02, 10)]
		[ExpectNoExceptions]
		public void TestCreateReceiptsAndDepositBatch()
		{
			var newFactory = new BusinessObjectFactory();
			var runDate = ZDateTime.Today;

			AssertEquals(60m, order1.ACO_Amount);
			AssertEquals(20m, invoice1.AH_OutstandingAmount);
			AssertEquals(30m, invoice2.AH_OutstandingAmount);
			AssertEquals(10m, journal.AH_OutstandingAmount);
			var receipt1 = order1.CreateReceiptsAndDepositBatch(newFactory, runDate, runDate, false);
			AssertNotNull(receipt1);
			AssertReceiptDetail(receipt1, order1.ACO_OH_Debtor, batch.ACB_AB, order1.ACO_RX_NKCurrency, 1m, $"{batch.ACB_BatchNumber}/{order1.ACO_OrderNumber}", 60m, 60m, -60m, 0m, runDate, runDate.Date, runDate.Date);
			AssertEquals(batch.ACB_BatchNumber, order1.ACO_BatchNumber);
			AssertEquals(batch.BankAccount.AB_Code, order1.ACO_Calc_AB_Code);
			AssertEquals(0m, invoice1.AH_OutstandingAmount);
			AssertEquals(runDate, invoice1.AH_FullyPaidDate);
			AssertEquals(0m, invoice2.AH_OutstandingAmount);
			AssertEquals(runDate, invoice2.AH_FullyPaidDate);
			AssertNotNull(receipt1.RelatedDepositBatch);
			AssertMatchLinks(Factory, receipt1.PK, -60m, invoice1.PK, 20m, invoice2.PK, 30m, journal.PK, 10m);
			var logs = batch.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_Reference, $"Receipt and Deposit Batch created for Order Number {order1.ACO_OrderNumber}."));
			AssertEquals(1, logs.Length);

			AssertEquals(50m, order2.ACO_Amount);
			AssertEquals(20m, invoice3.AH_OutstandingAmount);
			AssertEquals(30m, invoice4.AH_OutstandingAmount);
			var receipt2 = order2.CreateReceiptsAndDepositBatch(newFactory, runDate, runDate, true);
			AssertNotNull(receipt2);
			AssertReceiptDetail(receipt2, order2.ACO_OH_Debtor, batch.ACB_AB, order2.ACO_RX_NKCurrency, 1m, $"{batch.ACB_BatchNumber}/{order2.ACO_OrderNumber}", 50m, 50m, -50m, 0m, runDate, runDate.Date, runDate.Date);
			AssertEquals(batch.ACB_BatchNumber, order2.ACO_BatchNumber);
			AssertEquals(batch.BankAccount.AB_Code, order2.ACO_Calc_AB_Code);
			AssertEquals(0m, invoice3.AH_OutstandingAmount);
			AssertEquals(runDate, invoice3.AH_FullyPaidDate);
			AssertEquals(0m, invoice4.AH_OutstandingAmount);
			AssertEquals(runDate, invoice4.AH_FullyPaidDate);
			AssertEquals(0m, journal.AH_OutstandingAmount);
			AssertEquals(runDate, journal.AH_FullyPaidDate);
			AssertNull(receipt2.RelatedDepositBatch);
			AssertMatchLinks(Factory, receipt2.PK, -50m, invoice3.PK, 20m, invoice4.PK, 30m);
			logs = batch.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_Reference, $"Receipt and Deposit Batch created for Order Number {order2.ACO_OrderNumber}."));
			AssertEquals(0, logs.Length);
		}

		public void TestCreateReceiptsAndDepositBatchThrowException()
		{
			var today = ZDate.Today;
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			Factory.RefreshEnabled = false;
			AssertEquals(60m, order1.ACO_Amount);
			AssertEquals(20m, invoice1.AH_OutstandingAmount);
			AssertEquals(30m, invoice2.AH_OutstandingAmount);
			AssertEquals(10m, journal.AH_OutstandingAmount);

			ARReceipt receipt = null;

			newFactory.Saving += x =>
			{
				invoice1.AH_Desc += "added more description for concurrency error";
				invoice2.AH_Desc += "added more description for concurrency error";
				journal.AH_Desc += "added more description for concurrency error";
				Factory.Save();
			};

			var exThrown = false;
			try
			{
				receipt = order1.CreateReceiptsAndDepositBatch(newFactory, today, today, false);
			}
			catch (ReceiptMatchingProcessFailedException ex)
			{
				exThrown = true;
				AssertEquals(RibaProcessErrorMessages.ReceiptMatchingProcessFailedDueToSaveException(order1.ACO_OrderNumber), ex.UserFriendlyMessage);
			}
			Assert("ReceiptMatchingProcessFailedException should be thrown when concurrency error occurs", exThrown);
			AssertNull(receipt);
			exThrown = false;

			AssertEquals(60m, order1.ACO_Amount);
			invoice1.AH_OutstandingAmount = 30;
			invoice2.AH_OutstandingAmount = 25;
			journal.AH_OutstandingAmount = 10;

			try
			{
				receipt = order1.CreateReceiptsAndDepositBatch(newFactory, today, today, false);
			}
			catch (ReceiptMatchingProcessFailedException ex)
			{
				exThrown = true;
				AssertEquals(RibaProcessErrorMessages.ReceiptMatchingProcessFailedException(order1.ACO_OrderNumber), ex.UserFriendlyMessage);
			}
			Assert("ReceiptMatchingProcessFailedException should be thrown when amounts do not total to 0", exThrown);
			AssertNull(receipt);
			exThrown = false;

			AssertEquals(60m, order1.ACO_Amount);
			invoice1.AH_OutstandingAmount = 0;
			invoice1.AH_FullyPaidDate = today;
			invoice2.AH_OutstandingAmount = 0;
			invoice2.AH_FullyPaidDate = today;
			journal.AH_OutstandingAmount = 0;
			journal.AH_FullyPaidDate = today;

			try
			{
				receipt = order1.CreateReceiptsAndDepositBatch(newFactory, today, today, false);
			}
			catch (OrderIsAlreadyCancelledOrPaidException ex)
			{
				exThrown = true;
				AssertEquals(RibaProcessErrorMessages.OrderIsAlreadyCancelledOrPaidExceptionMessage(order1.ACO_OrderNumber), ex.UserFriendlyMessage);
			}
			Assert("ReceiptMatchingProcessFailedException should be thrown when transactions are already paid", exThrown);
			AssertNull(receipt);
		}

		[TestDate(2019, 07, 10)]
		public void TestPostDateAndInvoiceDateForCreateReceiptsAndDepositBatch()
		{
			var today = ZDate.Today;
			var yesterday = today.AddDays(-1);
			var oneMonthAgo = today.AddMonths(-1);

			TestObjectCreator.CreateTestPeriodsForEntireYear(today.Year);
			order1.ACO_DepositedDate = oneMonthAgo;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receipt1 = order1.CreateReceiptsAndDepositBatch(newFactory, today, yesterday, false);
			AssertEquals(oneMonthAgo, receipt1.AH_PostDate);
			AssertEquals(oneMonthAgo, receipt1.AH_InvoiceDate);

			Assert(order2.ACO_DepositedDate.IsEmpty);
			var receipt2 = order2.CreateReceiptsAndDepositBatch(newFactory, today, yesterday, false);
			AssertEquals(today, receipt2.AH_PostDate);
			AssertEquals(today.AddDays(-1), receipt2.AH_InvoiceDate);

			var depositBatchQuery = new ZQuery(AccTransactionHeaderSchema.AH_AB, batch.ACB_AB);
			depositBatchQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ReceiptBatch);
			var depositBatches = Factory.Load<DepositBatch>(depositBatchQuery);

			var expectedPostDate = new[] { oneMonthAgo, today };
			var expectedInvoiceDate = new[] { oneMonthAgo, yesterday };
			AssertEquals(2, depositBatches.Length);
			AssertContainsExactElementsInAnyOrder(expectedPostDate, depositBatches.Select(x => x.AH_PostDate));
			AssertContainsExactElementsInAnyOrder(expectedInvoiceDate, depositBatches.Select(x => x.AH_InvoiceDate));
		}

		internal static void AssertReceiptDetail(ARReceipt receipt, ZGuid debtorPK, ZGuid bankPK, ZString currency, ZDecimal exchangeRate, ZString reference, ZDecimal oSExTaxAmount, ZDecimal localExTaxAmount, ZDecimal invoiceAmount, ZDecimal outstandingAmount, ZDateTime fullyPaidDate, ZDate postDate, ZDate invoiceDate)
		{
			AssertEquals(debtorPK, receipt.AH_OH);
			AssertEquals(bankPK, receipt.AH_AB);
			AssertEquals(currency, receipt.AH_RX_NKTransactionCurrency);
			AssertEquals(exchangeRate, receipt.AH_ExchangeRate);
			AssertEquals(ReceiptTypes.DirectCredit, receipt.AH_ReceiptType);
			AssertEquals(reference, receipt.AH_ChequeOrReference);
			AssertEquals(oSExTaxAmount, receipt.AH_OSExTaxAmount);
			AssertEquals(localExTaxAmount, receipt.AH_LocalExTaxAmount);
			AssertEquals(invoiceAmount, receipt.AH_InvoiceAmount);
			AssertEquals(outstandingAmount, receipt.AH_OutstandingAmount);
			AssertEquals(fullyPaidDate, receipt.AH_FullyPaidDate);
			AssertEquals(postDate, receipt.AH_PostDate);
			AssertEquals(invoiceDate, receipt.AH_InvoiceDate);
		}

		internal static void AssertMatchLinks(BusinessObjectFactory factory, ZGuid receiptPK, ZDecimal receiptAmount, ZGuid invoice1PK, ZDecimal invoice1Amount, ZGuid invoice2PK, ZDecimal invoice2Amount,
			ZGuid? journalPK = null, ZDecimal? journalAmount = null)
		{
			// Pull out the Matchlink for Original ARReceiptRowMatch
			var aRReceiptRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, receiptPK);
			var aRReceiptRowMatch = factory.LoadTop1<TransactionMatchLink>(aRReceiptRowMatchFilter);
			AssertNotNull("Original ARReceiptRow should be matched", aRReceiptRowMatch);
			AssertEquals("Match amount of Original ARReceiptRow matchlink should be matched", receiptAmount, aRReceiptRowMatch.AP_Amount);
			// Pull out the Matchlink for Original ARInvoice1RowMatch
			var aRInvoice1RowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice1PK);
			var aRInvoice1RowMatch = factory.LoadTop1<TransactionMatchLink>(aRInvoice1RowMatchFilter);
			AssertNotNull("Original ARReceiptRow should be matched", aRInvoice1RowMatch);
			AssertEquals("Match amount of Original ARReceiptRow matchlink should be matched", invoice1Amount, aRInvoice1RowMatch.AP_Amount);
			// Pull out the Matchlink for Original ARInvoice2RowMatch
			var aRInvoice2RowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice2PK);
			var aRInvoice2RowMatch = factory.LoadTop1<TransactionMatchLink>(aRInvoice2RowMatchFilter);
			AssertNotNull("Original ARReceiptRow should be matched", aRInvoice2RowMatch);
			AssertEquals("Match amount of Original ARReceiptRow matchlink should be matched", invoice2Amount, aRInvoice2RowMatch.AP_Amount);
			// Pull out the Matchlink for Original ARJournalRowMatch (if any)
			if (journalPK.HasValue)
			{
				var aRJournalRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, journalPK.Value);
				var aRJournalRowMatch = factory.LoadTop1<TransactionMatchLink>(aRJournalRowMatchFilter);
				AssertNotNull("Original ARReceiptRow should be matched", aRJournalRowMatch);
				AssertEquals("Match amount of Original ARReceiptRow matchlink should be matched", journalAmount.Value, aRJournalRowMatch.AP_Amount);
			}
			AssertEquals(aRReceiptRowMatch.AP_MatchGroupNum, aRInvoice1RowMatch.AP_MatchGroupNum);
			AssertEquals(aRReceiptRowMatch.AP_MatchGroupNum, aRInvoice2RowMatch.AP_MatchGroupNum);
		}

		[TestDate(2016, 03, 25)]
		public void TestUMRReferenceFields()
		{
			var today = ZDateTime.Today;
			var document1 = Factory.NewWithValidTestData<JobRequiredDocument>();
			document1.EQ_DocType = "UMR";
			document1.EQ_DocUsage = "DBT";
			document1.EQ_DocPeriod = "PER";
			document1.EQ_ValidToDate = today.AddMonths(2);
			document1.EQ_DocNumber = "123456";
			document1.EQ_DateReceived = today.ToDateTimeOffset(null);
			TestObjectCreator.ABIGAS.RequiredDocuments.Add(document1);
			AssertEquals("123456", order1.CollectionRequestUMRReference);
			AssertEquals(today, order1.CollectionRequestUMRSignedDate);
			AssertEquals("123456", order2.CollectionRequestUMRReference);
			AssertEquals(today, order2.CollectionRequestUMRSignedDate);
			document1.EQ_ValidToDate = today.AddMonths(-2);
			AssertNullOrEmpty(order1.CollectionRequestUMRReference);
			AssertEquals(ZDateTime.Empty, order1.CollectionRequestUMRSignedDate);
			AssertNullOrEmpty(order2.CollectionRequestUMRReference);
			AssertEquals(ZDateTime.Empty, order2.CollectionRequestUMRSignedDate);
		}

		public void TestACO_Amount()
		{
			order1.ACO_Amount = 0m;
			order2.ACO_Amount = 0m;
			AssertEquals(0m, batch.ACB_TotalAmount);
			order1.IncludeInBatch = true;
			order1.ACO_Amount = 100m;
			AssertEquals(100m, batch.ACB_TotalAmount);
			order2.IncludeInBatch = true;
			order2.ACO_Amount = 200m;
			AssertEquals(300m, batch.ACB_TotalAmount);
			order1.ACO_Amount = 50m;
			AssertEquals(250m, batch.ACB_TotalAmount);
			order2.ACO_Amount = -20m;
			AssertEquals(30m, batch.ACB_TotalAmount);
		}

		public void TestPreventDelete()
		{
			var order = Factory.NewWithValidTestData<AccCollectionOrder>();
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(AccCollectionOrder)));
		}

		public void TestAutoLoggedForCollectionOrder()
		{
			var addedQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, "ADD");
			addedQuery.AddToFilter(new ZQuery(StmALogSchema.SL_Parent, order1.PK));
			var logs = order1.Logs.GetAllLogs().Find(addedQuery);
			AssertEquals(1, logs.Length);

			order1.ACO_CollectionDate = ZDateTime.Today.Date.AddDays(1);
			Factory.Save();

			var editedQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, "EDT");
			editedQuery.AddToFilter(new ZQuery(StmALogSchema.SL_Parent, order1.PK));
			logs = order1.Logs.GetAllLogs().Find(editedQuery);
			AssertEquals(1, logs.Length);
		}

		public void TestACO_AmountReadOnly()
		{
			Assert("Should always be read only", order1.ACO_AmountInfo.ReadOnly);
		}

		public void TestDescription()
		{
			AssertEquals("Collection Order", order1.Description);
		}

		public void TestBank()
		{
			batch.ACB_AB = TestObjectCreator.AUDBankAccount.PK;
			AssertEquals(TestObjectCreator.AUDBankAccount.PK, order1.BankAccount);
		}

		public void TestIncludeInOrder()
		{
			AssertEquals(60m, order1.ACO_Amount);

			line1_2.IncludeInOrder = false;
			AssertEquals(30m, order1.ACO_Amount);

			AssertEquals(3, order1.CollectionOrderLines.Count);

			Factory.Save();
			var newOrderLines = Factory.Load<AccCollectionOrderLine>(new ZQuery(AccCollectionOrderLineSchema.AOL_ACO, order1.PK));
			AssertEquals(2, newOrderLines.Length);
		}

		public void TestWorkflowType()
		{
			AssertEquals(WorkflowDescriptors.CollectionOrderCode, ((IWorkflowProviderCore)order1).WorkflowType);
		}

		public void TestDocManagerInfo()
		{
			var order = (AccCollectionOrder)GetNewBusinessObject();
			AssertEquals("DocManagerInfo.GetType()", typeof(DocManagerInfo), ((IDocManagerSupport)order).DocManagerInfo.GetType());
			AssertEquals("DocManagerInfo.DocManagerCode", Core.Constants.DocManagerCodes.CollectionOrder, ((IDocManagerSupport)order).DocManagerInfo.DocManagerCode);
		}

		public void TestIncludeInBatch()
		{
			order1.IncludeInBatch = false;
			order1.ACO_Amount = 30m;
			order1.IsCancelled = false;
			order2.IncludeInBatch = false;
			order2.ACO_Amount = 40m;
			order2.IsCancelled = false;
			batch.ACB_TotalAmount = 100m;
			order1.IncludeInBatch = true;
			AssertEquals(130m, batch.ACB_TotalAmount);
			order2.IncludeInBatch = true;
			AssertEquals(170m, batch.ACB_TotalAmount);
			order1.IncludeInBatch = false;
			AssertEquals(140m, batch.ACB_TotalAmount);
			order2.IncludeInBatch = false;
			AssertEquals(100m, batch.ACB_TotalAmount);
		}

		public void TestCollectionOrderLines()
		{
			batch.ACB_TotalAmount = 100m;
			order1.ACO_Amount = 40m;
			order2.ACO_Amount = 60m;
			Factory.Save();

			var orders = batch.CollectionOrders;
			AssertEquals(2, orders.Count);
			AssertEquals(5, orders[0].CollectionOrderLines.Count + orders[1].CollectionOrderLines.Count);
			foreach (var ord in orders)
			{
				foreach (var line in ord.CollectionOrderLines)
				{
					Assert(line.IncludeInOrder);
				}
			}
		}

		public void TestDebtorValidationType()
		{
			var collectionOrder1 = TestObjectCreator.CreateCollectionOrder(batch, ZDateTime.Today.Date, TestObjectCreator.Debtor, "0000051", 50m, false);
			var collectionOrder2 = TestObjectCreator.CreateCollectionOrder(batch, ZDateTime.Today.Date, TestObjectCreator.Debtor, "0000052", 50m, false);
			AssertEquals("DebtorValidationType by default", DebtorValidation.NoDebtorValidation, collectionOrder1.DebtorValidationType);
			AssertEquals("DebtorValidationType by default", DebtorValidation.NoDebtorValidation, collectionOrder2.DebtorValidationType);
			Assert("Precondition: ", !collectionOrder1.IsInDatabase);
			Assert("Precondition: ", !collectionOrder2.IsInDatabase);

			collectionOrder1.DebtorValidationType = DebtorValidation.DebtorIsRequired;
			collectionOrder2.DebtorValidationType = DebtorValidation.DebtorShouldBeEmpty;
			collectionOrder2.ACO_OH_Debtor = ZGuid.Empty;
			AssertNotEquals("Precondition: DebtorValidationType", DebtorValidation.NoDebtorValidation, collectionOrder1.DebtorValidationType);
			AssertNotEquals("Precondition: DebtorValidationType", DebtorValidation.NoDebtorValidation, collectionOrder2.DebtorValidationType);
			Factory.Save();

			Assert("Precondition: ", collectionOrder1.IsInDatabase);
			Assert("Precondition: ", collectionOrder2.IsInDatabase);
			AssertNotEquals(DebtorValidation.NoDebtorValidation, collectionOrder1.DebtorValidationType);
			AssertNotEquals(DebtorValidation.NoDebtorValidation, collectionOrder2.DebtorValidationType);

			var newFactory = new BusinessObjectFactory();
			var c_order1 = newFactory.Load<AccCollectionOrder>(collectionOrder1.PK);
			var c_order2 = newFactory.Load<AccCollectionOrder>(collectionOrder2.PK);
			AssertEquals("When loading a Collection Order, the DebtorValidationType should default to NoDebtorValidation, so that the user can edit a Collection Batch with no Debtor on Orders", DebtorValidation.NoDebtorValidation, c_order1.DebtorValidationType);
			AssertEquals("When loading a Collection Order, the DebtorValidationType should default to NoDebtorValidation, so that the user can edit a Collection Batch with no Debtor on Orders", DebtorValidation.NoDebtorValidation, c_order2.DebtorValidationType);
		}

		public void TestOrder_ReadOnly()
		{
			order1.IsCancelled = false;
			Assert(!order1.Order_ReadOnly);
			order1.IsCancelled = true;
			Assert(order1.Order_ReadOnly);

			order1.IsCancelled = false;
			order1.ACO_DepositedDate = ZDate.Empty;
			Assert(!order1.Order_ReadOnly);
			order1.ACO_DepositedDate = ZDate.Today;
			Assert(order1.Order_ReadOnly);

			order1.ACO_DepositedDate = ZDate.Empty;
			invoice1.AH_OutstandingAmount = 0m;
			invoice1.AH_FullyPaidDate = ZDateTime.Today;
			Assert(order1.Order_ReadOnly);
		}

		public void TestDelete()
		{
			order1.ACO_Amount = 30m;
			order1.IncludeInBatch = true;
			order2.ACO_Amount = 40m;
			order2.IncludeInBatch = false;
			batch.ACB_TotalAmount = 100m;
			order1.Delete();
			AssertEquals(70m, batch.ACB_TotalAmount);
			order2.Delete();
			AssertEquals(70m, batch.ACB_TotalAmount);
		}

		public void TestReject()
		{
			AssertEquals(110m, batch.ACB_TotalAmount);
			order1.Reject("my reject reason 1");
			Assert(order1.IsCancelled);
			AssertEquals("my reject reason 1", order1.ACO_CancelledReason);
			Assert(line1_1.IsCancelled);
			Assert(line1_2.IsCancelled);
			Assert(line1_3.IsCancelled);
			Assert(!batch.IsCancelled);

			AssertEquals(50m, batch.ACB_TotalAmount);
			order2.Reject("my reject reason 2");
			AssertEquals("my reject reason 2", order2.ACO_CancelledReason);
			Assert(order2.IsCancelled);
			Assert(line2_1.IsCancelled);
			Assert(line2_2.IsCancelled);

			Factory.Save();
			var logs = batch.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_Reference, $"Order Number {order1.ACO_OrderNumber} rejected."));
			AssertEquals(1, logs.Length);
			logs = batch.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_Reference, $"Order Number {order2.ACO_OrderNumber} rejected."));
			AssertEquals(1, logs.Length);
			Assert(batch.IsCancelled);
			AssertEquals(0m, batch.ACB_TotalAmount);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Deletion is tested in TestDelete method", true);
		}

		AccCollectionBatch batch;
		AccCollectionOrder order1;
		AccCollectionOrder order2;
		AccCollectionOrderLine line1_1;
		AccCollectionOrderLine line1_2;
		AccCollectionOrderLine line1_3;
		AccCollectionOrderLine line2_1;
		AccCollectionOrderLine line2_2;
		AccBankAccount bankAccount;
		ARInvoice invoice1;
		ARInvoice invoice2;
		ARInvoice invoice3;
		ARInvoice invoice4;
		ARJournal journal;

		protected override void SetUp()
		{
			base.SetUp();
			bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			batch = TestObjectCreator.CreateCollectionBatch(bankAccount, GlbCompany.CurrentCompany, "00001001", 110m, false);
			var now = ZDateTime.Now;
			var debtor = TestObjectCreator.ABIGAS;
			var refCurr = TestObjectCreator.AUD;
			var chargeCode = TestObjectCreator.CC1.PK;

			invoice1 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", refCurr, 1M, 20, 0M, 20, 0M, debtor, chargeCode, now, ZDateTime.Empty, now, false);
			invoice2 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", refCurr, 1M, 30, 0M, 30, 0M, debtor, chargeCode, now, ZDateTime.Empty, now, false);
			invoice3 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", refCurr, 1M, 20, 0M, 20, 0M, debtor, chargeCode, now, ZDateTime.Empty, now, false);
			invoice4 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV4", refCurr, 1M, 30, 0M, 30, 0M, debtor, chargeCode, now, ZDateTime.Empty, now, false);
			journal = TestObjectCreator.CreateJournal<ARJournal>(10m, now, debtor.PK);

			order1 = TestObjectCreator.CreateCollectionOrder(batch, now.Date, debtor, "0000001", 60m, false);
			line1_1 = TestObjectCreator.CreateCollectionOrderLine(order1, invoice1, false);
			line1_2 = TestObjectCreator.CreateCollectionOrderLine(order1, invoice2, false);
			line1_3 = TestObjectCreator.CreateCollectionOrderLine(order1, journal, false);

			order2 = TestObjectCreator.CreateCollectionOrder(batch, now.Date.AddDays(1), debtor, "0000002", 50m, false);
			line2_1 = TestObjectCreator.CreateCollectionOrderLine(order2, invoice3, false);
			line2_2 = TestObjectCreator.CreateCollectionOrderLine(order2, invoice4, false);

			Factory.Save();
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
	}

	[TestedType(typeof(AccCollectionOrder))]
	class AccCollectionOrderWorkflowProviderTest : WorkflowProviderTest<AccCollectionOrder, AccCollectionOrderProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => new AccCollectionOrderWorkflowDescriptor().Code;
	}
}
