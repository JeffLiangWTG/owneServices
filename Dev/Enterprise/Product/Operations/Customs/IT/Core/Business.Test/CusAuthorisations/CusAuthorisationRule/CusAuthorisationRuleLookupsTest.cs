using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(CusAuthorisationRuleLookups))]
sealed class CusAuthorisationRuleLookupsTest : Customs.Business.Testing.CusAuthorisationRuleLookupsAbstractTest<CusAuthorisationRuleLookups>
{
	public void TestRuleCodeListDefault()
	{
		var lookups = CusAuthorisationRuleLookupsForTesting();
		lookups.Parent.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
		AssertContains(ITCusAuthorisationRuleTypeList.Codes.Location, lookups.RuleCodeList.CodesAsString);
		AssertNotContains(ITCusAuthorisationRuleTypeList.Codes.Document, lookups.RuleCodeList.CodesAsString);
	}

	public void TestRuleCodeListForCustomsWarehousingCWP()
	{
		var lookups = CusAuthorisationRuleLookupsForTesting();
		lookups.Parent.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
		AssertContains(ITCusAuthorisationRuleTypeList.Codes.Document, lookups.RuleCodeList.CodesAsString);
	}

	public void TestRuleCodeListForCustomsWarehousingCW1()
	{
		var lookups = CusAuthorisationRuleLookupsForTesting();
		lookups.Parent.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
		AssertContains(ITCusAuthorisationRuleTypeList.Codes.Document, lookups.RuleCodeList.CodesAsString);
	}

	public void TestRuleCodeListForCustomsWarehousingCW2()
	{
		var lookups = CusAuthorisationRuleLookupsForTesting();
		lookups.Parent.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
		AssertContains(ITCusAuthorisationRuleTypeList.Codes.Document, lookups.RuleCodeList.CodesAsString);
	}

	public void TestRuleCodeListForInwardProcessing()
	{
		var lookups = CusAuthorisationRuleLookupsForTesting();
		lookups.Parent.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
		AssertContains(ITCusAuthorisationRuleTypeList.Codes.Document, lookups.RuleCodeList.CodesAsString);
	}

	public void TestRuleCodeListForOutwardProcessing()
	{
		var lookups = CusAuthorisationRuleLookupsForTesting();
		lookups.Parent.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
		AssertContains(ITCusAuthorisationRuleTypeList.Codes.Document, lookups.RuleCodeList.CodesAsString);
	}

	public void TestRuleCodeListForDeclarationOfIntent()
	{
		var lookups = CusAuthorisationRuleLookupsForTesting();
		lookups.Parent.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent;
		AssertEquals(1, lookups.RuleCodeList.Count);
		AssertContains(ITCusAuthorisationRuleTypeList.Codes.Use, lookups.RuleCodeList.CodesAsString);
	}

	public void TestRuleCodeListForAuthorizedConsignorTransit()
	{
		var lookups = CusAuthorisationRuleLookupsForTesting();
		lookups.Parent.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		AssertContains(ITCusAuthorisationRuleTypeList.Codes.Document, lookups.RuleCodeList.CodesAsString);
	}

	public void TestValueListAuthorizationDocument()
	{
		CusAuthorisationTestHelper.SetupSupportingDocumentData(Factory);
		var lookups = CusAuthorisationRuleLookupsForTesting();
		var authorisationHeader = lookups.Parent.AuthorisationHeader;

		ZZRefCusCodeListCombinedCollection RefreshValueListAuthorizationDocument()
		{
			var valueListAuthorizationDocumentToRefresh = lookups.ValueListAuthorizationDocument;
			valueListAuthorizationDocumentToRefresh.Load();
			return valueListAuthorizationDocumentToRefresh;
		}

		authorisationHeader.CPH_Type = "";
		var valueListAuthorizationDocument = RefreshValueListAuthorizationDocument();
		AssertContainsExactElementsInAnyOrder("ValueListAuthorizationDocument when CPH_Type is Empty ", new[] { "9004", "9112", "22YY" }, valueListAuthorizationDocument.Select(x => x.ZZD_Code));

		authorisationHeader.CPH_Type = "CWP";
		valueListAuthorizationDocument = RefreshValueListAuthorizationDocument();
		AssertContainsExactElementsInAnyOrder("ValueListAuthorizationDocument when CPH_Type is CWP", new[] { "9004" }, valueListAuthorizationDocument.Select(x => x.ZZD_Code));

		authorisationHeader.CPH_Type = "OPO";
		valueListAuthorizationDocument = RefreshValueListAuthorizationDocument();
		AssertContainsExactElementsInAnyOrder("ValueListAuthorizationDocument when CPH_Type is OPO", new[] { "9112" }, valueListAuthorizationDocument.Select(x => x.ZZD_Code));

		authorisationHeader.CPH_Type = "ACR";
		valueListAuthorizationDocument = RefreshValueListAuthorizationDocument();
		AssertContainsExactElementsInAnyOrder("ValueListAuthorizationDocument when CPH_Type is ACR", new[] { "22YY" }, valueListAuthorizationDocument.Select(x => x.ZZD_Code));
	}

	public void TestValueListAuthorisationUse()
	{
		var lookups = CusAuthorisationRuleLookupsForTesting();
		lookups.Parent.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Use;
		AssertType<CusAuthorisationRuleUseValueList>(lookups.ValueList);
	}

	protected override CusAuthorisationRuleLookups CusAuthorisationRuleLookupsForTesting()
	{
		var cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
		cusAuthorisationRule.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
		cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		return new CusAuthorisationRuleLookups(cusAuthorisationRule);
	}
}
