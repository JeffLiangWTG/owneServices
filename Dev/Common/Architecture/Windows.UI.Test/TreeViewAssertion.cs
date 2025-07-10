using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace NUnit.Framework
{
	public static class TreeViewAssertion
	{
		public static void AssertHasPath(TreeNode node, string nodePath, char separator = '/')
		{
			AssertHasPath(node.Name, node.Nodes, nodePath, separator);
		}

		public static void AssertHasPath(string rootName, TreeNodeCollection nodes, string nodePath, char separator = '/')
		{
			var splitIndex = nodePath.IndexOf(separator);
			var firstChildName = splitIndex > 0 ? nodePath.Substring(0, splitIndex) : nodePath;

			if (!nodes.ContainsKey(firstChildName))
			{
				rootName = string.IsNullOrEmpty(rootName) ? "<no_name>" : rootName;
				var childNodeNames = nodes.Cast<TreeNode>().Select(child => child.Name);
				var childDescription = nodes.Count > 0 ?
					string.Format(CultureInfo.CurrentCulture, "contained [ \"{0}\" ]", string.Join("\", \"", childNodeNames.ToArray())) :
					"was empty";

				Assertion.Fail(string.Format(CultureInfo.CurrentCulture, "Child with name \"{0}\" was not found in \"{1}\". Instead Nodes {2}", firstChildName, rootName, childDescription));
			}

			if (splitIndex > 0)
			{
				AssertHasPath(nodes[firstChildName], nodePath.Substring(splitIndex + 1));
			}

			Assertion.Assert(true);
		}

		public static void AssertDoesNotHavePath(TreeNodeCollection nodes, string nodePath, char separator = '/')
		{
			var itemExists = true;
			var parent = nodes;
			foreach (var childName in nodePath.Split(separator))
			{
				var next = parent[childName];
				if (next == null)
				{
					itemExists = false;
					break;
				}
				parent = next.Nodes;
			}

			Assertion.Assert("Path should not be found in the TreeNodeCollection, but was: '" + nodePath + "'", !itemExists);
		}
	}
}
