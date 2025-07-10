using System.Xml.Serialization;
using Enterprise.DataTransfer.Native.Common;

namespace Enterprise.DataTransfer.Native.Business.Responses
{
	[XmlSerializerAssembly("Enterprise.DataTransfer.Native.Business.XmlSerializers")]
	[XmlRoot("Response", Namespace = ReferenceDataXMLForDeSerialize.NameSpace_Unversioned_Native)]
	public class Response_Unversioned_Native : Response
	{
	}
}
