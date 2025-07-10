using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(ReportColumnSettings))]
	sealed class ReportColumnSettingsTest : ValueObjectTestCase
	{
		public void TestClone()
		{
			ReportColumnSettings settings = new ReportColumnSettings();
			settings.Worksheets.AddNew("Sheet1");
			settings.Worksheets.AddNew("Sheet2");
			settings.Version = 1;
			settings.SelectedGroupByName = "aaa";
			settings.SelectedSortOrderName = "bbb";
			settings.SelectedLanguage = Core.SharedConstants.Languages.ChineseSimplified;
			settings.Filters = new byte[5];

			ReportColumnSettings settingsClone = settings.Clone();
			AssertEquals("report column settings should be versoin 1", 1, settingsClone.Version);
			AssertEquals("report column settings should have 2 sheets", 2, settingsClone.Worksheets.Count);
			AssertEquals("SelectedGroupByName", "aaa", settingsClone.SelectedGroupByName);
			AssertEquals("SelectedSortOrderName", "bbb", settingsClone.SelectedSortOrderName);
			AssertEquals("SelectedLanguage", Core.SharedConstants.Languages.ChineseSimplified, settingsClone.SelectedLanguage);
			AssertEquals("Filters", 5, settingsClone.Filters.Length);
		}

		public void TestCloneVisibleOnly()
		{
			ReportColumnSettings settings = new ReportColumnSettings();
			settings.Worksheets.AddNew("Sheet1");
			settings.Worksheets["Sheet1"].ColumnHeadings.Add(new ColumnHeading("Column1", "", "", 1, 1, 10, false));
			settings.Worksheets["Sheet1"].ColumnHeadings.Add(new ColumnHeading("Column1", "", "", 1, 1, 10, true));
			settings.Worksheets.AddNew("Sheet2");
			settings.Version = 1;

			ReportColumnSettings settingsClone = settings.CloneVisibleOnly();
			AssertEquals("report column settings should be versoin 1", 1, settingsClone.Version);
			AssertEquals("report column settings should have 1 sheets", 1, settingsClone.Worksheets.Count);
			AssertEquals("Sheet 1 should have only 1 column", 1, settingsClone.Worksheets["Sheet1"].ColumnHeadings.Count);
		}
	}
}
