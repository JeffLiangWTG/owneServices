using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonGoodsMeasureWrapperTest : WrapperHelperTest<NCTS5CommonGoodsMeasureWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if item is null (departure)", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "item"), () => new NCTS5CommonGoodsMeasureWrapper((NctsDepartureCargoDesc)null));

				AssertExceptionThrown("Constructor Throws Exception if item is null (arrival)", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "item"), () => new NCTS5CommonGoodsMeasureWrapper((NctsArrivalCargoDesc)null));
			});
		}

		public void TestGrossMass()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapperDeparture = new NCTS5CommonGoodsMeasureWrapper(goodsItemDeparture);
					wrapperArrival = new NCTS5CommonGoodsMeasureWrapper(goodsItemArrival);

					goodsItemDeparture.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
					goodsItemDeparture.BY_GrossWeight = 1.1235678m;
					AssertEquals("TransitionalPeriod: Expected filled GrossMass 3 decimals when item is departure", 1.124m, wrapperDeparture.GrossMass);

					goodsItemArrival.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
					goodsItemArrival.BY_GrossWeight = 1.1235678m;
					AssertEquals("TransitionalPeriod: Expected filled with declared value GrossMass 3 decimals when item is arrival and unloaded state is NEW", 1.124m, wrapperArrival.GrossMass);

					goodsItemArrival.BY_UnloadedState = "DIF";
					goodsItemArrival.UnloadedGoodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
					goodsItemArrival.UnloadedGoodsItem.BY_GrossWeight = 1.1235678m;
					AssertEquals("TransitionalPeriod: Expected empty GrossMass when item is arrival, unloaded state is DIF and uloaded value is the same as declared value", ZDecimal.Zero, wrapperArrival.GrossMass);

					goodsItemArrival.UnloadedGoodsItem.BY_GrossWeight = 1.2235678m;
					AssertEquals("TransitionalPeriod: Expected filled with unloaded value GrossMass 3 decimals when item is arrival, unloaded state is DIF and unloaded value isnot  the same as declared value", 1.224m, wrapperArrival.GrossMass);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapperDeparture = new NCTS5CommonGoodsMeasureWrapper(goodsItemDeparture);
					wrapperArrival = new NCTS5CommonGoodsMeasureWrapper(goodsItemArrival);

					goodsItemArrival.BY_UnloadedState = "NEW";
					AssertEquals("FinalPeriod: Expected filled GrossMass 6 decimals when item is departure", 1.123568m, wrapperDeparture.GrossMass);
					AssertEquals("FinalPeriod: Expected filled with declared value GrossMass 6 decimals when item is arrival and unloaded state is NEW", 1.123568m, wrapperArrival.GrossMass);

					goodsItemArrival.BY_UnloadedState = "DIF";
					goodsItemArrival.UnloadedGoodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
					goodsItemArrival.UnloadedGoodsItem.BY_GrossWeight = 1.2235678m;
					AssertEquals("FinalPeriod: Expected filled with unloaded value GrossMass 6 decimals when item is arrival, unloaded state is DIF and unloaded value isnot  the same as declared value", 1.223568m, wrapperArrival.GrossMass);
				}
			});
		}

		public void TestGrossMassSpecified()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected true when item is departure", true, wrapperDeparture.GrossMassSpecified);

				goodsItemArrival.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
				goodsItemArrival.BY_GrossWeight = 1.1235m;
				AssertEquals("Expected true GrossMassSpecified when item is arrival and unloaded state is NEW", true, wrapperArrival.GrossMassSpecified);

				goodsItemArrival.BY_UnloadedState = "DIF";
				goodsItemArrival.UnloadedGoodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
				goodsItemArrival.UnloadedGoodsItem.BY_GrossWeight = 1.1235m;
				AssertEquals("Expected false GrossMassSpecified when item is arrival, unloaded state is DIF and uloaded value is the same as declared value", false, wrapperArrival.GrossMassSpecified);

				goodsItemArrival.UnloadedGoodsItem.BY_GrossWeight = 1.2235m;
				AssertEquals("Expected true GrossMassSpecified when item is arrival, unloaded state is DIF and unloaded value isnot  the same as declared value", true, wrapperArrival.GrossMassSpecified);
			});
		}

		public void TestNetMass()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					wrapperDeparture = new NCTS5CommonGoodsMeasureWrapper(goodsItemDeparture);
					wrapperArrival = new NCTS5CommonGoodsMeasureWrapper(goodsItemArrival);

					goodsItemDeparture.BY_CustomsUnitQty = Core.Constants.Weight.Kilograms;
					goodsItemDeparture.BY_CustomsQuantity = 1.1235678m;
					AssertEquals("TransitionalPeriod: Expected filled NetMass when BM_ReducedDatasetIndicator is false and item is departure", 1.124m, wrapperDeparture.NetMass);

					nctsHeaderDeparture.MovementHeader.BM_ReducedDatasetIndicator = true;
					AssertEquals("TransitionalPeriod: Expected empty NetMass when BM_ReducedDatasetIndicator is true and item is departure", ZDecimal.Zero, wrapperDeparture.NetMass);

					goodsItemArrival.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
					goodsItemArrival.BY_NetWeight = 1.1235678m;
					AssertEquals("TransitionalPeriod: Expected filled with declared value NetMass when item is arrival and unloaded state is NEW", 1.124m, wrapperArrival.NetMass);

					goodsItemArrival.BY_UnloadedState = "DIF";
					goodsItemArrival.UnloadedGoodsItem.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
					goodsItemArrival.UnloadedGoodsItem.BY_NetWeight = 1.1235678m;
					AssertEquals("TransitionalPeriod: Expected empty NetMass when item is arrival, unloaded state is DIF and uloaded value is the same as declared value", ZDecimal.Zero, wrapperArrival.NetMass);

					goodsItemArrival.UnloadedGoodsItem.BY_NetWeight = 1.2235678m;
					AssertEquals("TransitionalPeriod: Expected filled with unloaded value NetMass when item is arrival, unloaded state is DIF and unloaded value isnot  the same as declared value", 1.224m, wrapperArrival.NetMass);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					wrapperDeparture = new NCTS5CommonGoodsMeasureWrapper(goodsItemDeparture);
					wrapperArrival = new NCTS5CommonGoodsMeasureWrapper(goodsItemArrival);

					nctsHeaderDeparture.MovementHeader.BM_ReducedDatasetIndicator = false;
					goodsItemArrival.BY_UnloadedState = "NEW";
					AssertEquals("FinalPeriod: Expected filled GrossMass 6 decimals when item is departure", 1.123568m, wrapperDeparture.NetMass);
					AssertEquals("FinalPeriod: Expected filled with declared value GrossMass 6 decimals when item is arrival and unloaded state is NEW", 1.123568m, wrapperArrival.NetMass);

					goodsItemArrival.BY_UnloadedState = "DIF";
					goodsItemArrival.UnloadedGoodsItem.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
					goodsItemArrival.UnloadedGoodsItem.BY_NetWeight = 1.2235678m;
					AssertEquals("FinalPeriod: Expected filled with unloaded value GrossMass 6 decimals when item is arrival, unloaded state is DIF and unloaded value isnot  the same as declared value", 1.223568m, wrapperArrival.NetMass);
				}
			});
		}

		public void TestNetMassSpecified()
		{
			CombineAssertions(() =>
			{
				goodsItemDeparture.BY_CustomsUnitQty = Core.Constants.Weight.Kilograms;
				goodsItemDeparture.BY_CustomsQuantity = 1.1235m;
				nctsHeaderDeparture.MovementHeader.BM_ReducedDatasetIndicator = false;
				AssertEquals("Expected true NetMassSpecified when BM_ReducedDatasetIndicator is false and item is departure", true, wrapperDeparture.NetMassSpecified);

				nctsHeaderDeparture.MovementHeader.BM_ReducedDatasetIndicator = true;
				AssertEquals("Expected false NetMassSpecified when BM_ReducedDatasetIndicator is true and item is departure", false, wrapperDeparture.NetMassSpecified);

				goodsItemArrival.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
				goodsItemArrival.BY_NetWeight = 1.1235m;
				AssertEquals("Expected true NetMassSpecified when item is arrival and unloaded state is NEW", true, wrapperArrival.NetMassSpecified);

				goodsItemArrival.BY_UnloadedState = "DIF";
				goodsItemArrival.UnloadedGoodsItem.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
				goodsItemArrival.UnloadedGoodsItem.BY_NetWeight = 1.1235m;
				AssertEquals("Expected false NetMassSpecified when item is arrival, unloaded state is DIF and uloaded value is the same as declared value", false, wrapperArrival.NetMassSpecified);

				goodsItemArrival.UnloadedGoodsItem.BY_NetWeight = 1.2235m;
				AssertEquals("Expected true NetMassSpecified when item is arrival, unloaded state is DIF and unloaded value isnot  the same as declared value", true, wrapperArrival.NetMassSpecified);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeaderDeparture = Factory.New<NctsHeader>();
			nctsHeaderDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsBillDeparture = nctsHeaderDeparture.Bills.AddNew();
			goodsItemDeparture = nctsBillDeparture.GoodsItems.AddNew();
			wrapperDeparture = new NCTS5CommonGoodsMeasureWrapper(goodsItemDeparture);

			var nctsHeaderUnloaded = Factory.New<NctsHeader>();
			nctsHeaderUnloaded.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderUnloaded.SetMovementType(NctsMovementType.Codes.Arrival);
			var nctsBillUnloaded = nctsHeaderUnloaded.Bills.AddNew();
			goodsItemArrival = nctsBillUnloaded.ArrivalGoodsItems.AddNew();
			wrapperArrival = new NCTS5CommonGoodsMeasureWrapper(goodsItemArrival);
		}

		NctsHeader nctsHeaderDeparture;
		NctsDepartureCargoDesc goodsItemDeparture;
		NCTS5CommonGoodsMeasureWrapper wrapperDeparture;
		NctsArrivalCargoDesc goodsItemArrival;
		NCTS5CommonGoodsMeasureWrapper wrapperArrival;

		protected override NCTS5CommonGoodsMeasureWrapper GetProvider() => wrapperDeparture;
	}
}
