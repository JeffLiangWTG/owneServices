using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE428;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

class IDD428ItemWrapperTest : Enterprise.DocumentWrappers.Testing.DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		return IDD428ItemWrapper.New(1, goodsShipmentItem, Factory);
	}

	public void TestItemAmountInvoiced()
	{
		goodsShipmentItem.Commodity = new MCommodityType05FR();
		goodsShipmentItem.Commodity.InvoiceLine = new MInvoiceLineType();
		goodsShipmentItem.Commodity.InvoiceLine.ItemAmountInvoiced = 123;
		var wrapper = new IDD428ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("ItemAmountInvoiced", "123", wrapper.ItemAmountInvoiced);
	}

	public void TestStatisticalValue()
	{
		goodsShipmentItem.StatisticalValue = 1000;
		var wrapper = new IDD428ItemWrapper(1, goodsShipmentItem, Factory);
		AssertEquals("StatisticalValue", "1000", wrapper.StatisticalValue);
	}

	public void TestLiquidation()
	{
		goodsShipmentItem.DeclarationGoodsItemNumber = "123";
		var goodsShipmentItemFromDetailedTaxation = new GoodsShipmentItemType
		{
			DeclarationGoodsItemNumber = "123",
			DutiesAndTaxes = new List<DutiesAndTaxesType>()
			{
				new DutiesAndTaxesType()
				{
					TaxType = "ABC"
				}
			}
		};
		var wrapper = new IDD428ItemWrapper(1, goodsShipmentItem, goodsShipmentItemFromDetailedTaxation, new List<DutiesAndTaxesSummariesType>(), Factory);
		AssertEquals("Liquidation", "ABC", (wrapper.Liquidation.DutiesAndTax.FirstOrDefault() as IDD428DutiesAndTaxWrapper).TaxType);
	}

	protected override void SetUp()
	{
		base.SetUp();

		goodsShipmentItem = new MGoodsShipmentItemType05FR();
		goodsShipmentItem.Commodity = new MCommodityType05FR();
		goodsShipmentItem.Commodity.CalculationOfTaxes = new MCalculationOfTaxesType01FR();
	}

	MGoodsShipmentItemType05FR goodsShipmentItem;
}
