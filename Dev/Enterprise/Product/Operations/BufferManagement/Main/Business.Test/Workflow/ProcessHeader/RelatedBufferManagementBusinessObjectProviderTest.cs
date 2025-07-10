using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class RelatedBufferManagementBusinessObjectProviderTest : BMSTestCaseWithFactory
	{
		public void TestBusinessObjectsWithRelatedEvents_ShouldFindJobHeaderAndWorkflows()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var relatedBizos = org.BusinessObjectsWithRelatedEvents;
			AssertCollectionContains(jobHeader, relatedBizos);
			AssertCollectionContains(workflow1, relatedBizos);
			AssertCollectionContains(workflow2, relatedBizos);
		}
	}
}
