using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.CRM.Common.Testing
{
	public class CrmOpportunityLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatuses()
		{
			var collection = new GlowOpportunityStatusCollection
			{
				{ "ABC", (NoResString)"ABC Description" },
				{ "XYZ", (NoResString)"XYZ Description" }
			};

			OrganisationsDataRegistry.Instance.GlowOpportunityStatuses.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var opportunity = Factory.New<CrmOpportunity>();

			AssertEquals("Count", 2, opportunity.Lookups.Statuses.Count);
			AssertEquals("GetDescriptionFromCode(\"ABC\"", "ABC Description", opportunity.Lookups.Statuses.GetDescriptionFromCode("ABC"));
			AssertEquals("GetDescriptionFromCode(\"XYZ\"", "XYZ Description", opportunity.Lookups.Statuses.GetDescriptionFromCode("XYZ"));
		}
	}
}
