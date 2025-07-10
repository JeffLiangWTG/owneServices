using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSFECAmendmentDeclarationEDIMessage))]
	class CDSFECAmendmentDeclarationEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSFECAmendmentDeclarationEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.FecChallenge, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		}
	}
}
