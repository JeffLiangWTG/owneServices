using System.Xml.Serialization;
using Enterprise.DataTransfer.Native.Common;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	[XmlSerializerAssembly("Enterprise.DataTransfer.Native.Business.XmlSerializers")]
	[XmlRoot(ReferenceDataXMLForDeSerialize.HeaderElementName, Namespace = ReferenceDataXMLForDeSerialize.NameSpace_Universal)]
	public class HeaderData_Universal : HeaderData
	{
	}
}
