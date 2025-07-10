namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class GnssWrapperTest : Customs.Business.Testing.DataProviderTestCase<GnssWrapper>
	{
		public void TestLatitude()
		{
			AssertEquals("Latitude should equal E2_Latitude.", "45", Provider.Latitude);

			var locationGoodsAddress = Factory.NewWithValidTestData<EU.NCTS.Business.CusGoodsLocationAddress>();
			locationGoodsAddress.E2_Latitude = 0m;
			AssertEquals("Latitude should be null when E2_Latitude is 0.", null, GnssWrapper.New(locationGoodsAddress).Longitude);
		}

		public void TestLongitude()
		{
			AssertEquals("Longitude should equal E2_Longitude.", "12", Provider.Longitude);

			var locationGoodsAddress = Factory.NewWithValidTestData<EU.NCTS.Business.CusGoodsLocationAddress>();
			locationGoodsAddress.E2_Longitude = 0m;
			AssertEquals("Longitude should be null when E2_Longitude is 0.", null, GnssWrapper.New(locationGoodsAddress).Longitude);
		}

		protected override GnssWrapper GetProvider()
		{
			var locationGoodsAddress = Factory.NewWithValidTestData<EU.NCTS.Business.CusGoodsLocationAddress>();
			locationGoodsAddress.E2_Latitude = 45m;
			locationGoodsAddress.E2_Longitude = 12m;

			return GnssWrapper.New(locationGoodsAddress);
		}
	}
}
