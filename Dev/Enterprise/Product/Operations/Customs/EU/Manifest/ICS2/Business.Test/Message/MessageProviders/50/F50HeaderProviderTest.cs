namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

sealed class F50HeaderProviderTest : SendAndAmendHeader50ProviderBaseTest<F50HeaderProvider>
{
	public override void TestLRN()
	{
		AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
	}
}