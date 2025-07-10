using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public class CINImportResponseMessageDataObjectTest : TestCaseWithFactory
	{
		public void TestMessageContent()
		{
			var importMessageText = @"<CinMessage type=""WarehouseMovement-In"">
  <Header from=""CIN"" to=""doesNotMatter"" messageTime=""2011-12-13T14:15:16.017Z"" messageId=""12345"" />
  <WarehouseMovementInResponse>Expecting some kind of response in this format but that has not been defined as yet</WarehouseMovementInResponse>
</CinMessage>";

			var message = Factory.New<CINImportResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.CIN;
			message.EM_MessageSubType = MessageSubTypeList.Codes.CIN;
			message.EM_MessageText = importMessageText;
			AssertType<CINImportResponseMessageDataObject>("CIN import response message", message.MessageDataObject);

			var dataProvider = (ICINResponseDataProvider)message.MessageDataObject;

			CombineAssertions(() =>
			{
				Assert("Success", dataProvider.Success);
				AssertEquals("MessageID", "12345", dataProvider.MessageID);
			});
		}

		public void TestPrettier()
		{
			var message = Factory.New<CINImportResponseFREDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.CIN;
			message.EM_MessageSubType = MessageSubTypeList.Codes.CIN;

			AssertType<CINImportResponsePrettier>("CINImportResponsePrettier", message.MessageDataObject.Prettier);
		}
	}
}
