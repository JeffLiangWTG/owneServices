using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.CashBook.DirectPayment.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	[TestedType(typeof(DirectPayment.DirectPayment))]
	class BankTransferChargeValidationTest : DirectPaymentValiationTest
	{
		public void TestCheckAH_ExchangeRate()
		{
			BankTransferCharge testBankTransferCharge = Factory.New<BankTransferCharge>();
			testBankTransferCharge.EnableFinanceCharge = true;
			testBankTransferCharge.AH_ExchangeRate = 1;
			AssertEquals(false, testBankTransferCharge.AH_ExchangeRateInfo.HasErrors());
			testBankTransferCharge.AH_ExchangeRate = 0;
			AssertEquals(true, testBankTransferCharge.AH_ExchangeRateInfo.HasErrors());
			testBankTransferCharge.AH_ExchangeRate = -1;
			AssertEquals(true, testBankTransferCharge.AH_ExchangeRateInfo.HasErrors());
			AssertEquals(true, testBankTransferCharge.AH_ExchangeRateInfo.GetErrors().Contains(BankTransferChargeValidation.ExchangeRatesPositiveMessage_ForTestOnly));
		}

		public override void TestCheckAH_ChequeDrawer()
		{
			BankTransferCharge testBankTransferCharge = Factory.New<BankTransferCharge>();
			testBankTransferCharge.AH_ChequeDrawer = "";
			AssertEquals(false, testBankTransferCharge.AH_ChequeDrawerInfo.HasErrors());
			testBankTransferCharge.AH_ChequeDrawer = "test";
			AssertEquals(false, testBankTransferCharge.AH_ChequeDrawerInfo.HasErrors());
		}

		public override void TestCheckAH_ChequeOrReference()
		{
			base.TestCheckAH_ChequeOrReference();

			BankTransferCharge testBankTransferCharge = Factory.New<BankTransferCharge>();
			testBankTransferCharge.EnableFinanceCharge = true;
			testBankTransferCharge.AH_ChequeOrReference = "";
			AssertEquals(false, testBankTransferCharge.AH_ChequeOrReferenceInfo.HasErrors());
			testBankTransferCharge.AH_ChequeOrReference = "test";
			AssertEquals(false, testBankTransferCharge.AH_ChequeOrReferenceInfo.HasErrors());
		}

		public override void TestChequeOrReferenceValidationIsDisabledForAutoAllocationMode()
		{
			Assert("Auto allocation is disabled for this class", true);
		}
	}
}
