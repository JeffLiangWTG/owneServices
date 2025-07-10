using System;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMSEmailDefTest : BMSTestCaseWithFactory
	{
		public void TestSendEmail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "frodo@bagend.com";

			var bmsNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(bmsNotificationGroup);

			Factory.Save();

			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bmsNotificationGroup.PK.ToGuid());

			var email = new BMSEmailDef("Mai Email", "Mah Body");
			email.Send();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Mai Email", sentEmail.Subject);
			AssertEquals("Mah Body", sentEmail.Body);

			AssertEquals(1, sentEmail.Recipients.Count);
			AssertEquals("frodo@bagend.com", sentEmail.Recipients[0].Email);
		}

		public void TestSendEmail_ShouldReportSendingFailures()
		{
			var email = new BMSEmailDef("Mai Email", "Mah Body");
			email.Send();

			try
			{
				AssertEquals("Exception occurred when sending the following email:\r\n\r\nMai Email\r\nMah Body", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}
	}
}
