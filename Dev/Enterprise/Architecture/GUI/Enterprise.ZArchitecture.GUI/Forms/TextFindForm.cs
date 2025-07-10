using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture
{
	public partial class TextFindForm : ZChildForm
	{
		public TextFindForm(ITextFindControl parentControl)
		{
			this.parentControl = parentControl;
			currentHighlightTextIndex = -1;
			UpdateButtons();
		}
		readonly ITextFindControl parentControl;
		string oldSearchedText;
		bool oldMatchCaseChecked;
		int currentHighlightTextIndex;
		int[] highlightTextPositions;

		void UpdateButtons()
		{
			FindPreviousButton.Enabled = FindNextButton.Enabled = !string.IsNullOrEmpty(FindTextBox.Text);
		}

		public void StartFind(bool findNext)
		{
			var parentControlText = parentControl.GetText();
			if (!string.IsNullOrEmpty(parentControlText))
			{
				var searchedText = FindTextBox.Text;
				var isMatchCaseChecked = MatchCaseCheckBox.CheckState == System.Windows.Forms.CheckState.Checked;
				if (!string.IsNullOrEmpty(searchedText)
					&& (oldSearchedText != searchedText
						|| oldMatchCaseChecked != isMatchCaseChecked))
				{
					SearchForPossibleSearchResults(parentControlText, searchedText, isMatchCaseChecked);
					oldSearchedText = searchedText;
					oldMatchCaseChecked = isMatchCaseChecked;
					currentHighlightTextIndex = -1;
				}

				HighlightSearchedText(findNext);
			}
			else
			{
				MatchesLabel.Text = NoResultCaption;
			}
		}

		void SearchForPossibleSearchResults(string parentControlText, string searchedText, bool isMatchCaseChecked)
		{
			if (!isMatchCaseChecked)
			{
				searchedText = searchedText.ToLower();
				parentControlText = parentControlText.ToLower();
			}
			highlightTextPositions = parentControlText.AllIndexesOf(searchedText).ToArray();
		}

		int GetNextIndex(int lastResultIndex) => currentHighlightTextIndex >= lastResultIndex ? 0 : currentHighlightTextIndex + 1;

		int GetPreviousIndex(int lastResultIndex) => currentHighlightTextIndex <= 0 ? lastResultIndex : currentHighlightTextIndex - 1;

		void HighlightSearchedText(bool findNext)
		{
			var resultsAmount = highlightTextPositions?.Length ?? 0;
			if (resultsAmount != 0)
			{
				var lastResultIndex = resultsAmount - 1;

				currentHighlightTextIndex = findNext
					? GetNextIndex(lastResultIndex)
					: GetPreviousIndex(lastResultIndex);

				MatchesLabel.Text = Res.GetString("TextFindForm|4641E42C-DDD6-4F57-B73C-A12E3C64BB1B", "Match {0} of {1}", currentHighlightTextIndex + 1, resultsAmount);
				parentControl.HighlightText(highlightTextPositions[currentHighlightTextIndex], FindTextBox.Text.Length);
			}
			else
			{
				MatchesLabel.Text = NoResultCaption;
			}
		}

		static string NoResultCaption => Res.GetString("TextFindForm|NoResult", "No Results");

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
	}
}
