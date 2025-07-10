using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	public class RequestDeserializer_Versioned_Native : RequestDeserializer
	{
		protected override string NameSpace { get { return NativeXmlInfo.Namespace_2011_11; } }
		protected override string RootElementName { get { return ReferenceDataXMLForDeSerialize.RootElementName; } }
		protected override ZXmlSerializer GetNewHeaderDeSerializer()
		{
			return ZXmlSerializer.New(typeof(HeaderData_Versioned_Native));
		}

		public override IXmlSerializer GetResponseSerializer()
		{
			return new ObjectXmlSerializer<Response_Versioned_Native>();
		}

		protected override Response GetNewResponse()
		{
			return new Response_Versioned_Native();
		}
	}
}
