using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

class SupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestUnitOfQuantityCodeList()
	{
		var codesInList = supportingDocument.Lookups.UnitOfQuantityList.CodesAsString;
		CombineAssertions(() =>
		{
			AssertContains("NL - List of Customs Unit Qualifiers should contain 'AAA'.", "AAA", codesInList);
			AssertContains("NL - List of Customs Unit Qualifiers should contain 'BBB'.", "BBB", codesInList);
			AssertContains("NL - List of CustomsUnitQualifiers should contain 'CCC'.", "CCC", codesInList);
			AssertContains("NL - List of CustomsUnitQualifiers should contain 'DDD'.", "DDD", codesInList);
		});
	}

	public void TestUnitOfQuantity2CodeList()
	{
		var codesInList = supportingDocument.Lookups.UnitOfQuantity2List.CodesAsString;
		CombineAssertions(() =>
		{
			AssertContains("NL - List of Customs Unit Qualifiers should contain 'AAA'.", "AAA", codesInList);
			AssertContains("NL - List of Customs Unit Qualifiers should contain 'BBB'.", "BBB", codesInList);
			AssertContains("NL - List of CustomsUnitQualifiers should contain 'CCC'.", "CCC", codesInList);
			AssertContains("NL - List of CustomsUnitQualifiers should contain 'DDD'.", "DDD", codesInList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands, "Netherlands", eun);

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Unit Qualifiers", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Unit Qualifiers", Core.Constants.CountryCodes.Netherlands);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "AAA", "AAA Test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "BBB", "BBB Test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CCC", "CCC Test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DDD", "DDD Test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		supportingDocument = declaration.SupportingDocuments.AddNew();
	}

	SupportingDocument supportingDocument;
}
