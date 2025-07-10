using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedureWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedure>
	{
		public void TestNewOrNull()
		{
			AssertNull("When invoiceLine is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedureWrapper.NewOrNull(null));
			AssertNotNull("When invoiceLine is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedureWrapper.NewOrNull(invoiceLine));
		}

		public void TestCurrentCode()
		{
			AssertEquals("9806", Provider.CurrentCode.Value);
		}

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedure GetProvider()
			=> DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedureWrapper.NewOrNull(invoiceLine);

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_Procedure = "9806";
		}

		JobComInvoiceLine invoiceLine;
	}
}
