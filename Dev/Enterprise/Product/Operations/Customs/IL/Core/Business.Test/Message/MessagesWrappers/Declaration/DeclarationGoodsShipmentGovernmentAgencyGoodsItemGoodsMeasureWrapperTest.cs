using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure>
	{
		public void TestNewOrNull()
		{
			CombineAssertions("NewOrNull", () =>
			{
				AssertNull(DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureWrapper.NewOrNull(0, "", ""));
				AssertNull(DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureWrapper.NewOrNull(2, "", ""));
				AssertNull(DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureWrapper.NewOrNull(2, "O5", ""));
				AssertNotNull("All params must set", DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureWrapper.NewOrNull(2, "O5", "1"));
			});
		}

		public void TestDmExtensions()
		{
			AssertEquals("1", Provider.DmExtensions.MeasureQualifier.Value);
		}

		public void TestTariffQuantity()
		{
			var tariffQuantity = Provider.TariffQuantity;
			AssertEquals(1.012m, tariffQuantity.Value);
			AssertEquals(MeasurementUnitCommonCodeContentType.Item05, tariffQuantity.UnitCode);

			var provider = DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureWrapper.NewOrNull(1.0114m, "05", "1");
			AssertEquals(1.011m, provider.TariffQuantity.Value);
		}

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure GetProvider() => DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasureWrapper.NewOrNull(1.0117m, "05", "1");
	}
}
