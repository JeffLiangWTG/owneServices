namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class F43HeaderProviderTest : SendAndAmendHeader43ProviderBaseTest<F43HeaderProvider>
	{
		public override void TestLRN()
		{
			AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}
	}
}
