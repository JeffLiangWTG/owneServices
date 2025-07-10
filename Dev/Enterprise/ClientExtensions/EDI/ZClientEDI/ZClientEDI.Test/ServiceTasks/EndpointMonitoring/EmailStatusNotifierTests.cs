using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.ServiceTasks.Testing
{
	public class EmailStatusNotifierTests : TestCaseWithFactory
	{
		public void TestNotify_WhenNotificationGroupIsNotDefined_ShouldThrowException()
		{
			// Arrange.
			EDIDataRegistry.Instance.AvsMonitoringNotificationGroup = Guid.Empty;
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			var emailNotifier = new EmailStatusNotifier();
			// Act.
			var exception = AssertExceptionThrown<InvalidOperationException>(() => emailNotifier.Notify("[_MOCK_TITLE_]", "[_MOCK_MESSAGE_]", new Dictionary<string, string>()));
			// Assert.
			AssertEquals("Could not find a valid notification group.", exception.Message);
			AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestNotify_WhenNotificationGroupHasNoValidEmail_ShouldThrowException()
		{
			// Arrange.
			var notificationGroup = Factory.CreateNotificationGroup().AddStaff("STF", string.Empty);
			Factory.Save();
			EDIDataRegistry.Instance.AvsMonitoringNotificationGroup = notificationGroup.PK.ToGuid();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			var emailNotifier = new EmailStatusNotifier();
			// Act.
			var exception = AssertExceptionThrown<InvalidOperationException>(() => emailNotifier.Notify("[_MOCK_TITLE_]", "[_MOCK_MESSAGE_]", new Dictionary<string, string>()));
			// Assert.
			AssertEquals("Could not find any valid email from notification group.", exception.Message);
			AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestNotify_WhenFindingValidNotificationGroup_ShouldPutNotificationInOutgoingEmailManager()
		{
			// Arrange.
			var notificationGroup = Factory.CreateNotificationGroup().AddStaff("S01", "staff.01@mock.com").AddStaff("S02", "staff.02@mock.com");
			Factory.Save();
			EDIDataRegistry.Instance.AvsMonitoringNotificationGroup = notificationGroup.PK.ToGuid();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			var emailNotifier = new EmailStatusNotifier();
			var attachments = new Dictionary<string, string> { ["[_MOCK_ATTACHMENT_01_]"] = "[_MOCK_ATTACHMENT_DATA_01_]", ["[_MOCK_ATTACHMENT_02_]"] = "[_MOCK_ATTACHMENT_DATA_02_]" };
			// Act.
			emailNotifier.Notify("[_MOCK_TITLE_]", "[_MOCK_MESSAGE_]", attachments);
			// Assert.
			AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			var createdEmail = EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("[_MOCK_TITLE_]", createdEmail.Subject);
			AssertContains("[_MOCK_MESSAGE_]", createdEmail.Body);
			AssertArrayEqualsByElements(new[] { "staff.01@mock.com", "staff.02@mock.com" }, createdEmail.Recipients.Cast<RecipientDef>().Select(recipient => recipient.Email).ToArray());
			AssertEquals(2, createdEmail.Attachments.Count);
			foreach (var attachment in createdEmail.Attachments.Cast<AttachmentDef>())
			{
				AssertEquals(attachments[attachment.DisplayName], Encoding.UTF8.GetString(attachment.Data));
			}
		}

		public void TestNotify_WhenGettingNoAttachment_ShouldPutNotificationWithoutAnyAttachment()
		{
			// Arrange.
			var notificationGroup = Factory.CreateNotificationGroup().AddStaff("S01", "staff.01@mock.com").AddStaff("S02", "staff.02@mock.com");
			Factory.Save();
			EDIDataRegistry.Instance.AvsMonitoringNotificationGroup = notificationGroup.PK.ToGuid();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			var emailNotifier = new EmailStatusNotifier();
			// Act.
			emailNotifier.Notify("[_MOCK_TITLE_]", "[_MOCK_MESSAGE_]", null);
			// Assert.
			AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			var createdEmail = EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("[_MOCK_TITLE_]", createdEmail.Subject);
			AssertContains("[_MOCK_MESSAGE_]", createdEmail.Body);
			AssertArrayEqualsByElements(new[] { "staff.01@mock.com", "staff.02@mock.com" }, createdEmail.Recipients.Cast<RecipientDef>().Select(recipient => recipient.Email).ToArray());
			AssertEquals(0, createdEmail.Attachments.Count);
		}
	}
}
