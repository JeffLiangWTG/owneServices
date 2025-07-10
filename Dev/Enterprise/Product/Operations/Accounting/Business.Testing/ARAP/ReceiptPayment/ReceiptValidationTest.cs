using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public abstract class ReceiptValidationTest : ReceiptPaymentBaseValidationTest
	{
		public void TestCheckAH_ABIsActive()
		{
			AccBankAccount testBank = TestObjectCreator.CreateBankAccount("TST", "TEST", TestObjectCreator.AUD, null);

			testBank.AB_IsActive = true;
			TestReceipt.AH_AB = testBank.PK;
			Assert(!TestReceipt.AH_ABInfo.HasErrors());

			testBank.AB_IsActive = false;
			TestReceipt.AH_AB = testBank.PK;
			AssertHasError(TestReceipt.AH_ABInfo, "This Bank Account is inactive - it may not be used.");
		}

		public void TestCheckAH_ReceiptTypeIsCashAccount()
		{
			var testBank = TestObjectCreator.CreateBankAccount("TST-BANK", "TEST BANK", TestObjectCreator.AUD, null, AccountTypeCodeDescriptionPairList.Codes.BNK);
			testBank.AB_IsActive = true;
			var testCash = TestObjectCreator.CreateBankAccount("TST_CASH", "TEST CASH", TestObjectCreator.AUD, null, AccountTypeCodeDescriptionPairList.Codes.CSH);
			testCash.AB_IsActive = true;

			AssertNoErrors(TestReceipt.AH_ReceiptTypeInfo);

			TestReceipt.AH_AB = testBank.PK;
			AssertNoErrors(TestReceipt.AH_ReceiptTypeInfo);

			TestReceipt.AH_AB = testCash.PK;
			AssertNoErrors(TestReceipt.AH_ReceiptTypeInfo);

			TestReceipt.AH_ReceiptType = ReceiptTypes.Cheque;
			AssertHasError("Should have error", TestReceipt.AH_ReceiptTypeInfo, $"For Cash Account, please select CSH - Cash {TestReceipt.AH_ReceiptTypeInfo.HumanReadableName}.");

			TestReceipt.AH_ReceiptType = ReceiptTypes.Cash;
			AssertNoErrors(TestReceipt.AH_ReceiptTypeInfo);
		}

		public void TestChequeValidationIfTypeNotCheque()
		{
			TestReceipt.AH_ReceiptType = "CSH";

			TestReceipt.Validation.ValidateAH_ChequeDrawer();
			TestReceipt.Validation.ValidateAH_DrawerBranch();
			TestReceipt.Validation.ValidateAH_DrawerBank();

			Assert(!TestReceipt.AH_ChequeDrawerInfo.HasErrors());
			Assert(!TestReceipt.AH_DrawerBranchInfo.HasErrors());
			Assert(!TestReceipt.AH_DrawerBankInfo.HasErrors());
		}

		public void TestCheckAH_OSExTaxAmount()
		{
			TestReceipt.AH_OSExTaxAmount = 1m;
			AssertEquals(false, TestReceipt.AH_OSExTaxAmountInfo.HasErrors());

			TestReceipt.AH_OSExTaxAmount = 0m;
			AssertEquals(true, TestReceipt.AH_OSExTaxAmountInfo.GetErrors().Contains("Overseas amount must be greater than zero"));

			TestReceipt.AH_OSExTaxAmount = -1m;
			AssertEquals(true, TestReceipt.AH_OSExTaxAmountInfo.GetErrors().Contains("Overseas amount must be greater than zero"));

			TestReceipt.IsPostWithMatching = true;
			TestReceipt.AH_OSExTaxAmount = 1m;
			AssertEquals(false, TestReceipt.AH_OSExTaxAmountInfo.HasErrors());

			TestReceipt.AH_OSExTaxAmount = 0m;
			AssertEquals(false, TestReceipt.AH_OSExTaxAmountInfo.HasErrors());

			TestReceipt.AH_OSExTaxAmount = -1m;
			AssertEquals(true, TestReceipt.AH_OSExTaxAmountInfo.GetErrors().Contains("Overseas amount must be greater than or equal to zero"));
		}

		public void TextCheckAH_LocalExTaxAmount()
		{
			TestReceipt.AH_LocalExTaxAmount = 0;
			Assert("AH_LocalExTaxAmount should have errors", TestReceipt.AH_LocalExTaxAmountInfo.HasErrors());
			TestReceipt.AH_LocalExTaxAmount = 1;
			Assert("AH_LocalExTaxAmount should have no errors", !TestReceipt.AH_LocalExTaxAmountInfo.HasErrors());
			TestReceipt.AH_LocalExTaxAmount = 0;
			Assert("AH_LocalExTaxAmount should have errors", TestReceipt.AH_LocalExTaxAmountInfo.HasErrors());
			TestReceipt.IsPostWithMatching = true;
			Assert("AH_LocalExTaxAmount should have no errors", !TestReceipt.AH_LocalExTaxAmountInfo.HasErrors());
		}

		public void TestChequeValidationIfTypeCheque()
		{
			TestReceipt.AH_ReceiptType = "CHQ";

			TestReceipt.Validation.ValidateAH_ChequeDrawer();
			TestReceipt.Validation.ValidateAH_DrawerBranch();
			TestReceipt.Validation.ValidateAH_DrawerBank();

			Assert(TestReceipt.AH_ChequeDrawerInfo.HasErrors());
			Assert(TestReceipt.AH_DrawerBranchInfo.HasErrors());
			Assert(TestReceipt.AH_DrawerBankInfo.HasErrors());
		}

		public void TestChequeValidationForReceiptType()
		{
			Assert("Precondition: Receipt Type should have no errors", !TestReceipt.AH_ReceiptTypeInfo.HasErrors());
			TestReceipt.AH_ReceiptType = ZString.Empty;
			Assert("Receipt Type cannot be empty", TestReceipt.AH_ReceiptTypeInfo.HasErrors());
			TestReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("Receipt Type should have no errors", !TestReceipt.AH_ReceiptTypeInfo.HasErrors());
			TestReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			Assert("Receipt Type cannot be invalid", TestReceipt.AH_ReceiptTypeInfo.HasErrors());
		}

		public void TestCheckAH_ReceiptTypeSecurity_ARReceipt()
		{
			if (TestReceipt.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				Env.Security.NewReceivablesReceiptCheque.IsAllowed = true;
				TestReceipt.AH_ReceiptType = ReceiptTypes.Cheque;
				Assert("Receipt Type should have no errors", !TestReceipt.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewReceivablesReceiptCheque.IsAllowed = false;
				TestReceipt.AH_ReceiptType = ReceiptTypes.Cheque;
				AssertHasError(TestReceipt.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this receipt type.");

				Env.Security.NewReceivablesReceiptCash.IsAllowed = true;
				TestReceipt.AH_ReceiptType = ReceiptTypes.Cash;
				Assert("Receipt Type should have no errors", !TestReceipt.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewReceivablesReceiptCash.IsAllowed = false;
				TestReceipt.AH_ReceiptType = ReceiptTypes.Cash;
				AssertHasError(TestReceipt.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this receipt type.");

				Env.Security.NewReceivablesReceiptCreditCard.IsAllowed = true;
				TestReceipt.AH_ReceiptType = ReceiptTypes.CreditCard;
				Assert("Receipt Type should have no errors", !TestReceipt.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewReceivablesReceiptCreditCard.IsAllowed = false;
				TestReceipt.AH_ReceiptType = ReceiptTypes.CreditCard;
				AssertHasError(TestReceipt.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this receipt type.");

				Env.Security.NewReceivablesReceiptDirectCredit.IsAllowed = true;
				TestReceipt.AH_ReceiptType = ReceiptTypes.DirectCredit;
				Assert("Receipt Type should have no errors", !TestReceipt.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewReceivablesReceiptDirectCredit.IsAllowed = false;
				TestReceipt.AH_ReceiptType = ReceiptTypes.DirectCredit;
				AssertHasError(TestReceipt.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this receipt type.");
			}
			else
			{
				Assert("The test is not applicable for AP receipts", true);
			}
		}

		public void TestCheckAH_ReceiptTypeSecurity_APReceipt()
		{
			if (TestReceipt.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				Env.Security.NewPayablesReceiptCheque.IsAllowed = true;
				TestReceipt.AH_ReceiptType = ReceiptTypes.Cheque;
				Assert("Receipt Type should have no errors", !TestReceipt.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewPayablesReceiptCheque.IsAllowed = false;
				TestReceipt.AH_ReceiptType = ReceiptTypes.Cheque;
				AssertHasError(TestReceipt.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this receipt type.");

				Env.Security.NewPayablesReceiptCash.IsAllowed = true;
				TestReceipt.AH_ReceiptType = ReceiptTypes.Cash;
				Assert("Receipt Type should have no errors", !TestReceipt.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewPayablesReceiptCash.IsAllowed = false;
				TestReceipt.AH_ReceiptType = ReceiptTypes.Cash;
				AssertHasError(TestReceipt.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this receipt type.");

				Env.Security.NewPayablesReceiptCreditCard.IsAllowed = true;
				TestReceipt.AH_ReceiptType = ReceiptTypes.CreditCard;
				Assert("Receipt Type should have no errors", !TestReceipt.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewPayablesReceiptCreditCard.IsAllowed = false;
				TestReceipt.AH_ReceiptType = ReceiptTypes.CreditCard;
				AssertHasError(TestReceipt.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this receipt type.");

				Env.Security.NewPayablesReceiptDirectCredit.IsAllowed = true;
				TestReceipt.AH_ReceiptType = ReceiptTypes.DirectCredit;
				Assert("Receipt Type should have no errors", !TestReceipt.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewPayablesReceiptDirectCredit.IsAllowed = false;
				TestReceipt.AH_ReceiptType = ReceiptTypes.DirectCredit;
				AssertHasError(TestReceipt.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this receipt type.");
			}
			else
			{
				Assert("The test is not applicable for AR receipts", true);
			}
		}

		protected Receipt TestReceipt
		{
			get { return (Receipt)TestReceiptPayment; }
		}
	}
}
