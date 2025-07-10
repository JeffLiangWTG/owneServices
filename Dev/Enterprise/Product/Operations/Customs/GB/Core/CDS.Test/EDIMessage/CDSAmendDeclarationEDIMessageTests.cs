using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSAmendDeclarationEDIMessage))]
	class CDSAmendDeclarationEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSAmendDeclarationEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.AmendDeclaration, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		}
	}
}
