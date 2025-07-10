using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentConsignmentDMExtensionsRegisteredFacilityStorageSiteWrapperTest : DataProviderTestCase<DeclarationGoodsShipmentConsignmentDmExtensionsRegisteredFacilityStorageSiteWrapper>
	{
		public void TestFacilityType()
		{
			AssertNotNull("FacilityType", Provider.FacilityType);
			AssertEquals("FacilityType should be set as expected", "004", Provider.FacilityType.Value);
		}

		public void TestID()
		{
			AssertNotNull("ID", Provider.ID);
			AssertEquals("ID should be set as expected", "XYZ", Provider.ID.Value);
		}

		public void TestNewOrNull()
		{
			AssertNull("Provider", DeclarationGoodsShipmentConsignmentDmExtensionsRegisteredFacilityStorageSiteWrapper.NewOrNull(null));
			AssertNotNull("Provider", DeclarationGoodsShipmentConsignmentDmExtensionsRegisteredFacilityStorageSiteWrapper.NewOrNull(Factory.New<JobDeclaration>()));
		}

		protected override DeclarationGoodsShipmentConsignmentDmExtensionsRegisteredFacilityStorageSiteWrapper GetProvider()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_LocationOfGoods = "XYZ";

			return DeclarationGoodsShipmentConsignmentDmExtensionsRegisteredFacilityStorageSiteWrapper.NewOrNull(jobDeclaration);
		}
	}
}
