using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification>
	{
		public void TestNewOrNull()
		{
			AssertNull("When invoiceLine is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationWrapper.NewOrNull(null));
			AssertNotNull("When invoiceLine is not null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationWrapper.NewOrNull(invoiceLine));
		}

		public void TestDmExtensions() => AssertNull(Provider.DmExtensions);

		public void TestID()
		{
			AssertNull(Provider.ID);

			invoiceLine.JI_Tariff = "99999999";
			var wrapper = DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationWrapper.NewOrNull(invoiceLine);
			AssertEquals(invoiceLine.JI_Tariff, wrapper.ID.Value);
		}

		public void TestIdentificationTypeCode()
		{
			AssertEquals("HS", Provider.IdentificationTypeCode.Value);
		}

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification GetProvider()
			=> DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationWrapper.NewOrNull(invoiceLine);

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = Factory.New<JobComInvoiceLine>();
		}

		JobComInvoiceLine invoiceLine;
	}
}
