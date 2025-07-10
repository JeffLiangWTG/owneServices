using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common.Extensions;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.TWCustoms)]
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.TWCustomsForwarderManifest)]
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.TWCustomsLicensing)]
	public class TWCustomsMessageHandler : MessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.TWCustoms;

			switch (Message.SchemaName)
			{
				case EDIInterchangeTypeList.Descriptions.TWCustomsForwarderManifest:
					interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.TWCustomsForwarderManifest;
					break;
				case EDIInterchangeTypeList.Descriptions.TWCustomsLicensing:
					interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.TWCustomsLicensing;
					break;
				default:
					interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.TWCustoms;
					break;
			}

			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;

			Message.MessageStream.SeekBegin();

			var doc = XDocument.Load(Message.MessageStream, LoadOptions.None);
			var bodyNode = doc.XPathSelectElement((NoResString)"//*[local-name()='Response']");
			var interchangeNum = doc.XPathSelectElement((NoResString)"//*[local-name()='InterchangeNum']");
			var twDateTime = ZDateTime.UtcNow.AddHours(8).ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture);
			if (bodyNode != null)
			{
				interchange.EI_BodyText = bodyNode.ToString();
			}
			interchange.NumberStrategy = new GenericMessageNumberStrategy(interchange.Factory, interchangeNum.Value + twDateTime);

			return interchange;
		}
	}
}
