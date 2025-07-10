using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.IT.Business.ITPreviousDocumentValidationMessages;

namespace Enterprise.Customs.IT.Business;

public static class ITPreviousDocumentValidationHelper
{
	public static string ValidateReferenceNumberFormatForNmrn(BusinessObjectFactory factory, ZString referenceNumber)
	{
		var parts = referenceNumber.Split('-');

		if (referenceNumber.ToString().Any(char.IsWhiteSpace))
		{
			return NmrnReferenceNumberValidationMessages.NoSpacesAreAllowed;
		}

		if (referenceNumber.Length != MrnLength && parts.Length == 1)
		{
			return NmrnReferenceNumberValidationMessages.MrnOrRegistration;
		}

		if (referenceNumber.Length == MrnLength
			&& parts.Length == 1
			&& parts[0].ToString().All(char.IsLetterOrDigit))
		{
			return string.Empty;
		}

		if (parts.Length < ReferenceNumberRequiredFieldsLength)
		{
			return NmrnReferenceNumberValidationMessages.EnterAllRequiredFields;
		}

		if (parts.Length > ReferenceNumberRequiredFieldsLength)
		{
			return NmrnReferenceNumberValidationMessages.MoreFieldsThanRequired;
		}

		if (parts[0].IsEmpty)
		{
			return NmrnReferenceNumberValidationMessages.ProcedureIsEmpty;
		}

		if (!parts[1].ToString().All(char.IsNumber))
		{
			return NmrnReferenceNumberValidationMessages.RegistrationIsNotNumeric;
		}

		if (parts[1].Length > RegistrationMaxLength)
		{
			return NmrnReferenceNumberValidationMessages.RegistrationIsMoreThan8Digits;
		}

		if (!int.TryParse(parts[2], out int year)
			|| year < 1900
			|| year > ZDateTime.Now.Year)
		{
			return NmrnReferenceNumberValidationMessages.YearOfIssuing;
		}

		if (parts.Length == ReferenceNumberRequiredFieldsLength)
		{
			var officeCode = parts[3].AddPrefixToNumber(Core.Constants.CountryCodes.Italy);
			if (officeCode.IsEmpty || !factory.GetItalyCustomsOfficeCodeDescriptionPairList().ContainsCode(officeCode))
			{
				return ValidationCaptions.PreviousDocument.CustomsOfficeCodeIsNotValid;
			}
		}

		return string.Empty;
	}

	const int MrnLength = 18;
	const int ReferenceNumberRequiredFieldsLength = 4;
	const int RegistrationMaxLength = 8;
}
