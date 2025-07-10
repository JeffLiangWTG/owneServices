using System.IO;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Management.EventProcessing
{
	public class XmlEventDeserializer : IXmlEventDeserializer
	{
		IXmlEventValueObject IXmlEventDeserializer.Parse(TextReader eventXmlReader, IXmlImportLogger logger)
		{
			return eventXmlReader.Parse<UniversalEvent>(logger);
		}
	}
}