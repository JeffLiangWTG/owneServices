using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.UYCustoms)]
	public class UYCustomsMessageHandler : MessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UYCustoms;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.UYCustoms;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.SetEI_BodyTextSource(new StreamReaderSource(Message.MessageStream));

			return interchange;
		}
	}
}
