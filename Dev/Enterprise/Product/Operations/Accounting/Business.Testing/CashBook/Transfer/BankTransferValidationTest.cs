using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	public class BankTransferValidationTest : TestCaseWithFactory
	{
		public void TestValidateAll()
		{
			BankTransfer testBankTransfer = new BankTransfer(Factory, null);
			testBankTransfer.Description = "";
			testBankTransfer.Reference = "";
			testBankTransfer.Validation.ValidateAll();

			AssertEquals(true, testBankTransfer.DescriptionInfo.HasErrors());
			AssertEquals(true, testBankTransfer.ReferenceInfo.HasErrors());
		}

		public void TestValidateDescription()
		{
			BankTransfer testBankTransfer = new BankTransfer(Factory, null);

			testBankTransfer.Description = "abc";
			AssertEquals(false, testBankTransfer.DescriptionInfo.HasErrors());

			testBankTransfer.Description = "";
			AssertEquals(true, testBankTransfer.DescriptionInfo.HasErrors());
		}

		public void TestValidateReference()
		{
			BankTransfer testBankTransfer = new BankTransfer(Factory, null);

			testBankTransfer.Reference = "abc";
			AssertEquals(false, testBankTransfer.ReferenceInfo.HasErrors());

			testBankTransfer.Reference = "";
			AssertEquals(true, testBankTransfer.ReferenceInfo.HasErrors());
		}

		public void TestCheckFromBankAccount()
		{
			var bankTransfer = new BankTransfer(Factory, null);
			bankTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			AssertNoErrors(bankTransfer.BankTransferFromPKInfo);

			var paymentBatch = CreatePaymentBatch(TestObjectCreator.USDBankAccount.PK);
			bankTransfer.BankTransferFundingInfoCalculator_ForTestOnly = new BankTransferFundingInfoCalculator(bankTransfer);
			bankTransfer.BankTransferFundingInfoCalculator_ForTestOnly.ConfigureBankTransferFromPaymentBatch(paymentBatch, new List<Payment>());
			AssertNoErrors(bankTransfer.BankTransferFromPKInfo);

			bankTransfer.BankTransferFromPK = TestObjectCreator.EURBankAccount.PK;
			AssertHasError(bankTransfer.BankTransferFromPKInfo, "Funding currency is USD. You must select a bank account with the same currency.");

			bankTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			AssertNoErrors(bankTransfer.BankTransferFromPKInfo);
		}

		public void TestCheckBankCharge()
		{
			var warningMsg = "Finance Charge is calculated from Total Provider Fees in Funding Currency using the BUY exchange rate. If you override the Sell Exchange Rate, please also re-calculate and update Finance Charge Amount.";
			var errorMsg = "Finance Charge is calculated from Total Provider Fees in Funding Currency using the Funding Currency's BUY rate. To post this Bank Transfer, please calculate the Bank Charge using the Funding Currency's exchange rate as entered.";
			var bankTransfer = new BankTransfer(Factory, null);
			bankTransfer.FinanceChargeOSAmount = 20;
			AssertNoError(bankTransfer.FinanceChargeOSAmountInfo, errorMsg);
			AssertNoWarning(bankTransfer.FinanceChargeOSAmountInfo, warningMsg);

			var paymentBatch = CreatePaymentBatch(TestObjectCreator.AUDBankAccount.PK);
			bankTransfer.BankTransferFundingInfoCalculator_ForTestOnly = new BankTransferFundingInfoCalculator(bankTransfer);
			bankTransfer.BankTransferFundingInfoCalculator_ForTestOnly.ConfigureBankTransferFromPaymentBatch(paymentBatch, new List<Payment>());
			AssertNoError(bankTransfer.FinanceChargeOSAmountInfo, errorMsg);
			AssertNoWarning(bankTransfer.FinanceChargeOSAmountInfo, warningMsg);

			paymentBatch = CreatePaymentBatch(TestObjectCreator.USDBankAccount.PK);
			bankTransfer.BankTransferFundingInfoCalculator_ForTestOnly = new BankTransferFundingInfoCalculator(bankTransfer);
			bankTransfer.BankTransferFundingInfoCalculator_ForTestOnly.ConfigureBankTransferFromPaymentBatch(paymentBatch, new List<Payment>());
			AssertHasError(bankTransfer.FinanceChargeOSAmountInfo, errorMsg);
			AssertNoWarning(bankTransfer.FinanceChargeOSAmountInfo, warningMsg);

			bankTransfer.FinanceChargeOSAmount = 20;
			AssertNoError(bankTransfer.FinanceChargeOSAmountInfo, errorMsg);
			AssertHasWarning(bankTransfer.FinanceChargeOSAmountInfo, warningMsg);

			bankTransfer.FinanceChargeOSAmount = 0;
			TestObjectCreator.CreateExchangeRate(testObjectCreator.USD, 0.5m);
			bankTransfer.RunPreSaveValidation();
			AssertNoError(bankTransfer.FinanceChargeOSAmountInfo, errorMsg);
			AssertHasWarning(bankTransfer.FinanceChargeOSAmountInfo, warningMsg);
		}

		APPaymentBatchPoster CreatePaymentBatch(ZGuid fundingBankAccountPK)
		{
			var paymentBatch = Factory.New<APPaymentBatchPoster>();
			paymentBatch.APB_AB_FundingBankAccount = fundingBankAccountPK;
			paymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			return paymentBatch;
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}

		TestObjectCreator testObjectCreator;
	}
}
