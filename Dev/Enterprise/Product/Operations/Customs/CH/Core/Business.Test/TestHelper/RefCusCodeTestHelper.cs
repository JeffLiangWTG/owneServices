using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business.Testing;

public static class RefCusCodeTestHelper
{
	internal const string ValidPermitAuthorityCode = "12";
	internal const string ValidPermitAuthorityCodePast = "1X";
	internal const string InvalidPermitAuthorityCode = "45";

	internal const string ValidPermitTypeCode = "23";
	internal const string ValidPermitTypeCodePast = "2X";
	internal const string InvalidPermitTypeCode = "56";

	internal const string ValidCITESCommodityTypeListCode = "120";
	internal const string ValidCITESCommodityTypeListCodeDescription = "Code120";
	internal const string InvalidCITESCommodityTypeListCode = "888";

	internal const string ValidCITESScientificNameListCode = "220";
	internal const string ValidCITESScientificNameListCodeDescription = "Code220";
	internal const string InvalidCITESScientificNameListCode = "888";

	public const string ValidPreviousDocumentsListExport = "AA";
	public const string ValidPreviousDocumentsListExportPast = "AX";
	public const string ValidPreviousDocumentsListImport = "BB";
	public const string ValidPreviousDocumentsListImportPast = "BX";
	public const string InvalidPreviousDocumentsListExport = "T1";
	public const string InvalidPreviousDocumentsListImport = "T2";

	internal const string ValidSupportingDocument100 = "100";
	internal const string InvalidSupportingDocument = "199";
	internal const string ValidSupportingDocument101 = "101";
	internal const string ValidSupportingDocument102 = "102";
	internal const string ValidSupportingDocument103 = "103";

	internal const string ValidAdditionalInformationCode = UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic;
	internal const string ValidAdditionalInformationCode_ImportOnly = UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic;
	internal const string ValidAdditionalInformationCode_ImportOnly_Expired = "2X";
	internal const string ValidAdditionalInformationCode_ExportOnly = UniversalReferenceConstants.AdditionalInformationTypeCodes.ExportCodeMineralOil;
	internal const string InvalidAdditionalInformationCode = "99";

	internal const string ValidFreeZoneTrafficCode = "61";
	internal const string InvalidFreeZoneTrafficCode = "99";

	internal const string ValidBorderZoneTrafficCode = "60";
	internal const string InvalidBorderZoneTrafficCode = "99";

	internal const string ValidExportCodeMineralOil = "10";
	internal const string InvalidExportCodeMineralOil = "99";

	internal const string ValidVehicleModelNameCode = "879";
	internal const string ValidVehicleModelNameCodePast = "87X";
	internal const string InvalidVehicleModelNameCode = "XXX";

	internal const string ValidChargeTypeCode = "A00";
	internal const string InvalidChargeTypeCode = "XXX";

	internal const string ValidCustomsUnitOfQuantityCode = "KG";
	internal const string InvalidCustomsUnitOfQuantityCode = "YY";

	internal const string ValidPrimaryPreferenceCode = "NT";
	internal const string InvalidPrimaryPreferenceCode = "XX";

	internal const string ValidImportProcedureCode = "01";
	internal const string ValidExportProcedureCode = "02";
	internal const string InvalidProcedureCode = "99";

	internal const string ValidPermitObligationCode = "1";
	internal const string InvalidPermitObligationCode = "9";

	internal const string ValidNonCustomsLawObligationCode = UniversalReferenceConstants.NonCustomsLawObligationCodes.Needed;
	internal const string InvalidNonCustomsLawObligationCode = "9";

	internal const string ValidStorageTypeCode = "1";
	internal const string InvalidStorageTypeCode = "9";

	internal const string ValidStyleListCode = "01";
	internal const string ProvisionalStyleListCode = "02";
	internal const string InvalidStyleListCode = "XX";

	internal const string ValidEntrySubStyleListCode = "01";
	internal const string ReservationEntrySubStyleListCode = "02";
	internal const string InalidEntrySubStyleListCode = "XX";

	internal const string ValidDeclarationReasonCode = "1";
	internal const string InvalidDeclarationReasonCode = "X";

	public const string ValidCorrectionReasonTypeCode = "99";
	public const string InvalidCorrectionReasonTypeCode = "XX";

	public const string EntryStatusCode = "123";
	public const string EntryStatusDescription = "english description";

	internal const string ValidCountryOfOriginCode = "CH";
	internal const string InvalidCountryOfOriginCode = "XX";

	internal const string ValidDirectTransportationCountry = "KR";
	internal const string InvalidDirectTransportationCountry = "AU";

	internal const string UNPKGCodeAttributeYes = "IN";
	internal const string UNPKGCodeAttributeNo = "1B";

	public const string UNPKGCodeNoBulk = "P1";
	public const string UNPKGCodeWithBulkYes = "P2";
	public const string UNPKGCodeWithBreakBulkYes = "P3";

	internal const string ValidImportSupportingDocumentCode = "OZLNR";
	internal const string ValidImportSupportingDocumentCodePast = "OZXXX";
	internal const string ValidExportSupportingDocumentCode = "C641";
	internal const string ExportSupportingDocumentCodeWithReferenceY = "862";
	internal const string ExportSupportingDocumentCodeWithReferenceN = "863";
	internal const string ExportSupportingDocumentCodeWithIssuingDateY = "865";
	internal const string ExportSupportingDocumentCodeWithIssuingDateN = "866";
	internal const string InvalidSupportingDocumentCode = "9999";
	internal const string OriginDocument = "861";
	internal const string NoOriginDocument = "955";

	internal const string ValidTransportDocumentCode = "N952";
	internal const string TransportDocumentCodeBorderau = "N785";
	internal const string TransportDocumentCodeCarnetTIR = "N952";
	internal const string TransportDocumentCodeCarnetATA = "N955";
	internal const string InvalidTransportDocumentCode = "9999";

	internal const string ValidClearanceLocation_CustomsOffice = "1";
	internal const string ValidClearanceLocation_ImportOnly = "2";
	internal const string ValidClearanceLocation_ExportOnly = "3";
	internal const string InvalidClearanceLocation = "9";

	internal const string ValidNonCustomsLawTypeCode = "26";
	internal const string InvalidNonCustomsLawTypeCode = "XXX";

	internal const string ValidPermitItemDetailsKeyCode = "1";
	internal const string InvalidPermitItemDetailsKeyCode = "5";

	internal const string ValidWarehouseTypeCode = "1";
	internal const string InvalidWarehouseTypeCode = "3";

	internal const string ValidRefundTypeCode = "1";
	internal const string InvalidRefundTypeCode = "X";

	internal const string ValidSpecificCircumstanceIndicatorCode = "A20";
	internal const string InvalidSpecificCircumstanceIndicatorCode = "ABC";

	public const string ValidDocumentTypeCode = "1";
	public const string InvalidDocumentTypeCode = "9";

	public const string ValidEComplaintHeaderField = "HeaderField";
	public const string ValidEComplaintLineField = "LineField";
	public const string InvalidEComplaintField = "InvalidField";

	internal const string ValidNotifyCustomsOfficeCode = "CH001801";
	internal const string ValidNotifyCustomsOfficeCodePast = "CH00180X";
	internal const string InvalidNotifyCustomsOfficeCode = "DE001801";

	internal const string ValidSimpleCode = "0";
	internal const string ValidSimpleCodePast = "X";
	internal const string InvalidSimpleCode = "A";

	internal const string ValidInputControlCode = "1";
	internal const string InvalidInputControlCode = "0";

	public const string ValidNextProcedureCode = "1";
	public const string InvalidNextProcedureCode = "X";

	internal const string ValidExportAddDocAdditionalInformationCodeItemLevel = "ABCD";
	internal const string ValidExportAddDocAdditionalInformationCodeHeaderLevel = "DCBA";
	internal const string InvalidExportAddDocAdditionalInformationCodeWithWrongAttributeValue = "EFGH";
	internal const string InvalidExportAddDocAdditionalInformationCode = "IJKM";

	internal const string RestrictionCode1 = "100";
	internal const string RestrictionCode2 = "200";
	internal const string RestrictionCodeWithoutException = "300";
	internal const string InvalidRestrictionCode = "900";
	internal const string RestrictionExceptionCodeFor1 = "1";
	internal const string RestrictionExceptionCodeFor1And2 = "2";
	internal const string RestrictionExceptionCodeForNone = "3";
	internal const string RestrictionExceptionCodeOther = "999";
	internal const string InvalidRestrictionExceptionCode = "9";

	internal const string AdditionalInformationWithRestrictionCode_N1004 = "N1004";
	internal const string AdditionalInformationWithRestrictionCode_B1001 = "B1001";
	internal const string AdditionalInformationWithoutRestrictionCode_B1004 = "B1004";
	internal const string InvalidAdditionalInformationWithRestrictionCode = "X9999";
	internal const string AdditionalInformationRestrictionCode = "500";

	internal const string AdditionalInformationLinkedCodeType_N5004 = "N5004";
	internal const string AdditionalInformationLinkedCodeType_Invalid = "X9999";

	internal const string AdditionalInformationLinkedCode_N5004_1 = "5004_1";
	internal const string AdditionalInformationLinkedCode_N5004_2 = "5004_2";

	internal const string CEIStyleDefaultDescriptionEXP = "Ordinary";
	internal const string CEIStyleDefaultValuenEXP = UniversalReferenceConstants.InputControlCodes.Ordinary;
	internal const string CEIStyle1DescriptionEXP = "Simplified";
	internal const string CEIStyle1EXP = "1";

	internal const string ProcedureCodeDefaultDescriptionExp = "Export from free circulation";
	internal const string ProcedureCodeDefaultValueExp = "20";
	internal const string ProcedureCode41DescriptionExp = "Re-Export after inward processing";
	internal const string ProcedureCode41Exp = "41";
	internal const string ProcedureCode50DescriptionExp = "Outward processing (temporary exportation of goods for treatment, processing, and repair)";
	internal const string ProcedureCode50Exp = "50";

	internal const string CountryInEUAndCL010CountryList = "ES";
	internal const string CountryNotInEU = "AU";

	internal const string ValidTBSGACode = "05";
	internal const string InvalidTBSGACode = "96";

	internal const string ValidTBSGBCode = "02";
	internal const string InvalidTBSGBCode = "98";

	internal const string ValidTBSGCCode = "03";
	internal const string InvalidTBSGCCode = "97";

	internal const string ValidTBSGDCode = "01";
	internal const string InvalidTBSGDCode = "99";

	internal const string ValidTBSGECode = "01";
	internal const string InvalidTBSGECode = "99";

	internal const string ValidTBMGCode = "1";
	internal const string InvalidTBMGCode = "99";

	internal const string RestrictionCodeWithPermitNumberAllowedAttributeY = "101";
	internal const string RestrictionCodeWithPermitNumberAllowedAndPermitExceptionReasonAttributesY = "102";
	internal const string RestrictionCodeWithPermitNumberAllowedAndPermitExceptionReasonAttributesN = "103";

	internal const string RestrictionCodeWithAdditionalInformationAttributeY = "500";
	internal const string RestrictionCodeWithAdditionalInformationAttributeN = "999";

	internal const string ValidGIDNMCode = "1";
	internal const string ValidGIDNMCodePast = "X";
	internal const string InvalidGIDNMCode = "99";

	internal const string ValidDC44ICode = "1";
	internal const string ValidDC44ICodePast = "X";
	internal const string InvalidDC44ICode = "99";

	internal static void CreatePermitAuthorityCodeList(BusinessObjectFactory factory)
	{
		const string codeType = "PRMAU";

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Permit, codeType, codeType, Core.Constants.CountryCodes.Switzerland);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidPermitAuthorityCode, nameof(ValidPermitAuthorityCode), new ZDateTime(2024, 01, 01), ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidPermitAuthorityCodePast, nameof(ValidPermitAuthorityCodePast), ZDateTime.MinSmallDateTimeValue, new ZDateTime(2023, 12, 31));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidPermitAuthorityCode, nameof(InvalidPermitAuthorityCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	internal static void CreatePermitTypeCodeList(BusinessObjectFactory factory)
	{
		const string codeType = "PRMTY";

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Permit, codeType, codeType, Core.Constants.CountryCodes.Switzerland);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidPermitTypeCode, nameof(ValidPermitTypeCode), new ZDateTime(2024, 01, 01), ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidPermitTypeCodePast, nameof(ValidPermitTypeCodePast), ZDateTime.MinSmallDateTimeValue, new ZDateTime(2023, 12, 31));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidPermitTypeCode, nameof(InvalidPermitTypeCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	internal static void CreateCITESCommodityTypeList(BusinessObjectFactory factory)
	{
		const string codeType = "CITCT";
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidCITESCommodityTypeListCode, ValidCITESCommodityTypeListCodeDescription, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidCITESCommodityTypeListCode, InvalidCITESCommodityTypeListCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static void CreateCITESScientificNameList(BusinessObjectFactory factory)
	{
		const string codeType = "CITSN";
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidCITESScientificNameListCode, ValidCITESScientificNameListCodeDescription, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidCITESScientificNameListCode, InvalidCITESScientificNameListCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	public static void CreatePreviousDocumentsList(BusinessObjectFactory factory)
	{
		const string export = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection;
		const string import = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(export, export);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, export, ValidPreviousDocumentsListExport, nameof(ValidPreviousDocumentsListExport), new ZDateTime(2024, 01, 01), ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, export, ValidPreviousDocumentsListExportPast, nameof(ValidPreviousDocumentsListExportPast), ZDateTime.MinSmallDateTimeValue, new ZDateTime(2023, 12, 31));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, export, InvalidPreviousDocumentsListExport, nameof(InvalidPreviousDocumentsListExport), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeType(import, import);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, import, ValidPreviousDocumentsListImport, nameof(ValidPreviousDocumentsListImport), new ZDateTime(2024, 01, 01), ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, import, ValidPreviousDocumentsListImportPast, nameof(ValidPreviousDocumentsListImportPast), ZDateTime.MinSmallDateTimeValue, new ZDateTime(2023, 12, 31));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, import, InvalidPreviousDocumentsListImport, nameof(InvalidPreviousDocumentsListImport), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static void CreateSupportingDocumentsList(BusinessObjectFactory factory)
	{
		const string import = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(import, import);

		helper.CreateCusCodeList(Core.Constants.CountryCodes.Switzerland, import, ValidSupportingDocument100, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, import, ValidSupportingDocument101, nameof(ValidSupportingDocument101), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeList.Attributes.GSPCertificate, "Y");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, import, ValidSupportingDocument102, nameof(ValidSupportingDocument102), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeList.Attributes.GSPCertificate, "Y");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, import, ValidSupportingDocument103, nameof(ValidSupportingDocument103), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeList.Attributes.GSPCertificate, "N");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, import, InvalidSupportingDocument, nameof(InvalidSupportingDocument), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeList.Attributes.GSPCertificate, "Y");

		factory.Save();
	}

	internal static void CreateAdditionalInformationCodes(BusinessObjectFactory factory)
	{
		const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CHAdditionalInformation;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, UniversalReferenceConstants.AdditionalInformationTypeCodes.WarehouseNumber, nameof(UniversalReferenceConstants.AdditionalInformationTypeCodes.WarehouseNumber), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic, nameof(UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic), new ZDateTime(2024, 01, 01), ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, RefCusCodeTestHelper.ValidAdditionalInformationCode_ImportOnly_Expired, nameof(ValidAdditionalInformationCode_ImportOnly_Expired), ZDateTime.MinSmallDateTimeValue, new ZDateTime(2023, 12, 31));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, UniversalReferenceConstants.AdditionalInformationTypeCodes.BorderZoneTraffic, nameof(UniversalReferenceConstants.AdditionalInformationTypeCodes.BorderZoneTraffic), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, codeType, UniversalReferenceConstants.AdditionalInformationTypeCodes.ExportCodeMineralOil, nameof(UniversalReferenceConstants.AdditionalInformationTypeCodes.ExportCodeMineralOil), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeList.Attributes.IsExports, UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidAdditionalInformationCode, nameof(InvalidAdditionalInformationCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static void CreateExportAddDocAdditionalInformationCodes(BusinessObjectFactory factory)
	{
		const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportAddDocAdditionalInformation;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, codeType, ValidExportAddDocAdditionalInformationCodeItemLevel, nameof(ValidExportAddDocAdditionalInformationCodeItemLevel), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeList.Attributes.Level, UniversalReferenceConstants.RefCusCodeList.AttributeValues.Item);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, codeType, ValidExportAddDocAdditionalInformationCodeHeaderLevel, nameof(ValidExportAddDocAdditionalInformationCodeItemLevel), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeList.Attributes.Level, UniversalReferenceConstants.RefCusCodeList.AttributeValues.Header);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, codeType, InvalidExportAddDocAdditionalInformationCodeWithWrongAttributeValue, nameof(InvalidExportAddDocAdditionalInformationCodeWithWrongAttributeValue), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeList.Attributes.Level, UniversalReferenceConstants.RefCusCodeList.AttributeValues.No);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidExportAddDocAdditionalInformationCode, nameof(InvalidExportAddDocAdditionalInformationCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static void CreateFreeZoneTrafficCodes(BusinessObjectFactory factory)
	{
		const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FreeZoneTraffic;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidFreeZoneTrafficCode, nameof(ValidFreeZoneTrafficCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidFreeZoneTrafficCode, nameof(InvalidFreeZoneTrafficCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static void CreateBorderZoneTrafficCodes(BusinessObjectFactory factory)
	{
		const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BorderZoneTraffic;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidBorderZoneTrafficCode, nameof(ValidBorderZoneTrafficCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidBorderZoneTrafficCode, nameof(InvalidBorderZoneTrafficCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static void CreateExportCodeMineralOil(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.ExportCodeMineralOil;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidExportCodeMineralOil, nameof(ValidExportCodeMineralOil), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidExportCodeMineralOil, nameof(InvalidExportCodeMineralOil), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static void CreateTBSGACodeList(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.TBSGA;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidTBSGACode, nameof(ValidTBSGACode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidTBSGACode, nameof(InvalidTBSGACode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static void CreateTBSGBCodeList(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.TBSGB;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidTBSGBCode, nameof(ValidTBSGBCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidTBSGBCode, nameof(InvalidTBSGBCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static void CreateTBSGCCodeList(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.TBSGC;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidTBSGCCode, nameof(ValidTBSGCCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidTBSGCCode, nameof(InvalidTBSGCCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static void CreateTBSGDCodeList(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.TBSGD;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidTBSGDCode, nameof(ValidTBSGDCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidTBSGDCode, nameof(InvalidTBSGDCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static void CreateTBSGECodeList(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.TBSGE;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidTBSGECode, nameof(ValidTBSGECode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidTBSGECode, nameof(InvalidTBSGECode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static void CreateTBMGCodeList(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.TBMG;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidTBMGCode, nameof(ValidTBMGCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidTBMGCode, nameof(InvalidTBMGCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static void CreateVehicleModelNameCodeList(BusinessObjectFactory factory)
	{
		const string codeType = "VEHMC";

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Permit, codeType, codeType, Core.Constants.CountryCodes.Switzerland);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidVehicleModelNameCode, ValidVehicleModelNameCode, new ZDateTime(2024, 01, 01), ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidVehicleModelNameCodePast, ValidVehicleModelNameCodePast, ZDateTime.MinSmallDateTimeValue, new ZDateTime(2023, 12, 31));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidVehicleModelNameCode, InvalidVehicleModelNameCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	internal static void CreatePrimaryPreferenceCodeList(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreatePreferenceForCountry(ValidPrimaryPreferenceCode, nameof(ValidPrimaryPreferenceCode), Core.Constants.CountryCodes.Switzerland);
		helper.CreatePreferenceForCountry(InvalidPrimaryPreferenceCode, nameof(InvalidPrimaryPreferenceCode), Core.Constants.CountryCodes.Germany);
	}

	internal static void CreateProcedureCodeList(BusinessObjectFactory factory, string[] languages = null)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		var importProcedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Switzerland, ZString.Empty, ValidImportProcedureCode, ZString.Empty, ZString.Empty, nameof(ValidImportProcedureCode), "IMP");
		var exportProcedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Switzerland, ZString.Empty, ValidExportProcedureCode, ZString.Empty, ZString.Empty, nameof(ValidExportProcedureCode), "EXP");
		helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, ZString.Empty, InvalidProcedureCode, ZString.Empty, ZString.Empty, nameof(ValidImportProcedureCode), "IMP");

		if (languages != null)
		{
			foreach (var language in languages.Select(l => l.Substring(0, 2)))
			{
				helper.CreateRefCusProcedureLanguage(importProcedure, language, importProcedure.ZZ6_Description + language);
				helper.CreateRefCusProcedureLanguage(exportProcedure, language, exportProcedure.ZZ6_Description + language);
			}
		}
	}

	internal static void CreatePermitObligationCodeList(BusinessObjectFactory factory, string[] languages = null)
	{
		const string codeType = "PRMOC";
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		var codeList1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidPermitObligationCode, nameof(ValidPermitObligationCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var codeList2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidPermitObligationCode, nameof(InvalidPermitObligationCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		AddCodeListLanguages(helper, languages, codeList1, codeList2);
		factory.Save();
	}

	internal static void CreateNonCustomsLawObligationCodeList(BusinessObjectFactory factory, string[] languages = null)
	{
		const string codeType = "NCLOC";

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, UniversalReferenceConstants.NonCustomsLawObligationCodes.NotPossible, nameof(UniversalReferenceConstants.NonCustomsLawObligationCodes.NotPossible), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, UniversalReferenceConstants.NonCustomsLawObligationCodes.Needed, nameof(UniversalReferenceConstants.NonCustomsLawObligationCodes.Needed), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var code3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, UniversalReferenceConstants.NonCustomsLawObligationCodes.NotNeededAccordingDeclarant, nameof(UniversalReferenceConstants.NonCustomsLawObligationCodes.NotNeededAccordingDeclarant), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidNonCustomsLawObligationCode, nameof(InvalidNonCustomsLawObligationCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		AddCodeListLanguages(helper, languages, code1, code2, code3);
		factory.Save();
	}

	internal static void CreateStorageTypeCodeList(BusinessObjectFactory factory)
	{
		const string codeType = "STGCD";
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidStorageTypeCode, nameof(ValidStorageTypeCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidStorageTypeCode, nameof(InvalidStorageTypeCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static void CreateChargeTypeCodeList(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var countryCode = Core.Constants.CountryCodes.Switzerland;
		var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(countryCode, parent: eunId);
		helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RateTypes.Duty, "Duty");
		var duty = helper.CreateNewOrGetExistingRateType(countryCode, Constants.RateTypes.Duty, "Duty");
		helper.LoadOrCreateNewCusRateCode(factory, ValidChargeTypeCode, duty.PK);
		factory.Save();
	}

	internal static void CreateRateCodeList(BusinessObjectFactory factory, string rateType, string[] rateCodes, bool includeTranslations = false)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var countryCode = Core.Constants.CountryCodes.Switzerland;

		if (includeTranslations)
		{
			helper.CreateOrGetLanguage(SwissCustomsLanguageList.Codes.French, nameof(SwissCustomsLanguageList.Codes.French));
		}

		helper.CreateNewOrGetExistingDataGrouping(countryCode);
		var cusRateType = helper.CreateNewOrGetExistingRateType(countryCode, rateType);

		foreach (var rateCode in rateCodes)
		{
			var rateCodeView = helper.CreateCusRateCode(factory, rateCode, cusRateType.PK, description: $"{rateType}{rateCode}");
			if (includeTranslations)
			{
				var rateCodeLanguage = helper.LoadOrCreateNewCusRateCodeLanguage(factory, rateCodeView, SwissCustomsLanguageList.Codes.French);
				rateCodeLanguage.ZXC_Description = $"{rateCodeView.ZY1_Description}{rateCodeLanguage.ZXC_ZX6_NKLanguage}";
			}
		}
		factory.Save();
	}

	internal static void CreateCustomsUnitOfQuantityCodeList(BusinessObjectFactory factory)
	{
		const string codeType = "CUSUQ";

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidCustomsUnitOfQuantityCode, nameof(ValidCustomsUnitOfQuantityCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidCustomsUnitOfQuantityCode, nameof(InvalidCustomsUnitOfQuantityCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	internal static void CreateEnstyCodeList(BusinessObjectFactory factory)
	{
		const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty;
		const string attributeName = UniversalReferenceConstants.RefCusCodeList.Attributes.IsExports;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, codeType, codeType, Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, attributeName, codeType, Core.Constants.CountryCodes.Switzerland);

		var definitiv = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "01", "Definitiv", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "02", "Provisorisch", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "03", "Andere Zwischenveranlagungen", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "04", "VEV", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var provisional = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "05", "Provisorische Sammelanmeldung", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "06", "easy", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "99", "Andere", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		definitiv.Attributes.AddNew(attributeName, Customs.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
		provisional.Attributes.AddNew(attributeName, Customs.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);

		factory.Save();
	}

	public static void CreateEnsubCodeList(BusinessObjectFactory factory)
	{
		const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, codeType, codeType, Core.Constants.CountryCodes.Switzerland);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "01", "Gestellung", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "02", "Voranmeldung", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "03", "Nachtr�gliche Anmeldung", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "05", "Vorausanmeldung f�r Zu-/Beilad", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "99", "Andere", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	internal static void CreatePreasCodeList(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.DeclarationReason;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, codeType, codeType, Core.Constants.CountryCodes.Switzerland);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidDeclarationReasonCode, "Proof of origin for EU countries", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "2", "Proof of origin for EFTA countries", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "3", "Proof of origin for countries with free trade agreement", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "4", "Proof of origin for developing countries", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	public static void CreateCustomsStatusCodeListAndFrenchLanguage(BusinessObjectFactory factory)
	{
		const string codeType = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, "sample description");
		helper.CreateOrGetLanguage("FR", "French");
		factory.Save();

		var refCusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, EntryStatusCode, EntryStatusDescription, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeListLanguage(refCusCodeList, "FR", "description francaise");

		helper.CreateCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "456", "Additional testing code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	public static void CreateSelectionResultCodeListAndFrenchLanguage(BusinessObjectFactory factory, string code = "456", string description = "Additional testing code")
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.SeletionResult;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateOrGetLanguage("FR", "French");
		factory.Save();
		helper.CreateNewOrGetExistingCusCodeType(codeType, "sample description");
		var refCusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, EntryStatusCode, EntryStatusDescription, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, code, description, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeListLanguage(refCusCodeList, "FR", "description francaise");
		factory.Save();
	}

	internal static void CreateCountryOfOriginsList(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode).ZZZ_ZZZ_Grouping = eun.PK;
		var group1011 = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, "1011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var group1012 = helper.CreateTradeGroup(Core.Constants.CountryCodes.Switzerland, "1012", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.AddCountry(group1011, Core.Constants.CountryCodes.France, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.AddCountry(group1011, Core.Constants.CountryCodes.Germany, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.AddCountry(group1012, Core.Constants.CountryCodes.Switzerland, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.AddCountry(group1012, Core.Constants.CountryCodes.Liechtenstein, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

		factory.Save();
	}

	internal static void CreateOriginDocumentCodes(BusinessObjectFactory factory)
	{
		string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName("OriginDocument", "Origin Documents Codes", codeType, Core.Constants.CountryCodes.Switzerland);

		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "3", "Qualitätszeugnis", new ZDateTime(2024, 01, 01), ZDateTime.MaxSmallDateTimeValue, "OriginDocument", "Y");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, OriginDocument, "Ursprungszeugnis", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "OriginDocument", "Y");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "862", "Ursprungserklärung", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "OriginDocument", "Y");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "865", "Certificate of origin form GSP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "OriginDocument", "Y");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "866", "Statement on Origin", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "OriginDocument", "Y");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "872", "Declaration of origin transitional rules", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "OriginDocument", "Y");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "873", "Declaration of origin EUR-MED", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "OriginDocument", "Y");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "874", "Movement Certificate EUR-MED", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "OriginDocument", "Y");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "954", "EUR.1 Warenverkehrsbescheinigung", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "OriginDocument", "Y");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "964", "Movement Certificate EUR.1 transitional rules", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "OriginDocument", "Y");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, NoOriginDocument, "ATA Carnet", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	internal static void CreateDirectTransportationCodeList(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.DirectTransportationCountry;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidDirectTransportationCountry, "Korea", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidDirectTransportationCountry, "Australia", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	public static void CreateCorrectionReasonTypeList(BusinessObjectFactory factory, string codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.CorrectionReason, string[] languages = null)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland, parent: grouping);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		var code1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidCorrectionReasonTypeCode, nameof(ValidCorrectionReasonTypeCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "2", "Formelle Überprüfung", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		AddCodeListLanguages(helper, languages, code1, code2);
		factory.Save();
	}

	internal static void CreateSupportingDocumentCodes(BusinessObjectFactory factory)
	{
		const string dataGrouping = Core.Constants.CountryCodes.Switzerland;
		const string otherDataGrouping = Core.Constants.CountryCodes.Germany;
		const string importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
		const string exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
		helper.CreateNewOrGetExistingCusCodeType(importCodeType, nameof(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection), dataGrouping);
		helper.CreateNewOrGetExistingCusCodeList(dataGrouping, importCodeType, ValidImportSupportingDocumentCode, nameof(ValidImportSupportingDocumentCode), new ZDateTime(2024, 01, 01), ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(dataGrouping, importCodeType, ValidImportSupportingDocumentCodePast, nameof(ValidImportSupportingDocumentCodePast), ZDateTime.MinSmallDateTimeValue, new ZDateTime(2023, 12, 31));
		helper.CreateNewOrGetExistingCusCodeList(otherDataGrouping, importCodeType, InvalidSupportingDocumentCode, nameof(InvalidSupportingDocumentCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeType(exportCodeType, nameof(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection), dataGrouping);
		helper.CreateNewOrGetExistingCusCodeList(dataGrouping, exportCodeType, ValidExportSupportingDocumentCode, nameof(ValidExportSupportingDocumentCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(otherDataGrouping, exportCodeType, InvalidSupportingDocumentCode, nameof(InvalidSupportingDocumentCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var code = helper.CreateNewOrGetExistingCusCodeList(dataGrouping, exportCodeType, ExportSupportingDocumentCodeWithReferenceY, nameof(ExportSupportingDocumentCodeWithReferenceY), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.Reference, UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes);
		code = helper.CreateNewOrGetExistingCusCodeList(dataGrouping, exportCodeType, ExportSupportingDocumentCodeWithReferenceN, nameof(ExportSupportingDocumentCodeWithReferenceN), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.Reference, UniversalReferenceConstants.RefCusCodeList.AttributeValues.No);
		code = helper.CreateNewOrGetExistingCusCodeList(dataGrouping, exportCodeType, ExportSupportingDocumentCodeWithIssuingDateY, nameof(ExportSupportingDocumentCodeWithIssuingDateY), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.IssuingDate, UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes);
		code = helper.CreateNewOrGetExistingCusCodeList(dataGrouping, exportCodeType, ExportSupportingDocumentCodeWithIssuingDateN, nameof(ExportSupportingDocumentCodeWithIssuingDateN), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.IssuingDate, UniversalReferenceConstants.RefCusCodeList.AttributeValues.No);
		factory.Save();
	}

	internal static void CreateTransportDocumentCodes(BusinessObjectFactory factory)
	{
		const string dataGrouping = Core.Constants.CountryCodes.Switzerland;
		const string codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.TransportDocumentType;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType, dataGrouping);
		helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, TransportDocumentCodeCarnetATA, nameof(TransportDocumentCodeCarnetATA), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, TransportDocumentCodeBorderau, nameof(TransportDocumentCodeBorderau), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, TransportDocumentCodeCarnetTIR, nameof(TransportDocumentCodeCarnetTIR), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	internal static void CreateClearanceLocation(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.ClearanceLocation;
		const string dataGrouping = Core.Constants.CountryCodes.Switzerland;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
		helper.CreateNewOrGetExistingCusCodeType(codeType, nameof(UniversalReferenceConstants.RefCusCodeList.EdecTypes.ClearanceLocation), dataGrouping);
		CreateCode(ValidClearanceLocation_CustomsOffice, true, true);
		CreateCode(ValidClearanceLocation_ImportOnly, true, false);
		CreateCode(ValidClearanceLocation_ExportOnly, false, true);
		factory.Save();

		void CreateCode(string codeValue, bool isImports, bool isExports)
		{
			var code = helper.CreateCusCodeList(dataGrouping, codeType, codeValue, codeValue, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.IsImports, isImports ? UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes : UniversalReferenceConstants.RefCusCodeList.AttributeValues.No);
			helper.CreateCusCodeListAttribute(code.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.IsExports, isExports ? UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes : UniversalReferenceConstants.RefCusCodeList.AttributeValues.No);
		}
	}

	public static void CreateUNPKGCodeList(BusinessObjectFactory factory)
	{
		var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes;
		var dataGroupingCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType, dataGroupingCode);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Bulk, nameof(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Bulk), codeType, dataGroupingCode);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.BreakBulk, nameof(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.BreakBulk), codeType, dataGroupingCode);

		helper.CreateNewOrGetExistingCusCodeList(dataGroupingCode, codeType, UNPKGCodeNoBulk, nameof(UNPKGCodeNoBulk), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		var code = helper.CreateNewOrGetExistingCusCodeList(dataGroupingCode, codeType, UNPKGCodeWithBulkYes, nameof(UNPKGCodeWithBulkYes), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		code.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Bulk, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Bulk);

		code = helper.CreateNewOrGetExistingCusCodeList(dataGroupingCode, codeType, UNPKGCodeWithBreakBulkYes, nameof(UNPKGCodeWithBreakBulkYes), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		code.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.BreakBulk, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.BreakBulk);

		factory.Save();
	}

	internal static void CreateSpecificCircumstanceIndicatorList(BusinessObjectFactory factory)
	{
		var codeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.N0296;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidSpecificCircumstanceIndicatorCode, "Express consignments in the context of ext summary declarations", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "XXX", "Authorised economic operators AEO (during TP)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	internal static void CreateTobaccoLists(BusinessObjectFactory factory)
	{
		var dataGroupingCode = Core.Constants.CountryCodes.Switzerland;
		var helper = new UniversalReferenceTestDataHelper(factory);

		helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode);

		(string TypeCode, string Description, (string ListCode, string Description)[] List)[] data = new[] {
			(UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoMainGroup, nameof(UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoMainGroup),
				new[] {
						("1", "Cigars"),
						("2", "Cigarettes"),
						("3", "Cut Tobacco"),
						("4", "Assortment"),
						("5", "E-Cigarettes"),
					}),
			(UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoSubGroupCigars, nameof(UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoSubGroupCigars),
				new[] {
						("01", "Cheroot"),
						("02", "Zigarillos"),
						("03", "Kiel"),
						("04", "Longfiller"),
						("05", "Mediumfiller"),
						("06", "Shortfiller"),
						("07", "Virginia / Brissago"),
						("08", "Toscani (entire)"),
						("09", "Toscanelli"),
						("10", "Beedies"),
						("11", "Blunts"),
					}),
			(UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoSubGroupCigarettes, nameof(UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoSubGroupCigarettes),
				new[] {
						("01", "Inland Tobacco"),
						("02", "Maryland"),
						("03", "American Blend"),
						("04", "Orient"),
						("05", "European Blend"),
						("06", "Virginia"),
						("07", "Other"),
					}),
			(UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoSubGroupTobacco, nameof(UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoSubGroupTobacco),
				new[] {
						("01", "Pipe Tobacco"),
						("02", "Fine cut Tobacco"),
						("03", "Water pipe Tobacco"),
						("04", "Chewing Tobacco"),
						("05", "Snuff"),
						("06", "Cigar cuttings"),
					}),
			(UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoSubGroupAssortment, nameof(UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoSubGroupAssortment),
				new[] {
						("01", "Cigars"),
						("02", "Cigarettes"),
						("03", "Cut Tobacco"),
					}),
			(UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoSubGroupECigarettes, nameof(UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoSubGroupECigarettes),
				new[] {
						("01", "Eisposable E-Cigarettes"),
						("02", "Refillable E-Cigarettes"),
					}),
			(UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoBrand, nameof(UniversalReferenceConstants.RefCusCodeList.EdecTypes.TobaccoBrand),
				new[] {
						("1", "Kentucky"),
						("2", "Maryland"),
						("3", "Virginia dark"),
						("4", "Virginia light"),
						("5", "Burley"),
					}),
		};

		foreach (var listType in data)
		{
			helper.CreateNewOrGetExistingCusCodeType(listType.TypeCode, listType.Description, dataGroupingCode);
			foreach (var listItem in listType.List)
			{
				var minDate = ZDateTime.MinSmallDateTimeValue;
				var maxDate = ZDateTime.MaxSmallDateTimeValue;

				if (listItem.ListCode == "1" || listItem.ListCode == "01")
				{
					minDate = new ZDateTime(2024, 01, 01);
				}

				helper.CreateNewOrGetExistingCusCodeList(dataGroupingCode, listType.TypeCode, listItem.ListCode, listItem.Description, minDate, maxDate);
			}
		}

		factory.Save();
	}

	internal static void CreatePermitItemDetailsKeyCodeListAndFrenchLanguage(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.PermitItemDetailsKey;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, "sample description");
		helper.CreateOrGetLanguage("FR", "French");
		factory.Save();

		var refCusCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidPermitItemDetailsKeyCode, "Position number of permit", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "2", "Additional testing code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeListLanguage(refCusCodeList, "FR", "Numéro de position du permis");
		factory.Save();
	}

	internal static void CreateAdditionalCodes(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.AdditionalCodes;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		for (int n = 1; n < 5; n++)
		{
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, $"AC{n:000}", $"description AC{n:000}", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}
		factory.Save();
	}

	internal static void CreateNonCustomsLawTypeCodesList(BusinessObjectFactory factory)
	{
		const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NonCustomsLaw;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "26", "Cultural property", new ZDateTime(2024, 01, 01), ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "30", "PIC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "44", "Radioactive substances", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "66", "Waste (amber control procedure)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "669", "Increased controls LMR-RDA ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	internal static void CreateWarehouseCodeList(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.WarehouseType;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "1", "Bonded warehouse", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "2", "Interim storage abroad", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	public static void CreateDocumentTypeList(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.DocumentType;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, ValidDocumentTypeCode, nameof(ValidDocumentTypeCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, InvalidDocumentTypeCode, nameof(InvalidDocumentTypeCode), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	public static void CreateSelectionResultList(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.SeletionResult;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "1", "Free Without", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "2", "Free With", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "3", "Blocked", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	internal static void CreateRefundTypeList(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.RefundType;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, RefCusCodeTestHelper.ValidNextProcedureCode, "Refund", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "3", "Request for alcohol", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "4", "Request for at least 2 of the refund types 1 to 3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "5", "Beer tax refund", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "6", "Other refunds", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, RefCusCodeTestHelper.InvalidNextProcedureCode, "Invalid", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	public static void CreateEComplaintFieldNames(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.EdecTypes.EComplaintFields;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, codeType, ValidEComplaintHeaderField, nameof(ValidEComplaintHeaderField), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeList.Attributes.IsHeader, UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Switzerland, codeType, ValidEComplaintLineField, nameof(ValidEComplaintLineField), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, UniversalReferenceConstants.RefCusCodeList.Attributes.IsHeader, UniversalReferenceConstants.RefCusCodeList.AttributeValues.No);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "VATNumber", "Mehrwertsteuernummer", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "statisticalValue", "Statistischer Wert", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "correctionTerm", "Berichtigungsfrist", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	public static void CreateCustomsOfficesList(BusinessObjectFactory factory, string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CHCustomsOffice)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "CH001251", "Allschwil 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "CH001252", "Allschwil 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	public static void CreateTransportationTypeList(BusinessObjectFactory factory, string language = null)
	{
		const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TransportationType;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		if (language != null)
		{
			helper.CreateOrGetLanguage(language.Substring(0, 2), $"Language-{language}");
		}

		CreateCodeList("1", "Passenger car");
		CreateCodeList("2", "Truck");
		CreateCodeList("3", "Truck with standard trailer");
		CreateCodeList("4", "with semi-trailer");
		CreateCodeList("99", "Other");

		factory.Save();

		void CreateCodeList(string code, string description)
		{
			var codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, code, description, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			if (language != null)
			{
				helper.CreateCusCodeListLanguage(codeList, language.Substring(0, 2), $"{codeList.ZZD_Description} [{language.Substring(0, 2)}]");
			}
		}
	}

	public static void CreateInputControlList(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.InputControl;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, CEIStyle1EXP, CEIStyle1DescriptionEXP, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, CEIStyleDefaultValuenEXP, CEIStyleDefaultDescriptionEXP, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	public static void CreateNextProcedureList(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.NextProcedure;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "1", "Durchfuhr", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "2", "Zolllager", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "3", "Luftfracht", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, "4", "Postverkehr", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		factory.Save();
	}

	internal static void CreateRestrictionCodeLists(BusinessObjectFactory factory)
	{
		const string restrictionCodeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.Restrictions;
		const string exceptionCodeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1121;
		const string dataGrouping = Core.Constants.CountryCodes.Switzerland;
		const string otherGrouping = Core.Constants.CountryCodes.EuropeanUnion;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
		helper.CreateNewOrGetExistingDataGrouping(otherGrouping);
		helper.CreateNewOrGetExistingCusCodeType(restrictionCodeType, nameof(UniversalReferenceConstants.RefCusCodeList.PassarTypes.Restrictions), dataGrouping);
		helper.CreateNewOrGetExistingCusCodeType(exceptionCodeType, nameof(UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1121), dataGrouping);

		CreateRestrictionCode(RestrictionCode1);
		CreateRestrictionCode(RestrictionCode2);
		CreateRestrictionCode(RestrictionCodeWithoutException);
		CreateExceptionCode(RestrictionExceptionCodeFor1, new[] { RestrictionCode1 });
		CreateExceptionCode(RestrictionExceptionCodeFor1And2, new[] { RestrictionCode1, RestrictionCode2 });
		CreateExceptionCode(RestrictionExceptionCodeForNone, Array.Empty<string>());
		CreateExceptionCode(InvalidRestrictionExceptionCode, new[] { RestrictionCode1 }, grouping: otherGrouping);

		CreateRestrictionCode(InvalidRestrictionCode, grouping: otherGrouping);

		factory.Save();

		void CreateRestrictionCode(string codeValue, string grouping = null)
		{
			helper.CreateCusCodeList(grouping ?? dataGrouping, restrictionCodeType, codeValue, codeValue, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		void CreateExceptionCode(string codeValue, string[] restrictionCodes, string grouping = null)
		{
			var code = helper.CreateCusCodeList(grouping ?? dataGrouping, exceptionCodeType, codeValue, codeValue, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			foreach (var restrictionCode in restrictionCodes)
			{
				helper.CreateCusCodeListAttribute(code.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.PermitAuthority, restrictionCode);
			}
		}
	}

	internal static void CreateAdditionalInformationCodeRestrictions(BusinessObjectFactory factory, bool includeRestrictionCodes = false, bool includeLinkedCodeTypes = false, string[] additionalLinkedCodeValues = null)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1119;
		const string dataGrouping = Core.Constants.CountryCodes.Switzerland;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
		helper.CreateNewOrGetExistingCusCodeType(codeType, nameof(UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1119), dataGrouping);
		CreateCode(AdditionalInformationWithRestrictionCode_N1004, restrictionCode: AdditionalInformationRestrictionCode, linkedCodeType: AdditionalInformationLinkedCodeType_N5004);
		CreateCode(AdditionalInformationWithRestrictionCode_B1001, restrictionCode: AdditionalInformationRestrictionCode);
		CreateCode(AdditionalInformationWithoutRestrictionCode_B1004, AdditionalInformationWithoutRestrictionCode_B1004);

		if (includeLinkedCodeTypes)
		{
			CreateLinkedCode(AdditionalInformationLinkedCodeType_N5004, AdditionalInformationLinkedCode_N5004_1);
			CreateLinkedCode(AdditionalInformationLinkedCodeType_N5004, AdditionalInformationLinkedCode_N5004_2);
			if (additionalLinkedCodeValues != null)
			{
				foreach (var linkedCodeValue in additionalLinkedCodeValues)
				{
					CreateLinkedCode(AdditionalInformationLinkedCodeType_N5004, linkedCodeValue);
				}
			}
		}

		factory.Save();

		void CreateCode(string codeValue, string restrictionCode = null, string linkedCodeType = null)
		{
			var code = helper.CreateCusCodeList(dataGrouping, codeType, codeValue, codeValue, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			if (includeRestrictionCodes && restrictionCode != null)
			{
				helper.CreateCusCodeListAttribute(code.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.RestrictionCode, restrictionCode);
			}
			if (includeLinkedCodeTypes && linkedCodeType != null)
			{
				helper.CreateCusCodeListAttribute(code.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.LinkedCodeType, linkedCodeType);
				helper.CreateNewOrGetExistingCusCodeType(linkedCodeType, linkedCodeType, dataGrouping);
			}
		}

		void CreateLinkedCode(string codeType, string linkedCodeValue)
		{
			helper.CreateCusCodeList(dataGrouping, AdditionalInformationLinkedCodeType_N5004, linkedCodeValue, linkedCodeValue, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}
	}

	public static RefCusProcedure[] CreateProcedureList(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var ret = new RefCusProcedure[]
		{
			CreateRefCusProcedure(JobMessageTypeList.Codes.Export, Core.Constants.CountryCodes.Switzerland, ProcedureCodeDefaultDescriptionExp, ProcedureCodeDefaultValueExp),
			CreateRefCusProcedure(JobMessageTypeList.Codes.Export, Core.Constants.CountryCodes.Switzerland, ProcedureCode41DescriptionExp, ProcedureCode41Exp),
			CreateRefCusProcedure(JobMessageTypeList.Codes.Export, Core.Constants.CountryCodes.Switzerland, ProcedureCode50DescriptionExp, ProcedureCode50Exp)
		};
		CreateRefCusProcedure(JobMessageTypeList.Codes.Import, Core.Constants.CountryCodes.Switzerland, "TestRow", "35");
		CreateRefCusProcedure(JobMessageTypeList.Codes.Export, Core.Constants.CountryCodes.Latvia, "TestRow", "85");
		CreateRefCusProcedure(JobMessageTypeList.Codes.Import, Core.Constants.CountryCodes.Latvia, "TestRow", "33");

		factory.Save();
		return ret;

		RefCusProcedure CreateRefCusProcedure(string shipmentType, string nkDataGrupping, string description, string procedureCode)
		{
			var procedure = factory.New<RefCusProcedure>();
			helper.CreateNewOrGetExistingDataGrouping(nkDataGrupping);
			procedure.ZZ6_ShipmentType = shipmentType;
			procedure.ZZ6_ZZZ_NKDataGrouping = nkDataGrupping;
			procedure.ZZ6_Description = description;
			procedure.ZZ6_ProcedureCode = procedureCode;
			return procedure;
		}
	}

	public static void CreateEUCountryList(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var startDate = ZDate.Today.AddMonths(-1);
		var endDate = ZDate.Today.AddMonths(1);
		var eun = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

		helper.CreateCusCodeList(eun, "CL010", CountryInEUAndCL010CountryList, startDate, endDate);

		factory.Save();
	}

	internal static void CreateRestrictionCodeWithPermitNumberAttribute(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.Restrictions;
		const string dataGrouping = Core.Constants.CountryCodes.Switzerland;
		var valueTrue = "Y";
		var valueFalse = "N";

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
		helper.CreateNewOrGetExistingCusCodeType(codeType, nameof(codeType), dataGrouping);

		var code1 = helper.CreateCusCodeList(dataGrouping, codeType, RestrictionCodeWithPermitNumberAllowedAttributeY, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var code2 = helper.CreateCusCodeList(dataGrouping, codeType, RestrictionCodeWithPermitNumberAllowedAndPermitExceptionReasonAttributesY, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var code3 = helper.CreateCusCodeList(dataGrouping, codeType, RestrictionCodeWithPermitNumberAllowedAndPermitExceptionReasonAttributesN, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		helper.CreateCusCodeListAttribute(code1.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.PermitNumberAllowed, valueTrue);
		helper.CreateCusCodeListAttribute(code1.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.PermitExceptionReasonAllowed, valueFalse);

		helper.CreateCusCodeListAttribute(code2.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.PermitNumberAllowed, valueTrue);
		helper.CreateCusCodeListAttribute(code2.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.PermitExceptionReasonAllowed, valueTrue);

		helper.CreateCusCodeListAttribute(code3.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.PermitNumberAllowed, valueFalse);
		helper.CreateCusCodeListAttribute(code3.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.PermitExceptionReasonAllowed, valueFalse);

		factory.Save();
	}

	internal static void CreateRestrictionCodeAdditionalInformationAttribute(BusinessObjectFactory factory)
	{
		const string codeType = UniversalReferenceConstants.RefCusCodeList.PassarTypes.Restrictions;
		const string dataGrouping = Core.Constants.CountryCodes.Switzerland;

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType, dataGrouping);

		var yesCode = helper.CreateCusCodeList(dataGrouping, codeType, RestrictionCodeWithAdditionalInformationAttributeY, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var noCode = helper.CreateCusCodeList(dataGrouping, codeType, RestrictionCodeWithAdditionalInformationAttributeN, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		helper.CreateCusCodeListAttribute(yesCode.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.AdditionalInformation, UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes);
		helper.CreateCusCodeListAttribute(noCode.PK, UniversalReferenceConstants.RefCusCodeList.Attributes.AdditionalInformation, UniversalReferenceConstants.RefCusCodeList.AttributeValues.No);

		factory.Save();
	}

	internal static void CreateNotifyCustomsOfficeList(BusinessObjectFactory factory) => CreateSimpleList(factory, UniversalReferenceConstants.RefCusCodeList.EdecTypes.CustomsOffice, ValidNotifyCustomsOfficeCode, ValidNotifyCustomsOfficeCodePast, InvalidNotifyCustomsOfficeCode);

	internal static void CreateInAndOutwardDirectionList(BusinessObjectFactory factory, string[] languages = null) => CreateSimpleList(factory, UniversalReferenceConstants.RefCusCodeList.EdecTypes.Direction, ValidSimpleCode, ValidSimpleCodePast, InvalidSimpleCode, languages);

	internal static void CreateInAndOutwardRefinementTypeList(BusinessObjectFactory factory, string[] languages = null) => CreateSimpleList(factory, UniversalReferenceConstants.RefCusCodeList.EdecTypes.RefinementType, ValidSimpleCode, ValidSimpleCodePast, InvalidSimpleCode, languages);

	internal static void CreateInAndOutwardProcessTypeList(BusinessObjectFactory factory, string[] languages = null) => CreateSimpleList(factory, UniversalReferenceConstants.RefCusCodeList.EdecTypes.ProcessType, ValidSimpleCode, ValidSimpleCodePast, InvalidSimpleCode, languages);

	internal static void CreateInAndOutwardBillingTypeList(BusinessObjectFactory factory, string[] languages = null) => CreateSimpleList(factory, UniversalReferenceConstants.RefCusCodeList.EdecTypes.BillingType, ValidSimpleCode, ValidSimpleCodePast, InvalidSimpleCode, languages);

	internal static void CreateGoodsItemsDetailNameTypeList(BusinessObjectFactory factory) => CreateSimpleList(factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CHAdditionalInformation, ValidGIDNMCode, ValidGIDNMCodePast, InvalidGIDNMCode);

	internal static void CreateSupportingDocumentImportDirectionTypeList(BusinessObjectFactory factory) => CreateSimpleList(factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ValidDC44ICode, ValidDC44ICodePast, InvalidDC44ICode);

	public static void CreateEntryStatusList(BusinessObjectFactory factory) => CreateSimpleList(factory, UniversalReferenceConstants.RefCusCodeList.EntryStatus, ValidSimpleCode, ValidSimpleCodePast, InvalidSimpleCode);

	static void CreateSimpleList(BusinessObjectFactory factory, string codeType, string validCode, string validCodePast, string invalidCode, string[] languages = null)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, validCode, $"{validCode} - Description", new ZDateTime(2024, 01, 01), ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, validCodePast, $"{validCodePast} - Description", ZDateTime.MinSmallDateTimeValue, new ZDateTime(2023, 12, 31));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, invalidCode, $"{invalidCode} - Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		AddCodeListLanguages(helper, languages, code);
		factory.Save();
	}

	static void AddCodeListLanguages(UniversalReferenceTestDataHelper helper, string[] languages, params RefCusCodeList[] codes)
	{
		if (languages != null)
		{
			foreach (var language in languages.Select(l => l.Substring(0, 2)))
			{
				helper.CreateOrGetLanguage(language, language + "DESC");
				foreach (var code in codes)
				{
					helper.CreateCusCodeListLanguage(code, language, code.ZZD_Description + language);
				}
			}
		}
	}

	public static void CreateSwissLocalLanguages(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);

		CreateSwissLocalLanguage("DE");
		CreateSwissLocalLanguage("FR");
		CreateSwissLocalLanguage("IT");
		factory.Save();

		void CreateSwissLocalLanguage(string code)
		{
			var parentLanguageQuery = new ZQuery();
			parentLanguageQuery.AddToFilter(RefLocalLanguageSchema.RA_Code, code);
			parentLanguageQuery.AddToFilter(RefLocalLanguageSchema.RA_RN_NKCountryCode, code);
			var parentLanguage = factory.LoadTop1<IRefLocalLanguage>(parentLanguageQuery);

			var localLanguage = factory.New<IRefLocalLanguage>();
			localLanguage.RA_Code = code;
			localLanguage.RA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
			localLanguage.RA_RA_ParentLanguage = parentLanguage.PK;
			localLanguage.RA_Description = "Swiss-" + code;
			localLanguage.RA_IsActive = true;
			localLanguage.RA_IsSystem = true;

			if (Env.Security.DocBuilderLanguagesLookup.TryGetValue(localLanguage.FullLanguageCode, out SecurityCheckpoint securityCheckpoint))
			{
				securityCheckpoint.IsAllowed = true;
			}
			else
			{
				var securityInstance = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
				SecurityCheckpoint newSecurityCheckpoint = new SecurityCheckpoint("Dummy", (NoResString)"TESTING 1 2 3", null, securityInstance);
				newSecurityCheckpoint.IsAllowed = true;
				Env.Security.DocBuilderLanguagesLookup.Add(localLanguage.FullLanguageCode, newSecurityCheckpoint);
			}
		}
	}
}
