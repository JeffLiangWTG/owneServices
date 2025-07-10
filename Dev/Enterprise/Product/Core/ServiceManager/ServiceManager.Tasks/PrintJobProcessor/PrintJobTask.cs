using System.Threading;
using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"PRD",
	"Printing documents",
	"DOC",
	typeof(Enterprise.ServiceManager.Tasks.PrintJobProcessor.PrintJobTask),
	IsMandatory = true,
	AllowsMultipleInstances = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "15seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding("PRD", StmPrintJobQueueSchema.Constants.TableName, new[] { StmPrintJobQueueSchema.Constants.SPQ_JobType + "=PRN" }, "Printing documents - JobType = 'PRN'")]
[assembly: HostedServiceBusinessObjectBinding("PRD", StmPrintJobQueueSchema.Constants.TableName, new[] { StmPrintJobQueueSchema.Constants.SPQ_JobType + "=PRS" }, "Printing documents - JobType = 'PRS'")]

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	class PrintJobTask : PrintJobTaskCore
	{
		public override void RunTask(CancellationToken token)
		{
			RunTaskForJobTypes(requiresPrintServer: true, token, PrintJobType.PRN);
			RunTaskForJobTypes(requiresPrintServer: false, token, PrintJobType.PRS); // Should not check print server for PRS print jobs
		}
	}
}
