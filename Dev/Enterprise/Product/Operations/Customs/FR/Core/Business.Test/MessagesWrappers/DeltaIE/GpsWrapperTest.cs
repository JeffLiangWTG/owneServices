using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class GpsWrapperTest : Customs.Business.Testing.DataProviderTestCase<GpsWrapper>
	{
		protected override GpsWrapper GetProvider()
		{
			var locationGoodsAddress = Factory.NewWithValidTestData<CusGoodsLocationAddress>();
			locationGoodsAddress.E2_Latitude = 45m;
			locationGoodsAddress.E2_Longitude = 12m;

			return GpsWrapper.New(locationGoodsAddress);
		}

		public void TestLatitude()
		{
			AssertEquals("Latitude should be equal to E2_Latitude.", "45", Provider.Latitude);
		}

		public void TestLongitude()
		{
			AssertEquals("Longitude should be equal to E2_Longitude.", "12", Provider.Longitude);
		}
	}
}
