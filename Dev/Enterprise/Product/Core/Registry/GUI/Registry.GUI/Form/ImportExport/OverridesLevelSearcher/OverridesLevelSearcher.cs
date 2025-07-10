using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.GUI.Forms.Internal;

#pragma warning disable IDE0005 // Using directive is unnecessary.
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005 // Using directive is unnecessary.

namespace Enterprise.Registry.GUI
{
	class OverridesLevelSearcher : NonRecursiveTreeViewSearcher
	{
		readonly List<IOverrideLevel> overrideLevels;

		public OverridesLevelSearcher(List<IOverrideLevel> overrideLevels)
		{
			this.overrideLevels = overrideLevels;
		}

		protected override ISearchResults SearchCore(TreeView treeView, string criteria, StringComparison comparison)
		{
			var matches = new List<TreeViewPath>();
			if (treeViewPathsFlattened == null)
			{
				FlattenTreeDepthFirst();
			}

			var selectedIndex = treeView.SelectedNode != null ? GetSelectedIndex(treeView.SelectedNode) : -1;
			var index = 0;
			var offset = 0;
			foreach (var path in treeViewPathsFlattened)
			{
				if (path.Content.Contains(criteria, comparison))
				{
					matches.Add(path);

					if (index >= selectedIndex)
					{
						offset = Cont ? matches.Count - 1 : 0;
						selectedIndex = int.MaxValue;
					}
				}

				index++;
			}

			return new OverridesLevelSearchResults(treeView, matches, offset);
		}

		void FlattenTreeDepthFirst()
		{
			treeViewPathsFlattened = new List<TreeViewPath>();
			var rootPath = new TreeViewPath(string.Empty);

			foreach (var rootLevel in overrideLevels)
			{
				AddChildNodesToFlattenedTreeViewPaths(rootLevel, rootPath);
			}
		}

		void AddChildNodesToFlattenedTreeViewPaths(IOverrideLevel level, TreeViewPath parent = null)
		{
			if (!string.IsNullOrEmpty(level.PathRelativeToParent))
			{
				(var categorySplit, _) = RegistryItemTreeViewBuilder.ConvertCategory(level.PathRelativeToParent, level.PathRelativeToParent);

				foreach (var categoryitem in categorySplit)
				{
					var category = parent?.Children.FirstOrDefault(p => p.Content == categoryitem);
					if (category == null)
					{
						category = new TreeViewPath(categoryitem, parent);
						treeViewPathsFlattened.Add(category);
					}

					parent = category;
				}
			}

			var path = new TreeViewPath(level.Description, parent);
			treeViewPathsFlattened.Add(path);

			foreach (var subLevel in level.Children)
			{
				AddChildNodesToFlattenedTreeViewPaths(subLevel, path);
			}
		}

		int GetSelectedIndex(TreeNode selectedNode)
		{
			for (var i = 0; i < treeViewPathsFlattened.Count; i++)
			{
				var path = treeViewPathsFlattened[i];
				var node = selectedNode;
				var equals = true;
				while (!string.IsNullOrEmpty(node?.Text) && !string.IsNullOrEmpty(path?.Content))
				{
					equals = node.Text == path.Content && node.Index == path.IndexOfParent;
					if (!equals)
					{
						break;
					}

					node = node.Parent;
					path = path.Parent;
				}

				if (equals)
				{
					return i;
				}
			}
			return -1;
		}

		List<TreeViewPath> treeViewPathsFlattened;
	}
}
