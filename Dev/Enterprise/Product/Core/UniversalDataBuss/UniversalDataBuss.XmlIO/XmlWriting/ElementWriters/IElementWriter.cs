using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlWriting
{
	interface IElementWriter
	{
		void WriteXML(IDataObject dataStructure, DataOverrideManager overrideManager);
		PlacingWithinXml ElementPlacing { get; }
	}
}
