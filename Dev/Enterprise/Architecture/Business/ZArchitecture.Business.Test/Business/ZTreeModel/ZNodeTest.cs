using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ZNodeTest : TestCaseWithFactory
	{
		public void TestFindDescendantNodeInclusive()
		{
			var rootDummy1 = Factory.New<DummyBusinessObject>();
			var rootDummy2 = Factory.New<DummyBusinessObject>();
			var model = new DummyTreeModel(Factory, new[] { rootDummy1, rootDummy2 });
			var rootNode1 = model.RootNodes.First();
			var rootNode2 = model.RootNodes.Skip(1).First();

			var dummyA = Factory.New<DummyBusinessObject>();
			var dummyB = Factory.New<DummyBusinessObject>();
			var dummyC = Factory.New<DummyBusinessObject>();
			var nodeA = rootNode1.AddNewChild(dummyA);
			var nodeB = rootNode1.AddNewChild(dummyB);
			var nodeC = nodeB.AddNewChild(dummyC);

			using (model.GetNodeRefreshSuspender())
			{
				AssertEquals(nodeC, rootNode1.FindDescendantNodeInclusive(dummyC));
				AssertEquals(nodeC, nodeB.FindDescendantNodeInclusive(dummyC));
				AssertEquals(rootNode1, rootNode1.FindDescendantNodeInclusive(rootDummy1));
				AssertNull(nodeA.FindDescendantNodeInclusive(dummyC));
				AssertNull(nodeC.FindDescendantNodeInclusive(rootDummy1));

				AssertNull(rootNode2.FindDescendantNodeInclusive(dummyC));
			}
		}

		public void TestFindDescendantNode()
		{
			var rootDummy = Factory.New<DummyBusinessObject>();
			var model = new DummyTreeModel(Factory, new[] { rootDummy });
			var rootNode = model.RootNodes.Single();

			var dummyA = Factory.New<DummyBusinessObject>();
			var dummyB = Factory.New<DummyBusinessObject>();
			var dummyC = Factory.New<DummyBusinessObject>();
			var nodeA = rootNode.AddNewChild(dummyA);
			var nodeB = rootNode.AddNewChild(dummyB);
			var nodeC = nodeB.AddNewChild(dummyC);

			using (model.GetNodeRefreshSuspender())
			{
				AssertEquals(nodeC, rootNode.FindDescendantNode(dummyC));
				AssertEquals(nodeC, nodeB.FindDescendantNode(dummyC));
				AssertNull(rootNode.FindDescendantNode(rootDummy));
				AssertNull(nodeA.FindDescendantNode(dummyC));
				AssertNull(nodeC.FindDescendantNode(rootDummy));
			}
		}

		public void TestFindAncestorNode()
		{
			var rootDummy = Factory.New<DummyBusinessObject>();
			var model = new DummyTreeModel(Factory, new[] { rootDummy });
			var rootNode = model.RootNodes.Single();

			var dummyA = Factory.New<DummyBusinessObject>();
			var dummyB = Factory.New<DummyBusinessObject>();
			var dummyC = Factory.New<DummyBusinessObject>();
			var nodeA = rootNode.AddNewChild(dummyA);
			var nodeB = rootNode.AddNewChild(dummyB);
			var nodeC = nodeB.AddNewChild(dummyC);

			using (model.GetNodeRefreshSuspender())
			{
				AssertEquals(rootNode, nodeA.FindAncestorNode(rootDummy));
				AssertEquals(nodeB, nodeC.FindAncestorNode(dummyB));
				AssertNull(nodeC.FindAncestorNode(dummyA));
				AssertNull(rootNode.FindAncestorNode(dummyC));
			}
		}

		public void TestSetParentNode_FailRaisesEvent()
		{
			var rootDummy = Factory.New<DummyBusinessObject>();
			var dummyA = Factory.New<DummyBusinessObject>();
			var dummyB = Factory.New<DummyBusinessObject>();
			AddGenPivot(rootDummy, dummyA);
			AddGenPivot(rootDummy, dummyB);

			var model = new DummyTreeModel(Factory, new[] { rootDummy });
			var rootNode = model.RootNodes.Single();
			var nodeA = model.FindNode(dummyA);
			var nodeB = model.FindNode(dummyB);

			bool changeNodeParentFailedEventRaised = false;
			model.ChangeNodeParentFailed += (sender, e) => changeNodeParentFailedEventRaised = true;

			((DummyGenPivotNode)nodeB).ChangeParentOnBizObjOverrideForTesting = new ZNode<DummyBusinessObject>.ChangeParentOnBizObjResult(false, dummyB);
			nodeB.SetParentNode(nodeA, true);

			AssertEquals("ChangeNodeParentFailed Event Raised", true, changeNodeParentFailedEventRaised);
		}

		public void TestSetParentNode_WithChangeParentResultDifferentFromOriginalNodeParent()
		{
			var rootDummy = Factory.New<DummyBusinessObject>();
			var model = new DummyTreeModel(Factory, new[] { rootDummy });
			var rootNode = model.RootNodes.Single() as DummyGenPivotNode;

			var dummyA = Factory.New<DummyBusinessObject>();
			var dummyB = Factory.New<DummyBusinessObject>();
			var dummyC = Factory.New<DummyBusinessObject>();
			var nodeA = (DummyGenPivotNode)rootNode.AddNewChild(dummyA);
			var nodeB = (DummyGenPivotNode)rootNode.AddNewChild(dummyB);
			var nodeC = (DummyGenPivotNode)rootNode.AddNewChild(dummyC);

			nodeC.ChangeParentOnBizObjOverrideForTesting = new ZNode<DummyBusinessObject>.ChangeParentOnBizObjResult(true, dummyB);
			nodeC.SetParentNode(nodeA, false);

			AssertContainsExactElementsInAnyOrder("Shouldn't have added nodeC as child of nodeA because the change parent result was dummyB", Enumerable.Empty<DummyGenPivotNode>(), nodeA.GetChildNodeCollectionWithoutRefresh());
			AssertContainsExactElementsInAnyOrder("Should have added nodeC as child of nodeB because the change parent result was dummyB", new[] { nodeC }, nodeB.GetChildNodeCollectionWithoutRefresh());
			AssertEquals("NodeC parent node should be nodeB", nodeB, nodeC.GetParentNodeWithoutRefresh());
		}

		public void TestSetParentNode_WithNullChangeParentResult()
		{
			var rootDummy = Factory.New<DummyBusinessObject>();
			var model = new DummyTreeModel(Factory, new[] { rootDummy });
			var rootNode = model.RootNodes.Single() as DummyGenPivotNode;

			var dummyA = Factory.New<DummyBusinessObject>();
			var dummyB = Factory.New<DummyBusinessObject>();
			var dummyC = Factory.New<DummyBusinessObject>();
			var nodeA = (DummyGenPivotNode)rootNode.AddNewChild(dummyA);
			var nodeB = (DummyGenPivotNode)rootNode.AddNewChild(dummyB);
			var nodeC = (DummyGenPivotNode)rootNode.AddNewChild(dummyC);

			nodeC.ChangeParentOnBizObjOverrideForTesting = new ZNode<DummyBusinessObject>.ChangeParentOnBizObjResult(true, null);
			nodeC.SetParentNode(nodeA, false);

			AssertContainsExactElementsInAnyOrder("nodeC should no longer be a childNode of RootNode", new[] { nodeA, nodeB }, rootNode.GetChildNodeCollectionWithoutRefresh());
			AssertNull("NodeC parent node should be null", nodeC.GetParentNodeWithoutRefresh());
		}

		public void TestChildNodes_DoNotIncludeChildIfItProducesACycle()
		{
			var dummyA = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummyB = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummyC = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummyZ = Factory.NewWithValidTestData<DummyBusinessObject>();

			AddGenPivot(dummyA, dummyB);
			AddGenPivot(dummyB, dummyC);
			AddGenPivot(dummyC, dummyA);

			AddGenPivot(dummyA, dummyZ);

			Factory.Save();

			var model = new DummyTreeModel(Factory, new[] { dummyA });

			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<DummyBusinessObject>.PKOnlyComparer,
				new DummyBusinessObject[] { dummyB, dummyZ },
				model.FindNode(dummyA).ChildNodes.Select(node => node.BizObj));

			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<DummyBusinessObject>.PKOnlyComparer,
				new DummyBusinessObject[] { dummyC },
				model.FindNode(dummyB).ChildNodes.Select(node => node.BizObj));

			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				Enumerable.Empty<DummyBusinessObject>(),
				model.FindNode(dummyZ).ChildNodes.Select(node => node.BizObj));

			AssertContainsExactElementsInAnyOrder("Should not include dummyA as a child node of dummyC as it will produce a cycle",
				BusinessObjectEqualityComparer<DummyBusinessObject>.PKOnlyComparer,
				Enumerable.Empty<DummyBusinessObject>(),
				model.FindNode(dummyC).ChildNodes.Select(node => node.BizObj));
		}

		GenPivot AddGenPivot(DummyBusinessObject parent, DummyBusinessObject child)
		{
			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = parent.PK;
			pivot.XX_Relation2ID = child.PK;
			return pivot;
		}
	}
}
