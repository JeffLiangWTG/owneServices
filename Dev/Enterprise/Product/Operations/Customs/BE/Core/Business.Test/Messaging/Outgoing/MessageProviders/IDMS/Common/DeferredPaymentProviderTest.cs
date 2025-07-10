namespace Enterprise.Customs.BE.Business.Testing;

sealed class DeferredPaymentProviderTest : Customs.Business.Testing.DataProviderTestCase<DeferredPaymentProvider>
{
	public void TestSequenceNumber()
	{
		AssertEquals(3, provider.SequenceNumber);
	}

	public void TestDeferredPayment()
	{
		AssertEquals("TST", provider.DeferredPayment);
	}

	public void TestCCQualifier()
	{
		AssertEquals("NVT", provider.CCQualifier);
	}

	protected override DeferredPaymentProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		provider = new DeferredPaymentProvider(3, "TST");
	}
	DeferredPaymentProvider provider;
}
