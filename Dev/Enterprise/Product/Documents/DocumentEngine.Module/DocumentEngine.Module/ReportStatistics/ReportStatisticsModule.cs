using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Scheduler.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.DocumentEngine.Module.Res;
using ResString = Enterprise.DocumentEngine.Module.ResString;

namespace Enterprise.DocumentEngine.Scheduler.Module
{
	public class ReportStatisticsModule : ZFilterGridModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ReportStatistics; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ReportStatistics);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ReportStatisticsFilterControl(GridCollection, (ReportStatisticsFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new StmReportRunCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ReportStatisticsFilterBusinessObject();
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return true; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		#endregion

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			var result = base.GetNewAdditionalMenuItems().ToList();
			result.Add(new ZMenuItem(ResString.GetMultilingualString("bd216423-5d73-4bc2-b071-be40293c43ec", "Open Scheduled Report"), OpenScheduledReport));
			return result.ToArray();
		}

		void OpenScheduledReport(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects != null && SelectedBusinessObjects.Length > 0)
			{
				foreach (BusinessObject obj in SelectedBusinessObjects)
				{
					OpenScheduledReportCore(obj as StmReportRun);
				}
			}
			else if (CurrentBusinessObjectInGrid != null)
			{
				OpenScheduledReportCore(CurrentBusinessObjectInGrid as StmReportRun);
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		void OpenScheduledReportCore(StmReportRun stmReportRun)
		{
			if (stmReportRun.ScheduleTask != null)
			{
				var controller = ZControllerFactory.Instance.GetControllerForBizo(stmReportRun.ScheduleTask);
				controller.ShowEditForm(stmReportRun.ScheduleTask);
			}
			else
			{
				Globals.Message.Show(Res.GetString("eaef395a-ab08-43df-91f7-3375cf899ef9", "Could not find related scheduled report. Maybe it was a one-off or has been deleted."));
			}
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ReportStatistics; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion
	}
}
