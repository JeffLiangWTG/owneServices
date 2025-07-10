using System.IO;
using CargoWise.ComponentModel;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForNZCustomsEBACCA : EHubMessageBuilderForNZCustoms
	{
		public EHubMessageBuilderForNZCustomsEBACCA(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override bool RequiresMessage()
		{
			return false;
		}

		protected override Stream GetContentStream()
		{
			using (var readerBody = interchange.GetEI_BodyTextReader())
			{
				return readerBody.GetMessageStream();
			}
		}

		protected override NZCustomsMessageContentType ContentType
		{
			get { return NZCustomsMessageContentType.Xml; }
		}
	}
}
