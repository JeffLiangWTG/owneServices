using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public sealed class PaymentApprovalFundingInfoProviderTest : TestCaseWithFactory
	{
		public void TestGetFundingBankAccount()
		{
			var paymentApproval = CreatePaymentApproval();
			var provider = new PaymentApprovalFundingInfoProvider(paymentApproval);
			AssertEquals(ZGuid.Empty, provider.GetFundingBankAccount());

			paymentApproval.AV_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;
			provider = new PaymentApprovalFundingInfoProvider(paymentApproval);
			AssertEquals(TestObjectCreator.USDBankAccount.PK, provider.GetFundingBankAccount());

			provider = new PaymentApprovalFundingInfoProvider(null);
			AssertEquals(ZGuid.Empty, provider.GetFundingBankAccount());
		}

		public void TestGetFundingCurrency()
		{
			var paymentApproval = CreatePaymentApproval();
			var provider = new PaymentApprovalFundingInfoProvider(paymentApproval);
			AssertEquals(Env.CurrentCompany.LocalCurrency.Code, provider.GetFundingCurrency());

			paymentApproval.AV_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;
			provider = new PaymentApprovalFundingInfoProvider(paymentApproval);
			AssertEquals("USD", provider.GetFundingCurrency());

			provider = new PaymentApprovalFundingInfoProvider(null);
			AssertEquals(Env.CurrentCompany.LocalCurrency.Code, provider.GetFundingCurrency());
		}

		public void TestGetOriginalFundingCurrency()
		{
			var paymentApproval = CreatePaymentApproval();
			var provider = new PaymentApprovalFundingInfoProvider(paymentApproval);
			AssertEquals(Env.CurrentCompany.LocalCurrency.Code, provider.GetOriginalFundingCurrency());

			paymentApproval.AV_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;
			provider = new PaymentApprovalFundingInfoProvider(paymentApproval);
			AssertEquals(Env.CurrentCompany.LocalCurrency.Code, provider.GetOriginalFundingCurrency());

			var approval = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			approval.AV_RX_NKPaymentCurrency = "CAD";
			paymentApproval.AV_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;
			approval.AV_Amount = 200m;
			approval.AV_PayExRate = 0.5m;
			approval.InitializeForPaymentBatch(() => false);
			Factory.Save();

			paymentApproval.AV_AB_FundingBankAccount = TestObjectCreator.AUDBankAccount.PK;
			AssertEquals("USD", provider.GetOriginalFundingCurrency());

			provider = new PaymentApprovalFundingInfoProvider(null);
			AssertEquals(Env.CurrentCompany.LocalCurrency.Code, provider.GetOriginalFundingCurrency());
		}

		APPaymentApprovalWithAuthorisation CreatePaymentApproval()
		{
			var approval = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			approval.AV_RX_NKPaymentCurrency = "CAD";
			approval.AV_Amount = 200m;
			approval.AV_PayExRate = 0.5m;
			approval.InitializeForPaymentBatch(() => false);
			Factory.Save();
			return approval;
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
