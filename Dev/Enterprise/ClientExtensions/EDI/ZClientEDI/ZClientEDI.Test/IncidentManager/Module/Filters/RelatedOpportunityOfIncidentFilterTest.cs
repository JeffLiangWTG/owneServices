using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(RelatedOpportunitiesOfIncidentFilter))]
	class RelatedOpportunityOfIncidentFilterTest : ModuleFilterTestCase<RelatedOpportunitiesOfIncidentFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.GetOrCreateFilterCategory((NoResString)"Related Items");
		protected override RelatedOpportunitiesOfIncidentFilter GetNewModuleFilter()
		{
			var filter = new RelatedOpportunitiesOfIncidentFilter("moo", () => new OrgOpportunityCollection(Factory));
			filter.Category = FilterCategories.GetOrCreateFilterCategory((NoResString)"Related Items");
			return filter;
		}
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(expected: false, Filter.IsExpensiveQuery);
		}
	}
}
