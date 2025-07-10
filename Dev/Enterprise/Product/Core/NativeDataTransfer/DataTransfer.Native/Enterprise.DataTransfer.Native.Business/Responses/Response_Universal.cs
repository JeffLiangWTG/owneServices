using System.Xml.Serialization;
using Enterprise.DataTransfer.Native.Common;

namespace Enterprise.DataTransfer.Native.Business.Responses
{
	[XmlSerializerAssembly("Enterprise.DataTransfer.Native.Business.XmlSerializers")]
	[XmlRoot("Response", Namespace = ReferenceDataXMLForDeSerialize.NameSpace_Universal)]
	public class Response_Universal : Response
	{
	}
}
