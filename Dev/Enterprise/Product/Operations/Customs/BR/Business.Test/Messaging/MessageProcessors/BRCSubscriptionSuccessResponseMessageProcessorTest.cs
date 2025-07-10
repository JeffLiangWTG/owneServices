using System.Collections.Generic;
using System.Text.Json;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCSubscriptionSuccessResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCSubscriptionSuccessResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "SUB" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "SUC" };

		public void TestProcessResponseMessage_InvalidJson()
		{
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(Subscription, MessageTypeList.Codes.SUB, null).ResponseMessage;
			responseMessage.EM_MessageText = @"";
			Factory.Save();
			AssertExceptionThrown<JsonException>(() => ExecuteMessageProcessor(responseMessage));
		}

		public void TestProcessResponseMessageWithoutId()
		{
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(Subscription, MessageTypeList.Codes.SUB, null).ResponseMessage;
			responseMessage.EM_MessageText = jsonMessageWithoutId;
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertContains("Logger", "Message #1: Message deserialization was failed.", logger.LogMessages.ToString());
			});
		}

		public void TestProcessResponseMessage_SuccessResponseForOriginal()
		{
			AssertProcessResponseMessage(EDIMessageSubTypeList.Codes.Original, EDIMessageSubTypeList.Codes.Success, JsonMessage, GlbExternalPassword_BRS.StatusReasons.Subscribed, BRPasswordStatusList.Codes.Accepted, "1");
		}

		public void TestProcessResponseMessage_SuccessResponseForAmend()
		{
			AssertProcessResponseMessage(EDIMessageSubTypeList.Codes.Amend, EDIMessageSubTypeList.Codes.Success, JsonMessage, GlbExternalPassword_BRS.StatusReasons.Subscribed, BRPasswordStatusList.Codes.Accepted, "1");
		}

		public void TestProcessResponseMessage_SuccessResponseForCancel()
		{
			Subscription.GP_MailBoxID = "1";
			AssertProcessResponseMessage(EDIMessageSubTypeList.Codes.Cancel, EDIMessageSubTypeList.Codes.Success, "", GlbExternalPassword_BRS.StatusReasons.Canceled, BRPasswordStatusList.Codes.Canceled, "");
		}

		void AssertProcessResponseMessage(string requestMessageSubType, string responseMessageSubType, string messageText, string expectedStatusReason, string expectedPasswordStatus, string expectSubcriptionID)
		{
			var responseMessage = CreateResponseMessage(requestMessageSubType, responseMessageSubType, messageText);
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", Subscription.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkTable", "GlbExternalPassword", responseMessage.EM_LinkTable);
				AssertEquals("EM_LinkUniqueID", Subscription.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.SUB, responseMessage.EM_MessageType);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);

				var glbExternalPassword = responseMessage.EM_LinkedObject as GlbExternalPassword;
				AssertEquals("GP_StatusReason", expectedStatusReason, glbExternalPassword.GP_StatusReason);
				AssertEquals("GP_PasswordStatus", expectedPasswordStatus, glbExternalPassword.GP_PasswordStatus);
				AssertEquals("GP_MailBoxID", expectSubcriptionID, Subscription.GP_MailBoxID);
			});
		}

		BREDIMessage CreateResponseMessage(string requestMessageSubType, string responseMessageSubType, string messageText)
		{
			var (requestMessage, responseMessage) = BRCResponseMessageProcessorTest.CreateResponseMessage(Subscription,
				responseMessageType: MessageTypeList.Codes.SUB, responseMessageSubType: responseMessageSubType,
				requestMessageType: MessageTypeList.Codes.SUB, requestMessageSubType: requestMessageSubType);
			responseMessage.EM_MessageText = messageText;
			requestMessage.EM_SystemCreateUser = Staff.GS_Code;
			return responseMessage;
		}

		GlbStaff Staff => staff ?? (staff = CreateStaff("S01", "S01", "Staff01", "Dummy1@dummy.com"));
		GlbStaff staff;

		GlbStaff CreateStaff(ZString code, ZString loginName, ZString fullName, ZString email)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_LoginName = loginName;
			staff.GS_FullName = fullName;
			staff.GS_EmailAddress = email;
			return staff;
		}

		GlbExternalPassword_BRS CreateSubscription()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "TEST USER NAME";
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			var eventSubscription = new EventSubscriptionCollection(staff);
			var externalPasswordBr = eventSubscription.AddNew();
			externalPasswordBr.GP_UserID = EventIdList.Codes.DuexHistoric;
			return externalPasswordBr;
		}

		GlbExternalPassword_BRS Subscription => subscription ?? (subscription = CreateSubscription());
		GlbExternalPassword_BRS subscription;

		const string JsonMessage = @"
		{
			""id"": 1,
			""evento"": ""duex-historico"",
			""endpoint"": ""https://endpoint:443""
		}";

		readonly string jsonMessageWithoutId = @"
		{
			""evento"": ""duex-historico"",
			""endpoint"": ""https://endpoint:443""
		}";
	}
}
