using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Integration.Tests
{
	class LinkOrgHeaderToWorkItemTest : LinkEntityTest<OrgHeader, SalesEnquiry>
	{
	}

	class LinkWorkItemToOrgHeaderTest : LinkEntityTest<SalesEnquiry, OrgHeader>
	{
	}

	class LinkWorkItemToWorkItemTest : LinkEntityTest<SalesEnquiry, SalesEnquiry>
	{
	}

	abstract class LinkEntityTest<TEntity1, TEntity2> : NetworkTestCase
			where TEntity1 : BusinessObject, IWorkflowProvider
			where TEntity2 : BusinessObject, IWorkflowProvider
	{
		public void TestLinkEntity()
		{
			var diagram = CreateDiagram(Factory, name: "Root");
			var jobHeader1 = CreateJobHeader<TEntity1>(false);
			CreateWorkflow(jobHeader1, "Child Workflow1");
			var jobHeader2 = CreateJobHeader<TEntity2>(false);
			CreateWorkflow(jobHeader2, "Child Workflow2");

			Factory.Save();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var subDiagram1 = networkViewModel.CreateNewShape(diagram);
			var subDiagram2 = networkViewModel.CreateNewShape(diagram);

			network.LinkEntity(subDiagram1, jobHeader1);
			network.LinkEntity(subDiagram2, jobHeader2);

			network.CreateRelationship(subDiagram1, subDiagram2);

			Factory.Save();

			Assert(subDiagram1.PostRequisiteEntities.Select(s => s.PK).Contains(subDiagram2.PK));
			Assert(jobHeader1.IsPrerequisiteRecursiveOf(jobHeader2));
		}
	}
}
