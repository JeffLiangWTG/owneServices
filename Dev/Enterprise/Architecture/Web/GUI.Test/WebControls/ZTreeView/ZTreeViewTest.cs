using Enterprise.ZArchitecture.Web.GUI.Testing;
using TreeNode = Enterprise.ZArchitecture.Web.Business.TreeNode;
using TreeNodeCollection = Enterprise.ZArchitecture.Web.Business.TreeNodeCollection;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZTreeViewTest : WebControlTest
	{
		#region Setup

		TreeNode TestTreeNode;
		TreeNodeCollection TestTreeNodeCollection;

		protected override void SetUp()
		{
			base.SetUp();
			TestTreeNode = new TreeNode();
			TestTreeNodeCollection = new TreeNodeCollection(TestTreeNode);
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new ZTreeView();
		}

		ZTreeView TreeView
		{
			get { return (ZTreeView)Control; }
		}

		#endregion

		public void TestBindToWithTreeNodeCollectionOnlyIsBindable()
		{
			TreeView.BindTo = "";
			Page.Controls.Add(TreeView);
			Assert(TreeView.IsBindable(TestTreeNodeCollection));
		}

		public void TestBindToFailsWithNoBindToSet()
		{
			TreeView.BindTo = "";
			Page.Controls.Add(TreeView);
			AssertEquals(false, TreeView.IsBindable(TestTreeNode));
		}

		public void TestTreeViewNodeCollectionNotNull()
		{
			AssertNotNull(TreeView.Nodes);
		}
	}
}
