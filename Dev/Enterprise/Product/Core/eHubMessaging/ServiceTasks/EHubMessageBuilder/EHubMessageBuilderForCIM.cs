using CargoWise.ComponentModel;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForCIM : EHubMessageBuilderForEdifact
	{
		public EHubMessageBuilderForCIM(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override bool RequiresMessage()
		{
			return true;
		}
	}
}
