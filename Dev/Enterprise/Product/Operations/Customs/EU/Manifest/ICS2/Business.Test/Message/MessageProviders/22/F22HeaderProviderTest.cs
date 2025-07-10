namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class F22HeaderProviderTest : SendAndAmendHeader22ProviderBaseTest<F22HeaderProvider>
	{
		public override void TestLRN()
		{
			AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}
	}
}
