using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class FastAccessLayoutSubMenuItemsCreatorTest : TestCaseWithDummy
	{
		public void TestAddFastAccessLayoutMenus()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				form.TabGrid.OnPopup_CallForTesting();

				var menu = form.TabGrid.ContextMenu.MenuItems.FindByText("Available Layouts");
				AssertNotNull(menu);

				AssertEquals("one sub menu", 1, menu.MenuItems.Count);
				AssertEquals("Users see no layouts are avaialable", FastAccessLayoutSubMenuItemsCreator.ResetToSystemDefaultLayoutMenuText, menu.MenuItems[0].Text);
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;

				var layout = Factory.New<StmModuleFilter>();
				layout.S9_ModuleID = gridID;
				layout.S9_FilterName = "A";

				var layout2 = Factory.New<StmModuleFilter>();
				layout2.S9_ModuleID = gridID;
				layout2.S9_FilterName = "B";
				layout2.S9_IsPublished = true;

				var layout3 = Factory.New<StmModuleFilter>();
				layout3.S9_ModuleID = gridID;
				layout3.S9_FilterName = "C";
				layout3.S9_IsPublished = true;

				Factory.Save();

				form.Show();
				form.TabGrid.OnPopup_CallForTesting();

				var menu = form.TabGrid.ContextMenu.MenuItems.FindByText("Available Layouts");
				AssertNotNull(menu);

				AssertEquals("Three menu items should be available", 5, menu.MenuItems.Count);

				AssertEquals("B", menu.MenuItems[0].Text);
				AssertEquals("C", menu.MenuItems[1].Text);
				AssertEquals("A", menu.MenuItems[2].Text);
				AssertEquals(FastAccessLayoutSubMenuItemsCreator.ResetToSystemDefaultLayoutMenuText, menu.MenuItems[4].Text);
			}
		}

		public void TestEventHandlerForFastAccessLayout()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				var layoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				new DataGridLayoutManager().SavePreconfiguredLayout(layoutManageable, "[DescriptionVisible]", true, false, SaveColumnLayout.Ignore);

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;

				layoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				new DataGridLayoutManager().SavePreconfiguredLayout(layoutManageable, "[NumberVisible]", true, false, SaveColumnLayout.Ignore);

				form.TabGrid.OnPopup_CallForTesting();
				var menu = form.TabGrid.ContextMenu.MenuItems.FindByText("Available Layouts");
				AssertNotNull(menu);

				AssertEquals("SubMenu", 4, menu.MenuItems.Count);

				AssertEquals("PreCondition", FastAccessLayoutSubMenuItemsCreator.ResetToSystemDefaultLayoutMenuText, menu.MenuItems[3].Text);

				menu.MenuItems[3].PerformClick();
				AssertEquals("Should have been reset to default", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals("Should have been reset to default", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				//it is important to have these brackets around the name in the menu text as the event handler for the menu depends on the exact name
				AssertEquals("[DescriptionVisible]", menu.MenuItems[0].Text);

				menu.MenuItems[0].PerformClick();

				AssertEquals(false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
				form.TabGrid.SaveLayoutEvenIfColumnsAreUnchanged();
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;

				AssertEquals("CurrentColumnLayoutName", "[DescriptionVisible]", form.TabGrid.CurrentColumnLayout.ColumnLayoutName);
				AssertEquals("Number should be invisible", false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
			}
		}

		public void TestSaveLastSelectedLayout()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				var layoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				new DataGridLayoutManager().SavePreconfiguredLayout(layoutManageable, "[DescriptionVisible]", true, false, SaveColumnLayout.Ignore);

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;

				layoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				new DataGridLayoutManager().SavePreconfiguredLayout(layoutManageable, "[NumberVisible]", true, false, SaveColumnLayout.Ignore);

				AssertEquals(false, form.TabGrid.SaveLastSelectedLayout);
				form.TabGrid.OnPopup_CallForTesting();
				var menu = form.TabGrid.ContextMenu.MenuItems.FindByText("Available Layouts");
				AssertNotNull(menu);
				AssertEquals("SubMenu", 4, menu.MenuItems.Count);

				AssertEquals("[DescriptionVisible]", menu.MenuItems[0].Text);
				menu.MenuItems[0].PerformClick();

				AssertEquals(true, form.TabGrid.SaveLastSelectedLayout);
				AssertEquals(false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;

				AssertEquals("CurrentColumnLayoutName", "[DescriptionVisible]", form.TabGrid.CurrentColumnLayout?.ColumnLayoutName);
				AssertEquals("Number should be invisible", false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
			}
		}

		public void TestLastFocusedColumnStyleShouldHideEditControl()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;
				form.TabGrid.Focus();
				form.TabGrid.CurrentCell = new DataGridCell(0, 1);

				var layoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				new DataGridLayoutManager().SavePreconfiguredLayout(layoutManageable, "[TestLayout]", true, false, SaveColumnLayout.Ignore);

				AssertNotEquals(0, form.TabGrid.LastFocusedColumn.EditControl.Width);

				form.TabGrid.OnPopup_CallForTesting();
				var menu = form.TabGrid.ContextMenu.MenuItems.FindByText("Available Layouts");
				AssertNotNull(menu);
				AssertEquals("SubMenu", 3, menu.MenuItems.Count);

				AssertEquals("[TestLayout]", menu.MenuItems[0].Text);
				menu.MenuItems[0].PerformClick();

				AssertEquals(0, form.TabGrid.LastFocusedColumn.EditControl.Width);
			}
		}
	}
}
