using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(DirectDebitFileController))]
	class DirectDebitFileControllerTest : AccountingTransactionControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DirectDebitFile;
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.DDRFile; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.GenerateDirectDebitFile; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.CancelDirectDebitFile; }
		}

		public void TestShowDeleteFormIfClearedInCashbook()
		{
			DirectDebitBatchHeader testBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testBatch.AH_DateClearedInCashbook = new ZDateTime(2006, 1, 15);
			testBatch.AH_ReceiptType = ReceiptTypes.DirectDebit;
			Factory.Save();

			DirectDebitFileController testController = new DirectDebitFileController();
			using (IZForm testForm = testController.ShowDeleteForm(testBatch))
			{
				AssertNull(testForm);
				AssertEquals("This transaction cannot be reversed because it has been cleared in Cashbook. Please unclear this transaction from the cashbook before reversing.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCantDeleteWhenBatchContainsCancelledPayments()
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

			DirectDebitFileController testController = new DirectDebitFileController();
			using (IZForm testForm = testController.ShowDeleteForm(testBatch))
			{
				AssertNull(testForm);
				AssertEquals("You cannot cancel this DDR batch because one or more payments in the batch are canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowDeleteFormIfClearedInCashbook_IndividualPaymentCancelled()
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

			DirectDebitFileController testController = new DirectDebitFileController();
			using (IZForm testForm = testController.ShowDeleteForm(testBatch))
			{
				AssertNull(testForm);
				AssertEquals("You cannot cancel DDR Batch because some of the Payments / Direct Payments included in the batch is already cleared in cashbook.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowDeleteFormIfNotClearedInCashbook_IndividualPaymentCancelled()
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

			DirectDebitFileController testController = new DirectDebitFileController();
			using (IZForm testForm = testController.ShowDeleteForm(testBatch))
			{
				AssertNotNull(testForm);
			}
		}

		public void TestShowDeleteFormIfNotClearedInCashbook()
		{
			DirectDebitBatchHeader testBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testBatch.AH_DateClearedInCashbook = ZDateTime.Empty;
			Factory.Save();

			DirectDebitFileController testController = new DirectDebitFileController();
			using (IZForm testForm = testController.ShowDeleteForm(testBatch))
			{
				AssertNotNull(testForm);
			}
		}

		public void TestShowViewFormIfNotCancelled()
		{
			DirectDebitBatchHeader testBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testBatch.AH_IsCancelled = ZBool.False;
			Factory.Save();

			DirectDebitFileController testController = new DirectDebitFileController();
			using (IZForm testForm = testController.ShowViewForm(testBatch))
			{
				AssertNotNull(testForm);
			}
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return TestTransaction; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestTransaction = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			Factory.Save();
		}

		DirectDebitBatchHeader TestTransaction;
	}
}
