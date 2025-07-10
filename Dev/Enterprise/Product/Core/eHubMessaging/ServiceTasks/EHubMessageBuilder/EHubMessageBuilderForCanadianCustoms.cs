using System.IO;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.IO;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForCanadianCustoms : EHubMessageBuilder
	{
		public EHubMessageBuilderForCanadianCustoms(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override IeHubMessage BuildCore()
		{
			Stream messageStream = new VirtualMemoryStream();
			var writer = XmlWriter.Create(messageStream, new XmlWriterSettings() { OmitXmlDeclaration = true });
			writer.WriteStartElement("CACustoms", "http://cargowise.com/ehub/products/canadiancustoms");
			writer.WriteElementString("Reference", string.Format("{0} - {1}", interchange.EI_From, interchange.EI_To));
			writer.WriteElementString("MessageId", interchange.EI_SessionGUID.ToString());
			writer.WriteStartElement("Content");

			using (var readerBody = interchange.GetEI_BodyTextReader())
			{
				using (var readerHeader = interchange.GetEI_HeaderTextReader())
				{
					using (var readerFooter = interchange.GetEI_FooterTextReader())
					{
						using (var combinedStream = OutboundCommonExtensions.JoinMessagePartsStream(readerHeader, readerBody, readerFooter))
						{
							using (var encodedStream = combinedStream.EncodeStream())
							{
								encodedStream.SeekBegin();
								var encodedStreamReader = new StreamReader(encodedStream);
								encodedStreamReader.WriteToXmlWriter(writer);
							}
						}
					}
				}
			}

			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.Flush();

			return new eHubMessage(
				interchange.EI_SessionGUID.ToGuid(),
				interchange.EI_From,
				CanadianCustomsReplyHandler.GetCanadianCustomsEHubId(interchange.EI_To),
				MessageSchemaType.Xml,
				EDIInterchange.ApplicationCodes.CACustoms,
				schemaName,
				messageStream,
				interchange.EI_InterchangeType,
				null);
		}
	}
}
