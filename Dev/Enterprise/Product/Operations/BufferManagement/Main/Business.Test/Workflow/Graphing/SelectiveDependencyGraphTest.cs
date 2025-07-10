using CargoWise.PAVE.Common.Implementation;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class SelectiveDependencyGraphTest : BMSTestCaseWithFactory
	{
		public void TestGraph()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = CreateWorkflow(jobHeader, "workflow4");
			var workflow5 = CreateWorkflow(jobHeader, "workflow5");

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);
			workflow3.GetOrCreateDependencyLink(workflow4);

			workflow5.GetOrCreateDependencyLink(workflow4);

			// 1 -> 2 -> 3 -> 4
			//           5 -> 4

			var graph = new SelectiveDependencyGraph(new[] { workflow1, workflow3, workflow5 }).GetGraph();

			AssertEquals("workflow1 has no prereqs", true, graph.IsInEdgesEmpty(workflow1));
			AssertEquals("workflow1 has postreqs (indirectly to workflow3)", false, graph.IsOutEdgesEmpty(workflow1));

			AssertEquals("workflow3 has prereqs (indirectly from workflow1)", false, graph.IsInEdgesEmpty(workflow3));
			AssertEquals("workflow3 has no postreqs inside the graph", true, graph.IsOutEdgesEmpty(workflow3));

			AssertEquals("workflow5 has no prereqs", true, graph.IsInEdgesEmpty(workflow5));
			AssertEquals("workflow5 has no postreqs inside the graph", true, graph.IsOutEdgesEmpty(workflow5));
		}

		public void TestPrerequisitesAndPostrequisitesUsingTheMagicOfQuickgraph()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = CreateWorkflow(jobHeader, "workflow4");
			var workflow5 = CreateWorkflow(jobHeader, "workflow5");

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);
			workflow3.GetOrCreateDependencyLink(workflow4);

			workflow5.GetOrCreateDependencyLink(workflow4);

			// 1 -> 2 -> 3 -> 4
			//           5 -> 4

			var graph = new SelectiveDependencyGraph(new[] { workflow1, workflow3, workflow5 }).GetGraph();

			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ProcessHeader>(), workflow1.Prerequisites(graph));
			AssertContainsExactElementsInAnyOrder(new[] { workflow3 }, workflow1.Postrequisites(graph));

			AssertContainsExactElementsInAnyOrder(new[] { workflow1 }, workflow3.Prerequisites(graph));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ProcessHeader>(), workflow3.Postrequisites(graph));

			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ProcessHeader>(), workflow5.Prerequisites(graph));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ProcessHeader>(), workflow5.Postrequisites(graph));

			// And again without QuickGraph. Should use the entire network, rather than selective network.
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ProcessHeader>(), workflow1.Prerequisites());
			AssertContainsExactElementsInAnyOrder(new[] { workflow2 }, workflow1.Postrequisites());

			AssertContainsExactElementsInAnyOrder(new[] { workflow2 }, workflow3.Prerequisites());
			AssertContainsExactElementsInAnyOrder(new[] { workflow4 }, workflow3.Postrequisites());

			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ProcessHeader>(), workflow5.Prerequisites());
			AssertContainsExactElementsInAnyOrder(new[] { workflow4 }, workflow5.Postrequisites());
		}

		public void TestGraphDoesNotAddRedundantLinksByDefault()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);

			// 1 -> 2 -> 3

			var graph = new SelectiveDependencyGraph(new[] { workflow1, workflow2, workflow3 }).GetGraph();

			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ProcessHeader>(), workflow1.Prerequisites(graph));
			AssertContainsExactElementsInAnyOrder(new[] { workflow2 }, workflow1.Postrequisites(graph));

			AssertContainsExactElementsInAnyOrder(new[] { workflow1 }, workflow2.Prerequisites(graph));
			AssertContainsExactElementsInAnyOrder(new[] { workflow3 }, workflow2.Postrequisites(graph));

			AssertContainsExactElementsInAnyOrder(new[] { workflow2 }, workflow3.Prerequisites(graph));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ProcessHeader>(), workflow3.Postrequisites(graph));
		}

		public void TestGraphCanSupportRedundantLinks()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);
			workflow1.GetOrCreateDependencyLink(workflow3);

			// 1 -> 2 -> 3
			// 1 ------> 3

			var graph = new SelectiveDependencyGraph(new[] { workflow1, workflow2, workflow3 }).GetGraph();

			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ProcessHeader>(), workflow1.Prerequisites(graph));
			AssertContainsExactElementsInAnyOrder(new[] { workflow2, workflow3 }, workflow1.Postrequisites(graph));

			AssertContainsExactElementsInAnyOrder(new[] { workflow1 }, workflow2.Prerequisites(graph));
			AssertContainsExactElementsInAnyOrder(new[] { workflow3 }, workflow2.Postrequisites(graph));

			AssertContainsExactElementsInAnyOrder(new[] { workflow1, workflow2 }, workflow3.Prerequisites(graph));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ProcessHeader>(), workflow3.Postrequisites(graph));
		}
	}
}
