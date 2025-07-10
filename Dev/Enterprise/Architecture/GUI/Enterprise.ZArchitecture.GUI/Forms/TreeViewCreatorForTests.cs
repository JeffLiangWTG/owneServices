using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Forms.Internal
{
	public sealed class TreeViewCreatorForTests
	{
		TreeViewCreatorForTests()
		{
		}

		//Max 36 children for any node (node text length = depth so cannot overflow to double digits, 10 digits + 26 characters (could use things like punctuation if necessary)), all children must have parent's name plus ONE character (any character) added on
		public static ZTreeView CreateTree(string treeString)
		{
			var treeView = new ZTreeView();

			var linesWhole = treeString.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

			var lines = new List<List<string>>();
			linesWhole.ForEach(line => lines.Add(Regex.Split(line, @"\s+").Where(s => !String.IsNullOrEmpty(s)).ToList()));

			//Making a bank of nodes to assign from (much easier to track parents this way when building tree)
			var nodes = new List<TreeNode>();
			lines.ForEach(line => line.ForEach(nodeText => nodes.Add(new TreeNode(nodeText))));

			for (var i = 0; i < nodes.Count; i++)
			{
				var depth = nodes[i].Text.Length;

				if (depth == 1)
				{
					treeView.Nodes.Add(nodes[i]);
				}
				else
				{
					var parentNode = nodes.Find(node => node.Text == nodes[i].Text.Substring(0, nodes[i].Text.Length - 1));
					parentNode.Nodes.Add(nodes[i]);
				}
			}

			return treeView;
		}
	}
}
