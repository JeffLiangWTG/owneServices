using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Types;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing;

public static class CalculatorTestSerialiser
{
	public static string Serialise<T>(INextRunTimeCalculator value)
	{
		var settings = new XmlWriterSettings
		{
			OmitXmlDeclaration = true,
			CloseOutput = true,
			Encoding = Encoding.UTF8,
		};

		using var stringWriter = new StringWriter();
		using var xmlWriter = XmlWriter.Create(stringWriter, settings);
		var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
		var serializer = new XmlSerializer(typeof(T));
		serializer.Serialize(xmlWriter, value, ns);

		xmlWriter.Close();
		return new ZString(stringWriter.ToString());
	}

	public static T Deserialise<T>(string value)
	{
		var settings = new XmlReaderSettings
		{
			CloseInput = false,
			IgnoreComments = true,
		};

		using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(value));
		using var xmlReader = XmlReader.Create(memoryStream, settings);
		var serializer = new XmlSerializer(typeof(T));
		var calculator = (T)serializer.Deserialize(xmlReader)!;

		return calculator;
	}
}
