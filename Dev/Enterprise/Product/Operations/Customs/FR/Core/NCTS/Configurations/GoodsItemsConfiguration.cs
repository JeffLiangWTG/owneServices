using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Configurations
{
	public sealed class GoodsItemsConfiguration : EU.NCTS.Business.GoodsItemsConfiguration
	{
		protected override INctsDepartureCargoDescPhase5ValidationDecider GetDeparturePhase5ValidationDecider(NctsCommonCargoDesc goodsItem) => new Business.NCTS.NctsDepartureCargoDescPhase5ValidationDecider();
	}
}
