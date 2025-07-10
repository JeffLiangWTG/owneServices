using System.Threading;
using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"EDD",
	"Email and Fax documents delivery",
	"DOC",
	typeof(Enterprise.ServiceManager.Tasks.PrintJobProcessor.EmailJobTask),
	IsMandatory = true,
	AllowsMultipleInstances = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding("EDD", StmPrintJobQueueSchema.Constants.TableName, new[] { StmPrintJobQueueSchema.Constants.SPQ_JobType + "=EML" }, "Email delivery")]
[assembly: HostedServiceBusinessObjectBinding("EDD", StmPrintJobQueueSchema.Constants.TableName, new[] { StmPrintJobQueueSchema.Constants.SPQ_JobType + "=FAX" }, "Fax delivery")]

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	class EmailJobTask : PrintJobTaskCore
	{
		protected override int MaxBatchSize => -1;

		public override void RunTask(CancellationToken token)
		{
			RunTaskForJobTypes(requiresPrintServer: false, token, PrintJobType.EML, PrintJobType.FAX);
		}
	}
}
