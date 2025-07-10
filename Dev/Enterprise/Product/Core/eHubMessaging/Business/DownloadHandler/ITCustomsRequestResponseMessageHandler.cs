using System.Xml;
using CargoWise.eHub.Common.Extensions;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.ITCustomsRequestResponse)]
	class ITCustomsRequestResponseMessageHandler : MessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();

			Message.MessageStream.SeekBegin();
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(Message.MessageStream);

			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.eHub;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.ITCustomsRequestResponse;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.EI_HeaderText = xmlDoc.OuterXml;
			interchange.SetEI_BodyTextSource(new StreamReaderSource(Message.MessageStream));

			return interchange;
		}
	}
}
