using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginWrapperTest
		: Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin>
	{
		public void TestNewOrNull()
		{
			AssertNull("When invoiceLine is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginWrapper.NewOrNull(null));
			AssertNotNull("When invoiceLine is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginWrapper.NewOrNull(invoiceLine));
		}

		public void TestCountryCode()
		{
			AssertEquals("IL", Provider.CountryCode.Value);
		}

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin GetProvider()
			=> DeclarationGoodsShipmentGovernmentAgencyGoodsItemOriginWrapper.NewOrNull(invoiceLine);

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CountryOfOrigin = "IL";
		}

		JobComInvoiceLine invoiceLine;
	}
}
