using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public sealed class GoodsItemsConfiguration : EU.NCTS.Business.GoodsItemsConfiguration
	{
		protected override ZBool DeleteConfirmationSupportCore(EU.NCTS.Business.NctsHeader header) => true;

		protected override EU.NCTS.Business.INctsArrivalCargoDescPhase5ValidationDecider GetArrivalPhase5ValidationDecider() => new NctsArrivalCargoDescPhase5ValidationDecider();

		protected override EU.NCTS.Business.INctsDepartureCargoDescPhase5ValidationDecider GetDeparturePhase5ValidationDecider(EU.NCTS.Business.NctsCommonCargoDesc goodsItem) => new NctsDepartureCargoDescPhase5ValidationDecider(goodsItem);

		protected override EU.NCTS.Business.INctsAdditionalInfoPhase5ValidationDecider GetAdditionalInfoPhase5ValidationDecider() => new NctsAdditionalInfoPhase5ValidationDecider();

		protected override ZBool IsLiabilityCalculationForArrivalSupportedCore() => true;

		protected override EU.NCTS.Business.NctsPreviousDocumentConfiguration GetNewNctsPreviousDocumentConfiguration() => new NctsPreviousDocumentConfiguration();

		protected override bool IsCL016CodeListFilterActive => true;
	}
}
