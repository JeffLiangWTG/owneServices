using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRuleTargetCountryCollection))]
	public class ComplianceRuleTargetCountryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ComplianceRuleTargetCountryCollection>
	{
		protected override ComplianceRuleTargetCountryCollection GetCollectionToTest()
		{
			return new ComplianceRuleTargetCountryCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, "AU");

			return new ComplianceRuleTargetCountry(country);
		}

		public void TestAllowNew()
		{
			var result = new ComplianceRuleCandidateCollection();
			AssertEquals(false, result.AllowNew);
		}
	}
}
