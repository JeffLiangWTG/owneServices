using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class InventoryHouseConsignmentDataProvider : HouseConsignmentDataProvider
{
	public new static IEnumerable<InventoryHouseConsignmentDataProvider> NewCollection(NctsBillCollection bills)
	{
		return bills?.Cast<NctsBill>()
			.Where(nctsBill => nctsBill.MovementDetail.B9_UnloadedState.IsUnloadingStateMISorDIF())
			.Select(nctsBill => new InventoryHouseConsignmentDataProvider(nctsBill));
	}

	InventoryHouseConsignmentDataProvider(NctsBill nctsBill) : base(nctsBill, ZShort.ParseSafe(nctsBill.MovementDetail.B9_SeqNo, 0))
	{
	}

	protected override IEnumerable<IConsignmentItem> GetConsignmentItemDataProvidersCore(INctsCommonCargoDescCollection<NctsCommonCargoDesc> goodsItems) => InventoryConsignmentItemDataProvider.NewCollection(goodsItems);
}
