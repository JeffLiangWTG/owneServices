using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CARMStatementOfAccountMessage))]
	public class CARMStatementOfAccountMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CARMStatementOfAccountMessage>();
			AssertEquals(ApplicationCodeList.Codes.CACustoms, message.EM_ApplicationCode);
			AssertEquals(MessageTypeList.Codes.CARMStatementOfAccount, message.EM_MessageType);
		}

		public void TestIControllerIDProvider()
		{
			var message = Factory.New<CARMStatementOfAccountMessage>();
			var controllerIDProvider = (IControllerIDProvider)message;
			AssertEquals(message.PK, controllerIDProvider.BusinessObjectPK);
			AssertEquals(ControllerIDs.Messaging.EDIMessage, controllerIDProvider.ControllerID);
		}
	}
}
