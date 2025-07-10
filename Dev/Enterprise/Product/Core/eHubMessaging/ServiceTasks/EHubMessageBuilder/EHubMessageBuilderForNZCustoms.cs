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
	public abstract class EHubMessageBuilderForNZCustoms : EHubMessageBuilder
	{
		public EHubMessageBuilderForNZCustoms(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override IeHubMessage BuildCore()
		{
			Stream stream = new VirtualMemoryStream();
			var writer = XmlTextWriter.Create(stream, new XmlWriterSettings() { OmitXmlDeclaration = true });
			writer.WriteStartElement("NZCustoms", "http://cargowise.com/ehub/products/");
			writer.WriteElementString("Reference", interchange.EI_From);
			writer.WriteElementString("Type", ContentType.ToString());
			writer.WriteElementString("Authentication", Authentication);

			Stream contentStream = GetContentStream();

			if (contentStream == null)
			{
				interchange.Notes.AddRowError(Res.GetString("26bafb4d-7dec-4415-a9d9-2e24115eb6c7", "Message content is empty. Nothing to send."));
				stream.Dispose();
				return null;
			}
			else
			{
				writer.WriteStartElement("Content");

				using (contentStream)
				{
					using (var encodedStream = contentStream.EncodeStream())
					{
						encodedStream.SeekBegin();
						var encodedStreamReader = new StreamReader(encodedStream);
						encodedStreamReader.WriteToXmlWriter(writer);
					}
				}

				writer.WriteEndElement();
			}

			writer.WriteEndElement();
			writer.Flush();

			return new eHubMessage(
				interchange.EI_SessionGUID.ToGuid(),
				interchange.EI_From,
				GetRecipientID(interchange.EI_To),
				MessageSchemaType.Xml,
				interchange.EI_ApplicationCode,
				schemaName,
				stream);
		}

		protected abstract Stream GetContentStream();

		protected abstract NZCustomsMessageContentType ContentType { get; }

		protected virtual string Authentication
		{
			get
			{
				return string.Empty;
			}
		}

		public enum NZCustomsMessageContentType
		{
			Text,
			Xml,
			XmlWithAttachments
		}

		string GetRecipientID(string interchangeTo)
		{
			if (interchangeTo == EDIInterchange.InterchangePartyIDs.NZCustomsTestMailbox || interchangeTo == EDIInterchange.InterchangePartyIDs.NZMAFeBACCaTestMailbox)
			{
				return "NZCustomsTest";
			}

			return "NZCustoms";
		}
	}
}
