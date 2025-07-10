using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.Configuration)]
	public class ConfigurationMessageHandler : MessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			interchange.EI_ApplicationCode = GetApplicationCode(interchange.EI_From);
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.Configuration;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.SetEI_BodyTextSource(new StreamReaderSource(Message.MessageStream));

			return interchange;
		}

		string GetApplicationCode(string senderId)
		{
			switch (senderId)
			{
				case "XHUB_OFX_CONFIG":
					return ApplicationCodeList.Codes.OFX;
				default:
					return ApplicationCodeList.Codes.eHub;
			}
		}
	}
}
