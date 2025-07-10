using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	public partial class ReportStatisticsForm : ZTemplateForm
	{
		readonly ZPlugIn eDocsPlugin;

		public ReportStatisticsForm(StmReportRun stmReportRun)
			: base(stmReportRun)
		{
			InitializeComponent();

			MainTabControl.Controls.Remove(LogsTabPage);
			LogsTabPage.Dispose();
			if (!DataRegistry.Instance.ReportStatisticsLogExecutionPlan && string.IsNullOrWhiteSpace(BusinessEntity.RRI_ExecutionPlanText))
			{
				BusinessEntity.RRI_ExecutionPlanText = Res.GetString("8f540e8c-e88b-4f34-aed8-495ec8067382", "Enable registry setting '{0}' to log execution plans.", RawDataRegistry.Instance.ReportStatisticsLogExecutionPlan.Caption);
				BusinessEntity.HasChanges = false;
			}

			eDocsPlugin = PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
			if (eDocsPlugin is IEDocsPlugIn plugIn)
			{
				plugIn.DisableInsert();
			}

			Load += (_, _) => SetReadonlyExceptEDocsTab();
		}

		void SetReadonlyExceptEDocsTab()
		{
			MainTabControl.Controls
				.OfType<ZTabPage>()
				.Where(tab => tab != eDocsPlugin?.TabPage)
				.ForEach(tab => tab.SetReadOnlyIncludingChildren());
		}

		public new StmReportRun BusinessEntity
		{
			get { return (StmReportRun)base.BusinessEntity; }
		}

		protected override bool AllowNew => false;

		void openScheduledReportButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.ScheduleTask != null)
			{
				var controller = ZControllerFactory.Instance.GetControllerForBizo(BusinessEntity.ScheduleTask);
				controller.ShowEditForm(BusinessEntity.ScheduleTask);
			}
			else
			{
				Globals.Message.Show(Res.GetString("f916e3f3-25c2-45cc-8546-9a83da5d5b41", "Could not find related scheduled report. Maybe it was a one-off or has been deleted."));
			}
		}
	}
}
