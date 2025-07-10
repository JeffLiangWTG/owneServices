using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CusAuthorizationUsageLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeListForExportEntryInstruction()
	{
		SetUpRefData();
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		AssertRefCusCodeAuthorizationCodes(authorizationUsage, expectedCodes: new ZString[] { "AAA", "DDD", "YYY", "ZZZ" });
	}

	public void TestCodeListForExportInvoiceLine()
	{
		SetUpRefData();
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var authorizationUsage = invoiceLine.CusAuthorizationUsages.AddNew();
		AssertRefCusCodeAuthorizationCodes(authorizationUsage, expectedCodes: new ZString[] { "AAA", "DDD" });
	}

	public void TestCodeListWhenParentDeclarationIsNotFound()
	{
		var authorizationUsage = Factory.New<CusAuthorizationUsage>();
		AssertCodeDescriptionPairListAuthorizationCodes(authorizationUsage, expectedCodesAsString: "");
	}

	void AssertRefCusCodeAuthorizationCodes(CusAuthorizationUsage authorizationUsage, ZString[] expectedCodes)
	{
		var cusCodeListCollection = (ZZRefCusCodeListCombinedCollection)authorizationUsage.Lookups.CodeList;
		var codes = cusCodeListCollection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code).ToArray();
		AssertArrayEqualsByElements("Codes", expectedCodes, codes);
	}

	void AssertCodeDescriptionPairListAuthorizationCodes(CusAuthorizationUsage authorizationUsage, ZString expectedCodesAsString)
	{
		var codeList = (CodeDescriptionPairList)authorizationUsage.Lookups.CodeList;
		AssertEquals("Codes", expectedCodesAsString, codeList.CodesAsString);
	}

	void SetUpRefData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType("AUTH", "Authorizations");
		helper.CreateNewOrGetExistingCusCodeList("EUN", "AUTH", "ZZZ", "ZZZ - Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList("EUN", "AUTH", "YYY", "YYY - Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var codeListWithItemLevelAttribute1 = helper.CreateNewOrGetExistingCusCodeList("EUN", "AUTH", "DDD", "DDD - Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeListAttribute(codeListWithItemLevelAttribute1.PK, "Level", "Item");
		var codeListWithItemLevelAttribute2 = helper.CreateNewOrGetExistingCusCodeList("EUN", "AUTH", "AAA", "AAA - Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeListAttribute(codeListWithItemLevelAttribute2.PK, "Level", "Item");
	}
}
