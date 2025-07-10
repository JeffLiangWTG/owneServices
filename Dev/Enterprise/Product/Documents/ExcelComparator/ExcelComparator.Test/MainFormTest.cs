using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.IO;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enterprise.ExcelComparator.Testing
{
	sealed class MainFormTest : TestCase
	{
		public void TestIdenticalFilesDoesntBotherToPopupComparisonEngine()
		{
			var file1XlsPath = File1XlsPath;
			using (var mainForm = GetNewFormForTesting(file1XlsPath, file1XlsPath))
			{
				mainForm.Compare();

				AssertMultilineASCIIEquals("mainForm.Dialogs", @"
Title: Excel Comparator
=--------------------------------------------------------------------------------=
Excel Files have Identical comparable text content.
".Trim(), GetFormattedDialogs(mainForm.DialogsForTesting));
			}
		}

		public void TestCompareWorks()
		{
			using (var mainForm = GetNewFormForTesting(null, null))
			{
				mainForm.Compare();

#if NET
				AssertMultilineASCIIEquals("mainForm.Dialogs", @"Title: Error Generating Comparison Files
=--------------------------------------------------------------------------------=
An Error occurred reading []
""The value cannot be an empty string. (Parameter 'path')""

An Error occurred reading []
""The value cannot be an empty string. (Parameter 'path')""".Trim(), GetFormattedDialogs(mainForm.DialogsForTesting));
#else
				AssertMultilineASCIIEquals("mainForm.Dialogs", @"
Title: Error Generating Comparison Files
=--------------------------------------------------------------------------------=
An Error occurred reading []
""Empty path name is not legal.""

An Error occurred reading []
""Empty path name is not legal.""
".Trim(), GetFormattedDialogs(mainForm.DialogsForTesting));
#endif
				mainForm.DialogsForTesting.Clear();

				mainForm.FilePath1 = File1XlsPath;
				mainForm.FilePath2 = File2XlsPath;
				mainForm.Compare();
				AssertMultilineASCIIEquals("mainForm.Dialogs", "", GetFormattedDialogs(mainForm.DialogsForTesting));

				var tempPath = ComparableTextGenerator.GetTempFilePath();
				AssertEquals("mockComparisonTool.LastComparisonFilePath1", Path.Combine(tempPath, "File1.txt1"), mockComparisonTool.LastComparisonFilePath1);
				AssertEquals("mockComparisonTool.LastComparisonFilePath2", Path.Combine(tempPath, "File2.txt2"), mockComparisonTool.LastComparisonFilePath2);
			}
		}

		public void TestCompareConfigurableSectionWorks()
		{
			var configurableSectionsXlsPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ExcelComparator.Testing.TestFiles.FileWithConfigurableSections.xls", "FileWithConfigurableSections.xls");
			var configurableSectionsInDifferentOrderButSameStripContentsXlsPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.ExcelComparator.Testing.TestFiles.FileWithConfigurableSectionsInDifferentOrderButSameStripContents.xls", "FileWithConfigurableSectionsInDifferentOrderButSameStripContents.xls");
			
			using (var mainForm = GetNewFormForTesting(configurableSectionsXlsPath, configurableSectionsInDifferentOrderButSameStripContentsXlsPath))
			{
				mainForm.SetCompareSectionsOptionForTesting(false);
				mainForm.Compare();
				AssertEquals("mockComparisonTool.GetDifferences()", @"
line:[13]   file1:[{A}-[#ConfigurableSection:YYY, B]]   file2:[{A}-[#ConfigurableSection:XXX, C]]
line:[14]   file1:[{A}-[Text 2]]   file2:[{A}-[Text 3]   {B}-[=A6]]
line:[15]   file1:[{A}-[#ConfigurableSection:XXX, C]]   file2:[{A}-[#ConfigurableSection:YYY, B]]
line:[16]   file1:[{A}-[Text 3]   {B}-[=A8]]   file2:[{A}-[Text 2]]
".Trim(), mockComparisonTool.Differences);
				AssertEquals("mainForm.Dialogs", "", GetFormattedDialogs(mainForm.DialogsForTesting));

				mainForm.SetCompareSectionsOptionForTesting(true);
				mainForm.Compare();
				AssertEquals("mainForm.Dialogs", @"
Title: Excel Comparator
=--------------------------------------------------------------------------------=
Excel Files have Identical comparable text content.
".Trim(), GetFormattedDialogs(mainForm.DialogsForTesting));
			}
		}

		public void TestFile1ChangeButton()
		{
			var file1XlsPath = File1XlsPath;
			using (var mainForm = GetNewFormForTesting(null, null))
			{
				mainForm.file1ChangeButton_Click(null, null);
				AssertMultilineASCIIEquals("mainForm.Dialogs", @"
Title: Select File
=--------------------------------------------------------------------------------=
InitialDirectory=[]
FileName=[]
".Trim(), GetFormattedDialogs(mainForm.DialogsForTesting));
				mainForm.DialogsForTesting.Clear();

				mainForm.FilePath1 = file1XlsPath;

				mainForm.file1ChangeButton_Click(null, null);
				AssertMultilineASCIIEquals("mainForm.Dialogs", string.Format(@"
Title: Select File
=--------------------------------------------------------------------------------=
InitialDirectory=[{0}]
FileName=[{1}]
", Path.GetDirectoryName(file1XlsPath), file1XlsPath).Trim(), GetFormattedDialogs(mainForm.DialogsForTesting));
			}
		}

		public void TestFile2ChangeButton()
		{
			var file2XlsPath = File2XlsPath;
			using (var mainForm = GetNewFormForTesting(null, null))
			{
				mainForm.file2ChangeButton_Click(null, null);
				AssertMultilineASCIIEquals("mainForm.Dialogs", @"
Title: Select File
=--------------------------------------------------------------------------------=
InitialDirectory=[]
FileName=[]
".Trim(), GetFormattedDialogs(mainForm.DialogsForTesting));
				mainForm.DialogsForTesting.Clear();

				mainForm.FilePath2 = file2XlsPath;

				mainForm.file2ChangeButton_Click(null, null);
				AssertMultilineASCIIEquals("mainForm.Dialogs", string.Format(@"
Title: Select File
=--------------------------------------------------------------------------------=
InitialDirectory=[{0}]
FileName=[{1}]
", Path.GetDirectoryName(file2XlsPath), file2XlsPath).Trim(), GetFormattedDialogs(mainForm.DialogsForTesting));
			}
		}

		public void TestVisualStudioCompareSetupIsUsedAsPreferredCompareTool()
		{
			var storedPreferences = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\14.0\TeamFoundation\SourceControl\DiffTools\.*\Compare", true);
			var existingPreferences = storedPreferences?.GetValue("Command")?.ToString();
			var file1XlsPath = File1XlsPath;
			var file2XlsPath = File2XlsPath;
			try
			{
				if (!string.IsNullOrEmpty(existingPreferences))
				{
					storedPreferences.DeleteValue("Command");
				}
				else
				{
					storedPreferences = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\VisualStudio\14.0\TeamFoundation\SourceControl\DiffTools\.*\Compare");
				}

				var mainForm = new MainForm(file1XlsPath, file2XlsPath);
				AssertEquals("Should not have any stored preferences yet", mainForm.ComparisonTools[0].Path, "C:\\Program Files\\Beyond Compare 4\\BComp.exe");

				storedPreferences.SetValue("Command", "C:\\Totally\\Real\\Path");
				mainForm = new MainForm(file1XlsPath, file2XlsPath);
				AssertEquals("Should now have stored preferences as first option", mainForm.ComparisonTools[0].Path, "C:\\Totally\\Real\\Path");
			}
			finally
			{
				if (!string.IsNullOrEmpty(existingPreferences))
				{
					storedPreferences.SetValue("Command", existingPreferences);
				}
				else
				{
					Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\Microsoft\VisualStudio\14.0\TeamFoundation\SourceControl\DiffTools\.*\Compare");
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		string File1XlsPath => resourceRetriever.Value.SaveResourceToFile("Enterprise.ExcelComparator.Testing.TestFiles.File1.xls", "File1.xls");

		string File2XlsPath => resourceRetriever.Value.SaveResourceToFile("Enterprise.ExcelComparator.Testing.TestFiles.File2.xls", "File2.xls");

		MainForm GetNewFormForTesting(string filePath1, string filePath2)
		{
			var mainForm = new MainForm(filePath1, filePath2);
			mainForm.DialogsForTesting = new List<MainForm.Dialog>();
			mockComparisonTool = new MockComparisonTool();
			mainForm.comparisonTools = new List<IComparisonTool> { mockComparisonTool };
			return mainForm;
		}

		MockComparisonTool mockComparisonTool;

		string GetFormattedDialogs(List<MainForm.Dialog> dialogs)
		{
			string separatorLine = "=".PadRight(81, '-') + "=";
			List<String> result = new List<string>();
			foreach (var dialog in dialogs)
			{
				result.Add(string.Format("Title: {0}\r\n{1}\r\n{2}", dialog.Title, separatorLine, dialog.Message));
			}
			return string.Join("\r\n\r\n\r\n", result.ToArray());
		}
	}
}
