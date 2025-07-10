using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusCNClassification))]
	class CusCNClassificationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCompartibleMaxLength()
		{
			// This test ensures that transformation will not fall. Any changes in CN AddInfo Schema must be reflected in CusCNClassification and vice versa.
			AssertEquals(AutoCNJobComInvoiceLine.Schema.JI_CIQTariffMaxLength, CusCNClassificationSchema.CNC_CIQTariff.MaxLength);
			AssertEquals(AutoCNJobComInvoiceLine.Schema.JI_DestinationDistrictMaxLength, CusCNClassificationSchema.CNC_DestinationDistrict.MaxLength);
			AssertEquals(AutoCNJobComInvoiceLine.Schema.JI_DestinationRegionMaxLength, CusCNClassificationSchema.CNC_DestinationRegion.MaxLength);
			AssertEquals(AutoCNJobComInvoiceLine.Schema.JI_CIQEndUseMaxLength, CusCNClassificationSchema.CNC_EndUse.MaxLength);
			AssertEquals(AutoCNJobComInvoiceLine.Schema.JI_NameOfGoodsMaxLength, CusCNClassificationSchema.CNC_NameOfGoods.MaxLength);
			AssertEquals(AutoCNJobComInvoiceLine.Schema.JI_OriginDistrictMaxLength, CusCNClassificationSchema.CNC_OriginDistrict.MaxLength);
			AssertEquals(AutoCNJobComInvoiceLine.Schema.JI_OriginRegionMaxLength, CusCNClassificationSchema.CNC_OriginRegion.MaxLength);
			AssertEquals(AutoCNJobComInvoiceLine.Schema.JI_CIQOriginStateMaxLength, CusCNClassificationSchema.CNC_OriginState.MaxLength);
			AssertEquals(AutoCNJobComInvoiceLine.Schema.JI_TradeUnitQtyMaxLength, CusCNClassificationSchema.CNC_TradeUnitQty.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			return Factory.LoadTop1<CusCNClassification>(new ZQuery(CusCNClassificationSchema.CNC_CI, pivot.PK));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();
	}
}
