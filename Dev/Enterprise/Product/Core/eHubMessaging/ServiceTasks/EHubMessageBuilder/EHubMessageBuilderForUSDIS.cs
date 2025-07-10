using System.IO;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForUSDIS : EHubMessageBuilder
	{
		public EHubMessageBuilderForUSDIS(EDIInterchange ediInterchange, INotifications notifications)
			: base(ediInterchange, notifications)
		{
		}

		protected override bool RequiresMessage()
		{
			return false;
		}

		protected override IeHubMessage BuildCore()
		{
			using (var readerBody = interchange.GetEI_BodyTextReader())
			{
				var messageStream = readerBody.GetMessageStream();

				return new eHubMessage(
				interchange.EI_SessionGUID.ToGuid(),
				interchange.EI_From,
				"USDIS",
				MessageSchemaType.Xml,
				interchange.EI_ApplicationCode,
				schemaName,
				ModifyMessageStream(messageStream));
			}
		}

		protected virtual Stream ModifyMessageStream(Stream messageStream)
		{
			return messageStream;
		}
	}
}
