using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class DirectReceiptReversingTest : CashBookReversingTest
	{
		public void TestReversedDepositBatchWhenReverseTransaction()
		{
			var bankAccountPK = TestObjectCreator.USDBankAccount.PK;

			var directReceipt01 = Factory.NewWithValidTestData<DirectReceipt>();
			directReceipt01.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			directReceipt01.AH_ReceiptBatchNo = "00001580";
			directReceipt01.AH_AB = bankAccountPK;
			directReceipt01.ExchangeRate.Rate = 0.7m;
			var directReceipt01Line = (DirectReceiptLine)directReceipt01.Lines.AddNew();
			directReceipt01Line.AL_OSExTaxAmount = 10m;
			directReceipt01Line.AL_OSTaxAmount = 5m;

			var directReceipt02 = Factory.NewWithValidTestData<DirectReceipt>();
			directReceipt02.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			directReceipt02.AH_ReceiptBatchNo = "00001580";
			directReceipt02.AH_AB = bankAccountPK;
			directReceipt02.ExchangeRate.Rate = 0.7m;
			var directReceipt02Line = (DirectReceiptLine)directReceipt02.Lines.AddNew();
			directReceipt02Line.AL_OSExTaxAmount = 15m;
			directReceipt02Line.AL_OSTaxAmount = 5m;

			var directReceipt03 = Factory.NewWithValidTestData<DirectReceipt>();
			directReceipt03.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			directReceipt03.AH_ReceiptBatchNo = "00001580";
			directReceipt03.AH_AB = bankAccountPK;
			directReceipt03.ExchangeRate.Rate = 0.7m;
			var directReceipt03Line = (DirectReceiptLine)directReceipt03.Lines.AddNew();
			directReceipt03Line.AL_OSExTaxAmount = 20m;
			directReceipt03Line.AL_OSTaxAmount = 5m;
			Factory.Save();

			AssertEquals("PreCondition", 15m, directReceipt01.AH_OSTotal);
			AssertEquals("PreCondition", 20m, directReceipt02.AH_OSTotal);
			AssertEquals("PreCondition", 25m, directReceipt03.AH_OSTotal);
			AssertEquals("PreCondition", 14.29m, directReceipt01.AH_InvoiceAmount);
			AssertEquals("PreCondition", 21.43m, directReceipt02.AH_InvoiceAmount);
			AssertEquals("PreCondition", 28.57m, directReceipt03.AH_InvoiceAmount);
			AssertEquals("PreCondition", 7.14m, directReceipt01.AH_GSTAmount);
			AssertEquals("PreCondition", 7.14m, directReceipt02.AH_GSTAmount);
			AssertEquals("PreCondition", 7.14m, directReceipt03.AH_GSTAmount);

			var depositBatch01 = Factory.NewWithValidTestData<DepositBatch>();
			depositBatch01.AH_AB = bankAccountPK;
			depositBatch01.AH_TransactionNum = "00001580";
			depositBatch01.LoadTransactions(ZGuid.Empty);
			Factory.Save();

			AssertEquals("PreCondition: DRC InvoiceAmount + GSTAmount Sum", 85.71m, depositBatch01.AH_InvoiceAmount);
			AssertEquals("PreCondition: DRC OSTotal Sum", 60m, depositBatch01.AH_OSTotal);
			var filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.DirectReceipt).AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.CashBook);
			var receipts = new TransactionHeaderCollection(Factory, filter);
			receipts.Load();
			AssertEquals("PreCondition: There should be 3 DR", 3, receipts.Count);

			var testRecRev = new DirectReceiptReversing(directReceipt01);
			testRecRev.Reverse();
			Factory.Save();
			var reversedDepositBatches = new BusinessObjectFactory().Load<DepositBatch>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch).AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, depositBatch01.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

			AssertEquals("one Deposit batch created to reverse the transaction", 1, reversedDepositBatches.Length);
			AssertEquals("reversed batch AH_OSTotal is equals to the reversed transaction amount", -15m, reversedDepositBatches[0].AH_OSTotal);
			AssertEquals("reversed batch AH_InvoiceAmount is equals to the reversed transaction InvoiceAmount + GSTAmount", -21.43m, reversedDepositBatches[0].AH_InvoiceAmount);
		}

		public void TestReverseDirectCreditDirectReceipt()
		{
			DepositBatch testBatch = Factory.NewWithValidTestData<DepositBatch>();
			testBatch.AH_TransactionNum = "00001580";

			DirectReceipt testReceipt = Factory.NewWithValidTestData<DirectReceipt>();
			testReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			testReceipt.AH_ReceiptBatchNo = "00001580";

			DirectReceiptLine line = (DirectReceiptLine)testReceipt.Lines.AddNew();
			line.AL_OSExTaxAmount = 30m;
			line.AL_OSTaxAmount = 10m;

			var testDepositBatch = testReceipt.RelatedDepositBatch;
			testDepositBatch.AH_TransactionNum = "00001580";
			testDepositBatch.AH_ReceiptBatchNo = "00001580";
			testDepositBatch.AH_InvoiceAmount = 40m;
			testDepositBatch.AH_OSTotal = 40m;
			Factory.Save();

			testReceipt.Reload();

			ZQuery depBatFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, testReceipt.AH_ReceiptBatchNo);
			depBatFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			DepositBatch testDepBat = Factory.LoadTop1<DepositBatch>(depBatFilter);

			DirectReceiptReversing testRecRev = new DirectReceiptReversing(testReceipt);
			testRecRev.Reverse();
			Factory.Save();

			// Check the reversing Receipt
			ZQuery revRecFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.DirectReceipt);
			revRecFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, testReceipt.PK);

			TransactionHeaderCollection receipts = new TransactionHeaderCollection(Factory, revRecFilter);
			receipts.Load();
			AssertEquals("There should be one reversing receipt in the DB other than the original receipt", 1, receipts.Count);
			DirectReceipt revRec = (DirectReceipt)receipts[0];
			AssertEquals("Reversing Receipt should have local amount = -30", -30M, revRec.AH_InvoiceAmount);
			AssertEquals("Reversing Receipt should have GST amount = -10", -10M, revRec.AH_GSTAmount);
			AssertEquals("Reversing Receipt should have os amount = -40", -40M, revRec.AH_OSTotal);

			// Check the reversing deposit batch
			ZQuery revDepBatFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			revDepBatFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, testDepBat.PK);
			revDepBatFilter.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptBatchNo, SQLComparisonOperator.NotEqual, ZString.Empty);

			TransactionHeaderCollection depBats = new TransactionHeaderCollection(Factory, revDepBatFilter);
			depBats.Load();
			AssertEquals("There should be one reversing deposit batch in the DB other than the original deposit batch", 1, depBats.Count);

			DepositBatch revDepBat = (DepositBatch)depBats[0];
			AssertEquals("Reversing DepositBatch should have local amount = -40", -40M, revDepBat.AH_InvoiceAmount);
			AssertEquals("Reversing DepositBatch should have os amount = -40", -40M, revDepBat.AH_OSTotal);
		}

		protected override Type GetTestingClassType()
		{
			return typeof(DirectReceiptReversing);
		}

		protected override void SetupReversingIReversingInstance()
		{
			TestReversingIReversingInstance = new TestIDirectReceiptTransaction();
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = new TestIDirectReceiptTransaction();
		}
	}
}
