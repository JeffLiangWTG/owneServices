using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.DocumentEngine.Module.Res;

namespace Enterprise.DocumentEngine.Scheduler.Module
{
	public class ReportManagementModule : ZFilterGridModule
	{
		#region Standard Module Override

		public override ModuleIdentifier ID => ModuleIDs.ReportManagement;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ReportManagement);
		}

		public override bool AllowNew => false;

		public override bool AllowDelete => false;

		public override bool AllowEdit => false;

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ReportManagementFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ReportManagementFilterControl(GridCollection, (ReportManagementFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ReportScheduleTaskCollection(Factory);
		}

		protected override MenuItem[] GetNewAdditionalContextMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewAdditionalContextMenuItems());

			if (Env.Security.ReportManagementCancelExecution.IsAllowed)
			{
				var cancel = new ZMenuItem(Res.GetData("2cfcd4bc-7454-46d4-be47-67e69a70aa2f", "Cancel", "Cancel the execution of scheduled reports"), CancelScheduleReport);
				cancel.Name = "Cancel";
				result.Add(cancel);
			}

			if (Env.Security.ReportManagementCancelExecutionAndMarkAsInactive.IsAllowed)
			{
				var cancelAndMarkInactive = new ZMenuItem(Res.GetData("cb0a5203-8672-4bec-b0f7-0f288046cb6c", "Cancel && Mark As Inactive", "Cancel the execution of scheduled reports and mark as inactive"), CancelScheduleReportAndMarkAsInactive);
				cancelAndMarkInactive.Name = "CancelAndMarkInactive";
				result.Add(cancelAndMarkInactive);
			}

			return result.ToArray();
		}

		void CancelScheduleReport(object sender, EventArgs args)
		{
			if (SelectedBusinessObjects != null)
			{
				if (Globals.Message.Show(
					Res.GetString("127b2101-a534-434c-a5e7-401afe3f0429", "Do you want to cancel the execution of these selected reports?"),
					Res.GetString("7daf1b96-b034-46a0-aabf-45046a885b1f", "Cancel Selected Reports"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question) == DialogResult.Yes)
				{
					CancelScheduleReportCore(SelectedBusinessObjects, false);
				}
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		void CancelScheduleReportAndMarkAsInactive(object sender, EventArgs args)
		{
			if (SelectedBusinessObjects != null)
			{
				if (Globals.Message.Show(
					Res.GetString("be569c0a-fa13-4268-ba15-657419445b98", "Do you want to cancel the execution of these selected reports and mark them as inactive?"),
					Res.GetString("7daf1b96-b034-46a0-aabf-45046a885b1f", "Cancel Selected Reports"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question) == DialogResult.Yes)
				{
					CancelScheduleReportCore(SelectedBusinessObjects, true);
				}
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		protected void CancelScheduleReportCore(BusinessObject[] selectedElements, bool markAsInactive)
		{
			foreach (var bizo in selectedElements)
			{
				var currentReport = (ReportScheduleTask)bizo;
				if (currentReport != null)
				{
					if (markAsInactive)
					{
						currentReport.S5_IsActive = false;
					}

					currentReport.NotifyUserOfReportBeingCanceled();

					if (currentReport.RunningReport != null)
					{
						currentReport.RunningReport.RRI_Status = Constants.StmReportRunState.Cancelled;
					}
					else
					{
						currentReport.SkipCurrentQueuedRun();
					}
				}
			}
			ZExceptionReporting.ProcessWithSaveExceptionHandling(() => Factory.Save(), null);
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ReportManagement;

		#endregion

		#region Licence
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		#endregion
	}
}
