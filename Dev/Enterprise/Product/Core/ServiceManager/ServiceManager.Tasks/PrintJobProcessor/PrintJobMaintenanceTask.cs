using System.Security.Principal;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration;
using Enterprise.PrintProcessing;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"PJM",
	"Print Job Maintenance",
	"DOC",
	typeof(Enterprise.ServiceManager.Tasks.PrintJobProcessor.PrintJobMaintenanceTask),
	IsMandatory = true,
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "30minutes"
	)]

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	class PrintJobMaintenanceTask : EmailJobTask
	{
		protected override int MaxBatchSize => BaseMaxBatchSize;

		public override void RunTask(CancellationToken token)
		{
			var sid = WindowsIdentity.GetCurrent().User;
			if (sid.IsWellKnown(WellKnownSidType.LocalSystemSid) ||
				sid.IsWellKnown(WellKnownSidType.LocalServiceSid) ||
				sid.IsWellKnown(WellKnownSidType.NetworkServiceSid))
			{
				ServiceLogger?.Log(LogType.Warning, "Network printers are not available as the service is operating under the 'Local System' account. If network printers are connected to this machine please reinstall the service and select 'Log On' user and password.");
			}

			if (new PrintQueueManager().MaintainStmPrintQueue())
			{
				ServiceLogger?.Log(LogType.Information, "Available printers have been updated.");
			}

			var purgeDate = ZDateTime.Now.AddDays(-14);
			var query = new ZQuery(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.LessThanOrEqualTo, purgeDate);
			var printJobsQueue = new DbOnlyBusinessObjectQueue<StmPrintJob>(query);
			printJobsQueue.ProcessBatch((printJobs, e) => PrintJobManager.Delete(printJobs), 10, token);

			DocumentJobTask.CleanupStmDeliveryGroup(false);
		}
	}
}
