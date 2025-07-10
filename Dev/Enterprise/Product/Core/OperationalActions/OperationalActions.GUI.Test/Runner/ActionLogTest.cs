using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ErrorLevel = Enterprise.Services.OperationalActions.Support.OperationalActionLogErrorLevel;

namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	sealed class ActionLogTest : TestCase
	{
		public void TestHighestErrorLevelEncountered()
		{
			AssertEquals("Default to informational", ErrorLevel.Informational, Log.HighestErrorLevelEncountered);

			log.Notify(ErrorLevel.Informational, "");
			AssertEquals("Stick with informitional", ErrorLevel.Informational, Log.HighestErrorLevelEncountered);

			log.Notify(ErrorLevel.Warning, "");
			AssertEquals("Warning is higher than informational", ErrorLevel.Warning, log.HighestErrorLevelEncountered);

			log.Notify(ErrorLevel.Informational, "");
			AssertEquals("Informational is lower that warning", ErrorLevel.Warning, log.HighestErrorLevelEncountered);

			log.Notify(ErrorLevel.Error, "");
			AssertEquals("Error is higher than warning", ErrorLevel.Error, log.HighestErrorLevelEncountered);

			log.Notify(ErrorLevel.Warning, "");
			AssertEquals("warning is lower than error", ErrorLevel.Error, log.HighestErrorLevelEncountered);
		}

		public void TestHyperlink()
		{
			RichTextBox textBox = (RichTextBox)Log.Controls["logTextBox"];

			log.NotifyFormat(ErrorLevel.Informational, "{1} {0} {1}", new LogUrlLink("Link", new Uri("http://www.cargowise.com")), "Text");
			AssertEquals("Text Link#KEY0000 Text\n", textBox.Text); // Note: #KEY0000 is the hidden text included as part of the link to help resolve ambiguity.

			textBox.Select(0, 5);
			AssertEquals(false, textBox.GetSelectionLink());

			textBox.Select(5, 12);
			AssertEquals(true, textBox.GetSelectionLink());

			textBox.Select(17, 5);
			AssertEquals(false, textBox.GetSelectionLink());
		}

		#if !WINZOR
		public void TestAddingColouredTextWhenOtherTextSelected()
		{
			RichTextBox textBox = (RichTextBox)Log.Controls["logTextBox"];

			Log.Notify(ErrorLevel.Informational, "Informational");
			Log.Notify(ErrorLevel.Warning, "Warning");
			Log.Notify(ErrorLevel.Error, "Error");

			string rtf1 = textBox.Rtf;

			textBox.SelectionStart = 10;
			textBox.SelectionLength = 15;
			Log.Notify(ErrorLevel.Warning, "Blaticus");

			AssertMultilineASCIIEquals("Dont screw the formatting if some text happens to be selected when Notify() is called.", rtf1.Replace("\\cf2 Error\\par", "\\cf2 Error\\par\n\\cf1 Blaticus\\par"), textBox.Rtf);
			AssertEquals("SelectionStart should not have changed", 10, textBox.SelectionStart);
			AssertEquals("SelectionLength should not have changed", 15, textBox.SelectionLength);
		}
		#endif

		public void TestProgress()
		{
			Log.SetMasterProgressMax(3);
			AssertProgress("SetMasterProgressMax", 0, 3, 0, 0);

			Log.SetSectionProgressMax(2);
			AssertProgress("SetSectionProgressMax", 0, 3, 0, 2);

			Log.BumpSectionProgress();
			AssertProgress("BumpSectionProgress", 0, 3, 1, 2);

			Log.BumpSectionProgress();
			AssertProgress("BumpSectionProgress again", 0, 3, 2, 2);

			Log.BumpMasterProgress();
			AssertProgress("BumpSectionProgress", 1, 3, 0, 0);

			Log.BumpMasterProgress();
			AssertProgress("BumpSectionProgress again", 2, 3, 0, 0);

			log.SetSectionProgressMax(2);
			log.BumpSectionProgress();
			log.BumpSectionProgress();
			AssertProgress("Just before the end", 2, 3, 2, 2);

			Log.BumpMasterProgress();
			AssertProgress("BumpSectionProgress final", 3, 3, 1, 1);
		}

		#region Implementation

		ActionLog Log
		{
			get { return log ?? (log = new ActionLog()); }
		}
		ActionLog log;

		void AssertProgress(string message, int masterCount, int masterMax, int sectionCount, int sectionMax)
		{
			const string format = "({0}/{1}),({2}/{3})";

			ProgressBar mainProgress = (ProgressBar)Log.Controls["masterProgressBar"];
			ProgressBar sectionProgress = (ProgressBar)Log.Controls["sectionProgressBar"];

			AssertEquals(message,
				string.Format(format, masterCount, masterMax, sectionCount, sectionMax),
				string.Format(format, mainProgress.Value, mainProgress.Maximum, sectionProgress.Value, sectionProgress.Maximum)
			);
		}

		protected override void TearDown()
		{
			if (log != null)
			{
				log.Dispose();
				log = null;
			}
			base.TearDown();
		}

		#endregion
	}
}
