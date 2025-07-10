using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class GPSProviderTest : DataProviderTestCase<GPSProvider>
	{
		public void TestLatitude()
		{
			AssertEquals("Latitude", "10.5", Provider.Latitude);
		}

		public void TestLongitude()
		{
			AssertEquals("Latitude", "20.5", Provider.Longitude);
		}

		protected override GPSProvider GetProvider()
		{
			return GPSProvider.New("10.5", "20.5");
		}
	}
}
