using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(DummyTreeModel))]
	sealed class ZTreeModelTest : NonPersistentBusinessObjectTestCase
	{
		#region Node

		public void TestCreateNewNode()
		{
			var model = new DummyTreeModel(Factory, new[] { GetNewBizObjForTest() });
			var bizObj = GetNewBizObjForTest();

			var node = model.CreateNewNode(bizObj);
			AssertEquals(bizObj, node.BizObj);
		}

		public void TestFindNode()
		{
			AssertEquals("RootNodeA", RootNodeA.BizObj, Model.FindNode(RootNodeA.BizObj).BizObj);
			AssertEquals("NodeA1", NodeA1.BizObj, Model.FindNode(NodeA1.BizObj).BizObj);
			AssertEquals("NodeA1_1", NodeA1_1.BizObj, Model.FindNode(NodeA1_1.BizObj).BizObj);
			AssertEquals("NodeA1_2", NodeA1_2.BizObj, Model.FindNode(NodeA1_2.BizObj).BizObj);
			AssertEquals("NodeA2", NodeA2.BizObj, Model.FindNode(NodeA2.BizObj).BizObj);
			AssertEquals("NodeA2_1", NodeA2_1.BizObj, Model.FindNode(NodeA2_1.BizObj).BizObj);
			AssertEquals("NodeA2_2", NodeA2_2.BizObj, Model.FindNode(NodeA2_2.BizObj).BizObj);
			AssertEquals("NodeA2_2_1", NodeA2_2_1.BizObj, Model.FindNode(NodeA2_2_1.BizObj).BizObj);
			AssertEquals("NodeA2_3", NodeA2_3.BizObj, Model.FindNode(NodeA2_3.BizObj).BizObj);
		}

		public void TestToPath()
		{
			AssertArrayEqualsByElements("RootNodeA", new[] { RootNodeA }, RootNodeA.ToPath().FullPath);
			AssertArrayEqualsByElements("NodeA1", new[] { RootNodeA, NodeA1 }, NodeA1.ToPath().FullPath);
			AssertArrayEqualsByElements("NodeA1_1", new[] { RootNodeA, NodeA1, NodeA1_1 }, NodeA1_1.ToPath().FullPath);
			AssertArrayEqualsByElements("NodeA1_2", new[] { RootNodeA, NodeA1, NodeA1_2 }, NodeA1_2.ToPath().FullPath);
			AssertArrayEqualsByElements("NodeA2", new[] { RootNodeA, NodeA2 }, NodeA2.ToPath().FullPath);
			AssertArrayEqualsByElements("NodeA2_1", new[] { RootNodeA, NodeA2, NodeA2_1 }, NodeA2_1.ToPath().FullPath);
			AssertArrayEqualsByElements("NodeA2_2", new[] { RootNodeA, NodeA2, NodeA2_2 }, NodeA2_2.ToPath().FullPath);
			AssertArrayEqualsByElements("NodeA2_2_1", new[] { RootNodeA, NodeA2, NodeA2_2, NodeA2_2_1 }, NodeA2_2_1.ToPath().FullPath);
			AssertArrayEqualsByElements("NodeA2_3", new[] { RootNodeA, NodeA2, NodeA2_3 }, NodeA2_3.ToPath().FullPath);
		}

		#endregion

		#region GetChildren

		public void TestGetChildren()
		{
			AssertContainsExactElementsInAnyOrder("RootNodeA", new[] { NodeA1.BizObj, NodeA2.BizObj }, Model.GetChildNodes(RootNodeA.ToPath()).Cast<ZNode<DummyBusinessObject>>().Select(node => node.BizObj));
			AssertContainsExactElementsInAnyOrder("NodeA1", new[] { NodeA1_1.BizObj, NodeA1_2.BizObj }, Model.GetChildNodes(NodeA1.ToPath()).Cast<ZNode<DummyBusinessObject>>().Select(node => node.BizObj));
			AssertContainsExactElementsInAnyOrder("NodeA1_1", System.Array.Empty<ZNode<DummyBusinessObject>>(), Model.GetChildNodes(NodeA1_1.ToPath()).Cast<ZNode<DummyBusinessObject>>().Select(node => node.BizObj));
			AssertContainsExactElementsInAnyOrder("NodeA1_2", System.Array.Empty<ZNode<DummyBusinessObject>>(), Model.GetChildNodes(NodeA1_2.ToPath()).Cast<ZNode<DummyBusinessObject>>().Select(node => node.BizObj));
			AssertContainsExactElementsInAnyOrder("NodeA2", new[] { NodeA2_1.BizObj, NodeA2_2.BizObj, NodeA2_3.BizObj }, Model.GetChildNodes(NodeA2.ToPath()).Cast<ZNode<DummyBusinessObject>>().Select(node => node.BizObj));
			AssertContainsExactElementsInAnyOrder("NodeA2_1", System.Array.Empty<ZNode<DummyBusinessObject>>(), Model.GetChildNodes(NodeA2_1.ToPath()).Cast<ZNode<DummyBusinessObject>>().Select(node => node.BizObj));
			AssertContainsExactElementsInAnyOrder("NodeA2_2", new[] { NodeA2_2_1.BizObj }, Model.GetChildNodes(NodeA2_2.ToPath()).Cast<ZNode<DummyBusinessObject>>().Select(node => node.BizObj));
			AssertContainsExactElementsInAnyOrder("NodeA2_2_1", System.Array.Empty<ZNode<DummyBusinessObject>>(), Model.GetChildNodes(NodeA2_2_1.ToPath()).Cast<ZNode<DummyBusinessObject>>().Select(node => node.BizObj));
			AssertContainsExactElementsInAnyOrder("NodeA2_3", System.Array.Empty<ZNode<DummyBusinessObject>>(), Model.GetChildNodes(NodeA2_3.ToPath()).Cast<ZNode<DummyBusinessObject>>().Select(node => node.BizObj));
		}

		#endregion

		#region IsLeaf

		public void TestIsLeaf()
		{
			CombineAssertions(() =>
			{
				AssertEquals("RootNodeA", false, Model.IsLeaf(RootNodeA.ToPath()));
				AssertEquals("NodeA1", false, Model.IsLeaf((NodeA1.ToPath())));
				AssertEquals("NodeA1_1", true, Model.IsLeaf(NodeA1_1.ToPath()));
				AssertEquals("NodeA1_2", true, Model.IsLeaf(NodeA1_2.ToPath()));
				AssertEquals("NodeA2", false, Model.IsLeaf(NodeA2.ToPath()));
				AssertEquals("NodeA2_1", true, Model.IsLeaf(NodeA2_1.ToPath()));
				AssertEquals("NodeA2_2", false, Model.IsLeaf(NodeA2_2.ToPath()));
				AssertEquals("NodeA2_2_1", true, Model.IsLeaf(NodeA2_2_1.ToPath()));
				AssertEquals("NodeA2_3", true, Model.IsLeaf(NodeA2_3.ToPath()));

				AssertEquals("RootNodeB", true, Model.IsLeaf(RootNodeB.ToPath()));
			});
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Model = new DummyTreeModel(Factory, new[] { GetNewBizObjForTest(), GetNewBizObjForTest() });
			RootNodeA = Model.RootNodes.First();
			RootNodeB = Model.RootNodes.Skip(1).First();

			NodeA1 = RootNodeA.AddNewChild(GetNewBizObjForTest());
			{
				NodeA1_1 = NodeA1.AddNewChild(GetNewBizObjForTest());
				NodeA1_2 = NodeA1.AddNewChild(GetNewBizObjForTest());
			}

			NodeA2 = RootNodeA.AddNewChild(GetNewBizObjForTest());
			{
				NodeA2_1 = NodeA2.AddNewChild(GetNewBizObjForTest());
				NodeA2_2 = NodeA2.AddNewChild(GetNewBizObjForTest());
				{
					NodeA2_2_1 = NodeA2_2.AddNewChild(GetNewBizObjForTest());
				}

				NodeA2_3 = NodeA2.AddNewChild(GetNewBizObjForTest());
			}

			Factory.Save();
		}

		DummyTreeModel Model;
		ZNode<DummyBusinessObject> RootNodeA;
		ZNode<DummyBusinessObject> RootNodeB;
		ZNode<DummyBusinessObject> NodeA1;
		ZNode<DummyBusinessObject> NodeA1_1;
		ZNode<DummyBusinessObject> NodeA1_2;
		ZNode<DummyBusinessObject> NodeA2;
		ZNode<DummyBusinessObject> NodeA2_1;
		ZNode<DummyBusinessObject> NodeA2_2;
		ZNode<DummyBusinessObject> NodeA2_2_1;
		ZNode<DummyBusinessObject> NodeA2_3;

		DummyBusinessObject GetNewBizObjForTest()
		{
			return Factory.New<DummyBusinessObject>();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var root = Factory.New<DummyBusinessObject>();
			return new DummyTreeModel(Factory, new[] { root });
		}

		#endregion
	}
}
