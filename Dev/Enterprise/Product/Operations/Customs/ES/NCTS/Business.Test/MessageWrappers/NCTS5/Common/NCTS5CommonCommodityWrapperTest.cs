using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonCommodityWrapperTest : WrapperHelperTest<NCTS5CommonCommodityWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if item is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "item"), () => new NCTS5CommonCommodityWrapper(null));
		}

		public void TestDescriptionOfGoods()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapperDeparture = GetWrapperDeparture(goodsItemDeparture);
					wrapperArrival = GetWrapperArrival(goodsItemArrival);
					wrapperUnloaded = GetWrapperUnloaded(goodsItemUnloaded);
					goodsItemDeparture.BY_Description = "description";
					goodsItemArrival.BY_Description = "description2";
					goodsItemUnloaded.BY_Description = "description3";
					AssertEquals("TransitionalPeriod: Expected filled DescriptionOfGoods when item is departure", "description", wrapperDeparture.DescriptionOfGoods);
					AssertEquals("TransitionalPeriod: Expected filled DescriptionOfGoods when item is arrival", "description2", wrapperArrival.DescriptionOfGoods);
					AssertEquals("TransitionalPeriod: Expected filled DescriptionOfGoods when item is unloaded", "description3", wrapperUnloaded.DescriptionOfGoods);

					goodsItemDeparture.BY_Description = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456";
					var expectedTrimDescriptionDeparture = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
					goodsItemArrival.BY_Description = "2234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456";
					var expectedTrimDescriptionArrival = "2234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
					goodsItemUnloaded.BY_Description = "3234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456";
					var expectedTrimDescriptionUnloaded = "3234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
					AssertEquals("TransitionalPeriod: Expected filled DescriptionOfGoods trimmed when item is departure", expectedTrimDescriptionDeparture, wrapperDeparture.DescriptionOfGoods);
					AssertEquals("TransitionalPeriod: Expected filled DescriptionOfGoods trimmed when item is arrival", expectedTrimDescriptionArrival, wrapperArrival.DescriptionOfGoods);
					AssertEquals("TransitionalPeriod: Expected filled DescriptionOfGoods trimmed when item is unloaded", expectedTrimDescriptionUnloaded, wrapperUnloaded.DescriptionOfGoods);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapperDeparture = GetWrapperDeparture(goodsItemDeparture);
					wrapperArrival = GetWrapperArrival(goodsItemArrival);
					wrapperUnloaded = GetWrapperUnloaded(goodsItemUnloaded);
					goodsItemDeparture.BY_Description = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012";
					var expectedTrimDescriptionDeparture = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012";
					goodsItemArrival.BY_Description = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012";
					var expectedTrimDescriptionArrival = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012";
					goodsItemUnloaded.BY_Description = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012";
					var expectedTrimDescriptionUnloaded = "12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012";
					AssertEquals("FinalPeriod: Expected filled DescriptionOfGoods trimmed when item is departure", expectedTrimDescriptionDeparture, wrapperDeparture.DescriptionOfGoods);
					AssertEquals("FinalPeriod: Expected filled DescriptionOfGoods trimmed when item is arrival", expectedTrimDescriptionArrival, wrapperArrival.DescriptionOfGoods);
					AssertEquals("FinalPeriod: Expected filled DescriptionOfGoods trimmed when item is unloaded", expectedTrimDescriptionUnloaded, wrapperUnloaded.DescriptionOfGoods);
				}
			});
		}

		public void TestCommodityCode()
		{
			CombineAssertions(() =>
			{
				var commodityCodeDeparture = wrapperDeparture.CommodityCode;
				AssertNotNull("Expected filled CommodityCode when item is departure", commodityCodeDeparture);
				AssertSame("Cached CommodityCode when item is departure", wrapperDeparture.CommodityCode, commodityCodeDeparture);

				var commodityCodeArrival = wrapperArrival.CommodityCode;
				AssertNotNull("Expected filled CommodityCode when item is arrival", commodityCodeArrival);
				AssertSame("Cached CommodityCode when item is arrival", wrapperArrival.CommodityCode, commodityCodeArrival);

				var commodityCodeUnloaded = wrapperUnloaded.CommodityCode;
				AssertNotNull("Expected filled CommodityCode when item is unloaded", commodityCodeUnloaded);
				AssertSame("Cached CommodityCode when item is unloaded", wrapperUnloaded.CommodityCode, commodityCodeUnloaded);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeaderDeparture = Factory.New<NctsHeader>();
			nctsHeaderDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsBillDeparture = nctsHeaderDeparture.Bills.AddNew();
			goodsItemDeparture = nctsBillDeparture.GoodsItems.AddNew();
			wrapperDeparture = GetWrapperDeparture(goodsItemDeparture);

			var nctsHeaderUnloaded = Factory.New<NctsHeader>();
			nctsHeaderUnloaded.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderUnloaded.SetMovementType(NctsMovementType.Codes.Arrival);
			var nctsBillUnloaded = nctsHeaderUnloaded.Bills.AddNew();
			goodsItemArrival = nctsBillUnloaded.ArrivalGoodsItems.AddNew();
			wrapperArrival = GetWrapperArrival(goodsItemArrival);

			goodsItemArrival.BY_UnloadedState = "DIF";
			goodsItemUnloaded = goodsItemArrival.UnloadedGoodsItem;
			wrapperUnloaded = GetWrapperUnloaded(goodsItemUnloaded);
		}

		NctsDepartureCargoDesc goodsItemDeparture;
		NCTS5CommonCommodityWrapper wrapperDeparture;
		NctsArrivalCargoDesc goodsItemArrival;
		NCTS5CommonCommodityWrapper wrapperArrival;
		NctsUnloadedCargoDesc goodsItemUnloaded;
		NCTS5CommonCommodityWrapper wrapperUnloaded;

		NCTS5CommonCommodityWrapper GetWrapperDeparture(NctsDepartureCargoDesc item) => new NCTS5CommonCommodityWrapper(item);
		NCTS5CommonCommodityWrapper GetWrapperArrival(NctsArrivalCargoDesc item) => new NCTS5CommonCommodityWrapper(item);
		NCTS5CommonCommodityWrapper GetWrapperUnloaded(NctsUnloadedCargoDesc item) => new NCTS5CommonCommodityWrapper(item);

		protected override NCTS5CommonCommodityWrapper GetProvider() => wrapperDeparture;
	}
}
