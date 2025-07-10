using CargoWise.IO;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IXmlReader
	{
		void ReadXML(IDataObject dataStructure, SubStreamableStream inputStream, IXmlImportLogger logger);
		string Namespace { get; }
	}
}
