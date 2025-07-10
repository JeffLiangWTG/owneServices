using System.Xml.Serialization;
using Enterprise.DataTransfer.Native.Common;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	[XmlSerializerAssembly("Enterprise.DataTransfer.Native.Business.XmlSerializers")]
	[XmlRoot(ReferenceDataXMLForDeSerialize.HeaderElementName, Namespace = ReferenceDataXMLForDeSerialize.NameSpace_Unversioned_Native)]
	public class HeaderData_Unversioned_Native : HeaderData
	{
	}
}
