using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Enterprise.Client.EDI.Telematics.Tca
{
	static class TcaXmlSerializer
	{
		public static string SerializeToTelematicsRimData<T>(T instance, string schema)
		{
			var namespaces = new XmlSerializerNamespaces();
			namespaces.Add("tde-enr", schema);
			namespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
			return Serialize(instance, namespaces);
		}

		static string Serialize<T>(T instance, XmlSerializerNamespaces ns)
		{
			var settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = "\t",
				OmitXmlDeclaration = false,
				CloseOutput = true,
				Encoding = Encoding.UTF8,
			};

			using (var stringWriter = new Utf8StringWriter())
			using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
			{
				var serializer = new XmlSerializer(typeof(T));
				serializer.Serialize(xmlWriter, instance, ns);
				return stringWriter.ToString();
			}
		}

		class Utf8StringWriter : StringWriter
		{
			public override Encoding Encoding => Encoding.UTF8;
		}
	}
}
