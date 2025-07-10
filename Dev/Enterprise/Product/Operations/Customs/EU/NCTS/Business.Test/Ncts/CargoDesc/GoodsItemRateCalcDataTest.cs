using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class GoodsItemRateCalcDataTest : TestCaseWithFactory
	{
		public void TestUnitOfMeasureValueListSecondUnitQty()
		{
			AssertUnitOfMeasureValueListSecondUnitQty("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertUnitOfMeasureValueListSecondUnitQty("Arrival Good Item", goodsItemArrival);
			}

			void AssertUnitOfMeasureValueListSecondUnitQty(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					var goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
					goodsItem.BY_CustomsSecondUnitQty = "NAR";
					goodsItem.BY_CustomsSecondQuantity = 15.00m;
					AssertEquals("Second Unit Qty Measure value NAR 15.00", 15.00m, goodsItemRateData.UnitOfMeasureValueList[goodsItem.BY_CustomsSecondUnitQty]);

					goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
					goodsItem.BY_CustomsSecondUnitQty = "";
					goodsItem.BY_CustomsSecondQuantity = 5.00m;
					AssertEquals("Second Unit Qty Measure not defined", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItem.BY_CustomsUnitQty));

					goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
					goodsItem.BY_CustomsSecondUnitQty = "NAR";
					goodsItem.BY_CustomsSecondQuantity = 0.00m;
					AssertEquals("Second Unit Qty Measure value NAR 0.00", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItem.BY_CustomsUnitQty));
				});
			}
		}

		public void TestUnitOfMeasureValueListCustomsUnitQty()
		{
			AssertUnitOfMeasureValueListCustomsUnitQty("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertUnitOfMeasureValueListCustomsUnitQty("Arrival Good Item", goodsItemArrival);
			}

			void AssertUnitOfMeasureValueListCustomsUnitQty(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					var goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
					goodsItem.BY_CustomsUnitQty = "KGM";
					goodsItem.BY_CustomsQuantity = 15.00m;
					AssertEquals("Customs Unit Qty Measure value KGM 15.00", 15.00m, goodsItemRateData.UnitOfMeasureValueList[goodsItem.BY_CustomsUnitQty]);

					goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
					goodsItem.BY_CustomsUnitQty = "";
					goodsItem.BY_CustomsQuantity = 5.00m;
					AssertEquals("Customs Unit Qty Measure not defined", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItem.BY_CustomsUnitQty));

					goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
					goodsItem.BY_CustomsUnitQty = "KGM";
					goodsItem.BY_CustomsQuantity = 0.00m;
					AssertEquals("Customs Unit Qty Measure value KGM 0.00", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItem.BY_CustomsUnitQty));
				});
			}
		}

		public void TestUnitOfMeasureValueListThirdUnitQty_Phase4()
		{
			CombineAssertions(() =>
			{
				var goodsItemRateData = new GoodsItemRateCalcData(goodsItemDeparture);
				goodsItemDeparture.BY_CustomsThirdUnitQty = "DTN";
				goodsItemDeparture.BY_CustomsThirdQuantity = 15.00m;
				AssertEquals("Third Unit Qty Measure value DTN 15.00", 15.00m, goodsItemRateData.UnitOfMeasureValueList[goodsItemDeparture.BY_CustomsThirdUnitQty]);

				goodsItemRateData = new GoodsItemRateCalcData(goodsItemDeparture);
				goodsItemDeparture.BY_CustomsThirdUnitQty = "";
				goodsItemDeparture.BY_CustomsThirdQuantity = 5.00m;
				AssertEquals("Third Unit Qty Measure not defined", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItemDeparture.BY_CustomsThirdUnitQty));

				goodsItemRateData = new GoodsItemRateCalcData(goodsItemDeparture);
				goodsItemDeparture.BY_CustomsThirdUnitQty = "DTN";
				goodsItemDeparture.BY_CustomsThirdQuantity = 0.00m;
				AssertEquals("Third Unit Qty Measure value DTN 0.00", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItemDeparture.BY_CustomsThirdUnitQty));
			});
		}

		public void TestUnitOfMeasureValueListThirdUnitQty_Phase5()
		{
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItemDeparture = departureHeader.Bills.AddNew().GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				var goodsItemRateData = new GoodsItemRateCalcData(goodsItemDeparture);
				goodsItemDeparture.BY_CustomsThirdUnitQty = "DTN";
				goodsItemDeparture.BY_CustomsThirdQuantity = 15.00m;
				AssertEquals("Third Unit Qty Measure value DTN 15.00 for Departure", 15.00m, goodsItemRateData.UnitOfMeasureValueList[goodsItemDeparture.BY_CustomsThirdUnitQty]);

				goodsItemRateData = new GoodsItemRateCalcData(goodsItemDeparture);
				goodsItemDeparture.BY_CustomsThirdUnitQty = "";
				goodsItemDeparture.BY_CustomsThirdQuantity = 5.00m;
				AssertEquals("Third Unit Qty Measure not defined", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItemDeparture.BY_CustomsThirdUnitQty));

				goodsItemRateData = new GoodsItemRateCalcData(goodsItemDeparture);
				goodsItemDeparture.BY_CustomsThirdUnitQty = "DTN";
				goodsItemDeparture.BY_CustomsThirdQuantity = 0.00m;
				AssertEquals("Third Unit Qty Measure value DTN 0.00", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItemDeparture.BY_CustomsThirdUnitQty));

				using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
				{
					goodsItemRateData = new GoodsItemRateCalcData(goodsItemArrival);
					goodsItemArrival.BY_CustomsThirdUnitQty = "DTN";
					goodsItemArrival.BY_CustomsThirdQuantity = 15.00m;
					AssertEquals("Third Unit Qty Measure value DTN 15.00 for Arrival", 15.00m, goodsItemRateData.UnitOfMeasureValueList[goodsItemArrival.BY_CustomsThirdUnitQty]);
				}
			});
		}

		public void TestUnitOfMeasureValueListFourthUnitQty()
		{
			var goodsItemRateData = new GoodsItemRateCalcData(goodsItemDeparture);
			goodsItemDeparture.BY_CustomsFourthUnitQty = "NAR";
			goodsItemDeparture.BY_CustomsFourthQuantity = 10.00m;
			AssertEquals("Fourth Unit Qty Measure value NAR 10.00 is not in the list for phase4", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItemDeparture.BY_CustomsFourthUnitQty));

			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItemDeparture = departureHeader.Bills.AddNew().GoodsItems.AddNew();

			AssertUnitOfMeasureValueListFourthUnitQty("Departure Good Item", goodsItemDeparture);
			using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
			{
				AssertUnitOfMeasureValueListFourthUnitQty("Arrival Good Item", goodsItemArrival);
			}

			void AssertUnitOfMeasureValueListFourthUnitQty(ZString assertionText, NctsCommonCargoDesc goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
					goodsItem.BY_CustomsFourthUnitQty = "DTN";
					goodsItem.BY_CustomsFourthQuantity = 15.00m;
					AssertEquals("Fourth Unit Qty Measure value DTN 15.00", 15.00m, goodsItemRateData.UnitOfMeasureValueList[goodsItem.BY_CustomsFourthUnitQty]);

					goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
					goodsItem.BY_CustomsFourthUnitQty = "";
					goodsItem.BY_CustomsFourthQuantity = 5.00m;
					AssertEquals("Third Unit Qty Measure not defined", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItem.BY_CustomsFourthUnitQty));

					goodsItemRateData = new GoodsItemRateCalcData(goodsItem);
					goodsItem.BY_CustomsFourthUnitQty = "DTN";
					goodsItem.BY_CustomsFourthQuantity = 0.00m;
					AssertEquals("Third Unit Qty Measure value DTN 0.00", false, goodsItemRateData.UnitOfMeasureValueList.ContainsKey(goodsItem.BY_CustomsFourthUnitQty));
				});
			}
		}

		public void TestUnitOfMeasureValueListConversion()
		{
			CombineAssertions(() =>
			{
				var goodsItemRateData = new GoodsItemRateCalcData(goodsItemDeparture);
				goodsItemDeparture.BY_CustomsThirdUnitQty = "LTR";
				goodsItemDeparture.BY_CustomsThirdQuantity = 260.00m;

				AssertEquals("Third Unit Qty Measure value LTR 260.00", 260.00m, goodsItemRateData.UnitOfMeasureValueList["LTR"]);
				AssertEquals("Third Unit Qty Measure value HLT it's added to the dict with the correct conversion", 2.60m, goodsItemRateData.UnitOfMeasureValueList["HLT"]);
				AssertEquals("Third Unit Qty Measure value KLT it's added to the dict with the correct conversion", 0.26m, goodsItemRateData.UnitOfMeasureValueList["KLT"]);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			departureHeader = Factory.New<NctsHeader>();
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItemDeparture = departureHeader.Bills.AddNew().GoodsItems.AddNew();

			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			goodsItemArrival = arrivalHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			goodsItemArrival.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;

			Factory.Save();
		}

		NctsHeader departureHeader;
		NctsDepartureCargoDesc goodsItemDeparture;
		NctsArrivalCargoDesc goodsItemArrival;
	}
}
