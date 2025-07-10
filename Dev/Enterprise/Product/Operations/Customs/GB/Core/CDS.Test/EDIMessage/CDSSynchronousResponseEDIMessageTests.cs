using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSSynchronousResponseEDIMessage))]
	public class CDSSynchronousResponseEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSSynchronousResponseEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.SynchronousResponse, message.EM_MessageType);
			AssertEquals(CDSEDIMessageTypeList.Codes.ConversationID, message.EM_MessageSubType);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		}

		public void TestMessageDataObject()
		{
			var message = Factory.New<CDSSynchronousResponseEDIMessage>();
			message.EM_MessageText = @"<SynchronousResponse>
               <status>202</status>
               <code>ACCEPTED</code>
               <ResponseHeaders>
                              <x-conversation-id>77685C3D14D53E25E0540003BA9676AB</x-conversation-id>
               </ResponseHeaders>
</SynchronousResponse>
";
			var messageDataObject = message.MessageDataObject;
			AssertEquals("202", messageDataObject.Status);
			AssertEquals("ACCEPTED", messageDataObject.Code);
			Assert(messageDataObject.IsAccepted);
		}
	}
}
