using System;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class InventoryCommodityDataProvider : CommodityDataProvider
{
	public static InventoryCommodityDataProvider New(ZString houseConsignmentUnloadedState, NctsArrivalCargoDesc goodsItem) => goodsItem == null ? null : new InventoryCommodityDataProvider(houseConsignmentUnloadedState, goodsItem);

	InventoryCommodityDataProvider(ZString houseConsignmentUnloadedState, NctsArrivalCargoDesc goodsItem) : base(goodsItem)
	{
		this.houseConsignmentUnloadedState = houseConsignmentUnloadedState;
		this.unloadedGoodsItem = goodsItem.UnloadedGoodsItem;
	}
	NctsArrivalCargoDesc arrivalGoodsItem => (NctsArrivalCargoDesc)goodsItem;
	readonly ZString houseConsignmentUnloadedState;
	readonly NctsCommonCargoDesc unloadedGoodsItem;

	bool IsHouseConsignmentDIF => houseConsignmentUnloadedState == NctsUnloadedStateList.Codes.DIF;

	bool IsGoodsItemDIFWithDifference => arrivalGoodsItem.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF && InventoryDataProviderHelper.IsDifferent(arrivalGoodsItem.BY_HarmonisedTariff, unloadedGoodsItem.BY_HarmonisedTariff);

	bool IsMassProvided => GrossMass != null || NetMass != null;

	bool IsNP70216Valid => !IsHouseConsignmentDIF || IsGoodsItemDIFWithDifference || !arrivalGoodsItem.IsAllPackageMIS;

	protected override IGoodsMeasure GoodsMeasureCore => IsMassProvided && IsNP70216Valid ? base.GoodsMeasureCore : null;

	protected override ICommodityCode CommodityCodeCore => IsNP70216Valid ? base.CommodityCodeCore : null;

	protected override T? GetCargoDescValue<T>(Func<NctsCommonCargoDesc, T> getter) => !arrivalGoodsItem.IsDIFWithDifferencesIncludingPackages ? getter(goodsItem) : (getter(unloadedGoodsItem).IsEmpty || getter(unloadedGoodsItem).Equals(getter(goodsItem)) ? null : getter(unloadedGoodsItem));
}
