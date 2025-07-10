using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class DepositBatchReversingTest : ReversingBaseTest
	{
		public override void TestErrorMessages()
		{
			ZString expectedErrorMessage = "This transaction has already been canceled.";
			AssertEquals("The Error Message should be generated for reversing.", expectedErrorMessage, DepositBatchReversing.AlreadyReversedErrorMessage_ForTestOnly);
			expectedErrorMessage = "This transaction cannot be reversed because it has been matched with other transactions.";
			AssertEquals("The Error Message should be generated for reversing.", expectedErrorMessage, DepositBatchReversing.MatchedAndCantReverseErrorMessage_ForTestOnly);
			expectedErrorMessage = "This transaction cannot be reversed because it has been cleared in Cashbook. Please unclear this transaction from the cashbook before reversing.";
			AssertEquals("The Error Message should be generated for reversing.", expectedErrorMessage, DepositBatchReversing.ClearedInCashBookErrorMessage_ForTestOnly);
		}

		public new void TestInactiveOrgDoesNotAllowReversing()
		{
			Assert("There is no Organisation ralated DepositBatch.", true);
		}

		public new void TestCantReverseAlreadyReversedTransaction()
		{
			TestIReversingInstance = new DepositBatchParentForReversingTest(Factory, Factory.NewWithValidTestData<DepositBatch>());
			TestIReversingInstance.SetIsReversed(true);
			SetupReversingObject();
			Assert("Should never allow reverse on an already reversed transaction", !DepositBatchReversing.CanReverseTransaction);
			AssertEquals("Can't reverse error", DepositBatchReversing.AlreadyReversedErrorMessage_ForTestOnly, DepositBatchReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestCantReverseDirectCreditReceipt()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.DirectCredit, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, testReceipt.AH_ReceiptBatchNo);

			DepositBatch testBatch = Factory.LoadTop1<DepositBatch>(filter);
			DepositBatchParent batchParent = new DepositBatchParent(Factory, testBatch);
			Reversing = ReversingFactory.NewReversing(batchParent);

			Assert("Transaction must not be reversed.", !DepositBatchReversing.CanReverseTransaction);
			AssertEquals("Transaction must not be reversed with correct message.", DepositBatchReversing.DirectCreditReceiptMessage_ForTestOnly,
				DepositBatchReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestCantReverseHasCancelledReceipt()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.DirectCredit, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			testReceipt.AH_IsCancelled = true;
			((IMatching)testReceipt).CurrentMatchGroup.AddNew().AP_AH = testReceipt.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(testReceipt);
			Factory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, testReceipt.AH_ReceiptBatchNo);

			DepositBatch testBatch = Factory.LoadTop1<DepositBatch>(filter);
			DepositBatchParent batchParent = new DepositBatchParent(Factory, testBatch);
			Reversing = ReversingFactory.NewReversing(batchParent);

			Assert("Transaction must not be reversed.", !DepositBatchReversing.CanReverseTransaction);
			AssertEquals("Transaction must not be reversed with correct message.", DepositBatchReversing.ContainCancelledReceiptMessage_ForTestOnly,
				DepositBatchReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		[TestDate(1999, 7, 2)]
		public void TestCantReverseSubLedgerClosed_And_ClearedInCashbook()
		{
			PeriodManager testManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(1999, 6, 1));
			Factory.Save();
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			ZGuid firstBatchPK = testDepositBatchParent.DepositBatchLines[0].PK;
			DepositBatch testDepositBatch = Factory.Load<DepositBatch>(firstBatchPK);
			testDepositBatch.AH_PostDate = new ZDateTime(1999, 6, 1);
			Factory.Save();

			DepositBatchParent batchParent = new DepositBatchParent(Factory, testDepositBatch);
			Reversing = ReversingFactory.NewReversing(batchParent);

			Assert("Transaction must be reversed.", DepositBatchReversing.CanReverseTransaction);
			AssertEquals("Transaction must be reversed withou message.", ZString.Empty, DepositBatchReversing.GenerateCantReverseErrorMessage_ForTestOnly());

			testManager.CloseSubLedgerPeriod();
			Assert("Transaction must not be reversed.", !DepositBatchReversing.CanReverseTransaction);
			AssertEquals("Transaction must not be reversed with correct message.", DepositBatchReversing.PeriodSubLedgerClosedMessage_ForTestOnly,
				DepositBatchReversing.GenerateCantReverseErrorMessage_ForTestOnly());

			testDepositBatch.AH_PostDate = new ZDateTime(1999, 7, 1);
			testDepositBatch.AH_DateClearedInCashbook = new ZDateTime(2006, 2, 15);
			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(Factory))
			{
				Factory.Save();
			}

			Assert("Transaction must not be reversed.", !DepositBatchReversing.CanReverseTransaction);
			AssertEquals("Transaction must not be reversed with correct message.", DepositBatchReversing.ClearedInCashBookMessage_ForTestOnly,
				DepositBatchReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestCanReverse_ClearedReceiptLinkedToNonClearedDepositBatch()
		{
			var testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			var testDepositBatchParent = new DepositBatchParent(Factory);
			var firstBatchPK = testDepositBatchParent.DepositBatchLines[0].PK;
			var testDepositBatch = Factory.Load<DepositBatch>(firstBatchPK);
			Factory.Save();

			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfoSafe(testReceipt.PK, CriticalValidationInfoCollectorServiceKeyType.ClearedReceiptLinkedToNonClearedDepositBatch);

			var expectedReceiptBatchNumber = testReceipt.AH_ReceiptBatchNo;
			Assert("Precondition: expectedReceiptBatchNumber", !expectedReceiptBatchNumber.IsEmpty);

			var testReceiptClearedDate = new ZDateTime(2023, 8, 21);
			testReceipt.AH_DateClearedInCashbook = testReceiptClearedDate;

			var batchParent = new DepositBatchParent(Factory, testDepositBatch);
			TestObjectCreator.ReverseTransaction(batchParent, out _);

			var errorInfo = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfoSafe(testReceipt.PK, CriticalValidationInfoCollectorServiceKeyType.ClearedReceiptLinkedToNonClearedDepositBatch);

			AssertContains($"BatchReceiptNumber: {expectedReceiptBatchNumber}, ReceiptClearedDate: 21-Aug-23 00:00:00, DepositBatchClearedDate: ", errorInfo);
		}

		public void TestCanReverse_ClearedReceiptLinkedToNonClearedDepositBatch_WhenExistingBatchIsNull()
		{
			var testDepositBatchParent = new DepositBatchParent(Factory);
			TestObjectCreator.ReverseTransaction(testDepositBatchParent, out _);

			var expectedMessage = "ExistingBatch can't be null during reversing.";

			AssertContains(expectedMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#region Implementation

		protected override Type GetTestingClassType()
		{
			return typeof(DepositBatchReversing);
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = new DepositBatchParentForReversingTest(Factory);
		}

		protected override void SetupReversingIReversingInstance()
		{
			TestReversingIReversingInstance = Factory.NewWithValidTestData<DepositBatchForReversingTest>();
		}

		DepositBatchReversing DepositBatchReversing
		{
			get { return (DepositBatchReversing)Reversing; }
		}

		class DepositBatchParentForReversingTest : DepositBatchParent, IReversingForTests
		{
			public DepositBatchParentForReversingTest(BusinessObjectFactory factory, DepositBatch existingDepositBatchToBeLoaded)
				: base(factory, existingDepositBatchToBeLoaded)
			{
			}

			public DepositBatchParentForReversingTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			#region IReversingForTests Members

			public void SetIsClearedInCashbook(bool value)
			{
				TransactionHeader transaction = ReverseTransaction as TransactionHeader;
				if (transaction != null)
				{
					transaction.AH_DateClearedInCashbook = value ? ZDateTime.Now : ZDateTime.Empty;
				}
			}

			public void SetIsReversing(bool value)
			{
				fIsReversing = value;
			}

			public void SetIsReversed(bool value)
			{
				TransactionHeader transaction = ReverseTransaction as TransactionHeader;
				if (transaction != null)
				{
					transaction.AH_IsCancelled = value;
				}
			}

			public void SetReverseTransactionToBeGenerated(IReversing reverseTransaction)
			{
				fExistingDepositBatchToBeLoaded = reverseTransaction as DepositBatch;
			}

			#endregion

			#region ITransactionForTests Members

			public void SetFactory(BusinessObjectFactory factory)
			{
			}

			public ZDateTime FullyPaidDate
			{
				get
				{
					TransactionHeader transaction = ReverseTransaction as TransactionHeader;
					return transaction != null ? transaction.AH_FullyPaidDate : ZDateTime.Empty;
				}
				set
				{
					TransactionHeader transaction = ReverseTransaction as TransactionHeader;
					if (transaction != null)
					{
						transaction.AH_FullyPaidDate = value;
					}
				}
			}

			#endregion
		}

		class DepositBatchForReversingTest : DepositBatch, IReversingForTests
		{
			public DepositBatchForReversingTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IReversingForTests Members

			public void SetIsClearedInCashbook(bool value)
			{
				AH_DateClearedInCashbook = value ? ZDateTime.Now : ZDateTime.Empty;
			}

			public void SetIsReversed(bool value)
			{
				AH_IsCancelled = value;
			}

			public void SetReverseTransactionToBeGenerated(IReversing reverseTransaction)
			{
				fReverseTransaction = reverseTransaction as DepositBatch;
			}

			#endregion

			#region ITransactionForTests Members

			public void SetFactory(BusinessObjectFactory factory)
			{
			}

			public ZDateTime FullyPaidDate
			{
				get
				{
					return AH_FullyPaidDate;
				}
				set
				{
					AH_FullyPaidDate = value;
				}
			}

			#endregion
		}

		#endregion
	}
}
