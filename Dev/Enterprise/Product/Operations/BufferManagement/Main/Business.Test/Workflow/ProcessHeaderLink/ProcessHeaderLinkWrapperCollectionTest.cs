using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessHeaderLinkWrapperCollectionTest : BMSTestCaseWithFactory
	{
		public void TestLoad()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "IAMAGameChanger");
			var link1 = Factory.New<ProcessHeaderLink>();
			var link2 = Factory.New<ProcessHeaderLink>();

			AssertCollectionNotContains(link1, workflow.Links);
			AssertCollectionNotContains(link2, workflow.Links);
			link1.FP_FH_HeaderTo = workflow.PK;
			link2.FP_FH_HeaderFrom = workflow.PK;

			AssertCollectionContains(link1, workflow.Links);
			AssertCollectionContains(link2, workflow.Links);

			AssertCollectionContains(link2, workflow.Links_ForBinding.FromHeaderLinks);
			AssertCollectionContains(link1, workflow.Links_ForBinding.ToHeaderLinks);
		}
	}

	public static class ProcessHeaderLinkWrapperCollectionTestExtensions
	{
		public static ProcessHeaderLink AddNew(this ProcessHeaderLinkWrapperCollection collection)
		{
			var link = collection.Parent.Factory.New<ProcessHeaderLink>();
			ProcessHeaderLinkCollection.SetDefaultsForNewElement(link, collection.Parent, collection.Parent.JobHeader, ZString.Empty, null);
			return link;
		}
	}
}
