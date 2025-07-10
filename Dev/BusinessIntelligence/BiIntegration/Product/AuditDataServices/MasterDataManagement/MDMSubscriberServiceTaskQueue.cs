using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	Enterprise.AuditDataServices.MDM.MDMSubscriberServiceTask.Code,
	Enterprise.AuditDataServices.MDM.MDMSubscriberServiceTask.Description,
	typeof(Enterprise.AuditDataServices.MDM.MDMSubscriberServiceTaskQueue))]
namespace Enterprise.AuditDataServices.MDM
{
	public class MDMSubscriberServiceTaskQueue : AuditSubscriberTaskQueue
	{
		protected override AuditSubscriberTask GetAuditSubscriberServiceTask()
		{
			return new MDMSubscriberServiceTask();
		}
	}
}
