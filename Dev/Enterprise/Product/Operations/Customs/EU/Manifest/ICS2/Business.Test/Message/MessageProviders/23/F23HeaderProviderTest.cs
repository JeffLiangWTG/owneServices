namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class F23HeaderProviderTest : SendAndAmendHeader23ProviderBaseTest<F23HeaderProvider>
	{
		public override void TestLRN()
		{
			AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}
	}
}
