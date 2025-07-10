using System.IO;
using CargoWise.ComponentModel;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForNZCustomsLegacy : EHubMessageBuilderForNZCustoms
	{
		public EHubMessageBuilderForNZCustomsLegacy(EDIInterchange interchange, INotifications notifier)
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
				using (var readerHeader = interchange.GetEI_HeaderTextReader())
				{
					using (var readerFooter = interchange.GetEI_FooterTextReader())
					{
						return OutboundCommonExtensions.JoinMessagePartsStream(readerHeader, readerBody, readerFooter);
					}
				}
			}
		}

		protected override NZCustomsMessageContentType ContentType
		{
			get { return NZCustomsMessageContentType.Text; }
		}
	}
}
