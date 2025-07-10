using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.UniversalCopy;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalCopy.GUI.Testing
{
	public class UniversalCopyTemplateTreeViewTest : TestCaseWithFactory
	{
		public void TestGetBizoNodeImageIndex()
		{
			using (var treeView = new UniversalCopyTemplateTreeView())
			{
				AssertEquals(UniversalCopyTemplateTreeView.ImageIndexCopy, treeView.GetBizoNodeImageIndex(new CopyTemplateTreeBizo(new CopyTemplateTree(), Factory.New<UniversalCopyTemplate>())));

				var relatedBizo = new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode(), null, null);
				AssertEquals(UniversalCopyTemplateTreeView.ImageIndexNone, treeView.GetBizoNodeImageIndex(relatedBizo));
				relatedBizo.CopyTemplateNode.CopyMethod = RelatedEntityCopyMethod.Copy;
				AssertEquals(UniversalCopyTemplateTreeView.ImageIndexCopy, treeView.GetBizoNodeImageIndex(relatedBizo));
				relatedBizo.CopyTemplateNode.CopyMethod = RelatedEntityCopyMethod.Link;
				AssertEquals(UniversalCopyTemplateTreeView.ImageIndexLink, treeView.GetBizoNodeImageIndex(relatedBizo));
				relatedBizo.CopyTemplateNode.CopyMethod = RelatedEntityCopyMethod.LinkCopied;
				AssertEquals(UniversalCopyTemplateTreeView.ImageIndexLink, treeView.GetBizoNodeImageIndex(relatedBizo));

				var collectionBizo = new CollectionCopyTemplateBizo(new CollectionCopyTemplateNode(), null, null);
				AssertEquals(UniversalCopyTemplateTreeView.ImageIndexNone, treeView.GetBizoNodeImageIndex(collectionBizo));
				collectionBizo.CopyTemplateNode.CopyMethod = CollectionCopyMethod.All;
				AssertEquals(UniversalCopyTemplateTreeView.ImageIndexCopy, treeView.GetBizoNodeImageIndex(collectionBizo));
				collectionBizo.CopyTemplateNode.CopyMethod = CollectionCopyMethod.Filter;
				AssertEquals(UniversalCopyTemplateTreeView.ImageIndexFilter, treeView.GetBizoNodeImageIndex(collectionBizo));
			}
		}

		public void TestAddRelatedNode()
		{
			using (var treeView = new UniversalCopyTemplateTreeView())
			{
				AssertEquals("Precondition", 0, treeView.Nodes.Count);

				var relatedBizo = new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "RelatedElement" }, null, null);
				var newNode = treeView.AddNode(treeView.Nodes, relatedBizo);
				AssertEquals(1, treeView.Nodes.Count);

				AssertEquals("Related Element", newNode.Text);
				AssertSame(relatedBizo, newNode.BizO);
				AssertEquals(UniversalCopyTemplateTreeView.ImageIndexRelatedEntity, newNode.StateImageIndex);
			}
		}

		public void TestAddCollectionNode()
		{
			using (var treeView = new UniversalCopyTemplateTreeView())
			{
				AssertEquals("Precondition", 0, treeView.Nodes.Count);

				var collectionBizo = new CollectionCopyTemplateBizo(new CollectionCopyTemplateNode { Name = "CollectionElement" }, null, null);
				var newNode = treeView.AddNode(treeView.Nodes, collectionBizo);
				AssertEquals(1, treeView.Nodes.Count);

				AssertEquals("Collection Element", newNode.Text);
				AssertSame(collectionBizo, newNode.BizO);
				AssertEquals(UniversalCopyTemplateTreeView.ImageIndexCollection, newNode.StateImageIndex);
			}
		}

		public void TestAddNodes()
		{
			using (var treeView = new UniversalCopyTemplateTreeView())
			{
				var parentNode = new TreeNode();
				AssertEquals("Precondition", 0, parentNode.Nodes.Count);

				var entityNode = new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "RootElement", InnerNode = new EntityCopyTemplateNode() }, null, null);
				entityNode.ChildNodes.Add(new CollectionCopyTemplateBizo(new CollectionCopyTemplateNode { Name = "CollectionElement1" }, null, null));
				entityNode.ChildNodes.Add(new CollectionCopyTemplateBizo(new CollectionCopyTemplateNode { Name = "CollectionElement2" }, null, null));
				entityNode.ChildNodes.Add(new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "RelatedElement1" }, null, null));
				entityNode.ChildNodes.Add(new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "RelatedElement2" }, null, null));

				treeView.AddNodes(parentNode.Nodes, entityNode);
				AssertEquals(4, parentNode.Nodes.Count);
			}
		}

		public void TestSplitNodes()
		{
			using (var treeView = new UniversalCopyTemplateTreeView())
			{
				treeView.TreeViewNodeSorter = new NodeSorter();
				treeView.Sorted = true;
				var bizO = new CopyTemplateTreeBizo(new CopyTemplateTree(), Factory.New<UniversalCopyTemplate>());
				var parentNode = new UniversalCopyTreeNode(bizO, "parent");

				AssertEquals("Precondition", 0, parentNode.Nodes.Count);

				var entityNode = new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "RootElement", InnerNode = new EntityCopyTemplateNode() }, null, null);
				entityNode.ChildNodes.Add(
					new CollectionCopyTemplateBizo(
						new CollectionCopyTemplateNode
						{
							Name = "CollectionElement1",
							InnerNode = new EntityCopyTemplateNode(),
							ItemsTableName = DummyBizoSchema.Constants.TableName
						},
						null, entityNode));

				treeView.Nodes.Add(parentNode);
				treeView.AddNodes(parentNode.Nodes, entityNode);

				var originalNode = parentNode.Nodes[0] as UniversalCopyTreeNode;
				var collectionNode = parentNode.Nodes[0] as UniversalCopyTreeNode;

				using (var copyManager = new UniversalCopyManagerTest.UniversalCopyManagerForTest(typeof(DummyBusinessObject), DummyModuleIDs.Dummy))
				{
					treeView.CopyManager = copyManager;

					treeView.DuplicateCollectionNodeBizo(collectionNode, false);

					var newNodes = parentNode.Nodes;

					AssertEquals(2, newNodes[0].Nodes.Count);
					AssertEquals("Valid user string entered and returned for testing", newNodes[0].Nodes[0].Text);
					AssertEquals("Unfiltered rest", newNodes[0].Nodes[1].Text);
					AssertEquals(originalNode.BizO, ((UniversalCopyTreeNode)newNodes[0].Nodes[1]).BizO);
					AssertEquals(originalNode.BizO, ((UniversalCopyTreeNode)newNodes[0]).VisualNodeBizo);

					UnitTestUserNotification.Instance.AddUserResponse("New Split Collection");

					((UniversalCopyTreeNode)newNodes[0].Nodes[1]).ContextMenu.MenuItems[0].PerformClick();

					AssertEquals(3, newNodes[0].Nodes.Count);
					AssertEquals("New Split Collection", newNodes[0].Nodes[0].Text);
					AssertEquals("Valid user string entered and returned for testing", newNodes[0].Nodes[1].Text);

					UnitTestUserNotification.Instance.AddUserResponse("ZZZ late in alphabet split collection");

					((UniversalCopyTreeNode)newNodes[0]).ContextMenu.MenuItems[0].PerformClick();

					AssertEquals(4, newNodes[0].Nodes.Count);
					AssertEquals("ZZZ late in alphabet split collection", newNodes[0].Nodes[2].Text);
					AssertEquals("Unfiltered rest", newNodes[0].Nodes[3].Text);

					var removeAll = ((UniversalCopyTreeNode)newNodes[0]).ContextMenu.MenuItems[1];

					AssertEquals("Remove all split collections", removeAll.Text);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					removeAll.PerformClick();

					AssertEquals(0, newNodes[0].Nodes.Count);
				}
			}
		}

		public void TestSplitCollectionNoAvailableFilter()
		{
			using (var treeView = new UniversalCopyTemplateTreeView())
			{
				treeView.TreeViewNodeSorter = new NodeSorter();
				treeView.Sorted = true;
				var bizO = new CopyTemplateTreeBizo(new CopyTemplateTree(), Factory.New<UniversalCopyTemplate>());
				var parentNode = new UniversalCopyTreeNode(bizO, "parent");

				AssertEquals("Precondition", 0, parentNode.Nodes.Count);

				var entityNode = new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "RootElement", InnerNode = new EntityCopyTemplateNode() }, null, null);
				entityNode.ChildNodes.Add(
					new CollectionCopyTemplateBizo(
						new CollectionCopyTemplateNode
						{
							Name = "CollectionElement1",
							InnerNode = new EntityCopyTemplateNode(),
							ItemsTableName = "SomeFakeTableName"
						},
						null, entityNode));

				treeView.Nodes.Add(parentNode);
				treeView.AddNodes(parentNode.Nodes, entityNode);

				var collectionNode = parentNode.Nodes[0] as UniversalCopyTreeNode;

				using (var copyManager = new UniversalCopyManagerTest.UniversalCopyManagerForTest(typeof(DummyBusinessObject), DummyModuleIDs.Dummy))
				{
					treeView.CopyManager = copyManager;
					treeView.DuplicateCollectionNodeBizo(collectionNode, false);

					var newNodes = parentNode.Nodes;

					AssertEquals("Should not split collection node when filters are not found", 0, newNodes[0].Nodes.Count);
					AssertEquals("Cannot split collection CollectionElement1 to filtered parts: filters list not found.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestSplitNodesOrderAfterEditDescription()
		{
			using (var treeView = new UniversalCopyTemplateTreeView())
			{
				treeView.TreeViewNodeSorter = new NodeSorter();
				treeView.Sorted = true;
				var bizO = new CopyTemplateTreeBizo(new CopyTemplateTree(), Factory.New<UniversalCopyTemplate>());
				var parentNode = new UniversalCopyTreeNode(bizO, "parent");

				AssertEquals("Precondition", 0, parentNode.Nodes.Count);

				var entityNode = new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "RootElement", InnerNode = new EntityCopyTemplateNode() }, null, null);
				entityNode.ChildNodes.Add(
					new CollectionCopyTemplateBizo(
						new CollectionCopyTemplateNode
						{
							Name = "CollectionElement1",
							InnerNode = new EntityCopyTemplateNode(),
							ItemsTableName = DummyBizoSchema.Constants.TableName
						},
						null, entityNode));

				treeView.Nodes.Add(parentNode);
				treeView.AddNodes(parentNode.Nodes, entityNode);

				var collectionNode = parentNode.Nodes[0] as UniversalCopyTreeNode;

				using (var copyManager = new UniversalCopyManagerTest.UniversalCopyManagerForTest(typeof(DummyBusinessObject), DummyModuleIDs.Dummy))
				{
					treeView.CopyManager = copyManager;

					treeView.DuplicateCollectionNodeBizo(collectionNode, false);
					var newNodes = parentNode.Nodes;
					var treeNode = (UniversalCopyTreeNode)newNodes[0];

					const string nodeText1 = "BB New Split Collection";
					const string nodeText2 = "Valid user string entered and returned for testing";
					const string nodeText3 = "ZZ late in alphabet split collection";
					const string nodeText4 = "Unfiltered rest";

					UnitTestUserNotification.Instance.AddUserResponse(nodeText1);
					treeNode.Nodes[1].ContextMenu.MenuItems[0].PerformClick();

					UnitTestUserNotification.Instance.AddUserResponse(nodeText3);
					treeNode.ContextMenu.MenuItems[0].PerformClick();

					AssertEquals(4, treeNode.Nodes.Count);
					AssertEquals(nodeText1, treeNode.Nodes[0].Text);
					AssertEquals(nodeText2, treeNode.Nodes[1].Text);
					AssertEquals(nodeText3, treeNode.Nodes[2].Text);
					AssertEquals(nodeText4, treeNode.Nodes[3].Text);
					AssertEquals("Selected node should be the last added node", nodeText3, treeView.SelectedNode.Text);

					const string newDescription = "FF move item to second";
					treeNode.Nodes[2].Text = newDescription;
					((CopyTemplateNodeBizo)((UniversalCopyTreeNode)newNodes[0].Nodes[2]).BizO).Description = newDescription;

					AssertEquals(nodeText1, treeNode.Nodes[0].Text);
					AssertEquals(newDescription, treeNode.Nodes[1].Text);
					AssertEquals(nodeText2, treeNode.Nodes[2].Text);
					AssertEquals(nodeText4, treeNode.Nodes[3].Text);
					AssertEquals("Selected node should be the edited node", newDescription, treeView.SelectedNode.Text);

					var removeAll = treeNode.ContextMenu.MenuItems[1];
					AssertEquals("Remove all split collections", removeAll.Text);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					removeAll.PerformClick();

					AssertEquals(0, newNodes[0].Nodes.Count);
				}
			}
		}

		public void TestUpdateTreeNodeNameEventHandler()
		{
			using (var treeView = new UniversalCopyTemplateTreeView())
			using (var ucTemplateUserControl = new UniversalCopyTemplateUserControl())
			using (var manager = new UniversalCopyManagerTest.UniversalCopyManagerForTest(typeof(DummyBusinessObject), DummyModuleIDs.Dummy))
			using (var elementDetails = new CollectionNodeDetailsUserControl(manager, true))
			{
				var parentNode = new TreeNode();
				AssertEquals("Precondition", 0, parentNode.Nodes.Count);

				elementDetails.zTextBox1.Leave += ucTemplateUserControl.ZTextBox1_Leave;

				var entityNode = new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "RootElement", InnerNode = new EntityCopyTemplateNode() }, null, null);
				entityNode.ChildNodes.Add(new CollectionCopyTemplateBizo(new CollectionCopyTemplateNode { Name = "CollectionElement1" }, null, null));
				entityNode.ChildNodes.Add(new CollectionCopyTemplateBizo(new CollectionCopyTemplateNode { Name = "CollectionElement2" }, null, null));
				entityNode.ChildNodes.Add(new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "RelatedElement1" }, null, null));
				entityNode.ChildNodes.Add(new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "RelatedElement2" }, null, null));

				treeView.AddNodes(treeView.Nodes, entityNode);

				ucTemplateUserControl.templateTreeView = treeView;

				ucTemplateUserControl.templateTreeView.SelectedNode = treeView.Nodes[0];

				elementDetails.zTextBox1.Text = "new text";

				AssertEquals("Display text of node is the original name", "Collection Element 1", treeView.Nodes[0].Text);
				ucTemplateUserControl.ZTextBox1_Leave(elementDetails.zTextBox1, new EventArgs());
				AssertEquals("Display text has been updated", "new text", treeView.Nodes[0].Text);
			}
		}

		public void TestGetElementsNameFullPathToNode()
		{
			using (var treeView = new UniversalCopyTemplateTreeView())
			{
				var node1 = treeView.AddNode(treeView.Nodes, new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "E1" }, null, null));
				var node2 = treeView.AddNode(node1.Nodes, new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "E2" }, null, null));
				var node3 = treeView.AddNode(node1.Nodes, new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "E3" }, null, null));
				var node4 = treeView.AddNode(node3.Nodes, new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "E4" }, null, null));
				var node5 = treeView.AddNode(node4.Nodes, new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "E5" }, null, null));
				var node6 = treeView.AddNode(node2.Nodes, new RelatedEntityCopyTemplateBizo(new RelatedEntityCopyTemplateNode { Name = "E6" }, null, null));

				AssertEquals("E1", string.Join(".", treeView.GetElementsNameFullPathToNode(node1)));
				AssertEquals("E1.E2", string.Join(".", treeView.GetElementsNameFullPathToNode(node2)));
				AssertEquals("E1.E3", string.Join(".", treeView.GetElementsNameFullPathToNode(node3)));
				AssertEquals("E1.E3.E4", string.Join(".", treeView.GetElementsNameFullPathToNode(node4)));
				AssertEquals("E1.E3.E4.E5", string.Join(".", treeView.GetElementsNameFullPathToNode(node5)));
				AssertEquals("E1.E2.E6", string.Join(".", treeView.GetElementsNameFullPathToNode(node6)));
			}
		}
	}
}
