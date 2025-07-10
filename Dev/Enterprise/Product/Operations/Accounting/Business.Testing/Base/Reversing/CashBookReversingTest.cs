using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class CashBookReversingTest : ReversingBaseTest
	{
		#region TestCanTransactionBeReversed

		public void TestCanTransactionBeReversedForICashBook()
		{
			Assert("Transaction should be reversable since it has not been reversed or cleared in cashbook", CashBookReversing.CanReverseTransaction);
			AssertEquals("Error message should be empty", ZString.Empty, CashBookReversing.GenerateCantReverseErrorMessage_ForTestOnly());

			TestIReversingInstance.SetIsClearedInCashbook(true);
			Assert("Transaction should not be reversable since it has been cleared in cashbook", !CashBookReversing.CanReverseTransaction);
			AssertEquals("Error message should be cleared in cashbook", CashBookReversing.ClearedInCashBookErrorMessage_ForTestOnly, CashBookReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestCanTransactionBeReversedForBankTransfer()
		{
			BankTransfer testBankTransfer = new BankTransfer(Factory, null);
			testBankTransfer.BankTransferFromPK = TestObjectCreator.AUDBankAccount.PK;
			testBankTransfer.BankTransferToPK = TestObjectCreator.USDBankAccount.PK;
			testBankTransfer.TransferRowFrom.AH_IsCancelled = true;
			Reversing = ReversingFactory.NewReversing(testBankTransfer);
			Assert("Transaction should not be reversable since it has already been canceled.", !CashBookReversing.CanReverseTransaction);
			AssertEquals("Error message should be for canceled transaction", CashBookReversing.AlreadyCancelledMessage_ForTestOnly, CashBookReversing.GenerateCantReverseErrorMessage_ForTestOnly());

			testBankTransfer.TransferRowFrom.AH_IsCancelled = false;
			Assert("Transaction should be reversable since it has not been canceled.", CashBookReversing.CanReverseTransaction);
			AssertEquals("Error message should be empty", "", CashBookReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestCanReverseWhenBatchContainsCancelledPayments()
		{
			DirectDebitBatchHeader testBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			Factory.Save();

			APPayment paymentCancelled = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			APPayment paymentUnCancelled = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;

			paymentCancelled.AH_IsCancelled = ZBool.True;
			((IMatching)paymentCancelled).CurrentMatchGroup.AddNew().AP_AH = paymentCancelled.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(paymentCancelled);
			paymentCancelled.AH_ReceiptBatchNo = testBatch.AH_TransactionNum;
			paymentCancelled.AH_ReceiptType = ReceiptTypes.DirectDebit;

			paymentUnCancelled.AH_DateClearedInCashbook = ZDateTime.Empty;
			paymentUnCancelled.AH_ReceiptBatchNo = testBatch.AH_TransactionNum;
			paymentUnCancelled.AH_ReceiptType = ReceiptTypes.DirectDebit;

			Factory.Save();

			Reversing = ReversingFactory.NewReversing(testBatch);
			Assert("Transaction should not be reversable.", !CashBookReversing.CanReverseTransaction);
			AssertEquals("Error message should be for cleared payment", "You cannot cancel this DDR batch because one or more payments in the batch are canceled.",
				CashBookReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestCanReverseIfClearedInCashbook_IndividualPaymentCancelled()
		{
			DirectDebitBatchHeader testBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			Factory.Save();

			APPayment paymentCleared = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			APPayment paymentUncleared = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;

			paymentCleared.AH_DateClearedInCashbook = new ZDateTime(2006, 1, 15);
			paymentCleared.AH_ReceiptBatchNo = testBatch.AH_TransactionNum;
			paymentCleared.AH_ReceiptType = ReceiptTypes.DirectDebit;

			paymentUncleared.AH_DateClearedInCashbook = ZDateTime.Empty;
			paymentUncleared.AH_ReceiptBatchNo = testBatch.AH_TransactionNum;
			paymentUncleared.AH_ReceiptType = ReceiptTypes.DirectDebit;

			Factory.Save();

			Reversing = ReversingFactory.NewReversing(testBatch);
			Assert("Transaction should not be reversable.", !CashBookReversing.CanReverseTransaction);
			AssertEquals("Error message should be for cleared payments", "You cannot cancel DDR Batch because some of the Payments / Direct Payments included in the batch is already cleared in cashbook.",
				CashBookReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestCanReverseIfNotClearedInCashbook_IndividualPaymentCancelled()
		{
			DirectDebitBatchHeader testBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testBatch.AH_DateClearedInCashbook = ZDateTime.Empty;
			Factory.Save();

			APPayment paymentUncleared = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			APPayment paymentUncleared2 = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;

			paymentUncleared.AH_DateClearedInCashbook = ZDateTime.Empty;
			paymentUncleared.AH_ReceiptBatchNo = testBatch.AH_TransactionNum;
			paymentUncleared.AH_ReceiptType = ReceiptTypes.DirectDebit;

			paymentUncleared2.AH_DateClearedInCashbook = ZDateTime.Empty;
			paymentUncleared2.AH_ReceiptBatchNo = testBatch.AH_TransactionNum;
			paymentUncleared2.AH_ReceiptType = ReceiptTypes.DirectDebit;

			ARReceipt receiptCleaderd = Factory.NewWithValidTestData(typeof(ARReceipt)) as ARReceipt;

			receiptCleaderd.AH_DateClearedInCashbook = new ZDateTime(2006, 1, 15);
			receiptCleaderd.AH_ReceiptBatchNo = testBatch.AH_TransactionNum;
			receiptCleaderd.AH_ReceiptType = ReceiptTypes.DirectDebit;

			Factory.Save();

			Reversing = ReversingFactory.NewReversing(testBatch);
			Assert("Transaction should be reversable.", CashBookReversing.CanReverseTransaction);
			AssertEquals("Error message should be empty", "", CashBookReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TesCanReverseIfNotClearedInCashbook()
		{
			DirectDebitBatchHeader testBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testBatch.AH_DateClearedInCashbook = ZDateTime.Empty;
			Factory.Save();

			Assert("Transaction should be reversable.", CashBookReversing.CanReverseTransaction);
			AssertEquals("Error message should be empty", "", CashBookReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		#endregion

		public void TestOpeningReceiptReversing()
		{
			OpeningReceipt orc = Factory.New<OpeningReceipt>();
			orc.AH_OSExTaxAmount = 40m; // -ve in database
			orc.AH_LocalOutstandingAmount = 40m;

			ReversingFactory revFactory = new ReversingFactory();
			ReversingBase cashbookReverser = revFactory.NewReversing(orc);
			cashbookReverser.Reverse();

			OpeningReceipt reversingOrc = GetReverseTransaction(orc) as OpeningReceipt;
			AssertNotNull("Reversing Opening Receipt should have been created", reversingOrc);
			AssertEquals("InvoiceAmount on ReversingOrc should be 40", 40m, reversingOrc.AH_InvoiceAmount);
			Assert("Reversing Orc should be cancelled", reversingOrc.AH_IsCancelled);
		}

		public void TestOpeningPaymentReversing()
		{
			OpeningPayment opy = Factory.NewWithValidTestData<OpeningPayment>();
			opy.AH_OSExTaxAmount = 60m; // positive in DB
			opy.AH_LocalOutstandingAmount = 60m;

			ReversingFactory revFactory = new ReversingFactory();
			ReversingBase cashbookReverser = revFactory.NewReversing(opy);
			cashbookReverser.Reverse();

			OpeningPayment reversingDpy = GetReverseTransaction(opy) as OpeningPayment;
			AssertNotNull("Reversing Opening Payment should be created", reversingDpy);
			AssertEquals("InvoiceAmount on ReversingOpy should be -60", -60m, reversingDpy.AH_InvoiceAmount);
			Assert("Reversing Opy should be cancelled", reversingDpy.AH_IsCancelled);
		}

		public void TestReversingDirectReceipt()
		{
			DirectReceipt drc = Factory.New<DirectReceipt>();
			drc.Lines.AddNew();
			drc.Lines[0].AL_OSExTaxAmount = 20m;
			drc.Lines[0].AL_LocalExTaxAmount = 20m;
			drc.Lines.AddNew();
			drc.Lines[1].AL_OSExTaxAmount = 30m;
			drc.Lines[1].AL_LocalExTaxAmount = 30m;

			drc.AH_OSExTaxAmount = 50m; // positive in DB
			drc.AH_LocalOutstandingAmount = 50m;

			ReversingFactory revFactory = new ReversingFactory();
			ReversingBase cashbookReverser = revFactory.NewReversing(drc);
			cashbookReverser.Reverse();

			DirectReceipt reversingDrc = GetReverseTransaction(drc) as DirectReceipt;
			AssertNotNull("Reversing DirectReceipt should be created", reversingDrc);
			AssertEquals("InvoiceAmount on ReversingDrc should be -50", -50m, reversingDrc.AH_InvoiceAmount);
			Assert("ReversingDrc should be cancelled", reversingDrc.AH_IsCancelled);
		}

		public void TestReversingDirectPayment()
		{
			DirectPayment dpy = Factory.NewWithValidTestData<DirectPayment>();
			dpy.Lines.AddNew();
			dpy.Lines[0].AL_OSExTaxAmount = 15m;
			dpy.Lines[0].AL_LocalExTaxAmount = 15m;
			dpy.Lines.AddNew();
			dpy.Lines[1].AL_OSExTaxAmount = 25m;
			dpy.Lines[1].AL_LocalExTaxAmount = 25m;

			dpy.AH_OSExTaxAmount = 40m; // negative in DB
			dpy.AH_LocalOutstandingAmount = 40m;

			ReversingFactory revFactory = new ReversingFactory();
			ReversingBase cashbookReverser = revFactory.NewReversing(dpy);
			cashbookReverser.Reverse();

			DirectPayment reversingDpy = GetReverseTransaction(dpy) as DirectPayment;
			AssertNotNull("Reversing Direct payment should be created", reversingDpy);
			AssertEquals("InvoiceAmount on ReversingDpy should be 40", 40m, reversingDpy.AH_InvoiceAmount);
			Assert("ReversingDpy should be cancelled", reversingDpy.AH_IsCancelled);
		}

		[TestDate(2023, 01, 01)]
		public void TestReversingBankTransferCashbookExchangeDiff()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);

			var bankTransferWithoutExchangeVariance = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 500m, 1.0m);

			var bankTransferWithExchangeVariance = TestObjectCreator.CreateBankTransfer(ZDateTime.Today, TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 1000m, 1.0m);
			bankTransferWithExchangeVariance.ShouldCalculateExchangeVariance = true;
			bankTransferWithExchangeVariance.LocalBuyAmount = 200m;
			bankTransferWithExchangeVariance.LocalSellAmount = 100m;

			Factory.Save();

			AssertEquals(true, bankTransferWithExchangeVariance.ExchangeDiff.IsInDatabase);
			AssertEquals(false, bankTransferWithoutExchangeVariance.ExchangeDiff.IsInDatabase);
			AssertNotEquals("The transaction numbers should be different", bankTransferWithExchangeVariance.ExchangeDiff.AH_TransactionNum, bankTransferWithExchangeVariance.TransactionNumber);

			var revFactory = new ReversingFactory();
			var reversingBase1 = revFactory.NewReversing(bankTransferWithExchangeVariance.ExchangeDiff);
			var reversingBase2 = revFactory.NewReversing(bankTransferWithoutExchangeVariance.ExchangeDiff);

			AssertEquals(false, reversingBase1.CanReverseTransaction);
			AssertEquals(true, reversingBase2.CanReverseTransaction);

			AssertEquals("This EXX Transaction was created by Bank Transfer 00001001, and can only be reversed by reversing the Bank Transfer.", reversingBase1.CantReverseErrorMessage);
			AssertNullOrEmpty(reversingBase2.CantReverseErrorMessage);
		}

		#region Implementation

		TransactionHeader GetReverseTransaction(TransactionHeader transaction)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, transaction.PK);
			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory, filter);
			transactions.Load();
			AssertEquals("There should be 1 reversing transaction", 1, transactions.Count);
			return transactions[0];
		}

		protected override Type GetTestingClassType()
		{
			return typeof(CashBookReversing);
		}

		protected override void SetupReversingIReversingInstance()
		{
			TestReversingIReversingInstance = new TestICashBookTransaction();
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = new TestICashBookTransaction();
		}

		CashBookReversing CashBookReversing
		{
			get { return Reversing as CashBookReversing; }
		}

		#endregion
	}
}
