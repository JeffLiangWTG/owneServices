using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(DOAResponseFREDIMessage))]
	class DOAResponseFREDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValue()
		{
			var messageDOA = Factory.New<DOAResponseFREDIMessage>();
			AssertEquals("Default application code", FREDIMessage.ApplicationCodes.FRPortMessage, messageDOA.EM_ApplicationCode);
			AssertEquals("Default message type", MessageTypeList.Codes.POR, messageDOA.EM_MessageType);
			AssertEquals("Default message sub type", MessageSubTypeList.Codes.DOA, messageDOA.EM_MessageSubType);
			AssertEquals("Default message direction", EDIMessage.Direction.Receive, messageDOA.EM_ReceiveTransmit);
		}
	}
}
