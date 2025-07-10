using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business
{
	sealed class TreeNodeTest : TestCase
	{
		const string ShortDescription = "TestElement";
		const string LongDescription = "Element to test the tree node tag";
		const string Quantity = "piece";
		TestTreeNodeElement TestElement;

		protected override void SetUp()
		{
			base.SetUp();
			TestElement = new TestTreeNodeElement(ShortDescription, LongDescription, Quantity);
		}

		public void TestEmptyConstructor()
		{
			TreeNode testNode = new TreeNode();
			AssertNotNull(testNode);
			AssertNull(testNode.Parent);
			AssertNull(testNode.Text);
			AssertNull(testNode.Tag);
		}

		public void TestStringConstructor()
		{
			string nodeName = "New Node";
			TreeNode testNode = new TreeNode(nodeName);
			AssertEquals(nodeName, testNode.Text);
		}

		public void TestTreeElement()
		{
			TreeNode testNode = new TreeNode(ShortDescription);
			testNode.Tag = TestElement;
			AssertEquals(TestElement, testNode.Tag);
			AssertEquals(TestElement.ShortDescription, testNode.Text);
		}

		public void TestTreeElementDescriptionDefault()
		{
			TreeNode testNode = new TreeNode(TestElement);
			AssertEquals(TestElement.ShortDescription, testNode.Text);
		}

		public void TestTreeElementDescriptionOverride()
		{
			string descriptionOverride = "Overriden Description";
			TreeNode testNode = new TreeNode(descriptionOverride);
			testNode.Tag = TestElement;
			AssertEquals(descriptionOverride, testNode.Text);
			AssertEquals(TestElement.ShortDescription, testNode.Tag.ShortDescription);
		}

		void TraverseLateLoading(TreeNode node)
		{
			if (node.ChildNodesLoaded)
			{
				foreach (TreeNode childNode in node.Nodes)
				{
					TraverseLateLoading(childNode);
				}
			}
		}

		int GetDescendantCount(TreeNode node)
		{
			int descendants = 0;
			foreach (TreeNode childNode in node.Nodes)
			{
				descendants += GetDescendantCount(childNode);
				descendants++;
			}
			return descendants;
		}
	}
}
