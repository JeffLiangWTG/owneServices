namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class F14HeaderProviderTest : SendAndAmendHeader14ProviderBaseTest<F14HeaderProvider>
	{
		public override void TestLRN()
		{
			AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}
	}
}
