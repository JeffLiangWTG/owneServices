using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Customs.CA.Services
{
	public static class ValidateTransactionExtensions
	{
		public static byte[] Serialize(this IValueObject request)
		{
			MemoryStream stream = null;
			try
			{
				stream = new MemoryStream();
				StreamWriter textWriter = null;
				try
				{
					textWriter = new StreamWriter(stream);
					stream = null;
					using (var writer = new XmlTextWriter(textWriter))
					{
						textWriter = null;
						writer.Formatting = Formatting.Indented;
						var serializer = new XmlValueObjectSerializer(request.GetType());
						serializer.Serialize(writer, request, new XmlSerializerNamespaces(new XmlQualifiedName[] { new XmlQualifiedName("", "") }));
						writer.Flush();
						return ((MemoryStream)writer.BaseStream).ToArray();
					}
				}
				finally
				{
					if (textWriter != null)
					{
						textWriter.Dispose();
						stream = null;
					}
				}
			}
			finally
			{
				if (stream != null)
				{
					stream.Dispose();
				}
			}
		}

		public static T Deserialize<T>(this byte[] data)
		{
			var xml = new string(System.Text.Encoding.UTF8.GetChars(data));
			StringReader stringReader = null;

			try
			{
				stringReader = new StringReader(xml);
				using (XmlReader xmlReader = XmlReader.Create(stringReader))
				{
					stringReader = null;
					var serializer = new XmlValueObjectSerializer(typeof(T));
					return (T)serializer.Deserialize(xmlReader);
				}
			}
			finally
			{
				if (stringReader != null)
				{
					stringReader.Dispose();
				}
			}
		}
	}
}
