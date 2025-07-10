namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class F40HeaderProviderTest : SendAndAmendHeader40ProviderBaseTest<F40HeaderProvider>
	{
		public override void TestLRN()
		{
			AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}
	}
}
