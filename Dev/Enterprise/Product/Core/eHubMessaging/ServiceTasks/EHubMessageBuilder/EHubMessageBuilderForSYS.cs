
using CargoWise.ComponentModel;
using Enterprise.Messaging.Business;
namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForSYS : EHubMessageBuilderForXml
	{
		public EHubMessageBuilderForSYS(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override bool RequiresMessage()
		{
			return false;
		}

		protected override string GetSchemaName()
		{
			return InterchangeHasNoMessages ? Enterprise.eHubMessaging.Business.SystemMessage.SchemaName : base.GetSchemaName();
		}
	}
}
