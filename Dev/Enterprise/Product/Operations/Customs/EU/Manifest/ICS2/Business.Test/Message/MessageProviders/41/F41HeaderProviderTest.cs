namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class F41HeaderProviderTest : SendAndAmendHeader41ProviderBaseTest<F41HeaderProvider>
	{
		public override void TestLRN()
		{
			AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}
	}
}
