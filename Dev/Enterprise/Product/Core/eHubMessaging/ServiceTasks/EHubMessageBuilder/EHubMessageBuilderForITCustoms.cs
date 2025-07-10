using System.IO;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.IO;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForITCustoms : EHubMessageBuilder
	{
		public EHubMessageBuilderForITCustoms(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override bool RequiresMessage()
		{
			return false;
		}

		protected override IeHubMessage BuildCore()
		{
			string ns = EDIInterchangeTypeList.Descriptions.ITCustoms;
			ns = ns.Substring(0, ns.LastIndexOf('#'));

			var interchangeXml = new XmlDocument();
			interchangeXml.LoadXml(interchange.EI_HeaderText);

			interchangeXml.DocumentElement.SetAttribute("xmlns:ns", ns);
			var body = interchangeXml.CreateElement((NoResString)"Body");

			if (!string.IsNullOrEmpty(interchange.EI_BodyText) && interchange.EI_BodyText.TrimStart().StartsWith("<"))
			{
				body.InnerXml = interchange.EI_BodyText;
			}
			else
			{
				body.InnerText = interchange.EI_BodyText;
			}

			interchangeXml.DocumentElement.AppendChild(body);

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
