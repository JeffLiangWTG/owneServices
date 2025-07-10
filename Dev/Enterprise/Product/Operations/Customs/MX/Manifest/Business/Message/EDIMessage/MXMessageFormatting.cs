using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public static class MXMessageFormatting
	{
		public static ZString FormatWithXMLRepresentation(string messageText)
		{
			ZString result = "";
			using (var mStream = new MemoryStream())
			{
				using (var writer = new XmlTextWriter(mStream, Encoding.Unicode))
				{
					var document = new XmlDocument();
					try
					{
						document.LoadXml(messageText);
						writer.Formatting = Formatting.Indented;
						document.WriteContentTo(writer);
						writer.Flush();
						mStream.Flush();
						mStream.Position = 0;
						using (var sReader = new StreamReader(mStream))
						{
							result = sReader.ReadToEnd();
						}
					}
					catch (XmlException)
					{
						result = messageText;
					}
					mStream.Close();
				}
			}
			return result;
		}
	}
}
