namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class F24HeaderProviderTest : SendAndAmendHeader24ProviderBaseTest<F24HeaderProvider>
	{
		public override void TestLRN()
		{
			AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}
	}
}
