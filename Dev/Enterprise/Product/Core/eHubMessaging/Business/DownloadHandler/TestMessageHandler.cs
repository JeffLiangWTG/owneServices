using System.IO;
using CargoWise.eHub.Common.Extensions;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.TST)]
	class TestMessageHandler : MessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.TST;
			Message.MessageStream.SeekBegin();
			interchange.EI_BodyText = new StreamReader(Message.MessageStream).ReadToEnd();
			interchange.EI_Status = EDIInterchange.Status.Received;
			return interchange;
		}
	}
}
