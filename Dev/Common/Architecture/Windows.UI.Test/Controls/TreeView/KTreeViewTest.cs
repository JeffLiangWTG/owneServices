using System;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KTreeViewTest : TestCase
	{
		public void TestBindUnbind()
		{
			MockMasterObject root = collection.AddNew();
			MockDetailObject child1 = root.DetailObjects.AddNew();
			MockDetailObject child2 = root.DetailObjects.AddNew();

			treeView.SetDataBinding(collection, "DetailObjects");
			AssertEquals("Should bind to 2 child elements", 2, treeView.Nodes.Count);

			treeView.SetDataBinding(null, "");
			AssertEquals("Should have no child nodes now", 0, treeView.Nodes.Count);
		}

		public void TestDontBindUntilControlCreated()
		{
			KTreeView treeView = new KTreeView();
			MockMasterObject master = collection.AddNew();
			MockDetailObject detail1 = master.DetailObjects.AddNew();
			MockDetailObject detail2 = master.DetailObjects.AddNew();
			Form.Controls.Add(treeView);

			treeView.SetDataBinding(collection, "DetailObjects");
			AssertEquals("Shouldnt contain nodes until control is created", 0, treeView.Nodes.Count);

			Form.Show();
			AssertEquals("Should contain nodes once control created", 2, treeView.Nodes.Count);
		}

		public void TestPerformSelect_Cancel()
		{
			using (var treeView = new KTreeView())
			{
				bool afterSelectWasCalled = false;

				var node = treeView.Nodes.Add("key", "text");

				treeView.BeforeSelect += (sender, args) => args.Cancel = true;
				treeView.AfterSelect += (sender, args) => afterSelectWasCalled = true;

				treeView.PerformSelect(node);

				AssertNull("Since it was cancelled, no node should be selected", treeView.SelectedNode);
				Assert("After select should not be called", !afterSelectWasCalled);
			}
		}

		public void TestPerformSelect_EventsAreFired()
		{
			using (var treeView = new KTreeView())
			{
				bool afterSelectWasCalled = false, beforeSelectWasCalled = false;

				var node = treeView.Nodes.Add("key", "text");

				treeView.BeforeSelect += (sender, args) => beforeSelectWasCalled = true;
				treeView.AfterSelect += (sender, args) => afterSelectWasCalled = true;

				treeView.PerformSelect(node);

				Assert("Before select should be called", beforeSelectWasCalled);
				Assert("After select should be called", afterSelectWasCalled);
			}
		}

		public void TestPerformSelect_ItemIsSelected()
		{
			using (var treeView = new KTreeView())
			{
				var node = treeView.Nodes.Add("key", "text");

				treeView.PerformSelect(node);

				AssertEquals(node, treeView.SelectedNode);
			}
		}

		public void TestSettingDataSourceAndDataMember()
		{
			KTreeView view = new KTreeView();
			object source = new object();
			view.DataSource = source;

			AssertEquals(source, view.DataSource);
			view.DataMember = "x";
			AssertEquals(source, view.DataSource);
			AssertEquals("x", view.DataMember);
		}

		public void TestSelectedObject()
		{
			using (KTreeView view = new KTreeView())
			{
				Form.Controls.Add(view);
				Form.Show();

				MockMasterObjectCollection collection = new MockMasterObjectCollection();
				view.SetDataBinding(collection, "");
				MockMasterObject entity1 = collection.AddNew();
				MockMasterObject entity2 = collection.AddNew();

				view.SelectedObject = entity1;
				AssertEquals("First item selected", view.Nodes[0], view.SelectedNode);
				view.SelectedObject = entity2;
				AssertEquals("Second item selected", view.Nodes[1], view.SelectedNode);
				view.SelectedObject = null;
				AssertNull("Null selected", view.SelectedNode);
			}
		}

		public void TestOnActionDelete()
		{
			TestTreeView view = new TestTreeView();
			view.Nodes.Add(new TreeNode("x"));
			view.SelectedNode = view.Nodes[0];
			Form.Controls.Add(view);
			Form.Show();

			AssertEquals("Shouldnt have delete called initially for test", false, view.OnActionDeleteCalled);
			view.OnKeyUp(new KeyEventArgs(Keys.Delete));
			AssertEquals("Should have delete called due to key press", true, view.OnActionDeleteCalled);
		}

		public void TestOnActionOpen_WithKeyPress()
		{
			TestTreeView view = new TestTreeView();
			view.Nodes.Add(new TreeNode("x"));
			view.SelectedNode = view.Nodes[0];
			Form.Controls.Add(view);
			Form.Show();

			AssertEquals("Shouldnt have OnActionDelete called initially for test", false, view.OnActionOpenCalled);
			view.OnKeyPress(new KeyPressEventArgs('\r'));
			AssertEquals("Should have OnActionOpen called due to key press", true, view.OnActionOpenCalled);
		}

		public void TestOnActionOpen_WithDoubleClick()
		{
			TestTreeView view = new TestTreeView();
			view.Nodes.Add(new TreeNode("x"));
			view.SelectedNode = view.Nodes[0];
			Form.Controls.Add(view);
			Form.Show();

			AssertEquals("Shouldnt have OnActionDelete called initially for test", false, view.OnActionOpenCalled);
			view.OnDoubleClick(EventArgs.Empty);
			AssertEquals("Should have OnActionOpen called due to key press", true, view.OnActionOpenCalled);
		}

		#region Test Classes

		class TestTreeView : KTreeView
		{
			public bool OnActionDeleteCalled;
			protected override void OnActionDelete(TreeViewEventArgs e)
			{
				base.OnActionDelete(e);
				OnActionDeleteCalled = true;
			}

			public bool OnActionOpenCalled;
			protected override void OnActionOpen(TreeViewEventArgs e)
			{
				base.OnActionOpen(e);
				OnActionOpenCalled = true;
			}

			public new void OnDoubleClick(EventArgs e)
			{ base.OnDoubleClick(e); }

			internal new void OnKeyPress(KeyPressEventArgs e) => base.OnKeyPress(e);
			internal new void OnKeyUp(KeyEventArgs e) => base.OnKeyUp(e);
		}

		#endregion

		#region Implementation

		KForm formWithTreeView;
		KTreeView treeView;
		TreeNodeCollection nodes;
		MockMasterObjectCollection collection;

		protected override void SetUp()
		{
			base.SetUp();
			this.formWithTreeView = new KForm();
			this.treeView = new KTreeView();
			this.formWithTreeView.Controls.Add(this.treeView);
			this.nodes = this.treeView.Nodes;
			this.collection = new MockMasterObjectCollection();

			this.formWithTreeView.Show();
			AssertEquals("Should have no nodes initially", 0, nodes.Count);
		}

		KForm Form
		{ get { return form ?? (form = new KForm()); } }
		KForm form;

		protected override void TearDown()
		{
			base.TearDown();
			if (formWithTreeView != null)
			{
				formWithTreeView.Dispose();
			}
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
