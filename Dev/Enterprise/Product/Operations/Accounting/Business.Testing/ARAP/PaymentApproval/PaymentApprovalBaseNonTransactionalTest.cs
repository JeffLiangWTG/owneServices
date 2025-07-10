using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[UseSnapshotProtection]
	public class PaymentApprovalBaseNonTransactionalTest : TestCase
	{
		public void TestIsAllowedReturnsFalseWhenCashBookAllowFuturePostingOfCashBookTransactionsIsNull()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			testObjectCreator.ResetSecurityCore();
			AssertEquals(false, Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowedWithConstraint());
			var chequeBook = testObjectCreator.GetAutoPrintChequeBook(new BusinessObjectFactory(), 1, 200, 23);
			var bankAccount = factory.Load<AccBankAccount>(chequeBook.AK_AB);
			var paymentApprovalBase = testObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, bankAccount, chequeBook);
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Assert(AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.Value);
			Assert("CashBookAllowFuturePostingOfTransactions.IsAllowed should be false", !paymentApprovalBase.AllowFuturePosting);
		}

		public void TestAssigningChequeNumberHappensInsideTransaction()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);

			var chequeBook = testObjectCreator.GetAutoPrintChequeBook(new BusinessObjectFactory(), 1, 200, 23);
			var bankAccount = factory.Load<AccBankAccount>(chequeBook.AK_AB);
			var paymentApproval = testObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, bankAccount, chequeBook);
			paymentApproval.AV_Amount = 1000M;
			paymentApproval.AV_OH = new ZGuid("DC80504A-7221-4447-8BAC-477A5CC65F9E"); // attempt to forcefully trigger foreign key constraint exception
			paymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
			paymentApproval.IsAllowedToPost = true;

			try
			{
				factory.Save();
				Fail("Saving failed because of foreign key constraint exception.");
			}
			catch (ZSaveException)
			{
				Assert("ZSaveException occurred ", true);
			}

			var newFactory = new BusinessObjectFactory();
			var chequeBookInNewFactory = newFactory.Load<AccChequeBook>(chequeBook.PK);
			AssertEquals("Current cheque book number should not increase, it should remain 23", 23m, chequeBookInNewFactory.AK_CurrentNo);
		}

		public void TestAH_ChequeOrReferenceIsResetAndNoDoubleLogWhenPostingFails()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);

			var bankAccount = testObjectCreator.CreateBankAccount("TSTBNK1", "Test Bank Account", testObjectCreator.AUD, testObjectCreator.GLHeader1);
			var checkBook = testObjectCreator.CreateChequeBook("CH", 100, bankAccount);
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "TST101";
			orgHeader.OH_IsCreditor = true;

			factory.Save();

			PaymentApprovalBase testPaymentApproval = factory.New<APPaymentApprovalWithoutAuthorisation>();
			testPaymentApproval.AV_OH = orgHeader.PK;
			testPaymentApproval.AV_PaymentComment = "AP PAYMENT DESCRIPTION";
			testPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			testPaymentApproval.AV_AB = bankAccount.PK;
			testPaymentApproval.AV_AK = checkBook.PK;
			testPaymentApproval.AV_Amount = 1000M;
			testPaymentApproval.AV_Status = PaymentApprovalStatus.Posted;

			testObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), orgHeader, testPaymentApproval, 1000M);

			testPaymentApproval.CreateNewPayment();
			testPaymentApproval.NewPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			testPaymentApproval.NewPayment.ChequeBook = checkBook.PK;
			testPaymentApproval.NewPayment.AH_ChequeOrReference = "000001";

			testPaymentApproval.AV_AH = Guid.NewGuid();

			try
			{
				factory.Save();
				Assert(false);
			}
			catch
			{
			}

			Assert("Check number should be cleared", string.IsNullOrEmpty(testPaymentApproval.NewPayment.AH_ChequeOrReference));

			testPaymentApproval.AV_AH = testPaymentApproval.NewPayment.PK;
			factory.Save();

			AssertEquals("There should be only one PST log entry", 1, testPaymentApproval.Logs.GetAllLogs().Where(x => x.SL_SE_NKEvent == "PST").Count());
		}
	}
}
