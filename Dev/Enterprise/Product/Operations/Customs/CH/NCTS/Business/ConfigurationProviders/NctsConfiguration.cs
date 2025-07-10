using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NctsConfiguration : EU.NCTS.Business.NctsConfiguration
{
	protected override EU.NCTS.Business.MovementHeaderConfiguration GetNewMovementHeaderConfiguration() => new MovementHeaderConfiguration();

	protected override EU.NCTS.Business.ValidationRuleConfiguration GetNewValidationRuleConfiguration() => new ValidationRuleConfiguration();

	protected override EU.NCTS.Business.CountryOfRoutingConfiguration GetNewCountryOfRoutingConfiguration() => new CountryOfRoutingConfiguration();

	protected override EU.NCTS.Business.BillConfiguration GetNewBillConfiguration() => new BillConfiguration();

	protected override INctsHeaderDeparturePhase5ValidationDecider GetHeaderDeparturePhase5ValidationDecider() => new NctsHeaderDeparturePhase5ValidationDecider();

	protected override EU.NCTS.Business.MessageSendingConfiguration GetNewMessageSendingConfiguration() => new MessageSendingConfiguration();

	protected override ZBool UseLocalReferenceNumberIgnoreInDatabaseCheckCore => true;

	protected override EU.NCTS.Business.GoodsItemsConfiguration GetNewGoodsItemsConfiguration() => new GoodsItemsConfiguration();

	protected override EU.NCTS.Business.NctsEuOfficeCodeConfiguration GetNewNctsEuOfficeCodeConfiguration() => new NctsEuOfficeCodeConfiguration();

	protected override EU.NCTS.Business.GuaranteeConfiguration GetNewGuaranteeConfiguration() => new GuaranteeConfiguration();

	protected override EU.NCTS.Business.NctsPackageConfiguration GetNewNctsPackageConfiguration() => new NctsPackageConfiguration();

	protected override EU.NCTS.Business.NctsContainerConfiguration GetNewNctsContainerConfiguration() => new NctsContainerConfiguration();

	protected override EU.NCTS.Business.CusSealConfiguration GetNewCusSealConfiguration() => new CusSealConfiguration();

	protected override EU.NCTS.Business.CusSupplyChainActorReferenceConfiguration GetNewCusSupplyChainActorReferenceConfiguration() => new CusSupplyChainActorReferenceConfiguration();

	protected override EU.NCTS.Business.CommonPreviousDocumentConfiguration GetNewCommonPreviousDocumentConfiguration() => new CommonPreviousDocumentConfiguration();

	protected override ZBool DocDataPlugInSupportForDepartureMovement => true;
}
