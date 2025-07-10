namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

sealed class F17HeaderProviderTest : SendAndAmendHeader17ProviderBaseTest<F17HeaderProvider>
{
	public override void TestLRN()
	{
		AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
	}
}

