using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class GpsProviderTest : DataProviderTestCase<GpsProvider>
	{
		public void TestIGps()
		{
			Assert("Should implement IGps", Provider is IGps);
		}

		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("Has value, Empty", GpsProvider.New("52.3142599", string.Empty));
				AssertNull("Empty, Has value", GpsProvider.New(string.Empty, "-66.9437914"));
				AssertNotNull("Has value, Has value", GpsProvider.New("52.3142599", "-66.9437914"));
			});
		}

		public void TestLatitude()
		{
			AssertEquals("52.3142599", Provider.Latitude);
		}

		public void TestLongitude()
		{
			AssertEquals("-66.9437914", Provider.Longitude);
		}

		protected override GpsProvider GetProvider()
		{
			return GpsProvider.New("52.3142599", "-66.9437914");
		}
	}
}
