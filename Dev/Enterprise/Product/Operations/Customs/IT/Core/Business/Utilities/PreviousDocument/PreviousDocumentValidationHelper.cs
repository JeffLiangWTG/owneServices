using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business;

public static class PreviousDocumentValidationHelper
{
	public static void CheckCSI_Procedure(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		MandatoryValidation.MessageErrorIfNotEntered(previousDocument.CSI_ProcedureInfo);
		ListValidation.MessageErrorIfInvalidCode(previousDocument.CSI_ProcedureInfo);
	}

	public static void CheckCSI_Code(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		MandatoryValidation.MessageErrorIfNotEntered(previousDocument.CSI_CodeInfo);
		ListValidation.MessageErrorIfInvalidCode(previousDocument.CSI_CodeInfo);
	}

	public static void CheckCSI_SubType(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		MandatoryValidation.MessageErrorIfNotEntered(previousDocument.CSI_SubTypeInfo);
		ListValidation.MessageErrorIfInvalidCode(previousDocument.CSI_SubTypeInfo);
	}

	public static void CheckCSI_ReferenceNumber2(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var fieldsSettings = GetSettings(previousDocument);
		if (fieldsSettings.IsReferenceNumber2Editable)
		{
			MandatoryValidation.MessageErrorIfNotEntered(previousDocument.CSI_ReferenceNumber2Info);
		}
		else
		{
			MandatoryValidation.MessageErrorIfIsEntered(previousDocument.CSI_ReferenceNumber2Info);
		}
	}

	public static void CheckCSI_DateOfIssue(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var fieldsSettings = GetSettings(previousDocument);
		if (fieldsSettings.IsDateOfIssueEditable)
		{
			MandatoryValidation.MessageErrorIfNotEntered(previousDocument.CSI_DateOfIssueInfo);
		}
		else
		{
			MandatoryValidation.MessageErrorIfIsEntered(previousDocument.CSI_DateOfIssueInfo);
		}
	}

	public static void CheckCSI_LineNo(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var fieldsSettings = GetSettings(previousDocument);
		if (fieldsSettings.IsLineNoEditable)
		{
			MandatoryValidation.MessageErrorIfNotEntered(previousDocument.CSI_LineNoInfo);
		}
		else
		{
			MandatoryValidation.MessageErrorIfIsEntered(previousDocument.CSI_LineNoInfo);
		}
	}

	public static void CheckCSI_Status(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var fieldsSettings = GetSettings(previousDocument);
		if (!fieldsSettings.IsStatusEditable)
		{
			MandatoryValidation.MessageErrorIfIsEntered(previousDocument.CSI_StatusInfo);
		}
	}

	public static void CheckCSI_CustomsOffice(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var fieldsSettings = GetSettings(previousDocument);
		if (fieldsSettings.IsCustomsOfficeEditable)
		{
			MandatoryValidation.MessageErrorIfNotEntered(previousDocument.CSI_CustomsOfficeInfo);
			ListValidation.MessageErrorIfInvalidCode(previousDocument.CSI_CustomsOfficeInfo);
		}
		else
		{
			MandatoryValidation.MessageErrorIfIsEntered(previousDocument.CSI_CustomsOfficeInfo);
		}
	}

	static PreviousDocumentFieldsInfo GetSettings(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var combinationsProvider = new PreviousDocumentCombinationsProvider(previousDocument);
		var settingsProvider = new PreviousDocumentSettingsProvider(combinationsProvider);
		return settingsProvider.GetSettings();
	}
}
