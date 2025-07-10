using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core; //used for Enterprise.ZArchitecture.Core.StringExtensions Contains. No idea why Visual Studio thinks it's unnecessary...
#pragma warning restore IDE0005

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap
{
	public partial class MapTreeFindForm : ZChildForm
	{
		readonly TreeView originalTreeView;
		readonly MapTreeUserControl mapTreeUserControl;
		internal readonly List<MapTreeUserControl.MapTreeNotNode> matchingNotNodes = new List<MapTreeUserControl.MapTreeNotNode>();
		int indexFoundNodes;
		bool havePopulatedAggressively;

		public MapTreeFindForm()
		{
			InitializeComponent();
		}

		public MapTreeFindForm(TreeView treeView, MapTreeUserControl mapTreeUserControl)
		{
			originalTreeView = treeView;
			this.mapTreeUserControl = mapTreeUserControl;
			FindPreviousButton.Enabled = true;
			FindNextButton.Enabled = true;
			AcceptButton = FindNextButton;
		}

		#region UI Logic

		public void FindExactly(string bindingMember)
		{
			FindTextBox.Text = bindingMember;
			RefreshSearch();
			GiveMeNode(0);
		}

		void PopulateMatches()
		{
			if (string.IsNullOrWhiteSpace(FindTextBox.Text))
			{
				return;
			}

			//1) if we already have some nodes, swap over to aggression
			if (matchingNotNodes.Count > 0)
			{
				PopulateMatchesAggressively();
				return;
			}

			//2) if that fails, try splitting by . and iterating up through the roots (populating as needed)
			var members = FindTextBox.Text.ToLowerInvariant().Split('.', '+');
			var currentLayer = mapTreeUserControl.rootNotNodes;
			var partial = false;
			MapTreeUserControl.MapTreeNotNode found = null;
			foreach (var member in members)
			{
				found = null;
				foreach (var candidate in currentLayer)
				{
					if (candidate.name.ToLowerInvariant() == member)
					{
						currentLayer = candidate.Children;
						found = candidate;
						break;
					}
				}
				if (found == null)
				{
					//fall back to finding a partial match if we don't find an exact match
					foreach (var candidate in currentLayer)
					{
						if (candidate.name.ToLowerInvariant().Contains(member))
						{
							currentLayer = candidate.Children;
							found = candidate;
							partial = true;
							break;
						}
					}
				}
				if (found == null)
				{
					break;
				}
			}
			if (found != null)
			{
				matchingNotNodes.Add(found);
				Label.Text = partial ? Res.GetString("f503eb95-bb1d-4510-881c-cd9e204c4f8f", "Partial match found") : Res.GetString("db489ab3-358a-4d9f-8046-0167b3c594d9", "Exact match found");
				return;
			}

			//3) if that fails, just look at the text of every node
			PopulateMatchesAggressively();
		}

		void PopulateMatchesAggressively()
		{
			if (havePopulatedAggressively)
			{
				return;
			}
			RefreshSearch();
			foreach (var notNode in mapTreeUserControl.AllNotNodes)
			{
				if (notNode.text.Contains(FindTextBox.Text, StringComparison.CurrentCultureIgnoreCase))
				{
					matchingNotNodes.Add(notNode);
				}
			}

			havePopulatedAggressively = true;
		}

		internal MapTreeNode GiveMeNode(int impulse = 0)
		{
			if (matchingNotNodes.Count == 0)
			{
				PopulateMatches();
				if (impulse == 1)
				{
					impulse = 0;
				}
			}

			if (matchingNotNodes.Count == 0)
			{
				Label.Text = Res.GetString("TreeViewFindForm|53e475a5-e85b-41c1-bca9-2eee27b27354", "No Results");
				Globals.Message.ShowInformation(Res.GetString("A5B2F77C-25D8-4D0A-9748-E749A4FAD3A7", "The specified text was not found."), Res.GetString("0F219B5B-CA25-4D7B-BDC1-570FF4A2CB7E", "Not Found"));
				return null;
			}
			else
			{
				//Enact the following strategy:
				//1) If the count changed and the old node is still in the collection, then go to there +/- 1.
				//2) Else just wrap around.
				MapTreeUserControl.MapTreeNotNode old = null;
				var oldIndexFoundNodes = indexFoundNodes;
				var oldCount = matchingNotNodes.Count;
				if (matchingNotNodes.Count > indexFoundNodes)
				{
					old = matchingNotNodes[indexFoundNodes];
				}

				indexFoundNodes += impulse;
				if (indexFoundNodes < 0)
				{
					PopulateMatchesAggressively();
					indexFoundNodes = matchingNotNodes.Count - 1;
				}
				else if (indexFoundNodes >= matchingNotNodes.Count)
				{
					PopulateMatchesAggressively();
					indexFoundNodes = 0;
				}

				if (matchingNotNodes.Count > oldCount)
				{
					var index = matchingNotNodes.FindIndex(x => x == old);
					if (index > -1)
					{
						indexFoundNodes = index + impulse;
					}
				}

				if (indexFoundNodes < 0)
				{
					indexFoundNodes = matchingNotNodes.Count - 1;
				}
				else if (indexFoundNodes >= matchingNotNodes.Count)
				{
					indexFoundNodes = 0;
				}
			}

			if (matchingNotNodes.Count > indexFoundNodes)
			{
				var foundNode = matchingNotNodes[indexFoundNodes].MapTreeNode;
				if (originalTreeView.SelectedNode != foundNode)
				{
					originalTreeView.SelectedNode = foundNode;
					if (this.Visible)
					{
						FindTextBox.Focus();
					}
				}
				if (havePopulatedAggressively)
				{
					Label.Text = Res.GetString("53d38828-b6af-4ef9-813a-beb9d9a90043", "{0} out of {1} matches", indexFoundNodes + 1, matchingNotNodes.Count);
				}
				return foundNode;
			}
			else
			{
				Label.Text = Res.GetString("TreeViewFindForm|53e475a5-e85b-41c1-bca9-2eee27b27354", "No Results");
				Globals.Message.ShowInformation(Res.GetString("A5B2F77C-25D8-4D0A-9748-E749A4FAD3A7", "The specified text was not found."), Res.GetString("0F219B5B-CA25-4D7B-BDC1-570FF4A2CB7E", "Not Found"));
				return null;
			}
		}

		internal void RefreshSearch()
		{
			matchingNotNodes.Clear();
			indexFoundNodes = 0;
			havePopulatedAggressively = false;
		}

		#endregion

		#region Event Handlers
		void FindPreviousButton_Click(object sender, EventArgs e)
		{
			GiveMeNode(-1);
		}
		protected void FindNextButton_Click(object sender, EventArgs e)
		{
			GiveMeNode(1);
		}

		protected void FindTextBox_TextChanged(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(FindTextBox.Text))
			{
				RefreshSearch();
			}
		}
		protected void ResetButton_Click(object sender, EventArgs e)
		{
			RefreshSearch();
			if (originalTreeView.Nodes != null)
			{
				mapTreeUserControl.mapTreeView.CollapseAll();
			}
		}
		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void OnClosed(EventArgs e)
		{
			mapTreeUserControl.DisposeFindForm();
			base.OnClosed(e);
		}

		#endregion
	}
}
