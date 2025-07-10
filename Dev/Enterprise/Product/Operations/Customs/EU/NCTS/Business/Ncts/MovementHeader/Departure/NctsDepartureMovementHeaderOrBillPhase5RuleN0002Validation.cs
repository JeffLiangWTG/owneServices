using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business;

sealed class NctsDepartureMovementHeaderOrBillPhase5RuleN0002Validation
{
	public NctsDepartureMovementHeaderOrBillPhase5RuleN0002Validation(NctsDepartureMovementHeader departureMovementHeader)
	{
		this.departureMovementHeader = Argument.NotNull(departureMovementHeader, nameof(departureMovementHeader));
		header = Argument.NotNull(departureMovementHeader.Header, nameof(departureMovementHeader.Header));
	}

	internal void ValidateRuleN0002(ZPropertyInfo propertyInfo)
	{
		if (propertyInfo.Value.IsEmpty)
		{
			return;
		}

		if (IsRuleN0002Applicable(departureMovementHeader))
		{
			propertyInfo.AddMessageError(ValidationRuleConfiguration.Messages.NR0002aMessage(propertyInfo.HumanReadableName));
		}
		else if (Has66YYSupportingDocument(departureMovementHeader) && !header.Principal.IsEmpty && !PrincipalHasAeoWithRegNumContainingAeocOrAeof(header.Principal))
		{
			propertyInfo.AddWarning(ValidationRuleConfiguration.Messages.NR0002bMessage(propertyInfo.HumanReadableName));
		}
	}

	static bool PrincipalHasAeoWithRegNumContainingAeocOrAeof(JobDocAddress principal)
	{
		var aeoCodes = principal.Organisation?.CustomsCodes
			.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator) ?? [];

		var hasAEOCodeWithAeocOrAeofSubstring = aeoCodes
			.Any(code => code.SecuredCustomsRegNo.Contains(AuthorisedEconomicOperatorCustomsSimplifications)
			|| code.SecuredCustomsRegNo.Contains(AuthorisedEconomicOperatorCustomsSimplificationsSecurityAndSafety));

		return hasAEOCodeWithAeocOrAeofSubstring;
	}

	static bool Has66YYSupportingDocument(NctsDepartureMovementHeader departureMovementHeader)
		=> departureMovementHeader.SupportingDocuments
			.Cast<AutoCusSupportingInfo>()
			.Any(d => d.CSI_Code.EqualsIgnoringCase("66YY"));

	public static bool IsRuleN0002Applicable(NctsDepartureMovementHeader departureMovementHeader)
		=> departureMovementHeader.ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider departureValidationDecider
			&& departureValidationDecider.IsRuleN0002Active
			&& (departureMovementHeader.IsContainerised || (Has66YYSupportingDocument(departureMovementHeader) && PrincipalHasAeoWithRegNumContainingAeocOrAeof(departureMovementHeader.Header.Principal)));

	const string AuthorisedEconomicOperatorCustomsSimplifications = "AEOC";

	const string AuthorisedEconomicOperatorCustomsSimplificationsSecurityAndSafety = "AEOF";

	ValidationRuleConfiguration ValidationRuleConfiguration => header?.Configuration.ValidationRuleConfiguration;

	readonly NctsDepartureMovementHeader departureMovementHeader;

	readonly NctsHeader header;
}
