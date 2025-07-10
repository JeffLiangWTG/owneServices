using System;
using System.IO;
using System.Text;
using System.Xml;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.ES.Business
{
	public static class XmlUtils
	{
		public static string SerializeToString(object obj)
		{
			using (var stringWriter = new StringWriter())
			{
				var writerSettings = new XmlWriterSettings()
				{
					Indent = true,
					Encoding = new UTF8Encoding(false),
					NewLineChars = "\r\n",
					OmitXmlDeclaration = true
				};

				using (var writer = XmlWriter.Create(stringWriter, writerSettings))
				{
					var serializer = ZXmlSerializer.New(obj.GetType());
					serializer.Serialize(writer, obj);
				}

				return stringWriter.ToString();
			}
		}

		public static object Deserialize(Type type, byte[] xml)
		{
			using (var ms = new MemoryStream(xml))
			{
				var serializer = ZXmlSerializer.New(type);
				var obj = serializer.Deserialize(ms);

				return obj;
			}
		}
	}
}
