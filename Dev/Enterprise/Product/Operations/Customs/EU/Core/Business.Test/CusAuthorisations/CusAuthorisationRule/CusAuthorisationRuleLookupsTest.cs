using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusAuthorisationRuleLookups))]
	class CusAuthorisationRuleLookupsTest : Customs.Business.Testing.CusAuthorisationRuleLookupsAbstractTest<CusAuthorisationRuleLookups>
	{
		protected override CusAuthorisationRuleLookups CusAuthorisationRuleLookupsForTesting() => new CusAuthorisationRuleLookups(cusAuthorisationRule);

		[ExpectNoExceptions]
		public void TestRuleValueListForRuleCodeLOC()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CusAuthorisationRuleLookupsForTesting().ValueList, NUnit.Framework.Is.Not.EqualTo(typeof(OrgHeaderCollection)).Using(CustomComparers.TypeComparison), "No Header & No RuleCode");
				cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				NUnit.Framework.Assert.That(CusAuthorisationRuleLookupsForTesting().ValueList, NUnit.Framework.Is.Not.EqualTo(typeof(OrgHeaderCollection)).Using(CustomComparers.TypeComparison), "No Header & RuleCode LOC");
				var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
				cusAuthorisationHeader.CusAuthorisationRules.Add(cusAuthorisationRule);
				NUnit.Framework.Assert.That(CusAuthorisationRuleLookupsForTesting().ValueList, NUnit.Framework.Is.Not.EqualTo(typeof(OrgHeaderCollection)).Using(CustomComparers.TypeComparison), "Header No Type & RuleCode LOC");
				cusAuthorisationRule.AuthorisationHeader.CPH_Type = "TYP";
				NUnit.Framework.Assert.That(CusAuthorisationRuleLookupsForTesting().ValueList, NUnit.Framework.Is.Not.EqualTo(typeof(OrgHeaderCollection)).Using(CustomComparers.TypeComparison), "Header Type not CLO & RuleCode LOC");
				cusAuthorisationRule.AuthorisationHeader.CPH_Type = UniversalReferenceConstants.CusAuthorisationHeaderType.CustomsAuthorizedLocations;
				NUnit.Framework.Assert.That(CusAuthorisationRuleLookupsForTesting().ValueList, NUnit.Framework.Is.TypeOf<OrgHeaderCollection>(), "Header Type CLO & RuleCode LOC");
				cusAuthorisationRule.CPR_RuleCode = "";
				NUnit.Framework.Assert.That(CusAuthorisationRuleLookupsForTesting().ValueList, NUnit.Framework.Is.Not.EqualTo(typeof(OrgHeaderCollection)).Using(CustomComparers.TypeComparison), "Header Type CLO & No RuleCode");
			});
		}

		[ExpectNoExceptions]
		public void TestRuleDescriptionListForRuleCodeLOC()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(CusAuthorisationRuleLookupsForTesting().DescriptionList, NUnit.Framework.Is.Not.EqualTo(typeof(OrgAddressCollection)).Using(CustomComparers.TypeComparison), "No Header & No RuleCode");
				cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				NUnit.Framework.Assert.That(CusAuthorisationRuleLookupsForTesting().DescriptionList, NUnit.Framework.Is.Not.EqualTo(typeof(OrgAddressCollection)).Using(CustomComparers.TypeComparison), "No Header & RuleCode LOC");
				var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
				cusAuthorisationHeader.CusAuthorisationRules.Add(cusAuthorisationRule);
				NUnit.Framework.Assert.That(CusAuthorisationRuleLookupsForTesting().DescriptionList, NUnit.Framework.Is.Not.EqualTo(typeof(OrgAddressCollection)).Using(CustomComparers.TypeComparison), "Header No Type & RuleCode LOC");
				cusAuthorisationRule.AuthorisationHeader.CPH_Type = "TYP";
				NUnit.Framework.Assert.That(CusAuthorisationRuleLookupsForTesting().DescriptionList, NUnit.Framework.Is.Not.EqualTo(typeof(OrgAddressCollection)).Using(CustomComparers.TypeComparison), "Header Type not CLO & RuleCode LOC");
				cusAuthorisationRule.AuthorisationHeader.CPH_Type = UniversalReferenceConstants.CusAuthorisationHeaderType.CustomsAuthorizedLocations;
				NUnit.Framework.Assert.That(CusAuthorisationRuleLookupsForTesting().DescriptionList, NUnit.Framework.Is.TypeOf<OrgAddressCollection>(), "Header Type CLO & RuleCode LOC");
				cusAuthorisationRule.CPR_RuleCode = "";
				NUnit.Framework.Assert.That(CusAuthorisationRuleLookupsForTesting().DescriptionList, NUnit.Framework.Is.Not.EqualTo(typeof(OrgAddressCollection)).Using(CustomComparers.TypeComparison), "Header Type CLO & No RuleCode");
			});
		}

		[ExpectNoExceptions]
		public void TestDescriptionListLocation()
		{
			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			cusAuthorisationHeader.CusAuthorisationRules.Add(cusAuthorisationRule);
			cusAuthorisationRule.AuthorisationHeader.CPH_Type = UniversalReferenceConstants.CusAuthorisationHeaderType.CustomsAuthorizedLocations;
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress1 = Factory.New<OrgAddress>();
			orgAddress1.OA_OH = orgHeader.PK;
			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_OH = orgHeader.PK;
			cusAuthorisationRule.CPR_ValueFrom = orgHeader.PK.ToString();
			NUnit.Framework.Assert.That(CusAuthorisationRuleLookupsForTesting().DescriptionList.Count, NUnit.Framework.Is.EqualTo(2));
			cusAuthorisationRule.CPR_ValueFrom = ZString.Empty;
			NUnit.Framework.Assert.That(CusAuthorisationRuleLookupsForTesting().DescriptionList.Count, NUnit.Framework.Is.EqualTo(0));
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
		}
		CusAuthorisationRule cusAuthorisationRule;
	}
}
