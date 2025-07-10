using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ManagementTabPageDependencyMenuItemTest : DependencyMenuItemBaseTest
	{
		#region DependencyMenuItemBaseTest Overrides

		protected override bool ShouldSaveAfterActions => false;

		protected override ZToolStripMenuItem GetMenuItem(ProcessHeader workflow, string menuItemName)
		{
			return GetMenuItem(workflow, menuItemName, disposables);
		}

		#endregion

		#region Implementation

		internal static ZToolStripMenuItem GetMenuItem(ProcessHeader workflow, string menuItemName, DisposableList disposables)
		{
			var form = new ZForm { ControllerID = DummyControllerIDs.Dummy };
			var control = new WorkflowManagementUserControl();

			disposables.Add(form);

			form.Controls.Add(control);
			form.Show();
			control.SetDataBinding(workflow.JobHeader, string.Empty);

			Application.DoEvents();

			control.WorkflowsGrid.ListManager.Position = control.WorkflowsGrid.List.IndexOf(workflow);
			Application.DoEvents();

			AssertEquals(workflow, control.WorkflowsGrid.GetCurrent());

			var workflowsControl = control.FindAll<WorkflowsUserControl>().Single();
			workflowsControl.ContextMenu_Popup(workflowsControl.WorkflowsGrid.ContextMenu, EventArgs.Empty);

			var menuItem = (ZMenuItem)workflowsControl.WorkflowsGrid.ContextMenu.MenuItems.Cast<MenuItem>().Single(m => m.Text == menuItemName);

			menuItem.OnPopup_Exposed();

			return (ZToolStripMenuItem)MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(menuItem);
		}

		#endregion
	}

	class ManagementTabPageParentChildMenuItemTest : ParentChildMenuItemBaseTest
	{
		#region ParentChildMenuItemBaseTest Overrides

		protected override bool ShouldSaveAfterActions => false;

		protected override ZToolStripMenuItem GetMenuItem(ProcessHeader workflow, string menuItemName)
		{
			return ManagementTabPageDependencyMenuItemTest.GetMenuItem(workflow, menuItemName, disposables);
		}

		#endregion
	}
}
