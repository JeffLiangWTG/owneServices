using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Forms.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(TreeViewFindForm))]
	public class TreeFindFormTest : ZFormBasherTest
	{
		public void TestDefaultSearcherAssigned()
		{
			using (var treeView = TreeViewCreatorForTests.CreateTree(@"
																A			B
														     A0          B0	  B1
															  "))
			using (var testForm = new TreeViewFindFormForTest(treeView))
			{
				AssertNotNull("treeViewSearcher", testForm.GetTreeViewSearcher());
			}
		}

		public void TestFindNextNoMatchCase()
		{
			using (var testForm = new TreeViewFindFormForTest(testTreeView))
			{
				var findTextBox = testForm.GetFindTextBox();
				findTextBox.Text = "A";

				testForm.StartFindForTest(true);
				AssertEquals("Selected Node", "A", testTreeView.SelectedNode.Text);

				testForm.StartFindForTest(true);
				AssertEquals("Selected Node", "A0", testTreeView.SelectedNode.Text);

				testForm.StartFindForTest(true);
				AssertEquals("Selected Node", "A", testTreeView.SelectedNode.Text);
			}
		}

		public void TestFindNextWithMatchCase()
		{
			using (var testForm = new TreeViewFindFormForTest(testTreeView))
			{
				var findTextBox = testForm.GetFindTextBox();
				var matchCaseCheckBox = testForm.GetMatchCaseCheckBox();
				findTextBox.Text = "a";
				matchCaseCheckBox.Checked = true;

				testForm.StartFindForTest(true);
				Assert("Nothing should be selected", testTreeView.SelectedNode == null);

				findTextBox.Text = "A";
				testForm.StartFindForTest(true);
				AssertEquals("Selected Node", "A", testTreeView.SelectedNode.Text);

				testForm.StartFindForTest(true);
				AssertEquals("Selected Node", "A0", testTreeView.SelectedNode.Text);

				testForm.StartFindForTest(true);
				AssertEquals("Selected Node", "A", testTreeView.SelectedNode.Text);
			}
		}

		public void TestFindPreviousNoMatchCase()
		{
			using (var testForm = new TreeViewFindFormForTest(testTreeView))
			{
				var findTextBox = testForm.GetFindTextBox();
				findTextBox.Text = "b";

				testForm.StartFindForTest(false);
				AssertEquals("Selected Node", "B1", testTreeView.SelectedNode.Text);

				testForm.StartFindForTest(false);
				AssertEquals("Selected Node", "B0", testTreeView.SelectedNode.Text);

				testForm.StartFindForTest(false);
				AssertEquals("Selected Node", "B", testTreeView.SelectedNode.Text);

				testForm.StartFindForTest(false);
				AssertEquals("Selected Node", "B1", testTreeView.SelectedNode.Text);
			}
		}

		public void TestFindPreviousWithMatchCase()
		{
			using (var testForm = new TreeViewFindFormForTest(testTreeView))
			{
				var findTextBox = testForm.GetFindTextBox();
				var matchCaseCheckBox = testForm.GetMatchCaseCheckBox();
				findTextBox.Text = "b";
				matchCaseCheckBox.Checked = true;

				testForm.StartFindForTest(false);
				Assert("Nothing should be selected", testTreeView.SelectedNode == null);

				findTextBox.Text = "B";
				testForm.StartFindForTest(false);
				AssertEquals("Selected Node", "B1", testTreeView.SelectedNode.Text);

				testForm.StartFindForTest(false);
				AssertEquals("Selected Node", "B0", testTreeView.SelectedNode.Text);

				testForm.StartFindForTest(false);
				AssertEquals("Selected Node", "B", testTreeView.SelectedNode.Text);

				testForm.StartFindForTest(false);
				AssertEquals("Selected Node", "B1", testTreeView.SelectedNode.Text);
			}
		}

		public void TestFindNextWithSearchFromSelectedTrue()
		{
			using (var testForm = new TreeViewFindFormForTest(testTreeView))
			{
				var findTextBox = testForm.GetFindTextBox();
				var searchFromSelectedCheckBox = testForm.GetContinueCheckBox();

				findTextBox.Text = "b";
				searchFromSelectedCheckBox.Checked = true;

				testTreeView.SelectedNode = testTreeView.Nodes[1].Nodes[0]; //A A0 B [B0] B1
				testTreeView.SelectedNode.EnsureVisible();

				testForm.StartFindForTest(true);
				AssertEquals("Incorrect first result", "B0", testTreeView.SelectedNode.Text);

				testForm.StartFindForTest(true);
				AssertEquals("Incorrect second result", "B1", testTreeView.SelectedNode.Text);

				testForm.StartFindForTest(true);
				AssertEquals("Incorrect third result", "B", testTreeView.SelectedNode.Text);
			}
		}

		public void TestFindPreviousWithSearchFromSelectedTrue()
		{
			using (var testForm = new TreeViewFindFormForTest(testTreeView))
			{
				var findTextBox = testForm.GetFindTextBox();
				var searchFromSelectedCheckBox = testForm.GetContinueCheckBox();

				findTextBox.Text = "b";
				searchFromSelectedCheckBox.Checked = true;

				testTreeView.SelectedNode = testTreeView.Nodes[1].Nodes[0]; //A A0 B [B0] B1
				testTreeView.SelectedNode.EnsureVisible();

				testForm.StartFindForTest(false);
				AssertEquals("Incorrect first result", "B", testTreeView.SelectedNode.Text);

				testForm.StartFindForTest(false);
				AssertEquals("Incorrect second result", "B1", testTreeView.SelectedNode.Text);

				testForm.StartFindForTest(false);
				AssertEquals("Incorrect third result", "B0", testTreeView.SelectedNode.Text);
			}
		}

		public void TestMatchesLabelDisplayingTipsCorrectly()
		{
			using (var testForm = new TreeViewFindFormForTest(testTreeView))
			{
				testForm.Show();

				var findTextBox = testForm.GetFindTextBox();
				var findPreviousButton = testForm.GetFindPreviousButton();
				var findNextButton = testForm.GetFindNextButton();
				var matchesLabel = testForm.GetMatchesLabel();

				Assert("MatchesLabel should say \"No Results\" initially", matchesLabel.Text == "No Results");

				findTextBox.Text = "b";

				findNextButton.PerformClick();
				Assert("MatchesLabel should say \"Match 1 of 3\"", matchesLabel.Text == "Match 1 of 3");

				findNextButton.PerformClick();
				Assert("MatchesLabel should say \"Match 2 of 3\"", matchesLabel.Text == "Match 2 of 3");

				findNextButton.PerformClick();
				Assert("MatchesLabel should say \"Match 3 of 3\"", matchesLabel.Text == "Match 3 of 3");

				findNextButton.PerformClick();
				Assert("MatchesLabel should say \"Match 1 of 3\"", matchesLabel.Text == "Match 1 of 3");

				findTextBox.Text = "c";

				findNextButton.PerformClick();
				Assert("MatchesLabel should say \"No Results\"", matchesLabel.Text == "No Results");
			}
		}

		public void TestUpdateButtons()
		{
			using (var testForm = new TreeViewFindFormForTest(testTreeView))
			{
				testForm.Show();

				var findTextBox = testForm.GetFindTextBox();
				var findPreviousButton = testForm.GetFindPreviousButton();
				var findNextButton = testForm.GetFindNextButton();

				AssertEquals("FindTextBox.Text", "", findTextBox.Text);
				Assert("FindPreviousButton should be disabled", !findPreviousButton.Enabled);
				Assert("FindNextButton should be disabled", !findNextButton.Enabled);

				findTextBox.Text = "B";
				Assert("FindPreviousButton should be enabled", findPreviousButton.Enabled);
				Assert("FindNextButton should be enabled", findNextButton.Enabled);

				findNextButton.PerformClick();
				AssertEquals("B should be selected", "B", testTreeView.SelectedNode.Text);

				findPreviousButton.PerformClick();
				AssertEquals("B0 should be selected", "B1", testTreeView.SelectedNode.Text);

				findTextBox.Text = "";
				Assert("FindPreviousButton should be disabled", !findPreviousButton.Enabled);
				Assert("FindNextButton should be disabled", !findNextButton.Enabled);
			}
		}

		internal class RepeatedTreeView : ZTreeView
		{
			public RepeatedTreeView() : base()
			{
				Nodes.Add("same node name");
				Nodes.Add("same node name");

				Nodes[0].Nodes.Add("A");
				Nodes[0].Nodes.Add("same node name");
				Nodes[0].Nodes[0].Nodes.Add("same node name");
				Nodes[0].Nodes[0].Nodes[0].Nodes.Add("A");
				Nodes[0].Nodes[0].Nodes[0].Nodes.Add("same node name");
				Nodes[1].Nodes.Add("B");

				TreeViewSearcher = new RecursiveTreeViewSearcher();
			}

			public void PopulateNode(TreeNode node)
			{
				if (node.Level == 0 && node.Nodes.Count == 0)
				{
					node.Nodes.Add(node.Index == 0 ? "Inf2" : "Inf3");
				}
			}
		}

		public void TestTreeViewSearcherInstantiated()
		{
			using (var testForm = new TreeViewFindFormForTest(testTreeView))
			{
				Assert("FindForm doesn't have an instantiated TreeViewSearcher", testForm.GetTreeViewSearcher() != null);
			}
		}

		[DeveloperOnlyTest]
		public void TestShowRecursiveTree()
		{
			using (var testTreeView = new RepeatedTreeView())
			{
				ShowOnForm(testTreeView);
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
		}

		public void TestNonRecursiveTreeSearcher_SearchRepeatedNames()
		{
			using (var testTreeView = new RepeatedTreeView())
			using (var testForm = new TreeViewFindFormForTest(testTreeView))
			{
				testForm.SetTreeViewSearcher(new NonRecursiveTreeViewSearcher());

				testForm.Show();

				var findTextBox = testForm.GetFindTextBox();
				var findNextButton = testForm.GetFindNextButton();

				findTextBox.Text = "B";
				findNextButton.PerformClick();

				AssertEquals("When using a NonRecursiveTreeSearcher, deeply search nodes even if an identically named node has appeared before", "B", testTreeView.SelectedNode.Text);

				findTextBox.Text = "same node name";
				findNextButton.PerformClick();

				AssertEquals("When AddNodesWithRepeatedText is true, add nodes to the results list even if it contains identically named ones already", 5, testForm.GetResults().Total);
			}
		}

		public void TestRecursiveTreeSearcher_SearchesBreadthFirst_AndProcedurally()
		{
			using (var testTreeView = new RepeatedTreeView())
			using (var testForm = new TreeViewFindFormForTest(testTreeView))
			{
				testForm.Show();

				var findTextBox = testForm.GetFindTextBox();
				var findNextButton = testForm.GetFindNextButton();

				findTextBox.Text = "same node name";
				findNextButton.PerformClick();

				AssertEquals(testTreeView.SelectedNode, testTreeView.Nodes[1]);

				findNextButton.PerformClick();

				AssertEquals(testTreeView.SelectedNode, testTreeView.Nodes[0].Nodes[1]);
			}
		}

		#region TreeViewFindFormForTest

		public class TreeViewFindFormForTest : TreeViewFindForm
		{
			public TreeViewFindFormForTest(ZTreeView treeView) : base(treeView)
			{
			}

			public ITreeViewSearcher GetTreeViewSearcher()
			{
				return treeViewSearcher;
			}

			public void SetTreeViewSearcher(ITreeViewSearcher treeViewSearcher)
			{
				this.treeViewSearcher = treeViewSearcher;
			}

			public ISearchResults GetResults()
			{
				return results;
			}

			public ZButton GetFindPreviousButton()
			{
				return FindPreviousButton;
			}

			public ZButton GetFindNextButton()
			{
				return FindNextButton;
			}

			public TextBox GetFindTextBox()
			{
				return FindTextBox;
			}

			public ZCheckBox GetMatchCaseCheckBox()
			{
				return MatchCaseCheckBox;
			}

			public ZCheckBox GetContinueCheckBox()
			{
				return SearchFromSelectedNodeCheckBox;
			}

			public ZLabel GetMatchesLabel()
			{
				return MatchesLabel;
			}

			public void StartFindForTest(bool findNext)
			{
				StartFind(findNext);
			}
		}

		#endregion

		#region Implementation

		ZTreeView testTreeView;

		void PopulateTestTreeView()
		{
			testTreeView = TreeViewCreatorForTests.CreateTree(@"
																A			B
														     A0          B0	  B1
															  ");
			testTreeView.TreeViewSearcher = new NonRecursiveTreeViewSearcher();
		}

		protected override void SetUp()
		{
			base.SetUp();
			PopulateTestTreeView();
		}

		protected override void TearDown()
		{
			testTreeView.Dispose();
			base.TearDown();
		}

		protected override Form GetFormToBashCore()
		{
			return new TreeViewFindForm(testTreeView);
		}

		#endregion
	}
}
