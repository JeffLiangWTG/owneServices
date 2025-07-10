namespace Enterprise.Customs.CH.NCTS.Business;

public class GoodsItemsConfiguration : EU.NCTS.Business.GoodsItemsConfiguration
{
	protected override EU.NCTS.Business.INctsArrivalCargoDescPhase5ValidationDecider GetArrivalPhase5ValidationDecider() => new NctsArrivalCargoDescPhase5ValidationDecider();

	protected override EU.NCTS.Business.INctsDepartureCargoDescPhase5ValidationDecider GetDeparturePhase5ValidationDecider(EU.NCTS.Business.NctsCommonCargoDesc goodsItem) => new NctsDepartureCargoDescPhase5ValidationDecider();

	protected override EU.NCTS.Business.INctsAdditionalInfoPhase5ValidationDecider GetAdditionalInfoPhase5ValidationDecider() => new NctsAdditionalInfoPhase5ValidationDecider();

	protected override EU.NCTS.Business.NctsPreviousDocumentConfiguration GetNewNctsPreviousDocumentConfiguration() => new NctsPreviousDocumentConfiguration();
}
