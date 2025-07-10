using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class DirectPaymentReversingTest : CashBookReversingTest
	{
		public void TestReversedPaymentGenerateDDRBatch_ForDDL()
		{
			TestObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = false;

			var testPayment = Factory.NewWithValidTestData<DirectPayment>();
			testPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			testPayment.AH_OSExTaxAmount = 120m;
			testPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			testPayment.Lines.AddNew(testPayment.DependentTransactionLineType);
			testPayment.Lines[0].AL_OSExTaxAmount = 120m;
			Factory.Save();

			var dDRBatch = Factory.NewWithValidTestData((typeof(DirectDebitBatchHeader))) as DirectDebitBatchHeader;
			dDRBatch.AH_TransactionNum = ZString.Empty;
			dDRBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			Assert(testPayment.IncludeInTheBatch);

			Factory.Save();

			AssertEquals(ReceiptTypes.DirectDebitLine, testPayment.AH_ReceiptType);
			AssertEquals(-120m, testPayment.AH_OSTotal);
			AssertEquals(120m, dDRBatch.AH_OSTotal);

			TestObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = true;

			var payReversing = new DirectPaymentReversing(testPayment);
			payReversing.Reverse();
			Factory.Save();

			AssertReversedTransactionValues(testPayment, dDRBatch, ReceiptTypes.DirectDebitLine, ReceiptTypes.DirectDebit);
		}

		public void TestReversedPaymentGenerateDDRBatch_ForDDR()
		{
			TestObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = true;

			var testPayment = Factory.NewWithValidTestData<DirectPayment>();
			testPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			testPayment.AH_OSExTaxAmount = 120m;
			testPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			testPayment.Lines.AddNew(testPayment.DependentTransactionLineType);
			testPayment.Lines[0].AL_OSExTaxAmount = 120m;
			Factory.Save();

			var dDRBatch = Factory.NewWithValidTestData((typeof(DirectDebitBatchHeader))) as DirectDebitBatchHeader;
			dDRBatch.AH_TransactionNum = ZString.Empty;
			dDRBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			Assert(testPayment.IncludeInTheBatch);

			Factory.Save();

			AssertEquals(ReceiptTypes.DirectDebit, testPayment.AH_ReceiptType);
			AssertEquals(-120m, testPayment.AH_OSTotal);
			AssertEquals(120m, dDRBatch.AH_OSTotal);

			TestObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = false;

			var payReversing = new DirectPaymentReversing(testPayment);
			payReversing.Reverse();
			Factory.Save();

			AssertReversedTransactionValues(testPayment, dDRBatch, ReceiptTypes.DirectDebit, ReceiptTypes.NonRolledUpBatch);
		}

		void AssertReversedTransactionValues(DirectPayment testPayment, DirectDebitBatchHeader dDRBatch, string originalReceiptType, string rolledUpBatchType)
		{
			// Check the Original Pay
			testPayment.Reload();
			Assert(testPayment.AH_IsCancelled);
			AssertEquals(originalReceiptType, testPayment.AH_ReceiptType);
			AssertEquals(dDRBatch.AH_TransactionNum, testPayment.AH_ReceiptBatchNo);

			// Check the Original DDR Batch
			dDRBatch.Reload();
			Assert(!dDRBatch.AH_IsCancelled);
			AssertEquals(120.0m, dDRBatch.AH_OSTotal);

			// Check the Reversed DDR Batch
			ZQuery newReversedDDRFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DDRBatch);
			newReversedDDRFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, dDRBatch.PK);
			newReversedDDRFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, dDRBatch.AH_GC);

			DirectDebitBatchHeader[] createdDDR = Factory.Load(typeof(DirectDebitBatchHeader), newReversedDDRFilter) as DirectDebitBatchHeader[];
			AssertEquals(1, createdDDR.Length);
			Assert(createdDDR[0].PK != dDRBatch.PK);
			AssertEquals(TransactionTypes.DDRBatch, createdDDR[0].AH_TransactionType);
			AssertEquals(-dDRBatch.AH_OSTotal, createdDDR[0].AH_OSTotal);
			AssertEquals(rolledUpBatchType, createdDDR[0].AH_ReceiptType);

			// Check the Reversed Pay
			ZQuery newReversedPayFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DirectPayment);
			newReversedPayFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, testPayment.PK);
			newReversedPayFilter.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptBatchNo, createdDDR[0].AH_TransactionNum);
			newReversedPayFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, createdDDR[0].AH_GC);

			DirectPayment[] createdPayment = Factory.Load<DirectPayment>(newReversedPayFilter);
			AssertEquals(1, createdPayment.Length);
			Assert(createdPayment[0].PK != testPayment.PK);
			AssertEquals(TransactionTypes.DirectPayment, createdPayment[0].AH_TransactionType);
			AssertEquals(createdDDR[0].AH_TransactionNum, createdPayment[0].AH_ReceiptBatchNo);
			AssertEquals(testPayment.AH_ReceiptType, createdPayment[0].AH_ReceiptType);
			AssertEquals(-testPayment.AH_OSTotal, createdPayment[0].AH_OSTotal);
		}

		public void TestShowDeleteForm()
		{
			BankTransfer testBankTransfer = new BankTransfer(Factory, null);
			testBankTransfer.BankTransferFromPK = TestObjectCreator.AUDBankAccount.PK;
			testBankTransfer.BankTransferToPK = TestObjectCreator.USDBankAccount.PK;

			testBankTransfer.BuyExchangeRate = 0.5m;
			testBankTransfer.SellAmount = 100m;
			testBankTransfer.EnableFinanceCharge = true;
			testBankTransfer.FinanceChargeOSAmount = 10m;
			Factory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionCount, (byte)3);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			DirectPayment testBankTranferDirectPayment = Factory.LoadTop1<DirectPayment>(filter);

			Reversing = ReversingFactory.NewReversing(testBankTranferDirectPayment);
			AssertEquals("CanReverseTransaction when transaction is in Invoice Batch", false, DirectPaymentReversing.CanReverseTransaction);
			AssertEquals("GenerateCantReverseErrorMessage when transaction is in Invoice Batch", DirectPaymentReversing.CreatedByTRFMessage_ForTestOnly, DirectPaymentReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		protected override Type GetTestingClassType()
		{
			return typeof(DirectPaymentReversing);
		}

		protected override void SetupReversingIReversingInstance()
		{
			TestReversingIReversingInstance = new TestIDirectPaymentTransaction();
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = new TestIDirectPaymentTransaction();
		}

		DirectPaymentReversing DirectPaymentReversing
		{
			get { return (DirectPaymentReversing)Reversing; }
		}
	}
}
