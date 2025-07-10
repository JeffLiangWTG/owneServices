using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NotificationUnloadingNCTS5CommodityWrapperTest : WrapperHelperTest<NotificationUnloadingNCTS5CommodityWrapper>
	{
		public void TestGoodsMeasure()
		{
			var goodsMeasure = wrapper.GoodsMeasure;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled GoodsMeasure", goodsMeasure);
				AssertSame("Cached GoodsMeasure", wrapper.GoodsMeasure, goodsMeasure);

				goodsItem.BY_UnloadedState = "DIF";
				goodsItem.BY_GrossWeight = 5m;
				goodsItem.UnloadedGoodsItem.BY_GrossWeight = 5m;
				wrapper = GetWrapper(goodsItem);
				goodsMeasure = wrapper.GoodsMeasure;
				AssertNull("Expected null GoodsMeasure if not differences", goodsMeasure);

				goodsItem.UnloadedGoodsItem.BY_GrossWeight = 2m;
				wrapper = GetWrapper(goodsItem);
				goodsMeasure = wrapper.GoodsMeasure;
				AssertNotNull("Expected filled GoodsMeasure if has differences", goodsMeasure);
			});
		}

		public void TestCusCode()
		{
			CombineAssertions(() =>
			{
				goodsItem.BY_CusC4Number = "0010111-1";
				AssertEquals("Expected filled with declared value CusCode when unloaded state is NEW", "0010111-1", wrapper.CusCode);

				goodsItem.BY_UnloadedState = "DIF";
				goodsItem.UnloadedGoodsItem.BY_CusC4Number = "0010111-1";
				wrapper = GetWrapper(goodsItem);
				AssertEquals("Expected empty CusCode when unloaded state is DIF and unloaded value is not the same as declared value", ZString.Empty, wrapper.CusCode);

				goodsItem.UnloadedGoodsItem.BY_CusC4Number = "0020222-2";
				wrapper = GetWrapper(goodsItem);
				AssertEquals("Expected filled with unloaded value CusCode when unloaded state is DIF and unloaded value is not the same as declared value", "0020222-2", wrapper.CusCode);
			});
		}

		public void TestDescriptionOfGoods()
		{
			CombineAssertions(() =>
			{
				goodsItem.BY_Description = "description";
				AssertEquals("Expected filled with declared value DescriptionOfGoods when unloaded state is NEW", "description", wrapper.DescriptionOfGoods);

				goodsItem.BY_UnloadedState = "DIF";
				goodsItem.UnloadedGoodsItem.BY_Description = "description";
				wrapper = GetWrapper(goodsItem);
				AssertEquals("Expected empty DescriptionOfGoods when unloaded state is DIF and unloaded value is not the same as declared value", ZString.Empty, wrapper.DescriptionOfGoods);

				goodsItem.UnloadedGoodsItem.BY_Description = "description2";
				wrapper = GetWrapper(goodsItem);
				AssertEquals("Expected filled with unloaded value DescriptionOfGoods when unloaded state is DIF and unloaded value is not the same as declared value", "description2", wrapper.DescriptionOfGoods);
			});
		}

		public void TestCommodityCode()
		{
			CombineAssertions(() =>
			{
				goodsItem.BY_HarmonisedTariff = "2203001023";
				wrapper = GetWrapper(goodsItem);
				var commodityCodeDeparture = wrapper.CommodityCode;
				AssertNotNull("Expected filled CommodityCode when unloaded state is NEW", commodityCodeDeparture);
				AssertSame("Cached CommodityCode when item is departure", wrapper.CommodityCode, commodityCodeDeparture);
				AssertEquals("Expected filled with declared value CommodityCode.HarmonizedSystemSubHeadingCode when unloaded state is NEW", "220300", commodityCodeDeparture.HarmonizedSystemSubHeadingCode);
				AssertEquals("Expected filled with declared value CommodityCode.CombinedNomenclatureCode when unloaded state is NEW", "10", commodityCodeDeparture.CombinedNomenclatureCode);

				goodsItem.BY_UnloadedState = "DIF";
				goodsItem.UnloadedGoodsItem.BY_HarmonisedTariff = "2203001023";
				wrapper = GetWrapper(goodsItem);
				AssertNull("Expected empty CommodityCode when unloaded state is DIF and unloaded value is the same as declared value", wrapper.CommodityCode);

				goodsItem.UnloadedGoodsItem.BY_HarmonisedTariff = "1102012025";
				wrapper = GetWrapper(goodsItem);
				commodityCodeDeparture = wrapper.CommodityCode;
				AssertNotNull("Expected filled CommodityCode when unloaded state is DIF and unloaded value is not the same as declared value", commodityCodeDeparture);
				AssertEquals("Expected filled with unloaded value CommodityCode.HarmonizedSystemSubHeadingCode when unloaded state is DIF and unloaded value is not the same as declared value", "110201", commodityCodeDeparture.HarmonizedSystemSubHeadingCode);
				AssertEquals("Expected filled with unloaded value CommodityCode.CombinedNomenclatureCode when unloaded state is DIF and unloaded value is not the same as declared value", "20", commodityCodeDeparture.CombinedNomenclatureCode);

				goodsItem.UnloadedGoodsItem.BY_HarmonisedTariff = "1102011025";
				wrapper = GetWrapper(goodsItem);
				commodityCodeDeparture = wrapper.CommodityCode;
				AssertNotNull("Expected filled CommodityCode when unloaded state is DIF and unloaded value is not the same as declared value (only for Harmonized Code part)", commodityCodeDeparture);
				AssertEquals("Expected filled with unloaded value CommodityCode.HarmonizedSystemSubHeadingCode when unloaded state is DIF and unloaded value is not the same as declared value", "110201", commodityCodeDeparture.HarmonizedSystemSubHeadingCode);
				AssertEquals("Expected filled CommodityCode.CombinedNomenclatureCode when unloaded state is DIF and unloaded value is the same as declared value but Harmonized Code is different", "10", commodityCodeDeparture.CombinedNomenclatureCode);

				goodsItem.UnloadedGoodsItem.BY_HarmonisedTariff = "2203002025";
				wrapper = GetWrapper(goodsItem);
				commodityCodeDeparture = wrapper.CommodityCode;
				AssertNotNull("Expected filled CommodityCode when unloaded state is DIF and unloaded value is not the same as declared value (only for Combined Code)", commodityCodeDeparture);
				AssertEquals("Expected filled CommodityCode.HarmonizedSystemSubHeadingCode when unloaded state is DIF and unloaded value is the same as declared value but Combined Code is different", "220300", commodityCodeDeparture.HarmonizedSystemSubHeadingCode);
				AssertEquals("Expected filled with unloaded value CommodityCode.CombinedNomenclatureCode when unloaded state is DIF and unloaded value is not the same as declared value", "20", commodityCodeDeparture.CombinedNomenclatureCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var nctsBill = nctsHeader.Bills.AddNew();
			goodsItem = nctsBill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = "NEW";
			wrapper = GetWrapper(goodsItem);
		}

		NctsArrivalCargoDesc goodsItem;
		NotificationUnloadingNCTS5CommodityWrapper wrapper;

		NotificationUnloadingNCTS5CommodityWrapper GetWrapper(NctsArrivalCargoDesc item) => new NotificationUnloadingNCTS5CommodityWrapper(item);

		protected override NotificationUnloadingNCTS5CommodityWrapper GetProvider() => wrapper;
	}
}
