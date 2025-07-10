using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE426;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

sealed class IDD426LiquidationWrapperTest : Enterprise.DocumentWrappers.Testing.DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		return IDD426LiquidationWrapper.New(goodsShipmentItem, dutiesAndTaxesSummaries, Factory);
	}

	public void TestLiquidation()
	{
		goodsShipmentItem.DutiesAndTaxes = new List<DutiesAndTaxesType>
		{
			new DutiesAndTaxesType { TaxType = "ABC" }
		};

		var wrapper = IDD426LiquidationWrapper.New(goodsShipmentItem, dutiesAndTaxesSummaries, Factory);
		AssertEquals("DutiesAndTax", "ABC", (wrapper.DutiesAndTax.FirstOrDefault() as IDD426DutiesAndTaxWrapper).TaxType);
	}

	protected override void SetUp()
	{
		base.SetUp();

		goodsShipmentItem = new GoodsShipmentItemType { };
		dutiesAndTaxesSummaries = new List<DutiesAndTaxesSummariesType>();
	}

	GoodsShipmentItemType goodsShipmentItem;
	ICollection<DutiesAndTaxesSummariesType> dutiesAndTaxesSummaries;
}
