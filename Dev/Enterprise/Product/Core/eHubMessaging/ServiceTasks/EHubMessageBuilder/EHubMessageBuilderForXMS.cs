using CargoWise.ComponentModel;
using Enterprise.Messaging.Business;
namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForXMS : EHubMessageBuilderForXml
	{
		public EHubMessageBuilderForXMS(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override bool RequiresMessage()
		{
			return true;
		}
	}
}
