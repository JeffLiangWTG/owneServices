using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class DefaultJobNetworkTest : NetworkTestCase
	{
		public void TestEntities_ShouldAutoPopulateFromWorkflows()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var diagram = jobHeader.GetDefaultDiagram();
			var network = CreateNetwork(diagram);

			AssertEquals(3, network.Entities.Count);
			AssertCollectionContains(workflow1.GetDefaultShape(diagram), network.Shapes);
			AssertCollectionContains(workflow2.GetDefaultShape(diagram), network.Shapes);
			AssertCollectionContains(workflow3.GetDefaultShape(diagram), network.Shapes);
		}
	}
}
