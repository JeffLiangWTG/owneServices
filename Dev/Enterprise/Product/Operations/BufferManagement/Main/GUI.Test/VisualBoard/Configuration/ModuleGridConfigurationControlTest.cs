using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ModuleGridConfigurationControlTest : BMSTestCaseWithFactory
	{
		public void TestModuleGridConfigurationControl_SelectModuleDropEdit_SelectFilterLayoutGuidFindBox_ShouldHaveSameWidth()
		{
			var testConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "ORG");
			var board = testConfig.BucketBoard;
			board.Sections.DeleteAll();
			var section = board.Sections.AddNew();
			section.MS_SectionType = BMConstants.ModuleGridSectionType;

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();

				var control = form.Find(c => c is ModuleGridConfigurationControl).Cast<ModuleGridConfigurationControl>().SingleOrDefault();
				AssertNotNull(control);

				var findboxControl = control.FindSingle<ZArchitecture.ZGrid>().Controls.Find("FilterLayout", false).FirstOrDefault() as DataGridTextBox;
				AssertNotNull(findboxControl);

				var moduleDropEditControl = control.FindSingle<ZArchitecture.ZGrid>().Controls.Find("ModuleName", false).FirstOrDefault() as DataGridTextBox;
				AssertNotNull(moduleDropEditControl);

				AssertEquals(findboxControl.Width, moduleDropEditControl.Width);
			}
		}

		public void TestFilterLayoutFindBox_ShouldShowUnpublishedLayouts_WhenTheirNamesDoNotCollideWithPublishedLayouts()
		{
			var publishedFilter = Factory.New<StmModuleFilter>();
			publishedFilter.S9_ModuleID = ModuleIDs.WorkItem.Name;
			publishedFilter.S9_IsPublished = true;
			publishedFilter.S9_FilterName = "Alpha";

			var unpublishedFilter = Factory.New<StmModuleFilter>();
			unpublishedFilter.S9_ModuleID = ModuleIDs.WorkItem.Name;
			unpublishedFilter.S9_IsPublished = false;
			unpublishedFilter.S9_FilterName = "Beta";

			var testConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "ORG");
			var board = testConfig.BucketBoard;
			board.Sections.DeleteAll();
			var section = board.Sections.AddNew();
			section.MS_SectionType = BMConstants.ModuleGridSectionType;

			var config = (ModuleGridSectionConfiguration)section.Configuration;
			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			config.PanelConfigurations.Add(panelConfig);
			panelConfig.ModuleName = ModuleIDs.WorkItem.Name;

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();

				var control = form.Find(c => c is ModuleGridConfigurationControl).Cast<ModuleGridConfigurationControl>().SingleOrDefault();
				AssertNotNull(control);

				var filterControlSet = form.FindAll<ZGuidFindBox>().ToArray();
				var findboxControl = control.FindSingle<ZArchitecture.ZGrid>().Controls.Find("FilterLayout", false).FirstOrDefault() as DataGridTextBox;

				AssertNotNull(findboxControl);

				BusinessObject[] result = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown((o) =>
				{
					var popupForm = o as EmbeddedModulePopup;
					if (popupForm != null)
					{
						var toolStrip = popupForm.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip");
						var previewButton = toolStrip.Items["ToolStripPreviewDropButton"];

						result = popupForm.Module_ForTest.GridCollection.ToArray();

						AssertCollectionContains(publishedFilter.PK, result.Select(x => x.PK));
						AssertCollectionContains(unpublishedFilter.PK, result.Select(x => x.PK));
					}
				});

				ZFormModaliser.Show(new EmbeddedModulePopup(), form);
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
				config.Validation.ValidateAll();
				AssertNoNotifications(panelConfig.FilterLayoutInfo);
			}
		}

		public void TestFilterLayoutFindBox_ShouldNotShowUnpublishedLayouts_WhenTheirNamesCollideWithPublishedLayouts()
		{
			var publishedFilter = Factory.New<StmModuleFilter>();
			publishedFilter.S9_ModuleID = ModuleIDs.WorkItem.Name;
			publishedFilter.S9_IsPublished = true;
			publishedFilter.S9_FilterName = "Alpha";

			var unpublishedFilter = Factory.New<StmModuleFilter>();
			unpublishedFilter.S9_ModuleID = ModuleIDs.WorkItem.Name;
			unpublishedFilter.S9_IsPublished = false;
			unpublishedFilter.S9_FilterName = "Alpha";

			var testConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "ORG");
			var board = testConfig.BucketBoard;
			board.Sections.DeleteAll();
			var section = board.Sections.AddNew();
			section.MS_SectionType = BMConstants.ModuleGridSectionType;

			var config = (ModuleGridSectionConfiguration)section.Configuration;
			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			config.PanelConfigurations.Add(panelConfig);
			panelConfig.ModuleName = ModuleIDs.WorkItem.Name;

			Factory.Save();

			using (var form = new BMBoardForm(board))
			{
				form.Show();
				Application.DoEvents();

				var control = form.Find(c => c is ModuleGridConfigurationControl).Cast<ModuleGridConfigurationControl>().SingleOrDefault();
				AssertNotNull(control);

				var filterControlSet = form.FindAll<ZGuidFindBox>().ToArray();
				var findboxControl = control.FindSingle<ZArchitecture.ZGrid>().Controls.Find("FilterLayout", false).FirstOrDefault() as DataGridTextBox;

				AssertNotNull(findboxControl);

				BusinessObject[] result = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown((o) =>
				{
					var popupForm = o as EmbeddedModulePopup;
					if (popupForm != null)
					{
						var toolStrip = popupForm.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip");
						var previewButton = toolStrip.Items["ToolStripPreviewDropButton"];

						result = popupForm.Module_ForTest.GridCollection.ToArray();

						AssertCollectionContains(publishedFilter.PK, result.Select(x => x.PK));
						AssertCollectionNotContains(unpublishedFilter.PK, result.Select(x => x.PK));
					}
				});

				ZFormModaliser.Show(new EmbeddedModulePopup(), form);
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
				config.Validation.ValidateAll();
				AssertNoNotifications(panelConfig.FilterLayoutInfo);
			}
		}
	}
}
