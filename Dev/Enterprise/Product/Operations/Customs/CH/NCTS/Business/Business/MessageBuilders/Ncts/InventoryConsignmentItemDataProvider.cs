using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class InventoryConsignmentItemDataProvider : ConsignmentItemDataProvider
{
	public new static IEnumerable<InventoryConsignmentItemDataProvider> NewCollection(INctsCommonCargoDescCollection<NctsCommonCargoDesc> goodsItems)
	{
		if (goodsItems.IsNullOrEmpty())
		{
			return null;
		}

		var houseConsignmentUnloadedState = goodsItems.First().Bill.MovementDetail.B9_UnloadedState;
		var unloadedStateFilter = (Func<ZString, bool>)(houseConsignmentUnloadedState == NctsUnloadedStateList.Codes.MIS ? InventoryDataProviderHelper.IsUnloadingStateDECorMISorDIF : InventoryDataProviderHelper.IsUnloadingStateNEWorMISorDIF);

		return goodsItems
			.Where(x => unloadedStateFilter(x.BY_UnloadedState))
			.Cast<NctsArrivalCargoDesc>()
			.Select(goodsItem => new InventoryConsignmentItemDataProvider(goodsItem, houseConsignmentUnloadedState));
	}

	InventoryConsignmentItemDataProvider(NctsArrivalCargoDesc arrivalGoodsItem, ZString houseConsignmentUnloadedState) : base(arrivalGoodsItem)
	{
		this.arrivalGoodsItem = arrivalGoodsItem;
		this.houseConsignmentUnloadedState = houseConsignmentUnloadedState;
	}
	readonly NctsArrivalCargoDesc arrivalGoodsItem;
	readonly ZString houseConsignmentUnloadedState;

	NctsBill Bill => (NctsBill)arrivalGoodsItem.Bill;

	bool IsHouseConsignmentMIS => houseConsignmentUnloadedState == NctsUnloadedStateList.Codes.MIS;

	bool IsHouseConsignmentDIF => houseConsignmentUnloadedState == NctsUnloadedStateList.Codes.DIF;

	bool IsNP70237Valid => !IsHouseConsignmentMIS && !arrivalGoodsItem.IsUnloadedStateMIS;

	bool IsNP70216Valid => !IsHouseConsignmentDIF || arrivalGoodsItem.IsDIFWithDifferences || !arrivalGoodsItem.IsAllPackageMIS;

	protected override IEnumerable<IPackaging> GetPackagingDataProvidersCore(INctsPackageCollection<EU.NCTS.Business.NctsPackage, NctsCommonCargoDesc> packages) => InventoryConsignmentItemPackagingDataProvider.NewCollection(packages);

	protected override ICommodity GetCommodityDataProviderCore() => IsNP70237Valid && IsNP70216Valid ? InventoryCommodityDataProvider.New(houseConsignmentUnloadedState, arrivalGoodsItem) : null;

	public override string UnloadingRemarkCode => IsHouseConsignmentMIS ? Bill?.UnloadingRemarkCode : arrivalGoodsItem?.UnloadingRemarkCode;

	public override string UnloadingRemarkText => IsHouseConsignmentMIS ? Bill?.UnloadingRemarkText : arrivalGoodsItem?.UnloadingRemarkText;
}
