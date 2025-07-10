using System.IO;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.IO;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForUSCustoms : EHubMessageBuilder
	{
		public EHubMessageBuilderForUSCustoms(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override bool FailIfNoSchemaNameFound()
		{
			return false;
		}

		protected override IeHubMessage BuildCore()
		{
			Stream stream = new VirtualMemoryStream();
			using (var readerBody = interchange.GetEI_BodyTextReader())
			{
				using (var readerHeader = interchange.GetEI_HeaderTextReader())
				{
					using (var readerFooter = interchange.GetEI_FooterTextReader())
					{
						var writer = XmlTextWriter.Create(stream, new XmlWriterSettings() { OmitXmlDeclaration = true });
						writer.WriteStartElement("USCustoms");

						writer.WriteElementStream((NoResString)"Header", readerHeader);
						writer.WriteElementStream((NoResString)"Body", readerBody);
						writer.WriteElementStream((NoResString)"Footer", readerFooter);

						writer.WriteEndElement();
						writer.Flush();
					}
				}
			}

			return new eHubMessage(
				interchange.EI_SessionGUID.ToGuid(),
				interchange.EI_From,
				interchange.EI_To,
				MessageSchemaType.FlatFile,
				interchange.EI_ApplicationCode,
				schemaName,
				stream,
				string.Empty,
				string.Empty);
		}
	}
}
