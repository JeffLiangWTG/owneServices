using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.USDISReply)]
	class USDISReplyHandler : MessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.USCustomsDIS;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.USDISReply;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.SetEI_BodyTextSource(new StreamReaderSource(Message.MessageStream));

			return interchange;
		}
	}
}
