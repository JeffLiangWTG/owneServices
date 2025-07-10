using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class SubscriptionMessageManagerTest : TestCaseWithFactory
	{
		public void TestMessageFriendlyName()
		{
			AssertEquals(MessageTypeList.Descriptions.SUB, messageSubscriptionManager.MessageFriendlyName);
		}

		public void TestCanSendOriginal()
		{
			AssertEquals(true, messageSubscriptionManager.CanSendOriginal);
		}

		public void TestCanSendWithdrawal()
		{
			AssertEquals(false, messageSubscriptionManager.CanSendWithdrawal);
		}

		public void TestIsWaitingForResponse()
		{
			AssertEquals(false, messageSubscriptionManager.IsWaitingForResponse);
		}

		public void TestHasActiveMessages()
		{
			AssertEquals(false, messageSubscriptionManager.HasActiveMessages);
		}

		public void TestGenerateMessages()
		{
			var password = BRGlbStaffWrapper.Get(newStaff).CCTPassword;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(1);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			var message = messageSubscriptionManager.GenerateMessages().First();

			CombineAssertions("EDIMessage generated", () =>
			{
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.BRCustoms, message.EM_ApplicationCode);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.SUB, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", SubscriptionMessageTypesList.Codes.ORI, message.EM_MessageSubType);
				AssertEquals("EM_MessageOwner", ZString.Empty, message.EM_MessageOwner);
				Assert("EM_MessageText", !message.EM_MessageText.IsEmpty);
				AssertEquals("EM_GP", password.PK, message.EM_GP);
				AssertEquals("EM_ApplicationReference", ZString.Empty, message.EM_ApplicationReference);
				AssertEquals("EM_LinkTable", "GlbExternalPassword", message.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", messageSender.Subscription.PK, message.EM_LinkUniqueID);
				AssertEquals("GP_StatusReason", GlbExternalPassword_BRS.StatusReasons.AwaitingResponse, messageSender.Subscription.GP_StatusReason);
				AssertEquals("GP_PasswordStatus", "AWA", messageSender.Subscription.GP_PasswordStatus);
			});

			messageSubscriptionManager.RollbackOnSavingFailed();
			AssertEquals("GP_StatusReason", ZString.Empty, messageSender.Subscription.GP_StatusReason);
			AssertEquals("GP_PasswordStatus", ZString.Empty, messageSender.Subscription.GP_PasswordStatus);
		}

		protected override void SetUp()
		{
			base.SetUp();

			newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "XXX";

			var wrapper = BRGlbStaffWrapper.Get(newStaff);
			var subscription = wrapper.EventSubscriptions.AddNew();

			messageSender = new SubscriptionMessageSendingObject(subscription);
			messageSubscriptionManager = new SubscriptionMessageManager(messageSender);
		}

		GlbStaff newStaff;
		SubscriptionMessageSendingObject messageSender;
		SubscriptionMessageManager messageSubscriptionManager;
	}
}
