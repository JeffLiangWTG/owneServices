using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.Mail.Business.Test;
using Enterprise.Client.EDI.Registry;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.EConversation.Testing;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using MailManager;
using NUnit.Framework;
using static Enterprise.Core.Constants.CustomerService;
using Constants = CargoWise.EventReference.Constants;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.Mail.BatchProcessor.Testing;

public class CustomerServiceEmailProcessorTest : BusinessObjectEmailProcessorTestCase<CustomerServiceEmailProcessorForTest, SupportIncident>
{
	#region Create / Process

	public void TestEmailLoopDection()
	{
		EDIDataRegistry.Instance.CustomerSupportAutoReplyEmailTemplate = "ThisIsAutoReply";
		EDIDataRegistry.Instance.EnableEmailLoopDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		EDIDataRegistry.Instance.EmailLoopDetectingWindowDuration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
		EDIDataRegistry.Instance.EmailLoopMaximumEmailCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);

		var incident = Factory.NewWithValidTestData<SupportIncident>();
		incident.IM_IncidentNumber = "CS11111222";
		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();
		for (var i = 0; i < 5; i++)
		{
			var email = Factory.New<MailItem>();
			email.MI_Status = MailStatus.Processed;
			email.MI_Direction = MailDirection.Receive;
			email.MI_Application = processor.MailApplicationCode;
			email.MI_ReceivedDateTime = ZDateTime.UtcNow.AddMinutes(-2);
			email.MI_From = "User <123@123.com>";
			email.MI_Subject = $"{i}_XXXXXX {incident.Number} XXXXXXXX";
			email.MI_Body = $"Test{i}";
		}

		Factory.Save();

		var emailBuilder = new EmailBuilderForTesting()
			.From("User", "123@123.com")
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("TTTTTTestEmail")
			.WithAttachment("text1.txt", "Hi, this is an attachment. Cheers");

		var email1 = emailBuilder.GetEmail();
		processor.CreateAndProcessMailItem(email1, LoggerForTest);

		var expectedLogMessage = "Email(fix this CS11111222) from User <123@123.com> exceeds the maximum number of mail received (more than 5 emails in 10 minutes). CS11111222's notification was not disabled because of missing ALL tag.";

		Assert((LoggerForTest as EmailProcessorLoggerForTest).LogEntries.Any(x => x.Contains(expectedLogMessage)));
		AssertEquals("Should post error report", "Possible mail loop detected", ErrorReporter.LastKeyReported);
		AssertEquals("Should post error report", expectedLogMessage, ErrorReporter.LastMessageReported);
		Assert("Should not send auto reply", !Env.OutgoingMailManager.EmailsCreated.Any(x => x.Body.Contains("ThisIsAutoReply")));
		Assert("Notification should not be disabled because of missing ALL tag", !SupportIncidentEmailTriggeringRules.IsAllSuppressed(incident));

		var blnGroup = Factory.New<TagDefinition>();
		blnGroup.TGD_Code = "BLN";

		var magnitude = blnGroup.Magnitudes.AddNew();
		magnitude.TGM_Code = SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification;
		magnitude.TGM_IsActive = true;
		Factory.Save();

		var email2 = emailBuilder.Body("TestCaseOutOfLimitatiom").GetEmail();
		Env.OutgoingMailManager.EmailsCreated.Clear();
		ErrorReporter.Clear();
		processor.CreateAndProcessMailItem(email2, LoggerForTest);
		expectedLogMessage = "Email(fix this CS11111222) from User <123@123.com> exceeds the maximum number of mail received (more than 5 emails in 10 minutes). CS11111222's notification is disabled.";

		Assert("The email still needs to be processed", incident.EConversation.ExistingConversation.Messages.Any(x => x.Body.Contains("TestCaseOutOfLimitatiom")));
		Assert("Notification should be disabled because of missing ALL tag", SupportIncidentEmailTriggeringRules.IsAllSuppressed(incident));
		Assert((LoggerForTest as EmailProcessorLoggerForTest).LogEntries.Any(x => x.Contains(expectedLogMessage)));
		AssertEquals("Should post error report", "Possible mail loop detected", ErrorReporter.LastKeyReported);
		AssertEquals("Should post error report", expectedLogMessage, ErrorReporter.LastMessageReported);
		AssertEquals("Should not send any email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

		var email3 = emailBuilder.Body("SendAgainAfterSuppressing").GetEmail();
		ErrorReporter.Clear();
		processor.CreateAndProcessMailItem(email3, LoggerForTest);
		Assert("The email still needs to be processed", incident.EConversation.ExistingConversation.Messages.Any(x => x.Body.Contains("SendAgainAfterSuppressing")));
		Assert((LoggerForTest as EmailProcessorLoggerForTest).LogEntries.Any(x => x.Contains("Email(fix this CS11111222) from User <123@123.com> will be added to eDoc, but CSE will not send replies because incident notification is disabled")));
		AssertNullOrEmpty(ErrorReporter.LastKeyReported);
		AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		AssertEquals("Should not send any email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
	}

	public void TestValidateFromEmailAddress()
	{
		var method = typeof(CustomerServiceEmailProcessor).GetMethod("ValidateEmailWithSender", BindingFlags.Instance | BindingFlags.NonPublic);
		var processor = new CustomerServiceEmailProcessorForTest();
		AssertNotNull(method);

		var incidentClientOrg = Factory.NewWithValidTestData<OrgHeader>();
		var nonIncidentClientOrg = Factory.NewWithValidTestData<OrgHeader>();
		Factory.Save();
		var rightOHClient = incidentClientOrg.PK;
		var wrongOHClient = nonIncidentClientOrg.PK;
		AssertNotEquals(rightOHClient, wrongOHClient);

		var incidentClientOrgContact = incidentClientOrg.Contacts.AddNew();
		incidentClientOrgContact.OC_ContactName = "active";
		incidentClientOrgContact.OC_Email = "activeOC@123.com";
		incidentClientOrgContact.OC_IsActive = true;
		incidentClientOrgContact.OC_OH = rightOHClient;

		var nonIncidentClientOrgContact = incidentClientOrg.Contacts.AddNew();
		nonIncidentClientOrgContact.OC_ContactName = "active";
		nonIncidentClientOrgContact.OC_Email = "activeNOC@123.com";
		nonIncidentClientOrgContact.OC_IsActive = true;
		nonIncidentClientOrgContact.OC_OH = wrongOHClient;

		var inactivateIncidentClientOrgContact = incidentClientOrg.Contacts.AddNew();
		inactivateIncidentClientOrgContact.OC_ContactName = "inactive";
		inactivateIncidentClientOrgContact.OC_Email = "inactiveOC@123.com";
		inactivateIncidentClientOrgContact.OC_IsActive = false;
		inactivateIncidentClientOrgContact.OC_OH = rightOHClient;

		var inactivateNonIncidentClientOrgContact = incidentClientOrg.Contacts.AddNew();
		inactivateNonIncidentClientOrgContact.OC_ContactName = "inactive";
		inactivateNonIncidentClientOrgContact.OC_Email = "inactiveNOC@123.com";
		inactivateNonIncidentClientOrgContact.OC_IsActive = false;
		inactivateNonIncidentClientOrgContact.OC_OH = wrongOHClient;

		var activeStaff = Factory.NewWithValidTestData<GlbStaff>();
		activeStaff.GS_EmailAddress = "activeGS@123.com";
		activeStaff.GS_IsActive = true;

		var inactiveStaff = Factory.NewWithValidTestData<GlbStaff>();
		inactiveStaff.GS_EmailAddress = "inactiveGS@123.com";
		inactiveStaff.GS_IsActive = false;

		var incident = Factory.NewWithValidTestData<SupportIncident>();
		incident.IM_OH_Client = rightOHClient;
		var participant1 = incident.EConversation.Conversation.Participants.AddNew();
		participant1.JCP_EmailAddress = "AAA@123.com";

		Factory.Save();

		var mailItem = Factory.NewWithValidTestData<MailItem>();
		mailItem.MI_From = "aaa@123.COM";
		Assert("Email is not case sensitive", (bool)method.Invoke(processor, new object[] { mailItem, incident, new StringBuilder() }));

		mailItem.MI_From = "ACTIVEoc@123.COM";
		Assert("The active and right OC_OH contact email address included in the incident client should pass the validation", (bool)method.Invoke(processor, new object[] { mailItem, incident, new StringBuilder() }));
		mailItem.MI_From = "ACTIVEnoc@123.COM";
		Assert("The active but wrong OC_OH contact email address included in the incident client should pass the validation", !(bool)method.Invoke(processor, new object[] { mailItem, incident, new StringBuilder() }));
		mailItem.MI_From = "INACTIVEoc@123.COM";
		Assert("The right OC_OH contact but inactive email address included in the incident client should pass the validation", !(bool)method.Invoke(processor, new object[] { mailItem, incident, new StringBuilder() }));
		mailItem.MI_From = "INACTIVEnoc@123.COM";
		Assert("The inactive and wrong OC_OH contact email address included in the incident client should pass the validation", !(bool)method.Invoke(processor, new object[] { mailItem, incident, new StringBuilder() }));

		mailItem.MI_From = "ACTIVEgs@123.COM";
		Assert("Active staff should pass the validation", (bool)method.Invoke(processor, new object[] { mailItem, incident, new StringBuilder() }));
		mailItem.MI_From = "INACTIVEgs@123.COM";
		Assert("Inactive staff should not pass the validation", !(bool)method.Invoke(processor, new object[] { mailItem, incident, new StringBuilder() }));
		mailItem.MI_From = "unknown@123.COM";
		Assert("Unknown external sender should not pass the validation", !(bool)method.Invoke(processor, new object[] { mailItem, incident, new StringBuilder() }));
	}

	public void TestProcessCustomerServiceEmail_ValidEmailWithSender()
	{
		var client1 = Factory.New<OrgHeader>();
		client1.OH_Code = "TRT";
		client1.OH_FullName = "Name1";
		var contact = Factory.New<OrgContact>();
		contact.OC_OH = client1.PK;
		contact.OC_Phone = "9876 5432";
		contact.OC_Email = "test2@cargowise.com";

		var incident = Factory.New<SupportIncident>();
		incident.IM_IncidentNumber = "CS11111222";
		incident.IM_OH_Client = client1.PK;

		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "TTT";
		staff.GS_EmailAddress = "test3@cargowise.com";

		var convo = incident.EConversation.Conversation;
		convo.RelatedParties.AddNewParticipant("test1@cargowise.com");

		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();
		processor.NeedToSkipEmailSourceVerification = false;
		var email1 = new EmailBuilderForTesting()
			.From("test", "Test1@cargowise.com")
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email with subject.1")
			.WithAttachment("text1.txt", "Hi, this is an attachment. Cheers")
			.GetEmail();
		var email2 = new EmailBuilderForTesting()
			.From("test2", "test2@cargowise.com")
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email with subject.2")
			.WithAttachment("text2.txt", "Hi, this is an attachment. Cheers2")
			.GetEmail();
		var email3 = new EmailBuilderForTesting()
			.From("test3", "test3@cargowise.com")
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email with subject.3")
			.WithAttachment("text3.txt", "Hi, this is an attachment. Cheers3")
			.GetEmail();
		var email4 = new EmailBuilderForTesting()
			.From("test4", "test4@cargowise.com")
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email with subject.4")
			.WithAttachment("text4.txt", "Hi, this is an attachment. Cheers4")
			.GetEmail();

		var result = processor.CreateAndProcessMailItem(email1, LoggerForTest);
		var edoc1 = incident.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text1.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should be added to eConversation since the sender is an eConversation participant", convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi, this is an email with subject.1\r\nAttachment 1: [text1.txt]")));
		Assert("Email attachment should be added to eConversation since the sender is an eConversation participant", convo.Messages.Any(tup => tup.JCM_Body.Contains($"[text1.txt]({ShowStorageDocUrlHandler.Instance.Create(edoc1)})")));

		result = processor.CreateAndProcessMailItem(email2, LoggerForTest);
		var edoc2 = incident.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text2.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should be added to eConversation because the sender is an OrgContact", convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi, this is an email with subject.2\r\nAttachment 1: [text2.txt]")));
		Assert("Email attachment should be added to eConversation because the sender is an OrgContact", convo.Messages.Any(tup => tup.JCM_Body.Contains($"[text2.txt]({ShowStorageDocUrlHandler.Instance.Create(edoc2)})")));

		result = processor.CreateAndProcessMailItem(email3, LoggerForTest);
		var edoc3 = incident.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text3.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should be added to eConversation because the sender is a GlbStaff", convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi, this is an email with subject.3\r\nAttachment 1: [text3.txt]")));
		Assert("Email attachment should be added to eConversation because the sender is a GlbStaff", convo.Messages.Any(tup => tup.JCM_Body.Contains($"[text3.txt]({ShowStorageDocUrlHandler.Instance.Create(edoc3)})")));

		result = processor.CreateAndProcessMailItem(email4, LoggerForTest);
		var edoc4 = incident.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text4.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should not be in conversation because this sender's address cannot be found in the database", !convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi, this is an email with subject.4\r\nAttachment 1: [text4.txt]")));
		Assert("Email attachment not be in conversation because this sender's address cannot be found in the database", edoc4 == null);

		var expectedLogs =
@"Sender email address Test1@cargowise.com is in the valid list.

Email attached to: CS11111222
Sender email address test2@cargowise.com is in the valid list.

Email attached to: CS11111222
Sender email address test3@cargowise.com is in the valid list.

Email attached to: CS11111222
Sender email address test4@cargowise.com is not in the valid list.
Email doesn't have hidden token (uniqueId).
Email doesn't have displayed token (uniqueId).

Email is not attached to CS11111222.";
		var actualLogs = string.Join(System.Environment.NewLine, ((EmailProcessorLoggerForTest)LoggerForTest).LogEntries);
		AssertEquals(expectedLogs, actualLogs);
	}

	public void TestProcessCustomerServiceEmail_ValidEmailWithUniqueID()
	{
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "TTT";
		staff.GS_EmailAddress = "test3@cargowise.com";
		var incident = Factory.NewWithValidTestData<SupportIncident>();
		incident.IM_IncidentNumber = "CS11111222";

		var incident2 = Factory.NewWithValidTestData<SupportIncident>();
		incident2.IM_IncidentNumber = "CS00003333";
		Factory.Save();
		var processor = new CustomerServiceEmailProcessorForTest();
		processor.NeedToSkipEmailSourceVerification = false;

		var uniqueEmailId = CustomerServiceEmailUniqueIdUtil.GenerateUniqueEmailID(incident);
		var uniqueEmailId2 = CustomerServiceEmailUniqueIdUtil.GenerateUniqueEmailID(incident2);
		var generateUniqueEmailId = uniqueEmailId;
		var mark = string.Format(IncidentConstants.MarkContent, $"m_20234ASTXwtzg{generateUniqueEmailId}m_20234ASTXwtzg");
		var mark2 = string.Format(IncidentConstants.MarkContent, 123);

		var email1 = new EmailBuilderForTesting()
			.From("test1", "test1@cargowise.com")
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email with subject.1 " + mark)
			.WithAttachment("text1.txt", "Hi, this is an attachment. Cheers")
			.GetEmail();
		var email2 = new EmailBuilderForTesting()
			.From("test2", "test2@cargowise.com")
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email with subject.2 " + mark2)
			.WithAttachment("text2.txt", "Hi, this is an attachment. Cheers2")
			.GetEmail();
		var email3 = new EmailBuilderForTesting()
			.From("test3", "test3@cargowise.com")
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email with subject.3 " + mark2)
			.WithAttachment("text3.txt", "Hi, this is an attachment. Cheers3")
			.GetEmail();
		var email4 = new EmailBuilderForTesting()
			.From("test4", "test2@cargowise.com")
			.Subject("fix this " + incident2.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email which is taking another incident's unique ID " + mark)
			.WithAttachment("text4.txt", "Hi, this is an attachment. Cheers4")
			.GetEmail();

		var result = processor.CreateAndProcessMailItem(email1, LoggerForTest);

		var convo = incident.EConversation.Conversation;
		var edoc1 = incident.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text1.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should be in conversation because it contains an valid ID", convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi, this is an email with subject.1 " + mark + "\r\nAttachment 1: [text1.txt]")));
		Assert("Email attachment should be in conversation because it contains an valid ID", convo.Messages.Any(tup => tup.JCM_Body.Contains($"[text1.txt]({ShowStorageDocUrlHandler.Instance.Create(edoc1)})")));

		result = processor.CreateAndProcessMailItem(email2, LoggerForTest);
		var edoc2 = incident.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text2.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should not be in conversation because the email contains an invalid ID and the sender cannot be found in the database.", !convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi, this is an email with subject.2 " + mark2 + "\r\nAttachment 1: [text2.txt]")));
		Assert("Email attachment should not be in conversation because the email contains an invalid ID and the sender cannot be found in the database.", edoc2 == null);

		result = processor.CreateAndProcessMailItem(email3, LoggerForTest);
		var edoc3 = incident.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text3.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should be in conversation because the sender can be found in the database", convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi, this is an email with subject.3 " + mark2 + "\r\nAttachment 1: [text3.txt]")));
		Assert("Email attachment should be in conversation because the sender can be found in the database", convo.Messages.Any(tup => tup.JCM_Body.Contains($"[text3.txt]({ShowStorageDocUrlHandler.Instance.Create(edoc3)})")));

		result = processor.CreateAndProcessMailItem(email4, LoggerForTest);
		var edoc4 = incident2.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text4.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should not be in conversation because the email is taking another incident's ID and sender cannot be found in the database.", !incident.EConversation.Conversation.Messages.Any(tup => tup.JCM_Body.Contains("this is an email which is taking another incident's unique ID " + mark2 + "\r\nAttachment 1: [text4.txt]")));
		Assert("Email attachment should not be in conversation because the email is taking another incident's ID and sender cannot be found in the database.", !incident.EConversation.Conversation.Messages.Any(tup => tup.JCM_Body.Contains($"[text4.txt]({ShowStorageDocUrlHandler.Instance.Create(edoc3)})")));

		var expectedLogs =
$@"Sender email address test1@cargowise.com is not in the valid list.
Email ID {uniqueEmailId} matches original ID m_20234ASTXwtzg{generateUniqueEmailId}m_20234ASTXwtzg.

Email attached to: CS11111222
Sender email address test2@cargowise.com is not in the valid list.
Email ID {uniqueEmailId} doesn't match original ID 123.
Email doesn't have displayed token (uniqueId).

Email is not attached to CS11111222.
Sender email address test3@cargowise.com is in the valid list.

Email attached to: CS11111222
Sender email address test2@cargowise.com is not in the valid list.
Email ID {uniqueEmailId2} doesn't match original ID m_20234ASTXwtzg{generateUniqueEmailId}m_20234ASTXwtzg.
Email doesn't have displayed token (uniqueId).

Email is not attached to CS00003333.";
		var actualLogs = string.Join(System.Environment.NewLine, ((EmailProcessorLoggerForTest)LoggerForTest).LogEntries);
		AssertEquals(expectedLogs, actualLogs);
	}

	public void TestProcessCustomerServiceEmail_ValidateEmailWithUniqueBody()
	{
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "TTT";
		staff.GS_EmailAddress = "test3@cargowise.com";
		var incident = Factory.NewWithValidTestData<SupportIncident>();
		incident.IM_IncidentNumber = "CS11111222";

		var incident2 = Factory.NewWithValidTestData<SupportIncident>();
		incident2.IM_IncidentNumber = "CS00003333";
		Factory.Save();
		var processor = new CustomerServiceEmailProcessorForTest();
		processor.NeedToSkipEmailSourceVerification = false;

		var uniqueEmailId = CustomerServiceEmailUniqueIdUtil.GenerateUniqueEmailID(incident);
		var mark = uniqueEmailId;
		var mark2 = "123";

		var email1 = new EmailBuilderForTesting()
			.From("test1", "test1@cargowise.com")
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email with subject.1 " + mark)
			.WithAttachment("text1.txt", "Hi, this is an attachment. Cheers")
			.GetEmail();
		var email2 = new EmailBuilderForTesting()
			.From("test2", "test2@cargowise.com")
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email with subject.2 " + mark2)
			.WithAttachment("text2.txt", "Hi, this is an attachment. Cheers2")
			.GetEmail();
		var email3 = new EmailBuilderForTesting()
			.From("test3", "test3@cargowise.com")
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email with subject.3 " + mark2)
			.WithAttachment("text3.txt", "Hi, this is an attachment. Cheers3")
			.GetEmail();
		var email4 = new EmailBuilderForTesting()
			.From("test4", "test2@cargowise.com")
			.Subject("fix this " + incident2.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email which is taking another incident's unique ID " + mark)
			.WithAttachment("text4.txt", "Hi, this is an attachment. Cheers4")
			.GetEmail();

		var result = processor.CreateAndProcessMailItem(email1, LoggerForTest);

		var convo = incident.EConversation.Conversation;
		var edoc1 = incident.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text1.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should be in conversation because it contains an valid ID", convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi, this is an email with subject.1 " + mark + "\r\nAttachment 1: [text1.txt]")));
		Assert("Email attachment should be in conversation because it contains an valid ID", convo.Messages.Any(tup => tup.JCM_Body.Contains($"[text1.txt]({ShowStorageDocUrlHandler.Instance.Create(edoc1)})")));

		result = processor.CreateAndProcessMailItem(email2, LoggerForTest);
		var edoc2 = incident.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text2.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should not be in conversation because the email contains an invalid ID and the sender cannot be found in the database.", !convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi, this is an email with subject.2 " + mark2 + "\r\nAttachment 1: [text2.txt]")));
		Assert("Email attachment should not be in conversation because the email contains an invalid ID and the sender cannot be found in the database.", edoc2 == null);

		result = processor.CreateAndProcessMailItem(email3, LoggerForTest);
		var edoc3 = incident.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text3.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should be in conversation because the sender can be found in the database", convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi, this is an email with subject.3 " + mark2 + "\r\nAttachment 1: [text3.txt]")));
		Assert("Email attachment should be in conversation because the sender can be found in the database", convo.Messages.Any(tup => tup.JCM_Body.Contains($"[text3.txt]({ShowStorageDocUrlHandler.Instance.Create(edoc3)})")));

		result = processor.CreateAndProcessMailItem(email4, LoggerForTest);
		var edoc4 = incident2.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text4.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should not be in conversation because the email is taking another incident's ID and sender cannot be found in the database.", !incident.EConversation.Conversation.Messages.Any(tup => tup.JCM_Body.Contains("this is an email which is taking another incident's unique ID " + mark2 + "\r\nAttachment 1: [text4.txt]")));
		Assert("Email attachment should not be in conversation because the email is taking another incident's ID and sender cannot be found in the database.", !incident.EConversation.Conversation.Messages.Any(tup => tup.JCM_Body.Contains($"[text4.txt]({ShowStorageDocUrlHandler.Instance.Create(edoc3)})")));

		var expectedLogs =
$@"Sender email address test1@cargowise.com is not in the valid list.
Email doesn't have hidden token (uniqueId).
Email ID {uniqueEmailId} matches original ID {uniqueEmailId}.

Email attached to: CS11111222
Sender email address test2@cargowise.com is not in the valid list.
Email doesn't have hidden token (uniqueId).
Email doesn't have displayed token (uniqueId).

Email is not attached to CS11111222.
Sender email address test3@cargowise.com is in the valid list.

Email attached to: CS11111222
Sender email address test2@cargowise.com is not in the valid list.
Email doesn't have hidden token (uniqueId).
Email doesn't have displayed token (uniqueId).

Email is not attached to CS00003333.";
		var actualLogs = string.Join(System.Environment.NewLine, ((EmailProcessorLoggerForTest)LoggerForTest).LogEntries);
		AssertEquals(expectedLogs, actualLogs);
	}

	public void TestProcessCustomerServiceEmail_ValidateEmailWithUniqueBodyAndDiv()
	{
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "TTT";
		staff.GS_EmailAddress = "test3@cargowise.com";
		var incident = Factory.NewWithValidTestData<SupportIncident>();
		incident.IM_IncidentNumber = "CS11111222";

		Factory.Save();
		var processor = new CustomerServiceEmailProcessorForTest();
		processor.NeedToSkipEmailSourceVerification = false;

		var uniqueEmailId = CustomerServiceEmailUniqueIdUtil.GenerateUniqueEmailID(incident);
		var generateUniqueEmailId = uniqueEmailId;

		var mark = uniqueEmailId;
		var div = string.Format(IncidentConstants.MarkContent, $"m_20234ASTXwtzg{generateUniqueEmailId}m_20234ASTXwtzg");
		var mark2 = "123";
		var div2 = string.Format(IncidentConstants.MarkContent, $"123");

		var email1 = new EmailBuilderForTesting()
			.From("test1", "test1@cargowise.com")
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email with subject.1 " + mark + " " + div)
			.WithAttachment("text1.txt", "Hi, this is an attachment. Cheers")
			.GetEmail();
		var email2 = new EmailBuilderForTesting()
			.From("test2", "test2@cargowise.com")
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email with subject.2 " + mark2 + " " + div2)
			.WithAttachment("text2.txt", "Hi, this is an attachment. Cheers2")
			.GetEmail();
		var email3 = new EmailBuilderForTesting()
			.From("test3", "test3@cargowise.com")
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email with subject.3 " + mark2 + " " + div2)
			.WithAttachment("text3.txt", "Hi, this is an attachment. Cheers3")
			.GetEmail();
		var email4 = new EmailBuilderForTesting()
			.From("test4", "test2@cargowise.com")
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email which is has the correct Id in the body, but an incorrect Id in the div" + mark + " " + div2)
			.WithAttachment("text4.txt", "Hi, this is an attachment. Cheers4")
			.GetEmail();

		var result = processor.CreateAndProcessMailItem(email1, LoggerForTest);

		var convo = incident.EConversation.Conversation;
		var edoc1 = incident.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text1.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should be in conversation because it contains an valid ID", convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi, this is an email with subject.1 " + mark + " " + div + "\r\nAttachment 1: [text1.txt]")));
		Assert("Email attachment should be in conversation because it contains an valid ID", convo.Messages.Any(tup => tup.JCM_Body.Contains($"[text1.txt]({ShowStorageDocUrlHandler.Instance.Create(edoc1)})")));

		result = processor.CreateAndProcessMailItem(email2, LoggerForTest);
		var edoc2 = incident.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text2.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should not be in conversation because the email contains an invalid ID and the sender cannot be found in the database.", !convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi, this is an email with subject.2 " + mark2 + " " + div2 + "\r\nAttachment 1: [text2.txt]")));
		Assert("Email attachment should not be in conversation because the email contains an invalid ID and the sender cannot be found in the database.", edoc2 == null);

		result = processor.CreateAndProcessMailItem(email3, LoggerForTest);
		var edoc3 = incident.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text3.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should be in conversation because the sender can be found in the database", convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi, this is an email with subject.3 " + mark2 + " " + div2 + "\r\nAttachment 1: [text3.txt]")));
		Assert("Email attachment should be in conversation because the sender can be found in the database", convo.Messages.Any(tup => tup.JCM_Body.Contains($"[text3.txt]({ShowStorageDocUrlHandler.Instance.Create(edoc3)})")));

		result = processor.CreateAndProcessMailItem(email4, LoggerForTest);
		var edoc4 = incident.DocManagerInfo.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName.EqualsIgnoringCase("text4.txt"));
		Assert("Email should be always processed", result);
		Assert("Email body should be in conversation because it contains a valid ID.", !incident.EConversation.Conversation.Messages.Any(tup => tup.JCM_Body.Contains("this is an email which is taking another incident's unique ID " + mark + " " + div2 + "\r\nAttachment 1: [text4.txt]")));
		Assert("Email attachment should be in conversation because it contains a valid ID.", !incident.EConversation.Conversation.Messages.Any(tup => tup.JCM_Body.Contains($"[text4.txt]({ShowStorageDocUrlHandler.Instance.Create(edoc3)})")));

		var expectedLogs =
$@"Sender email address test1@cargowise.com is not in the valid list.
Email ID {uniqueEmailId} matches original ID m_20234ASTXwtzg{uniqueEmailId}m_20234ASTXwtzg.

Email attached to: CS11111222
Sender email address test2@cargowise.com is not in the valid list.
Email ID {uniqueEmailId} doesn't match original ID 123.
Email doesn't have displayed token (uniqueId).

Email is not attached to CS11111222.
Sender email address test3@cargowise.com is in the valid list.

Email attached to: CS11111222
Sender email address test2@cargowise.com is not in the valid list.
Email ID {uniqueEmailId} doesn't match original ID 123.
Email ID {uniqueEmailId} matches original ID {uniqueEmailId}.

Email attached to: CS11111222";
		var actualLogs = string.Join(System.Environment.NewLine, ((EmailProcessorLoggerForTest)LoggerForTest).LogEntries);
		AssertEquals(expectedLogs, actualLogs);
	}

	public void TestProcessCustomerServiceEmails_NoIncidentNumber()
	{
		var processor = new CustomerServiceEmailProcessorForTest();

		Email email = new EmailBuilderForTesting()
			.Subject("Hello fix this")
			.To("Someone", "test@edi.com.au")
			.Body("please fix this incident now")
			.GetEmail();

		processor.CreateAndProcessMailItem(email, LoggerForTest);

		MailItem mailItem = GetLastCreatedMailItem();
		AssertEquals("Mail Item MI_Application have to be 'CSV'", "CSV", mailItem.MI_Application);
		AssertEquals("Mail Item status is unprocessed as no incident was found", MailStatus.Unprocessed, mailItem.MI_Status);
		AssertEquals("Mail Item direction is set to receive", MailDirection.Receive, mailItem.MI_Direction);
		AssertEquals("saved", true, mailItem.IsInDatabase);

		AssertEquals("No outgoing emails should be created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
	}

	public void TestProcessCustomerServiceEmails_WithAutoReplyMessage()
	{
		var processor = new CustomerServiceEmailProcessorForTest();

		string autoReplyMail = @"Received: from mail-wi0-f169.google.com ([209.85.212.169]) by wisetechglobal.com with MailEnable ESMTP; Fri, 23 Jan 2015 15:11:43 +1100
Received: by mail-wi0-f169.google.com with SMTP id bs8so624089wib.0
        for <mailtester@enterprisedevelopment.cargowise.com>; Thu, 22 Jan 2015 20:12:04 -0800 (PST)
DKIM-Signature: v=1; a=rsa-sha256; c=relaxed/relaxed;
        d=gmail.com; s=20120113;
        h=to:from:date:message-id:subject:mime-version:content-type
         :content-transfer-encoding:content-disposition:precedence
         :auto-submitted;
        bh=KUa66/ZeC3eoAzJitfNvWcfwOQjOep7khGsgrEY9Mv8=;
        b=Kcro7FVFAmuNtOWtrILgAPgXSf1kePtLi4EIeWW+cywz8zTgyFccmfJD+8ipaeUI8H
         3VsU6tW8jsMqqVIeywLNJHzyBOTFiZQeyWRnqTyDhfOZ//0Gg8n2cttt7TiatTJbeVh4
         pvspcdoZl8rkJhs+IGkLhJHz5JI0HrqANIkpEP9p0ikNJ4NpC5MpTouguI4Fyyee7akQ
         iPxHC6XwwT3M0hi5GlU7wYsbvXMeNCe0NgS99+UFmUjxLVzWA4MeQjdn3tAm+IV/P2hX
         7H3ocmx/c2Aa4nrlXm5ZOOOGLYFKAiNk6HkJ2GaRcRw8MFx6ol8jPB9GA08L7z5+ZW0f
         JQng==
X-Received: by 10.194.242.6 with SMTP id wm6mr9847047wjc.7.1421986324609;
        Thu, 22 Jan 2015 20:12:04 -0800 (PST)
To: mailtester@enterprisedevelopment.cargowise.com
From: ""Edward Onwodi"" <edward.onwodi@gmail.com>
Date: Thu, 22 Jan 2015 20:12:04 -0800
Message-ID: <CADUmwtvXPYanvKeivuwc5X2w9AJUqaRkBMY3rb6zhfaAdLJZdA@mail.gmail.com>
Subject: I'm out of office Re: Notification of Incident: CS00000002 - Simple bug
MIME-Version: 1.0
Content-Type: text/html; charset=UTF-8
Content-Transfer-Encoding: 7bit
Content-Disposition: inline
Precedence: bulk
X-Autoreply: yes
Auto-Submitted: auto-replied
Return-Path: <edward.onwodi@gmail.com>

<div dir=""ltr""><div>I&#39;m currently out of office and would have limited access to mails. I will respond as soon as possible.</div><div><br></div><div>Thank you.</div></div>

";

		Email email = new Email(Encoding.UTF8.GetBytes(autoReplyMail));

		processor.CreateAndProcessMailItem(email, LoggerForTest);

		MailItem mailItem = GetLastCreatedMailItem();
		AssertNull(mailItem);
		Assert(email.IsEmailProcessingSkipped);
		AssertEquals("No outgoing emails should be created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
	}

	public void TestProcessCustomerServiceEmails_WithGeneratedMessageSource()
	{
		var processor = new CustomerServiceEmailProcessorForTest();

		var autoReplyMail = @"Received: from mail-wi0-f169.google.com ([209.85.212.169]) by wisetechglobal.com with MailEnable ESMTP; Fri, 23 Jan 2015 15:11:43 +1100
Received: by mail-wi0-f169.google.com with SMTP id bs8so624089wib.0
		for <mailtester@enterprisedevelopment.cargowise.com>; Thu, 22 Jan 2015 20:12:04 -0800 (PST)
DKIM-Signature: v=1; a=rsa-sha256; c=relaxed/relaxed;
		d=gmail.com; s=20120113;
		h=to:from:date:message-id:subject:mime-version:content-type
		:content-transfer-encoding:content-disposition:precedence
		:auto-submitted;
		bh=KUa66/ZeC3eoAzJitfNvWcfwOQjOep7khGsgrEY9Mv8=;
		b=Kcro7FVFAmuNtOWtrILgAPgXSf1kePtLi4EIeWW+cywz8zTgyFccmfJD+8ipaeUI8H
		3VsU6tW8jsMqqVIeywLNJHzyBOTFiZQeyWRnqTyDhfOZ//0Gg8n2cttt7TiatTJbeVh4
		pvspcdoZl8rkJhs+IGkLhJHz5JI0HrqANIkpEP9p0ikNJ4NpC5MpTouguI4Fyyee7akQ
		iPxHC6XwwT3M0hi5GlU7wYsbvXMeNCe0NgS99+UFmUjxLVzWA4MeQjdn3tAm+IV/P2hX
		7H3ocmx/c2Aa4nrlXm5ZOOOGLYFKAiNk6HkJ2GaRcRw8MFx6ol8jPB9GA08L7z5+ZW0f
		JQng==
X-Received: by 10.194.242.6 with SMTP id wm6mr9847047wjc.7.1421986324609;
		Thu, 22 Jan 2015 20:12:04 -0800 (PST)
To: mailtester@enterprisedevelopment.cargowise.com
From: ""Edward Onwodi"" <edward.onwodi@gmail.com>
Date: Thu, 22 Jan 2015 20:12:04 -0800
Message-ID: <CADUmwtvXPYanvKeivuwc5X2w9AJUqaRkBMY3rb6zhfaAdLJZdA@mail.gmail.com>
Subject: I'm out of office Re: Notification of Incident: CS00000002 - Simple bug
MIME-Version: 1.0
Content-Type: text/html; charset=UTF-8
Content-Transfer-Encoding: 7bit
Content-Disposition: inline
Precedence: bulk
X-MS-Exchange-Generated-Message-Source: Mailbox Rules Agent
Return-Path: <edward.onwodi@gmail.com>

<div dir=""ltr""><div>I&#39;m currently out of office and would have limited access to mails. I will respond as soon as possible.</div><div><br></div><div>Thank you.</div></div>

";

		var email = new Email(Encoding.UTF8.GetBytes(autoReplyMail));

		processor.CreateAndProcessMailItem(email, LoggerForTest);

		var mailItem = GetLastCreatedMailItem();
		AssertNull(mailItem);
		Assert(email.IsEmailProcessingSkipped);
		AssertEquals("No outgoing emails should be created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
	}

	public void TestProcessCustomerServiceEmails_IncidentNumberButNoIncident()
	{
		var processor = new CustomerServiceEmailProcessorForTest();

		Email email = new EmailBuilderForTesting()
			.Subject("Hello fix this CS00001111")
			.To("Someone", "test@edi.com.au")
			.Body("please fix this incident now")
			.GetEmail();

		processor.CreateAndProcessMailItem(email, LoggerForTest);

		MailItem mailItem = GetLastCreatedMailItem();
		AssertEquals("Mail Item MI_Application have to be 'CSV'", "CSV", mailItem.MI_Application);
		AssertEquals("Mail Item status is unprocessed as no incident was found", MailStatus.Unprocessed, mailItem.MI_Status);
		AssertEquals("Mail Item direction is set to receive", MailDirection.Receive, mailItem.MI_Direction);
		AssertEquals("saved", true, mailItem.IsInDatabase);

		AssertEquals("No outgoing emails should be created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
	}

	public void TestProcessFromEmailAddedToConversations()
	{
		var incident = Factory.New<SupportIncident>();
		incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();

		Email email1 = new EmailBuilderForTesting()
			.Subject("Hello fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hello, this is a Test mail eDoc. \n Thanks! \n Regards, \n John C")
			.GetEmail();
		Email email2 = new EmailBuilderForTesting()
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is another email with subject. Chao")
			.GetEmail();

		processor.CreateAndProcessMailItem(email1, LoggerForTest);
		processor.CreateAndProcessMailItem(email2, LoggerForTest);

		var convo = incident.EConversation.Conversation;
		var messages = convo.Messages;

		Assert(convo.Messages.Any(tup => tup.
				JCM_Body.Contains(email1.LatestMessageInBody)));
		Assert(convo.Messages.Any(tup => tup.
				JCM_Body.Contains(email2.LatestMessageInBody)));
		Assert(convo.Messages.Any(tup => tup.
				JCM_Body.Contains("test has been added to the conversation.")));
		AssertEquals(3, messages.Count);
	}

	public void TestProcessFromEmailAddedParticipants()
	{
		var incident = Factory.New<SupportIncident>();
		incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();

		Email email1 = new EmailBuilderForTesting()
			.Subject("Hello fix this " + incident.Number)
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.To("Someone", "test@edi.com.au")
			.Body("Hello, this is a Test mail eDoc. \n Thanks! \n Regards, \n John C")
			.GetEmail();

		Email email2 = new EmailBuilderForTesting()
			.Subject("Hello fix this " + incident.Number)
			.From("Somebody", "nonregistered@cargowise.com")
			.To("Someone", "test@edi.com.au")
			.Body("Hello, this is a Test mail eDoc. \n Thanks! \n Regards, \n John C")
			.GetEmail();

		processor.CreateAndProcessMailItem(email1, LoggerForTest);
		processor.CreateAndProcessMailItem(email2, LoggerForTest);

		var convo = incident.EConversation.Conversation;

		Assert(convo.Participants.Any(tup => tup.
				EmailAddress.Contains("nonregistered@cargowise.com")));

		Assert(convo.Participants.Any(tup => tup.
				EmailAddress.Contains("zubin.appoo@cargowise.com")));
	}

	public void TestProcessCustomerServiceEmail_EmailAddressBlockList()
	{
		var emailAddress = "123@123.com";
		var blockedEmailAddress = "blocked@123.com";

		var docType = Factory.New<RefDocType>();
		docType.RT_ReferenceType = "BPW";
		docType.RT_DocType = "COR";
		docType.RT_Desc = "Client Correspondence";
		docType.RT_IsPublished = ZBool.True;

		var org = Factory.NewWithValidTestData<OrgHeader>();
		org.OH_FullName = "Test Org";
		org.OH_RL_NKClosestPort = "AUSYD";

		var enterprise = Factory.New<LicenceEnterprise>();
		enterprise.LE_EnterpriseCode = "DDD";
		enterprise.LE_OH = org.PK;

		var database = Factory.New<LicenceDatabase>();
		database.LD_ServerCode = "SRV";
		database.LD_LE = enterprise.PK;
		database.LD_PublicEmailAddressForUpdate = "test@test.com";

		var clientCompany = Factory.New<ClientCompany>();
		clientCompany.LCC_Code = "COM";
		clientCompany.LCC_LD = database.PK;

		var contact = org.Contacts.AddNew();
		contact.OC_Email = emailAddress;
		contact.OC_IsActive = true;
		contact.OC_ContactName = "User";

		var blockedContact = org.Contacts.AddNew();
		blockedContact.OC_Email = blockedEmailAddress;
		blockedContact.OC_IsActive = true;
		blockedContact.OC_ContactName = "blockedUser";
		Factory.Save();

		var incident = Factory.New<SupportIncident>();
		incident.IM_OA_BranchAddress = org.MainAddress.PK;
		incident.IM_OH_Client = org.PK;
		incident.IM_OC_Contact = contact.PK;
		incident.IM_Description = "Help me";
		incident.DetailNoteText = "Don't know what i'm doing";
		incident.IM_LD = database.PK;
		incident.IM_LCC = clientCompany.PK;
		incident.IM_ClientIncidentReference = "CL111111";
		incident.IM_Category = "SUP";
		incident.OnLoaded();

		Factory.Save();

		EDIDataRegistry.Instance.EmailAddressBlockList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { blockedEmailAddress });

		var processor = new CustomerServiceEmailProcessorForTest();

		var email = new EmailBuilderForTesting()
			.Subject("[A]Hello fix this " + incident.Number)
			.From("User", "123@123.COM")
			.WithAttachment("textA.txt", "Hi, this is an attachment. Cheers");

		var blockedEmail = new EmailBuilderForTesting()
			.Subject("[B]Hello fix this " + incident.Number)
			.From("blockedUser", "BLocked@123.COM")
			.WithAttachment("textB.txt", "Hi, this is an attachment. Cheers");

		var logger = LoggerForTest as EmailProcessorLoggerForTest;

		logger.LogEntries.Clear();
		Assert(processor.CreateAndProcessMailItem(email.GetEmail(), LoggerForTest));
		AssertEquals(1, logger.LogEntries.Count);
		AssertStartsWith("There should be no extra logs", "Email attached to: ", logger.LogEntries[0]);

		logger.LogEntries.Clear();
		Assert(processor.CreateAndProcessMailItem(blockedEmail.GetEmail(), LoggerForTest));
		AssertEquals(2, logger.LogEntries.Count);
		AssertStartsWith("There should have a attach logs", "Email attached to: ", logger.LogEntries[0]);
		AssertEquals($"`{blockedEmail.GetEmail().Subject}` was only add into the eDoc of {incident.IM_IncidentNumber}, because the email from {blockedEmail.GetEmail().SenderAddress}.", logger.LogEntries[1]);

		incident.Reload();
		Assert(incident.EConversation.Conversation.Messages.Any(x => x.Body.Contains("[A]Hello fix this")));
		Assert("Blocked email should not be added to message", !incident.EConversation.Conversation.Messages.Any(x => x.Body.Contains("[B]Hello fix this")));

		Assert(incident.EConversation.Conversation.Participants.Any(x => x.JCP_ParticipantID == contact.PK));
		Assert(!incident.EConversation.Conversation.Participants.Any(x => string.Equals(x.Parent.Email, blockedEmailAddress, StringComparison.OrdinalIgnoreCase)));

		AssertEquals(3, incident.DocManagerInfo.AllEDocs.Cast<IeDoc>().Count());
		var files = incident.DocManagerInfo.AllEDocs.Cast<IeDoc>();
		Assert(files.First(x => x.FileName.Equals(email.GetEmail().Subject + ".eml")).IsPublished);
		Assert(files.Any(x => x.FileName.Equals("textA.txt")));

		Assert("Blocked email should not be published, because GetChangedPublishedEDocsAndLogToEConversation will add published email body to eConversation", !files.First(x => x.FileName.Equals(blockedEmail.GetEmail().Subject + ".eml")).IsPublished);
		AssertNull("Blocked email's attachment should not be added", files.FirstOrDefault(x => x.FileName.Equals("textB.txt")));
	}

	public void TestProcessFromEmailShouldContactParticipantsFromToOrCc()
	{
		AssemblyDataLookup.ClearDataForTesting();

		var docType = Factory.New<RefDocType>();
		docType.RT_ReferenceType = "BPW";
		docType.RT_DocType = "COR";
		docType.RT_Desc = "Client Correspondence";
		docType.RT_IsPublished = ZBool.True;

		var org = Factory.NewWithValidTestData<OrgHeader>();
		org.OH_FullName = "Test Org";
		org.OH_RL_NKClosestPort = "AUSYD";

		var enterprise = Factory.New<LicenceEnterprise>();
		enterprise.LE_EnterpriseCode = "DDD";
		enterprise.LE_OH = org.PK;

		var database = Factory.New<LicenceDatabase>();
		database.LD_ServerCode = "SRV";
		database.LD_LE = enterprise.PK;
		database.LD_PublicEmailAddressForUpdate = "test@test.com";

		var clientCompany = Factory.New<ClientCompany>();
		clientCompany.LCC_Code = "COM";
		clientCompany.LCC_LD = database.PK;

		Factory.Save();

		var contact = org.Contacts.AddNew();
		contact.OC_ContactName = "archie";
		contact.OC_Email = "archie@test.com.au";
		contact.OC_IsActive = true;

		var contact2 = org.Contacts.AddNew();
		contact2.OC_ContactName = "archie test to";
		contact2.OC_Email = "archietest_to@test.com.au";
		contact2.OC_IsActive = true;

		var contact3 = org.Contacts.AddNew();
		contact3.OC_ContactName = "archie test cc";
		contact3.OC_Email = "archietest_cc@test.com.au";
		contact3.OC_IsActive = true;

		var incident = Factory.New<SupportIncident>();
		incident.IM_OA_BranchAddress = org.MainAddress.PK;
		incident.IM_OH_Client = org.PK;
		incident.IM_OC_Contact = contact.PK;
		incident.IM_Description = "Help me";
		incident.DetailNoteText = "Don't know what i'm doing";
		incident.IM_LD = database.PK;
		incident.IM_LCC = clientCompany.PK;
		incident.IM_ClientIncidentReference = "CL111111";
		incident.IM_Category = "SUP";
		incident.OnLoaded(); // Initialise incident.OriginalStage to prevent sending extra email from SendAutomatedEmailToClientSystem

		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();

		var email1 = new EmailBuilderForTesting()
			.Subject("Hello fix this " + incident.Number)
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.To("support", "support@wisetechglobal.com")
			.To("archie test to", "archietest_to@test.com.au")
			.Body("Hello, this is a Test mail eDoc. \n Thanks! \n Regards, \n John C")
			.GetEmail();

		var email2 = new EmailBuilderForTesting()
			.Subject("Hello fix this " + incident.Number)
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.To("support", "support@wisetechglobal.com")
			.Cc("archie test cc", "archietest_cc@test.com.au")
			.Body("Hello, this is a Test mail eDoc. \n Thanks! \n Regards, \n John C")
			.GetEmail();

		processor.CreateAndProcessMailItem(email1, LoggerForTest);

		var convo1 = incident.EConversation.Conversation;
		var archieToEmailParticipant = convo1.Participants.Where(p => p.EmailAddress.Contains("archietest_to@test.com.au"));
		var archieToIdParticipant = convo1.Participants.Where(p => p.JCP_ParticipantID == contact2.PK);

		Assert(convo1.Participants.Any(p => p.EmailAddress.Contains("zubin.appoo@cargowise.com")));

		AssertEquals(1, archieToEmailParticipant.Count());
		AssertEquals(1, archieToIdParticipant.Count());

		processor.CreateAndProcessMailItem(email2, LoggerForTest);

		var convo2 = incident.EConversation.Conversation;
		var archieCcEmailParticipant = convo2.Participants.Where(p => p.EmailAddress.Contains("archietest_cc@test.com.au"));
		var archieCcIdParticipant = convo2.Participants.Where(p => p.JCP_ParticipantID == contact3.PK);

		Assert(convo2.Participants.Any(p => p.EmailAddress.Contains("zubin.appoo@cargowise.com")));

		AssertEquals(1, archieCcEmailParticipant.Count());
		AssertEquals(1, archieCcIdParticipant.Count());
	}

	public void TestProcessEmailAdderssShouldIgnoreCase()
	{
		AssemblyDataLookup.ClearDataForTesting();

		var docType = Factory.New<RefDocType>();
		docType.RT_ReferenceType = "BPW";
		docType.RT_DocType = "COR";
		docType.RT_Desc = "Client Correspondence";
		docType.RT_IsPublished = ZBool.True;

		var org = Factory.NewWithValidTestData<OrgHeader>();
		org.OH_FullName = "Test Org";
		org.OH_RL_NKClosestPort = "AUSYD";

		var enterprise = Factory.New<LicenceEnterprise>();
		enterprise.LE_EnterpriseCode = "DDD";
		enterprise.LE_OH = org.PK;

		var database = Factory.New<LicenceDatabase>();
		database.LD_ServerCode = "SRV";
		database.LD_LE = enterprise.PK;
		database.LD_PublicEmailAddressForUpdate = "test@test.com";

		var clientCompany = Factory.New<ClientCompany>();
		clientCompany.LCC_Code = "COM";
		clientCompany.LCC_LD = database.PK;

		Factory.Save();

		var contact = org.Contacts.AddNew();
		contact.OC_ContactName = "archie";
		contact.OC_Email = "archie@test.com.au";
		contact.OC_IsActive = true;

		var contact2 = org.Contacts.AddNew();
		contact2.OC_ContactName = "archie test to";
		contact2.OC_Email = "archietest_to@test.com.au";
		contact2.OC_IsActive = true;

		var incident = Factory.New<SupportIncident>();
		incident.IM_OA_BranchAddress = org.MainAddress.PK;
		incident.IM_OH_Client = org.PK;
		incident.IM_OC_Contact = contact.PK;
		incident.IM_Description = "Help me";
		incident.DetailNoteText = "Don't know what i'm doing";
		incident.IM_LD = database.PK;
		incident.IM_LCC = clientCompany.PK;
		incident.IM_ClientIncidentReference = "CL111111";
		incident.IM_Category = "SUP";
		incident.OnLoaded(); // Initialise incident.OriginalStage to prevent sending extra email from SendAutomatedEmailToClientSystem

		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();

		var email1 = new EmailBuilderForTesting()
			.Subject("Hello fix this " + incident.Number)
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.To("support", "support@wisetechglobal.com")
			.To("archie test to", "archietest_to@test.com.au")
			.Body("Hello, this is a Test mail eDoc. \n Thanks! \n Regards, \n John C")
			.GetEmail();

		var email2 = new EmailBuilderForTesting()
			.Subject("Hello fix this " + incident.Number)
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.To("support", "support@wisetechglobal.com")
			.To("archie test TO", "archietest_TO@test.com.au")
			.Body("Hello, this is a Test mail eDoc. \n Thanks! \n Regards, \n John C")
			.GetEmail();

		processor.CreateAndProcessMailItem(email1, LoggerForTest);

		var convo1 = incident.EConversation.Conversation;
		var archieToEmailParticipant = convo1.Participants.Where(p => p.EmailAddress.Contains("archietest_to@test.com.au"));
		var archieToIdParticipant = convo1.Participants.Where(p => p.JCP_ParticipantID == contact2.PK);

		Assert(convo1.Participants.Any(p => p.EmailAddress.Contains("zubin.appoo@cargowise.com")));

		AssertEquals(1, archieToEmailParticipant.Count());
		AssertEquals(1, archieToIdParticipant.Count());

		processor.CreateAndProcessMailItem(email2, LoggerForTest);

		var convo2 = incident.EConversation.Conversation;
		var archieTOEmailParticipant = convo2.Participants.Where(p => p.EmailAddress.Contains("archietest_TO@test.com.au"));

		Assert(convo2.Participants.Any(p => p.EmailAddress.Contains("zubin.appoo@cargowise.com")));

		AssertEquals(0, archieTOEmailParticipant.Count());
	}

	public void TestProcessEmailAdderssShouldIgnoreInvalidEmail()
	{
		AssemblyDataLookup.ClearDataForTesting();

		var docType = Factory.New<RefDocType>();
		docType.RT_ReferenceType = "BPW";
		docType.RT_DocType = "COR";
		docType.RT_Desc = "Client Correspondence";
		docType.RT_IsPublished = ZBool.True;

		var org = Factory.NewWithValidTestData<OrgHeader>();
		org.OH_FullName = "Test Org";
		org.OH_RL_NKClosestPort = "AUSYD";

		var enterprise = Factory.New<LicenceEnterprise>();
		enterprise.LE_EnterpriseCode = "DDD";
		enterprise.LE_OH = org.PK;

		var database = Factory.New<LicenceDatabase>();
		database.LD_ServerCode = "SRV";
		database.LD_LE = enterprise.PK;
		database.LD_PublicEmailAddressForUpdate = "test@test.com";

		var clientCompany = Factory.New<ClientCompany>();
		clientCompany.LCC_Code = "COM";
		clientCompany.LCC_LD = database.PK;

		Factory.Save();

		var contact = org.Contacts.AddNew();
		contact.OC_ContactName = "archie";
		contact.OC_Email = "archie@test.com.au";
		contact.OC_IsActive = true;

		var incident = Factory.New<SupportIncident>();
		incident.IM_OA_BranchAddress = org.MainAddress.PK;
		incident.IM_OH_Client = org.PK;
		incident.IM_OC_Contact = contact.PK;
		incident.IM_Description = "Help me";
		incident.DetailNoteText = "Don't know what i'm doing";
		incident.IM_LD = database.PK;
		incident.IM_LCC = clientCompany.PK;
		incident.IM_ClientIncidentReference = "CL111111";
		incident.IM_Category = "SUP";
		incident.OnLoaded(); // Initialise incident.OriginalStage to prevent sending extra email from SendAutomatedEmailToClientSystem

		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();

		var email1 = new EmailBuilderForTesting()
			.Subject("Hello fix this " + incident.Number)
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.To("support", "support@wisetechglobal.com")
			.To("archie test to", "archietest_to@test.com.au")
			.To("Invalid Email to", @"'archie_invalid@test.com'")
			.Body("Hello, this is a Test mail eDoc. \n Thanks! \n Regards, \n John C")
			.GetEmail();

		processor.CreateAndProcessMailItem(email1, LoggerForTest);

		var convo1 = incident.EConversation.Conversation;
		var archieToEmailParticipant = convo1.Participants.Where(p => p.EmailAddress.Contains("archietest_to@test.com.au"));
		var archieInvalidToEmailParticipant = convo1.Participants.Where(p => p.EmailAddress.Contains(@"'archie_invalid@test.com'"));

		Assert(convo1.Participants.Any(p => p.EmailAddress.Contains("zubin.appoo@cargowise.com")));

		AssertEquals(1, archieToEmailParticipant.Count());
		AssertEquals(0, archieInvalidToEmailParticipant.Count());
	}

	public void TestProcessFromEmailAddedEmailParticipantsFromToOrCc()
	{
		AssemblyDataLookup.ClearDataForTesting();

		RefDocType docType = Factory.New<RefDocType>();
		docType.RT_ReferenceType = "BPW";
		docType.RT_DocType = "COR";
		docType.RT_Desc = "Client Correspondence";
		docType.RT_IsPublished = ZBool.True;

		OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
		org.OH_FullName = "Test Org";
		org.OH_RL_NKClosestPort = "AUSYD";

		LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
		enterprise.LE_EnterpriseCode = "DDD";
		enterprise.LE_OH = org.PK;

		LicenceDatabase database = Factory.New<LicenceDatabase>();
		database.LD_ServerCode = "SRV";
		database.LD_LE = enterprise.PK;
		database.LD_PublicEmailAddressForUpdate = "test@test.com";

		ClientCompany clientCompany = Factory.New<ClientCompany>();
		clientCompany.LCC_Code = "COM";
		clientCompany.LCC_LD = database.PK;

		Factory.Save();

		OrgContact contact = org.Contacts.AddNew();
		contact.OC_ContactName = "archie";
		contact.OC_Email = "archie@test.com.au";
		contact.OC_IsActive = true;

		SupportIncident incident = Factory.New<SupportIncident>();
		incident.IM_OA_BranchAddress = org.MainAddress.PK;
		incident.IM_OH_Client = org.PK;
		incident.IM_OC_Contact = contact.PK;
		incident.IM_Description = "Help me";
		incident.DetailNoteText = "Don't know what i'm doing";
		incident.IM_LD = database.PK;
		incident.IM_LCC = clientCompany.PK;
		incident.IM_ClientIncidentReference = "CL111111";
		incident.IM_Category = "SUP";
		incident.OnLoaded(); // Initialise incident.OriginalStage to prevent sending extra email from SendAutomatedEmailToClientSystem

		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();

		Email email1 = new EmailBuilderForTesting()
			.Subject("Hello fix this " + incident.Number)
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.To("support", "support@wisetechglobal.com")
			.To("archie test to", "archietest_to@test.com.au")
			.Body("Hello, this is a Test mail eDoc. \n Thanks! \n Regards, \n John C")
			.GetEmail();

		Email email2 = new EmailBuilderForTesting()
			.Subject("Hello fix this " + incident.Number)
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.To("support", "support@wisetechglobal.com")
			.Cc("archie test cc", "archietest_cc@test.com.au")
			.Body("Hello, this is a Test mail eDoc. \n Thanks! \n Regards, \n John C")
			.GetEmail();

		processor.CreateAndProcessMailItem(email1, LoggerForTest);

		var convo1 = incident.EConversation.Conversation;
		var archieToEmailParticipant = convo1.Participants.Where(p => p.EmailAddress.Contains("archietest_to@test.com.au"));

		Assert(convo1.Participants.Any(p => p.EmailAddress.Contains("zubin.appoo@cargowise.com")));

		AssertEquals(1, archieToEmailParticipant.Count());

		processor.CreateAndProcessMailItem(email2, LoggerForTest);

		var convo2 = incident.EConversation.Conversation;
		var archieCcEmailParticipant = convo2.Participants.Where(p => p.EmailAddress.Contains("archietest_cc@test.com.au"));

		AssertEquals(1, archieCcEmailParticipant.Count());
	}

	public void TestProcessFromEmailAddedDuplicateParticipants()
	{
		AssemblyDataLookup.ClearDataForTesting();

		RefDocType docType = Factory.New<RefDocType>();
		docType.RT_ReferenceType = "BPW";
		docType.RT_DocType = "COR";
		docType.RT_Desc = "Client Correspondence";
		docType.RT_IsPublished = ZBool.True;

		OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
		org.OH_FullName = "Test Org";
		org.OH_RL_NKClosestPort = "AUSYD";

		LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
		enterprise.LE_EnterpriseCode = "DDD";
		enterprise.LE_OH = org.PK;

		LicenceDatabase database = Factory.New<LicenceDatabase>();
		database.LD_ServerCode = "SRV";
		database.LD_LE = enterprise.PK;
		database.LD_PublicEmailAddressForUpdate = "test@test.com";

		ClientCompany clientCompany = Factory.New<ClientCompany>();
		clientCompany.LCC_Code = "COM";
		clientCompany.LCC_LD = database.PK;

		Factory.Save();

		OrgContact contact = org.Contacts.AddNew();
		contact.OC_ContactName = "Samuel";
		contact.OC_Email = "sam@test.com.au";
		contact.OC_IsActive = true;

		OrgContact contact2 = org.Contacts.AddNew();
		contact2.OC_ContactName = "Samuel Inactive";
		contact2.OC_Email = "sam@test.com.au";
		contact2.OC_IsActive = false;

		SupportIncident incident = Factory.New<SupportIncident>();
		incident.IM_OA_BranchAddress = org.MainAddress.PK;
		incident.IM_OH_Client = org.PK;
		incident.IM_OC_Contact = contact.PK;
		incident.IM_Description = "Help me";
		incident.DetailNoteText = "Don't know what i'm doing";
		incident.IM_LD = database.PK;
		incident.IM_LCC = clientCompany.PK;
		incident.IM_ClientIncidentReference = "CL111111";
		incident.IM_Category = "SUP";
		incident.OnLoaded(); // Initialise incident.OriginalStage to prevent sending extra email from SendAutomatedEmailToClientSystem

		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();

		Email email1 = new EmailBuilderForTesting()
			.Subject("Hello fix this " + incident.Number)
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.To("Someone", "test@edi.com.au")
			.Body("Hello, this is a Test mail eDoc. \n Thanks! \n Regards, \n John C")
			.GetEmail();
		Email email2 = new EmailBuilderForTesting()
			.Subject("Hello fix this " + incident.Number)
			.From("Somebody", "nonregistered@cargowise.com")
			.To("Someone", "test@edi.com.au")
			.Body("Hello, this is a Test mail eDoc. \n Thanks! \n Regards, \n John C")
			.GetEmail();
		Email email3 = new EmailBuilderForTesting()
			.Subject("Hello testign a duplicate user " + incident.Number)
			.From("Samuel", "sam@test.com.au")
			.To("Someone", "test@edi.com.au")
			.Body("Hello, this is a Test mail eDoc. \n Thanks! \n Regards, \n John C")
			.GetEmail();
		Email email4 = new EmailBuilderForTesting()
			.Subject("Hello testign a duplicate user " + incident.Number)
			.From("Samuel", "sam@test.com.au")
			.To("Someone", "test@edi.com.au")
			.Body("Hello, this is a Test mail eDoc. \n Thanks! \n Regards, \n John C")
			.GetEmail();

		processor.CreateAndProcessMailItem(email1, LoggerForTest);
		processor.CreateAndProcessMailItem(email2, LoggerForTest);
		processor.CreateAndProcessMailItem(email3, LoggerForTest);
		processor.CreateAndProcessMailItem(email4, LoggerForTest);

		var convo = incident.EConversation.Conversation;
		var samEmailParticipant = convo.Participants.Where(p => p.EmailAddress.Contains("sam@test.com.au"));
		var samIdParticipant = convo.Participants.Where(p => p.JCP_ParticipantID == contact.PK);

		Assert(convo.Participants.Any(p => p.EmailAddress.Contains("nonregistered@cargowise.com")));
		Assert(convo.Participants.Any(p => p.EmailAddress.Contains("zubin.appoo@cargowise.com")));

		AssertEquals(1, samEmailParticipant.Count());
		AssertEquals(1, samIdParticipant.Count());
	}

	public void TestProcessFromForwardedEmailsToConversations()
	{
		var incident = Factory.New<SupportIncident>();
		incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
		Factory.Save();
		var processor = new CustomerServiceEmailProcessorForTest();

		Email email1 = new EmailBuilderForTesting()
			.Subject("Hello fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hello, this is a Test mail eDoc. \n Thanks! \n Regards, \n John C")
			.From("123@email.com")
			.GetEmail();
		Email email2 = new EmailBuilderForTesting()
			.Subject("fwd: fix this " + incident.Number)
			.WithForward(email1.Message.MessageId)
			.To("Someone", "test@edi.com.au")
			.Body("Hi... I am just clarifying stuff. Chao.\r\n------Forwarded message-------\r\n>Hello, this is a Test mail eDoc. \n> Thanks! \n> Regards, \n> John C")
			.From("123@email.com")
			.GetEmail();

		processor.CreateAndProcessMailItem(email1, LoggerForTest);
		processor.CreateAndProcessMailItem(email2, LoggerForTest);

		var convo = incident.EConversation.Conversation;
		var messages = convo.Messages;

		Assert(convo.Messages.Any(tup => tup.JCM_Body.Contains(email1.LatestMessageInBody)));
		Assert(convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi... I am just clarifying stuff. Chao.")));
		Assert(!convo.Messages.Any(tup => tup.JCM_Body.Contains("wrote:")));
		Assert(convo.Messages.Any(tup => tup.JCM_Body.Contains("test has been added to the conversation.")));
		Assert(convo.Messages.Any(tup => tup.JCM_Body.Contains("123 has been added to the conversation.")));
		AssertEquals(4, messages.Count);
	}

	public void TestProcessFromEmailAddedToConversationsManageAttachments()
	{
		var incident = Factory.New<SupportIncident>();
		incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
		Factory.Save();
		var processor = new CustomerServiceEmailProcessorForTest();

		Email email1 = new EmailBuilderForTesting()
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email with subject. Chao")
			.WithAttachment("text.txt", "Hi, this is an attachment. Cheers")
			.GetEmail();

		processor.CreateAndProcessMailItem(email1, LoggerForTest);

		var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
		var edoc = loadedIncident.DocManagerInfo.AllEDocs.Cast<IeDoc>().First(x => x.FileName.EqualsIgnoringCase("text.txt"));

		var convo = incident.EConversation.Conversation;
		Assert("Email body should be in conversation", convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi, this is an email with subject. Chao\r\nAttachment 1: [text.txt]")));
		Assert("Email attachment should be in conversation", convo.Messages.Any(tup => tup.JCM_Body.Contains($"[text.txt]({ShowStorageDocUrlHandler.Instance.Create(edoc)})")));
	}

	public void TestProcessFromEmailAddedToConversationsManageInlineImage()
	{
		var incident = Factory.New<SupportIncident>();
		incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;
		Factory.Save();
		var processor = new CustomerServiceEmailProcessorForTest();

		var builder = new EmailBuilderForTesting();
		var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		var imgBytes = resourceRetriever.GetBytes(@"ZClientEDI.Test.Mail.TestFiles.image1000.jpg");
		Email email1 = builder
			.Subject("fix this " + incident.Number)
			.To("Someone", "test@edi.com.au")
			.Body("Hi, this is an email with subject. [howdy] Chao")
			.HtmlBody("Hi, this is an email with subject. Chao <a href=\"cid:abc123\" alt=\"howdy\" />")
			.WithVisual("abc123.png", imgBytes, "abc123")
			.GetEmail();

		processor.CreateAndProcessMailItem(email1, LoggerForTest);

		var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
		var edoc = loadedIncident.DocManagerInfo.AllEDocs.Cast<IeDoc>().First(x => x.FileName.EqualsIgnoringCase("abc123.png"));

		var convo = incident.EConversation.Conversation;
		Assert("Email body should be in conversation", convo.Messages.Any(tup => tup.JCM_Body.Contains("Hi, this is an email with subject. Chao [abc123.png]")));
		Assert("Email attachment should be in conversation", convo.Messages.Any(tup => tup.JCM_Body.Contains($"[abc123.png]({ShowStorageDocUrlHandler.Instance.Create(edoc)})")));
	}

	public void TestProcessContactReplyEmails_IncidentFound()
	{
		GlbStaff someCustServiceStaff = Factory.New<GlbStaff>();
		someCustServiceStaff.GS_Code = "XCS";
		someCustServiceStaff.GS_EmailAddress = "somewhere@nowhere.com";

		var contact = Factory.NewWithValidTestData<OrgContact>();
		contact.OC_Email = "zubin.appoo@cargowise.com";
		contact.OC_ContactName = "Zubin Appoo";
		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();

		SupportIncident incident = Factory.New<SupportIncident>();
		incident.IM_GS_NKCustServiceContact = someCustServiceStaff.GS_Code;
		SetUpIncidentForStage(SupportIncidentCategoriesList.Codes.Support, incident);
		Factory.Save();

		incident.EConversation.Conversation.Participants.AddNewParticipant(contact);
		Factory.Save();

		Email email = new EmailBuilderForTesting()
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.Subject("Re: Update\\..// on In|cident: " + incident.IM_IncidentNumber + " - Question regarding \"air\" and <sea> invoice* statemen?ts")
			.To("Someone", "test@edi.com.au")
			.Body("please fix this incident now")
			.GetEmail();

		int initialCount = Factory.GetDatabaseCount(typeof(MailItem));

		processor.CreateAndProcessMailItem(email, LoggerForTest);

		MailItem mailItem = GetLastCreatedMailItem();
		AssertEquals("Mail Item MI_Application have to be 'CSV'", "CSV", mailItem.MI_Application);
		AssertEquals("Mail Item status is processed as incident was found", MailStatus.Processed, mailItem.MI_Status);
		AssertEquals("Mail Item direction is set to receive", MailDirection.Receive, mailItem.MI_Direction);
		AssertEquals("saved", true, mailItem.IsInDatabase);
		AssertEquals("saved - no changes", false, mailItem.HasChanges);

		BusinessObjectFactory newFactory = new BusinessObjectFactory();
		MailItem reloadedItem = newFactory.Load<MailItem>(mailItem.PK);
		AssertEquals("Mail Item status is processed as incident was found", MailStatus.Processed, mailItem.MI_Status);
		AssertEquals("Processed is persisted", MailStatus.Processed, reloadedItem.MI_Status);

		incident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
		DocManagerInfo docSupportInfo = ((IDocManagerSupport)incident).DocManagerInfo;
		AssertNotEquals("New email should be attached to the incident", 0, docSupportInfo.Files.Count);
		AssertEquals("Type of the document attached to the incident should be 'COR'", "COR", docSupportInfo.Files[0].DocType.ToUpper());
		AssertEquals("File name stripped of invalid chars", "Re Update.. on Incident " + incident.IM_IncidentNumber + " - Question regarding air and sea invoice statements", ((BusinessObject)docSupportInfo.Files[0])["SC_FileName"].ToString());
		AssertEquals("Text of the document attached to the incident should be list of recipients, subject and body",
			mailItem.MI_Header + System.Environment.NewLine + System.Environment.NewLine + mailItem.MI_Body,
			docSupportInfo.Files[0].GetImageDataReader().ConvertToAsciiStringAndCloseStream());

		AssertEquals("Have 1 emails", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		var copyEmail = Env.OutgoingMailManager.EmailsCreated[0];

		AssertEquals("File name stripped of invalid chars", "Re Update.. on Incident " + incident.IM_IncidentNumber + " - Question regarding air and sea invoice statements", ((BusinessObject)docSupportInfo.Files[0])["SC_FileName"].ToString());

		AssertEquals("Customer Service Incident: CS00000001 has been assigned to you", copyEmail.Subject);
	}

	public void TestProcessEmailParticipantReplyEmails_IncidentFound()
	{
		GlbStaff someCustServiceStaff = Factory.New<GlbStaff>();
		someCustServiceStaff.GS_Code = "XCS";
		someCustServiceStaff.GS_EmailAddress = "somewhere@nowhere.com";
		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();

		SupportIncident incident = Factory.New<SupportIncident>();
		incident.IM_GS_NKCustServiceContact = someCustServiceStaff.GS_Code;
		SetUpIncidentForStage(SupportIncidentCategoriesList.Codes.Support, incident);
		Factory.Save();

		Email email = new EmailBuilderForTesting()
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.Subject("Re: Update\\..// on In|cident: " + incident.IM_IncidentNumber + " - Question regarding \"air\" and <sea> invoice* statemen?ts")
			.To("Someone", "test@edi.com.au")
			.Body("please fix this incident now")
			.GetEmail();

		int initialCount = Factory.GetDatabaseCount(typeof(MailItem));

		processor.CreateAndProcessMailItem(email, LoggerForTest);

		MailItem mailItem = GetLastCreatedMailItem();
		AssertEquals("Mail Item MI_Application have to be 'CSV'", "CSV", mailItem.MI_Application);
		AssertEquals("Mail Item status is processed as incident was found", MailStatus.Processed, mailItem.MI_Status);
		AssertEquals("Mail Item direction is set to receive", MailDirection.Receive, mailItem.MI_Direction);
		AssertEquals("saved", true, mailItem.IsInDatabase);
		AssertEquals("saved - no changes", false, mailItem.HasChanges);

		BusinessObjectFactory newFactory = new BusinessObjectFactory();
		MailItem reloadedItem = newFactory.Load<MailItem>(mailItem.PK);
		AssertEquals("Mail Item status is processed as incident was found", MailStatus.Processed, mailItem.MI_Status);
		AssertEquals("Processed is persisted", MailStatus.Processed, reloadedItem.MI_Status);

		incident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
		DocManagerInfo docSupportInfo = ((IDocManagerSupport)incident).DocManagerInfo;
		AssertNotEquals("New email should be attached to the incident", 0, docSupportInfo.Files.Count);
		AssertEquals("Type of the document attached to the incident should be 'COR'", "COR", docSupportInfo.Files[0].DocType.ToUpper());
		AssertEquals("File name stripped of invalid chars", "Re Update.. on Incident " + incident.IM_IncidentNumber + " - Question regarding air and sea invoice statements", ((BusinessObject)docSupportInfo.Files[0])["SC_FileName"].ToString());
		AssertEquals("Text of the document attached to the incident should be list of recipients, subject and body",
			mailItem.MI_Header + System.Environment.NewLine + System.Environment.NewLine + mailItem.MI_Body,
			docSupportInfo.Files[0].GetImageDataReader().ConvertToAsciiStringAndCloseStream());

		AssertEquals("Have 2 emails", 2, Env.OutgoingMailManager.EmailsCreated.Count);
		var copyEmail = Env.OutgoingMailManager.EmailsCreated[0];

		AssertEquals("File name stripped of invalid chars", "Re Update.. on Incident " + incident.IM_IncidentNumber + " - Question regarding air and sea invoice statements", ((BusinessObject)docSupportInfo.Files[0])["SC_FileName"].ToString());

		AssertEquals("Customer Service Incident: CS00000001 has been assigned to you", copyEmail.Subject);

		var subscriberUpdateEConversationEmail = Env.OutgoingMailManager.EmailsCreated[1];
		AssertContains("New Messages in", subscriberUpdateEConversationEmail.Subject);
		AssertEquals("subscriberUpdateEConversationEmail Recipients count", 1, subscriberUpdateEConversationEmail.Recipients.Count);
		AssertEquals("subscriberUpdateEConversationEmail Recipient", "test@edi.com.au", subscriberUpdateEConversationEmail.Recipients[0].Email);
	}

	public void TestShouldTriggerSubscriberUpdateEmail_WhenProcessEmailParticipantReplyEmails()
	{
		var incident = Factory.New<SupportIncident>();

		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "XCS";
		staff.GS_EmailAddress = "somewhere@nowhere.com";

		var incidentContact = Factory.NewWithValidTestData<OrgContact>();
		incidentContact.OC_Email = "incident_contact@test.com";
		incident.IM_OC_Contact = incidentContact.PK;
		var contact = Factory.NewWithValidTestData<OrgContact>();
		contact.OC_Email = "contact@test.com";

		var org = Factory.NewWithValidTestData<OrgHeader>();
		org.OH_FullName = "Future Industries";
		org.OH_RL_NKClosestPort = "AUSYD";
		var orgAddress = org.Addresses.MainAddress;
		orgAddress.OA_Email = "org@test.com";
		orgAddress.OA_Address1 = "address 1";

		Factory.Save();

		incident.EConversation.Conversation.Participants.AddNewParticipant(org);
		incident.EConversation.Conversation.Participants.AddNewParticipant(staff);
		incident.EConversation.Conversation.Participants.AddNewParticipant(contact);

		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();

		incident.IM_GS_NKCustServiceContact = staff.GS_Code;
		SetUpIncidentForStage(SupportIncidentCategoriesList.Codes.Support, incident);
		Factory.Save();

		var email = new EmailBuilderForTesting()
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.Subject("Re: Update\\..// on In|cident: " + incident.IM_IncidentNumber + " - Question regarding \"air\" and <sea> invoice* statemen?ts")
			.To("Someone", "test@edi.com.au")
			.Body("please fix this incident now")
			.GetEmail();

		int initialCount = Factory.GetDatabaseCount(typeof(MailItem));

		processor.CreateAndProcessMailItem(email, LoggerForTest);

		var mailItem = GetLastCreatedMailItem();
		AssertEquals("Mail Item MI_Application have to be 'CSV'", "CSV", mailItem.MI_Application);
		AssertEquals("Mail Item status is processed as incident was found", MailStatus.Processed, mailItem.MI_Status);
		AssertEquals("Mail Item direction is set to receive", MailDirection.Receive, mailItem.MI_Direction);
		AssertEquals("saved", true, mailItem.IsInDatabase);
		AssertEquals("saved - no changes", false, mailItem.HasChanges);

		var newFactory = new BusinessObjectFactory();
		var reloadedItem = newFactory.Load<MailItem>(mailItem.PK);
		AssertEquals("Mail Item status is processed as incident was found", MailStatus.Processed, mailItem.MI_Status);
		AssertEquals("Processed is persisted", MailStatus.Processed, reloadedItem.MI_Status);

		incident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
		var docSupportInfo = ((IDocManagerSupport)incident).DocManagerInfo;
		AssertNotEquals("New email should be attached to the incident", 0, docSupportInfo.Files.Count);
		AssertEquals("Type of the document attached to the incident should be 'COR'", "COR", docSupportInfo.Files[0].DocType.ToUpper());
		AssertEquals("File name stripped of invalid chars", "Re Update.. on Incident " + incident.IM_IncidentNumber + " - Question regarding air and sea invoice statements", ((BusinessObject)docSupportInfo.Files[0])["SC_FileName"].ToString());
		AssertEquals("Text of the document attached to the incident should be list of recipients, subject and body",
			mailItem.MI_Header + System.Environment.NewLine + System.Environment.NewLine + mailItem.MI_Body,
			docSupportInfo.Files[0].GetImageDataReader().ConvertToAsciiStringAndCloseStream());

		AssertEquals("Have 4 emails", 4, Env.OutgoingMailManager.EmailsCreated.Count);
		var copyEmail = Env.OutgoingMailManager.EmailsCreated[0];

		AssertEquals("File name stripped of invalid chars", "Re Update.. on Incident " + incident.IM_IncidentNumber + " - Question regarding air and sea invoice statements", ((BusinessObject)docSupportInfo.Files[0])["SC_FileName"].ToString());
		AssertEquals("Customer Service Incident: CS00000001 has been assigned to you", copyEmail.Subject);

		var email1 = Env.OutgoingMailManager.EmailsCreated[1];
		AssertContains("New Messages in", email1.Subject);
		AssertEquals("Should only contain staff recipients", 1, email1.Recipients.Count);
		AssertEquals("Should only contain staff recipients", "somewhere@nowhere.com", email1.Recipients[0].Email);

		var email2 = Env.OutgoingMailManager.EmailsCreated[2];
		var email3 = Env.OutgoingMailManager.EmailsCreated[3];
		AssertContains("New Messages in", email2.Subject);
		AssertContains("New Messages in", email3.Subject);

		if (email2.Recipients.Count == 1)
		{
			AssertEquals("Should only contain contact recipients", true, email3.Recipients.Cast<RecipientDef>().Any(x => x.Email == "contact@test.com"));
			AssertEquals("Should only contain contact recipients", true, email3.Recipients.Cast<RecipientDef>().Any(x => x.Email == "org@test.com"));
			AssertEquals("Should only contain contact recipients", true, email3.Recipients.Cast<RecipientDef>().Any(x => x.Email == "incident_contact@test.com"));
			AssertEquals("Should only contain email recipients", true, email2.Recipients.Cast<RecipientDef>().Any(x => x.Email == "test@edi.com.au"));
		}
		else
		{
			AssertEquals("Should only contain contact recipients", true, email2.Recipients.Cast<RecipientDef>().Any(x => x.Email == "contact@test.com"));
			AssertEquals("Should only contain contact recipients", true, email2.Recipients.Cast<RecipientDef>().Any(x => x.Email == "org@test.com"));
			AssertEquals("Should only contain contact recipients", true, email2.Recipients.Cast<RecipientDef>().Any(x => x.Email == "incident_contact@test.com"));
			AssertEquals("Should only contain email recipients", true, email3.Recipients.Cast<RecipientDef>().Any(x => x.Email == "test@edi.com.au"));
		}
	}

	public void TestShouldNotTriggerSubscriberUpdateEmail_WhenProcessContactReplyEmails()
	{
		var incident = Factory.New<SupportIncident>();

		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "XCS";
		staff.GS_EmailAddress = "somewhere@nowhere.com";

		var incidentContact = Factory.NewWithValidTestData<OrgContact>();
		incidentContact.OC_Email = "incident_contact@test.com";
		incident.IM_OC_Contact = incidentContact.PK;
		var contact = Factory.NewWithValidTestData<OrgContact>();
		contact.OC_Email = "zubin.appoo@cargowise.com";
		contact.OC_ContactName = "Zubin Appoo";

		var org = Factory.NewWithValidTestData<OrgHeader>();
		org.OH_FullName = "Future Industries";
		org.OH_RL_NKClosestPort = "AUSYD";
		var orgAddress = org.Addresses.MainAddress;
		orgAddress.OA_Email = "org@test.com";
		orgAddress.OA_Address1 = "address 1";

		Factory.Save();

		incident.EConversation.Conversation.Participants.AddNewParticipant(org);
		incident.EConversation.Conversation.Participants.AddNewParticipant(staff);
		incident.EConversation.Conversation.Participants.AddNewParticipant(contact);

		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();

		incident.IM_GS_NKCustServiceContact = staff.GS_Code;
		SetUpIncidentForStage(SupportIncidentCategoriesList.Codes.Support, incident);
		Factory.Save();

		var email = new EmailBuilderForTesting()
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.Subject("Re: Update\\..// on In|cident: " + incident.IM_IncidentNumber + " - Question regarding \"air\" and <sea> invoice* statemen?ts")
			.To("Someone", "test@edi.com.au")
			.Body("please fix this incident now")
			.GetEmail();

		int initialCount = Factory.GetDatabaseCount(typeof(MailItem));

		processor.CreateAndProcessMailItem(email, LoggerForTest);

		var mailItem = GetLastCreatedMailItem();
		AssertEquals("Mail Item MI_Application have to be 'CSV'", "CSV", mailItem.MI_Application);
		AssertEquals("Mail Item status is processed as incident was found", MailStatus.Processed, mailItem.MI_Status);
		AssertEquals("Mail Item direction is set to receive", MailDirection.Receive, mailItem.MI_Direction);
		AssertEquals("saved", true, mailItem.IsInDatabase);
		AssertEquals("saved - no changes", false, mailItem.HasChanges);

		var newFactory = new BusinessObjectFactory();
		var reloadedItem = newFactory.Load<MailItem>(mailItem.PK);
		AssertEquals("Mail Item status is processed as incident was found", MailStatus.Processed, mailItem.MI_Status);
		AssertEquals("Processed is persisted", MailStatus.Processed, reloadedItem.MI_Status);

		incident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
		var docSupportInfo = ((IDocManagerSupport)incident).DocManagerInfo;
		AssertNotEquals("New email should be attached to the incident", 0, docSupportInfo.Files.Count);
		AssertEquals("Type of the document attached to the incident should be 'COR'", "COR", docSupportInfo.Files[0].DocType.ToUpper());
		AssertEquals("File name stripped of invalid chars", "Re Update.. on Incident " + incident.IM_IncidentNumber + " - Question regarding air and sea invoice statements", ((BusinessObject)docSupportInfo.Files[0])["SC_FileName"].ToString());
		AssertEquals("Text of the document attached to the incident should be list of recipients, subject and body",
			mailItem.MI_Header + System.Environment.NewLine + System.Environment.NewLine + mailItem.MI_Body,
			docSupportInfo.Files[0].GetImageDataReader().ConvertToAsciiStringAndCloseStream());

		AssertEquals("Have 1 emails", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		var copyEmail = Env.OutgoingMailManager.EmailsCreated[0];

		AssertEquals("File name stripped of invalid chars", "Re Update.. on Incident " + incident.IM_IncidentNumber + " - Question regarding air and sea invoice statements", ((BusinessObject)docSupportInfo.Files[0])["SC_FileName"].ToString());
		AssertEquals("Customer Service Incident: CS00000001 has been assigned to you", copyEmail.Subject);
	}

	public void TestProcessCustomerServiceEmails_IncidentNumberIncomplete_NotFound()
	{
		GlbStaff someCustServiceStaff = Factory.New<GlbStaff>();
		someCustServiceStaff.GS_Code = "XCS";
		someCustServiceStaff.GS_EmailAddress = "somewhere@nowhere.com";
		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();

		SupportIncident incident = Factory.New<SupportIncident>();
		incident.IM_GS_NKCustServiceContact = someCustServiceStaff.GS_Code;
		SetUpIncidentForStage(SupportIncidentCategoriesList.Codes.Support, incident);
		Factory.Save();

		Email email = new EmailBuilderForTesting()
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.Subject("Re: Update\\..// on In|cident: " + incident.IM_IncidentNumber.Replace("0000", "000") + " - Question regarding \"air\" and <sea> invoice* statemen?ts")
			.To("Someone", "test@edi.com.au")
			.Body("please fix this incident now")
			.GetEmail();

		processor.CreateAndProcessMailItem(email, LoggerForTest);

		MailItem mailItem = GetLastCreatedMailItem();
		AssertEquals("Mail Item MI_Application have to be 'CSV'", "CSV", mailItem.MI_Application);
		AssertEquals("Mail Item status is unprocessed as no incident was found", MailStatus.Unprocessed, mailItem.MI_Status);
		AssertEquals("Mail Item direction is set to receive", MailDirection.Receive, mailItem.MI_Direction);
		AssertEquals("saved", true, mailItem.IsInDatabase);
	}

	[ExpectNoExceptions]
	public void TestProcessCustomerServiceEmails_FileNameMaxLengthExceeded()
	{
		GlbStaff someCustServiceStaff = Factory.New<GlbStaff>();
		someCustServiceStaff.GS_Code = "XCS";
		someCustServiceStaff.GS_EmailAddress = "somewhere@nowhere.com";
		Factory.Save();

		SupportIncident incident = Factory.New<SupportIncident>();
		incident.IM_GS_NKCustServiceContact = someCustServiceStaff.GS_Code;
		SetUpIncidentForStage(SupportIncidentCategoriesList.Codes.Defect, incident);
		Factory.Save();

		string aReallyLongSubject = incident.IM_IncidentNumber + " " + new string('X', MailDBAttachmentsSchema.MA_FileName.MaxLength);
		Email email = new EmailBuilderForTesting()
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.Subject(aReallyLongSubject)
			.To("Someone", "test@edi.com.au")
			.Body("please fix this incident now")
			.GetEmail();

		var processor = new CustomerServiceEmailProcessorForTest();
		processor.CreateAndProcessMailItem(email, LoggerForTest);
	}

	public void TestProcessCustomerServiceEmails_NonSupportStage()
	{
		var processor = new CustomerServiceEmailProcessorForTest();

		GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "SCW";
		staff.GS_EmailAddress = "samuel@test.com";

		SupportIncident incident1 = Factory.New<SupportIncident>();
		SupportIncident incident2 = Factory.New<SupportIncident>();
		SetUpIncidentForStage(SupportIncidentCategoriesList.Codes.Defect, incident1);
		incident1.IM_GS_NKAssignedToCurrent = staff.GS_Code;
		SetUpIncidentForStage(SupportIncidentCategoriesList.Codes.FeatureRequest, incident2);
		incident2.IM_GS_NKAssignedToCurrent = staff.GS_Code;
		Factory.Save();
		Env.OutgoingMailManager.EmailsCreated.Clear();

		Email email1 = new EmailBuilderForTesting()
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.Subject("Re: Please fix " + incident1.IM_IncidentNumber)
			.To("Someone", "test@edi.com.au")
			.Body("please fix this incident now")
			.GetEmail();
		processor.CreateAndProcessMailItem(email1, LoggerForTest);
		MailItem mailItem = GetLastCreatedMailItem();
		AssertEquals("Mail Item MI_Application have to be 'CSV'", "CSV", mailItem.MI_Application);
		AssertEquals("Mail Item status is processed as incident was found", MailStatus.Processed, mailItem.MI_Status);
		AssertEquals("Mail Item direction is set to receive", MailDirection.Receive, mailItem.MI_Direction);
		AssertEquals("saved", true, mailItem.IsInDatabase);
		AssertEquals("saved - no changes", false, mailItem.HasChanges);
		incident1 = new BusinessObjectFactory().Load<SupportIncident>(incident1.PK);
		DocManagerInfo docSupportInfo = ((IDocManagerSupport)incident1).DocManagerInfo;
		AssertEquals("New email should be attached to the incident", 1, docSupportInfo.Files.Count);
		AssertContains(incident1.IM_IncidentNumber, ((BusinessObject)docSupportInfo.Files[0])["SC_FileName"].ToString());
		AssertEquals("Have emails", 1, Env.OutgoingMailManager.EmailsCreated.Count);

		EmailDef subscriberUpdateEConversationEmail1 = Env.OutgoingMailManager.EmailsCreated[0];
		AssertContains("New Messages in", subscriberUpdateEConversationEmail1.Subject);
		AssertEquals("subscriberUpdateEConversationEmail Recipients count", 1, subscriberUpdateEConversationEmail1.Recipients.Count);
		AssertEquals("subscriberUpdateEConversationEmail Recipient", "test@edi.com.au", subscriberUpdateEConversationEmail1.Recipients[0].Email);

		Env.OutgoingMailManager.EmailsCreated.Clear();
		Email email2 = new EmailBuilderForTesting()
			.From("Richard White", "richard.white@cargowise.com")
			.Subject("Re: MEH " + incident2.IM_IncidentNumber)
			.To("Support", "test@edi.com.au")
			.Body("MEH MEH MEH")
			.GetEmail();
		processor.CreateAndProcessMailItem(email2, LoggerForTest);
		mailItem = GetLastCreatedMailItem();
		AssertEquals("Mail Item MI_Application have to be 'CSV'", "CSV", mailItem.MI_Application);
		AssertEquals("Mail Item status is processed as incident was found", MailStatus.Processed, mailItem.MI_Status);
		AssertEquals("Mail Item direction is set to receive", MailDirection.Receive, mailItem.MI_Direction);
		AssertEquals("saved", true, mailItem.IsInDatabase);
		AssertEquals("saved - no changes", false, mailItem.HasChanges);
		incident2 = new BusinessObjectFactory().Load<SupportIncident>(incident2.PK);
		docSupportInfo = ((IDocManagerSupport)incident2).DocManagerInfo;
		AssertEquals("New email should be attached to the incident", 1, docSupportInfo.Files.Count);
		AssertContains(incident2.IM_IncidentNumber, ((BusinessObject)docSupportInfo.Files[0])["SC_FileName"].ToString());
		AssertEquals("Still have notifications", 1, Env.OutgoingMailManager.EmailsCreated.Count);

		EmailDef subscriberUpdateEConversationEmail2 = Env.OutgoingMailManager.EmailsCreated[0];
		AssertContains("New Messages in", subscriberUpdateEConversationEmail2.Subject);
		AssertEquals("subscriberUpdateEConversationEmail Recipients count", 1, subscriberUpdateEConversationEmail2.Recipients.Count);
		AssertEquals("subscriberUpdateEConversationEmail Recipient", "test@edi.com.au", subscriberUpdateEConversationEmail2.Recipients[0].Email);
	}

	#endregion

	#region Incident Retrieval

	public void TestGetIncidentNumberFromSubject()
	{
		var processor = new CustomerServiceEmailProcessorForTest();
		AssertEquals("CS00001234", processor.GetIncidentNumberFromSubject("Hello CS00001234 everyone"));
		AssertEquals("No guesswork", "", processor.GetIncidentNumberFromSubject("CS1234 everyone"));
		AssertEquals("no guesswork", "", processor.GetIncidentNumberFromSubject("Hello cS01234"));
		AssertEquals("lower case", "CS00001234", processor.GetIncidentNumberFromSubject("Hello cS00001234"));
		AssertEquals("No number", "", processor.GetIncidentNumberFromSubject("Hello everyone"));
		AssertEquals("Too long", "", processor.GetIncidentNumberFromSubject("Hello CS100001234 everyone"));
		AssertEquals("CS10001234", processor.GetIncidentNumberFromSubject("Hello CS10001234 everyone"));
		AssertEquals("no space", "", processor.GetIncidentNumberFromSubject("Hello CS10001234everyone"));
		AssertEquals("character in number", "", processor.GetIncidentNumberFromSubject("Hello CS100a1234"));
		AssertEquals("character before the name", "", processor.GetIncidentNumberFromSubject("Hello noCS00001234"));
		AssertEquals("should find incident", "CS00000101", processor.GetIncidentNumberFromSubject("RE: [Urgent] FW: HyundaiMM connection:B52 | Bollore Logistics | WiseTechGlobal| CS00000101 |"));
	}

	#endregion

	#region Attach Email to Incident

	[ExpectNoExceptions]
	public void TestAttachEmailToIncident_EDocShouldNotThrowArgumentException()
	{
		SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
		incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR5_Training;
		MailItem mail = CreateMailItem("Some One <someclient@somewhere.com>", "hello ");
		var processor = new CustomerServiceEmailProcessorForTest();
		processor.AttachEmailToIncidentForTest(mail, incident, LoggerForTest);
		DocManagerInfo docSupportInfo = ((IDocManagerSupport)incident).DocManagerInfo;
		StorageFile file = docSupportInfo.Files[0] as StorageFile;
		AssertEquals("Filename should be correctly stripped", "hello", file.SC_FileName);
	}

	public void TestAttachEmailToIncident_ShouldSendAsPublishedAttachment()
	{
		OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

		LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
		enterprise.LE_EnterpriseCode = "DDD";
		enterprise.LE_OH = org.PK;

		LicenceDatabase database = Factory.New<LicenceDatabase>();
		database.LD_ServerCode = "SRV";
		database.LD_LE = enterprise.PK;
		database.LD_PublicEmailAddressForUpdate = "test@test.com";

		ClientCompany clientCompany = Factory.New<ClientCompany>();
		clientCompany.LCC_Code = "COM";
		clientCompany.LCC_LD = database.PK;

		RefDocType docType = Factory.Load<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "COR"))[0];
		docType.RT_IsPublished = true;
		docType.RT_ReferenceType = "ALL";
		Factory.Save();

		SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
		incident.IM_LD = database.PK;
		incident.IM_LCC = clientCompany.PK;
		incident.IM_ClientIncidentReference = "CL111111";
		incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR5_Training;
		SetUpIncidentForStage(SupportIncidentCategoriesList.Codes.Support, incident);
		MailItem mail = CreateMailItem("Some One <someclient@somewhere.com>", "hellomoto");
		mail.MI_Body = "the other end, this must reach";
		mail.MI_Direction = DirectionList.Codes.Transmit;

		var processor = new CustomerServiceEmailProcessorForTest();
		Env.OutgoingMailManager.EmailsCreated.Clear();
		processor.AttachEmailToIncidentForTest(mail, incident, LoggerForTest);
		Factory.Save();
		AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

		// Customer System Email with attached Published Docs.
		EmailDef emailWithAttachedDocs = Env.OutgoingMailManager.EmailsCreated[0];
		AssertEquals("Customer Service Incident Raised - Your Ref: " + incident.IM_ClientIncidentReference, emailWithAttachedDocs.Subject);
		AssertEquals(1, emailWithAttachedDocs.Attachments.Count);
		AssertEquals("Incident Details.xml", emailWithAttachedDocs.Attachments[0].DisplayName);

		ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceResponse));
		Xsd.CustomerServiceResponse response;
		using (MemoryStream ms = new MemoryStream(emailWithAttachedDocs.Attachments[0].Data))
		{
			response = (Xsd.CustomerServiceResponse)serializer.Deserialize(ms);
		}
		AssertEquals(1, response.Attachments.Count);
		AssertEquals("hellomoto.eml", response.Attachments[0].FileName);
		AssertEquals("COR", response.Attachments[0].DocType);
		AssertContains("the other end, this must reach", Encoding.ASCII.GetString(response.Attachments[0].Data));
	}

	public void TestAttachEmailToIncident_ShouldAddMessageReceivedEventToIncidentManagementGroup()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();

		var enterprise = Factory.New<LicenceEnterprise>();
		enterprise.LE_EnterpriseCode = "DDD";
		enterprise.LE_OH = org.PK;

		var database = Factory.New<LicenceDatabase>();
		database.LD_ServerCode = "SRV";
		database.LD_LE = enterprise.PK;
		database.LD_PublicEmailAddressForUpdate = "test@test.com";

		var clientCompany = Factory.New<ClientCompany>();
		clientCompany.LCC_Code = "COM";
		clientCompany.LCC_LD = database.PK;

		var docType = Factory.Load<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "COR"))[0];
		docType.RT_IsPublished = true;
		docType.RT_ReferenceType = "ALL";
		Factory.Save();

		var incidentGroup = Factory.NewWithValidTestData<IncidentManagementGroup>();

		var incident = Factory.NewWithValidTestData<SupportIncident>();
		incident.IM_LD = database.PK;
		incident.IM_LCC = clientCompany.PK;
		incident.IM_ClientIncidentReference = "CL111111";
		incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR5_Training;
		SetUpIncidentForStage(SupportIncidentCategoriesList.Codes.Support, incident);
		MailItem mail = CreateMailItem("Some One <someclient@somewhere.com>", "hellomoto");
		mail.MI_Body = "the other end, this must reach";
		mail.MI_Direction = DirectionList.Codes.Transmit;

		var incidentLink = Factory.NewWithValidTestData<IncidentManagementLink>();
		incidentLink.INL_ING_Group = incidentGroup.PK;
		incidentLink.INL_IM_Incident = incident.PK;

		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();
		Env.OutgoingMailManager.EmailsCreated.Clear();
		processor.AttachEmailToIncidentForTest(mail, incident, LoggerForTest);
		Factory.Save();
		AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

		Factory.Save();

		var incidentGroupReloaded = new BusinessObjectFactory().Load<IncidentManagementGroup>(incidentGroup.PK);
		var messageReceivedLogs = incidentGroupReloaded.Logs.GetAllLogs().Where(x => x.SL_SE_NKEvent == AutoEvents.MessageReceivedCode).ToArray();
		AssertEquals("Should have added a single MRR event", 1, messageReceivedLogs.Length);
		var messageReceivedLog = messageReceivedLogs.First();
		var freeTextBits = new List<string>();
		var paramBits = new List<(string Key, string Value)>();
		EventLogReferenceBuilder.New().ParseReference(messageReceivedLog.SL_Reference, (text) => freeTextBits.Add(text), (key, value) => paramBits.Add((key, value)));
		AssertEquals(incident.IM_IncidentNumber, paramBits.First(x => x.Key == Constants.EventReferenceParameters.Codes.ReferenceNumber).Value);
	}

	#endregion

	#region Send Auto Answer

	public void TestSendSupportAutoReplyEmail_EmptyRegistryValue()
	{
		var processor = new CustomerServiceEmailProcessorForTest();

		Email email = new EmailBuilderForTesting()
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.Subject("CS00001234 Approval email")
			.To("Someone", "test@edi.com.au")
			.Body("please fix this incident now")
			.GetEmail();

		int initialCount = Env.OutgoingMailManager.EmailsCreated.Count;
		processor.CreateAndProcessMailItem(email, LoggerForTest);
		AssertEquals("Auto answer outgoing email should not be created", initialCount, Env.OutgoingMailManager.EmailsCreated.Count);

		EDIDataRegistry.Instance.CustomerSupportAutoReplyEmailTemplate = "<HTML><BODY><B>test</B></BODY></HTML>";
		processor.CreateAndProcessMailItem(email, LoggerForTest);
		AssertEquals("Auto answer outgoing email should be created", initialCount + 1, Env.OutgoingMailManager.EmailsCreated.Count);
		EmailDef answerEmail = Env.OutgoingMailManager.EmailsCreated[0];
		AssertEquals("Body should be empty", "<HTML><BODY><B>test</B></BODY></HTML>", answerEmail.Body);
	}

	public void TestSendSupportAutoReplyEmail()
	{
		GlbStaff someCustServiceStaff = Factory.New<GlbStaff>();
		someCustServiceStaff.GS_Code = "XCS";
		someCustServiceStaff.GS_EmailAddress = "somewhere@nowhere.com";
		Factory.Save();

		var processor = new CustomerServiceEmailProcessorForTest();

		SupportIncident incident = Factory.New<SupportIncident>();
		incident.IM_IncidentNumber = "CS00001234";
		incident.IM_GS_NKCustServiceContact = someCustServiceStaff.GS_Code;
		Factory.Save();

		EDIDataRegistry.Instance.CustomerSupportAutoReplyEmailTemplate = "<HTML><BODY><B>test</B></BODY></HTML>";
		Email email = new EmailBuilderForTesting()
			.From("Zubin Appoo", "zubin.appoo@cargowise.com")
			.Subject("CS00001234 Approval email")
			.To("Someone", "test@edi.com.au")
			.Body("please fix this incident now")
			.GetEmail();

		int initialCount = Env.OutgoingMailManager.EmailsCreated.Count;
		processor.CreateAndProcessMailItem(email, LoggerForTest);
		var targetEmail = Env.OutgoingMailManager.EmailsCreated.Where(x => !x.Subject.Contains("New Messages in")).ToList();
		AssertEquals("Auto answer outgoing email should be created", initialCount + 1, targetEmail.Count);

		EmailDef answerEmail = targetEmail[initialCount];
		AssertEquals("Answer Subject", "Re: CS00001234 Approval email", answerEmail.Subject);
		AssertEquals("Answer Recipients count", 1, answerEmail.Recipients.Count);
		AssertEquals("Answer Recipient", "Zubin Appoo <zubin.appoo@cargowise.com>", answerEmail.Recipients[0].Email);
		AssertEquals("has to be HTML", EmailContentTypes.HTML, answerEmail.ContentType);

		var expectedBody = $"{EDIDataRegistry.Instance.CustomerSupportAutoReplyEmailTemplate}<p>{SupportIncidentEmailBodyGeneralControls.GetReplyViaEConversationButton(incident)}</p>Regards,<br/>";
		AssertEquals("Body", expectedBody, answerEmail.Body);

		var emlText = @"
Subject: Enquiry
From: undisclosed-recipients:;
To: sam@gmail.com;
MIME-Version: 1.0

This is a junk mail.";
		email = new Email(Encoding.UTF8.GetBytes(emlText));

		EmailDef subscriberUpdateEConversationEmail = Env.OutgoingMailManager.EmailsCreated[initialCount + 1];
		AssertContains("New Messages in", subscriberUpdateEConversationEmail.Subject);
		AssertEquals("subscriberUpdateEConversationEmail Recipients count", 1, subscriberUpdateEConversationEmail.Recipients.Count);
		AssertEquals("subscriberUpdateEConversationEmail Recipient", "test@edi.com.au", subscriberUpdateEConversationEmail.Recipients[0].Email);

		Env.OutgoingMailManager.EmailsCreated.Clear();
		processor.CreateAndProcessMailItem(email, LoggerForTest);
		AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
	}

	#endregion

	#region Closed Incident When New Email Received

	public void TestClosedIncident_WhenANewEmailIsReceivedAndClosedInSupportStage()
	{
		TestReOpenClosedIncident(ZDateTime.Now.AddDays(-6), SupportIncidentLookups.Status.Working, SupportIncidentCategoriesList.Codes.Support);
	}

	public void TestClosedIncident_WhenANewEmailIsReceivedAndOlderThanSevenDays()
	{
		TestReOpenClosedIncident(ZDateTime.Now.AddDays(-8), SupportIncidentLookups.Status.Working, SupportIncidentCategoriesList.Codes.Support);
	}

	public void TestClosedIncident_WhenANewEmailIsReceivedAndClosedInDefectStage()
	{
		TestReOpenClosedIncident(ZDateTime.Now.AddDays(-6), SupportIncidentLookups.Status.Open, SupportIncidentCategoriesList.Codes.Defect,
				delegate (SupportIncident incident)
				{
					incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "this is a defect");
					incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.NoDefectFound, "");
				},
				delegate (SupportIncident incident)
				{
					AssertEquals("Sanity check", SupportIncidentLookups.Status.Closed, incident.IM_Status);
					AssertEquals("Sanity check", SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
				});
	}

	public void TestClosedIncident_WhenANewEmailIsReceivedAndClosedInFeatureRequestStage()
	{
		TestReOpenClosedIncident(ZDateTime.Now.AddDays(-6), SupportIncidentLookups.Status.Open, SupportIncidentCategoriesList.Codes.FeatureRequest,
				delegate (SupportIncident incident)
				{
					incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "this is a new feature");
					incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, "");
				},
				delegate (SupportIncident incident)
				{
					AssertEquals("Sanity check", SupportIncidentLookups.Status.Closed, incident.IM_Status);
					AssertEquals("Sanity check", SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
				});
	}

	public void TestDoNotReOpenClosedIncident_WhenANewEmailIsReceivedAndIM_StatusIsNotClosed()
	{
		TestReOpenClosedIncident(ZDateTime.Now.AddDays(-6), SupportIncidentLookups.Status.Open, SupportIncidentCategoriesList.Codes.Support,
				delegate
				{ },
				delegate (SupportIncident incident)
				{
					AssertEquals("Sanity check", SupportIncidentLookups.Status.Open, incident.IM_Status);
					AssertEquals("Sanity check", SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
				});
	}

	public void TestDoNotReOpenClosedIncident_WhenANewEmailIsReceivedAndIM_StatusIsNotClosedInDefect()
	{
		TestReOpenClosedIncident(ZDateTime.Now.AddDays(-6), SupportIncidentLookups.Status.Open, SupportIncidentCategoriesList.Codes.Defect,
				(incident) => incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "this is a defect"),
				delegate (SupportIncident incident)
				{
					AssertEquals("Sanity check", SupportIncidentLookups.Status.Open, incident.IM_Status);
					AssertEquals("Sanity check", SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
				});
	}

	public void TestDoNotReOpenClosedIncident_WhenANewEmailIsReceivedAndIM_StatusIsNotClosedInFeatureRequest()
	{
		TestReOpenClosedIncident(ZDateTime.Now.AddDays(-6), SupportIncidentLookups.Status.Open, SupportIncidentCategoriesList.Codes.FeatureRequest,
				(incident) => incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "this is a feature"),
				delegate (SupportIncident incident)
				{
					AssertEquals("Sanity check", SupportIncidentLookups.Status.Open, incident.IM_Status);
					AssertEquals("Sanity check", SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
				});
	}

	public void TestReOpenClosedIncident_ClosedAwaitingResponse()
	{
		TestReOpenClosedIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, ZDateTime.Now.AddDays(-14), SupportIncidentLookups.Status.Working, SupportIncidentCategoriesList.Codes.FeatureRequest,
				(incident) => incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, ""),
				(incident) => AssertEquals("Sanity check", SupportIncidentLookups.Status.Closed, incident.IM_Status));
	}

	delegate void IncidentDelegate(SupportIncident incident);

	void TestReOpenClosedIncident(ZDateTime incidentCloseDate, string expectedIM_Status, string expectedIM_Category)
	{
		TestReOpenClosedIncident(incidentCloseDate, expectedIM_Status, expectedIM_Category,
				(incident) => incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "Resolved"),
				(incident) => AssertEquals("Sanity check", SupportIncidentLookups.Status.Closed, incident.IM_Status));
	}

	void TestReOpenClosedIncident(ZDateTime incidentCloseDate, string expectedIM_Status, string expectedIM_Category, IncidentDelegate closingIncident, IncidentDelegate assertIncidentBeforeProcessing)
	{
		TestReOpenClosedIncident(SupportIncidentCategoriesList.Codes.Support, incidentCloseDate, expectedIM_Status, expectedIM_Category, closingIncident, assertIncidentBeforeProcessing);
	}

	void TestReOpenClosedIncident(ZString initialIM_Category, ZDateTime incidentCloseDate, string expectedIM_Status, string expectedIM_Category, IncidentDelegate closingIncident, IncidentDelegate assertIncidentBeforeProcessing)
	{
		GlbStaff staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "SCW");
		if (staff == null)
		{
			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";
		}

		SupportIncident incident = Factory.New<SupportIncident>();
		incident.IM_SystemCreateTimeUtc = incidentCloseDate.AddDays(-1);
		incident.IM_Category = initialIM_Category;
		incident.IM_IncidentNumber = "CS01234567";
		incident.AssignToStaff(staff, "");
		closingIncident(incident);
		incident.IM_CloseTimeUtc = incidentCloseDate;
		Factory.Save();

		assertIncidentBeforeProcessing(incident);
		var processor = new CustomerServiceEmailProcessorForTest();
		Email email = new EmailBuilderForTesting()
			.Subject("This is for CS01234567")
			.To("Someone", "test@edi.com.au")
			.From("support@cargowise.com")
			.Body("This is not yet fixed")
			.GetEmail();
		Assert("Should be processed successfully", processor.CreateAndProcessMailItem(email, LoggerForTest));
		AssertEquals(expectedIM_Status, incident.IM_Status);
		AssertEquals(expectedIM_Category, incident.IM_Category);
	}

	#endregion

	#region Implementation

	protected override INumberFountainProxy NumberFountainForBusinessObject
	{
		get { return Env.NumberFountains.CustomerServiceIncidentNo; }
	}

	protected override string ExpectedEmailTypeName
	{
		get { return "Support"; }
	}

	protected override string ExpectedMailApplicationCode
	{
		get { return EDIMailApplication.CustomerService; }
	}

	protected override CustomerServiceEmailProcessorForTest GetNewBusinessObjectEmailProcessor()
	{
		return new CustomerServiceEmailProcessorForTest();
	}

	protected override void SetUp()
	{
		base.SetUp();
		EDIDataRegistry.Instance.CustomerSupportAutoReplyEmailTemplate = string.Empty;
		EDIDataRegistry.Instance.CurrentVersionReportMailBox.Server = "TestMailServer";
		EDIDataRegistry.Instance.CurrentVersionReportMailBox.UserName = "CustomerService";
	}

	MailItem CreateMailItem(string from, string subject)
	{
		MailItem result = Factory.New<MailItem>();
		result.MI_From = from;
		result.MI_Subject = subject;
		result.MI_SendDateTime = ZDateTime.Now;
		result.MI_ReceivedDateTime = ZDateTime.Now;
		return result;
	}

	void SetUpIncidentForStage(string stage, SupportIncident incident)
	{
		incident.IM_Category = stage;
		incident.IM_Status = SupportIncidentLookups.Status.Open;

		if (stage == SupportIncidentCategoriesList.Codes.Defect || stage == SupportIncidentCategoriesList.Codes.FeatureRequest)
		{
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment;
		}
	}

	protected override void SetReferenceNumber(SupportIncident bizO, string refNumber)
	{
		bizO.IM_IncidentNumber = refNumber;
	}

	#endregion

	class TestCustomerServiceEmailProcessor : CustomerServiceEmailProcessorForTest
	{
		protected override void AttachEmailToAnExistingBusinessObject(MailItem mailItem, SupportIncident incident, IEmailProcessorLogger logger)
		{
			base.AttachEmailToAnExistingBusinessObject(mailItem, incident, logger);
			this.processedIncident = incident;
		}

		SupportIncident processedIncident;

		protected override void SaveMailFactory(MailItem mailItem)
		{
			if (!isSaveConcurrencyTriggered)
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				var loadedIncident = factory.Load<SupportIncident>(processedIncident.PK);
				loadedIncident.IM_Status = "OPN";
				loadedIncident.IM_ResolutionCode = "AUC";
				factory.Save();
				isSaveConcurrencyTriggered = true;
			}

			base.SaveMailFactory(mailItem);
		}

		bool isSaveConcurrencyTriggered;
	}

	[ExpectNoExceptions]
	public void TestReOpenClosedIncident_WhenANewEmailIsReceivedAndClosedInSupportStage_NoConcurrencyErrors()
	{
		GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "JNG";

		var contact = Factory.NewWithValidTestData<OrgContact>();
		contact.OC_Email = "test@edi.com.au";

		Factory.Save();

		var incidentFactory = new BusinessObjectFactory() { RefreshEnabled = false };

		var incident = incidentFactory.New<SupportIncident>();
		incident.IM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-7);
		incident.IM_OH_Client = contact.OC_OH;
		incident.IM_OC_Contact = contact.PK;
		incident.IM_IncidentNumber = "CS01234567";
		incident.IM_Status = "CLS";
		incident.IM_ResolutionCode = "CWR";
		incident.AssignToStaff(staff, "");
		incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "Resolved");
		incident.IM_CloseTimeUtc = ZDateTime.Now.AddDays(-6);
		incidentFactory.Save();

		//Remove IM_Contact participant to test old data. Because the SupportIncident in the current version will add it automatically, but the old cannot
		var participantFactory = new BusinessObjectFactory() { RefreshEnabled = false };
		participantFactory.LoadTop1<JobConversationParticipant>(new ZQuery(JobConversationParticipantSchema.JCP_ParticipantID, incident.IM_OC_Contact)).Delete();
		participantFactory.Save();

		var incidentToReOpen = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
		Assert(!incidentToReOpen.EConversation.Conversation.Participants.Any(x => x.JCP_ParticipantID == contact.PK));

		var email = new EmailBuilderForTesting()
			.Subject("This is for CS01234567")
			.To("Someone", "support@cargowise.com")
			.From("test@edi.com.au")
			.Body("This is not yet fixed")
			.GetEmail();

		var query = new ZQuery(MailDBItemsSchema.MI_Application, ExpectedMailApplicationCode);
		AssertEquals("Precondition", 0, Factory.Load<MailItem>(query).Length);

		CustomerServiceEmailProcessor processor = new TestCustomerServiceEmailProcessor();
		Assert("Should be processed successfully", processor.CreateAndProcessMailItem(email, LoggerForTest));

		var resultFactory = new BusinessObjectFactory() { RefreshEnabled = false };
		var emails = resultFactory.Load<MailItem>(query).ToList();
		AssertEquals(1, resultFactory.Load<MailItem>(query).Length);
		AssertEquals(1, resultFactory.Load<JobConversationMessage>(new ZQuery(JobConversationMessageSchema.JCM_Body, SQLComparisonOperator.Contains, "This is not yet fixed")).Length);
	}

	class CustomerServiceEmailProcessorForTestWithForcingFactorySave : CustomerServiceEmailProcessorForTest
	{
		protected override void AttachEmailToAnExistingBusinessObject(MailItem mailItem, SupportIncident incident, IEmailProcessorLogger logger)
		{
			incident.HasChanges = true; //This causes incident being saved in mailItem's factory
			base.AttachEmailToAnExistingBusinessObject(mailItem, incident, logger);
		}
	}

	public void TestReOpenClosedIncident_IncidentBeingSavedTwice()
	{
		var factory = new BusinessObjectFactory();
		factory.RefreshEnabled = false;

		var helper = ObjectFactory.Get<IBMTestHelper>();

		helper.EnableBMSInRegistry();
		var system = helper.CreateSystem(Factory, "INC");

		var templateOrg = factory.NewWithValidTestData<OrgHeader>();
		EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

		var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
		template.P0_ProcessType = "INC";
		template.P0_OH_Client = templateOrg.PK;

		var header = template.ProcessHeaders.AddNew();
		header.FH_CompletionStatement = "REQ Investigate";

		var templateTask = template.WorkflowItems.AddNew();
		templateTask.P9_FH_ProcessHeader = header.PK;
		templateTask.P9_Sequence = 10;
		templateTask.P9_Type = "AAA";
		templateTask.P9_Description = "Contact Client";
		templateTask.P9_GS_NKAssignedStaffMember = "JNG";
		templateTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

		var docType = factory.New<RefDocType>();
		docType.RT_DocType = "COR";
		docType.RT_IsPublished = true;
		docType.RT_ReferenceType = "ALL";

		factory.Save();

		GlbStaff staff = factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "JNG";
		staff.GS_FullName = "Jenny";

		SupportIncident incidentToReOpen = factory.New<SupportIncident>();
		incidentToReOpen.IM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-7);
		incidentToReOpen.IM_IncidentNumber = "CS01234567";
		incidentToReOpen.IM_Status = "CLS";
		incidentToReOpen.IM_ResolutionCode = "CWR";
		incidentToReOpen.AssignToStaff(staff, "");
		var task1 = incidentToReOpen.WorkflowItems.AddNew();
		task1.P9_Sequence = 10;
		task1.P9_Description = "Investigate";
		task1.P9_GS_NKAssignedStaffMember = "JNG";
		task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

		incidentToReOpen.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
		incidentToReOpen.IM_CloseTimeUtc = ZDateTime.Now.AddDays(-6);
		factory.Save();

		Email email = new EmailBuilderForTesting()
			.Subject("This is for CS01234567")
			.To("Someone", "test@edi.com.au")
			.From("support@cargowise.com")
			.Body("This is not yet fixed")
			.GetEmail();

		ZQuery query = new ZQuery(MailDBItemsSchema.MI_Application, ExpectedMailApplicationCode);
		AssertEquals("Precondition", 0, factory.Load<MailItem>(query).Length);

		var processor = new CustomerServiceEmailProcessorForTestWithForcingFactorySave();
		Assert("Should be processed successfully", processor.CreateAndProcessMailItem(email, LoggerForTest));
		AssertEquals(1, factory.Load<MailItem>(query).Length);

		var loadFactory = new BusinessObjectFactory();
		var loadedIncident = loadFactory.Load<SupportIncident>(incidentToReOpen.PK);
		AssertEquals(2, loadedIncident.WorkflowItems.Count);
		AssertEquals(SupportIncidentLookups.Status.Working, loadedIncident.IM_Status);
		AssertEquals("JNG", loadedIncident.IM_GS_NKCustServiceContact);
	}

	public void TestSendingCustomerServiceFinalClosureAutoReplyEmail()
	{
		var tempalteCollection = EDIDataRegistry.Instance.CustomerServiceFinalClosureAutoReplyEmailTemplate.Value;
		var templatePair = tempalteCollection.FirstOrDefault() as CodeDescriptionIncidentEmailTemplatePair;
		templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "[FLR]Customer Service Final Closure Auto Reply Email";
		templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "Test Only EmailBody";
		templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = "[FLR]Customer Service Final Closure Auto Reply Email";
		templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = "Test Only EmailBody";

		var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
		var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
		criticalityResolutionAndClosureBehaviour.Code = CriticalityCodes.CR4_SingleFunctionWithWorkAround;

		var expireDays = 30;
		var entProduct = resolutionAndClosureBehaviourCollection.AddNew();
		entProduct.ParentID = criticalityResolutionAndClosureBehaviour.ID;
		entProduct.Code = ProductTypes.Codes.Enterprise;
		entProduct.ClosedReopenRule = ResolutionAndClosureBehaviour.Constants.Code.AlwaysAllow;
		entProduct.DaysPendingCustomerToClosed = expireDays;

		var cw1Product = resolutionAndClosureBehaviourCollection.AddNew();
		cw1Product.ParentID = criticalityResolutionAndClosureBehaviour.ID;
		cw1Product.Code = ProductTypes.Codes.CargoWiseOne;
		cw1Product.ClosedReopenRule = ResolutionAndClosureBehaviour.Constants.Code.NeverAllow;
		cw1Product.DaysPendingCustomerToClosed = expireDays;

		EDIDataRegistry.Instance.CustomerServiceFinalClosureAutoReplyEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tempalteCollection);
		EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection);

		var contact = Factory.NewWithValidTestData<OrgContact>();
		contact.OC_Email = "123z@123.com";
		var incident = Factory.NewWithValidTestData<SupportIncident>();
		incident.IM_IncidentNumber = "CS00001234";
		incident.IM_Product = ProductTypes.Codes.Enterprise;
		incident.IM_Priority = CriticalityCodes.CR4_SingleFunctionWithWorkAround;
		incident.IM_OC_Contact = contact.PK;

		Factory.Save();
		AssertEquals(CriticalityCodes.CR4_SingleFunctionWithWorkAround, incident.IM_Priority);
		AssertEquals(ProductTypes.Codes.Enterprise, incident.IM_Product);
		Assert(!incident.IsClosedOrCancelled);

		var email = new EmailBuilderForTesting()
			.Subject($"This is for {incident.Number}")
			.To("Customer Service", "support@cargowise.com")
			.From("test@edi.com.au")
			.Body("This is not yet fixed")
			.GetEmail();

		var processor = new CustomerServiceEmailProcessorForTestWithForcingFactorySave();
		Assert("Should be processed successfully", processor.CreateAndProcessMailItem(email, LoggerForTest));
		Assert("Active incident should not send email", !Env.OutgoingMailManager.EmailsCreated.Any(x => x.Subject.StartsWith("[FLR]")));

		incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
		Factory.Save();
		Assert(incident.IsClosedOrCancelled);
		AssertEquals(ResolutionAndClosureBehaviour.Constants.Code.AlwaysAllow, incident.GetClosedReopenRule());
		Assert("Should be processed successfully", processor.CreateAndProcessMailItem(email, LoggerForTest));
		Assert("The email should not be send if current closed reopen rule is Always Allow", !Env.OutgoingMailManager.EmailsCreated.Any(x => x.Subject.StartsWith("[FLR]")));

		incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
		incident.IM_Product = ProductTypes.Codes.CargoWiseOne;
		Factory.Save();
		Assert(incident.IsClosedOrCancelled);
		AssertEquals(ResolutionAndClosureBehaviour.Constants.Code.NeverAllow, incident.GetClosedReopenRule());
		Assert("Should be processed successfully", processor.CreateAndProcessMailItem(email, LoggerForTest));

		var expectedEmail = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.StartsWith("[FLR]"));
		AssertNotNull("The email should be send if current closed reopen rule is Never Allow", expectedEmail);
		var recipients = expectedEmail.Recipients.ToStringCollection();
		Assert("The email should only be sent to reply's sender", recipients.Count == 1 && recipients[0] == "test@edi.com.au");
	}

	public void TestAttachEmailToIncident_ShouldSendAutoReplyToIncidentManagementGroup()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();

		var enterprise = Factory.New<LicenceEnterprise>();
		enterprise.LE_EnterpriseCode = "DDD";
		enterprise.LE_OH = org.PK;

		var database = Factory.New<LicenceDatabase>();
		database.LD_ServerCode = "SRV";
		database.LD_LE = enterprise.PK;
		database.LD_PublicEmailAddressForUpdate = "test@test.com";

		var clientCompany = Factory.New<ClientCompany>();
		clientCompany.LCC_Code = "COM";
		clientCompany.LCC_LD = database.PK;

		var docType = Factory.Load<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "COR"))[0];
		docType.RT_IsPublished = true;
		docType.RT_ReferenceType = "ALL";
		Factory.Save();

		var incidentGroup = Factory.NewWithValidTestData<IncidentManagementGroup>();
		incidentGroup.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
		incidentGroup.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
		incidentGroup.ING_Priority = CriticalityCodes.CR3_SingleFunctionNoWorkAround;
		incidentGroup.ING_IsAutoReply = true;
		incidentGroup.NowStage.ControlIncidents = true;
		var groupMessages = incidentGroup.IncidentManagementGroupMessages.Cast<IncidentManagementGroupMessage>(); // need call the collection to create default messages
		var autoReplay = groupMessages.First(m => m.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.AutoReply);
		autoReplay.IGM_Message = "Test auto reply message";
		autoReplay.IGM_IsPublished = true;

		var incident = Factory.NewWithValidTestData<SupportIncident>();
		incident.IM_LD = database.PK;
		incident.IM_LCC = clientCompany.PK;
		incident.IM_ClientIncidentReference = "CL111111";
		incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR5_Training;
		SetUpIncidentForStage(SupportIncidentCategoriesList.Codes.Support, incident);
		var mail = CreateMailItem("Some One <someclient@somewhere.com>", "hellomoto");
		mail.MI_Body = "the other end, this must reach";
		mail.MI_Direction = DirectionList.Codes.Transmit;

		var incidentLink = Factory.NewWithValidTestData<IncidentManagementLink>();
		incidentLink.INL_ING_Group = incidentGroup.PK;
		incidentLink.INL_IM_Incident = incident.PK;

		Factory.Save();

		AssertEquals("Should not contain auto-reply message", false, incident.EConversation.JobConversationForTest.Messages.Any(m => m.Body.Contains("Test auto reply message")));

		var processor = new CustomerServiceEmailProcessorForTest();
		Env.OutgoingMailManager.EmailsCreated.Clear();
		processor.AttachEmailToIncidentForTest(mail, incident, LoggerForTest);
		Factory.Save();
		AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

		Factory.Save();

		var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
		AssertEquals("Should contain auto-reply message", true, loadedIncident.EConversation.JobConversationForTest.Messages.Any(m => m.Body.Contains("Test auto reply message")));
	}

	public void TestEmailReplyShouldMakeIsCustomerResolvedToFalse()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();

		var enterprise = Factory.New<LicenceEnterprise>();
		enterprise.LE_EnterpriseCode = "DDD";
		enterprise.LE_OH = org.PK;

		var database = Factory.New<LicenceDatabase>();
		database.LD_ServerCode = "SRV";
		database.LD_LE = enterprise.PK;
		database.LD_PublicEmailAddressForUpdate = "test@test.com";

		var clientCompany = Factory.New<ClientCompany>();
		clientCompany.LCC_Code = "COM";
		clientCompany.LCC_LD = database.PK;

		var contact = Factory.NewWithValidTestData<OrgContact>();
		contact.OC_Email = "123z@123.com";

		var docType = Factory.Load<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "COR"))[0];
		docType.RT_IsPublished = true;
		docType.RT_ReferenceType = "ALL";
		Factory.Save();

		var incident = Factory.NewWithValidTestData<SupportIncident>();
		incident.IM_IncidentNumber = "CS00001234";
		incident.IM_LD = database.PK;
		incident.IM_LCC = clientCompany.PK;
		incident.IM_ClientIncidentReference = "CL111111";
		incident.IM_Product = ProductTypes.Codes.Enterprise;
		incident.IM_Priority = CriticalityCodes.CR4_SingleFunctionWithWorkAround;
		incident.IM_OC_Contact = contact.PK;
		SetUpIncidentForStage(SupportIncidentCategoriesList.Codes.Support, incident);
		Factory.Save();

		incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
		Factory.Save();

		Assert(incident.Request.INC_IsCustomerResolved);

		var email = new EmailBuilderForTesting()
			.Subject($"This is for CS00001234")
			.To("Customer Service", "support@cargowise.com")
			.From("123z@123.com")
			.Body("This is not yet fixed")
			.GetEmail();

		var processor = new CustomerServiceEmailProcessorForTestWithForcingFactorySave();
		Assert("Should be processed successfully", processor.CreateAndProcessMailItem(email, LoggerForTest));

		Assert(!incident.Request.INC_IsCustomerResolved);
	}
}

public class CustomerServiceEmailProcessorForTest : CustomerServiceEmailProcessor
{
	public bool NeedToSkipEmailSourceVerification = true;

	protected override bool ValidateEmail(MailItem mailItem, SupportIncident incident, IEmailProcessorLogger logger)
	{
		if (NeedToSkipEmailSourceVerification)
		{
			return true;
		}
		return base.ValidateEmail(mailItem, incident, logger);
	}
}

