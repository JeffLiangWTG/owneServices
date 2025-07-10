using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class FactoryExtensionsTest : TestCaseWithFactory
{
	public void TestGetItalyCustomsOfficeCodeDescriptionPairList()
	{
		var factory = Factory;

		var helper = new UniversalReferenceTestDataHelper(factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eun);
		_ = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");

		var codeIT009103Exit = helper.CreateNewOrGetExistingCusCodeList(
			dataGroupingCode: Core.Constants.CountryCodes.Italy,
			codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
			code: "IT009103",
			description: "Italy Airport",
			ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
		);
		var codeITSNN400Destination = helper.CreateNewOrGetExistingCusCodeList(
			dataGroupingCode: Core.Constants.CountryCodes.Italy,
			codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
			code: "ITSNN400",
			description: "Italy Bakery",
			ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
		);
		var codeDESNN400Destination = helper.CreateNewOrGetExistingCusCodeList(
			dataGroupingCode: Core.Constants.CountryCodes.Germany,
			codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
			code: "DESNN400",
			description: "Germany Bakery",
			ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
		);

		Factory.Save();

		var customsOfficeCodeDescriptionPairList = Factory.GetItalyCustomsOfficeCodeDescriptionPairList();
		AssertContainsExactElementsInAnyOrder("Only IT customs office codes", new[] { "IT009103", "ITSNN400" }, customsOfficeCodeDescriptionPairList.GetAllCodes());
		AssertSame("Cached", customsOfficeCodeDescriptionPairList, Factory.GetItalyCustomsOfficeCodeDescriptionPairList());
	}
}
