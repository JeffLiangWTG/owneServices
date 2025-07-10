using CargoWise.IO;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IXmlWriter
	{
		void WriteXML(IDataObject dataStructure, SubStreamableStream outputStream, string nameSpace = null, IDataOverrideProvider overrideProvider = null);
	}
}
