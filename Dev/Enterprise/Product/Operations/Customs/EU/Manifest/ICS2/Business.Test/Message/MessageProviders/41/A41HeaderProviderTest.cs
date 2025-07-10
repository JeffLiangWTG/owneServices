namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class A41HeaderProviderTest : SendAndAmendHeader41ProviderBaseTest<A41HeaderProvider>
	{
		public void TestAmendedItems()
		{
			IAmendedItemsProvider provider = Provider;

			AssertNull(provider.AmendedItems);
		}
	}
}
