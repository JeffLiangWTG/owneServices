using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Scheduler.Module
{
	public partial class ReportManagementFilterControl : ZFilterStripControl
	{
		public ReportManagementFilterControl(IBusinessObjectCollection gridCollection, ReportManagementFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();

			if (grid.ContextMenu != null)
			{
				grid.ContextMenu.Popup += ContextMenu_Popup;
			}
		}

		void ContextMenu_Popup(object sender, System.EventArgs e)
		{
			var cancelMenuItem = grid.ContextMenu.MenuItems.FindByName((NoResString)"Cancel"); // Key for find the menu
			var cancelAndMarkInactiveMenuItem = grid.ContextMenu.MenuItems.FindByName("CancelAndMarkInactive"); // Key for find the menu
			if (cancelMenuItem == null || cancelAndMarkInactiveMenuItem == null)
			{
				return;
			}

			if (grid.SelectedRowCount > 0)
			{
				if (Env.Instance.Security.ReportManagementCancelOtherUserReport.IsAllowed)
				{
					cancelMenuItem.Enabled = true;
					cancelAndMarkInactiveMenuItem.Enabled = true;
				}
				else
				{
					var selectedScheduleTasks = grid.SelectedElements;
					var isAllowed = true;
					foreach (var bizo in selectedScheduleTasks)
					{
						var currentScheduleTask = (StmScheduleTask)bizo;
						if (currentScheduleTask.S5_GS_NKPrintUser != GlbStaff.CurrentUser.GS_Code)
						{
							isAllowed = false;
							break;
						}
					}
					cancelMenuItem.Enabled = isAllowed;
					cancelAndMarkInactiveMenuItem.Enabled = isAllowed;
				}
			}
			else
			{
				cancelMenuItem.Enabled = false;
				cancelAndMarkInactiveMenuItem.Enabled = false;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (grid.ContextMenu != null)
				{
					grid.ContextMenu.Popup -= ContextMenu_Popup;
				}
			}

			base.Dispose(disposing);
		}
	}
}
