using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.IT.Business;

public static class XmlExtensions
{
	public static T DeserializeToObject<T>(this ZString xmlString) where T : class
	{
		Argument.NotNullOrEmpty(xmlString, nameof(xmlString));
		using (var stringReader = new StringReader(xmlString))
		{
			var serializer = ZXmlSerializer.New(typeof(T));
			return (T)serializer.Deserialize(stringReader);
		}
	}

	public static ZString SerializeToXml<T>(this T objectToSerialize, XmlWriterSettings xmlWriterSettings = null) where T : class
	{
		Argument.NotNull(objectToSerialize, nameof(objectToSerialize));

		using (var memoryStream = new MemoryStream())
		using (var writer = XmlWriter.Create(memoryStream, xmlWriterSettings))
		{
			var namespaces = new XmlSerializerNamespaces();
			namespaces.Add(ZString.Empty, ZString.Empty);

			var serializer = ZXmlSerializer.New(typeof(T));
			serializer.Serialize(writer, objectToSerialize, namespaces);
			return Encoding.Default.GetString(memoryStream.ToArray());
		}
	}
}
