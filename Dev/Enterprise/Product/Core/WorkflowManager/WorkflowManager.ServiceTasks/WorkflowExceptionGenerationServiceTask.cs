using System.Threading;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("WEX", "Workflow Exception Generator", "WFL", typeof(Enterprise.WorkflowManager.ServiceTasks.WorkflowExceptionGenerationServiceTask),
	IsMandatory = true,
	MinimumPeriod = "5minutes",
	DefaultScheduleRunEvery = "20minutes")
]

namespace Enterprise.WorkflowManager.ServiceTasks
{
	internal class WorkflowExceptionGenerationServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			WorkflowExceptionGenerationProcessor processor = new WorkflowExceptionGenerationProcessor();
			processor.Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}
	}
}
