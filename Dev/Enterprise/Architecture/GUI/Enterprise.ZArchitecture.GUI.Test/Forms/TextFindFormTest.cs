using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(TextFindForm))]
	sealed class TextFindFormTest : ZFormBasherTest
	{
#if !WINZOR
		public void TestNoText()
		{
			textFindControl.Text = ZString.Empty;
			using (var form = new TextFindForm(textFindControl))
			{
				var findTextBox = (ZTextBox)form.Controls.Find("FindTextBox", true).FirstOrDefault();
				var matchesLabel = (ZLabel)form.Controls.Find("MatchesLabel", true).FirstOrDefault();

				findTextBox.Text = "a";
				CombineAssertions(() =>
				{
					form.StartFind(true);
					AssertEquals("textFindControl selected text", string.Empty, textFindControl.SelectedText);
					AssertEquals("matchesLabel text", "No Results", matchesLabel?.Text);
				});
			}
		}

		public void TestNoMatches()
		{
			textFindControl.Text = "abcefg";
			using (var form = new TextFindForm(textFindControl))
			{
				var findTextBox = (ZTextBox)form.Controls.Find("FindTextBox", true).FirstOrDefault();
				var matchesLabel = (ZLabel)form.Controls.Find("MatchesLabel", true).FirstOrDefault();

				findTextBox.Text = "asd";
				CombineAssertions(() =>
				{
					form.StartFind(true);
					AssertEquals("textFindControl selected text", string.Empty, textFindControl.SelectedText);
					AssertEquals("matchesLabel text", "No Results", matchesLabel?.Text);
				});
			}
		}

		public void TestMatchCase_Enabled()
		{
			textFindControl.Text = "asd vxc";
			using (var form = new TextFindForm(textFindControl))
			{
				var matchesLabel = (ZLabel)form.Controls.Find("MatchesLabel", true).FirstOrDefault();
				var findTextBox = (ZTextBox)form.Controls.Find("FindTextBox", true).FirstOrDefault();

				var matchCaseCheckBox = (ZCheckBox)form.Controls.Find("MatchCaseCheckBox", true).FirstOrDefault();
				matchCaseCheckBox.Checked = true;
				CombineAssertions(() =>
				{
					findTextBox.Text = "ASD";
					form.StartFind(true);
					AssertEquals("textFindControl selected text ASD", string.Empty, textFindControl.SelectedText);
					AssertEquals("matchesLabel text ASD", "No Results", matchesLabel?.Text);

					findTextBox.Text = "asd";
					form.StartFind(true);
					AssertEquals("textFindControl selected text asd", "asd", textFindControl.SelectedText);
					AssertEquals("matchesLabel text asd", "Match 1 of 1", matchesLabel?.Text);
				});
			}
		}

		public void TestMatchCase_Disabled()
		{
			textFindControl.Text = "asd vxc";
			using (var form = new TextFindForm(textFindControl))
			{
				var matchesLabel = (ZLabel)form.Controls.Find("MatchesLabel", true).FirstOrDefault();
				var matchCaseCheckBox = (ZCheckBox)form.Controls.Find("MatchCaseCheckBox", true).FirstOrDefault();
				var findTextBox = (ZTextBox)form.Controls.Find("FindTextBox", true).FirstOrDefault();

				CombineAssertions(() =>
				{
					findTextBox.Text = "asd";
					form.StartFind(true);
					AssertEquals("textFindControl selected text asd", "asd", textFindControl.SelectedText);
					AssertEquals("matchesLabel text asd", "Match 1 of 1", matchesLabel?.Text);

					findTextBox.Text = "ASD";
					form.StartFind(true);
					AssertEquals("textFindControl selected text ASD", "asd", textFindControl.SelectedText);
					AssertEquals("matchesLabel text ASD", "Match 1 of 1", matchesLabel?.Text);
				});
			}
		}

		public void TestFindNext()
		{
			textFindControl.Text = "asd zxc poi poi zxc asd";
			using (var form = new TextFindForm(textFindControl))
			{
				var findTextBox = (ZTextBox)form.Controls.Find("FindTextBox", true).FirstOrDefault();
				var matchesLabel = (ZLabel)form.Controls.Find("MatchesLabel", true).FirstOrDefault();

				findTextBox.Text = "asd";
				CombineAssertions(() =>
				{
					form.StartFind(true);
					AssertEquals("match 1 of 2 ", "asd", textFindControl.SelectedText);
					AssertEquals("matchesLabel 1 of 2", "Match 1 of 2", matchesLabel?.Text);
					form.StartFind(true);
					AssertEquals("match 2 of 2 ", "asd", textFindControl.SelectedText);
					AssertEquals("matchesLabel 2 of 2", "Match 2 of 2", matchesLabel?.Text);
					form.StartFind(true);
					AssertEquals("match 1 of 2 wrap around", "asd", textFindControl.SelectedText);
					AssertEquals("matchesLabel 1 of 2 wrap around", "Match 1 of 2", matchesLabel?.Text);
				});
			}
		}

		public void TestFindPrevious()
		{
			textFindControl.Text = "asd zxc poi poi zxc asd";
			using (var form = new TextFindForm(textFindControl))
			{
				var findTextBox = (ZTextBox)form.Controls.Find("FindTextBox", true).FirstOrDefault();
				var matchesLabel = (ZLabel)form.Controls.Find("MatchesLabel", true).FirstOrDefault();

				findTextBox.Text = "asd";
				CombineAssertions(() =>
				{
					form.StartFind(false);
					AssertEquals("match 2 of 2 ", "asd", textFindControl.SelectedText);
					AssertEquals("matchesLabel 2 of 2", "Match 2 of 2", matchesLabel?.Text);
					form.StartFind(false);
					AssertEquals("match 1 of 2 ", "asd", textFindControl.SelectedText);
					AssertEquals("matchesLabel 1 of 2", "Match 1 of 2", matchesLabel?.Text);
					form.StartFind(false);
					AssertEquals("match 2 of 2 wrap around", "asd", textFindControl.SelectedText);
					AssertEquals("matchesLabel 2 of 2 wrap around", "Match 2 of 2", matchesLabel?.Text);
				});
			}
		}

		public void TestControls()
		{
			using (var form = new TextFindForm(textFindControl))
			{
				CombineAssertions(() =>
				{
					var findTextBox = (ZTextBox)form.Controls.Find("FindTextBox", true).FirstOrDefault();
					AssertNotNull("FindTextBox", findTextBox);

					var findNextButton = (ZButton)form.Controls.Find("FindNextButton", true).FirstOrDefault();
					AssertNotNull("FindNextButton", findNextButton);
					AssertEquals("FindNextButton should be disabled on start", false, findNextButton?.Enabled);
					AssertEquals("FindNextButton Caption", "Find Next", findNextButton?.CaptionResourceString?.Caption);

					var findPreviousButton = (ZButton)form.Controls.Find("FindPreviousButton", true).FirstOrDefault();
					AssertNotNull("FindPreviousButton", findPreviousButton);
					AssertEquals("FindPreviousButton should be disabled on start", false, findPreviousButton?.Enabled);
					AssertEquals("FindPreviousButton Caption", "Find Previous", findPreviousButton?.CaptionResourceString?.Caption);

					var closeButton = (ZButton)form.Controls.Find("CloseButton", true).FirstOrDefault();
					AssertNotNull("CloseButton", closeButton);
					AssertEquals("CloseButton Caption", "Close", closeButton?.CaptionResourceString?.Caption);

					var matchCaseCheckBox = (ZCheckBox)form.Controls.Find("MatchCaseCheckBox", true).FirstOrDefault();
					AssertNotNull("MatchCaseCheckBox", matchCaseCheckBox);
					AssertEquals("MatchCaseCheckBox Caption", "Match Case", matchCaseCheckBox?.CaptionResourceString?.Caption);
					AssertEquals("MatchCaseCheckBox Checked", false, matchCaseCheckBox.Checked);

					var matchesLabel = (ZLabel)form.Controls.Find("MatchesLabel", true).FirstOrDefault();
					AssertNotNull("MatchesLabel", matchesLabel);
					AssertEquals("MatchesLabel Default Caption", "No Results", matchesLabel?.CaptionResourceString?.Caption);

					var findLabel = (ZLabel)form.Controls.Find("FindLabel", true).FirstOrDefault();
					AssertNotNull("FindLabel", findLabel);
					AssertEquals("FindLabel Caption", "Find what", findLabel?.CaptionResourceString?.Caption);

					findTextBox.Text = "asd";
					AssertEquals("FindNextButton should be enabled on not empty findTextBox", true, findNextButton?.Enabled);
					AssertEquals("FindPreviousButton should be enabled on not empty findTextBox", true, findPreviousButton?.Enabled);
				});
			}
		}
#endif

		protected override Form GetFormToBashCore() => new TextFindForm(textFindControl);

		protected override void SetUp()
		{
			base.SetUp();
			textFindControl = new ZTextBox();
		}
		ZTextBox textFindControl;

		protected override void TearDown()
		{
			textFindControl.Dispose();
			base.TearDown();
		}
	}
}
