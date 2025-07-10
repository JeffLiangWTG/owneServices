using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.Business;

public static class PreviousDocumentHelper
{
	public static bool IsReferenceReadOnly(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var fieldsInfo = GetSettings(previousDocument);
		return !fieldsInfo.IsReferenceNumberEditable;
	}

	public static bool IsReference2ReadOnly(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var fieldsInfo = GetSettings(previousDocument);
		return !fieldsInfo.IsReferenceNumber2Editable;
	}

	public static bool IsDateOfIssueReadOnly(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var fieldsInfo = GetSettings(previousDocument);
		return !fieldsInfo.IsDateOfIssueEditable;
	}

	public static bool IsLineNoReadOnly(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var fieldsInfo = GetSettings(previousDocument);
		return !fieldsInfo.IsLineNoEditable;
	}

	public static bool IsStatusReadOnly(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var fieldsInfo = GetSettings(previousDocument);
		return !fieldsInfo.IsStatusEditable;
	}

	public static bool IsCustomsOfficeReadOnly(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var fieldsInfo = GetSettings(previousDocument);
		return !fieldsInfo.IsCustomsOfficeEditable;
	}

	public static bool IsNetMassReadOnly(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var fieldsInfo = GetSettings(previousDocument);
		return !fieldsInfo.IsNetMassEditable;
	}

	public static bool IsGrossMassReadOnly(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var fieldsInfo = GetSettings(previousDocument);
		return !fieldsInfo.IsGrossMassEditable;
	}

	public static bool IsPackageQuantityReadOnly(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var fieldsInfo = GetSettings(previousDocument);
		return !fieldsInfo.IsPackageQuantityEditable;
	}

	public static void SetDefaultAndEmptyField(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		if (previousDocument != null)
		{
			var fieldsInfo = GetSettings(previousDocument);

			var csiCodeList = previousDocument.Lookups?.CodeList?.Cast<ICodeDescription>()?.Select(x => x.Code).ToList() ?? new List<string>();
			var subTypeList = previousDocument.Lookups?.SubTypeList?.GetAllCodes()?.ToList() ?? new List<string>();

			previousDocument.CSI_Code = GetFirstOrEmpty(previousDocument.CSI_Code, csiCodeList);
			previousDocument.CSI_SubType = GetFirstOrEmpty(previousDocument.CSI_SubType, subTypeList);
			previousDocument.CSI_ReferenceNumber = fieldsInfo.IsReferenceNumberEditable ? previousDocument.CSI_ReferenceNumber : ZString.Empty;
			previousDocument.CSI_ReferenceNumber2 = fieldsInfo.IsReferenceNumber2Editable ? previousDocument.CSI_ReferenceNumber2 : ZString.Empty;
			previousDocument.CSI_DateOfIssue = fieldsInfo.IsDateOfIssueEditable ? previousDocument.CSI_DateOfIssue : ZDateTime.Empty;
			previousDocument.CSI_LineNo = fieldsInfo.IsLineNoEditable ? previousDocument.CSI_LineNo : ZShort.Zero;
			previousDocument.CSI_Status = fieldsInfo.IsStatusEditable ? previousDocument.CSI_Status : ZString.Empty;
			previousDocument.CSI_CustomsOffice = fieldsInfo.IsCustomsOfficeEditable ? previousDocument.CSI_CustomsOffice : ZString.Empty;
			previousDocument.CSI_Quantity = fieldsInfo.IsNetMassEditable ? previousDocument.CSI_Quantity : ZDecimal.Zero;
			previousDocument.CSI_UnitOfQuantity = fieldsInfo.IsNetMassEditable ? previousDocument.CSI_UnitOfQuantity : ZString.Empty;
			previousDocument.CSI_Quantity3 = fieldsInfo.IsGrossMassEditable ? previousDocument.CSI_Quantity3 : ZDecimal.Zero;
			previousDocument.CSI_UnitOfQuantity3 = fieldsInfo.IsGrossMassEditable ? previousDocument.CSI_UnitOfQuantity3 : ZString.Empty;
			previousDocument.CSI_PackQty = fieldsInfo.IsPackageQuantityEditable ? previousDocument.CSI_PackQty : ZInt.Zero;
			previousDocument.CSI_PackType = fieldsInfo.IsPackageQuantityEditable ? previousDocument.CSI_PackType : ZString.Empty;
		}
	}

	static ZString GetFirstOrEmpty(ZString currentValue, List<string> valuesList)
	{
		var value = ZString.Empty;
		if (valuesList.Contains(currentValue))
		{
			value = currentValue;
		}
		else if (valuesList.Count == 1)
		{
			value = valuesList.First();
		}
		return value;
	}

	public static TariffFormatter GetTariffFormatter()
	{
		return TariffFormatter.New(Core.Constants.CountryCodes.Italy);
	}

	public static string GetImportWrapperGroupKey(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		return new ZStringBuilder()
			.Append(previousDocument.CSI_Code)
			.Append(previousDocument.CSI_Procedure)
			.Append(previousDocument.CSI_DateOfIssue.ToItalianShortDateString())
			.Append(previousDocument.CSI_ReferenceNumber)
			.Append(previousDocument.CSI_ReferenceNumber2)
			.Append(previousDocument.CSI_LineNo.ToString())
			.Append(previousDocument.CSI_CustomsOffice)
			.Append(previousDocument.CSI_PackType)
			.Append(previousDocument.CSI_UnitOfQuantity)
			.Append(previousDocument.CSI_UnitOfQuantity3)
			.ToStringWithDelimiterBetweenAppends(",");
	}

	static PreviousDocumentFieldsInfo GetSettings(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		var combinationsProvider = new PreviousDocumentCombinationsProvider(previousDocument);
		var settingsProvider = new PreviousDocumentSettingsProvider(combinationsProvider);
		return settingsProvider.GetSettings();
	}
}
