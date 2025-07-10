using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class NctsConfiguration : EU.NCTS.Business.NctsConfiguration
{
	protected override EU.NCTS.Business.MovementHeaderConfiguration GetNewMovementHeaderConfiguration() => new MovementHeaderConfiguration();

	protected override ZBool DocDataPlugInSupportForDepartureMovement => true;

	protected override ZBool UseAdditionalDeclarationTypeCore => true;

	protected override ZBool MiscTabPageSupportCore(EU.NCTS.Business.NctsHeader nctsHeader) => nctsHeader.IsDepartureMovement;

	protected override EU.NCTS.Business.GoodsItemsConfiguration GetNewGoodsItemsConfiguration() => new GoodsItemsConfiguration();

	protected override EU.NCTS.Business.ValidationRuleConfiguration GetNewValidationRuleConfiguration() => new ValidationRuleConfiguration();

	protected override EU.NCTS.Business.MessageSendingConfiguration GetNewMessageSendingConfiguration() => new MessageSendingConfiguration();

	protected override EU.NCTS.Business.GuaranteeConfiguration GetNewGuaranteeConfiguration() => new GuaranteeConfiguration();

	protected override EU.NCTS.Business.CountryOfRoutingConfiguration GetNewCountryOfRoutingConfiguration() => new CountryOfRoutingConfiguration();

	protected override NctsEuOfficeCodeConfiguration GetNewNctsEuOfficeCodeConfiguration() => new NctsItOfficeCodeConfiguration();

	protected override EU.NCTS.Business.BillConfiguration GetNewBillConfiguration() => new BillConfiguration();

	protected override INctsHeaderDeparturePhase5ValidationDecider GetHeaderDeparturePhase5ValidationDecider() => new NctsHeaderDeparturePhase5ValidationDecider();

	protected override EU.NCTS.Business.NctsContainerConfiguration GetNewNctsContainerConfiguration() => new NctsContainerConfiguration();

	protected override EU.NCTS.Business.CusSealConfiguration GetNewCusSealConfiguration() => new CusSealConfiguration();

	protected override ZBool UsePresentationDateTimeCore => true;

	protected override EU.NCTS.Business.CommonPreviousDocumentConfiguration GetNewCommonPreviousDocumentConfiguration() => new CommonPreviousDocumentConfiguration();

	protected override EU.NCTS.Business.CusTransportMeansConfiguration GetNewCusTransportMeansConfiguration() => new CusTransportMeansConfiguration();
}
