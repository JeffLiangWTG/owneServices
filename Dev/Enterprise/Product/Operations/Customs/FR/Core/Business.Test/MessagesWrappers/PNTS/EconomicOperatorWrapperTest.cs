using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class EconomicOperatorWrapperTest : Customs.Business.Testing.DataProviderTestCase<EconomicOperatorWrapper>
	{
		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should equal E2_GovRegNum.", "GRN001", Provider.IdentificationNumber);
		}

		protected override EconomicOperatorWrapper GetProvider()
		{
			var locationGoodsAddress = Factory.NewWithValidTestData<CusGoodsLocationAddress>();
			locationGoodsAddress.E2_GovRegNum = "GRN001";

			return EconomicOperatorWrapper.New(locationGoodsAddress);
		}
	}
}
