using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSErrorResponseEDIMessage))]
	class CDSErrorResponseEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSErrorResponseEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.EHubErrorResponse, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		}
	}
}
