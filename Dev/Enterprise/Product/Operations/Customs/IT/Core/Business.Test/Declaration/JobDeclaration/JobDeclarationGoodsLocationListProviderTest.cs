using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationGoodsLocationListProviderTest : GoodsLocationListProviderTest<GoodsLocationList>
{
	public override void TestLocations()
	{
		declaration.ZG_AuthorisationNumber = "999999";
		AssertEquals("Valid authorisation number, but message type is empty", 0, declaration.Lookups.Locations.Count);

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
		declaration.JE_OH_Supplier = orgHeader.PK;
		AssertLookup("Valid locations are loaded for EXP message type", (CodeDescriptionPairList)declaration.Lookups.Locations, 2, new Dictionary<ZString, ZString> { { "MYLOC1", "MYLOC1 RULE DESCRIPTION" }, { "MYLOC2", "MYLOC2 RULE DESCRIPTION" } });

		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "MYLOC3", "MYLOC3 RULE DESCRIPTION");
		AssertLookup("Result has been cached and new authorisations are not loaded", (CodeDescriptionPairList)declaration.Lookups.Locations, 2, new Dictionary<ZString, ZString> { { "MYLOC1", "MYLOC1 RULE DESCRIPTION" }, { "MYLOC2", "MYLOC2 RULE DESCRIPTION" } });

		Factory.ClearCachedValue<CodeDescriptionPairList>($"{authorisationHeader.PK}|{declaration.JE_CustomsOffice}");
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = orgHeader.PK;
		authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport;
		AssertEquals(3, declaration.Lookups.Locations.Count);
		AssertLookup("Valid locations are loaded for IMP message type", (CodeDescriptionPairList)declaration.Lookups.Locations, 3, new Dictionary<ZString, ZString> { { "MYLOC1", "MYLOC1 RULE DESCRIPTION" }, { "MYLOC2", "MYLOC2 RULE DESCRIPTION" }, { "MYLOC3", "MYLOC3 RULE DESCRIPTION" } });
	}

	public void TestLocationsFilteredByPresentationCustomsOffice()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var authorisationHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, permitHolder: orgHeader.PK, "999999", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		var authorisationRule1 = CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "123456A");
		var linkedRule1 = authorisationRule1.LinkedCusAuthorisationRules.AddNew();
		linkedRule1.CPR_RuleCode = Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
		linkedRule1.CPR_ValueFrom = "IT137100";
		var authorisationRule2 = CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "123456B");
		var linkedRule2 = authorisationRule2.LinkedCusAuthorisationRules.AddNew();
		linkedRule2.CPR_RuleCode = Customs.Business.LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
		linkedRule2.CPR_ValueFrom = "IT137100";
		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "123456C");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OH_Importer = orgHeader.PK;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.ZG_AuthorisationNumber = "999999";
		declaration.JE_CustomsOffice = ZString.Empty;
		AssertEquals("All locations are listed", 3, declaration.Lookups.Locations.Count);
		AssertEquals("123456A, 123456B, 123456C", ((CodeDescriptionPairList)declaration.Lookups.Locations).CodesAsString);

		declaration.JE_CustomsOffice = "IT137100";
		AssertEquals("Filtered locations are listed", 2, declaration.Lookups.Locations.Count);
		AssertEquals("123456A, 123456B", ((CodeDescriptionPairList)declaration.Lookups.Locations).CodesAsString);
	}

	public override void TestLocationsWithEmptyAuthorizationNumber()
	{
		declaration.ZG_AuthorisationNumber = ZString.Empty;
		AssertType<GoodsLocationList>("When AuthorisationNumber is empty, GoodsLocationList is returned", declaration.Lookups.Locations);
		AssertEquals(2, declaration.Lookups.Locations.Count);

		Factory.ClearCachedValue<CodeDescriptionPairList>($"{authorisationHeader.PK}|{declaration.JE_CustomsOffice}");
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		AssertEquals(0, declaration.Lookups.Locations.Count);
	}

	public override void TestLocationsWithWrongAuthorizationNumber()
	{
		declaration.ZG_AuthorisationNumber = "123456";
		AssertEquals("Invalid authorisation number selected", 0, declaration.Lookups.Locations.Count);
	}

	void AssertLookup(ZString combineAssertionMessage, CodeDescriptionPairList lookup, int count, Dictionary<ZString, ZString> codeDescriptions)
	{
		CombineAssertions(combineAssertionMessage, () =>
		{
			AssertEquals("Lookup count", count, lookup.Count);
			foreach (var pair in codeDescriptions)
			{
				AssertEquals(pair.Value, lookup.GetDescriptionFromCode(pair.Key));
			}
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		authorisationHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, permitHolder: orgHeader.PK, "999999", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "MYLOC1", "MYLOC1 RULE DESCRIPTION");
		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "MYLOC2", "MYLOC2 RULE DESCRIPTION");
		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, "UND", "UNDEFINED");

		declaration = Factory.New<JobDeclaration>();
	}
	OrgHeader orgHeader;
	CusAuthorisationHeader authorisationHeader;
	JobDeclaration declaration;

	protected override GoodsLocationListProvider<GoodsLocationList> GetNewGoodsLocationListProvider(IAutHeaderWithCusOfficeProvider authorisationWithCustomsOfficeProvider, BusinessObjectFactory factory) => new JobDeclarationGoodsLocationListProvider(authorisationWithCustomsOfficeProvider, factory);
}
