using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	[TestedType(typeof(GVMSErrorResponseEDIMessage))]
	class GVMSErrorResponseEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<GVMSErrorResponseEDIMessage>();
			AssertEquals(Constants.GVMSMessageSubTypes.EHUBERRORRESPONSE, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		}
	}
}
