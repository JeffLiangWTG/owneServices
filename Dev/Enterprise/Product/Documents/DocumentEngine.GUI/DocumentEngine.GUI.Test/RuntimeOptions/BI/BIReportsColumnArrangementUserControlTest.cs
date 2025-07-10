using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
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
	sealed class BIReportsColumnArrangementUserControlTest : ZFormBasherTest
	{
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

				control.tabColumnConfig.SelectedIndex = 0;
				control.AvailableColumnsListBox.SelectedIndex = 0;
				control.AddButton.PerformClick();
				AssertEquals("Prerequisite: IncludedColumnsListBox.Items.Count", 3, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("Prerequisite: IncludedColumnsListBox.SelectedIndex", 2, control.IncludedColumnsListBox.SelectedIndex);
				control.tabColumnConfig.SelectedIndex = 1;

				control.tabColumnConfig.SelectedIndex = 0;
				control.MoveUpButton.PerformClick();
				AssertEquals("Prerequisite: IncludedColumnsListBox.Items.Count", 3, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("Prerequisite: IncludedColumnsListBox.SelectedIndex", 1, control.IncludedColumnsListBox.SelectedIndex);
				control.tabColumnConfig.SelectedIndex = 1;

				control.tabColumnConfig.SelectedIndex = 0;
				control.MoveDownButton.PerformClick();
				AssertEquals("Prerequisite: IncludedColumnsListBox.Items.Count", 3, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("Prerequisite: IncludedColumnsListBox.SelectedIndex", 2, control.IncludedColumnsListBox.SelectedIndex);
				control.tabColumnConfig.SelectedIndex = 1;

				control.tabColumnConfig.SelectedIndex = 0;
				control.RemoveButton.PerformClick();
				AssertEquals("Prerequisite: IncludedColumnsListBox.Items.Count", 2, control.IncludedColumnsListBox.Items.Count);
				AssertEquals("Prerequisite: IncludedColumnsListBox.SelectedIndex", 1, control.IncludedColumnsListBox.SelectedIndex);
				control.tabColumnConfig.SelectedIndex = 1;
			}
		}

		[GuiTest]
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

		public void TestMoveColumnUpAndDown()
		{
			using (var form = new ZForm())
			using (var control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

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

		public void TestPermanentlyDeleteRegistryItem()
		{
			var description = Guid.NewGuid().ToString();

			DocumentsDataRegistry.Instance.ReportColumnSettings.Set(description, Report.ColumnHeadingManager.ReportID, "test");
			AssertEquals("test", (DocumentsDataRegistry.Instance.ReportColumnSettings.Get(description, report.ColumnHeadingManager.ReportID)));

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
				var currentSetting = control.SettingsComboBox.SelectedItem as CombinedConfigurationManager;
				AssertNull("Precondition: There should not be any messages.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Current setting should be a pername setting", true, currentSetting != null);
				Assert("The setting should be saved.", DocumentsDataRegistry.Instance.ReportColumnSettings.Get("Save Me", HeadingManager.ReportID, ZGuid.Empty).Length > 0);
				var indexofNew = control.SettingsComboBox.SelectedIndex;

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

				for (var i = 0; i < control.SettingsComboBox.Items.Count; i++)
				{
					control.SettingsComboBox.SelectedIndex = i;
					var configName = control.SettingsComboBox.SelectedItem as ColumnConfigurationManager;
					Assert("Deleted configurations should not appear in configuration list", (!configName.Description.Equals(columnName)));
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

		public void TestColumnConfigurationPanelHidesWhenThereAreNoCustomisableColumns()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.ThreeFilters.xls", "ThreeFilters.xls");
			var excelTemplate = new ExcelTemplateForUnitTesting("ThreeFilters.xls", Path.GetFullPath(tempFileName));
			using (var reportInside = new Report(pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				reportInside.PrepareForRender();
				using (var arrangementControl = new BIReportsColumnArrangementUserControl(reportInside))
				{
					AssertEquals("Column configuration control should hide", false, arrangementControl.ColumnConfigurationPanel.Visible);
					AssertEquals("If column configuration control is hidden, the size of control should be reduced", arrangementControl.SettingsGroupBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(3), arrangementControl.Width);
					AssertEquals("If column configuration control is hidden, the size of control should be reduced", arrangementControl.SettingsGroupBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiX(3), arrangementControl.Height);
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

		Report Report
		{
			get
			{
				if (report == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					report = new MockReport(new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName)));
					((MockReport)report).SetColumnHeadingManager(HeadingManager);
				}
				return report;
			}
		}
		Report report;

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

		BIReportsColumnArrangementUserControlForTest GetNewControl() => new BIReportsColumnArrangementUserControlForTest(Report);
	}
}
