using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class Ucc6ExportActiveBorderGroupValidation
{
	public Ucc6ExportActiveBorderGroupValidation(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
		this.ucc6ExportActiveBorderGroup = new Ucc6ExportActiveBorderGroup(declaration);
	}

	readonly JobDeclaration declaration;
	readonly Ucc6ExportActiveBorderGroup ucc6ExportActiveBorderGroup;

	internal void ValidateIfActiveBorderGroupFieldsAreRequired(ZPropertyInfo property)
	{
		var areAllFieldsEmptyOrFilled = ucc6ExportActiveBorderGroup.AreAllFieldsEmptyOrFilled();
		if (!areAllFieldsEmptyOrFilled)
		{
			property.AddMessageError(ValidationCaptions.JobDeclaration.ActiveBorderGroupFieldsMustAllBeFilledOrMustAllBeEmpty);
		}

		ValidateRuleC0890IfApplicable(property);
	}

	#region Implementation

	void ValidateRuleC0890IfApplicable(ZPropertyInfo property)
	{
		if (declaration.IsTransitionPeriodAES30)
		{
			return;
		}

		var canApplyMandatoryValidationWhenEntryStyleExportNormal = ucc6ExportActiveBorderGroup.HasEntryStyleExportNormalAndHasAllowedProcedureCodes()
			&& !ucc6ExportActiveBorderGroup.HasEntryStyleExportNormalAndHasNotAllowedTransportMode();

		var canApplyMandatoryValidationWhenEntryStyleExportToSpecialTerritory = ucc6ExportActiveBorderGroup.HasEntryStyleExportToSpecialTerritoryAndHasAllowedProcedureCodes()
			&& !ucc6ExportActiveBorderGroup.HasEntryStyleExportToSpecialAndHasNotAllowedTransportMode();

		var isFieldBelongToTransportMediumAndIfAllOthersAreFilled = ucc6ExportActiveBorderGroup.IsPropertyInTransportMediumGroupAndAreOtherTransportMediumFieldsFilled(property.Name);

		if (!isFieldBelongToTransportMediumAndIfAllOthersAreFilled && (canApplyMandatoryValidationWhenEntryStyleExportNormal || canApplyMandatoryValidationWhenEntryStyleExportToSpecialTerritory))
		{
			var validationMessagePrefix = FormattableString.Invariant($"{ruleNumberC0890} ");
			MandatoryValidation.MessageErrorIfNotEntered(property, messagePrefix: validationMessagePrefix);
		}
	}

	const string ruleNumberC0890 = "[C0890]";

	#endregion
}
