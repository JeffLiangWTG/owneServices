using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Validation.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Integration.Accounting;

	public sealed class ChequeOrReferenceValidationTest : TestCaseWithFactory
	{
		public void TestCheckIsNumbersLettersAllowed()
		{
			Assert("Error should not be set", string.IsNullOrEmpty(ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(false, "a1234")));
			Assert("Error should be set", !string.IsNullOrEmpty(ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(true, "a1234")));
			Assert("Error should not be set", !string.IsNullOrEmpty(ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(false, "a1234.5")));
			Assert("Error should not be set", !string.IsNullOrEmpty(ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(true, "a1234.5")));
			Assert("Error should not be set", !string.IsNullOrEmpty(ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(false, "1234.5")));
			Assert("Error should not be set", !string.IsNullOrEmpty(ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(true, "1234.5")));
			Assert("Error should be set", !string.IsNullOrEmpty(ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(false, "a12,34")));
			Assert("Error should not be set", string.IsNullOrEmpty(ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(true, "1234")));
			Assert("Error should not be set", !string.IsNullOrEmpty(ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(true, "q1234")));
			Assert("Error should not be set", string.IsNullOrEmpty(ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(false, "1 / - a 234")));
		}

		public void TestGetChequeOrReferenceInUseErrorMessage()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			objectCreator.AUDBankAccount.AB_ChequeNumDigits = 6;
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.CashBook;
			header.AH_TransactionType = ZArchitecture.Core.TransactionTypes.DirectPayment;
			header.AH_AB = objectCreator.AUDBankAccount.PK;
			header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			header.AH_ChequeOrReference = "123456";
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_JobNum = "TEST123";
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AB = objectCreator.AUDBankAccount.PK;
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge.JR_ChequeNo = "654321";
			charge.JR_JH = job.PK;
			Factory.Save();
			BusinessObject consol = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>());
			consol[JobConsolSchema.JK_UniqueConsignRef] = "TEST321";
			BusinessObject consolCost = Factory.NewWithValidTestData(ObjectFactory.GetType<IJobConsolCost>());
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AB = objectCreator.AUDBankAccount.PK;
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge.JR_ChequeNo = "321654";
			charge.JR_JH = job.PK;
			charge.JR_E6 = consolCost.PK;
			Factory.Save();
			AssertEquals("Check number 123456 is already in use.", ChequeOrReferenceValidationHelper.GetInUseErrorMessage("123456", objectCreator.AUDBankAccount, Factory));
			AssertEquals("Check number 654321 is already used on Job TEST123.", ChequeOrReferenceValidationHelper.GetInUseErrorMessage("654321", objectCreator.AUDBankAccount, Factory));
			AssertEquals("Check number 321654 is already used on Consol TEST321.", ChequeOrReferenceValidationHelper.GetInUseErrorMessage("321654", objectCreator.AUDBankAccount, Factory));
		}

		public void TestCheckIsNotInChequeBook()
		{
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_StartNo = 1000;
			testChequeBook.AK_LastNo = 2000;
			AssertEquals(string.Empty, ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(null, "XXXX"));
			AssertEquals(GetExpectedErrorMessage(((decimal)int.MaxValue + 1).ToString()), ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(testChequeBook, ((decimal)int.MaxValue + 1).ToString()));
			AssertEquals(string.Empty, ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(testChequeBook, "1000"));
			AssertEquals(GetExpectedErrorMessage("999"), ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(testChequeBook, "999"));
			AssertEquals(string.Empty, ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(testChequeBook, "2000"));
			AssertEquals(GetExpectedErrorMessage("2001"), ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(testChequeBook, "2001"));
			AssertEquals(string.Empty, ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(testChequeBook, "1500"));
			AssertEquals(GetExpectedErrorMessage(""), ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(testChequeBook, ""));
			AssertEquals(GetExpectedErrorMessage("1500.5"), ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(testChequeBook, "1500.5"));
		}

		string GetExpectedErrorMessage(string chequeNum)
		{
			return string.Format(@"Check number {0} is not contained in the selected check book.
The check number must be between 001000 and 002000.", chequeNum);
		}
	}
}