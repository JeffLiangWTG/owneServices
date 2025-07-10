using System.Threading;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.ServiceTasks;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	DsbJobBatchCloseServiceTask.Code,
	DsbJobBatchCloseServiceTask.Description,
	"ACC",
	typeof(DsbJobBatchCloseServiceTask),
	IsMandatory = false,
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "5minutes",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)
]

namespace Enterprise.Accounting.ServiceTasks
{
	public class DsbJobBatchCloseServiceTask : ServiceProviderImpl
	{
		public const string Code = "JBC";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "DSB Job Batch Close Service Task";

		public override void RunTask(CancellationToken token)
		{
			var processor = new DsbJobBatchCloseProcessor(new JCSLogger(ServiceLogger));
			processor.Process(token);
		}
	}
}
