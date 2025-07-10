using System.IO;
using System.Text;
using System.Xml;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	class USCustomsMessageHandler : MessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			interchange.EI_ApplicationCode = string.Empty;
			interchange.EI_InterchangeType = string.Empty;
			interchange.EI_Status = EDIInterchange.Status.Queued;

			Message.MessageStream.Position = 0;
			var reader = new XmlTextReader(Message.MessageStream) { Normalization = false };

			var headerStream = GetElementStream(reader, (NoResString)"Header");
			var bodyStream = GetElementStream(reader, (NoResString)"Body");
			var footerStream = GetElementStream(reader, (NoResString)"Footer");

			interchange.SetEI_HeaderTextSource(new TextReaderSource(headerStream));
			interchange.SetEI_BodyTextSource(new TextReaderSource(bodyStream));
			interchange.SetEI_FooterTextSource(new TextReaderSource(footerStream));

			return interchange;
		}

		Stream GetElementStream(XmlReader reader, string name)
		{
			reader.ReadToFollowing(name);
			reader.Read();
			var result = new MemoryStream(Encoding.UTF8.GetBytes(reader.ReadContentAsString()));
			return result;
		}
	}
}
