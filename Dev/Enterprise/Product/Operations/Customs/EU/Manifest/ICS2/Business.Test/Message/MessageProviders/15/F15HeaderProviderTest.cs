namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class F15HeaderProviderTest : SendAndAmendHeader15ProviderBaseTest<F15HeaderProvider>
	{
		public override void TestLRN()
		{
			AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}
	}
}
