using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	Enterprise.AuditDataServices.Core.CoreSubscriberServiceTask.Code,
	Enterprise.AuditDataServices.Core.CoreSubscriberServiceTask.Description,
	typeof(Enterprise.AuditDataServices.Core.CoreSubscriberServiceTaskQueue))]

namespace Enterprise.AuditDataServices.Core
{
	public class CoreSubscriberServiceTaskQueue : AuditSubscriberTaskQueue
	{
		protected override AuditSubscriberTask GetAuditSubscriberServiceTask()
		{
			return new CoreSubscriberServiceTask();
		}
	}
}
