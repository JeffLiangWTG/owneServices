using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRuleCandidateCollection))]
	public class ComplianceRuleCandidateCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ComplianceRuleCandidateCollection>
	{
		protected override ComplianceRuleCandidateCollection GetCollectionToTest()
		{
			return new ComplianceRuleCandidateCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var newRule = Factory.New<ComplianceRule>();
			newRule.CRU_Origin = "AU";
			newRule.CRU_Destination = "CN";
			newRule.CRU_HarmonizedCode = "123456";

			return new ComplianceRuleCandidate(newRule);
		}

		public void TestAllowNew()
		{
			var result = new ComplianceRuleCandidateCollection();
			AssertEquals(false, result.AllowNew);
		}
	}
}
