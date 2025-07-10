using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRuleTargetCountry))]
	public class ComplianceRuleTargetCountryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTargetCountry()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, "AU");

			var candidate = new ComplianceRuleTargetCountry(country);
			AssertEquals("AU", candidate.Code);
			AssertEquals("Australia", candidate.Name);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, "AU");

			return new ComplianceRuleTargetCountry(country);
		}
	}
}
