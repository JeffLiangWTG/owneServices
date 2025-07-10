using System.Xml.Serialization;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.Business.Responses
{
	[XmlSerializerAssembly("Enterprise.DataTransfer.Native.Business.XmlSerializers")]
	[XmlRoot("Response", Namespace = NativeXmlInfo.Namespace_2011_11)]
	public class Response_Versioned_Native : Response
	{
	}
}
