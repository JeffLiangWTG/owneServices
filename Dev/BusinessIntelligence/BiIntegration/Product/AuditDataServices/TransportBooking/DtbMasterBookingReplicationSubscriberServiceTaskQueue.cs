using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	Enterprise.AuditDataServices.TransportBooking.DtbMasterBookingReplicationSubscriberServiceTask.Code,
	Enterprise.AuditDataServices.TransportBooking.DtbMasterBookingReplicationSubscriberServiceTask.Description,
	typeof(Enterprise.AuditDataServices.TransportBooking.DtbMasterBookingReplicationSubscriberServiceTaskQueue))]
namespace Enterprise.AuditDataServices.TransportBooking
{
	public class DtbMasterBookingReplicationSubscriberServiceTaskQueue : AuditSubscriberTaskQueue
	{
		protected override AuditSubscriberTask GetAuditSubscriberServiceTask() => new DtbMasterBookingReplicationSubscriberServiceTask();
	}
}
