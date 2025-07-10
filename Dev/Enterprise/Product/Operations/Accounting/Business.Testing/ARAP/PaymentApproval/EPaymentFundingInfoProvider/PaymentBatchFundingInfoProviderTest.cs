using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public sealed class PaymentBatchFundingInfoProviderTest : TestCaseWithFactory
	{
		public void TestGetFundingBankAccount()
		{
			var paymentBatch = Factory.New<APPaymentBatchPoster>();
			paymentBatch.APB_AB_FundingBankAccount = TestObjectCreator.AUDBankAccount.PK;
			paymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;

			var provider = new PaymentBatchFundingInfoProvider(paymentBatch);
			AssertEquals(TestObjectCreator.AUDBankAccount.PK, provider.GetFundingBankAccount());

			paymentBatch.APB_AB_FundingBankAccount = ZGuid.Empty;
			provider = new PaymentBatchFundingInfoProvider(paymentBatch);
			AssertEquals(ZGuid.Empty, provider.GetFundingBankAccount());

			provider = new PaymentBatchFundingInfoProvider(null);
			AssertEquals(ZGuid.Empty, provider.GetFundingBankAccount());
		}

		public void TestGetFundingCurrency()
		{
			var paymentBatch = Factory.New<APPaymentBatchPoster>();
			paymentBatch.APB_AB_FundingBankAccount = TestObjectCreator.AUDBankAccount.PK;
			paymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;

			var provider = new PaymentBatchFundingInfoProvider(paymentBatch);
			AssertEquals("AUD", provider.GetFundingCurrency());

			paymentBatch.APB_AB_FundingBankAccount = ZGuid.Empty;
			provider = new PaymentBatchFundingInfoProvider(paymentBatch);
			AssertEquals(Env.CurrentCompany.LocalCurrency.Code, provider.GetFundingCurrency());

			provider = new PaymentBatchFundingInfoProvider(null);
			AssertEquals(Env.CurrentCompany.LocalCurrency.Code, provider.GetFundingCurrency());
		}

		public void TestGetOriginalFundingCurrency()
		{
			AssertNotEquals("PreCondition", "USD", Env.CurrentCompany.LocalCurrency.Code);
			var paymentBatch = Factory.NewWithValidTestData<APPaymentBatchPoster>();
			Factory.Save();

			paymentBatch.APB_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;
			var provider = new PaymentBatchFundingInfoProvider(paymentBatch);
			AssertEquals(Env.CurrentCompany.LocalCurrency.Code, provider.GetOriginalFundingCurrency());

			paymentBatch = Factory.NewWithValidTestData<APPaymentBatchPoster>();
			paymentBatch.APB_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;
			Factory.Save();

			paymentBatch.APB_AB_FundingBankAccount = TestObjectCreator.AUDBankAccount.PK;
			provider = new PaymentBatchFundingInfoProvider(paymentBatch);
			AssertEquals("USD", provider.GetOriginalFundingCurrency());

			provider = new PaymentBatchFundingInfoProvider(null);
			AssertEquals(Env.CurrentCompany.LocalCurrency.Code, provider.GetOriginalFundingCurrency());
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
