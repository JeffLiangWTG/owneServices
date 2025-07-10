namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

sealed class F16HeaderProviderTest : SendAndAmendHeader16ProviderBaseTest<F16HeaderProvider>
{
	public override void TestLRN()
	{
		AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
	}
}
