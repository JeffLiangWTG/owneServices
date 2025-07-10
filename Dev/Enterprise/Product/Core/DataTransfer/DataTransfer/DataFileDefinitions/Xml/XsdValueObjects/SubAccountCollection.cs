using System.Xml.Serialization;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssembly("Enterprise.DataTransfer.XmlSerializers")]
	[ValueObjectSubclass("Enterprise.DataTransfer.Xml.XsdVersion1.AutoSubAccountCollection")]
	public class SubAccountCollection : AutoSubAccountCollection
	{
	}
}
