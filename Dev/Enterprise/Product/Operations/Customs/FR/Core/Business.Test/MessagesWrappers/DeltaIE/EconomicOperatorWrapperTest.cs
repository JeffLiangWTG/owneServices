using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class EconomicOperatorWrapperTest : Customs.Business.Testing.DataProviderTestCase<EconomicOperatorWrapper>
	{
		protected override EconomicOperatorWrapper GetProvider()
		{
			var locationGoodsAddress = Factory.NewWithValidTestData<CusGoodsLocationAddress>();
			locationGoodsAddress.E2_GovRegNum = "gov";

			return EconomicOperatorWrapper.New(locationGoodsAddress);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should be equal to E2_GovRegNum.", "gov", Provider.IdentificationNumber);
		}
	}
}
