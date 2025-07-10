using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing;

class GoodsItemRateCalcDataTest : TestCaseWithFactory
{
	public void TestUnitOfMeasureValueListSecondUnitQty()
	{
		var goodsItem = SetUpGoodsItem();
		AssertUnitOfMeasureValueListSecondUnitQty("Good Item", goodsItem);
		void AssertUnitOfMeasureValueListSecondUnitQty(ZString assertionText, TemporaryStoragePackedItem goodsItem)
		{
			CombineAssertions(assertionText, () =>
			{
				var goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
				goodsItem.API_CustomsUQ2 = "NAR";
				goodsItem.API_CustomsQty2 = 15.00m;
				AssertEquals("Second Unit Qty Measure value NAR 15.00", 15.00m, goodsItemRateData.UnitOfMeasureValueList[goodsItem.API_CustomsUQ2]);

				goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
				goodsItem.API_CustomsUQ2 = string.Empty;
				goodsItem.API_CustomsQty2 = 5.00m;
				AssertEquals("Second Unit Qty Measure not defined", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItem.API_CustomsUQ));

				goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
				goodsItem.API_CustomsUQ2 = "NAR";
				goodsItem.API_CustomsQty2 = 0.00m;
				AssertEquals("Second Unit Qty Measure value NAR 0.00", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItem.API_CustomsUQ));
			});
		}
	}

	public void TestUnitOfMeasureValueListCustomsUnitQty()
	{
		var goodsItem = SetUpGoodsItem();
		AssertUnitOfMeasureValueListCustomsUnitQty("Good Item", goodsItem);
		void AssertUnitOfMeasureValueListCustomsUnitQty(ZString assertionText, TemporaryStoragePackedItem goodsItem)
		{
			CombineAssertions(assertionText, () =>
			{
				var goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
				goodsItem.API_CustomsUQ = "KGM";
				goodsItem.API_CustomsQty = 15.00m;
				AssertEquals("Customs Unit Qty Measure value KGM 15.00", 15.00m, goodsItemRateData.UnitOfMeasureValueList[goodsItem.API_CustomsUQ]);

				goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
				goodsItem.API_CustomsUQ = string.Empty;
				goodsItem.API_CustomsQty = 5.00m;
				AssertEquals("Customs Unit Qty Measure not defined", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItem.API_CustomsUQ));

				goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
				goodsItem.API_CustomsUQ = "KGM";
				goodsItem.API_CustomsQty = 0.00m;
				AssertEquals("Customs Unit Qty Measure value KGM 0.00", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItem.API_CustomsUQ));
			});
		}
	}

	public void TestUnitOfMeasureValueListThirdUnitQty()
	{
		var goodsItem = SetUpGoodsItem();
		AssertUnitOfMeasureValueListCustomsUnitQty("Good Item", goodsItem);
		void AssertUnitOfMeasureValueListCustomsUnitQty(ZString assertionText, TemporaryStoragePackedItem goodsItem)
		{
			CombineAssertions(assertionText, () =>
			{
				var goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
				goodsItem.API_CustomsUQ3 = "KGM";
				goodsItem.API_CustomsQty3 = 15.00m;
				AssertEquals("Third Unit Qty Measure value KGM 15.00", 15.00m, goodsItemRateData.UnitOfMeasureValueList[goodsItem.API_CustomsUQ3]);

				goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
				goodsItem.API_CustomsUQ3 = string.Empty;
				goodsItem.API_CustomsQty3 = 5.00m;
				AssertEquals("Third Unit Qty Measure not defined", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItem.API_CustomsUQ3));

				goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
				goodsItem.API_CustomsUQ3 = "KGM";
				goodsItem.API_CustomsQty3 = 0.00m;
				AssertEquals("Third Unit Qty Measure value KGM 0.00", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItem.API_CustomsUQ3));
			});
		}
	}

	TemporaryStoragePackedItem SetUpGoodsItem()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_RN_NKCountry = "ES";
		var bill = header.Bills.AddNew();
		var goodsItem = bill.PackedItems.AddNew();

		return goodsItem;
	}
}
