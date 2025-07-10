using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BE.NCTS.Business;

public class NctsArrivalMovementHeader : EU.NCTS.Business.NctsArrivalMovementHeader
	, Integration.Customs.BE.IArrivalMovementHeader
{
	public NctsArrivalMovementHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		IsSimplifiedNctsProcedure = ZBool.True;
	}

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	public new EU.NCTS.Business.IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> ArrivalTransportInfos => (EU.NCTS.Business.IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>)base.ArrivalTransportInfos;
	protected override EU.NCTS.Business.IArrivalCusTransportMeansCollection<EU.NCTS.Business.ArrivalCusTransportMeans> GetNewArrivalTransportInfos() => new EU.NCTS.Business.ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>(this);

	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);

	protected override ZBool ShouldSyncDestinationTraderWithAuthorizationCore => false;

	public override ZDecimal TotalUnloadedGrossMassInKilograms => Header.Bills.Aggregate(ZDecimal.Zero, (current, bill)
		=> current + bill.ArrivalGoodsItems.Where(goodsItem => goodsItem.BY_UnloadedState != EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS && bill.MovementDetail.B9_UnloadedState != EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS)
		.Cast<EU.NCTS.Business.NctsArrivalCargoDesc>()
		.Sum(goodsItem => goodsItem.BY_UnloadedState == EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF ? goodsItem.UnloadedGoodsItem.GrossMassInKilograms : goodsItem.GrossMassInKilograms));
}
