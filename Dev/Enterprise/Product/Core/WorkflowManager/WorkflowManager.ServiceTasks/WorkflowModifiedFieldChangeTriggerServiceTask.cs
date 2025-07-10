using System.Threading;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("WMF", "Workflow Modified Field Change Trigger", "WFL", typeof(Enterprise.WorkflowManager.ServiceTasks.WorkflowModifiedFieldChangeTriggerServiceTask),
	IsMandatory = true,
	MinimumPeriod = "5minutes",
	DefaultScheduleRunEvery = "5minutes")
]

namespace Enterprise.WorkflowManager.ServiceTasks
{
	public class WorkflowModifiedFieldChangeTriggerServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			WorkflowModifiedFieldChangeTriggerProcessor processor = new WorkflowModifiedFieldChangeTriggerProcessor();
			processor.Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}
	}
}
