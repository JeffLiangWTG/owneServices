using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZFilteredTreeViewTest : TestCase
	{
		public void TestFilterNode()
		{
			using (var filterTreeView = new ZFilteredTreeView())
			{
				var displayTree = filterTreeView.DisplayTree;
				var node = displayTree.Nodes.Add("Sydney");
				node.Nodes.Add("Mascot");
				node.Nodes.Add("Central");
				node = displayTree.Nodes.Add("Nanjing");
				node.Nodes.Add("Xinjiekou");
				node.Nodes.Add("Tiananpark");

				AssertEquals("Tree node count before filtering", 6, displayTree.GetNodeCount(true));
				AssertEquals("Tree node text before filtering", "Sydney|Mascot|Central|Nanjing|Xinjiekou|Tiananpark", GetFormattedTreeNodesText(displayTree));

				filterTreeView.FilterTextBox.Text = "Mascot";
				KeySender.SendKeyDownToProcessCmdKey(filterTreeView.FilterTextBox, Keys.Enter);
				AssertEquals("Tree nodes count after filtering", 2, displayTree.GetNodeCount(true));
				AssertEquals("Tree node text after filtering", "Sydney|Mascot", GetFormattedTreeNodesText(displayTree));

				filterTreeView.FilterTextBox.Text = string.Empty;
				KeySender.SendKeyDownToProcessCmdKey(filterTreeView.FilterTextBox, Keys.Enter);
				AssertEquals("Tree nodes should be recovered", 6, displayTree.GetNodeCount(true));
				AssertEquals("Tree node text should be recovered", "Sydney|Mascot|Central|Nanjing|Xinjiekou|Tiananpark", GetFormattedTreeNodesText(displayTree));
			}
		}

		string GetFormattedTreeNodesText(TreeView view)
		{
			var result = new List<string>();
			RetriveAllTreeNodesText(view.Nodes, result);
			return string.Join("|", result.ToArray());
		}

		void RetriveAllTreeNodesText(TreeNodeCollection nodes, List<string> result)
		{
			foreach (TreeNode node in nodes)
			{
				result.Add(node.Text);
				RetriveAllTreeNodesText(node.Nodes, result);
			}
		}
	}
}
