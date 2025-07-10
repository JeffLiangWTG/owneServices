using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSDocumentUploadConfirmationResponse))]
	class CDSDocumentUploadConfirmationResponseTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = (CDSDocumentUploadConfirmationResponse)GetNewBusinessObject();

			AssertEquals(CDSEDIMessageTypeList.Codes.DocumentUploadConfirmation, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		}
	}
}
