using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class CusCalculationRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRuleTypeList()
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();
			var lookups = cusCalculationRule.Lookups;
			CombineAssertions(() =>
			{
				AssertType<CusCalculationRuleTypeList>("RuleTypeList: type", lookups.RuleTypeList);
				AssertEquals("RuleTypeList: count", 1, lookups.RuleTypeList.Count);
			});
			var cusCalculationRule2 = Factory.New<CusCalculationRule>();
			Assert("RuleTypeList should be cached in same factory", ReferenceEquals(cusCalculationRule.Lookups.RuleTypeList, cusCalculationRule2.Lookups.RuleTypeList));
		}

		public void TestTransportModeList()
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();
			var lookups = cusCalculationRule.Lookups;
			CombineAssertions(() =>
			{
				AssertType<CodeDescriptionPairList>("TransportModeList: type", lookups.TransportModeList);
				AssertEquals("TransportModeList: count", 3, lookups.TransportModeList.Count);
			});
			var cusCalculationRule2 = Factory.New<CusCalculationRule>();
			Assert("TransportModeList should be cached in same factory", ReferenceEquals(cusCalculationRule.Lookups.TransportModeList, cusCalculationRule2.Lookups.TransportModeList));
		}

		public void TestBasedOnList()
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();
			var lookups = cusCalculationRule.Lookups;
			CombineAssertions(() =>
			{
				AssertType<CodeDescriptionPairList>("BasedOnList: type", lookups.BasedOnList);
				AssertEquals("BasedOnList: count", 2, lookups.BasedOnList.Count);
			});
			var cusCalculationRule2 = Factory.New<CusCalculationRule>();
			Assert("BasedOnList should be cached in same factory", ReferenceEquals(cusCalculationRule.Lookups.BasedOnList, cusCalculationRule2.Lookups.BasedOnList));

			cusCalculationRule.CCR_RuleType = "";
			AssertEquals("When rule type is not INS, BasedOnList: count", 0, lookups.BasedOnList.Count);
		}
	}
}
