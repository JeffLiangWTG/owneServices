namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class F25HeaderProviderTest : SendAndAmendHeader25ProviderBaseTest<F25HeaderProvider>
	{
		public override void TestLRN()
		{
			AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}
	}
}
