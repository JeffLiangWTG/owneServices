using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	Enterprise.AuditDataServices.HVLV.HVLVSubscriberServiceTask.Code,
	Enterprise.AuditDataServices.HVLV.HVLVSubscriberServiceTask.Description,
	typeof(Enterprise.AuditDataServices.HVLV.HVLVSubscriberServiceTaskQueue))]
namespace Enterprise.AuditDataServices.HVLV
{
	public class HVLVSubscriberServiceTaskQueue : AuditSubscriberTaskQueue
	{
		protected override AuditSubscriberTask GetAuditSubscriberServiceTask()
		{
			return new HVLVSubscriberServiceTask();
		}
	}
}
