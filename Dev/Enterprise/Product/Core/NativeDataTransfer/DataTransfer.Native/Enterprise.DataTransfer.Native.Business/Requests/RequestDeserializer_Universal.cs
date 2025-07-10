using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	public class RequestDeserializer_Universal : RequestDeserializer
	{
		protected override string NameSpace { get { return ReferenceDataXMLForDeSerialize.NameSpace_Universal; } }
		protected override string RootElementName { get { return ReferenceDataXMLForDeSerialize.RootElementName_Universal; } }

		protected override ZXmlSerializer GetNewHeaderDeSerializer()
		{
			return ZXmlSerializer.New(typeof(HeaderData_Universal));
		}

		public override IXmlSerializer GetResponseSerializer()
		{
			return new ObjectXmlSerializer<Response_Universal>();
		}

		protected override Response GetNewResponse()
		{
			return new Response_Universal();
		}
	}
}
