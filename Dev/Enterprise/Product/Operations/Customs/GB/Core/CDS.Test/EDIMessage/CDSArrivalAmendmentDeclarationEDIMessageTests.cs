using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSArrivalAmendmentDeclarationEDIMessage))]
	class CDSArrivalAmendmentDeclarationEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSArrivalAmendmentDeclarationEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.ArrivalNotification, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		}
	}
}
