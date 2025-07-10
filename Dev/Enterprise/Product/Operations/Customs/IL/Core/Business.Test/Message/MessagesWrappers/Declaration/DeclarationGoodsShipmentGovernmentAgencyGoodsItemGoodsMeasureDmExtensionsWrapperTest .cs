using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureDmExtensionsWrapperTest
		: Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureDmExtensions>
	{
		public void TestMeasureQualifier()
		{
			AssertEquals("1", Provider.MeasureQualifier.Value);
		}

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureDmExtensions GetProvider()
			=> new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureDmExtensionsWrapper("1");
	}
}
