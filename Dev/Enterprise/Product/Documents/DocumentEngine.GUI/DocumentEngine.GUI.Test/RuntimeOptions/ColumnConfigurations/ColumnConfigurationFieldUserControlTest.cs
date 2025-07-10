using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(ColumnConfigurationFieldUserControl))]
	sealed class ColumnConfigurationFieldUserControlTest : RuntimeOptionUserControlBaseTest<ColumnConfigurationFieldUserControl>
	{
		public void TestSelectSettingThatReferencesWorkSheetThatNoLongerExistsNotifiesUser()
		{
			using (ColumnConfigurationFieldUserControl control = new ColumnConfigurationFieldUserControl())
			{
				control.SetFilter(ListeningField);

				CombinedConfigurationManager oldConfigurationManager = new CombinedConfigurationManager(Manager, "Old Configuration");
				Worksheet oldWorkSheet = oldConfigurationManager.HeadingManager.CurrentConfiguration.Worksheets.AddNew("Old Sheet", "Old Sheet Title");
				oldWorkSheet.ColumnHeadings.Add(new ColumnHeading("Column1", "Description", "Heading", 10, 10, 100, false));
				oldConfigurationManager.Save();

				AssertEquals("should be 3 items", 3, control.comboSettings.Items.Count);
				AssertEquals("First should be template default", Manager.DefaultTemplateConfigurationManager, control.comboSettings.Items[0]);
				AssertEquals("Second should be company default", Manager.CompanyDefaultConfigurationManager, control.comboSettings.Items[1]);
				AssertEquals("Third should be combined", oldConfigurationManager, control.comboSettings.Items[2]);

				control.comboSettings.SelectedIndex = 2;
				AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text"
					, @"The Configuration you have selected has Column Configurations in it for a Worksheet [Old Sheet] that no longer exists.

This can happen when the design of the report has been changed since the time that the Configuration you are trying to use was last saved.

These settings have been ignored, please check your Configuration settings then Save them to remove the redundant settings from your Configuration.

Report details:
- Menu Name: BestGoalieEver
- Business Context: Kelvin's Context
- Worksheet Name: Old Sheet"
					, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAllSavedSettingsForAReportAreInTheDropDown()
		{
			using (ColumnConfigurationFieldUserControl ctrl = new ColumnConfigurationFieldUserControl())
			{
				ctrl.SetFilter(ListeningField);

				CombinedConfigurationManager combined = new CombinedConfigurationManager(Manager, "Ho ho");
				combined.Save();
				AssertNotEquals(ctrl.comboSettings.Width, ctrl.comboSettings.DropDownWidth);
				Assert(ctrl.comboSettings.Width < ctrl.comboSettings.DropDownWidth);
				AssertEquals("should be 3 items", 3, ctrl.comboSettings.Items.Count);
				AssertEquals("First should be template default", Manager.DefaultTemplateConfigurationManager, ctrl.comboSettings.Items[0]);
				AssertEquals("Second should be company default", Manager.CompanyDefaultConfigurationManager, ctrl.comboSettings.Items[1]);
				AssertEquals("Third should be combined", combined, ctrl.comboSettings.Items[2]);

				combined.Delete();
				AssertEquals("should be 2 items", 2, ctrl.comboSettings.Items.Count);
				AssertEquals("Second should be template default", Manager.DefaultTemplateConfigurationManager, ctrl.comboSettings.Items[0]);
				AssertEquals("Third should be company default", Manager.CompanyDefaultConfigurationManager, ctrl.comboSettings.Items[1]);
			}
		}

		public void TestSetFilterSelectsCurrentItemInComboBox()
		{
			using (ColumnConfigurationFieldUserControl ctrl = new ColumnConfigurationFieldUserControl())
			{
				ListeningField.Value = Manager.CompanyDefaultConfigurationManager;
				ctrl.SetFilter(ListeningField);

				AssertEquals("should be 2 items", 2, ctrl.comboSettings.Items.Count);
				AssertEquals("First should be template default", Manager.DefaultTemplateConfigurationManager, ctrl.comboSettings.Items[0]);
				AssertEquals("Second should be company default", Manager.CompanyDefaultConfigurationManager, ctrl.comboSettings.Items[1]);
				AssertEquals("CompanyDefaultConfigurationManager should be seleted in combo", Manager.CompanyDefaultConfigurationManager, ctrl.comboSettings.SelectedItem);
			}
		}

		public void TestSetFilterSelectsZeroSelectedIndexInComboBox()
		{
			using (var ctrl = new ColumnConfigurationFieldUserControl())
			{
				ctrl.SetFilter(ListeningField);

				AssertEquals("should be 2 items", 2, ctrl.comboSettings.Items.Count);
				AssertEquals("First should be template default", Manager.DefaultTemplateConfigurationManager, ctrl.comboSettings.Items[0]);
				AssertEquals("Second should be company default", Manager.CompanyDefaultConfigurationManager, ctrl.comboSettings.Items[1]);
				AssertEquals("DefaultTemplateConfigurationManager should be seleted in combo which would be zero selected index", 0, ctrl.comboSettings.SelectedIndex);
			}
		}

		ColumnConfigurationField ListeningField
		{
			get
			{
				if (listeningField == null)
				{
					listeningField = new ColumnConfigurationField(Factory);
					((IColumnHeadingManagerListener)listeningField).SetManager(Manager);
				}
				return listeningField;
			}
		}
		ColumnConfigurationField listeningField;

		ColumnConfigurationsManager Manager
		{
			get
			{
				return manager ?? (manager = new ColumnConfigurationsManager(MenuItem.PK, false));
			}
		}
		ColumnConfigurationsManager manager;

		StmMenuItem MenuItem
		{
			get
			{
				if (menuItem == null)
				{
					menuItem = Factory.New<StmMenuItem>();
					menuItem.SU_BusinessContext = "Kelvin's Context";
					menuItem.SU_MenuName = "BestGoalieEver";
					Factory.Save();
				}
				return menuItem;
			}
		}
		StmMenuItem menuItem;
	}
}
