using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business;

public sealed class NctsConfiguration : EU.NCTS.Business.NctsConfiguration
{
	public override ZBool ReceiveIE043UnloadingPermissionDetailsMessage => false;

	protected override ZBool DocDataPlugInSupportForDepartureMovement => true;

	protected override ZBool UseAdditionalDeclarationTypeCore => true;

	protected override EU.NCTS.Business.GoodsItemsConfiguration GetNewGoodsItemsConfiguration() => new GoodsItemsConfiguration();

	protected override EU.NCTS.Business.GuaranteeConfiguration GetNewGuaranteeConfiguration() => new GuaranteeConfiguration();

	protected override EU.NCTS.Business.ValidationRuleConfiguration GetNewValidationRuleConfiguration() => new ValidationRuleConfiguration();

	protected override EU.NCTS.Business.MovementHeaderConfiguration GetNewMovementHeaderConfiguration() => new MovementHeaderConfiguration();

	protected override EU.NCTS.Business.NctsContainerConfiguration GetNewNctsContainerConfiguration() => new NctsContainerConfiguration();

	protected override EU.NCTS.Business.CountryOfRoutingConfiguration GetNewCountryOfRoutingConfiguration() => new CountryOfRoutingConfiguration();

	protected override EU.NCTS.Business.NctsEuOfficeCodeConfiguration GetNewNctsEuOfficeCodeConfiguration() => new NctsESOfficeCodeConfiguration();

	protected override EU.NCTS.Business.BillConfiguration GetNewBillConfiguration() => new BillConfiguration();

	protected override EU.NCTS.Business.NctsPackageConfiguration GetNewNctsPackageConfiguration() => new NctsPackageConfiguration();

	protected override EU.NCTS.Business.INctsHeaderDeparturePhase5ValidationDecider GetHeaderDeparturePhase5ValidationDecider() => new NctsHeaderDeparturePhase5ValidationDecider();

	protected override EU.NCTS.Business.INctsHeaderArrivalPhase5ValidationDecider GetHeaderArrivalPhase5ValidationDecider() => new NctsHeaderArrivalPhase5ValidationDecider();

	protected override EU.NCTS.Business.CommonPreviousDocumentConfiguration GetNewCommonPreviousDocumentConfiguration() => new CommonPreviousDocumentConfiguration();
}
