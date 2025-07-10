using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.Wow
{
	public partial class WoolworthsReportUserControl : ReportUserControl
	{
		public WoolworthsReportUserControl(ModuleIdentifier id, ReportCommandCollection reports, ISecurityCheckpoint securityCheckPoint)
			: base(id, reports, securityCheckPoint)
		{
			InitializeComponent();
		}

		protected override void PrintReportSet(ReportPrintSet set)
		{
			PrintReportSet(set, null);
		}

		protected internal void PrintReportSet(ReportPrintSet set, DeliveryInstructions instructions)
		{
			set.AfterReportRun += new PrintTask.AfterReportRunEventHandler(OnReportPrintSet_OnAfterReportRun);
			if (instructions != null)
			{
				set.Run(instructions);
			}
			else
			{
				set.Run(Env.Security.None);
			}
		}

		protected void OnReportPrintSet_OnAfterReportRun(object sender, IDeliverable itemToDeliver)
		{
			if (itemToDeliver is Report)
			{
				Report report = (Report)itemToDeliver;
				if (report.Name.ToLower().IndexOf("indent") != -1)
				{
					WoolworthsOrder.UpdateOrderStatusesForReport(report);
				}
			}
		}
	}

	#region Implementation
	#endregion

}
