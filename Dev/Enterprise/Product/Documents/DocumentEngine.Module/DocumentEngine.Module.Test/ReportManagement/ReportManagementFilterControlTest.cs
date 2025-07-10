using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Scheduler.Module.Testing
{
	sealed class ReportManagementFilterControlTest : TestCaseWithFactory
	{
		public void TestCancelMenuItemsApplicability()
		{
			using (var form = new ZForm())
			using (var module = new ReportManagementModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				var scheduleReport = (ReportScheduleTask)module.GridCollection.AddNew();
				var queuedReport = (ReportScheduleTask)module.GridCollection.AddNew();
				form.Show();
				Application.DoEvents();
				module.DisplayGrid.SetCurrentHitTestForTest(0, 0);

				var cancelMenuItem = module.DisplayGrid.ContextMenu.MenuItems.FindByName("Cancel");
				var cancelAndMarkInactiveMenuItem = module.DisplayGrid.ContextMenu.MenuItems.FindByName("CancelAndMarkInactive");

				module.DisplayGrid.OnPopup_CallForTesting();

				Assert(!cancelMenuItem.Enabled);
				Assert(!cancelAndMarkInactiveMenuItem.Enabled);

				module.DisplayGrid.SelectAllElements();
				module.DisplayGrid.OnPopup_CallForTesting();

				Assert(cancelMenuItem.Enabled);
				Assert(cancelAndMarkInactiveMenuItem.Enabled);

				var stmReportRun = scheduleReport.StmReportRuns.AddNew();
				stmReportRun.RRI_Status = Constants.StmReportRunState.Running;
				stmReportRun.RRI_S5_Schedule = scheduleReport.PK;
				var stmReportQueued = queuedReport.StmReportRuns.AddNew();
				stmReportQueued.RRI_Status = Constants.StmReportRunState.Queued;
				stmReportQueued.RRI_S5_Schedule = scheduleReport.PK;
				module.DisplayGrid.OnPopup_CallForTesting();
				Assert(cancelMenuItem.Enabled);
				Assert(cancelAndMarkInactiveMenuItem.Enabled);

				scheduleReport.S5_GS_NKPrintUser = "OTH";
				Env.Instance.Security.ReportManagementCancelOtherUserReport.IsAllowed = false;
				module.DisplayGrid.OnPopup_CallForTesting();
				Assert(!cancelMenuItem.Enabled);
				Assert(!cancelAndMarkInactiveMenuItem.Enabled);

				Env.Instance.Security.ReportManagementCancelOtherUserReport.IsAllowed = true;
				module.DisplayGrid.OnPopup_CallForTesting();
				Assert(cancelMenuItem.Enabled);
				Assert(cancelAndMarkInactiveMenuItem.Enabled);

				stmReportRun.RRI_Status = Constants.StmReportRunState.Queued;
				Assert(cancelMenuItem.Enabled);
				Assert(cancelAndMarkInactiveMenuItem.Enabled);
			}
		}
	}
}
