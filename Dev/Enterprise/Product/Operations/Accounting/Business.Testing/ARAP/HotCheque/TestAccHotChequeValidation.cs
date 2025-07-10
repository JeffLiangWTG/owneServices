using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.HotCheque.Testing
{
	internal class TestAccHotChequeValidation : BusinessObjectValidationTestCase
	{
		public void TestCheckAQ_OH_IsValid()
		{
			var validation = new AccHotChequeValidation(Cheque);
			Cheque.AQ_OH = TestObjectCreator.Debtor.PK;
			validation.ValidateAQ_OH();
			AssertHasError(Cheque.AQ_OHInfo, "Enter a valid Creditor.");
			Cheque.AQ_OH = TestObjectCreator.Creditor1.PK;
			validation.ValidateAQ_OH();
			AssertNoErrors(Cheque.AQ_OHInfo);
		}

		public void TestValidateAQ_OH()
		{
			Cheque.AQ_OH = ZGuid.Empty;
			Assert(Cheque.AQ_OHInfo.HasErrors());
		}

		public void TestValidateAQ_AK()
		{
			Cheque.AQ_AK = ZGuid.Empty;
			Assert(Cheque.AQ_AKInfo.HasErrors());

			AccChequeBook newChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			newChequeBook.AK_IsActive = ZBool.True;

			Cheque.AQ_AK = newChequeBook.PK;
			Assert("Should be no errors so far", !Cheque.AQ_AKInfo.HasErrors());
			newChequeBook.AK_IsActive = ZBool.False;
			Cheque.Validation.ValidateAQ_AK();
			Assert(Cheque.AQ_AKInfo.HasErrors());

			newChequeBook.AK_IsActive = ZBool.True;
			newChequeBook.AK_AutoPrintCheque = ZBool.True;
			Cheque.Validation.ValidateAQ_AK();
			AssertHasError(Cheque.AQ_AKInfo, "Auto printing of check is not configured properly.");

			newChequeBook.BankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			newChequeBook.AK_LastNo = 300000;
			newChequeBook.AK_SQ = Factory.NewWithValidTestData<StmPrintQueue>().PK;
			Cheque.Validation.ValidateAQ_AK();
			AssertNoErrors(Cheque.AQ_AKInfo);

			Env.Security.PrintCheque.IsAllowed = false;
			Cheque.Validation.ValidateAQ_AK();
			AssertHasError(Cheque.AQ_AKInfo, "You do not have the permission to print Check. Please Contact System Administrator.");

			Env.Security.PrintCheque.IsAllowed = true;
			Cheque.Validation.ValidateAQ_AK();
			AssertNoErrors(Cheque.AQ_AKInfo);

			Env.Security.PrintCheque.IsAllowed = false;
			newChequeBook.AK_AutoPrintCheque = ZBool.False;
			Cheque.Validation.ValidateAQ_AK();
			AssertNoErrors(Cheque.AQ_AKInfo);

			newChequeBook.BankAccount.AB_IsActive = false;
			Cheque.Validation.ValidateAQ_AK();
			AssertHasError(Cheque.AQ_AKInfo, "This check book is linked to inactive bank account.");
		}

		public void TestValidateAQ_ChequePayee()
		{
			Cheque.AQ_ChequePayee = "";
			Assert(Cheque.AQ_ChequePayeeInfo.HasErrors());
		}

		public void TestValidateAQ_Amount()
		{
			Cheque.AQ_Amount = 0;
			Assert(Cheque.AQ_AmountInfo.HasErrors());
		}

		public void TestValidateAQ_ChequeDate()
		{
			AccHotCheque cheque = Factory.New<AccHotCheque>();
			AccountingConfigurationRegistry.Instance.AccountingRequiredChequeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			cheque.AQ_ChequeDate = ZDateTime.Empty;
			AssertEquals("HasErrors", false, cheque.AQ_ChequeDateInfo.HasErrors());

			AccountingConfigurationRegistry.Instance.AccountingRequiredChequeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			cheque.Validation.ValidateAQ_ChequeDate();
			AssertEquals("HasErrors", true, cheque.AQ_ChequeDateInfo.HasErrors());
		}

		public void TestValidateAQ_JH()
		{
			AccHotCheque cheque = Factory.New<AccHotCheque>();
			AccountingConfigurationRegistry.Instance.AccountingRequiredJobNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			cheque.AQ_JH = ZGuid.Empty;
			AssertEquals("HasErrors", false, cheque.AQ_JHInfo.HasErrors());

			AccountingConfigurationRegistry.Instance.AccountingRequiredJobNumber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			cheque.Validation.ValidateAQ_JH();
			AssertEquals("HasErrors", true, cheque.AQ_JHInfo.HasErrors());
		}

		public void TestValidateAQ_MasterBill()
		{
			AccHotCheque cheque = Factory.New<AccHotCheque>();
			AccountingConfigurationRegistry.Instance.AccountingRequiredMasterBillNo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			cheque.AQ_MasterBill = "";
			AssertEquals("HasErrors", false, cheque.AQ_MasterBillInfo.HasErrors());

			AccountingConfigurationRegistry.Instance.AccountingRequiredMasterBillNo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			cheque.Validation.ValidateAQ_MasterBill();
			AssertEquals("HasErrors", true, cheque.AQ_MasterBillInfo.HasErrors());
		}

		public void TestValidateAQ_HouseBill()
		{
			AccHotCheque cheque = Factory.New<AccHotCheque>();
			AccountingConfigurationRegistry.Instance.AccountingRequiredHouseBillNo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			cheque.AQ_HouseBill = "";
			AssertEquals("HasErrors", false, cheque.AQ_HouseBillInfo.HasErrors());

			AccountingConfigurationRegistry.Instance.AccountingRequiredHouseBillNo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			cheque.Validation.ValidateAQ_HouseBill();
			AssertEquals("HasErrors", true, cheque.AQ_HouseBillInfo.HasErrors());
		}

		public void TestValidateAQ_Description()
		{
			AccHotCheque cheque = Factory.New<AccHotCheque>();
			AccountingConfigurationRegistry.Instance.AccountingRequiredDescription.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			cheque.AQ_Description = "";
			AssertEquals("HasErrors", false, cheque.AQ_DescriptionInfo.HasErrors());

			AccountingConfigurationRegistry.Instance.AccountingRequiredDescription.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			cheque.Validation.ValidateAQ_Description();
			AssertEquals("HasErrors", true, cheque.AQ_DescriptionInfo.HasErrors());
		}

		public void TestValidateAQ_GS()
		{
			AccHotCheque cheque = Factory.New<AccHotCheque>();
			AccountingConfigurationRegistry.Instance.AccountingRequiredStaff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			cheque.AQ_GS_NKResponsibleStaff = ZString.Empty;
			AssertEquals("HasErrors", false, cheque.AQ_GS_NKResponsibleStaffInfo.HasErrors());

			AccountingConfigurationRegistry.Instance.AccountingRequiredStaff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			cheque.Validation.ValidateAQ_GS_NKResponsibleStaff();
			AssertEquals("HasErrors", true, cheque.AQ_GS_NKResponsibleStaffInfo.HasErrors());
		}

		public void TestValidateAQ_ChequeNumberLeadingZeros()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = 5;

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 500;
			testChequeBook.AK_CurrentNo = 1;

			AccHotCheque testHotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			testHotCheque.AQ_AK = testChequeBook.PK;

			testHotCheque.AQ_ChequeNumber = "1";
			AssertEquals("Cheque number should be 00001", "00001", testHotCheque.AQ_ChequeNumber);
		}

		public void TestValidateAQ_ChequeNumber()
		{
			AccChequeBook testBook2 = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			AccHotCheque testHotCheque = Factory.New(typeof(AccHotCheque)) as AccHotCheque;

			fTestChequeBook.AK_AB = fTestBank.PK;
			testBook2.AK_AB = fTestBank.PK;
			Cheque.AQ_AK = fTestChequeBook.PK;			// Cheque.AQ_ChequeNumber is automatically set 
			testHotCheque.AQ_AK = fTestChequeBook.PK;		// TestHotCheque.AQ_ChequeNumber is automatically set 

			testHotCheque.AQ_ChequeNumber = "###";
			AssertHasError("cheque number must be numeric", testHotCheque.AQ_ChequeNumberInfo, "Only numbers are allowed in this field.");

			testHotCheque.AQ_ChequeNumber = "123";
			Cheque.AQ_ChequeNumber = "123";

			Factory.Save();

			Cheque.Validation.ValidateAQ_ChequeNumber();
			Assert("Cheque number must be unique", Cheque.AQ_ChequeNumberInfo.HasErrors());

			Cheque.AQ_AK = testBook2.PK;
			Cheque.AQ_ChequeNumber = "123";
			Factory.Save();

			Cheque.Validation.ValidateAQ_ChequeNumber();
			Assert("Cheque number must be unique", Cheque.AQ_ChequeNumberInfo.HasErrors());

			Cheque.AQ_ChequeNumber = "9876543210";
			AssertEquals("Cheque Number must be less than " + fTestBank.AB_ChequeNumDigits + 1, true, Cheque.AQ_ChequeNumberInfo.HasErrors());

			AccBankAccount testAccount2 = Factory.New(typeof(AccBankAccount)) as AccBankAccount;
			testAccount2.AB_AG = Factory.NewWithValidTestData(typeof(AccGLHeader)).PK;
			testAccount2.AB_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			testAccount2.AB_Code = "ABCBANK";
			fTestBank.AB_AccountNum = "!!";
			fTestBank.AB_BSB = "@@";
			fTestBank.AB_Code = "@@";
			testBook2.AK_AB = testAccount2.PK;
			testBook2.AK_StartNo = 122;
			testBook2.AK_LastNo = 124;
			Factory.Save();

			Cheque.AQ_ChequeNumber = "123";
			Cheque.Validation.ValidateAQ_ChequeNumber();
			Assert("expect no errors", !Cheque.AQ_ChequeNumberInfo.HasErrors());

			testBook2.AK_StartNo = 12;
			testBook2.AK_LastNo = 14;

			Factory.Save();
			Cheque.Validation.ValidateAQ_ChequeNumber();
			AssertHasError("number should be within the specified range", Cheque.AQ_ChequeNumberInfo, GetExpectedErrorMessage(Cheque.AQ_ChequeNumber));

			Cheque.AQ_ChequeNumber = ((decimal)int.MaxValue + 1).ToString();
			AssertHasError(Cheque.AQ_ChequeNumberInfo, GetExpectedErrorMessage(Cheque.AQ_ChequeNumber));

			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook();
			Cheque.AQ_AK = testBookWithAutoAllocation.PK;
			Cheque.Validation.ValidateAQ_ChequeNumber();
			Assert("Should be no errors, as the number is autoallocated", !Cheque.AQ_ChequeNumberInfo.HasErrors());
		}

		string GetExpectedErrorMessage(string chequeNum)
		{
			return string.Format(@"Check number {0} is not contained in the selected check book.
The check number must be between 000012 and 000014.", chequeNum);
		}

		AccHotCheque Cheque;
		AccBankAccount fTestBank;
		AccChequeBook fTestChequeBook;

		protected override void SetUp()
		{
			base.SetUp();
			Cheque = Factory.New<AccHotCheque>();
			Cheque.AQ_Printed = false;

			fTestBank = Factory.NewWithValidTestData<AccBankAccount>();
			fTestBank.AB_ChequeNumDigits = 5;
			fTestBank.AB_AG = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
			fTestBank.AB_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;

			fTestChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			fTestChequeBook.AK_AB = fTestBank.PK;
			fTestChequeBook.AK_StartNo = 10000;
			fTestChequeBook.AK_LastNo = 99999;

			Factory.Save();
		}

		AccChequeBook GetAutoPrintChequeBook()
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
