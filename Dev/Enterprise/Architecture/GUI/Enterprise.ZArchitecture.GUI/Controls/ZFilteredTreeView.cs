using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZFilteredTreeView : ZUserControl
	{
		public ZFilteredTreeView()
		{
			InitializeComponent();
			MissingResourceStringChecker.ExcludeFromTest(FilterTextBox);
			nodeCache = new List<TreeNode>();
			FilterTextBox.AcceptsReturn = true;
			FilterTextBox.Hotkeys.RegisterHotKey(Keys.Enter, FilterNodes);
			FilterTextBox.PlaceHolderText = Res.GetString("f8932989-d0b2-475c-99ef-241065b5f516", "Type here and press Enter to filter the tree list.");
		}

		void FilterNodes()
		{
			if (string.IsNullOrEmpty(lastFilterText))
			{
				UpdateNodeCache();
			}
			RebuildDisplayTree();
			lastFilterText = FilterTextBox.Text;
		}

		readonly List<TreeNode> nodeCache;
		string lastFilterText = string.Empty;

		void UpdateNodeCache()
		{
			nodeCache.Clear();
			foreach (TreeNode node in DisplayTree.Nodes)
			{
				var newNode = GetNewNode(node);
				nodeCache.Add(newNode);
			}
		}

		void RebuildDisplayTree()
		{
			DisplayTree.Nodes.Clear();
			foreach (var node in nodeCache)
			{
				var newNode = GetNewNode(node);
				if (!string.IsNullOrEmpty(FilterTextBox.Text))
				{
					FilterNodes(newNode);
					if (newNode.Nodes.Count > 0 || newNode.Text.IndexOf(FilterTextBox.Text, StringComparison.OrdinalIgnoreCase) != -1)
					{
						DisplayTree.Nodes.Add(newNode);
					}
				}
				else
				{
					DisplayTree.Nodes.Add(newNode);
				}
			}
		}

		TreeNode GetNewNode(TreeNode sourceNode)
		{
			var newNode = (TreeNode)sourceNode.Clone();
			if (sourceNode.IsExpanded)
			{
				newNode.Expand();
			}
			return newNode;
		}

		void FilterNodes(TreeNode parentNode)
		{
			if (parentNode.Nodes.Count > 0)
			{
				for (var i = parentNode.Nodes.Count - 1; i >= 0; i--)
				{
					var currentNode = parentNode.Nodes[i];
					if (currentNode.Text.IndexOf(FilterTextBox.Text, StringComparison.OrdinalIgnoreCase) == -1)
					{
						FilterNodes(currentNode);
						if (currentNode.Nodes.Count == 0)
						{
							parentNode.Nodes.RemoveAt(i);
						}
					}
					else if (currentNode.Nodes.Count > 0)
					{
						FilterNodes(currentNode);
					}
				}
			}
		}
	}
}
