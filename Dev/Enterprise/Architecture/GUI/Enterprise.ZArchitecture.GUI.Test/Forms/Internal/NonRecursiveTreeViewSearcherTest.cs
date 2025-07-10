using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Forms.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.Internal
{
	public class TreeViewSearcherTest : TestCase
	{
		public void TestPreviousResult()
		{
			using (var treeView = TreeViewCreatorForTests.CreateTree(@"
							A			                   B
						A0			                B0            B1
				"))
			{
				var testSearcher = new TreeViewSearcherForTest();

				var results = testSearcher.Search(treeView, "B", StringComparison.CurrentCulture);

				Assert(String.Format("PRE: Incorrect number of results, expected 3, found {0}", results.Total), results.Total == 3);

				results.Previous();

				Assert("Index not decremented as expected", ((NonRecursiveSearchResults)results).Index == 2);
			}
		}

		public void TestNextResult()
		{
			using (var treeView = TreeViewCreatorForTests.CreateTree(@"
							A			                   B
						A0			                B0            B1
				"))
			{
				var testSearcher = new TreeViewSearcherForTest();

				var results = testSearcher.Search(treeView, "B", StringComparison.CurrentCulture);

				Assert(String.Format("PRE: Incorrect number of results, expected 3, found {0}", results.Total), results.Total == 3);

				var currNode = results.Next();

				Assert(currNode == treeView.Nodes[1]);
			}
		}

		public void TestSearch()
		{
			using (var treeView = TreeViewCreatorForTests.CreateTree(@"
							A			                   B
						A0      A1	                B0            B1
					A00	   A10	 A11             B00         B10      B11
				A000   A001       A110        B000        B100  B101    B110
						A0010             B0000  B0001
				"))
			{
				var treeViewSearcher = new TreeViewSearcherForTest();

				var results = treeViewSearcher.Search(treeView, "A1", StringComparison.CurrentCulture);

				Assert("No matches found when should have found [A1, A10, A11, A110]", results.Total > 0);

				TreeNode currNode;
				var nodeNameSet = new HashSet<string>();

				do
				{
					currNode = results.Next();
				}
				while (nodeNameSet.Add(currNode.Text));

				AssertArrayEqualsByElements(String.Format("Incorrect results set. Results were: {0} when expected [A1, A10, A11, A110]", nodeNameSet.ToArray()), new string[] { "A1", "A10", "A11", "A110" }, nodeNameSet.ToArray());
			}
		}

		[DeveloperOnlyTest]
		public void TestCreateTree()
		{
			using (var treeView = TreeViewCreatorForTests.CreateTree(@"
							A			                   B
						A0			                B0            B1
					A00    A01					B00             B10   B11
                 A000   A010  A011			B000  B001        B100  B110
								A0110          B0010 B0011  B1000 B1001
				"))
			{
				ShowOnForm(treeView);

				var expectedNodes = new string[] { "A", "A0", "B", "B0", "B1" };
				var treeNodeNames = new List<string>();

				foreach (TreeNode node in treeView.Nodes)
				{
					treeNodeNames.Add(node.Text);
					foreach (TreeNode childNode in node.Nodes)
					{
						treeNodeNames.Add(childNode.Text);
					}
				}

				AssertArrayEqualsByElements(String.Format("Incorrect tree has been made. Expected (flattened) {0} but found (flattened) {1}", expectedNodes, treeNodeNames), expectedNodes, treeNodeNames.ToArray());
			}
		}

		void ShowOnForm(Control c)
		{
			var form = new ZChildForm();
			c.Dock = DockStyle.Fill;
			form.Controls.Add(c);

			form.Show();

			while (form.Visible)
			{
				Application.DoEvents();
			}
		}

		internal class TreeViewSearcherForTest : NonRecursiveTreeViewSearcher
		{
			public TreeViewSearcherForTest() : base()
			{
			}
		}
	}
}
