using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class RegistrationNumberHelperTest : TestCaseWithFactory
	{
		public void TestGetCY_DataListForSafeFoodForCanadiansLicence()
		{
			var importerbuyer = Factory.New<OrgHeader>();
			importerbuyer.OH_Code = "TE#1";
			var impAddInfo = OrgImpAddInfo.Get(importerbuyer);
			var sfLicense1 = impAddInfo.SafeFoodLicenses.AddNew();
			sfLicense1.CY_Code = "BANANA";
			sfLicense1.CY_Data = "BANANADESC";
			var sfLicense2 = impAddInfo.SafeFoodLicenses.AddNew();
			sfLicense2.CY_Code = "APPLE";
			sfLicense2.CY_Data = "APPLEDESC";

			var list = RegistrationNumberHelper.GetCY_DataListForSafeFoodForCanadiansLicence(Factory, importerbuyer);
			AssertEquals("2 licenses", 2, list.Count);
			AssertEquals("BANANA, APPLE", list.CodesAsString);

			var list2 = RegistrationNumberHelper.GetCY_DataListForSafeFoodForCanadiansLicence(Factory, importerbuyer);
			AssertEquals("Cached", list, list2);
		}

		public void TestValidationForCFIARegNum()
		{
			RegistrationNumberHelperTest.PrepareGlobalData(Factory);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.CA_OGDCFIA = ZBool.True;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var regNum = invoiceLine.CFIARegistrationNumbers.AddNew();
			var messageErrorr = "The number should be alphanumeric, dots, space, or hyphens.";
			regNum.CY_Code = "A01";
			regNum.CY_Data = "##ABC123";
			AssertHasWarningContaining(regNum.CY_DataInfo, messageErrorr);
			regNum.CY_Data = "ABC123";
			AssertNoWarningContaining(regNum.CY_DataInfo, messageErrorr);
			regNum.CY_Data = "-ABC123";
			AssertNoWarningContaining(regNum.CY_DataInfo, messageErrorr);
			regNum.CY_Data = "1-ABC123";
			AssertNoWarningContaining(regNum.CY_DataInfo, messageErrorr);
			regNum.CY_Data = "ABC 123";
			AssertNoWarningContaining(regNum.CY_DataInfo, messageErrorr);

			messageErrorr = "The number should be numeric only.";
			regNum.CY_Code = "A02";
			regNum.CY_Data = "ABC123";
			AssertHasMessageErrorContaining(regNum.CY_DataInfo, messageErrorr);
			regNum.CY_Data = "123.4";
			AssertNoMessageErrorContaining(regNum.CY_DataInfo, "The number should be numeric only.");
			regNum.CY_Data = "123";
			AssertNoMessageErrorContaining(regNum.CY_DataInfo, "The number should be numeric only.");
		}

		public static void PrepareGlobalData(BusinessObjectFactory factory)
		{
			var startDate = ZDateTime.UtcToday.AddDays(-1);
			var endDate = ZDateTime.UtcToday.AddDays(1);

			var helper = new UniversalReferenceTestDataHelper(factory);

			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);

			var caartCodeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIAAIRSRegistrationType, "CAART", dataGrouping: dataGrouping.ZZZ_DataGrouping);
			var calpcCodeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIALPCOType, "CALPC", dataGrouping: dataGrouping.ZZZ_DataGrouping);

			var caartFormat = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CFIAFormat, "CFIA AIRS Format", caartCodeType.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var calpcFormat = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CFIAFormat, "CFIA LPCO Format", calpcCodeType.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var calpcMaterialized = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CFIAMaterialized, "CFIA LPCO Materialized", calpcCodeType.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var calpcDematCountry = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CFIADematerializedCountry, "CFIA LPCO Dematerialized Country Codes", calpcCodeType.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);

			var airs01 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, caartCodeType.ZZK_CodeType, "A01", "AIRS 001", startDate, endDate);
			var airs02 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, caartCodeType.ZZK_CodeType, "A02", "AIRS 002", startDate, endDate);
			var airs03 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, caartCodeType.ZZK_CodeType, "A03", "AIRS 003", startDate, endDate);

			var airsFormat01 = helper.CreateNewOrGetExistingCusCodeListAttribute(airs01.PK, caartFormat.ZXE_Name, RegistrationNumberHelper.Alpha);
			var airsFormat02 = helper.CreateNewOrGetExistingCusCodeListAttribute(airs02.PK, caartFormat.ZXE_Name, RegistrationNumberHelper.Numeric);
			var airsFormat03 = helper.CreateNewOrGetExistingCusCodeListAttribute(airs03.PK, caartFormat.ZXE_Name, RegistrationNumberHelper.Confirmation);

			var lpco01 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, calpcCodeType.ZZK_CodeType, "C01", "LPCO 001", startDate, endDate);
			var lpco02 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, calpcCodeType.ZZK_CodeType, "C02", "LPCO 002", startDate, endDate);
			var lpco03 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, calpcCodeType.ZZK_CodeType, "C03", "LPCO 003", startDate, endDate);

			var lpcoFormat01 = helper.CreateNewOrGetExistingCusCodeListAttribute(lpco01.PK, calpcFormat.ZXE_Name, RegistrationNumberHelper.Alpha);
			var lpcoFormat02 = helper.CreateNewOrGetExistingCusCodeListAttribute(lpco02.PK, calpcFormat.ZXE_Name, RegistrationNumberHelper.Numeric);
			var lpcoFormat03 = helper.CreateNewOrGetExistingCusCodeListAttribute(lpco03.PK, calpcFormat.ZXE_Name, RegistrationNumberHelper.Confirmation);

			var lpcoMaterialized01 = helper.CreateNewOrGetExistingCusCodeListAttribute(lpco01.PK, calpcMaterialized.ZXE_Name, YesNoList.Codes.Yes);
			var lpcoMaterialized02 = helper.CreateNewOrGetExistingCusCodeListAttribute(lpco02.PK, calpcMaterialized.ZXE_Name, YesNoList.Codes.No);
			var lpcoMaterialized03 = helper.CreateNewOrGetExistingCusCodeListAttribute(lpco03.PK, calpcMaterialized.ZXE_Name, YesNoList.Codes.Yes);
			var lpcoDematCountry03 = helper.CreateNewOrGetExistingCusCodeListAttribute(lpco03.PK, calpcDematCountry.ZXE_Name, Core.Constants.CountryCodes.Australia);

			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var code01 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "2001", "2001 DESC", startDate, endDate);
			var attributePGA01 = helper.CreateNewOrGetExistingCusCodeListAttribute(code01.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.GAC);
			var code02 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "4001", "4001 DESC", startDate, endDate);
			var attributePGA02 = helper.CreateNewOrGetExistingCusCodeListAttribute(code02.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.TC);

			factory.Save();
		}
	}
}
