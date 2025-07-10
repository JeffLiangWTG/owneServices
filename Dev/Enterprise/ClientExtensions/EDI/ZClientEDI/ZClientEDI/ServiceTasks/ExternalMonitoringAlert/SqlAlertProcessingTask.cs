using System.Threading;
using Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(SqlAlertProcessingTask.ServiceTaskCode,
	SqlAlertProcessingTask.ServiceTaskDescription,
	"SYS",
	typeof(SqlAlertProcessingTask),
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = false,
	IsMandatory = false,
	MinimumPeriod = "1hour",
	MaximumPeriod = "10day",
	DefaultScheduleRunEvery = "1days",
	ActiveByDefault = true
	)]
namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert
{
	public class SqlAlertProcessingTask : ServiceProviderImpl
	{
		public const string ServiceTaskCode = "SQA";
		public const string ServiceTaskDescription = "SQL Monitoring Alert Processing Task";

		[HostedServiceRequirement]
		public static string CheckWiseGridReportingConnectionString() => HostedServiceRequirementAttribute.CheckValueIsNotNullOrEmptyString(EDIDataRegistry.Instance.ExternalMonitoringWiseGridReportingConnectionString);

		public SqlAlertProcessingTask()
		{
		}

		internal SqlAlertProcessingTask(AlertProcessor processor)
		{
			this.processor = processor;
		}

		public override void RunTask(CancellationToken token)
		{
			if (processor == null)
			{
				processor = new AlertProcessor(ServiceLogger);
			}

			processor.Run(token);
		}

		AlertProcessor processor;
	}
}
