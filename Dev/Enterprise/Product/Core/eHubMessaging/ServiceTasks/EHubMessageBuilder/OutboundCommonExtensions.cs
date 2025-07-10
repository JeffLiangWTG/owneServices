using System.IO;
using System.Xml;
using System.Xml.XPath;
using CargoWise.ComponentModel;
using CargoWise.eHub.Common.Extensions;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public static class OutboundCommonExtensions
	{
		public static void WriteElementStream(this XmlWriter writer, string elementName, TextReader reader)
		{
			writer.WriteStartElement(elementName);
			writer.WriteRaw("<![CDATA[");
			reader.WriteToXmlWriter(writer);
			writer.WriteRaw("]]>");
			writer.WriteEndElement();
		}

		public static Stream GetMessageStream(this TextReader reader)
		{
			const int bufferSize = 32000;

			var stream = new VirtualMemoryStream();
			var writer = new StreamWriter(stream);

			char[] buffer = new char[bufferSize];
			int position = reader.Read(buffer, 0, buffer.Length);
			while (position > 0)
			{
				writer.Write(buffer, 0, position);
				position = reader.Read(buffer, 0, buffer.Length);
			}

			writer.Flush();
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}

		public static ZString GetXPathVaue(this XPathDocument xPathDoc, string xPath)
		{
			var result = ZString.Empty;
			var iterator = xPathDoc.CreateNavigator().Select(xPath);
			if (iterator.MoveNext())
			{
				result = iterator.Current.Value;
			}
			return result;
		}

		public static Stream JoinMessagePartsStream(TextReader headerReader, TextReader bodyReader, TextReader footerReader)
		{
			var stream = new VirtualMemoryStream();
			var writer = new StreamWriter(stream);

			writer.AddStream(headerReader);
			writer.AddStream(bodyReader);
			writer.AddStream(footerReader);

			writer.Flush();
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}

		public static void AddEHubError(this EDIInterchange interchange, string message)
		{
			interchange.Notes.AddNew(true, Res.GetString("82FA5258-FD6D-41D2-97D6-D40DECCE2DB9", "eHub Error"), message);
		}

		public static void AddEHubError(this INotifications notifier, string message)
		{
			notifier.Notify(new WarningNotification(message));
		}
	}
}
