using System.ComponentModel;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class TreeNodeCollectionBinderTest : TestCase
	{
		public void TestBind()
		{
			MockMasterObject entity1 = collection.AddNew();
			AddDetailEntities(entity1);
			AssertEquals("1 node added", 1, nodes.Count);
			entity1.StringProperty2 = "entity1";
			AssertEquals("1 node added", "entity1", nodes[0].Text);

			MockMasterObject entity2 = collection.AddNew();
			AddDetailEntities(entity2);
			AssertEquals("2 nodes added", 2, nodes.Count);
			entity2.StringProperty2 = "entity2";
			AssertEquals("2 nodes added", "entity1", nodes[0].Text);
			AssertEquals("2 nodes added", "entity2", nodes[1].Text);

			entity2.StringProperty2 = "renamed";
			AssertEquals("node renamed", "renamed", nodes[1].Text);
			entity2.StringProperty2 = "entity2";

			collection.Remove(entity1);
			AssertEquals("1 node deleted, 1 remaining", 1, nodes.Count);
			AssertEquals("Only second node should be remaining", "entity2", nodes[0].Text);
		}

		public void TestSelectedNodeStaysSelectedOnItemChanged()
		{
			MockMasterObject entity1 = collection.AddNew();
			AddDetailEntities(entity1);
			MockMasterObject entity2 = collection.AddNew();
			AddDetailEntities(entity2);
			MockMasterObject entity3 = collection.AddNew();
			AddDetailEntities(entity3);

			treeView.SelectedNode = nodes[1];
			collection.FireListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, 1));

			entity1.StringProperty2 = "a";
			entity2.StringProperty2 = "b";
			entity3.StringProperty2 = "c";
			AssertEquals("Selected node should have been renamed", "b", treeView.SelectedNode.Text);
			AssertEquals("Selected node should remain selected", nodes[1], treeView.SelectedNode);
		}

		public void TestCurrencyPropagatesFromCurrencyManagerToSelectedNode()
		{
			MockMasterObject entity1 = collection.AddNew();
			AddDetailEntities(entity1);
			MockMasterObject entity2 = collection.AddNew();
			AddDetailEntities(entity2);
			MockMasterObject entity3 = collection.AddNew();
			AddDetailEntities(entity3);
			entity1.StringProperty2 = "1";
			entity1.StringProperty2 = "2";
			entity1.StringProperty2 = "3";

			treeView.BindingContext[collection].Position = 1;
			AssertEquals("Selected node should be 1", nodes[1], treeView.SelectedNode);
			treeView.BindingContext[collection].Position = 2;
			AssertEquals("Selected node should be 2", nodes[2], treeView.SelectedNode);
		}

		public void TestCurrencyPropagatesFromSelectedNodeToCurrencyManager()
		{
			MockMasterObject entity1 = collection.AddNew();
			AddDetailEntities(entity1);
			MockMasterObject entity2 = collection.AddNew();
			AddDetailEntities(entity2);
			MockMasterObject entity3 = collection.AddNew();
			AddDetailEntities(entity3);

			treeView.SelectedNode = nodes[1];
			AssertEquals("Position should be 1", 1, treeView.BindingContext[collection].Position);
			nodes[2].Expand();
			treeView.SelectedNode = nodes[2].Nodes[0];
			AssertEquals("Position should be 2, even though it was a child node selected", 2, treeView.BindingContext[collection].Position);
			treeView.SelectedNode = null;
			AssertEquals("Position should still be 2 even though none selected now", 2, treeView.BindingContext[collection].Position);
		}

		[ExpectNoExceptions]
		public void TestSettingPositionToNeg1()
		{
			collection.AddNew();
			collection.AddNew();

			treeView.BindingContext[collection].Position = 0;
			treeView.BindingContext[collection].Position = 1;
			collection.RemoveAt(0);
			collection.RemoveAt(0);
			treeView.BindingContext[collection].Position = -1;
		}

		public void TestDisplayPropertyName_Null()
		{
			this.binder.StopBinding();
			this.binder = new TreeNodeCollectionBinder(
				treeView, treeView.Nodes, collection, "", null);
			binder.StartBinding();

			this.treeView.NextNodeText = "whatever";
			MockMasterObject entity = collection.AddNew();
			AssertEquals("Should just use the text from NewTreeNode", "whatever", this.treeView.Nodes[0].Text);
			entity.StringProperty2 = "splaty"; // expect no exception
			AssertEquals("Should just use the text from NewTreeNode", "whatever", this.treeView.Nodes[0].Text);
		}

		public void TestDisplayPropertyName_Empty()
		{
			this.binder.StopBinding();
			this.binder = new TreeNodeCollectionBinder(treeView, collection, "", "");
			this.binder.StartBinding();

			MockMasterObject entity = collection.AddNew();
			AssertEquals("Should just go element.ToString()", entity.ToString(), nodes[0].Text);
		}

		public void TestChildNodesPopulated()
		{
			MockMasterObject entity1 = collection.AddNew();
			AddDetailEntities(entity1);
			MockMasterObject entity2 = collection.AddNew();
			AddDetailEntities(entity2);
			MockMasterObject entity3 = collection.AddNew();
			AddDetailEntities(entity3);

			for (int i = 0; i < nodes.Count; i++)
			{
				AssertEquals("Should have 1 placeholder node initially", 1, nodes[i].Nodes.Count);
				nodes[i].Expand();
				nodes[i].Collapse();
				AssertEquals("Should have 2 nodes now", 2, nodes[i].Nodes.Count);
				AssertEquals("Correct node text", "item1", nodes[i].Nodes[0].Text);
				AssertEquals("Correct node text", "item2", nodes[i].Nodes[1].Text);
			}

			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i].Expand();
				AssertEquals("Should still have 2 nodes", 2, nodes[i].Nodes.Count);
				AssertEquals("Correct node text", "item1", nodes[i].Nodes[0].Text);
				AssertEquals("Correct node text", "item2", nodes[i].Nodes[1].Text);
			}
		}

		public void TestWithNullChildBinder()
		{
			MockMasterObject master = collection.AddNew();
			MockDetailObject detail = master.DetailObjects.AddNew();
			detail.DetailDetailObjects.AddNew();

			nodes[0].Expand();
			nodes[0].Nodes[0].Expand();

			AssertEquals("Should have 1 master", 1, this.nodes.Count);
			AssertEquals("Should have 1 child", 1, this.nodes[0].Nodes.Count);
			AssertEquals("Should have 1 child child", 1, this.nodes[0].Nodes[0].Nodes.Count);
			AssertEquals(
				"Should have no placeholder thereafter as there should be a null binder",
				0, this.nodes[0].Nodes[0].Nodes[0].Nodes.Count);
		}

		public void TestResetListRetainsExpandedNodeStates()
		{
			for (int i = 0; i < 3; i++)
			{
				MockMasterObject master = collection.AddNew();
				master.StringProperty2 = "master" + i;
				for (int j = 0; j < 3; j++)
				{
					MockDetailObject detail = master.DetailObjects.AddNew();
					detail.StringProperty = "detail" + j;
					for (int k = 0; k < 3; k++)
					{
						MockDetailDetailObject detaildetail = detail.DetailDetailObjects.AddNew();
						detaildetail.StringProperty = "detaildetail" + k;
					}
				}
			}

			treeView.Nodes[1].Expand();
			treeView.Nodes[1].Nodes[1].Expand();
			TreeNode oldSelected = treeView.Nodes[1].Nodes[1].Nodes[1];
			treeView.SelectedNode = oldSelected;

			collection.ListChanged += new ListChangedEventHandler(Collection_ListChanged);
			collection.ResetBindings();
			AssertEquals(
				"Reset should have been called for this test",
				ListChangedType.Reset, lastListChanged.ListChangedType);
			AssertNotEquals(
				"Selected node should be a different instance due to the list changed for this test",
				oldSelected, treeView.SelectedNode);
			AssertEquals("Should have selected the node in the same position", treeView.Nodes[1].Nodes[1].Nodes[1], treeView.SelectedNode);

			AssertEquals(false, treeView.Nodes[0].IsExpanded);
			AssertEquals(true, treeView.Nodes[1].IsExpanded);
			AssertEquals(false, treeView.Nodes[2].IsExpanded);

			AssertEquals(false, treeView.Nodes[1].Nodes[0].IsExpanded);
			AssertEquals(true, treeView.Nodes[1].Nodes[1].IsExpanded);
			AssertEquals(false, treeView.Nodes[1].Nodes[2].IsExpanded);

			AssertEquals(false, treeView.Nodes[1].Nodes[1].Nodes[0].IsExpanded);
			AssertEquals(false, treeView.Nodes[1].Nodes[1].Nodes[1].IsExpanded);
			AssertEquals(false, treeView.Nodes[1].Nodes[1].Nodes[2].IsExpanded);
		}

		#region GetNodeFromItem / GetItemFromNode

		public void TestGetNodeFromItem()
		{
			MockMasterObject decoy = collection.AddNew();
			MockMasterObject master = collection.AddNew();
			MockDetailObject detail = master.DetailObjects.AddNew();
			detail.StringProperty = "detail";
			MockDetailDetailObject detaildetail = detail.DetailDetailObjects.AddNew();
			detaildetail.StringProperty = "detaildetail";

			binder.StartBinding();
			treeView.Nodes[1].Expand();
			treeView.Nodes[1].Nodes[0].Expand();

			TreeNode node;
			node = binder.GetNodeFromItem(detail);
			AssertEquals("detail", node.Text);
			node = binder.GetNodeFromItem(detaildetail);
			AssertEquals("detaildetail", node.Text);
		}

		public void TestGetItemFromNode()
		{
			MockMasterObject decoy = collection.AddNew();
			MockMasterObject master = collection.AddNew();
			MockDetailObject detail = master.DetailObjects.AddNew();
			detail.StringProperty = "detail";
			MockDetailDetailObject detaildetail = detail.DetailDetailObjects.AddNew();
			detaildetail.StringProperty = "detaildetail";

			binder.StartBinding();
			treeView.Nodes[1].Expand();
			treeView.Nodes[1].Nodes[0].Expand();

			object item;
			item = binder.GetItemFromNode(treeView.Nodes[1]);
			Assert(item is MockMasterObject);
			item = binder.GetItemFromNode(treeView.Nodes[1].Nodes[0]);
			Assert(item is MockDetailObject);
		}

		public void TestGetItemFromNode_GetNodeFromItem_WhenNodesAreNested()
		{
			for (int i = 0; i < 3; i++)
			{
				MockMasterObject master = collection.AddNew();
				master.StringProperty2 = "master" + i;
				for (int j = 0; j < 3; j++)
				{
					MockDetailObject detail = master.DetailObjects.AddNew();
					detail.StringProperty = "detail" + j;
					for (int k = 0; k < 3; k++)
					{
						MockDetailDetailObject detaildetail = detail.DetailDetailObjects.AddNew();
						detaildetail.StringProperty = "detaildetail" + k;
					}
				}
			}

			treeView.Nodes[1].Expand();
			treeView.Nodes[1].Nodes[1].Expand();
			TreeNode selectedNode = treeView.Nodes[1].Nodes[1].Nodes[1];
			treeView.SelectedNode = selectedNode; // after this statement, selectedNode.Parent becomes null (!)
			selectedNode = treeView.Nodes[1].Nodes[1].Nodes[1];

			MockDetailDetailObject selectedItem = (MockDetailDetailObject)treeView.GetItemFromNode(selectedNode);
			AssertEquals("GetItemFromNode when nodes are nested", "detaildetail1", selectedItem.StringProperty);
			TreeNode selectedNodeFromItem = treeView.GetNodeFromItem(selectedItem);
			AssertEquals("GetNodeFromItem when nodes are nested", selectedNode, selectedNodeFromItem);
		}

		#endregion

		#region LabelEdit

		public void TestLabelEdit_WithoutDisplayMember()
		{
			MockMasterObject entity = collection.AddNew();

			this.binder.StopBinding();
			this.binder = new TreeNodeCollectionBinder(
				treeView, treeView.Nodes, collection, "");
			binder.StartBinding();

			NodeLabelEditEventArgs e = new NodeLabelEditEventArgs(treeView.Nodes[0]);
			treeView.OnBeforeLabelEdit(e);
			AssertEquals("Should be cancelled as you can't edit a non-existant display member", true, e.CancelEdit);
		}

		public void TestLabelEdit_WithReadOnlyDisplayMember()
		{
			MockMasterObject entity = collection.AddNew();
			NodeLabelEditEventArgs e;

			entity.StringProperty2ReadOnly = false;
			e = new NodeLabelEditEventArgs(treeView.Nodes[0]);
			treeView.OnBeforeLabelEdit(e);
			AssertEquals("Shouldnt be cancelled as it is not read only", false, e.CancelEdit);

			entity.StringProperty2ReadOnly = true;
			e = new NodeLabelEditEventArgs(treeView.Nodes[0]);
			treeView.OnBeforeLabelEdit(e);
			AssertEquals("Should be cancelled as it is read only", true, e.CancelEdit);
		}

		public void TestLabelEdit_IEditableObjectEndEditOnObject()
		{
			EditableMockMasterObject entity = new EditableMockMasterObject();
			collection.Add(entity);
			NodeLabelEditEventArgs e;

			entity.StringProperty2ReadOnly = false;
			e = new NodeLabelEditEventArgs(treeView.Nodes[0], "x");
			((IEditableObject)entity).BeginEdit();
			treeView.OnBeforeLabelEdit(e);
			treeView.OnAfterLabelEdit(e);
			AssertEquals("Object should have IEditableObject.EndEdit called on it", false, entity.IsEdit);
		}

		public void TestLabelEdit_DontChangeIfLabelNullAfterEdit()
		{
			MockMasterObject entity = collection.AddNew();

			this.binder.StopBinding();
			this.binder = new TreeNodeCollectionBinder(
				treeView, treeView.Nodes, collection, "");
			binder.StartBinding();

			entity.StringProperty2 = "splaty";
			NodeLabelEditEventArgs e;
			e = new NodeLabelEditEventArgs(treeView.Nodes[0], "splaty");
			treeView.OnBeforeLabelEdit(e);
			e = new NodeLabelEditEventArgs(treeView.Nodes[0], null);
			treeView.OnAfterLabelEdit(e);
			AssertEquals("Should not have changed", "splaty", entity.StringProperty2);
		}

		// this doesnt actually test the problem properly, but not sure if that is possible..
		[ExpectNoExceptions]
		public void TestLabelEdit_WhenNodeJustGotDeleted()
		{
			MockMasterObject entity = collection.AddNew();

			this.binder.StopBinding();
			this.binder = new TreeNodeCollectionBinder(treeView, treeView.Nodes, collection, "");
			binder.StartBinding();

			entity.StringProperty2 = "splaty";
			NodeLabelEditEventArgs e;
			e = new NodeLabelEditEventArgs(treeView.Nodes[0], "splaty");
			treeView.OnBeforeLabelEdit(e);
			e = new NodeLabelEditEventArgs(treeView.Nodes[0], null);

			// expect no exception
			e.Node.Remove();
			treeView.OnAfterLabelEdit(e);
		}

		#endregion

		#region FirstNode / LastNode / FirstNodeIndex / AppendNodesTo

		public void TestFirstNodeLastNode()
		{
			this.treeView.Nodes.Add(new TreeNode("node_before_bound_list"));
			this.treeView.Nodes.Add(new TreeNode("node_after_bound_list"));
			this.treeView.RootBinder.FirstNodeIndex = 1;

			MockMasterObject entity1 = this.collection.AddNew();
			entity1.StringProperty2 = "1";
			AssertNodes("node_before_bound_list", "1", "node_after_bound_list");
			AssertEquals("FirstNode with 1 bound entity", this.treeView.Nodes[1], this.binder.FirstNode);
			AssertEquals("LastNode with 1 bound entity", this.treeView.Nodes[1], this.binder.LastNode);

			MockMasterObject entity2 = this.collection.AddNew();
			MockMasterObject entity3 = this.collection.AddNew();
			entity2.StringProperty2 = "2";
			entity3.StringProperty2 = "3";
			AssertNodes("node_before_bound_list", "1", "2", "3", "node_after_bound_list");
			AssertEquals("FirstNode with 3 bound entities", this.treeView.Nodes[1], this.binder.FirstNode);
			AssertEquals("LastNode with 3 bound entities", this.treeView.Nodes[3], this.binder.LastNode);

			this.collection.Remove(entity3);
			AssertNodes("node_before_bound_list", "1", "2", "node_after_bound_list");
			AssertEquals("LastNode just after the last bound entity is deleted", this.treeView.Nodes[2], this.binder.LastNode);

			this.collection.Clear();
			AssertNodes("node_before_bound_list", "node_after_bound_list");
			AssertEquals("When there are no bound tree nodes", null, this.binder.FirstNode);
			AssertEquals("When there are no bound tree nodes", null, this.binder.LastNode);
		}

		public void TestFirstNodeIndex()
		{
			this.treeView.Nodes.Add(new TreeNode("node_before_bound_list"));
			this.treeView.Nodes.Add(new TreeNode("node_after_bound_list"));

			this.treeView.RootBinder.FirstNodeIndex = 1;
			MockMasterObject entity = this.collection.AddNew();
			entity.StringProperty2 = "text";
			AssertNodes("node_before_bound_list", "text", "node_after_bound_list");

			this.treeView.RootBinder.FirstNodeIndex = 2;
			this.treeView.Nodes.Insert(1, new TreeNode("node_before_bound_list2"));
			entity.StringProperty2 = "text2";
			AssertNodes("node_before_bound_list", "node_before_bound_list2", "text2", "node_after_bound_list");

			this.collection.Remove(entity);
			entity = this.collection.AddNew();
			entity.StringProperty2 = "text3";
			AssertNodes("node_before_bound_list", "node_before_bound_list2", "text3", "node_after_bound_list");

			this.treeView.RootBinder.FirstNodeIndex = 1;
			this.treeView.Nodes[1].Remove();
			entity.StringProperty2 = "text4";
			AssertNodes("node_before_bound_list", "text4", "node_after_bound_list");

			this.collection.Remove(entity);
			entity = this.collection.AddNew();
			entity.StringProperty2 = "text5";
			AssertNodes("node_before_bound_list", "text5", "node_after_bound_list");
		}

		public void TestAppendNodesTo()
		{
			MockMasterObjectCollection collection1 = new MockMasterObjectCollection();
			MockMasterObjectCollection collection2 = new MockMasterObjectCollection();

			TreeNodeCollectionBinder binder1 = new TreeNodeCollectionBinder(treeView, collection1, "", "StringProperty2");
			TreeNodeCollectionBinder binder2 = new TreeNodeCollectionBinder(treeView, collection2, "", "StringProperty2");
			binder2.AppendNodesTo(binder1);
			binder1.StartBinding();
			binder2.StartBinding();
			AssertEquals("Should have no nodes initially", 0, nodes.Count);

			MockMasterObject collection1_entity1 = collection1.AddNew();
			collection1_entity1.StringProperty2 = "collection1_entity1";
			AssertNodes("collection1_entity1");

			MockMasterObject collection2_entity1 = collection2.AddNew();
			collection2_entity1.StringProperty2 = "collection2_entity1";
			AssertNodes("collection1_entity1", "collection2_entity1");

			MockMasterObject collection1_entity2 = collection1.AddNew();
			collection1_entity2.StringProperty2 = "collection1_entity2";
			AssertNodes("collection1_entity1", "collection1_entity2", "collection2_entity1");

			MockMasterObject collection2_entity2 = collection2.AddNew();
			collection2_entity2.StringProperty2 = "collection2_entity2";
			AssertNodes("collection1_entity1", "collection1_entity2", "collection2_entity1", "collection2_entity2");

			collection1.Remove(collection1_entity1);
			AssertNodes("collection1_entity2", "collection2_entity1", "collection2_entity2");

			collection1.Remove(collection1_entity2);
			AssertNodes("collection2_entity1", "collection2_entity2");

			collection2.Remove(collection2_entity1);
			AssertNodes("collection2_entity2");

			collection2.Remove(collection2_entity2);
			AssertNodes(System.Array.Empty<string>());
		}

		void AssertNodes(params string[] treeNodeTextStrings)
		{
			AssertEquals("Incorrect number of nodes", treeNodeTextStrings.Length, nodes.Count);
			for (int i = 0; i < treeNodeTextStrings.Length; i++)
			{
				AssertEquals("Node text at index " + i, treeNodeTextStrings[i], nodes[i].Text);
			}
		}

		#endregion

		#region Test Classes

		class TestTreeView : KTreeView
		{
			public ITreeNodeCollectionBinder RootBinder;
			public string NextNodeText = "";

			public new void OnBeforeLabelEdit(NodeLabelEditEventArgs e)
			{ base.OnBeforeLabelEdit(e); }

			public new void OnAfterLabelEdit(NodeLabelEditEventArgs e)
			{ base.OnAfterLabelEdit(e); }

			protected internal override void OnQueryNewTreeNode(QueryNewTreeNodeEventArgs e)
			{
				base.OnQueryNewTreeNode(e);
				e.NewTreeNode.Text = NextNodeText;
			}

			protected override void OnQueryNodeDataBindings(QueryTreeNodeDataBindingsEventArgs e)
			{
				if (e.ParentItem == null)
				{
					e.NodeBinders.Add(DataSource, "", "StringProperty2");
					RootBinder = e.NodeBinders[0];
				}
				MockMasterObject master = e.ParentItem as MockMasterObject;
				if (master != null)
				{
					e.NodeBinders.Add(master.DetailObjects, "", MockDetailObject.Properties.StringProperty.Name);
				}
				MockDetailObject detail = e.ParentItem as MockDetailObject;
				if (detail != null)
				{
					e.NodeBinders.Add(detail.DetailDetailObjects, "", MockDetailDetailObject.Properties.StringProperty.Name);
				}
			}
		}

		class EditableMockMasterObject : MockMasterObject, IEditableObject
		{
			public bool IsEdit { get; private set; }

			public void BeginEdit()
			{
				IsEdit = true;
			}

			public void CancelEdit()
			{
				IsEdit = false;
			}

			public void EndEdit()
			{
				IsEdit = false;
			}
		}

		#endregion

		#region Implementation

		KForm form;
		TestTreeView treeView;
		TreeNodeCollectionBinder binder;
		TreeNodeCollection nodes;
		MockMasterObjectCollection collection;

		void AddDetailEntities(MockMasterObject entity)
		{
			MockDetailObject detail1 = entity.DetailObjects.AddNew();
			detail1.StringProperty = "item1";
			MockDetailObject detail2 = entity.DetailObjects.AddNew();
			detail2.StringProperty = "item2";
		}

		ListChangedEventArgs lastListChanged;
		void Collection_ListChanged(object sender, ListChangedEventArgs e)
		{ lastListChanged = e; }

		protected override void SetUp()
		{
			base.SetUp();
			this.form = new KForm();
			this.treeView = new TestTreeView();
			this.nodes = this.treeView.Nodes;
			this.collection = new MockMasterObjectCollection();
			this.treeView.SetDataBinding(collection, "");
			this.form.Controls.Add(this.treeView);

			this.form.Show();
			this.binder = (TreeNodeCollectionBinder)treeView.RootBinder;
			this.binder.StartBinding();
			AssertEquals("Should have no nodes initially", 0, nodes.Count);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
