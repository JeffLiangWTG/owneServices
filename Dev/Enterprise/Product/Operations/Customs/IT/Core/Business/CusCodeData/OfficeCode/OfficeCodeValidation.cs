using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public class OfficeCodeValidation : EuOfficeCodeValidation
{
	public OfficeCodeValidation(EuOfficeCode parent) : base(parent)
	{
	}

	protected new OfficeCode Parent => (OfficeCode)base.Parent;

	protected override void CheckCY_Code()
	{
		base.CheckCY_Code();

		if (Declaration != null)
		{
			var customsOfficeData = Parent.CY_Data;
			var codePropertyInfo = Parent.CY_CodeInfo;

			ValidateRuleR906(customsOfficeData, codePropertyInfo);
			ValidateRuleR907(customsOfficeData, codePropertyInfo);
			ValidateRuleR0676(codePropertyInfo);
		}
	}

	protected override ZString ErrorMessageForInvalidCY_Data => GetErrorMessageForInvalidCY_Data();

	ZString GetErrorMessageForInvalidCY_Data()
	{
		var isUCC6AndIsExport = Declaration?.IsUCC6AndIsExport ?? false;
		if (isUCC6AndIsExport)
		{
			return GetErrorMessageForInvalidCY_DataForUCC6Export();
		}

		return base.ErrorMessageForInvalidCY_Data;
	}

	ZString GetErrorMessageForInvalidCY_DataForUCC6Export()
	{
		switch (Parent.CY_Code)
		{
			case EuOfficeCodesTypes.Codes.OfficeOfPresentation:
				return ValidationCaptions.OfficeCode.GetSelectedOfficeNotInTheListForPurpose(UniversalReferenceConstants.CommonResStrings.OfficeOfPresentationForCentralizedClearance);

			case EuOfficeCodesTypes.Codes.SupervisingOffice:
				return ValidationCaptions.OfficeCode.GetSelectedOfficeNotInTheListForPurpose(EuOfficeCodesTypes.Descriptions.SupervisingOffice);

			default:
				return base.ErrorMessageForInvalidCY_Data;
		}
	}

	void ValidateRuleR906(ZString customsOfficeData, ZPropertyInfo propertyInfo)
	{
		if (IsParentValidOfficeOfTransit(customsOfficeData) && !IsAndorra(customsOfficeData) && IsCountryOfficeOfDestinationAndorra())
		{
			propertyInfo.AddMessageError(ValidationCaptions.OfficeCode.OfficeOfTransitMustBeAD);
		}

		bool IsCountryOfficeOfDestinationAndorra() => IsAndorra(Declaration.CustomsOffices.GetOfficeOfDestination()?.CY_Data ?? ZString.Empty);
		bool IsAndorra(ZString data) => data.Left(2) == Core.Constants.CountryCodes.Andorra;
	}

	void ValidateRuleR907(ZString customsOfficeData, ZPropertyInfo propertyInfo)
	{
		if (IsParentValidOfficeOfTransit(customsOfficeData) && IsCountryOfficeOfDestinationSanMarino() && !Parent.IsPartOfEuropeanUnion)
		{
			propertyInfo.AddMessageError(ValidationCaptions.OfficeCode.OfficeOfTransitMustBePartOfEU);
		}

		bool IsCountryOfficeOfDestinationSanMarino() => (Declaration.CustomsOffices.GetOfficeOfDestination()?.CY_Data ?? ZString.Empty).Left(2) == Core.Constants.CountryCodes.SanMarino;
	}

	void ValidateRuleR0676(ZPropertyInfo propertyInfo)
	{
		var declaration = Declaration;

		if (declaration.IsUCC6AndIsExport && IsParentOfficeOfPresentation)
		{
			var cclAuthorizationExists = declaration.CustomsEntryInstructions
				.Cast<CusEntryInstruction>()
				.SelectMany(x => x.CusAuthorizationUsages).HasAuthorizationOfType(CusAuthorizationHeaderTypeList.Codes.CentralizedClearance);

			if (!cclAuthorizationExists)
			{
				propertyInfo.AddMessageError(ValidationCaptions.OfficeCode.R0676_CCLCodeMustBePresentInEntryInstructionAuthorizationsType);
			}
		}
	}

	ZBool IsParentValidOfficeOfTransit(ZString customsOfficeData) => Parent.IsOfficeOfTransit && !customsOfficeData.IsEmpty;
	ZBool IsParentOfficeOfPresentation => Parent.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfPresentation;
	JobDeclaration Declaration => Parent.Declaration;
}
