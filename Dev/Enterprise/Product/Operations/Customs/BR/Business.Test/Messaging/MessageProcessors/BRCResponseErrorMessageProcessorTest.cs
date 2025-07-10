using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Common.BR;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class BRCResponseErrorMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCResponseErrorMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "XER" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => Array.Empty<string>();

		public void TestProcessMessage_InvalidUniversalEvent()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var (requestMessage, responseMessage) = CreateResponseMessage(entry, MessageTypeList.Codes.CDE, responseMessageText: CreateInvalidUniversalEvent());
			Factory.Save();

			var logger = new LoggingInformationForTesting();
			new BRCResponseErrorMessageProcessor(logger).ProcessMessage(responseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", entry.PK, requestMessage.EM_LinkUniqueID);
				AssertEquals("EM_Status", EDIMessage.Status.Failed, responseMessage.EM_Status);
				AssertEquals("CH_Status", ZString.Empty, entry.CH_Status);
				AssertEquals("Error logged", "Error: \tMessage #1: Message Text is not a XER Universal Event.\r\n", logger.LogMessages.ToString());
			});
		}

		public void TestProcessMessage_CusEntryHeader_CDE()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CDE;

			var (requestMessage, responseMessage) = CreateResponseMessage(entry, requestMessageType: MessageTypeList.Codes.CDE);
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", entry.PK, requestMessage.EM_LinkUniqueID);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("CH_Status", BRMessageStatusList.Codes.Rejected, entry.CH_Status);
				AssertEquals("Log Created on Entry Header", 1, entry.Logs.GetAllLogs().Count);
				AssertEquals("Log AutoEvents.MessageRejected Created", AutoEvents.MessageRejected.Code, entry.Logs.MostRecentLog.SL_SE_NKEvent);
				AssertEquals("EM_MessageInterpretation", "<H3>Service Error</H3>" +
							"<table border=\"1\" cellpadding=\"2\" cellspacing=\"0\" class=\"table\" style=\"white-space:pre\" width=\"100%\">" +
							"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
							"<tr><td>Unauthorized</td><td>ErrorMessage</td></tr>" +
							"</table>", responseMessage.EM_MessageInterpretation);
			});
		}

		public void TestProcessMessage_CusEntryHeader_CIH_ORI()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CDI;

			var (requestMessage, responseMessage) = CreateResponseMessage(entry, requestMessageType: MessageTypeList.Codes.CIH);
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("CH_Status", BRMessageStatusList.Codes.Rejected, entry.CH_Status);
				AssertEquals("MessageRejected log added", AutoEvents.MessageRejected.Code, entry.Logs.MostRecentLog.SL_SE_NKEvent);
			});
		}

		public void TestProcessMessage_CusEntryHeader_CIH_UPD()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CDI;
			entry.CH_BGMReference = "B00001000-1";
			entry.MovementReferenceNumberSetter("38BR15856778945");

			var (requestMessage, responseMessage) = CreateResponseMessage(entry, requestMessageType: MessageTypeList.Codes.CIH, requestMessageSubType: EDIMessageSubTypeList.Codes.Update);
			CreateLineMessageAndSetAsProcessed(entry, EDIMessageSubTypeList.Codes.Addition);
			CreateLineMessageAndSetAsProcessed(entry, EDIMessageSubTypeList.Codes.Update);
			var lineResponseMessage = CreateLineMessage(entry, EDIMessageSubTypeList.Codes.Deletion);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<MessageProcessLockException>("MessageProcessLockException thrown to postpone",
					"Message #1 postponed: Entry Header B00001000-1, has CIL message waiting response.", () =>
					{
						var logger = ExecuteMessageProcessor(responseMessage);
						AssertEquals("Logger", "Warning: \tMessage #1 postponed: Entry Header B00001000-1, has CIL message waiting response.\r\n", logger.LogMessages.ToString());
					});

				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, responseMessage.EM_Status);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("CH_Status", BRMessageStatusList.Codes.NotSent, entry.CH_Status);
				AssertNull("Log AutoEvents.MessageRejected should not be created", entry.Logs.MostRecentLog);
			});

			lineResponseMessage.EM_Status = EDIMessage.Status.ProcessedOK;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("EM_LinkUniqueID", entry.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entry.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
				AssertEquals("CH_Status", BRMessageStatusList.Codes.Rejected, entry.CH_Status);
				AssertEquals("MessageRejected log added", AutoEvents.MessageRejected.Code, entry.Logs.MostRecentLog.SL_SE_NKEvent);
			});
		}

		EDIMessage CreateLineMessage(CusEntryHeader entry, string lineMessageSubType)
		{
			return BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CIL, EDIMessageSubTypeList.Codes.Success,
				requestMessageSubType: lineMessageSubType).ResponseMessage;
		}

		void CreateLineMessageAndSetAsProcessed(CusEntryHeader entry, string lineMessageSubType)
		{
			CreateLineMessage(entry, lineMessageSubType).EM_Status = EDIMessage.Status.ProcessedOK;
		}

		public void TestProcessMessage_Subscription_ResponseForCancel()
		{
			BRCustomsDataRegistry.Instance.SendSubscriptionErrorsTo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			SetupNotificationGroup(BRCustomsDataRegistry.Instance.SendSubscriptionErrorsToGroup, "Dummy2@dummy.com", "EDE");
			var password = CreateSubscription();

			var (requestMessage, responseMessage) = CreateResponseMessage(password, MessageTypeList.Codes.SUB, EDIMessageSubTypeList.Codes.Cancel);
			requestMessage.EM_SystemCreateUser = Staff.GS_Code;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions("When outgoing message EM_MessageSubType = CAN", () =>
			{
				AssertEquals("EM_GB", password.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkUniqueID", password.PK, requestMessage.EM_LinkUniqueID);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("GP_PasswordStatus", BRPasswordStatusList.Codes.Rejected, password.GP_PasswordStatus);
				AssertEquals("GP_StatusReason", GlbExternalPassword_BRS.StatusReasons.CancelationRejected, password.GP_StatusReason);
			});

			AssertEmailSent("Service error response has been received for subscription DU-E - Historic for staff Staff01",
				GetExpectedEmailBody(password) + ExpectedServiceErrorHtml, new[] { "Dummy1@dummy.com", "Dummy2@dummy.com" });
		}

		public void TestProcessMessage_Subscription_ResponseForOriginal()
		{
			BRCustomsDataRegistry.Instance.SendSubscriptionErrorsTo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			SetupNotificationGroup(BRCustomsDataRegistry.Instance.SendSubscriptionErrorsToGroup, "Dummy2@dummy.com", "EDE");
			var password = CreateSubscription();

			var (requestMessage, responseMessage) = CreateResponseMessage(password, MessageTypeList.Codes.SUB, EDIMessageSubTypeList.Codes.Original);
			requestMessage.EM_SystemCreateUser = Staff.GS_Code;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions("When outgoing message EM_MessageSubType <> CAN", () =>
			{
				AssertEquals("EM_GB", password.Company.FirstActiveBranch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkUniqueID", password.PK, requestMessage.EM_LinkUniqueID);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);
				AssertEquals("GP_PasswordStatus", BRPasswordStatusList.Codes.Rejected, password.GP_PasswordStatus);
				AssertEquals("GP_StatusReason", GlbExternalPassword_BRS.StatusReasons.SubscriptionRejected, password.GP_StatusReason);
			});

			AssertEmailSent("Service error response has been received for subscription DU-E - Historic for staff Staff01",
				GetExpectedEmailBody(password) + ExpectedServiceErrorHtml, new[] { "Dummy1@dummy.com", "Dummy2@dummy.com" });
		}

		public void TestProcessMessage_Catalog()
		{
			var catalog1 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var (requestMessages, responseMessage) = CreateResponseMessage(new[] { catalog1 }, MessageTypeList.Codes.CAT);
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			AssertMessageProcessed(catalog1);

			var catalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var catalog3 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			(requestMessages, responseMessage) = CreateResponseMessage(new[] { catalog2, catalog3 }, MessageTypeList.Codes.CAT);
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			AssertMessageProcessed(catalog2);
			AssertMessageProcessed(catalog3);

			void AssertMessageProcessed(CusGoodsCatalog catalog)
			{
				CombineAssertions(() =>
				{
					var message = catalog.Messages.LastIncomingMessage;
					AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
					AssertEquals("EM_LinkTable", "CusGoodsCatalog", message.EM_LinkTable);
					AssertEquals("EM_LinkUniqueID", catalog.PK, message.EM_LinkUniqueID);
					AssertEquals("EM_MessageType", MessageTypeList.Codes.XER, message.EM_MessageType);
					AssertEquals("EM_MessageSubType", ZString.Empty, message.EM_MessageSubType);
					AssertEquals("EM_MessageText", responseMessage.EM_MessageText, message.EM_MessageText);
					AssertEquals("EM_GB", catalog.Company.FirstActiveBranch.PK, message.EM_GB);

					AssertEquals("Log AutoEvents.MessageRejected Created", AutoEvents.MessageRejected.Code, catalog.Logs.MostRecentLog.SL_SE_NKEvent);
					AssertEquals("CGC_MessageStatus", EDIMessage.Status.Rejected, catalog.CGC_MessageStatus);
				});
			}
		}

		public void TestProcessMessage_OrgHeader()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var (requestMessage, responseMessage) = CreateResponseMessage(importer, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.ManufacturerZipFile);
			Factory.Save();

			var expectedEvent = AutoEvents.MessageRejected;
			ExecuteMessageProcessor(responseMessage);
			AssertNull("Log MessageRejected not added for CAT|MZI", importer.Logs.MostRecentLogByEventTime(expectedEvent));

			(requestMessage, responseMessage) = CreateResponseMessage(importer, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.OperatorZipFile);
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			AssertNull("Log MessageRejected not added for CAT|OZI", importer.Logs.MostRecentLogByEventTime(expectedEvent));

			(requestMessage, responseMessage) = CreateResponseMessage(importer, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.CatalogZipFile);
			Factory.Save();
			ExecuteMessageProcessor(responseMessage);

			AssertEquals("Log MessageRejected added", expectedEvent.Code, importer.Logs.MostRecentLog.SL_SE_NKEvent);
			AssertEquals("SL_Reference", requestMessage.PK.ToString(), importer.Logs.MostRecentLog.SL_Reference);
		}

		public void TestProcessMessage_LPCO()
		{
			var lpco1 = Factory.NewWithValidTestData<CusLPCOHeader>();
			var (requestMessages, responseMessage) = CreateResponseMessage(new[] { lpco1 }, MessageTypeList.Codes.LPC);
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			AssertMessageProcessed(lpco1);

			var lpco2 = Factory.NewWithValidTestData<CusLPCOHeader>();
			var lpco3 = Factory.NewWithValidTestData<CusLPCOHeader>();
			(requestMessages, responseMessage) = CreateResponseMessage(new[] { lpco2, lpco3 }, MessageTypeList.Codes.LPC);
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);

			var responseMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive).AddToFilter(EDIMessageSchema.EM_LinkTable, CusLPCOHeader.Schema.TableName));
			AssertEquals(3, responseMessages.Length);

			AssertMessageProcessed(lpco2);
			AssertMessageProcessed(lpco3);

			AssertNotNull(responseMessages.Single(x => x.PK == responseMessage.PK));

			void AssertMessageProcessed(CusLPCOHeader lpcoHeader)
			{
				CombineAssertions(() =>
				{
					var message = lpcoHeader.Messages.LastIncomingMessage;
					AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
					AssertEquals("EM_LinkTable", "CusPermitHeader", message.EM_LinkTable);
					AssertEquals("EM_LinkUniqueID", lpcoHeader.PK, message.EM_LinkUniqueID);
					AssertEquals("EM_MessageType", MessageTypeList.Codes.XER, message.EM_MessageType);
					AssertEquals("EM_MessageSubType", ZString.Empty, message.EM_MessageSubType);
					AssertEquals("EM_MessageText", responseMessage.EM_MessageText, message.EM_MessageText);
					AssertEquals("EM_GB", lpcoHeader.Company.FirstActiveBranch.PK, message.EM_GB);

					AssertEquals("CPH_MessageStatus", BRMessageStatusList.Codes.Rejected, lpcoHeader.CPH_MessageStatus);
				});
			}
		}

		public void TestProcessMessage_ForeignOperator()
		{
			var foreignOperator1 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator1.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator1.BFR_OH_ForeignOperator = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator1.BFR_AuthorityIdentifier = "1";
			foreignOperator1.BFR_AuthorityVersion = "1";

			var foreignOperator2 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator2.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator2.BFR_OH_ForeignOperator = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator2.BFR_AuthorityIdentifier = "2";
			foreignOperator2.BFR_AuthorityVersion = "2";

			var foreignOperator3 = Factory.NewWithValidTestData<CusBRForeignOperator>();
			foreignOperator3.BFR_OH_Owner = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator3.BFR_OH_ForeignOperator = Factory.NewWithValidTestData<OrgHeader>().PK;
			foreignOperator3.BFR_AuthorityIdentifier = "3";
			foreignOperator3.BFR_AuthorityVersion = "3";

			Factory.Save();
			var (requestMessages, responseMessage) = CreateResponseMessage(new[] { foreignOperator1, foreignOperator2, foreignOperator3 }, MessageTypeList.Codes.XER);
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);

			var message1 = foreignOperator1.Messages.LastIncomingMessage;
			var message2 = foreignOperator2.Messages.LastIncomingMessage;
			var message3 = foreignOperator3.Messages.LastIncomingMessage;

			AssertEquals(message1.EM_EI, message2.EM_EI);
			AssertEquals(message1.EM_EI, message3.EM_EI);

			AssertMessage(foreignOperator1, message1);
			AssertMessage(foreignOperator2, message2);
			AssertMessage(foreignOperator3, message3);

			void AssertMessage(CusBRForeignOperator foreignOperator, EDIMessage message)
			{
				CombineAssertions(() =>
				{
					AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
					AssertEquals("EM_LinkTable", "CusBRForeignOperator", message.EM_LinkTable);
					AssertEquals("EM_LinkUniqueID", foreignOperator.PK, message.EM_LinkUniqueID);
					AssertEquals("EM_MessageType", MessageTypeList.Codes.XER, message.EM_MessageType);
					AssertEquals("EM_MessageSubType", ZString.Empty, message.EM_MessageSubType);
					AssertEquals("EM_MessageText", responseMessage.EM_MessageText, message.EM_MessageText);

					AssertEquals("Log AutoEvents.MessageRejected Created", AutoEvents.MessageRejected.Code, foreignOperator.Logs.MostRecentLog.SL_SE_NKEvent);
					AssertEquals("BFR_MessageStatus", EDIMessage.Status.Rejected, foreignOperator.BFR_MessageStatus);
				});
			}
		}

		#region Emails Notifications

		public void TestSendEmailNotification_NoEmails()
		{
			BRCustomsDataRegistry.Instance.SendExportDeclarationErrorsTo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NoEmails);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_DeclarationReference = "Ref123";

			var logger = new LoggingInformationForTesting();
			var (requestMessage, responseMessage) = CreateResponseMessage(entry, MessageTypeList.Codes.CDE);
			requestMessage.EM_SystemCreateUser = Staff.GS_Code;
			Factory.Save();

			var processor = ExecuteMessageProcessor(responseMessage);

			AssertEquals("No email should be created", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestSendEmailNotificationToStaff()
		{
			BRCustomsDataRegistry.Instance.SendExportDeclarationErrorsTo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMember);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_DeclarationReference = "Ref124";
			entry.CH_BGMReference = "xxxx";
			var (requestMessage, responseMessage) = CreateResponseMessage(entry, MessageTypeList.Codes.CDE);
			requestMessage.EM_SystemCreateUser = Staff.GS_Code;
			Factory.Save();

			var processor = ExecuteMessageProcessor(responseMessage);

			AssertEmailSent("Service error response has been received for job " + declaration.JE_DeclarationReference,
				GetExpectedEmailBody(entry) + ExpectedServiceErrorHtml, new[] { "Dummy1@dummy.com" });
		}

		public void TestSendEmailNotificationToGroup()
		{
			BRCustomsDataRegistry.Instance.SendExportDeclarationErrorsTo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NominatedGroup);

			SetupNotificationGroup(BRCustomsDataRegistry.Instance.SendExportDeclarationErrorsToGroup, "Dummy2@dummy.com", "EDE");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_DeclarationReference = "Ref125";
			entry.CH_BGMReference = "xxxx";
			var (requestMessage, responseMessage) = CreateResponseMessage(entry, MessageTypeList.Codes.CDE);
			requestMessage.EM_SystemCreateUser = Staff.GS_Code;
			Factory.Save();

			var processor = ExecuteMessageProcessor(responseMessage);

			AssertEmailSent("Service error response has been received for job " + declaration.JE_DeclarationReference,
				GetExpectedEmailBody(entry) + ExpectedServiceErrorHtml, new[] { "Dummy2@dummy.com" });
		}

		public void TestSendEmailNotificationToGroupAndStaff()
		{
			BRCustomsDataRegistry.Instance.SendExportDeclarationErrorsTo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);

			SetupNotificationGroup(BRCustomsDataRegistry.Instance.SendExportDeclarationErrorsToGroup, "Dummy2@dummy.com", "EDE");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var (requestMessage, responseMessage) = CreateResponseMessage(entry, MessageTypeList.Codes.CDE);
			declaration.JE_DeclarationReference = "Ref125";
			entry.CH_BGMReference = "xxxx";
			requestMessage.EM_SystemCreateUser = Staff.GS_Code;
			Factory.Save();

			var processor = ExecuteMessageProcessor(responseMessage);

			AssertEmailSent("Service error response has been received for job " + declaration.JE_DeclarationReference,
				GetExpectedEmailBody(entry) + ExpectedServiceErrorHtml, new[] { "Dummy1@dummy.com", "Dummy2@dummy.com" });
		}

		public void TestProcessMessage_LinkedObjectUnknown()
		{
			BRCustomsDataRegistry.Instance.SendProductCatalogErrorsTo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);

			SetupNotificationGroup(BRCustomsDataRegistry.Instance.SendExportDeclarationErrorsToGroup, "Dummy2@dummy.com", "EDE");

			var message = Factory.New<BREDIMessage>();
			message.EM_MessageText = CreateRejectUniversalEvent();
			message.EM_LinkedObject = Factory.NewWithValidTestData<CusGoodsCatalog>();
			Factory.Save();

			AssertNoExceptionThrown(() => ExecuteMessageProcessor(message));
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		void AssertEmailSent(string subject, string body, string[] recipients) => CombineAssertions(() =>
		{
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Email Subject", subject, email.Subject);
			AssertContains("Email Body", body, email.Body);
			AssertContainsExactElementsInAnyOrder("Email Recipients", recipients, email.Recipients.ToStringCollection());
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		});

		string GetExpectedEmailBody(CusEntryHeader entryHeader) => $"<strong>Entry reference number {entryHeader.CH_BGMReference} failed</strong><br />\r\n<br />\r\nClick here to open the job: {EmailDefBuilder.GetJobLink(entryHeader.Declaration, entryHeader.Declaration.JE_DeclarationReference)}";

		string GetExpectedEmailBody(GlbExternalPassword_BRS externalPassword) => $"<strong>Subscription {externalPassword.GP_UserID} failed</strong><br />\r\n<br />\r\nClick here to open the staff: {EmailDefBuilder.GetJobLink(ControllerIDs.GlbStaff, externalPassword.Staff.PK.ToGuid(), externalPassword.Staff.GS_FullName)}";

		string ExpectedServiceErrorHtml => "<H3>Service Error</H3><table border=\"1\" cellpadding=\"2\" cellspacing=\"0\" class=\"table\" style=\"white-space:pre\" width=\"100%\"><tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr><tr><td>Unauthorized</td><td>ErrorMessage</td></tr></table>";

		#endregion

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
			var eventSubscription = new EventSubscriptionCollection(Staff);
			var externalPasswordBr = eventSubscription.AddNew();
			externalPasswordBr.GP_UserID = EventIdList.Codes.DuexHistoric;
			return externalPasswordBr;
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

		(BREDIMessage requestMessage, BREDIMessage responseMessage) CreateResponseMessage(BusinessObject businessObject, string requestMessageType, string requestMessageSubType = null, string responseMessageText = null)
		{
			var (requestMessages, responseMessage) = CreateResponseMessage(new[] { businessObject }, requestMessageType, requestMessageSubType, responseMessageText);
			return (requestMessages.Single(), responseMessage);
		}

		(IEnumerable<BREDIMessage> requestMessages, BREDIMessage responseMessage) CreateResponseMessage(BusinessObject[] businessObjects, string requestMessageType, string requestMessageSubType = null, string responseMessageText = null)
		{
			var (requestMessages, responseMessage) = BRCResponseMessageProcessorTest.CreateMultipleMessagesAndInterchange(businessObjects, MessageTypeList.Codes.XER, ZString.Empty, requestMessageType, requestMessageSubType ?? EDIMessageSubTypeList.Codes.Original);
			responseMessage.EM_MessageText = responseMessageText ?? CreateRejectUniversalEvent();
			return (requestMessages, responseMessage);
		}

		string CreateRejectUniversalEvent() => UniversalEventTestDataHelper.CreateUniversalEventXml(Events.InterchangeRejectedCode, MessageTypeList.Codes.XER, responseType: "Unauthorized", reason: "ErrorMessage");

		string CreateInvalidUniversalEvent() => UniversalEventTestDataHelper.CreateUniversalEventXml(Events.InterchangeRejectedCode, messageType: "XXX", responseType: "Unauthorized", reason: "ErrorMessage");
	}
}
