using System.Xml.Serialization;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	[XmlSerializerAssembly("Enterprise.DataTransfer.Native.Business.XmlSerializers")]
	[XmlRoot(ReferenceDataXMLForDeSerialize.HeaderElementName, Namespace = NativeXmlInfo.Namespace_2011_11)]
	public class HeaderData_Versioned_Native : HeaderData
	{
	}
}
