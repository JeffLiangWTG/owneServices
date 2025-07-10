using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	Enterprise.AuditDataServices.DeviceManagement.DeviceManagementSubscriberServiceTask.Code,
	Enterprise.AuditDataServices.DeviceManagement.DeviceManagementSubscriberServiceTask.Description,
	typeof(Enterprise.AuditDataServices.DeviceManagement.DeviceManagementSubscriberServiceTaskQueue))]
namespace Enterprise.AuditDataServices.DeviceManagement
{
	public class DeviceManagementSubscriberServiceTaskQueue : AuditSubscriberTaskQueue
	{
		protected override AuditSubscriberTask GetAuditSubscriberServiceTask()
		{
			return new DeviceManagementSubscriberServiceTask();
		}
	}
}
