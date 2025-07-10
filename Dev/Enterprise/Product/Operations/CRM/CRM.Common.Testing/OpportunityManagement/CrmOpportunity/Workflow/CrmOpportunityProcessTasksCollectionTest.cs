using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.CRM.Common.Testing
{
	[TestedType(typeof(CrmOpportunityProcessTasksCollection))]
	sealed class CrmOpportunityProcessTasksCollectionTest : ProcessTaskCollectionTest<CrmOpportunityProcessTasksCollection>
	{
		protected override CrmOpportunityProcessTasksCollection GetCollectionToTestCore()
		{
			var opportunity = Factory.NewWithValidTestData<CrmOpportunity>();
			return new CrmOpportunityProcessTasksCollection(opportunity);
		}
	}
}
