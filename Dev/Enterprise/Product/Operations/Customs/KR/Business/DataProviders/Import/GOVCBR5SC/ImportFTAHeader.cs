using System.Xml.Serialization;

namespace Enterprise.Customs.KR.Business
{
	[XmlRoot("ImportFTAHeader")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ImportFTAHeader : ImportFTAHeaderCore
	{
	}
}
