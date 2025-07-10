using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.CRM.Common.Testing
{
	[TestedType(typeof(CrmOpportunityScope))]
	public class CrmOpportunityScopeTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("CrmOpportunityScope is not used in CW1, only in GLOW", true);
		}

		[DeveloperOnlyTest]
		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			Assert("CrmOpportunityScope is not used in CW1, only in GLOW", true);
		}

		[DeveloperOnlyTest]
		public override void TestFetchForLoad()
		{
			Assert("CrmOpportunityScope is not used in CW1, only in GLOW", true);
		}
	}
}
