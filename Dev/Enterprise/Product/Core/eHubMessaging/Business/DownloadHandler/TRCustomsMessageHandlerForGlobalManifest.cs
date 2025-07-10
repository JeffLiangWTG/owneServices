using System;
using System.Text;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.TRCustomsGlobalManifest)]
	public class TRCustomsMessageHandlerForGlobalManifest : MessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.TRCustoms;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.TRCustomsGlobalManifest;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;

			Message.MessageStream.SeekBegin();

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(Message.MessageStream);

			var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
			nsmgr.AddNamespace((NoResString)"ns", "http://cargowise.com/xhub/products/TRCustoms");

			var bodyNode = xmlDoc.SelectSingleNode("//ns:MessageBodyBase64", nsmgr);
			interchange.EI_BodyText = Encoding.UTF8.GetString(Convert.FromBase64String(bodyNode.InnerXml));

			return interchange;
		}
	}
}
