using System.IO;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IXmlEventDeserializer
	{
		IXmlEventValueObject Parse(TextReader eventXmlReader, IXmlImportLogger logger);
	}

	public static class IXmlEventDeserializerExtension
	{
		public static IXmlEventValueObject Parse(this IXmlEventDeserializer deserializer, string xmlString)
		{
			using (var stringReader = new StringReader(xmlString))
			{
				return deserializer.Parse(stringReader, new DummyLogger());
			}
		}
	}
}
