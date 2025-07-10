using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	public class RequestDeserializer_Unversioned_Native : RequestDeserializer
	{
		protected override string NameSpace { get { return ReferenceDataXMLForDeSerialize.NameSpace_Unversioned_Native; } }
		protected override string RootElementName { get { return ReferenceDataXMLForDeSerialize.RootElementName; } }
		protected override ZXmlSerializer GetNewHeaderDeSerializer()
		{
			return ZXmlSerializer.New(typeof(HeaderData_Unversioned_Native));
		}

		public override IXmlSerializer GetResponseSerializer()
		{
			return new ObjectXmlSerializer<Response_Unversioned_Native>();
		}

		protected override Response GetNewResponse()
		{
			return new Response_Unversioned_Native();
		}
	}
}
