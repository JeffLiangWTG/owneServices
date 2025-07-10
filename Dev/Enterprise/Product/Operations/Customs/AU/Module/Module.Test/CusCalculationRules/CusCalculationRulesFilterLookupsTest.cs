using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(CusCalculationRulesFilterLookups))]
	sealed class CusCalculationRulesFilterLookupsTest : Customs.Module.Testing.CusCalculationRulesFilterLookupsTest
	{
		public override void TestRuleTypeList()
		{
			AssertEquals("RuleTypeList should equals to the lookups list", cusCalculationRule.Lookups.RuleTypeList, lookups.RuleTypeList);
			AssertEquals("RuleType of correct type", typeof(CusCalculationRuleTypeList), lookups.RuleTypeList.GetType());
			AssertEquals("List Count", 1, lookups.RuleTypeList.Count);
		}

		public override void TestTransportModeList()
		{
			AssertEquals("TransportModeList should equals to the lookups list", cusCalculationRule.Lookups.TransportModeList, lookups.TransportModeList);
			AssertEquals("TransportMode of correct type", typeof(CodeDescriptionPairList), lookups.TransportModeList.GetType());
			AssertEquals("List Count", 3, lookups.TransportModeList.Count);
		}

		CusCalculationRule cusCalculationRule;
		CusCalculationRulesFilterLookups lookups;
		CusCalculationRulesFilterBusinessObject filterBizObj;

		protected override void SetUp()
		{
			base.SetUp();
			cusCalculationRule = Factory.NewWithValidTestData<CusCalculationRule>();
			filterBizObj = new CusCalculationRulesFilterBusinessObject();
			lookups = new CusCalculationRulesFilterLookups(filterBizObj);
		}
	}
}
