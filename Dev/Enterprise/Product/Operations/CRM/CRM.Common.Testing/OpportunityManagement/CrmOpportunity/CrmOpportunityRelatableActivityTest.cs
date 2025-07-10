using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.CRM.Common.Testing
{
	[TestedType(typeof(CrmOpportunity))]
	public class CrmOpportunityRelatableActivityTest : RelatableActivityTestCase<CrmOpportunity>
	{
		protected override CrmOpportunity GetNewActivity()
		{
			return Factory.NewWithValidTestData<CrmOpportunity>();
		}

		public void TestRelatableActivityValues()
		{
			var opportunity = GetNewActivity();
			opportunity.COP_OpportunityName = "opp name";
			opportunity.COP_ProductType = "T1";
			opportunity.COP_SalesType = "T2";

			var collection = new GlowOpportunityStatusCollection
			{
				{ "OPN", (NoResString)"Open" },
				{ "CLS", (NoResString)"Cloesd" }
			};
			OrganisationsDataRegistry.Instance.GlowOpportunityStatuses.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			opportunity.COP_Status = "OPN";

			AssertEquals(RelatableActivityTypeList.Codes.CrmOpportunityManager, opportunity.ActivityType);
			AssertEquals("opp name; T1; T2; Open", opportunity.Summary);
			AssertEquals(false, opportunity.ShouldIgnoreSuperAndSubActivityRelationships);
			AssertEquals(true, opportunity.SupportViewRelatedCommunications);
		}
	}
}
