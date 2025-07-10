using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSAmendmentComparisonEDIMessage))]
	class CDSAmendmentComparisonEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSAmendmentComparisonEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.NewAmendment, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessageStatusList.Codes.Acknowledged, message.EM_Status);
		}
	}
}
