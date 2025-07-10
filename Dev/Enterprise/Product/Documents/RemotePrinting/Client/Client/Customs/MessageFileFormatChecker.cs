using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Enterprise.RemotePrinting.Client
{
	sealed class MessageFileFormatChecker
	{
		public MessageFileFormatChecker(INotifications notifications)
		{
			this.notifications = notifications;
		}

		readonly INotifications notifications;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		public bool CheckIsZipCompressedData(byte[] data)
		{
			var result = data != null && data.Length >= 4 && BitConverter.ToInt32(data, 0) == 0x04034b50;
			if (!result)
			{
				Log("Unable to parse the message content into a ZIP file.");
			}

			return result;
		}

		public bool CheckIsXMLDocument(string content)
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
			return CheckIsXMLDocument(stream);
		}

		public bool CheckIsXMLDocument(Stream stream)
		{
			var result = false;

			stream?.Seek(0, SeekOrigin.Begin);

			try
			{
				var document = new XmlDocument();
				document.Load(stream);

				result = true;
			}
			catch (XmlException ex)
			{
				Log($"Unable to parse the message content into an XML document. Error: {ex.Message}");
			}

			return result;
		}

		void Log(string message)
		{
			notifications?.AddMessage(message);
		}
	}
}
