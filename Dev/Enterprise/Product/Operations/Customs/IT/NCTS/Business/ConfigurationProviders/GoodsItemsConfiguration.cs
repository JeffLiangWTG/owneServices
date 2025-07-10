using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class GoodsItemsConfiguration : EU.NCTS.Business.GoodsItemsConfiguration
{
	protected override ZBool DeleteConfirmationSupportCore(EU.NCTS.Business.NctsHeader header) => true;

	protected override INctsDepartureCargoDescPhase5ValidationDecider GetDeparturePhase5ValidationDecider(NctsCommonCargoDesc goodsItem) => new NctsDepartureCargoDescPhase5ValidationDecider();

	protected override INctsAdditionalInfoPhase5ValidationDecider GetAdditionalInfoPhase5ValidationDecider() => new NctsAdditionalInfoPhase5ValidationDecider();

	protected override EU.NCTS.Business.NctsPreviousDocumentConfiguration GetNewNctsPreviousDocumentConfiguration() => new NctsPreviousDocumentConfiguration();

	protected override EU.NCTS.Business.NctsSupportingDocumentConfiguration GetNewNctsSupportingDocumentConfiguration() => new NctsSupportingDocumentConfiguration();

	protected override bool IsCL016CodeListFilterActive => true;
}
