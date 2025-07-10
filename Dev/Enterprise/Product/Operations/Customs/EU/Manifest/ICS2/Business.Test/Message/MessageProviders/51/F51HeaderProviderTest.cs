namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class F51HeaderProviderTest : SendAndAmendHeader51ProviderBaseTest<F51HeaderProvider>
	{
		public override void TestLRN()
		{
			AssertEquals(ICS2OutboundEDIMessage.LRNPlaceHolder, Provider.LRN);
		}
	}
}
