using System;
using System.IO;
using System.Text;
using System.Xml.XPath;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.IO;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageBuilderForXml : EHubMessageBuilder
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String is an XML fragment")]
		public const string XmlDeclaration = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

		public EHubMessageBuilderForXml(EDIInterchange interchange, INotifications notifier)
			: base(interchange, notifier)
		{
		}

		protected override IeHubMessage BuildCore()
		{
			string fileName = string.Empty;
			string emailSubject = String.Empty;

			if (!interchange.EI_HeaderText.IsEmpty)
			{
				var xPathDoc = new XPathDocument(new MemoryStream(Encoding.UTF8.GetBytes(interchange.EI_HeaderText)));
				fileName = xPathDoc.GetXPathVaue("/EDIDelivery/FileName");
				emailSubject = xPathDoc.GetXPathVaue("/EDIDelivery/EmailSubject");
			}

			var stream = GetMessageStream();

			if (stream.Length == 0)
			{
				LogInterchangeError(Res.GetString("DF2CF15A-18BF-4335-ACC4-C429EFE690C7", "Cannot create {0} message: message content is empty.", interchange.TransportModeDescription));
				return null;
			}

			return new eHubMessage(
				interchange.EI_SessionGUID.ToGuid(),
				interchange.EI_From,
				interchange.EI_To,
				MessageSchemaType.Xml,
				interchange.EI_ApplicationCode,
				schemaName,
				stream,
				emailSubject,
				fileName);
		}

		Stream GetMessageStream()
		{
			const int bufferSize = 32000;

			var stream = new VirtualMemoryStream();
			var writer = new StreamWriter(stream);

			using (var reader = interchange.GetEI_BodyTextReader())
			{
				CheckIfItIsXmlDocumentAndDeclarationDoesNotExistsThanAdd(reader, writer);

				var buffer = new char[bufferSize];
				int position;
				while ((position = reader.Read(buffer, 0, buffer.Length)) > 0)
				{
					writer.Write(buffer, 0, position);
				}
			}

			writer.Flush();
			stream.SeekBegin();
			return stream;
		}

		static void CheckIfItIsXmlDocumentAndDeclarationDoesNotExistsThanAdd(TextReader reader, StreamWriter writer)
		{
			const int declarationCheckSize = 5;

			var buffer = new char[declarationCheckSize];
			var position = reader.Read(buffer, 0, declarationCheckSize);
			var first5Chars = new string(buffer, 0, position);

			if (first5Chars.Length > 0 && first5Chars.TrimStart()[0] == '<' && !String.Equals(first5Chars, XmlDeclaration.Substring(0, 5), StringComparison.CurrentCultureIgnoreCase))
			{
				writer.Write(XmlDeclaration);
			}

			writer.Write(buffer, 0, position);
		}
	}
}
