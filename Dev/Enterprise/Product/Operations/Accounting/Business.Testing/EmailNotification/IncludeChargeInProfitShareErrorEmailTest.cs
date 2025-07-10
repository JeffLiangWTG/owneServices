using CargoWise.ComponentModel;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	using System;
	using CargoWise.Types;
	using Enterprise.Environment;

	class IncludeChargeInProfitShareErrorEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		public void TestEmailSubjectAndBody()
		{
			var shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			NotificationCollection notifications = new NotificationCollection();
			notifications.AddError(@"This is a dummy error message. 
Please make sure you do the right thing.");
			var emailSender = new IncludeChargeInProfitShareErrorEmail(shipment, notifications);
			emailSender.Send();
			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Email subject", "Include Charge in Profit Share failed for Job S00010001", email.Subject);
			AssertEquals("Email body", @"<html>
<body>
<p>Include Charge in Profit share was run as a workflow actions and an attempt to run for Job S00010001 failed because of the following error(s):</p>
<ul><li>This is a dummy error message. </li><li>Please make sure you do the right thing.</li></ul>
</body>
</html>", email.Body);
		}

		public void TestInvalidArguments()
		{
			var shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			AssertNoExceptionThrown(() => new IncludeChargeInProfitShareErrorEmail(shipment, new NotificationCollection()));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new IncludeChargeInProfitShareErrorEmail(null, new NotificationCollection()));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new IncludeChargeInProfitShareErrorEmail(shipment, null));
		}

		protected override Type EmailDefType
		{
			get
			{
				return typeof(IncludeChargeInProfitShareErrorEmail);
			}
		}

		protected override void SetUp()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Now.AddDays(-1));
			var currentStaffMember = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaffMember.GS_EmailAddress = "somebody@somedomain.com";
			var group = Factory.New<GlbGroup>();
			group.Staff.Add(currentStaffMember);
			Factory.Save();
			AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
			}
		}

		TestObjectCreator fTestObjectCreator;
	}
}
