using System.Collections;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class RegistryTreeNodeComparerTest : TestCase
	{
		public void TestCompare()
		{
			IComparer comparer = new RegistryTreeNodeComparer();

			TreeNode node1 = new TreeNode("b");
			TreeNode node2 = new TreeNode("c");
			TreeNode node1Child = new TreeNode("d");
			TreeNode node2Child = new TreeNode("a");
			node1.Nodes.Add(node1Child);
			node2.Nodes.Add(node2Child);

			AssertEquals("comparer.Compare(node1, node1)", 0, comparer.Compare(node1, node1));
			AssertEquals("comparer.Compare(node2, node2)", 0, comparer.Compare(node2, node2));
			AssertEquals("comparer.Compare(node1Child, node1Child)", 0, comparer.Compare(node1Child, node1Child));
			AssertEquals("comparer.Compare(node2Child, node2Child)", 0, comparer.Compare(node2Child, node2Child));

			AssertEquals("comparer.Compare(node1, node2)", -1, comparer.Compare(node1, node2));
			AssertEquals("comparer.Compare(node2, node1)", 1, comparer.Compare(node2, node1));

			AssertEquals("comparer.Compare(node1, node1Child)", 1, comparer.Compare(node1, node1Child));
			AssertEquals("comparer.Compare(node1Child, node1)", -1, comparer.Compare(node1Child, node1));

			AssertEquals("comparer.Compare(node1, node2Child)", 1, comparer.Compare(node1, node2Child));
			AssertEquals("comparer.Compare(node2Child, node1)", -1, comparer.Compare(node2Child, node1));

			AssertEquals("comparer.Compare(node2, node1Child)", 1, comparer.Compare(node2, node1Child));
			AssertEquals("comparer.Compare(node1Child, node2)", -1, comparer.Compare(node1Child, node2));

			AssertEquals("comparer.Compare(node2, node2Child)", 1, comparer.Compare(node2, node2Child));
			AssertEquals("comparer.Compare(node2Child, node2)", -1, comparer.Compare(node2Child, node2));
		}
	}
}
