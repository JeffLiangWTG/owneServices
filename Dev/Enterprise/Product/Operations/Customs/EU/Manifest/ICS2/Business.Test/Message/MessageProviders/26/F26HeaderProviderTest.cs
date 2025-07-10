namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class F26HeaderProviderTest : SendAndAmendHeader26ProviderBaseTest<F26HeaderProvider>
	{
		public override void TestLRN()
		{
			AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}
	}
}
