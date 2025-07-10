using System;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	class PeriodClosureEmailNotificationProcessorEmailTest : AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get { return typeof(PeriodClosureEmailNotificationProcessorEmail); }
		}

		public void TestEmailSubjectAndBody()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			var email = new PeriodClosureEmailNotificationProcessorEmail("TestSubject", "TestBody");
			email.Send();

			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var emailsCreated = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("TestSubject", emailsCreated.Subject);
			AssertEquals("TestBody", emailsCreated.Body);
			AssertEquals(2, emailsCreated.Recipients.Count);
		}

		protected override void SetUp()
		{
			var admin1 = Factory.NewWithValidTestData<GlbStaff>();
			admin1.GS_EmailAddress = "admin1@cargowise.com";
			var admin2 = Factory.NewWithValidTestData<GlbStaff>();
			admin2.GS_EmailAddress = "admin2@cargowise.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(admin1);
			group.Staff.Add(admin2);
			AccountingConfigurationRegistry.Instance.AutoPeriodClosureNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			Factory.Save();
		}
	}
}
