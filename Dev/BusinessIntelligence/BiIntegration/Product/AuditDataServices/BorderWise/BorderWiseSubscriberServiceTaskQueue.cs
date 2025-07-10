using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	Enterprise.AuditDataServices.BorderWise.BorderWiseSubscriberServiceTask.Code,
	Enterprise.AuditDataServices.BorderWise.BorderWiseSubscriberServiceTask.Description,
	typeof(Enterprise.AuditDataServices.BorderWise.BorderWiseSubscriberServiceTaskQueue))]
namespace Enterprise.AuditDataServices.BorderWise
{
	public class BorderWiseSubscriberServiceTaskQueue : AuditSubscriberTaskQueue
	{
		protected override AuditSubscriberTask GetAuditSubscriberServiceTask()
		{
			return new BorderWiseSubscriberServiceTask();
		}
	}
}
