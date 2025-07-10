namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class GNSSProviderTest : Customs.Business.Testing.DataProviderTestCase<GNSSProvider>
	{
		public void TestLatitude()
		{
			AssertEquals("53.3302", Provider.Latitude);
		}

		public void TestLongitude()
		{
			AssertEquals("-7.7543", Provider.Longitude);
		}

		protected override GNSSProvider GetProvider() => new GNSSProvider(new CargoWise.Types.ZGeography("-7.7543, 53.3302"));
	}
}
