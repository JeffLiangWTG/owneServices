using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentConsignmentLoadingLocationWrapperTest : DataProviderTestCase<DeclarationGoodsShipmentConsignmentLoadingLocationWrapper>
	{
		public void TestID()
		{
			AssertNotNull("ID", Provider.ID);
			AssertEquals("ID should be set as expected", "LEON", Provider.ID.Value);
		}

		public void TestNewOrNull()
		{
			AssertNull("Provider", DeclarationGoodsShipmentConsignmentLoadingLocationWrapper.NewOrNull(null));
			AssertNotNull("Provider", DeclarationGoodsShipmentConsignmentLoadingLocationWrapper.NewOrNull(Factory.New<JobDeclaration>()));
		}

		protected override DeclarationGoodsShipmentConsignmentLoadingLocationWrapper GetProvider()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_RL_NKPortOfLoading = "LEON";

			return DeclarationGoodsShipmentConsignmentLoadingLocationWrapper.NewOrNull(jobDeclaration);
		}
	}
}
