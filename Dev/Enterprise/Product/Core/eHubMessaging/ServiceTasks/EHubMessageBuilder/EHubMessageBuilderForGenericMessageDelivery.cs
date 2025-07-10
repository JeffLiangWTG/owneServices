using System.IO;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.IO;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForGenericMessageDelivery : EHubMessageBuilder
	{
		public EHubMessageBuilderForGenericMessageDelivery(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override IeHubMessage BuildCore()
		{
			var settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = true;

			var messageStream = new VirtualMemoryStream();
			var xmlWriter = XmlWriter.Create(messageStream, settings);
			BuildInterchange().WriteTo(xmlWriter);
			xmlWriter.Flush();

			messageStream.Seek(0, SeekOrigin.Begin);

			return new eHubMessage(
			interchange.EI_SessionGUID.ToGuid(),
			interchange.EI_From,
			interchange.EI_To,
			MessageSchemaType.Xml,
			interchange.EI_ApplicationCode,
			schemaName,
			messageStream);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "XML element name")]
		XDocument BuildInterchange()
		{
			string ns = EDIInterchangeTypeList.Descriptions.GenericMessageDelivery;
			ns = ns.Substring(0, ns.LastIndexOf('#'));
			var result = XDocument.Parse($@"<ns0:GenericMessageInterchange xmlns:ns0 = ""{ns}""/>");

			result.Root.Add(
				new XElement(
					"Header",
					new XElement("SenderID", interchange.EI_From),
					new XElement("RecipientID", interchange.EI_To),
					new XElement("InterchangeType", interchange.EI_InterchangeType),
					new XElement("InterchangeNumber", interchange.EI_InterchangeNum)));

			using (var reader = interchange.GetEI_BodyTextReader())
			{
				var bodyText = reader.ReadToEnd();

				if (XmlUtils.IsValidXml(bodyText))
				{
					var bodyXml = XElement.Parse(bodyText);
					result.Root.Add(
						new XElement("Body", bodyXml)
					);
				}
				else
				{
					result.Root.Add(
						new XElement("Body", bodyText)
					);
				}
			}

			return result;
		}

		protected override bool RequiresMessage()
		{
			return interchange.EI_InterchangeType != EDIInterchangeTypeList.Codes.RefDataRepoMessage;
		}
	}
}
