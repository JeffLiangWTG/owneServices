using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class AdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestAdditionalInfoCodes()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var countryCode = Core.Constants.CountryCodes.Italy;
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(countryCode, "Italy", eun);

		var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
		helper.CreateNewOrGetExistingCusCodeType(codeType, "Additional Information");
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", codeType, countryCode);

		var cusCode1 = helper.CreateCusCodeList(countryCode, codeType, "11111", "ABC description for test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var attribute1 = cusCode1.Attributes.AddNew("Direction", "IMPORT");

		var cusCode2 = helper.CreateCusCodeList(countryCode, codeType, "22222", "ABC description for test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var attribute2 = cusCode2.Attributes.AddNew("Direction", "IMPORT");

		var cusCode3 = helper.CreateCusCodeList(countryCode, codeType, "33333", "ABC description for test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var attribute3 = cusCode3.Attributes.AddNew("Direction", "IMPORT");

		Factory.Save();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
		var additionalInfoLookups = additionalInfo.Lookups;

		AssertEquals("AdditionalInfo codes count", 3, additionalInfoLookups.CodeList.Count);
		var codes = additionalInfoLookups.CodeList as CodeDescriptionPairList;
		AssertNotNull(codes);
		AssertEquals("AdditionalInfo codes", "11111, 22222, 33333", codes.CodesAsString);
	}
}
