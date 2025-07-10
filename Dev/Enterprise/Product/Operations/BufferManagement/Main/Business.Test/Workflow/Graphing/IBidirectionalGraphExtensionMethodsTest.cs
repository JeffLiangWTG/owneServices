using CargoWise.PAVE.Common.Implementation;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class IBidirectionalGraphExtensionMethodsTest : BMSTestCaseWithFactory
	{
		public void TestGetAllPrerequisites()
		{
			// 1 -> 2 -> 3
			//      4 -> 3
			// 5 -> 6

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4");
			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow5");
			var workflow6 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow6");

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);
			workflow4.GetOrCreateDependencyLink(workflow3);
			workflow5.GetOrCreateDependencyLink(workflow6);

			var graph = JobDependencyGraph.Create(jobHeader).GetGraph();

			AssertContainsExactElementsInAnyOrder(new[] { workflow4, workflow2, workflow1 }, graph.GetAllPrerequisites(new[] { workflow3 }));
			AssertContainsExactElementsInAnyOrder(new[] { workflow5, workflow1 }, graph.GetAllPrerequisites(new[] { workflow6, workflow2 }));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ProcessHeader>(), graph.GetAllPrerequisites(new[] { workflow4, workflow1 }));
			AssertContainsExactElementsInAnyOrder(new[] { workflow1 }, graph.GetAllPrerequisites(new[] { workflow5, workflow2 }));
		}
	}
}
