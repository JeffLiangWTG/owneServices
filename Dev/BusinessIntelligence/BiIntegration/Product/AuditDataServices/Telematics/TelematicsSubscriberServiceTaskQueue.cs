using Enterprise.AuditDataServices.Subscription;
using Enterprise.AuditDataServices.Telematics;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	TelematicsSubscriberServiceTask.Code,
	TelematicsSubscriberServiceTask.Description,
	typeof(TelematicsSubscriberServiceTaskQueue))]
namespace Enterprise.AuditDataServices.Telematics
{
	public class TelematicsSubscriberServiceTaskQueue : AuditSubscriberTaskQueue
	{
		protected override AuditSubscriberTask GetAuditSubscriberServiceTask()
		{
			return new TelematicsSubscriberServiceTask();
		}
	}
}
