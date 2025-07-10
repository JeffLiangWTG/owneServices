namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class F13HeaderProviderTest : SendAndAmendHeader13ProviderBaseTest<F13HeaderProvider>
	{
		public override void TestLRN()
		{
			AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}
	}
}
