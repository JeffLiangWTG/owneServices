namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class F44HeaderProviderTest : SendAndAmendHeader44ProviderBaseTest<F44HeaderProvider>
	{
		public override void TestLRN()
		{
			AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}
	}
}
