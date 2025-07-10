using CargoWise.eHub.Common.Extensions;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.CustomsWare)]
	class CustomsWareMessageHandler : MessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.CustomsWare;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.CustomsWare;
			Message.MessageStream.SeekBegin();
			interchange.SetEI_BodyTextOrDataSource(Message.MessageStream);
			interchange.EI_Status = EDIInterchange.Status.Queued;
			return interchange;
		}
	}
}
