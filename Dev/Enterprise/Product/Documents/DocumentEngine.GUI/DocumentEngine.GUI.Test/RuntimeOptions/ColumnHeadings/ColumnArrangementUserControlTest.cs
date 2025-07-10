using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed partial class ColumnArrangementUserControlTest : ZFormBasherTest
	{
		public void TestSettingsComboBoxSelectedItemShouldBeDefaultWhenReportCurrentManagerIsNull()
		{
			headingManager = new ColumnConfigurationsManager(ZGuid.NewZGuid(), false);
			var linkedField = new LookupField(Factory);
			linkedField.DisplayName = "Test";
			linkedField.SetCollectionProvider(new RefUNLOCOCollectionProvider(Factory));
			headingManager.AddLinkedField(linkedField);
			headingManager.SaveToFilterField = "Test";
			headingManager.IsReportTitleChangeable = true;
			headingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("2", "2", "2", 1, 4, 1, true));
			headingManager.DefaultTemplateConfigurationManager.SetTitle("Sheet1", "Title1");

			using (var control = GetNewControl())
			{
				AssertEquals("should be 2 items", 2, control.SettingsComboBox.Items.Count);
				AssertEquals("First should be template default", Report.ColumnHeadingManager.DefaultTemplateConfigurationManager, control.SettingsComboBox.Items[0]);
				AssertEquals("Second should be company default", Report.ColumnHeadingManager.CompanyDefaultConfigurationManager, control.SettingsComboBox.Items[1]);
				AssertEquals("DefaultTemplateConfigurationManager should be seleted in combo", Report.ColumnHeadingManager.DefaultTemplateConfigurationManager, control.SettingsComboBox.SelectedItem);
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestColumnPropertiesWithNoItem()
		{
			using (var form = new ZForm())
			using (var control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.tabColumnConfig.SelectedIndex = 0;
				control.IncludedColumnsListBox.Items.Clear();
				AssertEquals("Included columns should contain nothing", 0, control.IncludedColumnsListBox.Items.Count);
				control.tabColumnConfig.SelectedIndex = 1;
				AssertEquals("The DisplayName TextBox must be empty", "", control.ColumnDisplayNameTextBox.Text);
			}
		}

		public void TestSelectedColumnsListBox()
		{
			using (var form = new ZForm())
			using (var control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.AddButton.PerformClick();
				AssertEquals("Prerequisite: IncludedColumnsListBox.Items.Count", 2, control.IncludedColumnsListBox.Items.Count);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "3", "3", "3", 3, 1, 3, false);
				control.tabColumnConfig.SelectedIndex = 1;
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Add button clicked and Column properties tab is selected", control.IncludedColumnsListBox.Items.Count, control.SelectedColumnslistBox.Items.Count);
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Add button clicked and Column properties tab is selected", "4", control.SelectedColumnslistBox.Items[0].ToString());
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Add button clicked and Column properties tab is selected", "3", control.SelectedColumnslistBox.Items[1].ToString());
				AssertEquals("SelectedIndex in SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Add button clicked and Column properties tab is selected", control.SelectedColumnslistBox.SelectedIndex, control.IncludedColumnsListBox.SelectedIndex);

				control.tabColumnConfig.SelectedIndex = 0;
				control.AvailableColumnsListBox.SelectedIndex = 0;
				control.AddButton.PerformClick();
				AssertEquals("Prerequisite: IncludedColumnsListBox.Items.Count", 3, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("Prerequisite: IncludedColumnsListBox.SelectedIndex", 2, control.IncludedColumnsListBox.SelectedIndex);
				control.tabColumnConfig.SelectedIndex = 1;
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Add button clicked and Column properties tab is selected", 3, control.SelectedColumnslistBox.Items.Count);
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Add button clicked and Column properties tab is selected", "4", control.SelectedColumnslistBox.Items[0].ToString());
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Add button clicked and Column properties tab is selected", "3", control.SelectedColumnslistBox.Items[1].ToString());
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Add button clicked and Column properties tab is selected", "1", control.SelectedColumnslistBox.Items[2].ToString());
				AssertEquals("SelectedIndex in SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Add button clicked and Column properties tab is selected", control.SelectedColumnslistBox.SelectedIndex, control.IncludedColumnsListBox.SelectedIndex);

				control.tabColumnConfig.SelectedIndex = 0;
				control.MoveUpButton.PerformClick();
				AssertEquals("Prerequisite: IncludedColumnsListBox.Items.Count", 3, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("Prerequisite: IncludedColumnsListBox.SelectedIndex", 1, control.IncludedColumnsListBox.SelectedIndex);
				control.tabColumnConfig.SelectedIndex = 1;
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Move Up button clicked and Column properties tab is selected", 3, control.SelectedColumnslistBox.Items.Count);
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Move Up button clicked and Column properties tab is selected", "4", control.SelectedColumnslistBox.Items[0].ToString());
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Move Up button clicked and Column properties tab is selected", "1", control.SelectedColumnslistBox.Items[1].ToString());
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Move Up button clicked and Column properties tab is selected", "3", control.SelectedColumnslistBox.Items[2].ToString());
				AssertEquals("SelectedIndex in SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Move Up button clicked and Column properties tab is selected", control.SelectedColumnslistBox.SelectedIndex, control.IncludedColumnsListBox.SelectedIndex);

				control.tabColumnConfig.SelectedIndex = 0;
				control.MoveDownButton.PerformClick();
				AssertEquals("Prerequisite: IncludedColumnsListBox.Items.Count", 3, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("Prerequisite: IncludedColumnsListBox.SelectedIndex", 2, control.IncludedColumnsListBox.SelectedIndex);
				control.tabColumnConfig.SelectedIndex = 1;
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Move Down button clicked and Column properties tab is selected", 3, control.SelectedColumnslistBox.Items.Count);
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Move Down button clicked and Column properties tab is selected", "4", control.SelectedColumnslistBox.Items[0].ToString());
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Move Down button clicked and Column properties tab is selected", "3", control.SelectedColumnslistBox.Items[1].ToString());
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Move Down button clicked and Column properties tab is selected", "1", control.SelectedColumnslistBox.Items[2].ToString());
				AssertEquals("SelectedIndex in SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Move Down button clicked and Column properties tab is selected", control.SelectedColumnslistBox.SelectedIndex, control.IncludedColumnsListBox.SelectedIndex);

				control.tabColumnConfig.SelectedIndex = 0;
				control.RemoveButton.PerformClick();
				AssertEquals("Prerequisite: IncludedColumnsListBox.Items.Count", 2, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("Prerequisite: IncludedColumnsListBox.SelectedIndex", 1, control.IncludedColumnsListBox.SelectedIndex);
				control.tabColumnConfig.SelectedIndex = 1;
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Remove button clicked and Column properties tab is selected", 2, control.SelectedColumnslistBox.Items.Count);
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Remove button clicked and Column properties tab is selected", "4", control.SelectedColumnslistBox.Items[0].ToString());
				AssertEquals("SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Remove button clicked and Column properties tab is selected", "3", control.SelectedColumnslistBox.Items[1].ToString());
				AssertEquals("SelectedIndex in SelectedColumnslistBox list must be the same as in IncludedColumnsListBox after Remove button clicked and Column properties tab is selected", control.SelectedColumnslistBox.SelectedIndex, control.IncludedColumnsListBox.SelectedIndex);
			}
		}

		[GuiTest]
		[RequiresSTA]
		public void TestAvailableColumnsListBox()
		{
			var manager = HeadingManager.DefaultTemplateConfigurationManager;
			manager.AddHeading("Sheet1", new ColumnHeading("5", "5", "5", 5, 5, 8, true));
			manager.AddHeading("Sheet1", new ColumnHeading("6", "6", "6", 6, 5, 8, true));
			manager.AddHeading("Sheet1", new ColumnHeading("7", "7", "7", 7, 5, 8, true));
			manager.Load(Report.FilterCollection, Report.GroupByCollection, Report.SortOrderCollection, Report.OrientationManager, Report.Parent);

			using (var form = new ZForm())
			using (var control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("AvailableColumnsListBox.Items.Count", 5, control.AvailableColumnsListBox.Items.Count);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[0], "1", "1", "1", 2, 3, 2, true);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[1], "2", "2", "2", 1, 4, 1, true);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[2], "5", "5", "5", 5, 5, 8, true);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[3], "6", "6", "6", 6, 5, 8, true);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[4], "7", "7", "7", 7, 5, 8, true);
			}
		}

		[GuiTest]
		public void TestAvailableColumnsListBox_HideIfDescriptionEmpty()
		{
			var manager = HeadingManager.DefaultTemplateConfigurationManager;
			manager.AddHeading("Sheet1", new ColumnHeading("5", "", "", 5, 5, 5, true));
			manager.AddHeading("Sheet1", new ColumnHeading("6", "", "", 6, 6, 6, true) { HideIfDescriptionEmpty = true });
			manager.AddHeading("Sheet1", new ColumnHeading("7", "7", "", 7, 7, 7, true) { HideIfDescriptionEmpty = true });
			manager.Load(Report.FilterCollection, Report.GroupByCollection, Report.SortOrderCollection, Report.OrientationManager, Report.Parent);

			using (var form = new ZForm())
			using (var control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("AvailableColumnsListBox.Items.Count", 5, control.AvailableColumnsListBox.Items.Count);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[0], "1", "1", "1", 2, 3, 2, true);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[1], "2", "2", "2", 1, 4, 1, true);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[2], "5", "", "", 5, 5, 5, true);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[3], "6", "", "", 6, 6, 6, true, true);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[4], "7", "7", "", 7, 7, 7, true, true);
			}
		}

		public void TestComboBoxConfigWorksheetShouldBeTranslatable()
		{
			using (var mockData = Res.UseMockData())
			{
				var key = DocBuilderResourceStrings.ReportNameKeyPrefix + "Sheet1";
				mockData.Put(key, new ResourceStringData(key, string.Empty, string.Empty, "测试", string.Empty));
				using (var control = GetNewControl())
				{
					AssertEquals("测试", ((Worksheet)control.comboBoxConfigWorksheet.SelectedItem).NameLocalized);
				}
			}
		}

		public void TestAddAndRemoveColumns()
		{
			using (var form = new ZForm())
			using (var control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.AddButton.PerformClick();

				AssertEquals("AvailableColumnsListBox.Items.Count", 2, control.AvailableColumnsListBox.Items.Count);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[0], "1", "1", "1", 2, 3, 2, true);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[1], "2", "2", "2", 1, 4, 1, true);

				AssertEquals("IncludedColumnsListBox.Items.Count", 2, control.IncludedColumnsListBox.Items.Count);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "3", "3", "3", 3, 1, 3, false);
				control.AvailableColumnsListBox.SelectedIndex = 0;
				control.AddButton.PerformClick();

				AssertEquals("AvailableColumnsListBox.Items.Count", 1, control.AvailableColumnsListBox.Items.Count);
				AssertEquals("AvailableColumnsListBox.SelectedIndex", 0, control.AvailableColumnsListBox.SelectedIndex);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[0], "2", "2", "2", 1, 3, 1, true);

				AssertEquals("IncludedColumnsListBox.Items.Count", 3, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("IncludedColumnsListBox.SelectedIndex", 2, control.IncludedColumnsListBox.SelectedIndex);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "3", "3", "3", 3, 1, 3, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "1", "1", "1", 2, 2, 2, false);

				control.AddButton.PerformClick();

				AssertEquals("AvailableColumnsListBox.Items.Count", 0, control.AvailableColumnsListBox.Items.Count);
				AssertEquals("IncludedColumnsListBox.Items.Count", 4, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("IncludedColumnsListBox.SelectedIndex", 3, control.IncludedColumnsListBox.SelectedIndex);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "3", "3", "3", 3, 1, 3, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "1", "1", "1", 2, 2, 2, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[3], "2", "2", "2", 1, 3, 1, false);

				control.RemoveButton.PerformClick();

				AssertEquals("AvailableColumnsListBox.Items.Count", 1, control.AvailableColumnsListBox.Items.Count);
				AssertEquals("AvailableColumnsListBox.SelectedIndex", 0, control.AvailableColumnsListBox.SelectedIndex);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[0], "2", "2", "2", 1, 0, 1, true);

				AssertEquals("IncludedColumnsListBox.Items.Count", 3, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("IncludedColumnsListBox.SelectedIndex", 2, control.IncludedColumnsListBox.SelectedIndex);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "3", "3", "3", 3, 1, 3, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "1", "1", "1", 2, 2, 2, false);

				control.IncludedColumnsListBox.ClearSelected();
				control.IncludedColumnsListBox.SelectedIndex = 1;
				control.RemoveButton.PerformClick();

				AssertEquals("AvailableColumnsListBox.Items.Count", 2, control.AvailableColumnsListBox.Items.Count);
				AssertEquals("AvailableColumnsListBox.SelectedIndex", 1, control.AvailableColumnsListBox.SelectedIndex);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[0], "2", "2", "2", 1, 0, 1, true);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[1], "3", "3", "3", 3, 1, 3, true);

				AssertEquals("IncludedColumnsListBox.Items.Count", 2, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("IncludedColumnsListBox.SelectedIndex", 1, control.IncludedColumnsListBox.SelectedIndex);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "1", "1", "1", 2, 1, 2, false);
			}
		}

		[RequiresSTA]
		public void TestAddAndRemoveColumns_MultiMode()
		{
			using (var form = new ZForm())
			using (var control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.AddButton.PerformClick();

				AssertEquals("AvailableColumnsListBox.Items.Count", 2, control.AvailableColumnsListBox.Items.Count);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[0], "1", "1", "1", 2, 3, 2, true);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[1], "2", "2", "2", 1, 4, 1, true);

				AssertEquals("IncludedColumnsListBox.Items.Count", 2, control.IncludedColumnsListBox.Items.Count);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "3", "3", "3", 3, 1, 3, false);
				control.AvailableColumnsListBox.SelectedIndex = 0;
				control.AvailableColumnsListBox.SelectedIndex = 1;
				control.AddButton.PerformClick();

				AssertEquals("AvailableColumnsListBox.Items.Count", 0, control.AvailableColumnsListBox.Items.Count);
				AssertEquals("IncludedColumnsListBox.Items.Count", 4, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("IncludedColumnsListBox.SelectedIndex", 2, control.IncludedColumnsListBox.SelectedIndex);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "3", "3", "3", 3, 1, 3, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "1", "1", "1", 2, 2, 2, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[3], "2", "2", "2", 1, 3, 1, false);

				control.IncludedColumnsListBox.ClearSelected();
				control.IncludedColumnsListBox.SelectedIndex = 3;
				control.RemoveButton.PerformClick();

				AssertEquals("AvailableColumnsListBox.Items.Count", 1, control.AvailableColumnsListBox.Items.Count);
				AssertEquals("AvailableColumnsListBox.SelectedIndex", 1, control.AvailableColumnsListBox.SelectedIndices.Count);
				AssertEquals("AvailableColumnsListBox.SelectedIndex", 0, control.AvailableColumnsListBox.SelectedIndex);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[0], "2", "2", "2", 1, 0, 1, true);

				AssertEquals("IncludedColumnsListBox.Items.Count", 3, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("IncludedColumnsListBox.SelectedIndex", 2, control.IncludedColumnsListBox.SelectedIndex);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "3", "3", "3", 3, 1, 3, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "1", "1", "1", 2, 2, 2, false);

				control.IncludedColumnsListBox.ClearSelected();
				control.IncludedColumnsListBox.SelectedIndex = 1;
				control.IncludedColumnsListBox.SelectedIndex = 2;
				control.RemoveButton.PerformClick();

				AssertEquals("AvailableColumnsListBox.Items.Count", 3, control.AvailableColumnsListBox.Items.Count);
				AssertEquals("AvailableColumnsListBox.SelectedIndices.Count", 2, control.AvailableColumnsListBox.SelectedIndices.Count);
				AssertEquals("AvailableColumnsListBox.SelectedIndex", true, control.AvailableColumnsListBox.SelectedIndices.Contains(0));
				AssertEquals("AvailableColumnsListBox.SelectedIndex", true, control.AvailableColumnsListBox.SelectedIndices.Contains(2));
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[0], "1", "1", "1", 2, 2, 2, true);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[1], "2", "2", "2", 1, 0, 1, true);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[2], "3", "3", "3", 3, 1, 3, true);

				AssertEquals("IncludedColumnsListBox.Items.Count", 1, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("IncludedColumnsListBox.SelectedIndex", 0, control.IncludedColumnsListBox.SelectedIndex);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
			}
		}

		public void TestSettingsComboxBoxIndexChanged()
		{
			var dummyHeadingManager = new ColumnConfigurationsManager(ZGuid.Empty, false);
			dummyHeadingManager.CurrentConfiguration.Worksheets.AddNew("Sheet1", "Title2");
			dummyHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings.Add(new ColumnHeading("1"));
			dummyHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].Hidden = true;

			HeadingManager.UpdateFromDeserialisedValue(dummyHeadingManager, null);

			using (var control = GetNewControl())
			{
				control.SettingsComboBox.SelectedIndex = 0;
				AssertEquals("RemoveButton.Enabled", false, control.DeleteButton.Enabled);
				AssertEquals("SaveButton.Enabled", false, control.SaveButton.Enabled);
				AssertEquals("report.ColumnHeadingManager.Headings[0].Hidden", true, HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].Hidden);
				AssertEquals("The report title should be Title2", "Title2", HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].Title);

				control.SettingsComboBox.SelectedIndex = 2;
				AssertEquals("RemoveButton.Enabled", true, control.DeleteButton.Enabled);
				AssertEquals("SaveButton.Enabled", true, control.SaveButton.Enabled);
				AssertEquals("report.ColumnHeadingManager.Headings[0].Hidden", true, HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].Hidden);
				AssertEquals("The report title should be Title1", "Title1", HeadingManager.CurrentConfiguration.Worksheets["Sheet1"].Title);
			}
		}

		public void TestSettingsComboxBoxDefaultSelectedItem()
		{
			Report.ColumnHeadingManager.RefreshCurrentConfiguration(Report.ColumnHeadingManager.CurrentConfiguration, Report.ColumnHeadingManager.CompanyDefaultConfigurationManager);

			using (var control = GetNewControl())
			{
				AssertEquals("should be 2 items", 2, control.SettingsComboBox.Items.Count);
				AssertEquals("First should be template default", Report.ColumnHeadingManager.DefaultTemplateConfigurationManager, control.SettingsComboBox.Items[0]);
				AssertEquals("Second should be company default", Report.ColumnHeadingManager.CompanyDefaultConfigurationManager, control.SettingsComboBox.Items[1]);
				AssertEquals("CompanyDefaultConfigurationManager should be seleted in combo", Report.ColumnHeadingManager.CompanyDefaultConfigurationManager, control.SettingsComboBox.SelectedItem);
			}
		}

		[RequiresSTA]
		public void TestModifyingSelectedColumnHeading()
		{
			using (var report = new Report(pack, MultipleTemplatesWithOptionalColumns, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();

				using (var form = new Form())
				using (var arrangementControl = new ColumnArrangementUserControl(report))
				{
					form.Controls.Add(arrangementControl);
					form.Show();
					arrangementControl.tabColumnConfig.SelectedTab = arrangementControl.tabPage2;

					AssertEquals("ColumnDisplayNameTextBox.Text", "Sheet 1 Column 1", arrangementControl.ColumnDisplayNameTextBox.Text);
					AssertEquals("ColumnDisplayWidthCalcEdit.Text", "156", arrangementControl.ColumnDisplayWidthCalcEdit.Text);

					AssertEquals("Selected columns list should have 3 items", 3, arrangementControl.SelectedColumnslistBox.Items.Count);
					arrangementControl.SelectedColumnslistBox.SelectedIndex = 0;
					AssertEquals("Selected Column should be 'Sheet 1 Column 1'", "Sheet 1 Column 1", arrangementControl.SelectedColumnslistBox.Text);
					AssertEquals("ColumnDisplayWidthCalcEdit.Text", "156", arrangementControl.ColumnDisplayWidthCalcEdit.Text);

					arrangementControl.ColumnPropertiesGroupBox.Focus();
					arrangementControl.ColumnDisplayNameTextBox.Focus();
					arrangementControl.ColumnDisplayNameTextBox.Text = "6";
					arrangementControl.SettingsComboBox.Focus();
					ColumnHeadingTest.AssertPropertyValues(arrangementControl.SelectedColumnslistBox.Items[0], "Sheet 1 Column 1", "Sheet 1 Column 1", "6", 1, 0, 156, false);

					arrangementControl.SettingsComboBox.Focus();
					arrangementControl.ColumnDisplayWidthCalcEdit.Focus();
					arrangementControl.ColumnDisplayWidthCalcEdit.Text = "7";
					arrangementControl.SettingsComboBox.Focus();
					ColumnHeadingTest.AssertPropertyValues(arrangementControl.SelectedColumnslistBox.Items[0], "Sheet 1 Column 1", "Sheet 1 Column 1", "6", 1, 0, 7, false);

					arrangementControl.SelectedColumnslistBox.SelectedIndex = 1;
					AssertEquals("ColumnDisplayNameTextBox.Text", "Sheet 1 Column 2", arrangementControl.ColumnDisplayNameTextBox.Text);
					AssertEquals("ColumnDisplayWidthCalcEdit.Text", "156", arrangementControl.ColumnDisplayWidthCalcEdit.Text);
				}
			}
		}

		public void TestValidateSelectedColumnHeading()
		{
			using (var report = new Report(pack, MultipleTemplatesWithOptionalColumns, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();

				using (var form = new Form())
				using (var arrangementControl = new ColumnArrangementUserControl(report))
				{
					form.Controls.Add(arrangementControl);
					form.Show();
					arrangementControl.tabColumnConfig.SelectedTab = arrangementControl.tabPage2;

					var columnHeadingNotificationsOnTextBox = arrangementControl.NotificationExOnTextBox.Notifications1 as List<PropertyNotification>;
					var columnHeadingNotificationsOnLabel = arrangementControl.NotificationExOnLabel.Notifications1 as List<PropertyNotification>;
					Assert(columnHeadingNotificationsOnTextBox != null);
					Assert(columnHeadingNotificationsOnLabel != null);

					arrangementControl.ColumnDisplayNameTextBox.Text = "TestColumn";
					arrangementControl.ColumnTagNameTextBox.Text = ZString.Empty;
					arrangementControl.ColumnDisplayNameTextBox_Leave(null, new EventArgs());
					Assert("Should not have warning message have shown in columnDisplayNameTextBox", columnHeadingNotificationsOnTextBox.Count == 0);
					Assert("Should not have warning message have shown on SelectedColumns label", columnHeadingNotificationsOnLabel.Count == 0);

					arrangementControl.ColumnDisplayNameTextBox.Text = string.Empty;
					arrangementControl.ColumnDisplayNameTextBox_Leave(null, new EventArgs());
					Assert("Should have warning message have shown in columnDisplayNameTextBox", columnHeadingNotificationsOnTextBox.Count > 0);
					Assert("Should have warning message have shown on SelectedColumns label", columnHeadingNotificationsOnLabel.Count > 0);
					AssertEquals("A specific warning message should have shown in columnDisplayNameTextBox", "This column cannot be exported to CSV or XML files because its display name doesn't contain any alphabetic character.", columnHeadingNotificationsOnTextBox[0].Message);
					AssertEquals("A specific warning message should have shown on SelectedColumns label", "One or more columns cannot be exported to CSV or XML files because their display names don't contain any alphabetic character.", columnHeadingNotificationsOnLabel[0].Message);

					arrangementControl.ColumnDisplayNameTextBox.Text = "@#$";
					arrangementControl.ColumnDisplayNameTextBox_Leave(null, new EventArgs());
					Assert("Should have warning message have shown in columnDisplayNameTextBox", columnHeadingNotificationsOnTextBox.Count > 0);
					Assert("Should have warning message have shown on SelectedColumns label", columnHeadingNotificationsOnLabel.Count > 0);
					AssertEquals("A specific warning message should have shown in columnDisplayNameTextBox", "This column cannot be exported to CSV or XML files because its display name doesn't contain any alphabetic character.", columnHeadingNotificationsOnTextBox[0].Message);
					AssertEquals("A specific warning message should have shown on SelectedColumns label", "One or more columns cannot be exported to CSV or XML files because their display names don't contain any alphabetic character.", columnHeadingNotificationsOnLabel[0].Message);

					arrangementControl.ColumnTagNameTextBox.Text = "Sango";
					arrangementControl.UpdateSelectedColumnTagName();
					arrangementControl.ColumnDisplayNameTextBox_Leave(null, new EventArgs());
					Assert("Shouldn't have warning message have shown in columnDisplayNameTextBox", columnHeadingNotificationsOnTextBox.Count == 0);
					Assert("Shouldn't have warning message have shown on SelectedColumns label", columnHeadingNotificationsOnLabel.Count == 0);
				}
			}
		}

		[RequiresSTA]
		public void TestValidateTagNameTextBox()
		{
			using (var report = new Report(pack, MultipleTemplatesWithOptionalColumns, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();

				using (var form = new Form())
				using (var arrangementControl = new ColumnArrangementUserControl(report))
				{
					form.Controls.Add(arrangementControl);
					form.Show();
					arrangementControl.tabColumnConfig.SelectedTab = arrangementControl.tabPage2;

					var columnHeadingNotificationsOnTagNameTextBox = arrangementControl.NotificationExOnTagNameTextBox.Notifications1 as List<PropertyNotification>;
					Assert(columnHeadingNotificationsOnTagNameTextBox != null);

					arrangementControl.ColumnTagNameTextBox.Text = "TestColumn";
					arrangementControl.ColumnTagNameTextBox_Leave(null, new EventArgs());
					Assert("Should not have warning message have shown in columnTagNameTextBox", columnHeadingNotificationsOnTagNameTextBox.Count == 0);

					arrangementControl.ColumnTagNameTextBox.Text = string.Empty;
					arrangementControl.ColumnTagNameTextBox_Leave(null, new EventArgs());
					Assert("Shouldn't have warning message have shown in columnTagNameTextBox", columnHeadingNotificationsOnTagNameTextBox.Count == 0);

					arrangementControl.ColumnTagNameTextBox.Text = "Sango % _ 123543@#$";
					arrangementControl.ColumnTagNameTextBox_Leave(null, new EventArgs());
					AssertEquals("The Value after formatting should be equal to ", "SangoPercent_123543", arrangementControl.ColumnTagNameTextBox.Text);
					Assert("Should have warning message have shown in columnTagNameTextBox", columnHeadingNotificationsOnTagNameTextBox.Count > 0);
					AssertEquals("A specific warning message should have shown in columnTagNameTextBox", ElementNameRulesTest, columnHeadingNotificationsOnTagNameTextBox[0].Message);
				}
			}
		}

		public void TestDuplicateTagNameTextBox()
		{
			using (var report = new Report(pack, MultipleTemplatesWithOptionalColumns, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();

				using (var form = new Form())
				using (var arrangementControl = new ColumnArrangementUserControl(report))
				{
					form.Controls.Add(arrangementControl);
					form.Show();
					arrangementControl.tabColumnConfig.SelectedTab = arrangementControl.tabPage2;

					var columnHeadingNotificationsOnTagNameTextBox = arrangementControl.NotificationExOnTagNameTextBox.Notifications1 as List<PropertyNotification>;
					var columnHeadingNotificationsOnLabel = arrangementControl.NotificationExOnLabel.Notifications1 as List<PropertyNotification>;
					Assert(columnHeadingNotificationsOnTagNameTextBox != null);
					Assert(columnHeadingNotificationsOnLabel != null);

					arrangementControl.ColumnPropertiesGroupBox.Focus();
					AssertEquals("ColumnDisplayNameTextBox.Text", "Sheet 1 Column 1", arrangementControl.ColumnDisplayNameTextBox.Text);
					AssertEquals("ColumnDisplayWidthCalcEdit.Text", "156", arrangementControl.ColumnDisplayWidthCalcEdit.Text);

					AssertEquals("Selected columns list should have 3 items", 3, arrangementControl.SelectedColumnslistBox.Items.Count);
					var firstItemListBox = (ColumnHeading)arrangementControl.SelectedColumnslistBox.Items[0];
					firstItemListBox.TagName = "Aragon";

					arrangementControl.SelectedColumnslistBox.Focus();
					arrangementControl.SelectedColumnslistBox.SelectedIndex = 1;

					arrangementControl.ColumnTagNameTextBox.Text = "1Aragon";
					arrangementControl.ColumnTagNameTextBox_Leave(null, new EventArgs());
					AssertEquals("Selected Column should be 'Sheet 1 Column 1'", arrangementControl.ColumnTagNameTextBox.Text, ((ColumnHeading)arrangementControl.SelectedColumnslistBox.Items[1]).TagName);

					Assert("Should have warning message have shown in columnTagNameTextBox", columnHeadingNotificationsOnLabel.Count > 0);
					AssertEquals("A specific warning message should have shown in columnTagNameTextBox", "One or more columns cannot be properly exported to CSV or XML files. Their display names are not unique once invalid XML characters have been removed.", columnHeadingNotificationsOnLabel[0].Message);
					AssertEquals("A specific warning message should have shown in columnTagNameTextBox", ElementNameRulesTest, columnHeadingNotificationsOnTagNameTextBox[0].Message);
				}
			}
		}

		[RequiresSTA]
		public void TestMoveColumnUpAndDown()
		{
			using (var form = new ZForm())
			using (var control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.IncludedColumnsListBox.ClearSelected();
				control.IncludedColumnsListBox.SelectedIndex = 0;
				control.MoveUpButton.PerformClick();

				AssertEquals("IncludedColumnsListBox.SelectedIndex", 0, control.IncludedColumnsListBox.SelectedIndex);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "3", "3", "3", 3, 1, 3, false);

				control.MoveDownButton.PerformClick();

				AssertEquals("IncludedColumnsListBox.SelectedIndex", 1, control.IncludedColumnsListBox.SelectedIndex);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "3", "3", "3", 3, 0, 3, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "4", "4", "4", 4, 1, 4, false);

				control.MoveDownButton.PerformClick();

				AssertEquals("IncludedColumnsListBox.SelectedIndex", 1, control.IncludedColumnsListBox.SelectedIndex);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "3", "3", "3", 3, 0, 3, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "4", "4", "4", 4, 1, 4, false);

				control.MoveUpButton.PerformClick();
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "3", "3", "3", 3, 1, 3, false);
			}
		}

		[RequiresSTA]
		public void TestMoveColumnUpAndDown_MultiMode()
		{
			using (var form = new ZForm())
			using (var control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.AvailableColumnsListBox.ClearSelected();
				control.AvailableColumnsListBox.SelectedIndex = 1;
				control.AddButton.PerformClick();
				control.AvailableColumnsListBox.ClearSelected();
				control.AvailableColumnsListBox.SelectedIndex = 0;
				control.AddButton.PerformClick();
				control.IncludedColumnsListBox.ClearSelected();
				control.IncludedColumnsListBox.SelectedIndices.Add(0);
				control.IncludedColumnsListBox.SelectedIndices.Add(2);

				control.MoveUpButton.PerformClick();
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 0", true, control.IncludedColumnsListBox.SelectedIndices.Contains(0));
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 2", true, control.IncludedColumnsListBox.SelectedIndices.Contains(2));
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "3", "3", "3", 3, 1, 3, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "2", "2", "2", 1, 2, 1, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[3], "1", "1", "1", 2, 3, 2, false);

				control.IncludedColumnsListBox.ClearSelected();
				control.IncludedColumnsListBox.SelectedIndices.Add(0);
				control.IncludedColumnsListBox.SelectedIndices.Add(1);
				control.MoveDownButton.PerformClick();
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 1", true, control.IncludedColumnsListBox.SelectedIndices.Contains(1));
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 2", true, control.IncludedColumnsListBox.SelectedIndices.Contains(2));
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "2", "2", "2", 1, 0, 1, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "4", "4", "4", 4, 1, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "3", "3", "3", 3, 2, 3, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[3], "1", "1", "1", 2, 3, 2, false);

				control.MoveDownButton.PerformClick();
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 2", true, control.IncludedColumnsListBox.SelectedIndices.Contains(2));
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 3", true, control.IncludedColumnsListBox.SelectedIndices.Contains(3));
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "2", "2", "2", 1, 0, 1, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "1", "1", "1", 2, 1, 2, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "4", "4", "4", 4, 2, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[3], "3", "3", "3", 3, 3, 3, false);

				control.MoveDownButton.PerformClick();
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 2", true, control.IncludedColumnsListBox.SelectedIndices.Contains(2));
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 3", true, control.IncludedColumnsListBox.SelectedIndices.Contains(3));
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "2", "2", "2", 1, 0, 1, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "1", "1", "1", 2, 1, 2, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "4", "4", "4", 4, 2, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[3], "3", "3", "3", 3, 3, 3, false);

				control.MoveUpButton.PerformClick();
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 1", true, control.IncludedColumnsListBox.SelectedIndices.Contains(1));
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 2", true, control.IncludedColumnsListBox.SelectedIndices.Contains(2));
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "2", "2", "2", 1, 0, 1, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "4", "4", "4", 4, 1, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "3", "3", "3", 3, 2, 3, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[3], "1", "1", "1", 2, 3, 2, false);

				control.IncludedColumnsListBox.ClearSelected();
				control.IncludedColumnsListBox.SelectedIndices.Add(1);
				control.IncludedColumnsListBox.SelectedIndices.Add(3);
				control.MoveDownButton.PerformClick();
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 1", true, control.IncludedColumnsListBox.SelectedIndices.Contains(1));
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 3", true, control.IncludedColumnsListBox.SelectedIndices.Contains(3));
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "2", "2", "2", 1, 0, 1, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "4", "4", "4", 4, 1, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "3", "3", "3", 3, 2, 3, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[3], "1", "1", "1", 2, 3, 2, false);

				control.MoveUpButton.PerformClick();
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 0", true, control.IncludedColumnsListBox.SelectedIndices.Contains(0));
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 2", true, control.IncludedColumnsListBox.SelectedIndices.Contains(2));
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "2", "2", "2", 1, 1, 1, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "1", "1", "1", 2, 2, 2, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[3], "3", "3", "3", 3, 3, 3, false);

				control.MoveDownButton.PerformClick();
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 1", true, control.IncludedColumnsListBox.SelectedIndices.Contains(1));
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 3", true, control.IncludedColumnsListBox.SelectedIndices.Contains(3));
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "2", "2", "2", 1, 0, 1, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "4", "4", "4", 4, 1, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "3", "3", "3", 3, 2, 3, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[3], "1", "1", "1", 2, 3, 2, false);
			}
		}

		public void TestDragAndDropIncludedColumnsListBox()
		{
			var type = typeof(ZListBox);
			var methodZListBox_DragOver = type.GetMethod("ZListBox_DragOver", BindingFlags.NonPublic | BindingFlags.Instance);
			var methodZListBox_DragDrop = type.GetMethod("ZListBox_DragDrop", BindingFlags.NonPublic | BindingFlags.Instance);

			using (var form = new ZForm())
			using (var control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.IncludedColumnsListBox.ClearSelected();
				control.AvailableColumnsListBox.ClearSelected();
				control.AvailableColumnsListBox.SelectedIndex = 1;
				control.AddButton.PerformClick();
				control.AvailableColumnsListBox.SelectedIndex = 0;
				control.AddButton.PerformClick();
				Assert("MoveUpButton should be enabled", control.MoveUpButton.Enabled);
				Assert("MoveDownButton should be enabled", control.MoveDownButton.Enabled);

				// Move items up
				control.IncludedColumnsListBox.SelectedIndices.Add(1);
				control.IncludedColumnsListBox.SelectedIndices.Add(3);
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 1", true, control.IncludedColumnsListBox.SelectedIndices.Contains(1));
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 3", true, control.IncludedColumnsListBox.SelectedIndices.Contains(3));
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "3", "3", "3", 3, 1, 3, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "2", "2", "2", 1, 2, 1, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[3], "1", "1", "1", 2, 3, 2, false);

				var dataObj = new DataObject();
				dataObj.SetData(control.IncludedColumnsListBox);
				var drgevent = new DragEventArgs(dataObj, 0, 5, 10, DragDropEffects.Move, DragDropEffects.Move);
				methodZListBox_DragOver.Invoke(control.IncludedColumnsListBox, new object[] { control.IncludedColumnsListBox.SelectedItems, drgevent });
				methodZListBox_DragDrop.Invoke(control.IncludedColumnsListBox, new object[] { control.IncludedColumnsListBox.SelectedItems, drgevent });
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "3", "3", "3", 3, 0, 3, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "1", "1", "1", 2, 1, 2, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "4", "4", "4", 4, 2, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[3], "2", "2", "2", 1, 3, 1, false);

				// Move items down
				control.IncludedColumnsListBox.SelectedIndices.Clear();
				control.IncludedColumnsListBox.SelectedIndices.Add(0);
				control.IncludedColumnsListBox.SelectedIndices.Add(1);
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 0", true, control.IncludedColumnsListBox.SelectedIndices.Contains(0));
				AssertEquals("IncludedColumnsListBox.SelectedIndex: 1", true, control.IncludedColumnsListBox.SelectedIndices.Contains(1));

				dataObj = new DataObject();
				dataObj.SetData(control.IncludedColumnsListBox);
				drgevent = new DragEventArgs(dataObj, 0, 5, 70, DragDropEffects.Move, DragDropEffects.Move);
				methodZListBox_DragOver.Invoke(control.IncludedColumnsListBox, new object[] { control.IncludedColumnsListBox.SelectedItems, drgevent });
				methodZListBox_DragDrop.Invoke(control.IncludedColumnsListBox, new object[] { control.IncludedColumnsListBox.SelectedItems, drgevent });
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "2", "2", "2", 1, 1, 1, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "3", "3", "3", 3, 2, 3, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[3], "1", "1", "1", 2, 3, 2, false);
			}
		}

		public void TestDragAndDropAvailableColumnsListBoxToIncludedColumnsListBox()
		{
			var type = typeof(ZListBox);
			var methodZListBox_DragOver = type.GetMethod("ZListBox_DragOver", BindingFlags.NonPublic | BindingFlags.Instance);
			var methodZListBox_DragDrop = type.GetMethod("ZListBox_DragDrop", BindingFlags.NonPublic | BindingFlags.Instance);

			using (var form = new ZForm())
			using (var control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("AvailableColumnsListBox.Items.Count", 2, control.AvailableColumnsListBox.Items.Count);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[0], "1", "1", "1", 2, 3, 2, true);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[1], "2", "2", "2", 1, 4, 1, true);

				AssertEquals("IncludedColumnsListBox.Items.Count", 2, control.IncludedColumnsListBox.Items.Count);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "3", "3", "3", 3, 1, 3, false);

				control.AvailableColumnsListBox.SelectedIndices.Add(0);
				var dataObj = new DataObject();
				dataObj.SetData(control.AvailableColumnsListBox);
				var drgevent = new DragEventArgs(dataObj, 0, 5, 20, DragDropEffects.Move, DragDropEffects.Move);
				Assert("isDragging should be false", !control.isDragging);
				control.AvailableColumnsListBox_MouseDown(null, null);
				Assert("isDragging should be true", control.isDragging);

				methodZListBox_DragOver.Invoke(control.IncludedColumnsListBox, new object[] { control.IncludedColumnsListBox.SelectedItems, drgevent });
				methodZListBox_DragDrop.Invoke(control.IncludedColumnsListBox, new object[] { control.IncludedColumnsListBox.SelectedItems, drgevent });
				control.AvailableColumnsListBox_MouseUp(null, null);
				Assert("isDragging should be false", !control.isDragging);

				AssertEquals("AvailableColumnsListBox.Items.Count", 1, control.AvailableColumnsListBox.Items.Count);
				ColumnHeadingTest.AssertPropertyValues(control.AvailableColumnsListBox.Items[0], "2", "2", "2", 1, 3, 1, true);

				AssertEquals("IncludedColumnsListBox.Items.Count", 3, control.IncludedColumnsListBox.Items.Count);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[0], "4", "4", "4", 4, 0, 4, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[1], "1", "1", "1", 2, 1, 2, false);
				ColumnHeadingTest.AssertPropertyValues(control.IncludedColumnsListBox.Items[2], "3", "3", "3", 3, 2, 3, false);
			}

			using (var form = new ZForm())
			using (var control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.AvailableColumnsListBox.SelectedIndices.Add(0);
				var dataObj = new DataObject();
				dataObj.SetData(control.AvailableColumnsListBox);
				var drgevent = new DragEventArgs(dataObj, 0, 5, 20, DragDropEffects.Move, DragDropEffects.Move);
				Assert("isDragging should be false", !control.isDragging);
				control.AvailableColumnsListBox_MouseDown(null, null);
				Assert("isDragging should be true", control.isDragging);

				methodZListBox_DragOver.Invoke(control.IncludedColumnsListBox, new object[] { control.IncludedColumnsListBox.SelectedItems, drgevent });
				methodZListBox_DragDrop.Invoke(control.IncludedColumnsListBox, new object[] { control.IncludedColumnsListBox.SelectedItems, drgevent });

				control.IncludedColumnsListBox_MouseDown(null, null);
				Assert("isDragging should be false", !control.isDragging);
			}
		}

		public void TestPermanentlyDeleteRegistryItem()
		{
			var description = Guid.NewGuid().ToString();

			DocumentsDataRegistry.Instance.ReportColumnSettings.Set(description, Report.ColumnHeadingManager.ReportID, "test");
			AssertEquals("test", DocumentsDataRegistry.Instance.ReportColumnSettings.Get(description, report.ColumnHeadingManager.ReportID));

			DocumentsDataRegistry.Instance.ReportColumnSettings.Delete(description, Report.ColumnHeadingManager.ReportID);
			foreach (var registryItem in DocumentsDataRegistry.Instance.ReportColumnSettings.GetSavedColumnSettingKeysForAllCompanies(report.ColumnHeadingManager.ReportID))
			{
				Assert("Registry items should be permanently deleted", (registryItem.Description != description));
			}
		}

		public void TestSaveAndDeleteSetting()
		{
			var columnName = Guid.NewGuid().ToString();

			DocumentsDataRegistry.Instance.ReportColumnSettings.Set(columnName, Report.ColumnHeadingManager.ReportID, "");
			DocumentsDataRegistry.Instance.ReportColumnSettings.Delete(columnName, Report.ColumnHeadingManager.ReportID);

			using (var parentZForm = new ZForm())
			using (var control = GetNewControl())
			{
				parentZForm.Controls.Add(control);
				parentZForm.Show();
				AssertNull("Precondition: There should not be any messages.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				control.SettingsComboBox.SelectedIndex = 1;
				control.SaveButton.PerformClick();
				AssertEquals("A message should be shown.", "Do you want to save the current report settings for \"" + HeadingManager.CompanyDefaultConfigurationManager.ToString() + "\"?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("The company setting should not be saved.", DocumentsDataRegistry.Instance.ReportColumnSettings.Get("", HeadingManager.ReportID, GlbCompany.CurrentCompany.PK).Length == 0);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				control.SaveButton.PerformClick();
				AssertEquals("A message should be shown.", "Do you want to save the current report settings for \"" + HeadingManager.CompanyDefaultConfigurationManager.ToString() + "\"?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("The company setting should be saved.", DocumentsDataRegistry.Instance.ReportColumnSettings.Get("", HeadingManager.ReportID, GlbCompany.CurrentCompany.PK).Length > 0);
				AssertEquals("SettingsComboBox.SelectedIndex", 1, control.SettingsComboBox.SelectedIndex);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.CreateNewSettingForTest();
				CombinedConfigurationManager currentSetting = control.SettingsComboBox.SelectedItem as CombinedConfigurationManager;
				AssertNull("Precondition: There should not be any messages.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Current setting should be a pername setting", true, currentSetting != null);
				Assert("The setting should be saved.", DocumentsDataRegistry.Instance.ReportColumnSettings.Get("Save Me", HeadingManager.ReportID, ZGuid.Empty).Length > 0);
				int indexofNew = control.SettingsComboBox.SelectedIndex;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.SettingsComboBox.SelectedIndex = -1;
				control.DeleteButton.PerformClick();
				AssertEquals("A message should be shown.", "Please select a setting to delete.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("The company setting should not be deleted.", DocumentsDataRegistry.Instance.ReportColumnSettings.Get("", HeadingManager.ReportID, GlbCompany.CurrentCompany.PK).Length > 0);
				AssertEquals("The named setting should not be deleted.", true, DocumentsDataRegistry.Instance.ReportColumnSettings.Get("Save Me", HeadingManager.ReportID).Length > 0);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				control.SettingsComboBox.SelectedIndex = indexofNew;

				control.DeleteButton.PerformClick();
				AssertEquals("A message should be shown.", "Do you want to delete the current report settings under \"Save Me\"?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("The company setting should not be deleted.", DocumentsDataRegistry.Instance.ReportColumnSettings.Get("", HeadingManager.ReportID, GlbCompany.CurrentCompany.PK).Length > 0);
				AssertEquals("The named setting should not be deleted.", true, DocumentsDataRegistry.Instance.ReportColumnSettings.Get("Save Me", HeadingManager.ReportID).Length > 0);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				control.DeleteButton.PerformClick();
				AssertEquals("A message should be shown.", "Do you want to delete the current report settings under \"Save Me\"?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("The company setting should not be deleted.", DocumentsDataRegistry.Instance.ReportColumnSettings.Get("", HeadingManager.ReportID, GlbCompany.CurrentCompany.PK).Length > 0);
				AssertEquals("The named setting should be deleted.", 0, DocumentsDataRegistry.Instance.ReportColumnSettings.Get("Save Me", HeadingManager.ReportID).Length);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				control.SettingsComboBox.SelectedIndex = 1;
				control.DeleteButton.PerformClick();
				AssertEquals("A message should be shown.", "Do you want to delete the current report settings under \"" + HeadingManager.CompanyDefaultConfigurationManager.ToString() + "\"?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("The company setting should be deleted.", DocumentsDataRegistry.Instance.ReportColumnSettings.Get("", HeadingManager.ReportID, GlbCompany.CurrentCompany.PK).Length == 0);

				for (int i = 0; i < control.SettingsComboBox.Items.Count; i++)
				{
					control.SettingsComboBox.SelectedIndex = i;
					var configName = control.SettingsComboBox.SelectedItem as ColumnConfigurationManager;
					Assert("Deleted configurations should not appear in configuration list", (!configName.Description.Equals(columnName)));
				}
			}
		}

		public void TestConfigPopulateForTemplateWithMultipleWorksheets()
		{
			using (var report = new Report(pack, MultipleTemplatesWithOptionalColumns, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				using (var arrangementControl = new ColumnArrangementUserControl(report))
				{
					arrangementControl.SettingsComboBox.SelectedIndex = 0;

					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].Hidden = true;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[1].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[2].Hidden = false;

					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet2"].ColumnHeadings[0].Hidden = true;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet2"].ColumnHeadings[1].Hidden = true;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet2"].ColumnHeadings[2].Hidden = true;

					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet3"].ColumnHeadings[0].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet3"].ColumnHeadings[1].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet3"].ColumnHeadings[2].Hidden = false;
					report.ColumnHeadingManager.FireOnColumnConfigurationLoaded();

					AssertEquals("Worksheet combo on Configuration Management form should have 3 values", 3, arrangementControl.comboBoxConfigWorksheet.Items.Count);
					AssertEquals("First item in Worksheet combo on Configuration Management should be sheet1", "Sheet1", arrangementControl.comboBoxConfigWorksheet.Items[0].ToString());
					AssertEquals("Second item in Worksheet combo on Configuration Management should be sheet2", "Sheet2", arrangementControl.comboBoxConfigWorksheet.Items[1].ToString());
					AssertEquals("Third item in Worksheet combo on Configuration Management should be sheet3", "Sheet3", arrangementControl.comboBoxConfigWorksheet.Items[2].ToString());

					arrangementControl.comboBoxConfigWorksheet.SelectedIndex = 0;

					AssertEquals("Available columns should contain 1 item", 1, arrangementControl.AvailableColumnsListBox.Items.Count);
					AssertEquals("Available columns should contain 'Sheet 1 Column 1'", "Sheet 1 Column 1", arrangementControl.AvailableColumnsListBox.Items[0].ToString());
					AssertEquals("Included columns should contain 2 items", 2, arrangementControl.IncludedColumnsListBox.Items.Count);
					AssertEquals("Included columns should contain 'Sheet 1 Column 2'", "Sheet 1 Column 2", arrangementControl.IncludedColumnsListBox.Items[0].ToString());
					AssertEquals("Included columns should contain 'Sheet 1 Column 3'", "Sheet 1 Column 3", arrangementControl.IncludedColumnsListBox.Items[1].ToString());

					arrangementControl.comboBoxConfigWorksheet.SelectedIndex = 1;
					AssertEquals("Available columns should contain 3 items", 3, arrangementControl.AvailableColumnsListBox.Items.Count);
					AssertEquals("Available columns should contain 'Sheet 2 Column 1'", "Sheet 2 Column 1", arrangementControl.AvailableColumnsListBox.Items[0].ToString());
					AssertEquals("Available columns should contain 'Sheet 2 Column 2'", "Sheet 2 Column 2", arrangementControl.AvailableColumnsListBox.Items[1].ToString());
					AssertEquals("Available columns should contain 'Sheet 2 Column 3'", "Sheet 2 Column 3", arrangementControl.AvailableColumnsListBox.Items[2].ToString());
					AssertEquals("Included columns should contain nothing", 0, arrangementControl.IncludedColumnsListBox.Items.Count);

					arrangementControl.comboBoxConfigWorksheet.SelectedIndex = 2;
					AssertEquals("Available columns should contain nothing", 0, arrangementControl.AvailableColumnsListBox.Items.Count);
					AssertEquals("Incldued columns should contain 3 items", 3, arrangementControl.IncludedColumnsListBox.Items.Count);
					AssertEquals("Incldued columns should contain 'Sheet 3 Column 1'", "Sheet 3 Column 1", arrangementControl.IncludedColumnsListBox.Items[0].ToString());
					AssertEquals("Incldued columns should contain 'Sheet 3 Column 2'", "Sheet 3 Column 2", arrangementControl.IncludedColumnsListBox.Items[1].ToString());
					AssertEquals("Incldued columns should contain 'Sheet 3 Column 3'", "Sheet 3 Column 3", arrangementControl.IncludedColumnsListBox.Items[2].ToString());
				}
			}
		}

		public void TestPropertiesPopulateForTempalteWithMultipleWorksheests()
		{
			using (var report = new Report(pack, MultipleTemplatesWithOptionalColumns, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				using (var arrangementControl = new ColumnArrangementUserControl(report))
				{
					arrangementControl.SettingsComboBox.SelectedIndex = 0;

					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0].Hidden = true;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[1].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[2].Hidden = false;

					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet2"].ColumnHeadings[0].Hidden = true;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet2"].ColumnHeadings[1].Hidden = true;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet2"].ColumnHeadings[2].Hidden = true;

					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet3"].ColumnHeadings[0].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet3"].ColumnHeadings[1].Hidden = false;
					report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet3"].ColumnHeadings[2].Hidden = false;
					report.ColumnHeadingManager.FireOnColumnConfigurationLoaded();

					AssertEquals("selected columns should contain 2 items", 2, arrangementControl.SelectedColumnslistBox.Items.Count);
					AssertEquals("selected columns should contain 'Sheet 1 Column 2'", "Sheet 1 Column 2", arrangementControl.SelectedColumnslistBox.Items[0].ToString());
					AssertEquals("selected columns should contain 'Sheet 1 Column 3'", "Sheet 1 Column 3", arrangementControl.SelectedColumnslistBox.Items[1].ToString());

					arrangementControl.comboBoxConfigWorksheet.SelectedIndex = 1;
					AssertEquals("selected columns should contain nothing", 0, arrangementControl.SelectedColumnslistBox.Items.Count);

					arrangementControl.comboBoxConfigWorksheet.SelectedIndex = 2;
					AssertEquals("selected columns should contain 3 items", 3, arrangementControl.SelectedColumnslistBox.Items.Count);
					AssertEquals("selected columns should contain 'Sheet 3 Column 1'", "Sheet 3 Column 1", arrangementControl.SelectedColumnslistBox.Items[0].ToString());
					AssertEquals("selected columns should contain 'Sheet 3 Column 2'", "Sheet 3 Column 2", arrangementControl.SelectedColumnslistBox.Items[1].ToString());
					AssertEquals("selected columns should contain 'Sheet 3 Column 3'", "Sheet 3 Column 3", arrangementControl.SelectedColumnslistBox.Items[2].ToString());
				}
			}
		}

		public void TestOptionsPopulateForTemplateWithMultipleWorksheets()
		{
			using (var report = new Report(pack, MultipleTemplatesWithOptionalColumns, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				using (var zform = new ZForm())
				{
					var arrangementControl = new ColumnArrangementUserControl(report);
					zform.Controls.Add(arrangementControl);
					zform.Show();
					arrangementControl.tabColumnConfig.SelectTab("OptionTabPage");      // ensure ReportTitleTextBox.Handle is created

					AssertEquals("Worksheet combo on Configuration Management form should have 3 values", 3, arrangementControl.comboBoxConfigWorksheet.Items.Count);

					arrangementControl.comboBoxConfigWorksheet.SelectedIndex = 0;
					AssertEquals("The report title of the first sheet should be set correctly", "SHEET NUMBER 1", arrangementControl.ReportTitleTextBox.Text);

					arrangementControl.comboBoxConfigWorksheet.SelectedIndex = 1;
					AssertEquals("The report title of the second sheet should be set correctly", "SHEET NUMBER 2", arrangementControl.ReportTitleTextBox.Text);

					arrangementControl.comboBoxConfigWorksheet.SelectedIndex = 2;
					AssertEquals("The report title of the third sheet should be empty", string.Empty, arrangementControl.ReportTitleTextBox.Text);
				}
			}
		}

		public void TestStartupState()
		{
			using (var control = GetNewControl())
			{
				AssertEquals("AvailableColumnsListBox.Items.Count", 2, control.AvailableColumnsListBox.Items.Count);
				AssertEquals("AvailableColumnsListBox.SelectedIndex", -1, control.AvailableColumnsListBox.SelectedIndex);
				control.SettingsComboBox.SelectedIndex = 0;
				AssertEquals("Should be column 1", "1", ((ColumnHeading)control.AvailableColumnsListBox.Items[0]).DisplayLabel);
				AssertEquals("Should be column 2", "2", ((ColumnHeading)control.AvailableColumnsListBox.Items[1]).DisplayLabel);

				AssertEquals("IncludedColumnsListBox.Items.Count", 2, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("IncludedColumnsListBox.SelectedIndex", 0, control.IncludedColumnsListBox.SelectedIndex);
				AssertEquals("Should be column 4", "4", ((ColumnHeading)control.IncludedColumnsListBox.Items[0]).DisplayLabel);
				AssertEquals("Should be column 3", "3", ((ColumnHeading)control.IncludedColumnsListBox.Items[1]).DisplayLabel);

				AssertEquals("SettingsComboBox.Items.Count", 2, control.SettingsComboBox.Items.Count);
				AssertEquals("SettingsComboBox.Items[0]", HeadingManager.DefaultTemplateConfigurationManager, control.SettingsComboBox.Items[0]);
				AssertEquals("SettingsComboBox.Items[1]", HeadingManager.CompanyDefaultConfigurationManager, control.SettingsComboBox.Items[1]);
				AssertEquals("SettingsComboBox.SelectedIndex", 0, control.SettingsComboBox.SelectedIndex);
				AssertEquals("SettingsComboBox.Text", HeadingManager.DefaultTemplateConfigurationManager.ToString(), control.SettingsComboBox.Text);
			}
		}

		public void TestHideOptionTab()
		{
			HeadingManager.IsReportTitleChangeable = false;
			using (ColumnArrangementUserControl control = GetNewControl())
			{
				AssertEquals("The option tab should not be displayed", 2, control.tabColumnConfig.TabPages.Count);
			}
			HeadingManager.IsReportTitleChangeable = true;
			using (ColumnArrangementUserControl control = GetNewControl())
			{
				AssertEquals("The option tab should be displayed", 3, control.tabColumnConfig.TabPages.Count);
			}
		}

		public void TestColumnConfigurationPanelHidesWhenThereAreNoCustomisableColumns()
		{
			using (var reportInside = new Report(pack, ThreeFilters, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				reportInside.PrepareForRender();
				using (var arrangementControl = new ColumnArrangementUserControl(reportInside))
				{
					AssertEquals("Column configuration control should hide", false, arrangementControl.ColumnConfigurationPanel.Visible);
					AssertEquals("If column configuration control is hidden, the size of control should be reduced", arrangementControl.SettingsGroupBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(3), arrangementControl.Width);
					AssertEquals("If column configuration control is hidden, the size of control should be reduced", arrangementControl.SettingsGroupBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiX(3), arrangementControl.Height);
				}
			}
		}

		public void TestFiltersAreLoadedFromSavedConfigurationsForScheduledReports()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.TextFilter.xls", "TextFilter.xls");
			var textFilterTemplate = new ExcelTemplateForUnitTesting("TextFilter.xls", Path.GetFullPath(tempFileName));
			using (var report = new Report(pack, textFilterTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				report.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
				report.PrepareForRender();
				((TextField)report.FilterCollection[1]).Value = "TESTVALUE";
				report.ColumnHeadingManager.CompanyDefaultConfigurationManager.Save(report.FilterCollection, string.Empty, string.Empty, string.Empty, string.Empty);
				report.FilterCollection.ClearValues();

				using (var arrangementControl = new ColumnArrangementUserControl(report))
				{
					arrangementControl.SettingsComboBox.SelectedIndex = 0;
					arrangementControl.SettingsComboBox.SelectedIndex = 1;
					AssertEquals("Filter values should have loaded", "TESTVALUE", ((TextField)report.FilterCollection[1]).Value);
				}
			}
		}

		DocumentPack pack;

		protected override void SetUp()
		{
			base.SetUp();
			pack = new DocumentPack(Factory.New<StmMenuItem>());
		}

		protected override void TearDown()
		{
			base.TearDown();
			report?.Dispose();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new ZEmptyFormForBasherTest();
			form.CaptionRenderingEnabled = true;
			form.Height = 1000;
			form.Width = 1000;
			form.Controls.Add(GetNewControl());
			return form;
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(PrintTaskTest).Assembly));

		Report report;
		Report Report
		{
			get
			{
				if (report == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					report = new MockReport(new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName)));
					((MockReport)report).SetColumnHeadingManager(HeadingManager);
					report.PrepareForRender();
				}
				return report;
			}
		}

		ExcelTemplateForUnitTesting multipleTemplatesWithOptionalColumns;
		ExcelTemplateForUnitTesting MultipleTemplatesWithOptionalColumns
		{
			get
			{
				if (multipleTemplatesWithOptionalColumns == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.MultipleTemplatesWithOptionalColumns.xls", "MultipleTemplatesWithOptionalColumns.xls");
					multipleTemplatesWithOptionalColumns = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalColumns.xls", Path.GetFullPath(tempFileName));
				}
				return multipleTemplatesWithOptionalColumns;
			}
		}

		ExcelTemplateForUnitTesting threeFilters;
		ExcelTemplateForUnitTesting ThreeFilters
		{
			get
			{
				if (threeFilters == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.ThreeFilters.xls", "ThreeFilters.xls");
					threeFilters = new ExcelTemplateForUnitTesting("ThreeFilters.xls", Path.GetFullPath(tempFileName));
				}
				return threeFilters;
			}
		}

		ColumnConfigurationsManager HeadingManager
		{
			get
			{
				if (headingManager == null)
				{
					headingManager = new ColumnConfigurationsManager(ZGuid.NewZGuid(), false);
					var linkedField = new LookupField(Factory);
					linkedField.DisplayName = "Test";
					linkedField.SetCollectionProvider(new RefUNLOCOCollectionProvider(Factory));
					headingManager.AddLinkedField(linkedField);
					headingManager.SaveToFilterField = "Test";
					headingManager.IsReportTitleChangeable = true;
					headingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("2", "2", "2", 1, 4, 1, true));
					headingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("1", "1", "1", 2, 3, 2, true));
					headingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("3", "3", "3", 3, 2, 3, false));
					headingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("4", "4", "4", 4, 1, 4, false));
					headingManager.DefaultTemplateConfigurationManager.SetTitle("Sheet1", "Title1");
					headingManager.DefaultTemplateConfigurationManager.Load(Report.FilterCollection, Report.GroupByCollection, Report.SortOrderCollection, Report.OrientationManager, Report.Parent);
				}
				return headingManager;
			}
		}
		ColumnConfigurationsManager headingManager;

		ColumnArrangementUserControlForTest GetNewControl() => new ColumnArrangementUserControlForTest(Report);

		const string ElementNameRulesTest = "Element name is subject to the following rules:\r\n\tCase-sensitive.\r\n\tMust start with a letter or underscore.\r\n\tCan contain letters, digits, hyphens, underscores, and periods.\r\n\tCannot contain spaces.";
	}
}
