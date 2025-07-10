using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Subscription.Testing
{
	class NotificationSubscriptionProviderTest : TestCaseWithFactory
	{
		public void TestEndpoint()
		{
			var subscription = Factory.New<GlbExternalPassword_BRS>();
			var messageObject = new SubscriptionMessageSendingObject(subscription);
			var provider = new NotificationSubscriptionProvider(messageObject);
			AssertEquals(BREDIInterchange.EndpointAddressPlaceHolder, provider.Endpoint);

			messageObject.MessageType = SubscriptionMessageTypesList.Codes.CAN;
			provider = new NotificationSubscriptionProvider(messageObject);
			AssertNull("Endpoint for CAN", provider.Endpoint);
		}

		public void TestEvent()
		{
			var subscription = Factory.New<GlbExternalPassword_BRS>();
			var messageObject = new SubscriptionMessageSendingObject(subscription);
			var provider = new NotificationSubscriptionProvider(messageObject);

			AssertNotificationSubscriptionProvider(EventIdList.Codes.DuexHistoric, "duex-historico");
			AssertNotificationSubscriptionProvider(EventIdList.Codes.ProductCatalog, "catp-prod-desativado");
			AssertNotificationSubscriptionProvider(EventIdList.Codes.CctReleasedCargo, "cctr-carga-liberada");
			AssertNotificationSubscriptionProvider(EventIdList.Codes.CctBlockedCargo, "cctr-carga-bloqueio");
			AssertNotificationSubscriptionProvider(EventIdList.Codes.CctRedChannel, "cctr-canal-vermelho");
			AssertNotificationSubscriptionProvider(EventIdList.Codes.LpcoStatusChange, "talp-altsit-lpco-anu");
			AssertNotificationSubscriptionProvider(EventIdList.Codes.LpcoExigencyInclusion, "talp-inclusao-exig");
			AssertNotificationSubscriptionProvider(EventIdList.Codes.LpcoExigencyCancelation, "talp-cancela-exig");
			AssertNotificationSubscriptionProvider(EventIdList.Codes.DuimpDiagnosis, "dimp-diag-import");
			AssertNotificationSubscriptionProvider(EventIdList.Codes.DuimpRegister, "dimp-registro-import");
			AssertNotificationSubscriptionProvider(EventIdList.Codes.DuimpStatus, "dimp-situacao-import");

			void AssertNotificationSubscriptionProvider(ZString userId, ZString expectedEvent)
			{
				subscription.GP_UserID = userId;
				AssertEquals(expectedEvent, provider.Event);
			}

			messageObject.MessageType = SubscriptionMessageTypesList.Codes.CAN;
			provider = new NotificationSubscriptionProvider(messageObject);
			AssertNull("Event for CAN", provider.Event);
		}

		public void TestID()
		{
			var subscription = Factory.New<GlbExternalPassword_BRS>();
			subscription.GP_UserID = EventIdList.Codes.DuexHistoric;
			subscription.GP_MailBoxID = "1";

			var messageObject = new SubscriptionMessageSendingObject(subscription);
			messageObject.MessageType = SubscriptionMessageTypesList.Codes.AMD;

			var provider = new NotificationSubscriptionProvider(messageObject);
			AssertEquals("ID", 1, provider.ID);

			messageObject.MessageType = SubscriptionMessageTypesList.Codes.CAN;
			AssertEquals("ID", 1, provider.ID);

			subscription.GP_MailBoxID = "X";
			AssertEquals("ID", 0, provider.ID);

			messageObject.MessageType = SubscriptionMessageTypesList.Codes.ORI;
			AssertEquals("ID", 0, provider.ID);
		}

		public void TestToMessageString()
		{
			var subscription = Factory.New<GlbExternalPassword_BRS>();
			subscription.GP_UserID = EventIdList.Codes.DuexHistoric;
			subscription.GP_MailBoxID = "1";
			var messageObject = new SubscriptionMessageSendingObject(subscription);

			CombineAssertions(() =>
			{
				messageObject.MessageType = SubscriptionMessageTypesList.Codes.ORI;
				AssertEquals("ORI", expectedORI, messageObject.GetMessageText());

				messageObject.MessageType = SubscriptionMessageTypesList.Codes.AMD;
				AssertEquals("AMD", expectedAMD, messageObject.GetMessageText());

				messageObject.MessageType = SubscriptionMessageTypesList.Codes.CAN;
				AssertEquals("CAN", expectedCAN, messageObject.GetMessageText());
			});
		}

		public const string expectedORI = @"{
  ""evento"": ""duex-historico"",
  ""endpoint"": ""<<ENDPOINT PLACE HOLDER>>""
}";

		public const string expectedAMD = @"{
  ""id"": 1,
  ""evento"": ""duex-historico"",
  ""endpoint"": ""<<ENDPOINT PLACE HOLDER>>""
}";

		public const string expectedCAN = @"{
  ""id"": 1
}";
	}
}
