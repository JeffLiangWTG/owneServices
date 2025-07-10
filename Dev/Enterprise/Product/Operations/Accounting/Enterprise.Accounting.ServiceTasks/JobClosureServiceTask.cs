using System;
using System.Threading;
using CargoWise.Data;
using Enterprise.Accounting.Business.JobInvoicing;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Accounting.ServiceTasks.JobClosureServiceTask.Code,
	"Job Closure Service Task",
	"ACC",
	typeof(Enterprise.Accounting.ServiceTasks.JobClosureServiceTask),
	IsMandatory = false,
	AllowsMultipleInstances = false, //This is temporary. We will revert it in a future workitem where we are going to modify the JCS queue process. 
	CanRunInAnyBranch = true,
	MinimumPeriod = "1day",
	DefaultScheduleRunEvery = "1day",
	ActiveByDefault = true)
]

[assembly: HostedServiceQueueProvider(
	Enterprise.Accounting.ServiceTasks.JobClosureServiceTask.Code,
	"Job Closure Queue",
	typeof(Enterprise.Accounting.ServiceTasks.JobClosureServiceTask.QueueProvider))
]

namespace Enterprise.Accounting.ServiceTasks
{
	public class JobClosureServiceTask : ServiceProviderImpl
	{
		public const string Code = "JCS";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Job Closure";

		public override void RunTask(CancellationToken token)
		{
			var processor = new AutoJobStatusUpdateProcessor(ServiceLogger);
			processor.Process(token);
		}

		public class QueueProvider : IHostedServiceQueueProvider
		{
			public QueueResult QueueResult
			{
				get
				{
					var result = QueueResult.Error;
					var query = "SELECT COUNT(1), ISNULL(MAX(DATEDIFF(second, JHC_SystemCreateTimeUtc, GetUTCDate())), 0) FROM dbo.JobToCloseQueue";
					Db.Connection.ExecuteReader(query, reader => result = new QueueResult(reader.GetInt32(0), TimeSpan.FromSeconds(reader.GetInt32(1))));
					return result;
				}
			}
		}
	}
}
