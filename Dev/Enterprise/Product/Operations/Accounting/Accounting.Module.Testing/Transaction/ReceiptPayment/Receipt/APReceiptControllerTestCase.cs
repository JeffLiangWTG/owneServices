using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ZAPReceiptController))]
	public class APReceiptControllerTestCase : AccountingTransactionControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ZAPReceipt;
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewPayablesReceipt; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReversePayablesReceipt; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.PayablesTransactions; }
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return TestTransaction; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestTransaction = Factory.NewWithValidTestData<APReceipt>();
			Factory.Save();
		}

		public virtual void TestModuleID()
		{
			AssertEquals(ModuleIDs.APTransaction, Controller.ModuleID);
		}

		APReceipt TestTransaction;

		#region Reversal Checkpoint With Misc Transactions

		[SuspendCriticalValidation]
		public void TestReversalCheckpointIsAllowed_WithOverpaymentTransactionInMatchGroup()
		{
			SetupDateForMatchGroupCheckpoint(TransactionTypes.Overpayment);
			Env.Security.PayablesUnMatchTransactionsOverpaymentType.IsAllowed = true;
			AssertReversalCheckpointWithMiscTransactionInMatchGroup(true, "Manage -> Payables -> Match Transactions -> Unmatch Payables Transactions -> Overpayment Transactions");
		}

		[SuspendCriticalValidation]
		public void TestReversalCheckpointIsDenied_WithOverpaymentTransactionInMatchGroup()
		{
			SetupDateForMatchGroupCheckpoint(TransactionTypes.Overpayment);
			Env.Security.PayablesUnMatchTransactionsOverpaymentType.IsAllowed = false;
			AssertReversalCheckpointWithMiscTransactionInMatchGroup(false, "Manage -> Payables -> Match Transactions -> Unmatch Payables Transactions -> Overpayment Transactions");
		}

		[SuspendCriticalValidation]
		public void TestReversalCheckpointIsDenied_WithMultipleOverpaymentTransactionInMatchGroup()
		{
			SetupDateForMatchGroupCheckpoint(TransactionTypes.Overpayment, miscTransactionCount: 2);
			Env.Security.PayablesUnMatchTransactionsOverpaymentType.IsAllowed = false;
			AssertReversalCheckpointWithMiscTransactionInMatchGroup(false, "Manage -> Payables -> Match Transactions -> Unmatch Payables Transactions -> Overpayment Transactions");
		}

		[SuspendCriticalValidation]
		public void TestReversalCheckpointIsDenied_WithPartialOverpaymentTransactionInMatchGroup()
		{
			SetupDateForMatchGroupCheckpoint(TransactionTypes.Overpayment, miscTransactionCount: 0, includeTransactionInMultipleMatchGroups: true);
			Env.Security.PayablesUnMatchTransactionsOverpaymentType.IsAllowed = false;
			AssertReversalCheckpointWithMiscTransactionInMatchGroup(true, "Manage -> Payables -> Match Transactions -> Unmatch Payables Transactions -> Overpayment Transactions");
		}

		[SuspendCriticalValidation]
		public void TestReversalCheckpoint_WithDiscountTransactionInMatchGroup()
		{
			SetupDateForMatchGroupCheckpoint(TransactionTypes.Discount);
			Env.Security.PayablesUnMatchTransactionsOverpaymentType.IsAllowed = false;
			AssertReversalCheckpointWithMiscTransactionInMatchGroup(true, "Manage -> Payables -> Match Transactions -> Unmatch Payables Transactions -> Overpayment Transactions");
		}

		[SuspendCriticalValidation]
		public void TestReversalCheckpoint_WithJournalTransactionInMatchGroup()
		{
			SetupDateForMatchGroupCheckpoint(TransactionTypes.Journal);
			Env.Security.PayablesUnMatchTransactionsOverpaymentType.IsAllowed = false;
			AssertReversalCheckpointWithMiscTransactionInMatchGroup(true, "Manage -> Payables -> Match Transactions -> Unmatch Payables Transactions -> Overpayment Transactions");
		}

		[SuspendCriticalValidation]
		public void TestReversalCheckpoint_WithExchangeVarianceTransactionInMatchGroup()
		{
			SetupDateForMatchGroupCheckpoint(TransactionTypes.ExchangeDifference);
			Env.Security.PayablesUnMatchTransactionsOverpaymentType.IsAllowed = false;
			AssertReversalCheckpointWithMiscTransactionInMatchGroup(true, "Manage -> Payables -> Match Transactions -> Unmatch Payables Transactions -> Overpayment Transactions");
		}

		void AssertReversalCheckpointWithMiscTransactionInMatchGroup(bool reversalShouldBeAllowed, string expectedCheckpointPath)
		{
			UnitTestUserNotification.Instance.ClearMessages();

			try
			{
				Controller.ShowDeleteForm(ParentTransactionHeaderRow);

				if (reversalShouldBeAllowed)
				{
					AssertNotNull("Should show normal reversing form", Controller.LastShownForm);
					AssertNull("No notifications should be shown", UnitTestUserNotification.Instance.LastMessage.Text);

					var apReceiptController = (ZAPReceiptController)AccountingTransactionController;
					var reversingBizo = apReceiptController.GetLoadedBusinessEntityInLocalFactory_ForTestOnly(ParentTransactionHeaderRow) as IReversing;
					AssertNotNull("Transaction should be IReversing (duh!)", reversingBizo);
					var reversalBizo = reversingBizo.ReverseTransaction;
					AssertNotNull("Should have a reversal transaction", reversalBizo);
					AssertEquals("Transaction should be reversing", true, reversingBizo.IsReversing);
					AssertEquals("Transaction should be reversed", true, reversingBizo.IsReversed);
				}
				else
				{
					AssertNull("Should not show normal reversing form", Controller.LastShownForm);
					AssertEquals("Security notification should be shown when security is denied", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + expectedCheckpointPath, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				if (Controller != null && Controller.LastShownForm != null)
				{
					((System.Windows.Forms.Form)Controller.LastShownForm).Close();
				}
			}
		}

		void SetupDateForMatchGroupCheckpoint(string miscTransactionType, int miscTransactionCount = 1, bool includeTransactionInMultipleMatchGroups = false)
		{
			// Note this scenario is not particularly realistic as AP overpayments are not allowed.
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today);

			var apInvoice = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "AP1234", testObjectCreator.AUD, 1.0m, 200m, 0m, 200m, 0m);
			apInvoice.AH_OH = testObjectCreator.Debtor.PK;
			apInvoice.AH_LocalOutstandingAmount = 0M;

			TestTransaction = testObjectCreator.CreateAPReceipt(0m, 250m, apInvoice.AH_PostDate, apInvoice.AH_PostDate, testObjectCreator.Creditor1.PK, testObjectCreator.AUDBankAccount.PK);
			TestTransaction.AH_InvoiceAmount = TestTransaction.AH_OSTotalAmount;
			TestTransaction.AH_LocalOutstandingAmount = 0M;

			var invoiceMatchGroup = ((IMatching)apInvoice).CurrentMatchGroup.AddNew();
			invoiceMatchGroup.AP_AH = apInvoice.PK;
			invoiceMatchGroup.AP_Amount = -200m;
			invoiceMatchGroup.AP_MatchGroupNum = "M00001234";
			invoiceMatchGroup.AP_MatchDate = apInvoice.AH_PostDate;

			var receiptMatchGroup = ((IMatching)TestTransaction).CurrentMatchGroup.AddNew();
			receiptMatchGroup.AP_AH = TestTransaction.PK;
			receiptMatchGroup.AP_Amount = 250m;
			receiptMatchGroup.AP_MatchGroupNum = "M00001234";
			receiptMatchGroup.AP_MatchDate = apInvoice.AH_PostDate;

			for (int i = 0; i < miscTransactionCount; i++)
			{
				var amount = 50m / miscTransactionCount;
				var miscTransaction = CreateMiscTransactionOfType(amount);
				var miscMatchGroup = ((IMatching)miscTransaction).CurrentMatchGroup.AddNew();
				miscMatchGroup.AP_AH = miscTransaction.PK;
				miscMatchGroup.AP_Amount = -amount;
				miscMatchGroup.AP_MatchGroupNum = "M00001234";
				miscMatchGroup.AP_MatchDate = apInvoice.AH_PostDate;
			}

			if (includeTransactionInMultipleMatchGroups)
			{
				var otherInvoice = (APInvoice)testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "AP4321", testObjectCreator.AUD, 1.0m, 100m, 0m, 100m, 0m);
				otherInvoice.AH_OH = testObjectCreator.Debtor.PK;
				otherInvoice.AH_LocalOutstandingAmount = 0M;

				var otherInvoiceMatchGroup = ((IMatching)otherInvoice).CurrentMatchGroup.AddNew();
				otherInvoiceMatchGroup.AP_AH = otherInvoice.PK;
				otherInvoiceMatchGroup.AP_Amount = -50m;
				otherInvoiceMatchGroup.AP_MatchGroupNum = "M00004321";
				otherInvoiceMatchGroup.AP_MatchDate = otherInvoice.AH_PostDate;

				var otherInvoiceMatchGroup2 = ((IMatching)otherInvoice).CurrentMatchGroup.AddNew();
				otherInvoiceMatchGroup2.AP_AH = otherInvoice.PK;
				otherInvoiceMatchGroup2.AP_Amount = -50m;
				otherInvoiceMatchGroup2.AP_MatchGroupNum = "M00001234";
				otherInvoiceMatchGroup2.AP_MatchDate = otherInvoice.AH_PostDate;

				var otherReceipt = testObjectCreator.CreateAPReceipt(0m, 100m, apInvoice.AH_PostDate, apInvoice.AH_PostDate, testObjectCreator.Creditor1.PK, testObjectCreator.AUDBankAccount.PK);
				otherReceipt.AH_InvoiceAmount = TestTransaction.AH_OSTotalAmount;
				otherReceipt.AH_LocalOutstandingAmount = 0M;

				var otherReceiptMatchGroup = ((IMatching)TestTransaction).CurrentMatchGroup.AddNew();
				otherReceiptMatchGroup.AP_AH = otherReceipt.PK;
				otherReceiptMatchGroup.AP_Amount = 100m;
				otherReceiptMatchGroup.AP_MatchGroupNum = "M00004321";
				otherReceiptMatchGroup.AP_MatchDate = apInvoice.AH_PostDate;

				var otherMiscTransaction = CreateMiscTransactionOfType(50m);
				var otherMiscMatchGroup = ((IMatching)otherMiscTransaction).CurrentMatchGroup.AddNew();
				otherMiscMatchGroup.AP_AH = otherMiscTransaction.PK;
				otherMiscMatchGroup.AP_Amount = -50m;
				otherMiscMatchGroup.AP_MatchGroupNum = "M00004321";
				otherMiscMatchGroup.AP_MatchDate = apInvoice.AH_PostDate;
			}

			TransactionHeader CreateMiscTransactionOfType(decimal amount)
				=> miscTransactionType == TransactionTypes.Overpayment ? testObjectCreator.CreateOverpayment<APOverpayment>(amount, apInvoice.AH_PostDate, testObjectCreator.Debtor.PK)
				 : miscTransactionType == TransactionTypes.Journal ? testObjectCreator.CreateJournal<APJournal>(amount, apInvoice.AH_PostDate, testObjectCreator.Debtor.PK)
				 : miscTransactionType == TransactionTypes.ExchangeDifference ? testObjectCreator.CreateExchangeDifference<APExchangeDifference>(amount, apInvoice.AH_PostDate, testObjectCreator.Debtor.PK)
				 : miscTransactionType == TransactionTypes.Discount ? testObjectCreator.CreateAPDiscount(amount, apInvoice.AH_PostDate, testObjectCreator.Debtor.PK)
				 : throw new ArgumentException($"Transaction Type '{miscTransactionType}' is not misc transaction type");

			Factory.Save();
		}

		#endregion
	}
}
