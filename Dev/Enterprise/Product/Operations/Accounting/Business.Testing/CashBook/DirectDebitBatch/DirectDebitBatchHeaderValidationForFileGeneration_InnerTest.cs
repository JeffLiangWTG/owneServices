using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	class DirectDebitBatchHeaderValidationForFileGeneration_InnerTest : DirectDebitBatchHeaderValidation_InnerTest
	{
		public void TestCheckAH_AB_ValidateBankAccountNumber()
		{
			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.CBA;
			testBank.AB_AccountNum = "12345678901";

			DirectDebitBatchHeader dDRBatch = Factory.New<DirectDebitBatchHeader>();
			dDRBatch.AH_AB = testBank.PK;

			dDRBatch.ValidateBeforeFileGeneration();
			AssertHasError(dDRBatch.AH_ABInfo, "The Bank Account Number setup must be nine characters or less");

			testBank.AB_AccountNum = "123456789";
			dDRBatch.AH_AB = testBank.PK;
			AssertNoError(dDRBatch.AH_ABInfo, "The Bank Account Number setup must be nine characters or less");
		}

		public void TestCheckAH_AB_ValidateBankAccountNumber_ForASB()
		{
			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.ASB;
			testBank.AB_AccountNum = "123456";

			DirectDebitBatchHeader dDRBatch = Factory.New<DirectDebitBatchHeader>();
			dDRBatch.AH_AB = testBank.PK;

			dDRBatch.ValidateBeforeFileGeneration();
			AssertHasError(dDRBatch.AH_ABInfo, "The Bank Account Number setup must be nine characters in length");

			testBank.AB_AccountNum = "123456789";
			dDRBatch.AH_AB = testBank.PK;
			dDRBatch.ValidateBeforeFileGeneration();
			AssertNoError(dDRBatch.AH_ABInfo, "The Bank Account Number setup must be nine characters in length");

			testBank.AB_AccountNum = "1234567891";
			dDRBatch.AH_AB = testBank.PK;
			dDRBatch.ValidateBeforeFileGeneration();
			AssertHasError(dDRBatch.AH_ABInfo, "The Bank Account Number setup must be nine characters in length");
		}

		public void TestCheckAH_AB_ValidateBankAccountNumber_ForWNZ()
		{
			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.WNZ;
			testBank.AB_AccountNum = "1234567890123";

			DirectDebitBatchHeader dDRBatch = Factory.New<DirectDebitBatchHeader>();
			dDRBatch.AH_AB = testBank.PK;
			dDRBatch.ValidateBeforeFileGeneration();

			AssertHasError(dDRBatch.AH_ABInfo, "The Bank Account Number setup must be less than or equal to 12 characters in length");

			testBank.AB_AccountNum = "123456789";
			dDRBatch.AH_AB = testBank.PK;
			AssertNoError(dDRBatch.AH_ABInfo, "The Bank Account Number setup must be less than or equal to 12 characters in length");
		}

		public void TestCheckAH_AB_ValidateBankAccountEFTUserID()
		{
			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.WBC;
			testBank.AB_AccountEFTUserID = "1234567890";

			DirectDebitBatchHeader dDRBatch = Factory.New<DirectDebitBatchHeader>();
			dDRBatch.AH_AB = testBank.PK;

			dDRBatch.ValidateBeforeFileGeneration();
			AssertHasError(dDRBatch.AH_ABInfo, "The Bank Account EFT User ID must be less than or equal to 6 characters in length");

			testBank.AB_AccountEFTUserID = "123456";
			dDRBatch.AH_AB = testBank.PK;
			AssertNoError(dDRBatch.AH_ABInfo, "The Bank Account EFT User ID must be less than or equal to 6 characters in length");
		}
	}
}
