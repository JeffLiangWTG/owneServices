using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.eHubMessaging.Tests.Business
{
	public class eServicesHealthCheckHtmlEmailBuilderTest : TestCaseWithFactory
	{
		#region Testing Recipient Logic

		public void TestGetRightFallbackForeHubNotificationGroup()
		{
			TestHelpers.CreateStaff("STA", Factory);
			var staffPMG = TestHelpers.CreateStaff("PMG", Factory);
			var staffPMG1 = TestHelpers.CreateStaff("PM1", Factory);
			var groupPMG = Factory.Load<GlbGroup>(new ZQuery(ZArchitecture.Schema.GlbGroupSchema.GG_Code, "PMG")).FirstOrDefault();
			var groupPMG1 = TestHelpers.CreateGroup("PM1", Factory);

			TestHelpers.CreateGroupLink(groupPMG, staffPMG, Factory);
			TestHelpers.CreateGroupLink(groupPMG1, staffPMG1, Factory);

			eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupPMG1.PK.ToGuid());

			var interchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			interchange1.EI_InterchangeNum = "NUM1";
			var interchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			interchange2.EI_InterchangeNum = "NUM2";
			var listOfFailedInterchanges = new List<KeyValuePair<EDIInterchange, string>>();
			listOfFailedInterchanges.Add(new KeyValuePair<EDIInterchange, string>(interchange1, "error details 1"));
			listOfFailedInterchanges.Add(new KeyValuePair<EDIInterchange, string>(interchange2, "error details 2"));
			Env.OutgoingMailManager.CreateAndSave(new eServicesHealthCheckHtmlEmailBuilder(eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationGroup, GlbCompany.CurrentCompany.PK.ToGuid(), ServiceTaskNames.EHubOutboundMessages, "Health Check Test").GetResult());

			AssertEquals("1 messages sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			CombineAssertions(() =>
			{
				var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Created email should contain only 1 recipient", 1, createdEmail.Recipients.Count);
				AssertEquals("Created email should send only to PM1", "PM1@PM1.com", createdEmail.Recipients[0].Email);
			});
		}

		public void TestUseAllStaffAsRecipient()
		{
			TestHelpers.CreateStaff("STA", Factory);
			var emailBuilder = new eServicesHealthCheckHtmlEmailBuilder(eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationGroup, GlbCompany.CurrentCompany.PK.ToGuid(), ServiceTaskNames.EHubOutboundMessages, "Health Check Test");
			emailBuilder.SetBody("");
			Env.OutgoingMailManager.CreateAndSave(emailBuilder.GetResult());

			AssertEquals("1 messages sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			CombineAssertions(() =>
			{
				var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Attachments", 2, createdEmail.Attachments.Count);
				AssertEquals("Recipient count", 1, createdEmail.Recipients.Count);
				AssertEquals("Recipient", "STA@STA.com", createdEmail.Recipients[0].Email);
				AssertContains("This notification are sending to all the staff listed in the CargoWise System", createdEmail.Body);
			});
		}

		public void TestUseSystemNotificationGroupAsRecipient()
		{
			TestHelpers.CreateStaff("STA", Factory);
			var pmxstaff = TestHelpers.CreateStaff("PMX", Factory);
			var pmxgroup = TestHelpers.CreateGroup("PMX", Factory);
			TestHelpers.CreateGroupLink(pmxgroup, pmxstaff, Factory);
			DataRegistry.Instance.RawRegistry.NotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, pmxgroup.PK.ToGuid());

			var emailBuilder = new eServicesHealthCheckHtmlEmailBuilder(eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationGroup, GlbCompany.CurrentCompany.PK.ToGuid(), ServiceTaskNames.EHubOutboundMessages, "Health Check Test");
			emailBuilder.SetBody("");
			Env.OutgoingMailManager.CreateAndSave(emailBuilder.GetResult());

			AssertEquals("1 messages sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			CombineAssertions(() =>
			{
				var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Attachments", 2, createdEmail.Attachments.Count);
				AssertEquals("Recipient count", 1, createdEmail.Recipients.Count);
				AssertEquals("Recipient", "PMX@PMX.com", createdEmail.Recipients[0].Email);

				AssertContains("You are listed as a member of the System Notification Group", createdEmail.Body);
			});
		}

		public void TestUseeHubNotificationGroupForSystemAsRecipient()
		{
			TestHelpers.CreateStaff("STA", Factory);
			var pm0Staff = TestHelpers.CreateStaff("PM0", Factory);
			var pm0Group = TestHelpers.CreateGroup("PM0", Factory);
			TestHelpers.CreateGroupLink(pm0Group, pm0Staff, Factory);
			eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pm0Group.PK.ToGuid());
			var pm1Group = TestHelpers.CreateGroup("PM1", Factory);
			eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, pm1Group.PK.ToGuid());

			var emailBuilder = new eServicesHealthCheckHtmlEmailBuilder(eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationGroup, GlbCompany.CurrentCompany.PK.ToGuid(), ServiceTaskNames.EHubOutboundMessages, "Health Check Test");
			emailBuilder.SetBody("");
			Env.OutgoingMailManager.CreateAndSave(emailBuilder.GetResult());

			AssertEquals("1 messages sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			CombineAssertions(() =>
			{
				var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Attachments", 2, createdEmail.Attachments.Count);
				AssertEquals("Recipient count", 1, createdEmail.Recipients.Count);
				AssertEquals("Recipient", "PM0@PM0.com", createdEmail.Recipients[0].Email);

				AssertContains("You are listed as a member of the eHub Error Notification Group for this CargoWise System", createdEmail.Body);
			});
		}

		public void TestUseeHubNotificationGroupForCompanyAsRecipient()
		{
			TestHelpers.CreateStaff("STA", Factory);
			var pmxstaff = TestHelpers.CreateStaff("PMX", Factory);
			var pmxgroup = TestHelpers.CreateGroup("PMX", Factory);
			TestHelpers.CreateGroupLink(pmxgroup, pmxstaff, Factory);
			DataRegistry.Instance.RawRegistry.NotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, pmxgroup.PK.ToGuid());
			var pm0Staff = TestHelpers.CreateStaff("PM0", Factory);
			var pm0Group = TestHelpers.CreateGroup("PM0", Factory);
			TestHelpers.CreateGroupLink(pm0Group, pm0Staff, Factory);
			eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pm0Group.PK.ToGuid());
			var pm1Staff = TestHelpers.CreateStaff("PM1", Factory);
			var pm1Group = TestHelpers.CreateGroup("PM1", Factory);
			TestHelpers.CreateGroupLink(pm1Group, pm1Staff, Factory);
			eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, pm1Group.PK.ToGuid());

			var emailBuilder = new eServicesHealthCheckHtmlEmailBuilder(eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationGroup, GlbCompany.CurrentCompany.PK.ToGuid(), ServiceTaskNames.EHubOutboundMessages, "Health Check Test");
			emailBuilder.SetBody("");
			Env.OutgoingMailManager.CreateAndSave(emailBuilder.GetResult());

			AssertEquals("1 messages sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			CombineAssertions(() =>
			{
				var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Attachments", 2, createdEmail.Attachments.Count);
				AssertEquals("Recipient count", 1, createdEmail.Recipients.Count);
				AssertEquals("Recipient", "PM1@PM1.com", createdEmail.Recipients[0].Email);

				AssertContains("Body_1", "You are listed as a member of the eHub Error Notification Group for this company", createdEmail.Body);
			});
		}

		#endregion

		public void TestSetSubject()
		{
			TestHelpers.CreateStaff("STA", Factory);
			var emailBuilder = new eServicesHealthCheckHtmlEmailBuilder(eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationGroup, GlbCompany.CurrentCompany.PK.ToGuid(), ServiceTaskNames.EHubOutboundMessages, "Health Check Test");
			emailBuilder.SetSubject("Hello World");
			Env.OutgoingMailManager.CreateAndSave(emailBuilder.GetResult());

			AssertEquals("1 messages sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			CombineAssertions(() =>
			{
				var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Subject", "Hello World", createdEmail.Subject);
			});
		}

		public void TestSetBody()
		{
			TestHelpers.CreateStaff("STA", Factory);
			var emailBuilder = new eServicesHealthCheckHtmlEmailBuilder(eHubMessagingRegistry.Instance.eHubFailedEDIInterchangeNotificationGroup, GlbCompany.CurrentCompany.PK.ToGuid(), ServiceTaskNames.EHubOutboundMessages, "Test");
			emailBuilder.SetBody("Hello World");
			Env.OutgoingMailManager.CreateAndSave(emailBuilder.GetResult());

			AssertEquals("1 messages sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var createdEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("Body ErrorDescription", "<b>Test Health Check report for eHub Outbound Messages:</b></big><br/>Hello World<br/><br/><b>Notes</b><br/>You are receiving this Email because:<br/><ul><li>This notification are sending to all the staff listed in the CargoWise System</li></ul>If you are not supposed to receive this email, please contact your system administrator to change the corresponding notification group setting<br/>[System] > [Registry] > [eServices] > [eHub] > [Health Check] > [Failed EDI Interchange] > [Email Recipients]", createdEmail.Body);
		}
	}
}
