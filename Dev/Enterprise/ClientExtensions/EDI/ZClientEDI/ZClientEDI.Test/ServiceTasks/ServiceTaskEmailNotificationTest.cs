using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.ServiceTasks.Testing
{
	public class ServiceTaskEmailNotificationTest : TestCaseWithFactory
	{
		public void TestLogErrorAndSendEmail()
		{
			var staffToNotification = Factory.NewWithValidTestData<GlbStaff>();
			staffToNotification.GS_EmailAddress = "test@123.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staffToNotification);
			Factory.Save();

			using (EDIDataRegistry.Instance.CertProcessingNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var log = new TestServiceLogger();
				ServiceTaskEmailNotification.SendEmailToGroup("Test Send Email", "Send Email Sucessful", EDIDataRegistry.Instance.CertProcessingNotificationGroup, log);

				AssertEquals(1, Environment.Env.OutgoingMailManager.EmailsCreated.Count);

				var email = Environment.Env.OutgoingMailManager.EmailsCreated.First();
				AssertEquals("Test Send Email", email.Subject);
				AssertStartsWith("Email body", "Send Email Sucessful", email.Body);
			}
		}

		public void TestLogErrorAndSendEmailIfNoStaffInGroup()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			Factory.Save();

			using (EDIDataRegistry.Instance.CertProcessingNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var log = new TestServiceLogger();
				ServiceTaskEmailNotification.SendEmailToGroup("Test Send Email Failed", "Send Email Failed", EDIDataRegistry.Instance.CertProcessingNotificationGroup, log);

				AssertEquals(0, Environment.Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals(1, log.Count);
				Assert(log[0].Contains("This email could not be delivered to the Post Masters Group as it is either empty or no members have an email address specified."));
			}
		}
	}
}
