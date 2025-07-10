using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class IM415HeaderProviderTest : IM413_414_415MessageProviderTest<IM415HeaderProvider>
	{
		public void TestImportOperation()
		{
			CombineAssertions("ImportOperation", () => {
				AssertEquals("LRN", AISOutboundEDIMessage.LRNPlaceHolder, Provider.ImportOperation.LRN);
				AssertEquals("AdditionalDeclarationType", "A", Provider.ImportOperation.AdditionalDeclarationType);
			});
		}

		protected override IM415HeaderProvider GetProvider()
		{
			return new IM415HeaderProvider(messageSendingObject);
		}
	}
}
