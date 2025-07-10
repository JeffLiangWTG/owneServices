using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business.Testing
{
	public class CustomsRuleRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRuleCodes()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rule = customsRule.Rules.AddNew();
			var lookups = rule.Lookups;
			AssertEquals("CustomsRuleRuleCodeList", typeof(CustomsRuleRuleCodeList), lookups.RuleCodes.GetType());
			AssertEquals("CustomsRuleRuleCodeList count", 4, lookups.RuleCodes.Count);
		}

		public void TestValueFromCodes()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rule = customsRule.Rules.AddNew();
			var lookups = rule.Lookups;
			AssertEquals("Default", typeof(CodeDescriptionPairList), lookups.ValueFromCodes.GetType());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
			AssertEquals("TotalCustomsDisbursement", typeof(CodeDescriptionPairList), lookups.ValueFromCodes.GetType());

			rule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsValue;
			AssertEquals("TotalCustomsValue", typeof(CodeDescriptionPairList), lookups.ValueFromCodes.GetType());
		}

		IDisposable countryDisposition;
		protected override void SetUp()
		{
			base.SetUp();
			countryDisposition = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada);
		}

		protected override void TearDown()
		{
			base.TearDown();
			countryDisposition.Dispose();
		}
	}
}
