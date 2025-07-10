using System.IO;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForEdifact : EHubMessageBuilder
	{
		public EHubMessageBuilderForEdifact(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override IeHubMessage BuildCore()
		{
			Stream stream = null;
			using (var readerBody = interchange.GetEI_BodyTextReader())
			{
				using (var readerHeader = interchange.GetEI_HeaderTextReader())
				{
					using (var readerFooter = interchange.GetEI_FooterTextReader())
					{
						stream = OutboundCommonExtensions.JoinMessagePartsStream(readerHeader, readerBody, readerFooter);
					}
				}
			}

			return new eHubMessage(
				interchange.EI_SessionGUID.ToGuid(),
				interchange.EI_From,
				interchange.EI_To,
				MessageSchemaType.FlatFile,
				interchange.EI_ApplicationCode,
				schemaName,
				stream,
				string.Empty,
				string.Empty);
		}
	}
}
