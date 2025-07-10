using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

class CusAuthorisationTestHelper
{
	public static void SetupSupportingDocumentData(BusinessObjectFactory factory)
	{
		var authorisationHeaderCountryCode = Core.Constants.CountryCodes.Italy;
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Document Type (EU Box 44 Exports)");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Document Type (EU Box 44 Imports)");
		helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, "Document Type (EU Box 44 NCTS)");
		helper.CreateNewOrGetExistingDataGrouping(authorisationHeaderCountryCode);
		helper.CreateNewOrGetExistingCusCodeList(authorisationHeaderCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "9112", "Drugs Precursor Chemicals Individual Licence", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(authorisationHeaderCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "9004", "Certificates of origin for steel quotas.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(authorisationHeaderCountryCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, "22YY", "Tute e camici.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}
}
