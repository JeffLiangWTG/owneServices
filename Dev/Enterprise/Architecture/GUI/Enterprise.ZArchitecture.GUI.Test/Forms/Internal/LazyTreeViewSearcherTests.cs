using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Forms.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.Internal
{
	public class LazyTreeViewSearcherTests : TestCase
	{
		public void TestSearchesInDepthFirstOrder()
		{
			// Arrange
			using (var treeView = CreateTreeViewForTest())
			{
				var searcher = new LazyTreeViewSearcher();
				treeView.SelectedNode = FindByText(treeView.Nodes, "Node 2 - 2");

				// Act
				var searchResult = searcher.Search(treeView, criteria: "Node", StringComparison.Ordinal);

				// Assert
				AssertEquals(-1, searchResult.Total);
				AssertEquals("Node 2 - 2", searchResult.Next()?.Text);
				AssertEquals("Node 2 - 2 - 2", searchResult.Next()?.Text);
				AssertEquals("Node 3", searchResult.Next()?.Text);
				AssertEquals("Node 3 - 1", searchResult.Next()?.Text);
				AssertEquals("Node 1", searchResult.Next()?.Text);
				AssertEquals("Node 1 - 1", searchResult.Next()?.Text);
				AssertEquals("Node 1 - 1 - 1", searchResult.Next()?.Text);
				AssertEquals("Node 1 - 2", searchResult.Next()?.Text);
				AssertEquals("Node 2", searchResult.Next()?.Text);
				AssertEquals("Node 2 - 1", searchResult.Next()?.Text);
				AssertNull(searchResult.Next());
			}
		}

		public void TestSearchWithNoSelectedNode()
		{
			// Arrange
			using (var treeView = CreateTreeViewForTest())
			{
				var searcher = new LazyTreeViewSearcher();

				// Act
				var searchResult = searcher.Search(treeView, criteria: "Node", StringComparison.Ordinal);

				// Assert
				AssertEquals(-1, searchResult.Total);
				AssertEquals("Node 1", searchResult.Next()?.Text);
				AssertEquals("Node 1 - 1", searchResult.Next()?.Text);
				AssertEquals("Node 1 - 1 - 1", searchResult.Next()?.Text);
				AssertEquals("Node 1 - 2", searchResult.Next()?.Text);
				AssertEquals("Node 2", searchResult.Next()?.Text);
				AssertEquals("Node 2 - 1", searchResult.Next()?.Text);
				AssertEquals("Node 2 - 2", searchResult.Next()?.Text);
				AssertEquals("Node 2 - 2 - 2", searchResult.Next()?.Text);
				AssertEquals("Node 3", searchResult.Next()?.Text);
				AssertEquals("Node 3 - 1", searchResult.Next()?.Text);
				AssertNull(searchResult.Next());
			}
		}

		public void TestBackwardSearch()
		{
			// Arrange
			using (var treeView = CreateTreeViewForTest())
			{
				var searcher = new LazyTreeViewSearcher();
				treeView.SelectedNode = FindByText(treeView.Nodes, "Node 2 - 2");

				// Act
				var searchResult = searcher.Search(treeView, criteria: "Node", StringComparison.Ordinal);

				// Assert
				AssertEquals(-1, searchResult.Total);
				AssertEquals("Node 2 - 2", searchResult.Previous()?.Text);
				AssertEquals("Node 2 - 1", searchResult.Previous()?.Text);
				AssertEquals("Node 2", searchResult.Previous()?.Text);
				AssertEquals("Node 1 - 2", searchResult.Previous()?.Text);
				AssertEquals("Node 1 - 1 - 1", searchResult.Previous()?.Text);
				AssertEquals("Node 1 - 1", searchResult.Previous()?.Text);
				AssertEquals("Node 1", searchResult.Previous()?.Text);
				AssertEquals("Node 3 - 1", searchResult.Previous()?.Text);
				AssertEquals("Node 3", searchResult.Previous()?.Text);
				AssertEquals("Node 2 - 2 - 2", searchResult.Previous()?.Text);
				AssertNull(searchResult.Previous());
			}
		}

		public void TestEmptyTree()
		{
			// Arrange
			using (var emptyTree = new TreeView())
			{
				var searcher = new LazyTreeViewSearcher();

				// Act
				var searchResult = searcher.Search(emptyTree, criteria: "Node", StringComparison.Ordinal);

				// Assert
				AssertNull(searchResult.Next());
				AssertNull(searchResult.Next());
				AssertNull(searchResult.Previous());
				AssertNull(searchResult.Previous());
			}
		}

		public void TestSingleNodeTree()
		{
			// Arrange
			using (var tree = new TreeView())
			{
				tree.Nodes.Add("Node 1");
				var searcher = new LazyTreeViewSearcher();

				// Act
				var searchResult = searcher.Search(tree, "Node", StringComparison.Ordinal);

				// Assert
				AssertEquals("Node 1", searchResult.Next()?.Text);
				AssertNull(searchResult.Next());
				AssertEquals("Node 1", searchResult.Next()?.Text);
				AssertNull(searchResult.Next());
				AssertEquals("Node 1", searchResult.Previous()?.Text);
				AssertNull(searchResult.Previous());
				AssertEquals("Node 1", searchResult.Previous()?.Text);
				AssertNull(searchResult.Previous());
			}
		}

		public void TestTreeViewWithExpandingNodes()
		{
			// Arrange
			using (var treeView = new ExpandableTreeForSearch())
			{
				var searcher = new LazyTreeViewSearcher();

				// Act
				var searchResult = searcher.Search(treeView, criteria: "Node", StringComparison.Ordinal);

				// Assert
				AssertEquals(-1, searchResult.Total);
				AssertEquals("Node 1", searchResult.Next()?.Text);
				AssertEquals("Node 1 - 1", searchResult.Next()?.Text);
				AssertEquals("Node 1 - 1 - 1", searchResult.Next()?.Text);
				AssertEquals("Node 1 - 2", searchResult.Next()?.Text);
				AssertEquals("Node 2", searchResult.Next()?.Text);
				AssertEquals("Node 2 - 1", searchResult.Next()?.Text);
				AssertNull(searchResult.Next());
			}
		}

		public void TestTreeViewWithExpandingNodesBackwards()
		{
			// Arrange
			using (var treeView = new ExpandableTreeForSearch())
			{
				var searcher = new LazyTreeViewSearcher();

				// Act
				var searchResult = searcher.Search(treeView, criteria: "Node", StringComparison.Ordinal);

				// Assert
				AssertEquals(-1, searchResult.Total);
				AssertEquals("Node 1", searchResult.Previous()?.Text);
				AssertEquals("Node 2 - 1", searchResult.Previous()?.Text);
				AssertEquals("Node 2", searchResult.Previous()?.Text);
				AssertEquals("Node 1 - 2", searchResult.Previous()?.Text);
				AssertEquals("Node 1 - 1 - 1", searchResult.Previous()?.Text);
				AssertEquals("Node 1 - 1", searchResult.Previous()?.Text);
				AssertNull(searchResult.Previous());
			}
		}

		public void TestCustomPredicate()
		{
			// Arrange
			using (var treeView = new TreeView())
			{
				var node1 = treeView.Nodes.Add("key1", "Node 1");
				node1.Nodes.Add("key11", "Node 1 - 1");
				var node2 = treeView.Nodes.Add("different2", "Node 2");
				node2.Nodes.Add("key12", "Node 2 - 1");

				var searcher = new LazyTreeViewSearcher((node, str, comparison) => node.Name.IndexOf(new string(str.Reverse().ToArray()), comparison) >= 0);

				// Act
				var searchResult = searcher.Search(treeView, criteria: "yek", StringComparison.Ordinal);

				// Assert
				AssertEquals(-1, searchResult.Total);
				AssertEquals("Node 1", searchResult.Next()?.Text);
				AssertEquals("Node 1 - 1", searchResult.Next()?.Text);
				AssertEquals("Node 2 - 1", searchResult.Next()?.Text);
				AssertNull(searchResult.Next());
			}
		}

		TreeView CreateTreeViewForTest()
		{
			var treeView = new TreeView();
			var node1 = treeView.Nodes.Add("Node 1");
			var node2 = treeView.Nodes.Add("Node 2");
			var node3 = treeView.Nodes.Add("Node 3");

			var node11 = node1.Nodes.Add("Node 1 - 1");
			node11.Nodes.Add("Node 1 - 1 - 1");
			node1.Nodes.Add("Node 1 - 2");

			node2.Nodes.Add("Node 2 - 1");
			var node22 = node2.Nodes.Add("Node 2 - 2");
			node22.Nodes.Add("Node 2 - 2 - 2");
			node22.Nodes.Add("Non-findable-1");

			node3.Nodes.Add("Node 3 - 1");
			node3.Nodes.Add("Non-findable-2");

			return treeView;
		}

		TreeNode FindByText(TreeNodeCollection collection, string text)
		{
			return FindRecursive(collection) ?? throw new InvalidOperationException($"Node not found by text: '{text}'");

			TreeNode FindRecursive(TreeNodeCollection nodes)
			{
				foreach (TreeNode node in nodes)
				{
					if (node.Text == text)
					{
						return node;
					}

					var found = FindRecursive(node.Nodes);
					if (found != null)
					{
						return found;
					}
				}

				return null;
			}
		}

		class ExpandableTreeForSearch : TreeView, ITreeViewWithExpandingNodes
		{
			public ExpandableTreeForSearch()
			{
				Nodes.Add("Node 1");
				Nodes.Add("Node 2");
			}

			public void ExpandNodeIfNeeded(TreeNode node)
			{
				if (node.Nodes.Count > 0)
				{
					return;
				}

				switch (node.Text)
				{
					case "Node 1":
						node.Nodes.Add("Node 1 - 1");
						node.Nodes.Add("Node 1 - 2");
						break;
					case "Node 2":
						node.Nodes.Add("Node 2 - 1");
						break;
					case "Node 1 - 1":
						node.Nodes.Add("Node 1 - 1 - 1");
						break;
				}
			}
		}
	}
}
