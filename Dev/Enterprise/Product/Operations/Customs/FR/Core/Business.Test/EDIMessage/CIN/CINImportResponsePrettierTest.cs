using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public class CINImportResponsePrettierTest : TestCaseWithFactory
	{
		public void TestGetOutgoingMessageInterpretation()
		{
			var messageText = "Any string here is ok";
			var ediMessage = Factory.New<CINImportResponseFREDIMessage>();
			ediMessage.EM_MessageType = MessageTypeList.Codes.CIN;
			ediMessage.EM_MessageSubType = MessageSubTypeList.Codes.CIN;
			ediMessage.EM_MessageText = messageText;
			ediMessage.EM_ReceiveTransmit = "TRX";

			AssertEquals("Message interpretation of outgoing messages should reflect the raw message text", messageText, ediMessage.EM_MessageInterpretation);
		}

		public void TestGetMessageInterpretation_Valid()
		{
			var importMessageText = @"<CinMessage type=""WarehouseMovement-In"">
  <Header from=""CIN"" to=""PUT-CIN-ID-HERE"" messageTime=""2011-12-13T14:15:16.017Z"" messageId=""12345"" />
  <WarehouseMovementInResponse>Expecting some kind of response in this format but that has not been defined as yet</WarehouseMovementInResponse>
</CinMessage>";

			var message = Factory.New<CINImportResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.CIN;
			message.EM_MessageSubType = MessageSubTypeList.Codes.CIN;
			message.EM_MessageText = importMessageText;

			AssertEquals(@"<H1>The content of the response is not defined as yet</H1>", message.EM_MessageInterpretation);
		}
	}
}
