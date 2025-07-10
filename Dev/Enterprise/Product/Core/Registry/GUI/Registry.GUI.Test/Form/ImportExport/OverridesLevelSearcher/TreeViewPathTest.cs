using System.Linq;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class TreeViewPathTest : TestCase
	{
		public void TestProperties()
		{
			var parent = new TreeViewPath("parent");
			var child1 = new TreeViewPath("child1", parent);
			var child2 = new TreeViewPath("child2", parent);

			AssertEquals("parent", parent.Content);
			AssertEquals("child1", child1.Content);
			AssertEquals("child2", child2.Content);

			AssertEquals(parent, child1.Parent);
			AssertEquals(parent, child2.Parent);

			AssertEquals(-1, parent.IndexOfParent);
			AssertEquals(0, child1.IndexOfParent);
			AssertEquals(1, child2.IndexOfParent);

			AssertEquals(2, parent.Children.Count());

			TreeViewPath[] expectedChildren = { child1, child2 };
			AssertContainsExactElementsInExactOrder(expectedChildren, parent.Children);
		}

		public void TestGetFullPaths()
		{
			var emptyPath = new TreeViewPath("");
			AssertEquals(0, emptyPath.GetFullPaths().Count());

			var path1 = new TreeViewPath("1");
			TreeViewPath[] expectedPaths1 = { path1 };
			AssertContainsExactElementsInExactOrder(expectedPaths1, path1.GetFullPaths());

			var path11 = new TreeViewPath("1-1", path1);
			TreeViewPath[] expectedPaths11 = { path1, path11 };
			AssertContainsExactElementsInExactOrder(expectedPaths11, path11.GetFullPaths());

			var path111 = new TreeViewPath("1-1-1", path11);

			TreeViewPath[] expectedPaths111 = { path1, path11, path111 };
			AssertContainsExactElementsInExactOrder(expectedPaths111, path111.GetFullPaths());
		}
	}
}
