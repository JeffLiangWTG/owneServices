using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.ReflectiveFieldMap;
using DocumentWrapperForTest = Enterprise.DocumentEngine.ReflectiveFieldMap.Testing.DocumentWrapperForTest;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap.Testing
{
	sealed class MapTreeUserControlTest : TestCaseWithFactory
	{
		public void TestConstructionAndLoading()
		{
			using (var userControl = new MapTreeUserControl())
			{
				AssertEquals("Precondition: userControl.mapTreeView.Nodes.Count", 0, userControl.mapTreeView.Nodes.Count);
				userControl.SetReflectors([new DocDataProviderReflector(typeof(DocumentWrapperForTest))]);
				AssertNotEquals("userControl.mapTreeView.Nodes.Count", 0, userControl.mapTreeView.Nodes.Count);
			}
		}

		public void TestExpandingTreeViewNode()
		{
			using (var userControl = new MapTreeUserControl())
			{
				userControl.SetReflectors([new DocDataProviderReflector(typeof(DocumentWrapperForTest))]);
				AssertEquals("Precondition: userControl.mapTreeView.Nodes[0] is collapsed", false, userControl.mapTreeView.Nodes[0].IsExpanded);
				userControl.ExpandTreeViewNode(1);
				AssertEquals("Precondition: userControl.mapTreeView.Nodes[1] non-existent node", false, userControl.mapTreeView.Nodes[0].IsExpanded);
				userControl.ExpandTreeViewNode(0);
				AssertEquals("userControl.mapTreeView.Nodes[0] is expanded", true, userControl.mapTreeView.Nodes[0].IsExpanded);
			}
		}

		public void TestSetReflectors()
		{
			using (var userControl = new MapTreeUserControl())
			{
				userControl.SetReflectors([new DocDataProviderReflector(typeof(DocumentWrapperForTest)), new DocDataProviderReflector(typeof(DummyBusinessObject))]);
				var node1 = userControl.mapTreeView.Nodes[0];
				AssertEquals("Node.Tag", typeof(DocumentWrapperForTest), (Type)node1.Tag);
				AssertEquals("Node.Tag", "Data Source Type: DocumentWrapperForTest\r", node1.Text);

				var node2 = userControl.mapTreeView.Nodes[1];
				AssertEquals("Node.Tag", typeof(DummyBusinessObject), (Type)node2.Tag);
				AssertEquals("Node.Tag", "Data Source Type: DummyBusinessObject\r", node2.Text);
			}
		}
	}
}
