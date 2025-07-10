using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	sealed class OperationalActionsModulePluginTest : TestCaseWithFactory
	{
		public void TestGetActionMenuItemsToAddCore()
		{
			var items = Plugin.GetActionMenuItemToAdd();
			AssertEquals("should have returned a menu item", 1, items.Count);

			using (var item = items[0])
			{
				AssertEquals("Operational Actions", item.Text);
				AssertEquals("IsParent", true, item.IsParent);
			}

			var manager = new OperationalActionManager(Factory, new OperationalActionContext(Module.OperationalActionSupporter, "Module Name"));
			manager.Actions.RemoveAndDeleteAll();
			OperationalAction action1 = manager.Actions.AddNew();
			OperationalAction action2 = manager.Actions.AddNew();
			action1.SU_MenuName = "Action1";
			action1.SU_MenuPath = "";
			action2.SU_MenuName = "Action2";
			action2.SU_MenuPath = "/";
			Factory.Save();

			items = Plugin.GetActionMenuItemToAdd();
			AssertEquals("should have returned 2 menu item", 2, items.Count);

			using (var item = items[0])
			{
				AssertEquals("Operational Actions", item.Text);
				AssertEquals("IsParent", true, item.IsParent);
				item.OnPopup(EventArgs.Empty);
				AssertEquals("Action1", item.MenuItems[0].Text);
				AssertEquals("IsParent", false, item.MenuItems[0].IsParent);
			}
			using (var item = items[1])
			{
				AssertEquals("Action2", item.Text);
				AssertEquals("IsParent", false, item.IsParent);
			}
		}

		public void TestGetButtonGridMenuItemsToAddCore()
		{
			DummyBusinessObject parent = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject child1 = parent.Collection.AddNew();
			DummyChildBusinessObject child2 = parent.Collection.AddNew();
			DummyChildBusinessObject child3 = parent.Collection.AddNew();

			using (ZForm form = new ZForm(parent))
			{
				form.Size = new Size(300, 200);

				ZTextBoxColumnStyleInfo column = new ZTextBoxColumnStyleInfo();
				column.Caption = "Description";
				column.ColumnName = "Z0_Description";

				ZModuleButtonGrid grid = new ZModuleButtonGrid();
				grid.Dock = DockStyle.Fill;
				grid.BindToFindBoxList = "FilteredCollection";
				grid.BindToGridList = "Collection";
				grid.ModuleID = DummyModuleIDs.Dummy;
				grid.InnerGrid.ColumnStyles.Add(column);

				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				using (MenuItem item = Plugin.GetButtonGridMenuItemToAdd(grid))
				{
					AssertNotNull("should have returned a menu item", item);
					AssertEquals("Operational Actions", item.Text);
					AssertEquals("IsParent", true, item.IsParent);
				}
			}
		}

		public void TestRequiresMultiSelect()
		{
			AssertEquals("Requires multi-select.", true, Plugin.RequiresMultiSelect);
		}

		#region Implementation

		OperationalActionsModulePlugin Plugin
		{
			get { return plugin ?? (plugin = new OperationalActionsModulePlugin(new ModuleSelection(Module), new OperationalActionContext(Module.OperationalActionSupporter, "Module Name"))); }
		}
		OperationalActionsModulePlugin plugin;

		DummyModuleWithActionsSupport<DummyBusinessObjectWithDocumentSupport> Module
		{
			get { return module ?? (module = new DummyModuleWithActionsSupport<DummyBusinessObjectWithDocumentSupport>()); }
		}
		DummyModuleWithActionsSupport<DummyBusinessObjectWithDocumentSupport> module;

		protected override void TearDown()
		{
			using (module)
			{
				base.TearDown();
			}
		}

		#endregion
	}
}
