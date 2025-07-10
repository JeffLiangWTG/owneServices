using System.IO;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.eHub.Common.Extensions;
using CargoWise.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForNZCustomsWCO : EHubMessageBuilderForNZCustoms
	{
		public EHubMessageBuilderForNZCustomsWCO(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override bool RequiresMessage()
		{
			return true;
		}

		protected override Stream GetContentStream()
		{
			Stream stream = new VirtualMemoryStream();
			var writer = XmlTextWriter.Create(stream, new XmlWriterSettings() { OmitXmlDeclaration = true });

			writer.WriteStartElement("Documents", "http://cargowise.com/ehub/products/xmlwithattachments");

			//Add Declaration
			writer.WriteStartElement("Document");
			writer.WriteElementString("DocumentType", "DEC");
			writer.WriteElementString("ContentType", "Xml");
			writer.WriteElementString("FileName", "Declaration.xml");
			writer.WriteStartElement("Content");

			using (var reader = interchange.GetEI_BodyTextReader())
			{
				using (var messageStream = reader.GetMessageStream())
				{
					using (var comressedAndEncodedSream = messageStream.CompressAndEncode())
					{
						var comressedAndEncodedReader = new StreamReader(comressedAndEncodedSream);
						comressedAndEncodedReader.WriteToXmlWriter(writer);
					}
				}
			}

			writer.WriteEndElement();
			writer.WriteEndElement();

			//Add attachments
			var message = interchange.ContainedMessages[0];

			foreach (EDIMessageAttach attachment in message.MessageAttachments)
			{
				var storageDocs = attachment.GetAttachment();

				if (storageDocs != null)
				{
					writer.WriteStartElement("Document", "http://cargowise.com/ehub/products/xmlwithattachments");
					writer.WriteElementString("DocumentType", attachment.EG_EdiMsgDocType);

					writer.WriteElementString("ContentType", storageDocs.DataType);

					writer.WriteElementString("FileName", attachment.EG_FileName);
					writer.WriteStartElement("Content");

					using (var supportingDocumentStream = storageDocs.GetImageDataReader())
					{
						using (var comressedAndEncodedSream = supportingDocumentStream.CompressAndEncode())
						{
							var comressedAndEncodedReader = new StreamReader(comressedAndEncodedSream);
							comressedAndEncodedReader.WriteToXmlWriter(writer);
						}
					}

					writer.WriteEndElement();
					writer.WriteEndElement();
				}
			}

			writer.WriteEndElement();

			writer.Flush();
			stream.SeekBegin();

			return stream;
		}

		protected override string Authentication
		{
			get
			{
				return interchange.EI_FooterText;
			}
		}

		protected override NZCustomsMessageContentType ContentType
		{
			get { return NZCustomsMessageContentType.XmlWithAttachments; }
		}
	}
}
