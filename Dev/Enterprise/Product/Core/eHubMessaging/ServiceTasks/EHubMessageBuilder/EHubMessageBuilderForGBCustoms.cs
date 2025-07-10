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
	public class EHubMessageBuilderForGBCustoms : EHubMessageBuilder
	{
		public EHubMessageBuilderForGBCustoms(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "XML string")]
		protected override IeHubMessage BuildCore()
		{
			string ns = EDIInterchangeTypeList.Descriptions.GBCustoms;
			ns = ns.Substring(0, ns.LastIndexOf('#'));

			XElement bodyContentXElement;
			string bodyNotAsXml = "";
			using (var reader = interchange.GetEI_BodyTextReader())
			{
				try
				{
					bodyNotAsXml = reader.ReadToEnd();
					bodyContentXElement = XElement.Parse(bodyNotAsXml);
				}
				catch (XmlException)
				{
					bodyContentXElement = null;
				}
			}

			var interchangeXml = XDocument.Parse($@"<ns0:GBCustoms xmlns:ns0 = ""{ns}""/>");

			var bodyElement = bodyContentXElement != null ? new XElement("Body", bodyContentXElement)
															: new XElement("Body", bodyNotAsXml);
			interchangeXml.Root.Add(
				new XElement("Header", XElement.Parse(interchange.EI_HeaderText)),
				bodyElement);

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
