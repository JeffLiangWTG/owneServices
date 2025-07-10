using System.IO;
using System.Xml;
using System.Xml.Linq;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.IO;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForESCustoms : EHubMessageBuilder
	{
		public EHubMessageBuilderForESCustoms(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override bool RequiresMessage()
		{
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "XML string")]
		protected override IeHubMessage BuildCore()
		{
			var isEdifactMessage = interchange.EI_To.EqualsIgnoringCase("ESCustomsEDIFACT");

			string ns = EDIInterchangeTypeList.Descriptions.ESCustoms;
			ns = ns.Substring(0, ns.LastIndexOf('#'));

			var headerContent = XElement.Parse(interchange.EI_HeaderText);
			headerContent.Add(new XElement("InterchangeType", interchange.EI_InterchangeType));

			var interchangeXml = XDocument.Parse($@"<ns0:ESCustoms xmlns:ns0 = ""{ns}""/>");
			interchangeXml.Root.Add(headerContent);

			using (var reader = interchange.GetEI_BodyTextReader())
			{
				var bodyContent = reader.ReadToEnd();
				if (isEdifactMessage)
				{
					interchangeXml.Root.Add(new XElement("Body", bodyContent));
				}
				else
				{
					interchangeXml.Root.Add(new XElement("Body", XElement.Parse(bodyContent)));
				}
			}

			var settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = true;

			var messageStream = new VirtualMemoryStream();
			var xmlWriter = XmlWriter.Create(messageStream, settings);
			interchangeXml.WriteTo(xmlWriter);
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
	}
}
