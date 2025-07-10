using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.UniversalDataBuss.Testing.Management
{
	public class ImportResultNotifierTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestEmailNotificationWithExtendEmailService()
		{
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			logger.NotificationEmailManager.Register(new EmailProcessorForTest());

			eServicesRegistry.Instance.ImportRejectedNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateGroupWithUser("rejected@test.com").PK.ToGuid());

			var message = Factory.BOFactory.New<IEDIMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Rejected;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;

			var emails = GetImportResultNotificationEmails(message, logger);
			Assert("Should contains the expected text from the NotificationEmailManager.", emails.All(c => c.Body.Contains("Add From EmailProcessorForTest")));
			Assert("Should contains the expected file name from the NotificationEmailManager.", emails.All(c => c.Attachments.Cast<AttachmentDef>().Any(d => d.DisplayName == "AttachMentFileForTest")));
		}

		public void TestEmailNotificationFormatUniversalShipment()
		{
			eServicesRegistry.Instance.ImportRejectedNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateGroupWithUser("rejected@test.com").PK.ToGuid());

			var message = Factory.BOFactory.New<IEDIMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Rejected;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;

			var emails = GetImportResultNotificationEmails(message);
			AssertEquals("Group notification created", 1, emails.Count);

			AssertEquals(1, emails[0].Recipients.Count);
			AssertEquals("rejected@test.com", emails[0].Recipients[0]);

			AssertEquals("XML Universal Shipment Rejected (REJ) – Message # 002 from [HYEDAUIKB]", emails[0].Subject);
			AssertContains(string.Format("<p><a href='{0}'><b>Message #</b> 002</a></p><p><b>Status:</b> REJ - Rejected</p><p><b>Data Import Log</b></p><p><ul>Warning - Warning 1<br/>Warning - Warning 2<br/>Warning - Warning 3<br/>Error - Error 1<br/>Error - Error 2<br/>Error - Error 3<br/>ImportResults<br/></ul><br/><br/>Please check message log for details. If you are unable to solve this problem, please forward this email to the CargoWise support team.</p><p><br/><br/><b>Notes</b><br/>You are receiving this Email because:<br/><ul><li>The registry item 'Import Rejected Notification Group' is configured to notify the group 'SGR - Call Me SGR' which you are a member of in this {1} System.</li></ul>If you are not supposed to receive this email, please contact your system administrator to change the corresponding notification group setting.</p>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Messaging.EDIMessage, message.PK.ToGuid()), Enterprise.Core.Constants.ProductName), emails[0].Body);

			AssertEquals("HTM", emails[0].ContentType.ContentTypeCode);
		}

		public void TestEmailNotificationFormatUniversalEvent()
		{
			eServicesRegistry.Instance.ImportRejectedNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateGroupWithUser("rejected@test.com").PK.ToGuid());

			var message = Factory.BOFactory.New<IEDIMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Rejected;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;

			var emails = GetImportResultNotificationEmails(message);
			AssertEquals("Group notification created", 1, emails.Count);

			AssertEquals(1, emails[0].Recipients.Count);
			AssertEquals("rejected@test.com", emails[0].Recipients[0]);

			AssertEquals("XML Universal Event Rejected (REJ) – Message # 002 from [HYEDAUIKB]", emails[0].Subject);
			AssertContains(string.Format("<p><a href='{0}'><b>Message #</b> 002</a></p><p><b>Status:</b> REJ - Rejected</p><p><b>Data Import Log</b></p><p><ul>Debug - Debug 1<br/>Info 1<br/>Info 2<br/>Info 3<br/>Warning - Warning 1<br/>Warning - Warning 2<br/>Warning - Warning 3<br/>Error - Error 1<br/>Error - Error 2<br/>Error - Error 3<br/>ImportResults<br/></ul><br/><br/>Please check message log for details. If you are unable to solve this problem, please forward this email to the CargoWise support team.</p><p><br/><br/><b>Notes</b><br/>You are receiving this Email because:<br/><ul><li>The registry item 'Import Rejected Notification Group (For Universal Events)' is configured to notify the group 'SGR - Call Me SGR' which you are a member of in this {1} System.</li></ul>If you are not supposed to receive this email, please contact your system administrator to change the corresponding notification group setting.</p>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Messaging.EDIMessage, message.PK.ToGuid()), Enterprise.Core.Constants.ProductName), emails[0].Body);
			AssertEquals("HTM", emails[0].ContentType.ContentTypeCode);
		}

		public void TestEmailNotificationFormatUniversalEvent_UsingEventNotificationGroup()
		{
			eServicesRegistry.Instance.ImportRejectedNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateGroupWithUser("rejected@test.com").PK.ToGuid());
			eServicesRegistry.Instance.ImportEventRejectedNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateGroupWithUser("rejectedEvent@test.com", "REJ", "USR", "event user").PK.ToGuid());

			var message = Factory.BOFactory.New<IEDIMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Rejected;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;

			var emails = GetImportResultNotificationEmails(message);
			AssertEquals("Group notification created", 1, emails.Count);

			AssertEquals(1, emails[0].Recipients.Count);
			AssertEquals("rejectedEvent@test.com", emails[0].Recipients[0]);

			AssertEquals("XML Universal Event Rejected (REJ) – Message # 002 from [HYEDAUIKB]", emails[0].Subject);
		}

		public void TestRejectedDontNotifyForInternalCommunication()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			var message = Factory.BOFactory.New<IEDIMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Rejected;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;
			var messageLogger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			var notifier = new ImportResultNotifier(message, messageLogger);
			notifier.Notify();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestRejectedNotificationDefault()
		{
			AssertEquals(0, GetImportResultNotificationEmails(EDIMessageStatusList.Codes.Rejected).Count);
		}

		public void TestRejectedNotification()
		{
			eServicesRegistry.Instance.ImportRejectedNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateGroupWithUser("rejected@test.com").PK.ToGuid());

			var emails = GetImportResultNotificationEmails(EDIMessageStatusList.Codes.Rejected);
			AssertEquals("Group notification created", 1, emails.Count);

			AssertEquals(1, emails[0].Recipients.Count);

			var email = emails[0];
			AssertEquals("rejected@test.com", email.Recipients[0]);
			AssertEquals("XML Universal Shipment Rejected (REJ) – Message # 002 from [HYEDAUIKB]", email.Subject);
			AssertContains($"You are receiving this Email because:<br/><ul><li>The registry item 'Import Rejected Notification Group' is configured to notify the group 'SGR - Call Me SGR' which you are a member of in this {Enterprise.Core.Constants.ProductName} System.</li></ul>", email.Body);
			AssertEquals("HTM", email.ContentType.ContentTypeCode);
		}

		public void TestDiscardedNotificationDefault()
		{
			AssertEquals(0, GetImportResultNotificationEmails(EDIMessageStatusList.Codes.Discarded).Count);
		}

		public void TestDiscardedNotification()
		{
			var groupId = CreateGroupWithUser("discarded@test.com").PK.ToGuid();
			eServicesRegistry.Instance.ImportDiscardedNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupId);

			var emails = GetImportResultNotificationEmails(EDIMessageStatusList.Codes.Discarded);
			AssertEquals("Group notification created", 1, emails.Count);

			var email = emails[0];
			AssertEquals("discarded@test.com", email.Recipients[0]);
			AssertEquals("XML Universal Shipment Discarded (DCD) – Message # 002 from [HYEDAUIKB]", email.Subject);
			AssertContains($"You are receiving this Email because:<br/><ul><li>The registry item 'Import Discarded Notification Group' is configured to notify the group 'SGR - Call Me SGR' which you are a member of in this {Enterprise.Core.Constants.ProductName} System.</li></ul>", email.Body);
			AssertEquals("HTM", email.ContentType.ContentTypeCode);
		}

		public void TestDiscardedNotificationFallback()
		{
			var groupId = CreateGroupWithUser("discarded@test.com").PK.ToGuid();
			eServicesRegistry.Instance.ImportDiscardedNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupId);

			var emails = GetImportResultNotificationEmails(EDIMessageStatusList.Codes.Discarded);
			AssertEquals("Group notification created", 1, emails.Count);

			var email = emails[0];
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("discarded@test.com", email.Recipients[0]);
			AssertEquals("XML Universal Shipment Discarded (DCD) – Message # 002 from [HYEDAUIKB]", email.Subject);
		}

		public void TestErrorNotificationGroupDefault()
		{
			AssertEquals(0, GetImportResultNotificationEmails(EDIMessageStatusList.Codes.Error).Count);
		}

		public void TestErrorNotificationUserNone()
		{
			var group = CreateGroupWithUser("error@test.com");

			var rule = new InboundMessageNotificationsRule()
			{
				NotifyGroup = group.PK,
				NotifyGroupWhenUserFound = true,
				NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.None
			};

			eServicesRegistry.Instance.ImportWithErrorsNotificationConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, rule);

			var emails = GetImportResultNotificationEmails(EDIMessageStatusList.Codes.Error);
			AssertEquals("Group notification created", 1, emails.Count);

			AssertEquals(1, emails[0].Recipients.Count);

			var email = emails[0];
			AssertEquals("error@test.com", email.Recipients[0]);
			AssertEquals("XML Universal Shipment Processed with Errors (ERR) – Message # 002 from [HYEDAUIKB]", email.Subject);
			AssertContains($"You are receiving this Email because:<br/><ul><li>The registry item 'Inbound Message Processed With Errors Notification Configuration' is configured to notify the group 'SGR - Call Me SGR' which you are a member of in this {Enterprise.Core.Constants.ProductName} System.</li></ul>", email.Body);
			AssertEquals("HTM", email.ContentType.ContentTypeCode);
		}

		public void TestErrorNotificationsUserLastEditing_WhatIfThereAreNoLogs()
		{
			var group = CreateGroupWithUser("error@test.com");

			var rule = new InboundMessageNotificationsRule()
			{
				NotifyGroup = group.PK,
				NotifyGroupWhenUserFound = true,
				NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.LastUserEditing
			};

			eServicesRegistry.Instance.ImportWithErrorsNotificationConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, rule);

			var message = Factory.BOFactory.New<IEDIMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Error;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			var messageLogger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var notifier = new ImportResultNotifier(message, messageLogger);
			notifier.Notify();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestErrorNotificationUserAllEditing()
		{
			var group = CreateGroupWithUser("error@test.com");
			var rule = new InboundMessageNotificationsRule()
			{
				NotifyGroup = group.PK,
				NotifyGroupWhenUserFound = true,
				NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.AllUsersEditing
			};
			eServicesRegistry.Instance.ImportWithErrorsNotificationConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, rule);

			var emails = GetImportResultNotificationEmails(EDIMessageStatusList.Codes.Error);

			AssertEquals(1, emails.Count);

			var email = emails[0];
			AssertEquals(3, email.Recipients.Count);

			var recipients = email.Recipients.Cast<RecipientDef>().OrderBy(r => r.Email).Select(r => r.Email).ToArray();

			AssertEquals("error@test.com", recipients[0]);
			AssertEquals("firsteditor@test.com", recipients[1]);
			AssertEquals("lasteditor@test.com", recipients[2]);
			AssertEquals("XML Universal Shipment Processed with Errors (ERR) – Message # 002 from [HYEDAUIKB]", email.Subject);
			AssertContains($"You are receiving this Email because:<br/><ul><li>The registry item 'Inbound Message Processed With Errors Notification Configuration' is configured to notify 'All Editors' for this {Enterprise.Core.Constants.ProductName} System.</li><li>", email.Body);
			AssertEquals("HTM", email.ContentType.ContentTypeCode);
		}

		public void TestErrorNotificationGroupWhenUserFound()
		{
			var group = CreateGroupWithUser("error@test.com");
			var rule = new InboundMessageNotificationsRule()
			{
				NotifyGroup = group.PK,
				NotifyGroupWhenUserFound = false,
				NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.AllUsersEditing
			};
			eServicesRegistry.Instance.ImportWithErrorsNotificationConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, rule);

			var emails = GetImportResultNotificationEmails(EDIMessageStatusList.Codes.Error);

			AssertEquals(1, emails.Count);

			var email = emails[0];
			AssertEquals(2, email.Recipients.Count);

			var recipients = email.Recipients.Cast<RecipientDef>().OrderBy(r => r.Email).Select(r => r.Email).ToArray();

			AssertEquals("firsteditor@test.com", recipients[0]);
			AssertEquals("lasteditor@test.com", recipients[1]);
			AssertEquals("XML Universal Shipment Processed with Errors (ERR) – Message # 002 from [HYEDAUIKB]", email.Subject);
			AssertContains($"You are receiving this Email because:<br/><ul><li>The registry item 'Inbound Message Processed With Errors Notification Configuration' is configured to notify 'All Editors' for this {Enterprise.Core.Constants.ProductName} System.</li></ul>", email.Body);
			AssertEquals("HTM", email.ContentType.ContentTypeCode);
		}

		public void TestWarningsNotificationGroupDefault()
		{
			AssertEquals(0, GetImportResultNotificationEmails(EDIMessageStatusList.Codes.Warning).Count);
		}

		public void TestWarningsNotificationUserNone()
		{
			var group = CreateGroupWithUser("warning@test.com");

			var rule = new InboundMessageNotificationsRule()
			{
				NotifyGroup = group.PK,
				NotifyGroupWhenUserFound = true,
				NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.None
			};

			eServicesRegistry.Instance.ImportWithWarningNotificationConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, rule);

			var emails = GetImportResultNotificationEmails(EDIMessageStatusList.Codes.Warning);
			AssertEquals("Group notification created", 1, emails.Count);

			var email = emails[0];
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("warning@test.com", email.Recipients[0]);
			AssertEquals("XML Universal Shipment Processed with Warnings (WAR) – Message # 002 from [HYEDAUIKB]", email.Subject);
			AssertContains($"You are receiving this Email because:<br/><ul><li>The registry item 'Inbound Message Processed With Warnings Notification Configuration' is configured to notify the group 'SGR - Call Me SGR' which you are a member of in this {Enterprise.Core.Constants.ProductName} System.</li></ul>", email.Body);
			AssertEquals("HTM", email.ContentType.ContentTypeCode);
		}

		public void TestWarningsNotificationUserAllEditing()
		{
			var group = CreateGroupWithUser("warning@test.com");
			var rule = new InboundMessageNotificationsRule()
			{
				NotifyGroup = group.PK,
				NotifyGroupWhenUserFound = true,
				NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.AllUsersEditing
			};
			eServicesRegistry.Instance.ImportWithWarningNotificationConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, rule);

			var emails = GetImportResultNotificationEmails(EDIMessageStatusList.Codes.Warning);

			AssertEquals(1, emails.Count);

			AssertEquals(3, emails[0].Recipients.Count);

			var recipients = emails[0].Recipients.Cast<RecipientDef>().OrderBy(r => r.Email).Select(r => r.Email).ToArray();

			AssertEquals("firsteditor@test.com", recipients[0]);
			AssertEquals("lasteditor@test.com", recipients[1]);
			AssertEquals("warning@test.com", recipients[2]);
		}

		public void TestWarningsNotificationGroupWhenUserFound()
		{
			var group = CreateGroupWithUser("warning@test.com");
			var rule = new InboundMessageNotificationsRule()
			{
				NotifyGroup = group.PK,
				NotifyGroupWhenUserFound = false,
				NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.AllUsersEditing
			};
			eServicesRegistry.Instance.ImportWithWarningNotificationConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, rule);

			var emails = GetImportResultNotificationEmails(EDIMessageStatusList.Codes.Warning);

			AssertEquals(1, emails.Count);

			AssertEquals(2, emails[0].Recipients.Count);

			var recipients = emails[0].Recipients.Cast<RecipientDef>().OrderBy(r => r.Email).Select(r => r.Email).ToArray();

			AssertEquals("firsteditor@test.com", recipients[0]);
			AssertEquals("lasteditor@test.com", recipients[1]);
		}

		public void TestProcessedOKNotificationGroupDefault()
		{
			AssertEquals(0, GetImportResultNotificationEmails(EDIMessageStatusList.Codes.ProcessedOK).Count);
		}

		public void TestProcessedOKNotificationUserNone()
		{
			var group = CreateGroupWithUser("ok@test.com");

			var rule = new InboundMessageNotificationsRule()
			{
				NotifyGroup = group.PK,
				NotifyGroupWhenUserFound = true,
				NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.None
			};

			eServicesRegistry.Instance.ImportOKNotificationConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, rule);

			var emails = GetImportResultNotificationEmails(EDIMessageStatusList.Codes.ProcessedOK);
			AssertEquals("Group notification created", 1, emails.Count);

			AssertEquals(1, emails[0].Recipients.Count);
			AssertEquals("ok@test.com", emails[0].Recipients[0]);
		}

		public void TestProcessedWithErrorUniversalEvent_Default()
		{
			var group = CreateGroupWithUser("error@test.com");
			var rule = new InboundMessageNotificationsRule()
			{
				NotifyGroup = group.PK,
				NotifyGroupWhenUserFound = false,
				NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.AllUsersEditing
			};
			eServicesRegistry.Instance.ImportWithErrorsNotificationConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, rule);

			var emails = GetImportResultNotificationEmails(EDIMessageStatusList.Codes.Error, EDIMessageSubTypeList.Codes.XmlUniversalEvent);

			AssertEquals(1, emails.Count);

			var email = emails[0];
			AssertEquals(2, email.Recipients.Count);

			var recipients = email.Recipients.Cast<RecipientDef>().OrderBy(r => r.Email).Select(r => r.Email).ToArray();

			AssertEquals("firsteditor@test.com", recipients[0]);
			AssertEquals("lasteditor@test.com", recipients[1]);
			AssertEquals("XML Universal Event Processed with Errors (ERR) – Message # 002 from [HYEDAUIKB]", email.Subject);
			AssertContains($"You are receiving this Email because:<br/><ul><li>The registry item 'Inbound Universal Event Processed With Errors Notification Configuration' is configured to notify 'All Editors' for this {Enterprise.Core.Constants.ProductName} System.</li></ul>", email.Body);
			AssertEquals("HTM", email.ContentType.ContentTypeCode);
		}

		public void TestProcessedWithErrorUniversalEvent_Override()
		{
			var group = CreateGroupWithUser("error@test.com");
			var rule = new InboundMessageNotificationsRule()
			{
				NotifyGroup = group.PK,
				NotifyGroupWhenUserFound = false,
				NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.AllUsersEditing
			};
			eServicesRegistry.Instance.ImportWithErrorsNotificationConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, rule);
			var eventGroup = CreateGroupWithUser("warningEvent@test.com", "WAR", "RAW", "Warning User");

			var eventRule = new InboundMessageNotificationsRule()
			{
				NotifyGroup = eventGroup.PK,
				NotifyGroupWhenUserFound = true,
				NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.None
			};

			eServicesRegistry.Instance.ImportEventWithErrorsNotificationConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, eventRule);
			var emails = GetImportResultNotificationEmails(EDIMessageStatusList.Codes.Error, EDIMessageSubTypeList.Codes.XmlUniversalEvent);

			AssertEquals(1, emails.Count);

			var email = emails[0];
			AssertEquals(1, email.Recipients.Count);

			var recipients = email.Recipients.Cast<RecipientDef>().OrderBy(r => r.Email).Select(r => r.Email).ToArray();

			AssertEquals("warningEvent@test.com", recipients[0]);
			AssertEquals("XML Universal Event Processed with Errors (ERR) – Message # 002 from [HYEDAUIKB]", email.Subject);
			AssertContains($"You are receiving this Email because:<br/><ul><li>The registry item 'Inbound Universal Event Processed With Errors Notification Configuration' is configured to notify the group 'WAR - Call Me WAR' which you are a member of in this {Enterprise.Core.Constants.ProductName} System.</li></ul>", email.Body);
			AssertEquals("HTM", email.ContentType.ContentTypeCode);
		}

		public void TestDiscardedNotificationUserLastEditing_XUS()
		{
			TestProcessedNotificationUserLastEditing_Core(EDIMessageStatusList.Codes.Discarded, EDIMessageSubTypeList.Codes.XmlUniversalShipment, emails => AssertionsForProcessedDiscardedNotificationUsersShouldSend(emails, "Shipment"));
		}

		public void TestDiscardedNotificationUserLastEditing_XUA()
		{
			TestProcessedNotificationUserLastEditing_Core(EDIMessageStatusList.Codes.Discarded, EDIMessageSubTypeList.Codes.XmlUniversalActivity, emails => AssertionsForProcessedDiscardedNotificationUsersShouldSend(emails, "Activity"));
		}

		public void TestDiscardedNotificationUserLastEditing_XUT()
		{
			TestProcessedNotificationUserLastEditing_Core(EDIMessageStatusList.Codes.Discarded, EDIMessageSubTypeList.Codes.XmlUniversalTransaction, emails => AssertionsForProcessedDiscardedNotificationUsersShouldSend(emails, "Transaction"));
		}

		public void TestDiscardedNotificationUserLastEditing_XUE()
		{
			TestProcessedNotificationUserLastEditing_Core(EDIMessageStatusList.Codes.Discarded, EDIMessageSubTypeList.Codes.XmlUniversalEvent, emails => AssertEquals("No email should be generated for XUE", 0, emails.Count));
		}

		public void TestWarningsNotificationUserLastEditing_XUS()
		{
			TestProcessedNotificationUserLastEditing_Core(EDIMessageStatusList.Codes.Warning, EDIMessageSubTypeList.Codes.XmlUniversalShipment, AssertionsForProcessedWarningsNotificationUsersShouldSend);
		}

		public void TestWarningsNotificationUserLastEditing_XUA()
		{
			TestProcessedNotificationUserLastEditing_Core(EDIMessageStatusList.Codes.Warning, EDIMessageSubTypeList.Codes.XmlUniversalActivity, AssertionsForProcessedWarningsNotificationUsersShouldSend);
		}

		public void TestWarningsNotificationUserLastEditing_XUT()
		{
			TestProcessedNotificationUserLastEditing_Core(EDIMessageStatusList.Codes.Warning, EDIMessageSubTypeList.Codes.XmlUniversalTransaction, AssertionsForProcessedWarningsNotificationUsersShouldSend);
		}

		public void TestWarningsNotificationUserLastEditing_XUE()
		{
			TestProcessedNotificationUserLastEditing_Core(EDIMessageStatusList.Codes.Warning, EDIMessageSubTypeList.Codes.XmlUniversalEvent, emails => AssertEquals("No email should be generated for XUE", 0, emails.Count));
		}

		public void TestProcessedOKNotificationUserLastEditing_XUS()
		{
			TestProcessedNotificationUserLastEditing_Core(EDIMessageStatusList.Codes.ProcessedOK, EDIMessageSubTypeList.Codes.XmlUniversalShipment, emails => AssertionsForProcessedOKNotificationUserShouldSend(emails, "Shipment"));
		}

		public void TestProcessedOKNotificationUserLastEditing_XUA()
		{
			TestProcessedNotificationUserLastEditing_Core(EDIMessageStatusList.Codes.ProcessedOK, EDIMessageSubTypeList.Codes.XmlUniversalActivity, emails => AssertionsForProcessedOKNotificationUserShouldSend(emails, "Activity"));
		}

		public void TestProcessedOKNotificationUserLastEditing_XUT()
		{
			TestProcessedNotificationUserLastEditing_Core(EDIMessageStatusList.Codes.ProcessedOK, EDIMessageSubTypeList.Codes.XmlUniversalTransaction, emails => AssertionsForProcessedOKNotificationUserShouldSend(emails, "Transaction"));
		}

		public void TestProcessedOKNotificationUserLastEditing_XUE()
		{
			TestProcessedNotificationUserLastEditing_Core(EDIMessageStatusList.Codes.ProcessedOK, EDIMessageSubTypeList.Codes.XmlUniversalEvent, emails => AssertEquals("No email should be generated for XUE", 0, emails.Count));
		}

		void TestProcessedNotificationUserLastEditing_Core(string messageStatus, string messageSubType, Action<List<EmailDef>> assertions)
		{
			var okGroup = CreateGroupWithUser("ok@test.com");

			var okRule = new InboundMessageNotificationsRule()
			{
				NotifyGroup = okGroup.PK,
				NotifyGroupWhenUserFound = true,
				NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.LastUserEditing
			};
			var warnGroup = CreateGroupWithUser("warning@test.com", "WAR", "RAW", "Warning User");

			var warnRule = new InboundMessageNotificationsRule()
			{
				NotifyGroup = warnGroup.PK,
				NotifyGroupWhenUserFound = true,
				NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.LastUserEditing
			};

			eServicesRegistry.Instance.ImportOKNotificationConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, okRule);
			eServicesRegistry.Instance.ImportWithWarningNotificationConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, warnRule);

			var groupId = CreateGroupWithUser("discarded@test.com", "DCD", "DCU", "Discard User").PK.ToGuid();
			eServicesRegistry.Instance.ImportDiscardedNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupId);

			assertions(GetImportResultNotificationEmails(messageStatus, messageSubType));
		}

		void AssertionsForProcessedDiscardedNotificationUsersShouldSend(List<EmailDef> emails, string type)
		{
			AssertEquals("Group notification created", 1, emails.Count);

			var email = emails[0];
			AssertEquals("discarded@test.com", email.Recipients[0]);
			AssertEquals($"XML Universal {type} Discarded (DCD) – Message # 002 from [HYEDAUIKB]", email.Subject);
			AssertContains($"You are receiving this Email because:<br/><ul><li>The registry item 'Import Discarded Notification Group' is configured to notify the group 'DCD - Call Me DCD' which you are a member of in this {Enterprise.Core.Constants.ProductName} System.</li></ul>", email.Body);
			AssertEquals("HTM", email.ContentType.ContentTypeCode);
		}

		void AssertionsForProcessedWarningsNotificationUsersShouldSend(List<EmailDef> emails)
		{
			AssertEquals(1, emails.Count);
			AssertEquals(2, emails[0].Recipients.Count);
			var recipients = emails[0].Recipients.Cast<RecipientDef>().OrderBy(r => r.Email).Select(r => r.Email).ToArray();
			AssertEquals("lasteditor@test.com", recipients[0]);
			AssertEquals("warning@test.com", recipients[1]);
		}

		void AssertionsForProcessedOKNotificationUserShouldSend(List<EmailDef> emails, string type)
		{
			AssertEquals(1, emails.Count);
			var email = emails[0];
			AssertEquals(2, email.Recipients.Count);
			var recipients = email.Recipients.Cast<RecipientDef>().OrderBy(r => r.Email).Select(r => r.Email).ToArray();

			AssertEquals("lasteditor@test.com", recipients[0]);
			AssertEquals("ok@test.com", recipients[1]);
			AssertEquals($"XML Universal {type} Processed OK (PRS) – Message # 002 from [HYEDAUIKB]", email.Subject);
			AssertContains($"You are receiving this Email because:<br/><ul><li>The registry item 'Inbound Message Processed OK Notification Configuration' is configured to notify the 'Last Editor' for this {Enterprise.Core.Constants.ProductName} System.</li><li>", email.Body);
			AssertEquals("HTM", email.ContentType.ContentTypeCode);
		}

		public void TestProcessedOKNotificationUserAllEditing()
		{
			var group = CreateGroupWithUser("ok@test.com");
			var rule = new InboundMessageNotificationsRule()
			{
				NotifyGroup = group.PK,
				NotifyGroupWhenUserFound = true,
				NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.AllUsersEditing
			};
			eServicesRegistry.Instance.ImportOKNotificationConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, rule);

			var emails = GetImportResultNotificationEmails(EDIMessageStatusList.Codes.ProcessedOK);

			AssertEquals(1, emails.Count);
			AssertEquals(3, emails[0].Recipients.Count);
			var recipients = emails[0].Recipients.Cast<RecipientDef>().OrderBy(r => r.Email).Select(r => r.Email).ToArray();

			AssertEquals("firsteditor@test.com", recipients[0]);
			AssertEquals("lasteditor@test.com", recipients[1]);
			AssertEquals("ok@test.com", recipients[2]);
		}

		public void TestProcessedOKNotificationGroupWhenUserFound()
		{
			var group = CreateGroupWithUser("ok@test.com");
			var rule = new InboundMessageNotificationsRule()
			{
				NotifyGroup = group.PK,
				NotifyGroupWhenUserFound = false,
				NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.AllUsersEditing
			};
			eServicesRegistry.Instance.ImportOKNotificationConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, rule);

			var emails = GetImportResultNotificationEmails(EDIMessageStatusList.Codes.ProcessedOK);

			AssertEquals(1, emails.Count);

			AssertEquals(2, emails[0].Recipients.Count);

			var recipients = emails[0].Recipients.Cast<RecipientDef>().OrderBy(r => r.Email).Select(r => r.Email).ToArray();

			AssertEquals("firsteditor@test.com", recipients[0]);
			AssertEquals("lasteditor@test.com", recipients[1]);
		}

		public void TestEmailNotificationFormatUniversalShipment_XmlElementShouldBeEscaped()
		{
			eServicesRegistry.Instance.ImportRejectedNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateGroupWithUser("rejected@test.com").PK.ToGuid());

			var message = Factory.BOFactory.New<IEDIMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Rejected;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;

			var messageLogger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			messageLogger.Log(LogType.Warning, "2024-06-26 02:34:49.550| Warning - Line 1: Element <Shipment>.<PortOfLoading> opened at line 1 was excluded as it was missing mandatory elements. Missing: Code.");
			var emails = GetImportResultNotificationEmails(message, messageLogger);
			AssertEquals("Group notification created", 1, emails.Count);

			AssertEquals(1, emails[0].Recipients.Count);
			AssertEquals("rejected@test.com", emails[0].Recipients[0]);

			AssertEquals("XML Universal Shipment Rejected (REJ) – Message # 002 from [HYEDAUIKB]", emails[0].Subject);
			AssertContains(string.Format("2024-06-26 02:34:49.550| Warning - Line 1: Element &lt;Shipment&gt;.&lt;PortOfLoading&gt; opened at line 1 was excluded as it was missing mandatory elements. Missing: Code."), emails[0].Body);

			AssertEquals("HTM", emails[0].ContentType.ContentTypeCode);
		}

		#region Implementation

		List<EmailDef> GetImportResultNotificationEmails(string messageStatus, string messageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment)
		{
			var message = Factory.BOFactory.New<IEDIMessage>();
			message.EM_Status = messageStatus;
			message.EM_MessageSubType = messageSubType;
			return GetImportResultNotificationEmails(message);
		}

		List<EmailDef> GetImportResultNotificationEmails(IEDIMessage message, XmlSessionTracker messageLogger = null)
		{
			var interchange = Factory.BOFactory.New<IEDIInterchange>();
			interchange.EI_From = "HYEDAUIKB";
			interchange.EI_To = "HEYDMZDAU";
			interchange.EI_ReceiveTransmit = "RCV";

			message.EM_MessageNum = "002";
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_EI = interchange.PK;
			message.EM_GB = Env.CurrentBranch.PK;

			messageLogger = messageLogger ?? new XmlSessionTracker(new ServiceTaskLogForTesting());
			messageLogger.Log(LogType.Debug, "Debug 1");
			messageLogger.Log(LogType.Information, "Info 1");
			messageLogger.Log(LogType.Information, "Info 2");
			messageLogger.Log(LogType.Information, "Info 3");
			messageLogger.Log(LogType.Warning, "Warning 1");
			messageLogger.Log(LogType.Warning, "Warning 2");
			messageLogger.Log(LogType.Warning, "Warning 3");
			messageLogger.Log(LogType.Error, "Error 1");
			messageLogger.Log(LogType.Error, "Error 2");
			messageLogger.Log(LogType.Error, "Error 3");
			messageLogger.Log(LogType.Information, "ImportResults");

			var firstEditUser = CreateStaff("FUR", "First user", "firsteditor@test.com");
			var lastEditUser = CreateStaff("LUR", "Last user", "lasteditor@test.com");

			var logs = ((EnterpriseBusinessObject)message).Logs;

			var firstEditEvent = logs.AddNew();
			using (firstEditEvent.LockForUpdatingKeyFieldsForTesting())
			{
				firstEditEvent.SL_GS_NKUser = firstEditUser.GS_Code;
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				firstEditEvent.SL_SE_NKEvent = Events.EditedARecordCode;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				firstEditEvent.SL_EventTime = ZDateTime.Now.AddMinutes(1);
			}

			var lastEditEvent = logs.AddNew();

			using (lastEditEvent.LockForUpdatingKeyFieldsForTesting())
			{
				lastEditEvent.SL_GS_NKUser = lastEditUser.GS_Code;
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				lastEditEvent.SL_SE_NKEvent = Events.EditedARecordCode;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
				lastEditEvent.SL_EventTime = ZDateTime.Now.AddMinutes(2);
			}

			using (Env.SetTemporaryUserContext(lastEditUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				message.EM_SystemLastEditUser = lastEditUser.GS_Code;
				Factory.SaveForTesting();
			}

			var notifier = new ImportResultNotifier(message, messageLogger);
			notifier.Notify();

			return Env.OutgoingMailManager.EmailsCreated;
		}

		GlbGroup CreateGroupWithUser(string userEmail, string groupName = "SGR", string staffCode = "SUR", string userName = "Test user")
		{
			var group = CreateGroup(groupName);
			var staff = CreateStaff(staffCode, userName, userEmail);
			CreateGroupLink(group, staff);
			return group;
		}

		GlbGroup CreateGroup(string groupName)
		{
			var result = Factory.New<GlbGroup>();
			result.GG_Code = groupName;
			result.GG_Desc = "Call Me " + groupName;
			Factory.SaveForTesting();
			return result;
		}

		GlbStaff CreateStaff(string code, string userName, string userEmail)
		{
			var result = Factory.New<GlbStaff>();
			result.GS_Code = code;
			result.GS_LoginName = userName;
			result.GS_IsSystemAccount = false;
			result.GS_EmailAddress = userEmail;
			Factory.SaveForTesting();
			return result;
		}

		void CreateGroupLink(GlbGroup group, GlbStaff staff)
		{
			var result = Factory.New<GlbGroupLink>();
			result.GK_GG = group.PK;
			result.GK_GS = staff.PK;
			Factory.SaveForTesting();
		}

		class EmailProcessorForTest : INotificationEmailProcessor
		{
			public HtmlEmailDef Process(IEDIMessage message, HtmlEmailDef htmlEmailDef)
			{
				htmlEmailDef.Attachments.Add(new AttachmentDef("AttachMentFileForTest", Array.Empty<byte>()));
				htmlEmailDef.Body += "Add From EmailProcessorForTest";

				return htmlEmailDef;
			}
		}

		#endregion
	}
}
