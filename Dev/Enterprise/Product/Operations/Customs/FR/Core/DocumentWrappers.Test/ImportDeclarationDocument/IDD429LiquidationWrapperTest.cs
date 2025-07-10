using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE429;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

sealed class IDD429LiquidationWrapperTest : Enterprise.DocumentWrappers.Testing.DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		return IDD429LiquidationWrapper.New(goodsShipmentItem, dutiesAndTaxesSummaries, Factory);
	}

	public void TestLiquidation()
	{
		goodsShipmentItem.DutiesAndTaxes = new List<DutiesAndTaxesType>
		{
			new DutiesAndTaxesType { TaxType = "ABC" }
		};

		var wrapper = IDD429LiquidationWrapper.New(goodsShipmentItem, dutiesAndTaxesSummaries, Factory);
		AssertEquals("DutiesAndTax", "ABC", (wrapper.DutiesAndTax.FirstOrDefault() as IDD429DutiesAndTaxWrapper).TaxType);
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
