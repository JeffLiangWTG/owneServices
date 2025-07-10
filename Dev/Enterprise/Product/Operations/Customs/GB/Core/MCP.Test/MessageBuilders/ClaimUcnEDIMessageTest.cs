using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.MCP.MessageBuilders.Testing
{
	[TestedType(typeof(ClaimUcnEDIMessage))]
	internal class ClaimUcnEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<ClaimUcnEDIMessage>();
			AssertEquals(ApplicationCodeList.Codes.GbMcpClaimUcn, message.EM_ApplicationCode);
			AssertEquals(Constants.EDIMessageTypes.UCN, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
		}
	}
}
