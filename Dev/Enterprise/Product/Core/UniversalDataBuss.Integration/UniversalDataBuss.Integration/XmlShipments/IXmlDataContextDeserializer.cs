using System.IO;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IXmlDataContextDeserializer
	{
		IDataContextDataObject Parse(TextReader xmlReader, IXmlImportLogger logger);
	}
}
