namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class F10HeaderProviderTest : SendAndAmendHeader10ProviderBaseTest<F10HeaderProvider>
	{
		public override void TestLRN()
		{
			AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}
	}
}
