using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsArrivalAndUnloadingCargoDesc))]
	sealed class NctsArrivalAndUnloadingCargoDescTest : EU.NCTS.Business.Testing.NctsCommonCargoDescAbstractTest<NctsHeader>
	{
		public void TestValidation()
		{
			AssertType<NctsArrivalAndUnloadingCargoDescValidation>(goodsItem.Validation);
		}

		public void TestLineNoIsReadOnly()
		{
			AssertEquals("GoodsItem is not readonly when arrival movement", false, goodsItem.BY_LineNoInfo.ReadOnly);

			var unloadingGoodsItem = goodsItem.Header.UnloadingMovementHeader.GoodsItems.AddNew();
			unloadingGoodsItem.IsNew = false;
			AssertEquals("GoodsItem is readonly when unloading movement and IsNew is false", true, unloadingGoodsItem.BY_LineNoInfo.ReadOnly);

			unloadingGoodsItem.IsNew = true;
			AssertEquals("GoodsItem is not readonly when unloading movement and IsNew is true", false, unloadingGoodsItem.BY_LineNoInfo.ReadOnly);
		}

		public void TestBY_MonetaryValueMaxDecimal()
		{
			AssertEquals(2, goodsItem.BY_MonetaryValueInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
		}

		public void TestBillOfLadingItemMaxLength()
		{
			AssertEquals(21, goodsItem.BillOfLadingItemInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
		}

		public void TestShouldAddUnloadingGoodsToWrapper()
		{
			var unloadingGoodsItem = (NctsArrivalAndUnloadingCargoDesc)goodsItem.Header.UnloadingMovementHeader.GoodsItems.AddNew();
			unloadingGoodsItem.HasDifferences = false;
			unloadingGoodsItem.IsNew = false;
			unloadingGoodsItem.IsMissing = false;

			CombineAssertions(() =>
			{
				AssertEquals("Unloading GoodItem ShouldAddUnloadingGoodsToWrapper should be false if HasDifferences and IsNew and IsMissing is false", false, unloadingGoodsItem.ShouldAddUnloadingGoodsToWrapper);

				unloadingGoodsItem.HasDifferences = true;
				AssertEquals("Unloading GoodItem ShouldAddUnloadingGoodsToWrapper should be true if HasDifferences is true", true, unloadingGoodsItem.ShouldAddUnloadingGoodsToWrapper);

				unloadingGoodsItem.HasDifferences = false;
				unloadingGoodsItem.IsNew = true;
				AssertEquals("Unloading GoodItem ShouldAddUnloadingGoodsToWrapper should be true if IsNew is true", true, unloadingGoodsItem.ShouldAddUnloadingGoodsToWrapper);

				unloadingGoodsItem.IsNew = false;
				unloadingGoodsItem.IsMissing = true;
				AssertEquals("Unloading GoodItem ShouldAddUnloadingGoodsToWrapper should be true if IsMissing is true", true, unloadingGoodsItem.ShouldAddUnloadingGoodsToWrapper);
			});
		}

		public void TestCanDeleteNonDeclared()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var goodsItem = nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
			goodsItem.IsNew = false;

			CombineAssertions(() =>
			{
				AssertEquals("Unloading GoodItem can't be deleted cause is not new", false, goodsItem.CanDelete);

				goodsItem.IsNew = true;
				AssertEquals("Unloading GoodItem can be deleted cause is not new", true, goodsItem.CanDelete);

				var goodsItemArrival = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
				AssertEquals("Arrival GoodItem can be deleted", true, goodsItemArrival.CanDelete);
			});
		}

		public void TestCanDeleteDeclared()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var goodsItem = nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
			goodsItem.IsNew = true;

			CombineAssertions(() =>
			{
				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl;
				AssertEquals("Unloading GoodItem can't be deleted cause the Arrival Status is DCC", false, goodsItem.CanDelete);

				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;
				nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.Rejected;
				AssertEquals("Unloading GoodItem can be deleted cause the Arrival Status is not DCC or AWO and Message Status is not MAS", true, goodsItem.CanDelete);

				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsWrittenOff;
				AssertEquals("Unloading GoodItem can't be deleted cause the Arrival Status is AWO", false, goodsItem.CanDelete);

				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;
				nctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;
				AssertEquals("Unloading GoodItem can't be deleted cause the Message Status is MAS", false, goodsItem.CanDelete);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			goodsItem = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
		}
		NctsArrivalAndUnloadingCargoDesc goodsItem;

		protected override ZString CountryCode => Core.Constants.CountryCodes.Spain;

		protected override BusinessObject GetNewBusinessObject() => goodsItem;
	}
}
