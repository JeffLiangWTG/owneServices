using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCSubscriptionErrorResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCSubscriptionErrorResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "SUB" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "ERR" };

		public void TestProcessResponseMessage_ErrorResponseForOriginal()
		{
			AssertProcessResponseMessage(EDIMessageSubTypeList.Codes.Original, EDIMessageSubTypeList.Codes.Error, SubscriptionErrorMessagePrettyFormatterTest.JsonError, GlbExternalPassword_BRS.StatusReasons.SubscriptionRejected, BRPasswordStatusList.Codes.Rejected, "");
		}

		public void TestProcessResponseMessage_ErrorResponseForAmend()
		{
			AssertProcessResponseMessage(EDIMessageSubTypeList.Codes.Amend, EDIMessageSubTypeList.Codes.Error, SubscriptionErrorMessagePrettyFormatterTest.JsonError, GlbExternalPassword_BRS.StatusReasons.SubscriptionRejected, BRPasswordStatusList.Codes.Rejected, "");
		}

		public void TestProcessResponseMessage_ErrorResponseForCancel()
		{
			Subscription.GP_MailBoxID = "1";
			AssertProcessResponseMessage(EDIMessageSubTypeList.Codes.Cancel, EDIMessageSubTypeList.Codes.Error, SubscriptionErrorMessagePrettyFormatterTest.JsonError, GlbExternalPassword_BRS.StatusReasons.CancelationRejected, BRPasswordStatusList.Codes.Rejected, "1");
		}
		public void TestNotSendNotification()
		{
			BRCustomsDataRegistry.Instance.SendSubscriptionErrorsTo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NoEmails);

			var responseMessage = CreateResponseMessage(EDIMessageSubTypeList.Codes.Error, EDIMessageSubTypeList.Codes.Error, SubscriptionErrorMessagePrettyFormatterTest.JsonError);
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			AssertEquals("No email should be created", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestSendNotification()
		{
			BRCustomsDataRegistry.Instance.SendSubscriptionErrorsTo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			SetupNotificationGroup(BRCustomsDataRegistry.Instance.SendSubscriptionErrorsToGroup, "Dummy2@dummy.com", "EDE");

			var responseMessage = CreateResponseMessage(EDIMessageSubTypeList.Codes.Error, EDIMessageSubTypeList.Codes.Error, SubscriptionErrorMessagePrettyFormatterTest.JsonError);
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);

			var jobLink = EmailDefBuilder.GetJobLink(ControllerIDs.GlbStaff, Subscription.Staff.PK.ToGuid(), Subscription.Staff.GS_FullName);

			CombineAssertions(() =>
			{
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("Email Subject", "Customs error response has been received for subscription DU-E - Historic for staff TEST USER NAME", email.Subject);
				AssertContains("Email Body", @$"<strong>Subscription DU-E - Historic failed</strong><br />
<br />
Click here to open the staff: {jobLink}{ExpectedErrorHtml}", email.Body);
				AssertContainsExactElementsInAnyOrder("Email Recipients", new[] { "Dummy1@dummy.com", "Dummy2@dummy.com" }, email.Recipients.ToStringCollection());
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		string ExpectedErrorHtml => @"<H3>Customs Error</H3><table border=""1"" cellpadding=""2"" cellspacing=""0"" class=""table"" style=""white-space:pre"" width=""100%""><tr><td><strong>Info</strong></td><td><strong>Description</strong></td></tr><tr><td>code</td><td>PLAT-ER9100</td></tr><tr><td>message</td><td>TEST SUB</td></tr><tr><td>ambiente</td><td>TRE</td></tr><tr><td>usuario</td><td>00302993738</td></tr></table>";

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

		void SetupNotificationGroup(IRegistryItem notificationRegistryItem, ZString emailAddress, ZString code)
		{
			var postmasters = Factory.Load<GlbGroup>(Groups.PostMastersGroupPK);
			var postMaster = postmasters.Staff.AddNew();
			postMaster.GS_EmailAddress = "PostMaster@Gallifrey.com";

			if (notificationRegistryItem != null)
			{
				var group = Factory.New<GlbGroup>();
				group.GG_Code = code;
				group.GG_Desc = "GroupExpDeclarationErrors";

				var staff = group.Staff.AddNew();
				staff.GS_Code = code;
				staff.GS_LoginName = "loginName";
				staff.GS_EmailAddress = emailAddress;

				Factory.Save();
				notificationRegistryItem.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid());
				Factory.Save();
			}
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
	}
}
