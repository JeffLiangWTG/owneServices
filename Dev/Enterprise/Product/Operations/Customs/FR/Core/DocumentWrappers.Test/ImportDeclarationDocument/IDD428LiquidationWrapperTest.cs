using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE428;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

sealed class IDD428LiquidationWrapperTest : Enterprise.DocumentWrappers.Testing.DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		return IDD428LiquidationWrapper.New(goodsShipmentItem, dutiesAndTaxesSummaries, Factory);
	}

	public void TestLiquidation()
	{
		goodsShipmentItem.DutiesAndTaxes = new List<DutiesAndTaxesType>
		{
			new DutiesAndTaxesType { TaxType = "ABC" }
		};

		var wrapper = IDD428LiquidationWrapper.New(goodsShipmentItem, dutiesAndTaxesSummaries, Factory);
		AssertEquals("DutiesAndTax", "ABC", (wrapper.DutiesAndTax.FirstOrDefault() as IDD428DutiesAndTaxWrapper).TaxType);
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
