using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class ECSMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendArrivalMessage()
		{
			var errorCollector = new EU.Business.ErrorCollector();
			new ECSMessageSender(exitDetail, MessageSubTypeList.Codes.ARR, errorCollector).Send();

			AssertECSMessageCreated<ECSArrivalFREDIMessage>(exitDetail.Messages[0], "<schemaID>MessageIE507</schemaID>", "ARR");
		}

		public void TestSendDepartureMessage()
		{
			var errorCollector = new EU.Business.ErrorCollector();
			new ECSMessageSender(exitDetail, MessageSubTypeList.Codes.DEP, errorCollector).Send();

			AssertECSMessageCreated<ECSDepartureFREDIMessage>(exitDetail.Messages[0], "<schemaID>MessageIE618</schemaID>", "DEP");
		}

		public static void AssertECSMessageCreated<T>(EDIMessage message, ZString messageText, ZString messageSubType)
		{
			AssertType<T>("FR message created", message);
			AssertContains("EM_MessageText", messageText, message.EM_MessageText);
			AssertEquals("EM_ApplicationCode", "FRC", message.EM_ApplicationCode);
			AssertEquals("EM_Status", "QUE", message.EM_Status);
			AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
			AssertEquals("EM_MessageType", "ECS", message.EM_MessageType);
			AssertEquals("EM_MessageSubType", messageSubType, message.EM_MessageSubType);
		}

		public void TestSendWithUnknownMessageSubType()
		{
			var errorCollector = new EU.Business.ErrorCollector();
			new ECSMessageSender(exitDetail, "XXX", errorCollector).Send();

			AssertEquals("Failed to send message\nCW1 doesn't yet support building message type ECS XXX", errorCollector.GetErrorsAsString());
		}

		CusExitDetail exitDetail;

		protected override void SetUp()
		{
			base.SetUp();

			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitDetail = exitHeader.CusExitDetails.AddNew();
		}
	}
}
