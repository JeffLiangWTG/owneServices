namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class NetworkDependencyGraphTest : NetworkTestCase
	{
		public void TestConstructGraph()
		{
			var diagram = CreateDiagram(Factory, name: "diagram");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			var childShape3 = networkViewModel.CreateNewShape(diagram);

			network.CreateRelationship(childShape1, childShape2);
			network.CreateRelationship(childShape2, childShape3);

			var graph = new NetworkDependencyGraph(network);

			AssertEquals("1", graph.GetSequence(childShape1.Shape));
			AssertEquals("2", graph.GetSequence(childShape2.Shape));
			AssertEquals("3", graph.GetSequence(childShape3.Shape));

			AssertEquals(true, graph.IsDirectedAcyclicGraph());
		}

		public void TestConstructGraph_NotDAG()
		{
			var diagram = CreateDiagram(Factory, name: "diagram");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			var childShape3 = networkViewModel.CreateNewShape(diagram);

			network.CreateRelationship(childShape1, childShape2);
			network.CreateRelationship(childShape2, childShape3);

			var circularDependency = Factory.New<BMNCNAttachment>();
			circularDependency.BNA_BNS_FromShape = childShape3.PK;
			circularDependency.BNA_BNS_ToShape = childShape1.PK;
			circularDependency.BNA_BNS_Owner = diagram.PK;

			var graph = new NetworkDependencyGraph(network);

			AssertEquals(false, graph.IsDirectedAcyclicGraph());
		}
	}
}
