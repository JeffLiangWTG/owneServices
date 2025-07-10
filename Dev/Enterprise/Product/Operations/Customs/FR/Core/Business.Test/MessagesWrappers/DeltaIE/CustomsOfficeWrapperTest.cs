using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class CustomsOfficeWrapperTest : Customs.Business.Testing.DataProviderTestCase<CustomsOfficeWrapper>
	{
		protected override CustomsOfficeWrapper GetProvider()
		{
			var locationGoods = Factory.NewWithValidTestData<CusGoodsLocation>();
			locationGoods.CGL_CustomsOffice = "CUSOFFICE";
			return CustomsOfficeWrapper.New(locationGoods);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should be equal to CGL_CustomsOffice.", "CUSOFFICE", Provider.ReferenceNumber);
		}
	}
}
