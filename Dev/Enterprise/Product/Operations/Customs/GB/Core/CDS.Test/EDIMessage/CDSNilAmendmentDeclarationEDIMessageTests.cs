using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSNilAmendmentDeclarationEDIMessage))]
	class CDSNilAmendmentDeclarationEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSNilAmendmentDeclarationEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.NilAmendment, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		}
	}
}
