using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity>
	{
		public void TestNewOrNull()
		{
			AssertNull("When invoiceLine is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityWrapper.NewOrNull(null));
			AssertNotNull("When invoiceLine is null", DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityWrapper.NewOrNull(invoiceLine));
		}

		public void TestClassification()
		{
			AssertEquals(1, Provider.Classification.Count);
			AssertType<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassificationWrapper>(Provider.Classification.Single());
		}

		public void TestDmExtensions()
		{
			AssertNotNull(Provider.DmExtensions);
		}

		public void TestDutyTaxFee()
		{
			AssertNull(Provider.DutyTaxFee);
		}

		public void TestGovernmentProcedure()
		{
			AssertEquals(1, Provider.GovernmentProcedure.Count);
			AssertType<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGovernmentProcedureWrapper>(Provider.GovernmentProcedure.Single());
		}

		protected override IDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity GetProvider()
			=> DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityWrapper.NewOrNull(invoiceLine);

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = Factory.New<JobComInvoiceLine>();
		}

		JobComInvoiceLine invoiceLine;
	}
}
