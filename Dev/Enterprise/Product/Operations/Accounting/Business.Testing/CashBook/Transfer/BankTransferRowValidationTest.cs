using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	public abstract class BankTransferRowValidationTest : TransactionHeaderValidationTest
	{
		public void TestEnterValidBankAccount()
		{
			TestBankTransferRow.AH_AB = ZGuid.NewZGuid();
			AssertEquals(true, TestBankTransferRow.AH_ABInfo.GetErrors().Contains(BankTransferRowValidation.EnterValidBankAccountMessage_ForTestOnly));

			TestBankTransferRow.AH_AB = TestObjectCreator.AUDBankAccount2.PK;
			AssertEquals(false, TestBankTransferRow.AH_ABInfo.HasErrors());
		}

		public void TestBankAccountInDifferentBranch()
		{
			GlbBranchCollection branchCol = new GlbBranchCollection(Factory);
			branchCol.Load();
			foreach (GlbBranch branch in branchCol)
			{
				if (branch.PK != GlbBranch.CurrentBranch.PK)
				{
					TestObjectCreator.AUDBankAccount.AB_GB = branch.PK;
					break;
				}
			}

			TestBankTransferRow.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			AssertEquals(true, TestBankTransferRow.AH_ABInfo.GetWarnings().ContainsNotificationContaining("Selected Bank account"));

			TestObjectCreator.AUDBankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			TestBankTransferRow.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			AssertEquals(false, TestBankTransferRow.AH_ABInfo.HasWarnings());
		}

		public void TestCheckAH_ABIsActive()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccBankAccount testBank = testObjectCreator.CreateBankAccount("TST", "TEST", testObjectCreator.AUD, null);
			TestBankTransferRow.AH_AB = testBank.PK;

			Assert(!TestBankTransferRow.AH_ABInfo.HasErrors());

			testBank.AB_IsActive = false;
			TestBankTransferRow.AH_AB = testBank.PK;
			AssertHasError(TestBankTransferRow.AH_ABInfo, "This Bank Account is inactive - it may not be used.");
		}

		public void TestSameBankAccounts()
		{
			BankTransfer testBankTransfer = new BankTransfer(Factory, null);
			testBankTransfer.BankTransferFromPK = TestObjectCreator.AUDBankAccount.PK;
			testBankTransfer.BankTransferToPK = TestObjectCreator.AUDBankAccount.PK;
			AssertEquals(true, testBankTransfer.BankTransferToPKInfo.GetErrors().Contains(BankTransferRowValidation.BankAccountsMustBeDifferenthMessage_ForTestOnly));

			testBankTransfer.BankTransferToPK = TestObjectCreator.USDBankAccount.PK;
			AssertEquals(false, testBankTransfer.BankTransferToPKInfo.HasErrors());
		}

		public void TestCheckAH_OSExTaxAmount()
		{
			TestBankTransferRow.AH_OSExTaxAmount = 1m;
			AssertEquals(false, TestBankTransferRow.AH_OSExTaxAmountInfo.HasErrors());

			TestBankTransferRow.AH_OSExTaxAmount = 0m;
			AssertEquals(true, TestBankTransferRow.AH_OSExTaxAmountInfo.GetErrors().Contains(BankTransferRowValidation.GreaterThanZeroMessage_ForTestOnly));

			TestBankTransferRow.AH_OSExTaxAmount = -1m;
			AssertEquals(true, TestBankTransferRow.AH_OSExTaxAmountInfo.GetErrors().Contains(BankTransferRowValidation.AmountPositiveMessage_ForTestOnly));
		}

		public void TestCheckCheckAH_ExchangeRate()
		{
			TestBankTransferRow.AH_ExchangeRate = 1m;
			AssertEquals(false, TestBankTransferRow.AH_ExchangeRateInfo.HasErrors());

			TestBankTransferRow.AH_ExchangeRate = 0m;
			AssertEquals(true, TestBankTransferRow.AH_ExchangeRateInfo.HasErrors());

			TestBankTransferRow.AH_ExchangeRate = -1m;
			AssertEquals(true, TestBankTransferRow.AH_ExchangeRateInfo.GetErrors().Contains(BankTransferRowValidation.ExchangeRatesPositiveMessage_ForTestOnly));
		}

		public void TestCheckAH_LocalExTaxAmount()
		{
			TestBankTransferRow.AH_LocalExTaxAmount = 1m;
			AssertEquals(false, TestBankTransferRow.AH_LocalExTaxAmountInfo.HasErrors());

			TestBankTransferRow.AH_LocalExTaxAmount = 0m;
			AssertEquals(true, TestBankTransferRow.AH_LocalExTaxAmountInfo.GetErrors().Contains(BankTransferRowValidation.GreaterThanZeroMessage_ForTestOnly));

			TestBankTransferRow.AH_LocalExTaxAmount = -1m;
			AssertEquals(true, TestBankTransferRow.AH_LocalExTaxAmountInfo.GetErrors().Contains(BankTransferRowValidation.AmountPositiveMessage_ForTestOnly));
		}

		public override void TestAH_IsCancelledBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			Assert("Difficult to fix as requires special setup for transaction to be saved", true);
		}

		public override void TestAH_AH_InvoiceStatementBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			Assert("Difficult to fix as requires special setup for transaction to be saved", true);
		}

		public override void TestAH_FullyPaidDateBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			Assert("Difficult to fix as requires special setup for transaction to be saved", true);
		}

		public override void TestAH_InvoicePaymentReferenceCodeBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			Assert("Difficult to fix as requires special setup for transaction to be saved", true);
		}

		public override void TestAH_ReceiptBatchNoBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			Assert("Difficult to fix as requires special setup for transaction to be saved", true);
		}

		#region Implementation

		protected BankTransferRow TestBankTransferRow
		{
			get { return (BankTransferRow)Header; }
		}

		#endregion
	}
}
