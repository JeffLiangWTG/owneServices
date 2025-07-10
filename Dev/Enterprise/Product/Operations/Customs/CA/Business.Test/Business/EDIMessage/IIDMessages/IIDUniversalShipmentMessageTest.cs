using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(IIDUniversalShipmentMessage))]
	sealed class IIDUniversalShipmentMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<IIDUniversalShipmentMessage>();
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.CAIMP, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.IntegratedImportDeclaration, message.EM_MessageType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
		}
	}
}
