using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.GUI.ReflectiveFieldMap;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	[TestedType(typeof(VisualizerMenuCustomisationForm))]
	sealed class VisualizerMenuCustomisationFormTest : ZFormBasherTest
	{
		public void TestDisplayVisualizerMenuDetailsTab()
		{
			var menuCustomisation = new VisualizerMenuCustomisation(
				new DummyNonPersistentBusinessObject(),
				new BusinessObjectFactory(),
				"Hello");

			using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
			{
				form.Show();

				Assert("Visualizer menu details tab is display", form.VisualizerMenuDetailsTab.TabVisible);
			}
		}

		public void TestUnusedControlsAreHidden()
		{
			var menuCustomisation = new VisualizerMenuCustomisation(
				new DummyNonPersistentBusinessObject(),
				new BusinessObjectFactory(),
				"Hello");

			using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
			{
				form.Show();

				Assert("Menu details tab is hidden", !form.MenuDetailsTab.TabVisible);

				var isClientSpecificinfo = form.AvailableTemplatesGrid.ColumnStyles.Cast<ZGridColumnInfo>().
					FirstOrDefault(info => info.ColumnName == StmTemplateSchema.Constants.SO_IsClientSpecific);

				AssertNull("IsClientSpecific column is hidden", isClientSpecificinfo);
			}
		}

		public void TestShowFilterEvaluator()
		{
			var menuCustomisation = new VisualizerMenuCustomisation(
				new DummyNonPersistentBusinessObject(),
				new BusinessObjectFactory(),
				"Hello");

			using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
			{
				form.Show();

				var showFilterMenuItem = form.MenusGrid.ContextMenu.MenuItems.FindByText("Show Macro Evaluator");

				AssertNotNull("Show Macro Evaluator menu item", showFilterMenuItem);
			}
		}

		[ExpectNoExceptions]
		public void TestShowDataSourceMapForContainerLoadPlanTemplate()
		{
			var factory = new BusinessObjectFactory();
			var consol = (BusinessObject)factory.New<Forwarding.IForwardingConsol>();
			var menuCustomisation = new VisualizerMenuCustomisation(consol, factory, "Hello");

			var containerLoadPlanTemplate = menuCustomisation.AvailableTemplates.OfType<VisualizerTemplate>()
				.FirstOrDefault(t => t.SO_Name == "ContainerLoadPlan");
			AssertNotNull(containerLoadPlanTemplate);

			using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
			{
				form.Show();
				form.SelectTemplateTab();
				form.AvailableTemplatesGrid.SelectSingleElement(containerLoadPlanTemplate);

				var dataSourceMapMenuItem = form.AvailableTemplatesGrid.ContextMenu.MenuItems.FindByText("Data Source Map");
				AssertNotNull("Data Source Map menu item", dataSourceMapMenuItem);

				dataSourceMapMenuItem.PerformClick();

				var mapTreeForm = Application.OpenForms.OfType<MapTreeForm>().FirstOrDefault();
				AssertNotNull(mapTreeForm);
				mapTreeForm.Dispose();
			}
		}

		public void TestShowRestrictionColumnsInGrid()
		{
			var menuCustomisation = new VisualizerMenuCustomisation(
				new DummyNonPersistentBusinessObject(),
				new BusinessObjectFactory(),
				"Hello");

			using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
			{
				form.Show();

				var grid = form.MenusGrid;
				foreach (var columnName in new[]
				{
					StmMenuItemSchema.Constants.SU_DeliveryRestrictionType,
					StmMenuItemSchema.Constants.SU_DeliveryRestrictionMacro,
					StmMenuItemSchema.Constants.SU_DeliveryRestrictionDescription
				})
				{
					var column = grid.Columns.FirstOrDefault(c => c.ColumnName == columnName);
					AssertNotNull(column);
					Assert(column.IsVisible);
				}
			}
		}

		public void TestShowRestrictionControls()
		{
			var menuCustomisation = new VisualizerMenuCustomisation(
				new DummyNonPersistentBusinessObject(),
				new BusinessObjectFactory(),
				"Hello");

			using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
			{
				form.Show();

				foreach (var controlName in new[]
				{
					"visualizerDeliveryRestrictionDropEdit",
					"visualizerDeliveryRestrictionConditionTextBox",
					"visualizerDeliveryRestrictionDescriptionTextBox"
				})
				{
					var control = form.VisualizerMenuDetailsTab.Controls.Find(controlName, false).FirstOrDefault();
					AssertNotNull(control);
					Assert(control.Visible);
				}
			}
		}

		public void TestRunFormClick_WithFilterSatisfied()
		{
			var oldIsDeveloperValue = GlbStaff.CurrentUser.GS_IsDeveloper;

			try
			{
				var factory = new BusinessObjectFactory();
				var consol = factory.New<Forwarding.IForwardingConsol>();

				var menuCustomisation = new VisualizerMenuCustomisation(
								(BusinessObject)consol,
								factory,
								"Hello");

				var containerLoadPlanTemplate = menuCustomisation.AvailableTemplates.OfType<VisualizerTemplate>()
				.FirstOrDefault(t => t.SO_Name == "ContainerLoadPlan");
				AssertNotNull(containerLoadPlanTemplate);

				SetupMenuItemWithOneTemplateCheckedOut(menuCustomisation);

				GlbStaff.CurrentUser.GS_IsDeveloper = true;
				using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
				{
					form.Show();

					var runMenuItem = form.MenusGrid.ContextMenu.MenuItems.FindByText("Run");

					form.CurrentMenuExposed.SU_FilterList = "JK_TransportMode == \"AAA\"";

					consol.JK_TransportMode = "AAA";
					Factory.Save();
					factory.Save();

					runMenuItem.PerformClick();

					CombineAssertions(() =>
					{
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						AssertType("visualizer form has been shown", typeof(DocumentVisualizerForm), ZFormModaliser.LastFormShownDialogForTest);
					});
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsDeveloper = oldIsDeveloperValue;
			}
		}

		public void TestRunFormClick_WithFilterUnsatisfied()
		{
			var oldIsDeveloperValue = GlbStaff.CurrentUser.GS_IsDeveloper;

			try
			{
				var factory = new BusinessObjectFactory();
				var consol = factory.New<Forwarding.IForwardingConsol>();

				var menuCustomisation = new VisualizerMenuCustomisation(
								(BusinessObject)consol,
								factory,
								"Hello");

				var containerLoadPlanTemplate = menuCustomisation.AvailableTemplates.OfType<VisualizerTemplate>()
				.FirstOrDefault(t => t.SO_Name == "ContainerLoadPlan");
				AssertNotNull(containerLoadPlanTemplate);

				SetupMenuItemWithOneTemplateCheckedOut(menuCustomisation);

				GlbStaff.CurrentUser.GS_IsDeveloper = true;
				using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
				{
					form.Show();

					var runMenuItem = form.MenusGrid.ContextMenu.MenuItems.FindByText("Run");

					form.CurrentMenuExposed.SU_FilterList = "JK_TransportMode == \"AAA\"";

					consol.JK_TransportMode = "BBB";
					Factory.Save();
					factory.Save();

					runMenuItem.PerformClick();

					CombineAssertions(() =>
					{
						AssertEquals("'Bill Of Lading' form cannot be shown because the filter doesn't match current execution context.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertNull("visualizer form has not been shown", ZFormModaliser.LastFormShownDialogForTest);
					});
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsDeveloper = oldIsDeveloperValue;
			}
		}

		public void TestIsLanguageEditingEnabledForVisualizerMenuPathTextBox()
		{
			var oldIsDeveloperValue = GlbStaff.CurrentUser.GS_IsDeveloper;

			try
			{
				var menuCustomisation = new VisualizerMenuCustomisation(
					new DummyNonPersistentBusinessObject(),
					new BusinessObjectFactory(),
					"Hello");

				GlbStaff.CurrentUser.GS_IsDeveloper = true;
				using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
				{
					form.Show();
					Assert(form.VisualizerMenuPathTextBox.IsLanguageEditingEnabled);
				}

				GlbStaff.CurrentUser.GS_IsDeveloper = false;
				using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
				{
					form.Show();
					Assert(!form.VisualizerMenuPathTextBox.IsLanguageEditingEnabled);
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsDeveloper = oldIsDeveloperValue;
			}
		}

		public void TestIsLanguageEditingEnabledForMenuGrid()
		{
			var oldIsDeveloperValue = GlbStaff.CurrentUser.GS_IsDeveloper;

			try
			{
				var menuCustomisation = new VisualizerMenuCustomisation(
					new DummyNonPersistentBusinessObject(),
					new BusinessObjectFactory(),
					"Hello");

				GlbStaff.CurrentUser.GS_IsDeveloper = true;
				using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
				{
					form.Show();
					AssertColumnLanguageEditing(form.MenusGrid, true);
				}

				GlbStaff.CurrentUser.GS_IsDeveloper = false;
				using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
				{
					form.Show();
					AssertColumnLanguageEditing(form.MenusGrid, false);
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsDeveloper = oldIsDeveloperValue;
			}
		}

		public void TestShowDetailsTabControlsReadOnly()
		{
			var oldIsDeveloperValue = GlbStaff.CurrentUser.GS_IsDeveloper;
			var oldIsSystemAccountValue = GlbStaff.CurrentUser.GS_IsSystemAccount;

			try
			{
				var menuCustomisation = new VisualizerMenuCustomisation(
					new DummyNonPersistentBusinessObject(),
					new BusinessObjectFactory(),
					"Hello");

				SetupMenuItemWithOneTemplateCheckedOut(menuCustomisation);

				GlbStaff.CurrentUser.GS_IsDeveloper = true;
				GlbStaff.CurrentUser.GS_IsSystemAccount = true;
				using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
				{
					form.Show();

					AssertMenuDetailsTabControlReadOnly(form.VisualizerMenuDetailsTab, false);
				}

				GlbStaff.CurrentUser.GS_IsDeveloper = false;
				GlbStaff.CurrentUser.GS_IsSystemAccount = false;
				using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
				{
					form.Show();

					var detailsTab = form.VisualizerMenuDetailsTab;
					AssertMenuDetailsTabControlReadOnly(detailsTab, "visualizerSystemCheckBox", false);
					AssertMenuDetailsTabControlReadOnly(detailsTab, "visualizerPublishedCheckBox", false);
					AssertMenuDetailsTabControlReadOnly(detailsTab, "visualizerAutoDeliveryCheckBox", true);
					AssertMenuDetailsTabControlReadOnly(detailsTab, "visualizerMenuPathTextBox", false);
					AssertMenuDetailsTabControlReadOnly(detailsTab, "visualizerDocGroupDropEdit", false);
					AssertMenuDetailsTabControlReadOnly(detailsTab, "visualizerPrimaryDocumentPicker", false);
					AssertMenuDetailsTabControlReadOnly(detailsTab, "visualizerFilterTextBox", false);
					AssertMenuDetailsTabControlReadOnly(detailsTab, "visualizerDeliveryRestrictionConditionTextBox", true);
					AssertMenuDetailsTabControlReadOnly(detailsTab, "visualizerDeliveryRestrictionDescriptionTextBox", true);
					AssertMenuDetailsTabControlReadOnly(detailsTab, "visualizerDeliveryRestrictionDropEdit", false);
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsDeveloper = oldIsDeveloperValue;
				GlbStaff.CurrentUser.GS_IsSystemAccount = oldIsSystemAccountValue;
			}
		}

		public void TestAdjustMenuGridMenu()
		{
			var oldIsDeveloperValue = GlbStaff.CurrentUser.GS_IsDeveloper;
			try
			{
				var menuCustomisation = new VisualizerMenuCustomisation(
					new DummyNonPersistentBusinessObject(),
					new BusinessObjectFactory(),
					"Hello");

				GlbStaff.CurrentUser.GS_IsDeveloper = true;
				using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
				{
					form.Show();

					var menuItems = form.MenusGrid.ContextMenu.MenuItems;
					Assert(menuItems.Count > 3);
					AssertNotNull("Run menu item should be included", menuItems.FindByText("Run"));
					AssertNotNull("Find menu item should be included", menuItems.FindByText("&Find"));
					AssertNotNull("Find Next menu item should be include", menuItems.FindByText("Find &Next"));
				}

				GlbStaff.CurrentUser.GS_IsDeveloper = false;
				using (var form = new VisualizerMenuCustomisationFormForTesting(menuCustomisation))
				{
					form.Show();

					var menuItems = form.MenusGrid.ContextMenu.MenuItems;
					Assert(menuItems.Count > 3);
					AssertNotNull("Run menu item should be included", menuItems.FindByText("Run"));
					AssertNotNull("Find menu item should be included", menuItems.FindByText("&Find"));
					AssertNotNull("Find Next menu item should be include", menuItems.FindByText("Find &Next"));
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsDeveloper = oldIsDeveloperValue;
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var menuCustomisation = new VisualizerMenuCustomisation(
				new DummyNonPersistentBusinessObject(),
				new BusinessObjectFactory(),
				"Hello");

			return new VisualizerMenuCustomisationForm(menuCustomisation);
		}

		void AssertColumnLanguageEditing(ZGrid grid, bool expectedResult)
		{
			foreach (var columnName in new[]
			{
				StmMenuItemSchema.Constants.SU_MenuName,
				StmMenuItemSchema.Constants.SU_Hint,
				StmMenuItemSchema.Constants.SU_MenuPath
			})
			{
				var columnStyle = grid.Columns[columnName].ColumnStyle as ZTranslatableTextBoxColumnStyle;
				AssertNotNull(columnStyle);

				var editControl = (ZTranslatableTextControl)columnStyle.EditControl;
				AssertEquals(expectedResult, editControl.IsLanguageEditingEnabled);
			}
		}

		void SetupMenuItemWithOneTemplateCheckedOut(VisualizerMenuCustomisation menuCustomisation)
		{
			var template = Factory.New<VisualizerTemplate>();
			template.SO_Name = "Blah Blah";
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentVisualizer.Testing.Core_Legacy.Template.TestTemplate.xls", "TestTemplate.xls");
				template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(tempFileName);
			}
			template.IsCheckedOutByMe = true;

			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Bill Of Lading";

			var pivot = Factory.New<VisualizerMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;
			pivot.SI_DocumentTitle = "ORIGINAL";
			pivot.SI_DataStoreName = "7-11";

			menuCustomisation.Menus.Add(menuItem);
		}

		void AssertMenuDetailsTabControlReadOnly(ZTabPage tabPage, bool expectedResult)
		{
			foreach (var controlName in new[]
			{
				"visualizerSystemCheckBox",
				"visualizerPublishedCheckBox",
				"visualizerAutoDeliveryCheckBox",
				"visualizerMenuPathTextBox",
				"visualizerDocGroupDropEdit",
				"visualizerPrimaryDocumentPicker",
				"visualizerFilterTextBox",
				"visualizerDeliveryRestrictionConditionTextBox",
				"visualizerDeliveryRestrictionDescriptionTextBox",
				"visualizerDeliveryRestrictionDropEdit"
			})
			{
				AssertMenuDetailsTabControlReadOnly(tabPage, controlName, expectedResult);
			}
		}

		void AssertMenuDetailsTabControlReadOnly(ZTabPage tabPage, string controlName, bool expectedResult)
		{
			var control = tabPage.Controls.Find(controlName, false).FirstOrDefault();
			AssertNotNull(control);
			AssertEquals(expectedResult, control.GetReadOnly());
		}

		class VisualizerMenuCustomisationFormForTesting : VisualizerMenuCustomisationForm
		{
			public VisualizerMenuCustomisationFormForTesting(VisualizerMenuCustomisation menuCustomisation)
				: base(menuCustomisation)
			{
			}

			public ZTabPage MenuDetailsTab
			{
				get { return base.menuDetailsTab; }
			}

			public StmMenuItemBase CurrentMenuExposed
			{
				get { return base.CurrentMenu; }
			}

			public ZTabPage VisualizerMenuDetailsTab
			{
				get { return base.visualizerMenuDetailsTab; }
			}

			public ZGrid AvailableTemplatesGrid
			{
				get { return base.availableTemplatesGrid; }
			}

			public new ZGrid MenusGrid
			{
				get { return base.MenusGrid; }
			}

			public ZTabPage MenuTemplatesTabPage
			{
				get { return base.menuTemplatesTabPage; }
			}

			public ZTranslatableTextControl VisualizerMenuPathTextBox
			{
				get { return base.visualizerMenuPathTextBox; }
			}

			public void SelectTemplateTab()
			{
				PivotAndChildMenuTabControl.SelectedTab = menuTemplatesTabPage;
			}
		}

		#endregion
	}
}
