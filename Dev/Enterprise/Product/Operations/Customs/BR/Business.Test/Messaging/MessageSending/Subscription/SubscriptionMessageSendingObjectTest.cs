using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.Customs.BR.Business.GlbExternalPassword_BRS;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(SubscriptionMessageSendingObject))]
	class SubscriptionMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			AssertEquals("Default Message Type", SubscriptionMessageTypesList.Codes.ORI, subscriptionSendingObject.MessageType);
		}

		public void TestSubscriptionMessageTypes()
		{
			string[] expectedMessageTypeCodes = new string[] { "ORI", "AMD", "CAN" };
			AssertContainsExactElementsInExactOrder(expectedMessageTypeCodes, subscriptionSendingObject.SubscriptionMessageTypes.GetAllCodes());
		}
		public void TestProperties()
		{
			subscription.GP_MailBoxID = "001";
			subscription.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			var password = BRGlbStaffWrapper.Get(staff).CCTPassword;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(1);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			var ediMessage = Factory.NewWithValidTestData<BREDIMessage>();

			ediMessage.EM_LinkTable = subscription.TableName;
			ediMessage.EM_LinkUniqueID = subscription.PK;
			ediMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			ediMessage.EM_MessageType = MessageTypeList.Codes.SUB;
			ediMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.Original;
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			ediMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 06, 16);

			var messageString = @"{
  ""evento"": ""duex-historico"",
  ""endpoint"": ""<<ENDPOINT PLACE HOLDER>>""
}";

			CombineAssertions(() =>
			{
				AssertEquals("MessageAttachee", subscription, subscriptionSendingObject.MessageAttachee);
				AssertEquals("MessageType", SubscriptionMessageTypesList.Codes.ORI, subscriptionSendingObject.MessageType);
				AssertEquals("GetMessageText", messageString, subscriptionSendingObject.GetMessageText());
				AssertEquals("GetMessageTypeForEDIMessage", MessageTypeList.Codes.SUB, subscriptionSendingObject.GetMessageTypeForEDIMessage());
				AssertEquals("GetApplicationReference", "001", subscriptionSendingObject.GetApplicationReference());
				AssertEquals("GetMessageOwner", ZString.Empty, subscriptionSendingObject.GetMessageOwner());
				AssertEquals("GetGlbExternalPasswordPK", password.PK, subscriptionSendingObject.GetGlbExternalPasswordPK());
				AssertEquals("EventId", EventIdList.Codes.DuexHistoric, subscriptionSendingObject.EventId);
				AssertEquals("SubmittedDate", new ZDateTime(2023, 06, 16).ToLocalBranchTime(), subscriptionSendingObject.SubmittedDate);
				AssertEquals("Status", BRPasswordStatusList.Codes.Valid, subscriptionSendingObject.Status);
			});
		}

		public void TestValidateMessageType()
		{
			subscriptionSendingObject.ShouldSend = true;
			subscriptionSendingObject.MessageType = SubscriptionMessageTypesList.Codes.CAN;
			AssertHasErrorContaining(subscriptionSendingObject.MessageTypeInfo, "Unsubscribed events cannot be either amended or canceled.");

			subscriptionSendingObject.MessageType = SubscriptionMessageTypesList.Codes.AMD;
			AssertHasErrorContaining(subscriptionSendingObject.MessageTypeInfo, "Unsubscribed events cannot be either amended or canceled.");

			subscriptionSendingObject.Subscription.GP_MailBoxID = "1";
			subscriptionSendingObject.ShouldSend = true;
			AssertNoErrorContaining(subscriptionSendingObject.MessageTypeInfo, "Unsubscribed events cannot be either amended or canceled.");

			subscriptionSendingObject.Subscription.GP_MailBoxID = ZString.Empty;
			subscriptionSendingObject.ShouldSend = false;
			AssertNoErrorContaining(subscriptionSendingObject.MessageTypeInfo, "Unsubscribed events cannot be either amended or canceled.");

			subscriptionSendingObject.ShouldSend = true;
			subscriptionSendingObject.MessageType = SubscriptionMessageTypesList.Codes.ORI;
			AssertNoErrorContaining(subscriptionSendingObject.MessageTypeInfo, "Unsubscribed events cannot be either amended or canceled.");

			subscriptionSendingObject.Subscription.GP_StatusReason = StatusReasons.Subscribed;
			subscriptionSendingObject.MessageType = SubscriptionMessageTypesList.Codes.ORI;
			AssertHasErrorContaining(subscriptionSendingObject.MessageTypeInfo, "Cannot subscribed to an already subscribed event.");

			subscriptionSendingObject.Subscription.GP_StatusReason = ZString.Empty;
			subscriptionSendingObject.MessageType = SubscriptionMessageTypesList.Codes.ORI;
			AssertNoErrorContaining(subscriptionSendingObject.MessageTypeInfo, "Cannot subscribed to an already subscribed event.");
		}

		public void TestGetMessageText()
		{
			AssertContains("\"evento\": \"duex-historico\",", subscriptionSendingObject.GetMessageText());
		}

		protected override BusinessObject GetNewBusinessObject() => subscriptionSendingObject;

		protected override void SetUp()
		{
			base.SetUp();

			staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XXX";

			subscription = BRGlbStaffWrapper.Get(staff).EventSubscriptions.AddNew();
			subscription.GP_UserID = EventIdList.Codes.DuexHistoric;
			subscription.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			subscriptionSendingObject = new SubscriptionMessageSendingObject(subscription);
		}

		GlbStaff staff;
		GlbExternalPassword_BRS subscription;
		SubscriptionMessageSendingObject subscriptionSendingObject;
	}
}
