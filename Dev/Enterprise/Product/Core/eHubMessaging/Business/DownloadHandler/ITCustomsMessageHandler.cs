using System.Xml;
using CargoWise.eHub.Common.Extensions;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.ITCustoms)]
	class ITCustomsMessageHandler : MessageHandler
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "xml element name")]
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.ITCustoms;

			Message.MessageStream.SeekBegin();
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(Message.MessageStream);

			var bodyNode = xmlDoc.GetElementsByTagName("Message")[0];
			var interchangeTypeNode = xmlDoc.GetElementsByTagName("MessageType")[0];

			if (bodyNode.FirstChild != null && bodyNode.FirstChild.NodeType != XmlNodeType.Text)
			{
				interchange.EI_BodyText = bodyNode.InnerXml;
			}
			else
			{
				interchange.EI_BodyText = bodyNode.InnerText;
			}
			xmlDoc.DocumentElement.RemoveChild(bodyNode);
			interchange.EI_HeaderText = xmlDoc.OuterXml;
			interchange.EI_InterchangeType = interchangeTypeNode.InnerText;

			return interchange;
		}
	}
}
