using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	Enterprise.AuditDataServices.Accounting.GeneralLedgerDataSubscriberServiceTask.Code,
	Enterprise.AuditDataServices.Accounting.GeneralLedgerDataSubscriberServiceTask.Description,
	typeof(Enterprise.AuditDataServices.Accounting.GeneralLedgerDataSubscriberServiceTaskQueue))]
namespace Enterprise.AuditDataServices.Accounting
{
	public class GeneralLedgerDataSubscriberServiceTaskQueue : AuditSubscriberTaskQueue
	{
		protected override AuditSubscriberTask GetAuditSubscriberServiceTask()
		{
			return new GeneralLedgerDataSubscriberServiceTask();
		}
	}
}
