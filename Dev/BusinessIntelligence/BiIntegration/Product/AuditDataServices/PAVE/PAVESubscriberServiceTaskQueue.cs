using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	Enterprise.AuditDataServices.PAVE.PAVESubscriberServiceTask.Code,
	Enterprise.AuditDataServices.PAVE.PAVESubscriberServiceTask.Description,
	typeof(Enterprise.AuditDataServices.PAVE.PAVESubscriberServiceTaskQueue))]

namespace Enterprise.AuditDataServices.PAVE
{
	public class PAVESubscriberServiceTaskQueue : AuditSubscriberTaskQueue
	{
		protected override AuditSubscriberTask GetAuditSubscriberServiceTask() => new PAVESubscriberServiceTask();
	}
}
