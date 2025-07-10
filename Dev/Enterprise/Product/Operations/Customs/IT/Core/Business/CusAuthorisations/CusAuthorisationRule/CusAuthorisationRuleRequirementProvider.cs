using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business;

public class CusAuthorisationRuleRequirementProvider
{
	public CusAuthorisationRuleRequirementProvider(CusAuthorisationHeader authorisationHeader)
	{
		this.authorisationHeader = Argument.NotNull(authorisationHeader, nameof(authorisationHeader));
	}
	readonly CusAuthorisationHeader authorisationHeader;

	public CusAuthorisationRuleRequirement LocRuleRequirement => new CusAuthorisationRuleRequirement(CusAuthorisationRuleTypeList.Codes.Location, 0, 0)
		.AddAdditionalValidatorOnValue(locValue => ValidateLocRuleValueFromFormat(locValue));

	public CusAuthorisationRuleRequirement DocRuleRequirement => new CusAuthorisationRuleRequirement(ITCusAuthorisationRuleTypeList.Codes.Document, 0, 1);

	public CusAuthorisationRuleRequirement UseRuleRequiremnt => new CusAuthorisationRuleRequirement(ITCusAuthorisationRuleTypeList.Codes.Use, 1, 1);

	AdditionalValidatorResult ValidateLocRuleValueFromFormat(ZString locValue)
	{
		var validationResult = ZString.Empty;
		if (!locValue.IsEmpty
			&& authorisationHeader.CPH_Type != CusAuthorizationHeaderTypeList.Codes.TemporaryStorage
			&& !ValidationUtility.IsStartWithOneOrMoreDigitsAndEndWithAnUppercaseAlphabeticCharacter(locValue))
		{
			validationResult = ValidationCaptions.CusAuthorisations.ValueMustStartWithOneOrMoreDigitsAndEndWithOneUpperAlphabeticalCharacter;
		}
		return (validationResult, CargoWise.EntityFramework.NotificationType.MessageError);
	}
}
