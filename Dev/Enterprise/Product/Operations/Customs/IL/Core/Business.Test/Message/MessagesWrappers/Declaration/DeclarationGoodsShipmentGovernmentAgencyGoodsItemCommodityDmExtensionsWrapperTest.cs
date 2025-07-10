using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDmExtensionsWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDmExt>
	{
		public void TestNewOrNull()
		{
			AssertNull("When invoiceLine is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDmExtensionsWrapper.NewOrNull(null));
			AssertNotNull("When invoiceLine is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDmExtensionsWrapper.NewOrNull(invoiceLine));
		}

		public void TestDutyRegimeCode()
		{
			AssertEquals("400", Provider.DutyRegimeCode.Value);
		}

		public void TestProductIdentification()
		{
			AssertNull(Provider.ProductIdentification);
		}

		public void TestProductName()
		{
			AssertNull(Provider.ProductName);
		}

		public void TestSerialNumbers()
		{
			AssertNull(Provider.SerialNumbers);
		}

		public void TestTradeLevyAndExampt()
		{
			AssertNull(Provider.TradeLevyAndExampt);
		}

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDmExt GetProvider()
			=> DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDmExtensionsWrapper.NewOrNull(invoiceLine);

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_PrimaryPreference = "400";
		}

		JobComInvoiceLine invoiceLine;
	}
}
