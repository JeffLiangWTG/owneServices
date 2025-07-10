namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class DeferredPaymentProviderTest : Customs.Business.Testing.DataProviderTestCase<DeferredPaymentProvider>
	{
		public void TestDeferredPayment()
		{
			AssertEquals("DeferredPayment", "12345", deferredPaymentProvider.DeferredPayment);
		}

		public void TestCcQualifier()
		{
			AssertNull("CcQualifier", deferredPaymentProvider.CcQualifier);
		}

		protected override DeferredPaymentProvider GetProvider()
		{
			return deferredPaymentProvider;
		}

		protected override void SetUp()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_PaymentAccountNumber = "12345";

			deferredPaymentProvider = new DeferredPaymentProvider(manifestHeader);
		}

		DeferredPaymentProvider deferredPaymentProvider;
	}
}
