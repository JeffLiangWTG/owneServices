using System;
using System.Windows.Forms;
using CargoWise.Data;
using Enterprise.ZArchitecture.GUI.Forms.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class TreeViewFindForm : ZChildForm
	{
		readonly ZTreeView originalTreeView;
#if DEBUG
		protected
#endif
		ISearchResults results;
		string oldText;
		bool oldCaseBool;
		bool oldContBool;
		TreeNode oldSelectedNode;

		public TreeViewFindForm(TreeView treeView)
		{
			originalTreeView = (ZTreeView)treeView;
			AcceptButton = FindNextButton;
			UpdateButtons();

			SearchFromSelectedNodeCheckBox.Checked = true;

			oldText = FindTextBox.Text;
			oldCaseBool = MatchCaseCheckBox.Checked;
			oldContBool = SearchFromSelectedNodeCheckBox.Checked;
			oldSelectedNode = treeView.SelectedNode;

			treeViewSearcher = originalTreeView.TreeViewSearcher ?? new NonRecursiveTreeViewSearcher();
		}

		#region UI Logic

		void UpdateButtons()
		{
			FindPreviousButton.Enabled = FindNextButton.Enabled = !string.IsNullOrEmpty(FindTextBox.Text);
		}

		protected void StartFind(bool findNext)
		{
			using (Db.Connection.StartScalarCaching())
			{
				originalTreeView.LoadRegistryNodes();
				AcceptButton = (findNext) ? FindNextButton : FindPreviousButton;

				if (oldText != FindTextBox.Text || oldCaseBool != MatchCaseCheckBox.Checked || oldContBool != SearchFromSelectedNodeCheckBox.Checked || (SearchFromSelectedNodeCheckBox.Checked && oldSelectedNode != originalTreeView.SelectedNode))
				{
					MatchesLabel.Text = Res.GetString("TreeViewFindForm|5a5aBe6b-bef3-4ae0-acd3-1f34a7750122", "Searching...");
					MatchesLabel.Refresh();

					if (treeViewSearcher is NonRecursiveTreeViewSearcher nonRecursiveTreeViewSearcher)
					{
						nonRecursiveTreeViewSearcher.Cont = SearchFromSelectedNodeCheckBox.Checked;
					}

					results = treeViewSearcher.Search(originalTreeView, FindTextBox.Text, MatchCaseCheckBox.Checked ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);

					if (results.Total > 0 || results.Total == -1)
					{
						originalTreeView.SelectedNode = findNext ? results.Next() : results.Previous();
						if (originalTreeView.SelectedNode != null)
						{
							originalTreeView.SelectedNode.EnsureVisible();
							FindTextBox.Focus();
							MatchesLabel.Text = results.Total > 0 ? Res.GetString("TreeViewFindForm|50a9437b-dd3a-48a0-843b-d738422211c5", "Match {0} of {1}", ((NonRecursiveSearchResults)results).GetOffsetIndex() + 1, ((NonRecursiveSearchResults)results).Total) : "";
						}
						else
						{
							MatchesLabel.Text = Res.GetString("TreeViewFindForm|0cf728a9-8119-4b17-b4d8-d23b5f3f1f32", "No Results");
						}
					}
					else if (results.Total == 0)
					{
						MatchesLabel.Text = Res.GetString("TreeViewFindForm|0cf728a9-8119-4b17-b4d8-d23b5f3f1f32", "No Results");
					}

					oldText = FindTextBox.Text;
					oldCaseBool = MatchCaseCheckBox.Checked;
					oldContBool = SearchFromSelectedNodeCheckBox.Checked;
					oldSelectedNode = originalTreeView.SelectedNode;
				}
				else
				{
					if (results.Total > 0 || results.Total == -1)
					{
						originalTreeView.SelectedNode = findNext ? results.Next() : results.Previous();
						if (originalTreeView.SelectedNode != null)
						{
							originalTreeView.SelectedNode.EnsureVisible();
							FindTextBox.Focus();
							oldSelectedNode = originalTreeView.SelectedNode;
							MatchesLabel.Text = results.Total > 0 ? Res.GetString("TreeViewFindForm|50a9437b-dd3a-48a0-843b-d738422211c5", "Match {0} of {1}", ((NonRecursiveSearchResults)results).GetOffsetIndex() + 1, ((NonRecursiveSearchResults)results).Total) : "";
						}
						else
						{
							MatchesLabel.Text = Res.GetString("TreeViewFindForm|0cf728a9-8119-4b17-b4d8-d23b5f3f1f32", "No Results");
						}
					}
				}
			}
		}

		#endregion

		#region Searching

		protected ITreeViewSearcher treeViewSearcher;

		#endregion

		#region Event Handlers

		void FindPreviousButton_Click(object sender, EventArgs e)
		{
			StartFind(false);
		}

		void FindNextButton_Click(object sender, EventArgs e)
		{
			StartFind(true);
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void FindTextBox_TextChanged(object sender, EventArgs e)
		{
			UpdateButtons();
		}

		#endregion
	}
}
