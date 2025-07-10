using CargoWise.Types;

namespace Enterprise.Customs.GB.Business
{
	public sealed class NctsConfiguration : EU.NCTS.Business.NctsConfiguration
	{
		protected override EU.NCTS.Business.MovementHeaderConfiguration GetNewMovementHeaderConfiguration() => new MovementHeaderConfiguration();

		protected override EU.NCTS.Business.GuaranteeConfiguration GetNewGuaranteeConfiguration() => new GuaranteeConfiguration();

		protected override ZBool DocDataPlugInSupportForDepartureMovement => true;

		protected override EU.NCTS.Business.MessageSendingConfiguration GetNewMessageSendingConfiguration() => new MessageSendingConfiguration();

		protected override ZBool UseAdditionalDeclarationTypeCore => true;

		protected override EU.NCTS.Business.ValidationRuleConfiguration GetNewValidationRuleConfiguration() => new ValidationRuleConfiguration();

		protected override EU.NCTS.Business.CountryOfRoutingConfiguration GetNewCountryOfRoutingConfiguration() => new CountryOfRoutingConfiguration();

		protected override EU.NCTS.Business.NctsEuOfficeCodeConfiguration GetNewNctsEuOfficeCodeConfiguration() => new NctsEuOfficeCodeConfiguration();

		protected override ZBool AllowMixedCaseAuthorisationNumbersCore => true;
	}
}
