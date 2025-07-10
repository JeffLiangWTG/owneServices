using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectPayment.Testing
{
	[TestedType(typeof(DirectPayment))]
	class DirectPaymentValiationTest : DirectTransactionHeaderBaseValidationTest
	{
		public void TestCheckAH_DrawerBank()
		{
			AssertEquals("Percondition", true, TestBizO.AH_DrawerBankInfo.ReadOnly);

			TestBizO.AH_DrawerBank = string.Empty;
			AssertEquals("Should not check Drawer Bank when ReadOnly", false, TestBizO.AH_DrawerBankInfo.HasErrors());

			fTestBank.AB_AllowAutoDDR = true;
			TestBizO.AH_ReceiptType = ReceiptTypes.DirectDebit;
			TestBizO.AH_AB = fTestBank.PK;
			AssertEquals("Percondition", false, TestBizO.AH_DrawerBankInfo.ReadOnly);

			TestBizO.AH_DrawerBank = string.Empty;
			AssertEquals("Should check Drawer Bank when NOT ReadOnly", true, TestBizO.AH_DrawerBankInfo.HasError("Please enter a Drawer Bank."));

			TestBizO.AH_DrawerBank = "abc";
			AssertEquals("Should check Drawer Bank when NOT ReadOnly", false, TestBizO.AH_DrawerBankInfo.HasErrors());

			fTestBank.AB_AutoDDRFormat = Constants.DDRFileFormat.ASB;
			TestBizO.AH_DrawerBank = "123";
			AssertEquals("Should check Drawer Bank when NOT ReadOnly", true, TestBizO.AH_DrawerBankInfo.HasError(TestBizO.BankAccount.GetInvalidAccountNumberErrorMessage()));
		}

		public void TestCheckAH_DrawerBranch()
		{
			AssertEquals("Percondition", true, TestBizO.AH_DrawerBranchInfo.ReadOnly);

			TestBizO.AH_DrawerBranch = string.Empty;
			AssertEquals("Should not check Drawer Branch when ReadOnly", false, TestBizO.AH_DrawerBranchInfo.HasErrors());

			fTestBank.AB_AllowAutoDDR = true;
			TestBizO.AH_ReceiptType = ReceiptTypes.DirectDebit;
			TestBizO.AH_AB = fTestBank.PK;
			AssertEquals("Percondition", false, TestBizO.AH_DrawerBranchInfo.ReadOnly);

			TestBizO.AH_DrawerBranch = string.Empty;
			AssertEquals("Should check Drawer Branch when NOT ReadOnly", true, TestBizO.AH_DrawerBranchInfo.HasError("Please enter a Drawer Branch."));

			TestBizO.AH_DrawerBranch = "abc";
			AssertEquals("Should check Drawer Branch when NOT ReadOnly", false, TestBizO.AH_DrawerBranchInfo.HasErrors());

			fTestBank.AB_AutoDDRFormat = Constants.DDRFileFormat.ASB;
			TestBizO.AH_DrawerBranch = "123";
			AssertEquals("Should check Drawer Branch when NOT ReadOnly", true, TestBizO.AH_DrawerBranchInfo.HasError(TestBizO.BankAccount.GetInvalidBSBNumberErrorMessage()));
		}

		public void TestCheckAH_ChequeOrReference_ValidateMiddleChequeNumber()
		{
			TestBizO = Factory.NewWithValidTestData<DirectPayment>();
			TestBizO.AH_AB = fTestBank.PK;
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestBizO.ChequeBookPK = fTestChequeBook.PK;
			TestBizO.AH_ChequeOrReference = "10000";
			Factory.Save();

			TestBizO = Factory.NewWithValidTestData<DirectPayment>();
			TestBizO.AH_AB = fTestBank.PK;
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestBizO.ChequeBookPK = fTestChequeBook.PK;
			TestBizO.AH_ChequeOrReference = "10003";
			Factory.Save();

			TestBizO = Factory.NewWithValidTestData<DirectPayment>();
			TestBizO.AH_AB = fTestBank.PK;
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestBizO.ChequeBookPK = fTestChequeBook.PK;
			TestBizO.AH_ChequeOrReference = "10002";

			AssertEquals("Should have NO error", false, TestBizO.AH_ChequeOrReferenceInfo.HasErrors());

			TestBizO = Factory.NewWithValidTestData<DirectPayment>();
			TestBizO.AH_AB = fTestBank.PK;
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestBizO.ChequeBookPK = fTestChequeBook.PK;
			TestBizO.AH_ChequeOrReference = "10003";

			AssertEquals("Should have Error", true, TestBizO.AH_ChequeOrReferenceInfo.HasErrors());
		}

		public void TestCheckChequeBookPK()
		{
			string warningSamePrinterMessage = AccChequeBook.WarningChequeBookWithSamePrinterMessageStart + "'Book1'" + AccChequeBook.WarningSamePrinterMessageEnd;

			BusinessObject printer = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			fTestChequeBook.AK_AutoPrintCheque = ZBool.True;
			fTestChequeBook.AK_SQ = printer.PK;
			fTestChequeBook.AK_Code = "Book1";

			AccChequeBook book2 = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			book2.AK_AB = fTestBank.PK;
			book2.AK_GB = fTestChequeBook.AK_GB;
			book2.AK_Code = "Book2";
			book2.AK_AutoPrintCheque = ZBool.True;
			book2.AK_SQ = printer.PK;

			TestBizO = Factory.NewWithValidTestData<DirectPayment>();
			TestBizO.AH_AB = fTestBank.PK;
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestBizO.ChequeBookPK = book2.PK;
			TestBizO.AH_ChequeOrReference = "10003";
			Assert("Should have warning about another Check Book with the same printer", TestBizO.ChequeBookPKInfo.HasWarning(warningSamePrinterMessage));

			book2.AK_SQ = Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>().PK;
			((DirectPaymentValidation)TestBizO.Validation).ValidateChequeBookPK();
			AssertHasError(TestBizO.ChequeBookPKInfo, "Auto printing of check is not configured properly.");

			fTestChequeBook.BankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			((DirectPaymentValidation)TestBizO.Validation).ValidateChequeBookPK();
			AssertNoErrors(TestBizO.ChequeBookPKInfo);

			Env.Security.PrintCheque.IsAllowed = false;
			((DirectPaymentValidation)TestBizO.Validation).ValidateChequeBookPK();
			AssertHasError(TestBizO.ChequeBookPKInfo, "You do not have the permission to print Check. Please Contact System Administrator.");

			Env.Security.PrintCheque.IsAllowed = true;
			((DirectPaymentValidation)TestBizO.Validation).ValidateChequeBookPK();
			AssertNoErrors(TestBizO.ChequeBookPKInfo);

			Env.Security.PrintCheque.IsAllowed = false;
			book2.AK_AutoPrintCheque = ZBool.False;
			((DirectPaymentValidation)TestBizO.Validation).ValidateChequeBookPK();
			AssertNoErrors(TestBizO.ChequeBookPKInfo);
		}

		public void TestCheckAH_ChequeOrReference_ChequeNumberInBookRange()
		{
			DirectPayment testBizO = Factory.NewWithValidTestData<DirectPayment>();
			testBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testBizO.ChequeBookPK = fTestChequeBook.PK;
			testBizO.AH_ChequeOrReference = "1234";
			AssertHasError(testBizO.AH_ChequeOrReferenceInfo, GetExpectedErrorMessage(testBizO.AH_ChequeOrReference));

			testBizO.AH_ChequeOrReference = "12345";
			AssertEquals("Valid Cheque Number", false, testBizO.AH_ChequeOrReferenceInfo.HasErrors());

			testBizO.AH_ChequeOrReference = "123456";
			AssertHasError(testBizO.AH_ChequeOrReferenceInfo, GetExpectedErrorMessage(testBizO.AH_ChequeOrReference));

			testBizO.AH_ChequeOrReference = ((decimal)int.MaxValue + 1).ToString();
			AssertHasError(testBizO.AH_ChequeOrReferenceInfo, GetExpectedErrorMessage(testBizO.AH_ChequeOrReference));

			testBizO.AH_ChequeOrReference = "12X45";
			AssertHasError(testBizO.AH_ChequeOrReferenceInfo, "Only numbers are allowed in this field.");
		}

		string GetExpectedErrorMessage(string chequeNum)
		{
			return string.Format(@"Check number {0} is not contained in the selected check book.
The check number must be between 10000 and 99999.", chequeNum);
		}

		public void TestCheckAH_ChequeOrReference_ChequeNumberInUse()
		{
			PrepareForTestChequeNumberInUse();

			DirectPayment testBizO2 = Factory.NewWithValidTestData<DirectPayment>();
			testBizO2.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testBizO2.AH_AB = fTestBank.PK;
			testBizO2.ChequeBookPK = fTestChequeBook.PK;

			testBizO2.AH_ChequeOrReference = "10001";
			AssertHasErrors("JobCharge with this cheque number", testBizO2.AH_ChequeOrReferenceInfo);
			AssertHasErrorContaining(testBizO2.AH_ChequeOrReferenceInfo, "Check number 10001 is already used on Job ");

			testBizO2.AH_ChequeOrReference = "10002";
			AssertNoErrors("Payment Approval with this cheque number", testBizO2.AH_ChequeOrReferenceInfo);

			testBizO2.AH_ChequeOrReference = "10003";
			AssertNoErrors("Hot Cheque with this cheque number", testBizO2.AH_ChequeOrReferenceInfo);

			testBizO2.AH_ChequeOrReference = "10004";
			AssertHasErrors("AP Payment with this cheque number", testBizO2.AH_ChequeOrReferenceInfo);

			testBizO2.AH_ChequeOrReference = "10005";
			AssertHasErrors("Direct Payment with this cheque number", testBizO2.AH_ChequeOrReferenceInfo);

			testBizO2.AH_ChequeOrReference = "10006";
			AssertNoErrors("Reversed Payment with this cheque number", testBizO2.AH_ChequeOrReferenceInfo);

			testBizO2.AH_ChequeOrReference = "10007";
			AssertNoErrors("Nothing using this cheque number", testBizO2.AH_ChequeOrReferenceInfo);
		}

		public void TestCheckAH_ChequeOrReference_CancelledChequeNumber()
		{
			DirectPayment testBizO = Factory.NewWithValidTestData<DirectPayment>();
			testBizO.AH_AB = fTestBank.PK;
			testBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testBizO.ChequeBookPK = fTestChequeBook.PK;
			testBizO.AH_ChequeOrReference = "12345";
			AssertEquals("Valid cheque number", false, testBizO.AH_ChequeOrReferenceInfo.HasErrors());

			testBizO.AH_IsCancelled = true;

			DirectPayment testBizO2 = Factory.NewWithValidTestData<DirectPayment>();
			testBizO2.AH_AB = fTestBank.PK;
			testBizO2.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testBizO2.ChequeBookPK = fTestChequeBook.PK;
			testBizO2.AH_ChequeOrReference = "12345";
			AssertEquals("Cancelled Cheque number", false, testBizO2.AH_ChequeOrReferenceInfo.HasErrors());

			Factory.Save();

			DirectPayment testBizO3 = Factory.NewWithValidTestData<DirectPayment>();
			testBizO3.AH_AB = fTestBank.PK;
			testBizO3.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testBizO3.ChequeBookPK = fTestChequeBook.PK;
			testBizO3.AH_ChequeOrReference = "12345";
			AssertEquals("Used Cheque number", true, testBizO3.AH_ChequeOrReferenceInfo.HasErrors());
		}

		public void TestBankAccountNumberDDRValidationASB()
		{
			fTestBank.AB_AllowAutoDDR = true;
			fTestBank.AB_AutoDDRFormat = Constants.DDRFileFormat.ASB;
			TestBizO.AH_AB = fTestBank.PK;
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			TestBizO.AH_DrawerBank = "123";
			AssertEquals(true, TestBizO.AH_DrawerBankInfo.HasError(TestBizO.BankAccount.GetInvalidAccountNumberErrorMessage()));

			TestBizO.AH_DrawerBank = "123456789";
			AssertEquals(false, TestBizO.AH_DrawerBankInfo.HasErrors());

			TestBizO.AH_DrawerBank = "12345678901";
			AssertEquals(true, TestBizO.AH_DrawerBankInfo.HasError(TestBizO.BankAccount.GetInvalidAccountNumberErrorMessage()));
		}

		public void TestBankAccountNumberDDRValidationOtherBanks()
		{
			fTestBank.AB_AllowAutoDDR = true;
			fTestBank.AB_AutoDDRFormat = Constants.DDRFileFormat.ANZ;
			TestBizO.AH_AB = fTestBank.PK;
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			TestBizO.AH_DrawerBank = "123";
			AssertEquals(false, TestBizO.AH_DrawerBankInfo.HasErrors());

			TestBizO.AH_DrawerBank = "123456789";
			AssertEquals(false, TestBizO.AH_DrawerBankInfo.HasErrors());

			TestBizO.AH_DrawerBank = "12345678901";
			AssertEquals(true, TestBizO.AH_DrawerBankInfo.HasError(TestBizO.BankAccount.GetInvalidAccountNumberErrorMessage()));
		}

		public void TestValidateAll_ValidateChequeBookPK()
		{
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			TestBizO.AH_AB = testBank.PK;
			TestBizO.RunPreSaveValidation();
			AssertEquals(true, TestBizO.ChequeBookPKInfo.HasErrors());

			AccChequeBook testCheque = Factory.NewWithValidTestData<AccChequeBook>();
			TestBizO.ChequeBookPK = testCheque.PK;
			TestBizO.RunPreSaveValidation();
			AssertEquals(false, TestBizO.ChequeBookPKInfo.HasErrors());
		}

		public void TestAV_AKHasAutoAllocationValidation()
		{
			AccChequeBook newChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			TestBizO.AH_AB = newChequeBook.AK_AB;
			TestBizO.ChequeBookPK = newChequeBook.PK;

			Assert("Should be no errors so far", !TestBizO.ChequeBookPKInfo.HasErrors());

			newChequeBook.AK_IsActive = ZBool.False;
			TestBizO.ChequeBookPK = ZGuid.Empty;
			TestBizO.ChequeBookPK = newChequeBook.PK;
			AssertHasError(TestBizO.ChequeBookPKInfo, AccChequeBookAutoAllocationValidation.ChequeBookIsInActiveMessage);
		}

		public virtual void TestChequeOrReferenceValidationIsDisabledForAutoAllocationMode()
		{
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook autoPrintChequeBook = GetAutoPrintChequeBook(testBank);

			TestBizO = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;

			TestBizO.AH_AB = testChequeBook.AK_AB;
			TestBizO.ChequeBookPK = testChequeBook.PK;
			TestBizO.AH_ChequeOrReference = "BLAH!";
			AssertHasErrors("Cheque Number should have an error", TestBizO.AH_ChequeOrReferenceInfo);

			TestBizO.AH_AB = autoPrintChequeBook.AK_AB;
			TestBizO.ChequeBookPK = autoPrintChequeBook.PK;
			TestBizO.AH_ChequeOrReference = "BLAH!";
			AssertNoErrors("Should not have any errors as now in autoallocation mode", TestBizO.AH_ChequeOrReferenceInfo);
		}

		public override void TestCheckAH_OSTotalAmount()
		{
			foreach (CodeDescriptionPair pair in TestBizO.PaymentMethods)
			{
				TestBizO.AH_ReceiptType = pair.Code;
				if (pair.Code == ReceiptTypes.Cheque)
				{
					TestBizO.AH_OSExTaxAmount = -1m;
					((DirectPaymentValidation)TestBizO.Validation).ValidateAH_OSTotalAmount();
					AssertHasError(TestBizO.AH_OSTotalAmountInfo, "Total cannot be negative.");

					TestBizO.AH_OSExTaxAmount = 0m;
					((DirectPaymentValidation)TestBizO.Validation).ValidateAH_OSTotalAmount();
					AssertHasWarning(TestBizO.AH_OSTotalAmountInfo, "You are posting a payment with a payment type of 'CHQ' with zero value. This functionality is typically used to record canceled check numbers.");
					AssertNoErrors(TestBizO.AH_OSTotalAmountInfo);

					DirectPaymentLine line = (DirectPaymentLine)TestBizO.Lines.AddNew();
					line.AL_OSExTaxAmount = 0m;
					((DirectPaymentValidation)TestBizO.Validation).ValidateAH_OSTotalAmount();
					AssertNoWarnings(TestBizO.AH_OSTotalAmountInfo);
					AssertHasError(TestBizO.AH_OSTotalAmountInfo, "Total cannot be zero. If you want to create this transaction to record a canceled check, you should remove the transaction detail.");

					TestBizO.AH_OSExTaxAmount = 10m;
					line.AL_OSExTaxAmount = 10m;
					((DirectPaymentValidation)TestBizO.Validation).ValidateAH_OSTotalAmount();
					AssertNoErrors(TestBizO.AH_OSTotalAmountInfo);

					((DirectPaymentLine)TestBizO.Lines.AddNew()).AL_OSExTaxAmount = -20;
					AssertHasError(TestBizO.AH_OSTotalAmountInfo, "Total cannot be negative.");

					TestBizO.Lines.RemoveAndDeleteAll();
					AssertHasWarning(TestBizO.AH_OSTotalAmountInfo, "You are posting a payment with a payment type of 'CHQ' with zero value. This functionality is typically used to record canceled check numbers.");
					AssertNoErrors(TestBizO.AH_OSTotalAmountInfo);

					TestBizO.AH_ReceiptType = ReceiptTypes.Cash;
					AssertHasError(TestBizO.AH_OSTotalAmountInfo, "Total cannot be zero.");
				}
				else
				{
					base.TestCheckAH_OSTotalAmount();
				}
			}
		}

		#region Implementation

		protected AccChequeBook GetAutoPrintChequeBook(AccBankAccount bankAccount)
		{
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueue>());
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		protected override void SetUp()
		{
			base.SetUp();

			fTestBank = Factory.NewWithValidTestData<AccBankAccount>();
			fTestBank.AB_ChequeNumDigits = 5;
			fTestChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			fTestChequeBook.AK_AB = fTestBank.PK;
			fTestChequeBook.AK_StartNo = 10000;
			fTestChequeBook.AK_LastNo = 99999;
		}

		AccBankAccount fTestBank;
		AccChequeBook fTestChequeBook;

		#region PrepareForTestChequeNumberInUse

		void PrepareForTestChequeNumberInUse()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job testJob = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);

			Charge charge = creator.CreateCharge(testJob, creator.CC1, "Charge 1", creator.AUD, 1m, creator.Creditor1, creator.AUD, 1m, creator.Agent);
			charge.JR_RX_NKCostCurrency = creator.AUD.RX_Code;
			charge.JR_OSCostExRate = 1m;
			charge.JR_APInvoiceNum = "5";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge.JR_AB = fTestBank.PK;
			charge.BankAccount.AB_ChequeNumDigits = 5;
			charge.JR_AK = fTestChequeBook.PK;
			charge.JR_ChequeNo = "10001";
			Assert("Cheque number is not used yet", !charge.JR_ChequeNoInfo.HasErrors());

			APPaymentApprovalWithAuthorisation paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			paymentApproval.AV_OH = creator.ABIGAS.PK;
			paymentApproval.AV_AB = fTestBank.PK;
			paymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			paymentApproval.AV_AK = fTestChequeBook.PK;
			paymentApproval.AV_ChequeOrReference = "10002";
			Assert("Cheque number is not used yet", !paymentApproval.AV_ChequeOrReferenceInfo.HasErrors());

			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			hotCheque.AQ_AK = fTestChequeBook.PK;
			hotCheque.AQ_ChequeNumber = "10003";
			Assert("Cheque number is not used yet", !hotCheque.AQ_ChequeNumberInfo.HasErrors());

			Payment payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			payment.AH_AB = fTestBank.PK;
			payment.ChequeBook = fTestChequeBook.PK;
			payment.AH_ChequeOrReference = "10004";
			Assert("Cheque number is not used yet", !payment.AH_ChequeOrReferenceInfo.HasErrors());

			DirectPayment directPayment = Factory.NewWithValidTestData<DirectPayment>();
			directPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			directPayment.AH_AB = fTestBank.PK;
			directPayment.ChequeBookPK = fTestChequeBook.PK;
			directPayment.AH_ChequeOrReference = "10005";
			Assert("Cheque number is not used yet", !directPayment.AH_ChequeOrReferenceInfo.HasErrors());

			Payment reversedPayment = Factory.NewWithValidTestData<APPayment>();
			reversedPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			reversedPayment.AH_AB = fTestBank.PK;
			reversedPayment.ChequeBook = fTestChequeBook.PK;
			reversedPayment.AH_ChequeOrReference = "10006";
			reversedPayment.AH_IsCancelled = true;
			TransactionMatchLink matchLink = ((IMatching)reversedPayment).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
			matchLink.AP_AH = reversedPayment.PK;
			matchLink.AP_Amount = reversedPayment.AH_OutstandingAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLink);

			Factory.Save();
		}

		#endregion

		#endregion
	}
}
