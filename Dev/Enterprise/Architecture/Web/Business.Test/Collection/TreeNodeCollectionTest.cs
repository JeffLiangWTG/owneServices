using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business
{
	sealed class TreeNodeCollectionTest : TestCase
	{
		TreeNode TestRootNode;
		TreeNodeCollection TestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			TestRootNode = new TreeNode("Test Node");
			TestCollection = new TreeNodeCollection(TestRootNode);
		}

		public void TestInitialCollectionIsEmpty()
		{
			AssertEquals(0, TestCollection.Count);
		}

		public void TestOwnerIsSet()
		{
			AssertEquals(TestCollection.Owner, TestRootNode);
		}

		public void TestAddStringNode()
		{
			AssertEquals(0, TestCollection.Count);
			TestCollection.Add("New Node");
			AssertEquals(1, TestCollection.Count);
			AssertEquals("New Node", TestCollection[0].Text);
		}

		public void TestAddNode()
		{
			AssertEquals(0, TestCollection.Count);
			TreeNode newNode = new TreeNode("New Node");
			int returnedCount = TestCollection.Add(newNode);
			AssertEquals(1, returnedCount);
			AssertEquals(1, TestCollection.Count);
			AssertEquals(newNode, TestCollection[0]);
		}

		public void TestAddedNodeHasParentLinked()
		{
			AssertEquals(0, TestCollection.Count);
			TreeNode parentNode = new TreeNode("ParentNode");
			TreeNode newNode = new TreeNode("New Node");
			AssertNull(newNode.Parent);

			TestCollection.Owner = parentNode;
			AssertEquals(parentNode, TestCollection.Owner);

			TestCollection.Add(newNode);
			AssertEquals(parentNode, newNode.Parent);
		}

		public void TestIndexer()
		{
			AssertEquals(0, TestCollection.Count);
			TreeNode node1 = new TreeNode("Node 1");
			TreeNode node2 = new TreeNode("Node 2");

			TestCollection.Add(node1);
			AssertEquals(1, TestCollection.Count);

			TestCollection.Add(node2);
			AssertEquals(2, TestCollection.Count);

			AssertEquals(node1, TestCollection[0]);
			AssertEquals(node2, TestCollection[1]);
		}

		public void TestFind()
		{
			AssertEquals(0, TestCollection.Count);

			string root = "Node1";
			string child1 = "Child1";
			string child2 = "Child2";
			string grandChild1 = "GrandChild1";
			string grandChild2 = "GrandChild2";
			string grandChild3 = "GrandChild3";
			string grandChild4 = "GrandChild4";

			TreeNode rootNode = new TreeNode(root);
			TreeNode childNode1 = new TreeNode(child1);
			TreeNode childNode2 = new TreeNode(child2);
			TreeNode grandChildNode1 = new TreeNode(grandChild1);
			TreeNode grandChildNode2 = new TreeNode(grandChild2);
			TreeNode grandChildNode3 = new TreeNode(grandChild3);
			TreeNode grandChildNode4 = new TreeNode(grandChild4);

			childNode1.Nodes.Add(grandChildNode1);
			childNode1.Nodes.Add(grandChildNode2);
			childNode2.Nodes.Add(grandChildNode3);
			childNode2.Nodes.Add(grandChildNode4);

			AssertEquals(2, childNode1.Nodes.Count);
			AssertEquals(2, childNode2.Nodes.Count);
			AssertEquals(0, rootNode.Nodes.Count);

			TestCollection.Add(rootNode);
			AssertEquals(rootNode, TestCollection.Find(root));
			AssertEquals(rootNode, TestCollection.Find(root, false));
			AssertEquals(rootNode, TestCollection.Find(root, true));

			AssertNull(TestCollection.Find(child1));
			AssertNull(TestCollection.Find(child1, false));
			AssertNull(TestCollection.Find(child1, true));

			TestCollection.Add(childNode1);
			TestCollection.Add(childNode2);

			AssertNull(TestCollection.Find(grandChild1));
			AssertNull(TestCollection.Find(grandChild1, false));
			AssertEquals(grandChildNode1, TestCollection.Find(grandChild1, true));
		}
	}
}
