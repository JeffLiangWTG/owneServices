using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRuleCandidate))]
	public class ComplianceRuleCandidateTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var newRule = Factory.New<ComplianceRule>();
			newRule.CRU_Origin = "AU";
			newRule.CRU_Destination = "CN";
			newRule.CRU_HarmonizedCode = "123456";
			newRule.CRU_RiskStatus = "BLK";

			return new ComplianceRuleCandidate(newRule);
		}

		public void TestComplianceRule()
		{
			var newRule = Factory.New<ComplianceRule>();
			newRule.CRU_Origin = "AU";
			newRule.CRU_Destination = "CN";
			newRule.CRU_HarmonizedCode = "123456";
			newRule.CRU_RiskStatus = "BLK";

			var candidate = new ComplianceRuleCandidate(newRule);
			AssertEquals("AU", candidate.Origin);
			AssertEquals("CN", candidate.Destination);
			AssertEquals("123456", candidate.HarmonizedCode);
			AssertEquals("BLK", candidate.RiskStatus);
		}
	}
}
