using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	Enterprise.AuditDataServices.Glow.GlowSubscriberServiceTask.Code,
	Enterprise.AuditDataServices.Glow.GlowSubscriberServiceTask.Description,
	typeof(Enterprise.AuditDataServices.Glow.GlowSubscriberServiceTaskQueue))]
namespace Enterprise.AuditDataServices.Glow
{
	public class GlowSubscriberServiceTaskQueue : AuditSubscriberTaskQueue
	{
		protected override AuditSubscriberTask GetAuditSubscriberServiceTask()
		{
			return new GlowSubscriberServiceTask();
		}
	}
}
