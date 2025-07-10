using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	Enterprise.AuditDataServices.XT.XTSubscriberServiceTask.Code,
	Enterprise.AuditDataServices.XT.XTSubscriberServiceTask.Description,
	typeof(Enterprise.AuditDataServices.XT.XTSubscriberServiceTaskQueue))]
namespace Enterprise.AuditDataServices.XT
{
	public class XTSubscriberServiceTaskQueue : AuditSubscriberTaskQueue
	{
		protected override AuditSubscriberTask GetAuditSubscriberServiceTask()
		{
			return new XTSubscriberServiceTask();
		}
	}
}
