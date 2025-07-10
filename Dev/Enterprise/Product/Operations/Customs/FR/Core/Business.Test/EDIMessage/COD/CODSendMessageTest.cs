using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(CODSendMessage))]
	class CODSendMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValue()
		{
			var messageDOA = Factory.New<CODSendMessage>();
			AssertEquals("Default application code", FREDIMessage.ApplicationCodes.FRCustomsMessage, messageDOA.EM_ApplicationCode);
			AssertEquals("Default message type", MessageTypeList.Codes.COD, messageDOA.EM_MessageType);
			AssertEquals("Default message sub type", MessageTypeList.Codes.COD, messageDOA.EM_MessageSubType);
			AssertEquals("Default message direction", EDIMessage.Direction.Transmit, messageDOA.EM_ReceiveTransmit);
		}
	}
}
