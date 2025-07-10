using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public sealed class GoodsItemsConfiguration : EU.NCTS.Business.GoodsItemsConfiguration
{
	protected override INctsDepartureCargoDescPhase5ValidationDecider GetDeparturePhase5ValidationDecider(NctsCommonCargoDesc goodsItem) => new NctsDepartureCargoDescPhase5ValidationDecider();
}
