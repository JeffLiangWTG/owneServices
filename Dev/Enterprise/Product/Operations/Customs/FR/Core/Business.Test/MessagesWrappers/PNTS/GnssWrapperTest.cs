using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class GnssWrapperTest : Customs.Business.Testing.DataProviderTestCase<GnssWrapper>
	{
		public void TestLatitude()
		{
			AssertEquals("Latitude should equal E2_Latitude.", "45", Provider.Latitude);
		}

		public void TestLongitude()
		{
			AssertEquals("Longitude should equal E2_Longitude.", "12", Provider.Longitude);
		}

		protected override GnssWrapper GetProvider()
		{
			var locationGoodsAddress = Factory.NewWithValidTestData<CusGoodsLocationAddress>();
			locationGoodsAddress.E2_Latitude = 45m;
			locationGoodsAddress.E2_Longitude = 12m;

			return GnssWrapper.New(locationGoodsAddress);
		}
	}
}
