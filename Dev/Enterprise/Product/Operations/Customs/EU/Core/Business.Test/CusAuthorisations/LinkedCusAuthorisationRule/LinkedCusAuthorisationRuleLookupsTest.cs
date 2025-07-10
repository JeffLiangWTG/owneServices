using Enterprise.Customs.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	class LinkedCusAuthorisationRuleLookupsTest : Customs.Business.Testing.LinkedCusAuthorisationRuleLookupsTest
	{
		[ExpectNoExceptions]
		public void TestRuleCodeListForACT()
		{
			linkedCusAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.Active;
			NUnit.Framework.Assert.That(lookups.ValueList, NUnit.Framework.Is.EquivalentTo(new YesNoList()).Using(CustomComparers.TypeComparison), "List for ACT");
		}
		[ExpectNoExceptions]
		public void TestRuleCodeListForIEB()
		{
			linkedCusAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.IEB;
			NUnit.Framework.Assert.That(lookups.ValueList, NUnit.Framework.Is.EquivalentTo(new ImportExportList()).Using(CustomComparers.TypeComparison), "List for IEB");
		}
		[ExpectNoExceptions]
		public void TestRuleCodeListForOFT()
		{
			linkedCusAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.OfficeType;
			NUnit.Framework.Assert.That(lookups.ValueList, NUnit.Framework.Is.EquivalentTo(new LocationQualifierList()).Using(CustomComparers.TypeComparison), "List for OFT");
		}

		protected override void SetUp()
		{
			base.SetUp();
			linkedCusAuthorisationRule = base.Factory.NewWithValidTestData<LinkedCusAuthorisationRule>();
			linkedCusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = "NL";
			lookups = new LinkedCusAuthorisationRuleLookups(linkedCusAuthorisationRule);
		}

		LinkedCusAuthorisationRule linkedCusAuthorisationRule;
		LinkedCusAuthorisationRuleLookups lookups;
	}
}
