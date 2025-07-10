using System.IO;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForAUCustoms : EHubMessageBuilder
	{
		public EHubMessageBuilderForAUCustoms(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override IeHubMessage BuildCore()
		{
			Stream messageStream = new VirtualMemoryStream();
			var writer = XmlTextWriter.Create(messageStream, new XmlWriterSettings() { OmitXmlDeclaration = true });
			writer.WriteStartElement("AUCustoms", "http://cargowise.com/ehub/products/");
			writer.WriteElementString("Reference", interchange.EI_From);
			writer.WriteStartElement("Content");

			using (var reader = interchange.GetEI_BodyTextReader())
			{
				using (var stream = reader.GetMessageStream())
				{
					using (var encodedStream = stream.EncodeStream())
					{
						encodedStream.SeekBegin();
						var encodedStreamReader = new StreamReader(encodedStream);
						encodedStreamReader.WriteToXmlWriter(writer);
					}
				}
			}

			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.Flush();

			return new eHubMessage(
				interchange.EI_SessionGUID.ToGuid(),
				interchange.EI_From,
				"AUCustoms",
				MessageSchemaType.Xml,
				interchange.EI_ApplicationCode,
				schemaName,
				messageStream);
		}
	}
}
