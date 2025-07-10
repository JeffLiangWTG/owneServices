namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ContentInformationProviderTest : Customs.Business.Testing.DataProviderTestCase<ContentInformationProvider>
	{
		public void TestContentType()
		{
			AssertEquals("C", dataProvider.ContentType);
		}

		public void TestDegreePercentage()
		{
			AssertEquals(45.10m, dataProvider.DegreePercentage);
		}

		protected override void SetUp()
		{
			var content = Factory.New<ContentInformationType>();
			content.CY_Code = "C";
			content.CY_Data = "45.10";
			dataProvider = new ContentInformationProvider(content);
		}
		ContentInformationProvider dataProvider;

		protected override ContentInformationProvider GetProvider() => dataProvider;
	}
}
