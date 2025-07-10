using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public sealed class EPaymentFundingInfoProviderFactoryTest : TestCaseWithFactory
	{
		public void TestGetProvider()
		{
			var paymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			paymentApproval.AV_RX_NKPaymentCurrency = "CAD";
			paymentApproval.AV_Amount = 200m;
			paymentApproval.AV_PayExRate = 0.5m;
			Factory.Save();

			var provider = EPaymentFundingInfoProviderFactory.CreateProvider(paymentApproval);
			AssertType<PaymentApprovalFundingInfoProvider>(provider);

			var paymentBatch = Factory.New<APPaymentBatchPoster>();
			paymentBatch.APB_AB_FundingBankAccount = TestObjectCreator.AUDBankAccount.PK;
			paymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			paymentApproval.AV_APB_PaymentBatch = paymentBatch.PK;
			paymentApproval.InitializeForPaymentBatch(() => false);
			Factory.Save();

			provider = EPaymentFundingInfoProviderFactory.CreateProvider(paymentApproval);
			AssertType<PaymentBatchFundingInfoProvider>(provider);

			paymentApproval.AV_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;
			provider = EPaymentFundingInfoProviderFactory.CreateProvider(paymentApproval);
			AssertType<PaymentApprovalFundingInfoProvider>(provider);
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
