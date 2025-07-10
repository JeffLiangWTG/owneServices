using System.Xml;
using CargoWise.eHub.Common.Extensions;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.ESCustoms)]
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.ESCustomsError)]
	class ESCustomsMessageHandler : MessageHandler
	{
		const string ErrorInterchangeTypeCode = "ERR";

		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;

			Message.MessageStream.SeekBegin();
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(Message.MessageStream);

			var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
			nsmgr.AddNamespace("ns0", "http://cargowise.com/xhub/products/ESCustoms");

			var headerNode = xmlDoc.SelectSingleNode("//Headers", nsmgr);
			var bodyNode = xmlDoc.SelectSingleNode("//Body", nsmgr);

			var interchangeTypeNode = headerNode.SelectSingleNode("InterchangeType");
			headerNode.RemoveChild(interchangeTypeNode);

			var interchangeType = Message.SchemaName == EDIInterchangeTypeList.Descriptions.ESCustomsError
				? ErrorInterchangeTypeCode
				: interchangeTypeNode.InnerText;

			interchange.EI_BodyText = bodyNode.InnerXml;
			interchange.EI_HeaderText = headerNode.OuterXml;
			interchange.EI_InterchangeType = interchangeType;

			return interchange;
		}
	}
}
