using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	using System;
	using CargoWise.Types;
	using Enterprise.Environment;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.ZArchitecture;

	class JobCostPosterEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		public void TestEmailSubjectAndBody()
		{
			job.JH_OA_LocalChargesAddr = ZGuid.Empty;
			Factory.Save();
			job.RunPreSaveValidation();
			AssertHasError("Precondition", job.JH_OA_LocalChargesAddrInfo, "Please enter Local Client or Overseas Agent.");
			var poster = new JobCostPoster(shipment);
			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => poster.Process(new NotificationBuffer()));
			AssertNotNull("Email in exception", exceptionThrown.Emails);
			AssertEquals("Emails Count", 1, exceptionThrown.Emails.Length);
			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AccountingEmailDef email = (AccountingEmailDef)exceptionThrown.Emails[0];
			email.Create(Factory);
			AssertEquals("Email subject", "Cost posting errors for S00010001", email.Subject);
			AssertEquals("Email body", @"Cost posting was run as a workflow action, and an attempt to post cost(s) for S00010001 failed because of the following errors:
You cannot post because job S00010001 has errors. Please fix errors before posting.
 - Overseas Agent: Please enter Local Client or Overseas Agent.
 - Local Client: Please enter Local Client or Overseas Agent.", email.Body);
			job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			job.Charges.RemoveAndDeleteAll();
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			poster = new JobCostPoster(shipment);
			exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => poster.Process(new NotificationBuffer()));
			AssertNotNull("Email in exception", exceptionThrown.Emails);
			AssertEquals("Emails Count", 1, exceptionThrown.Emails.Length);
			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			email = (AccountingEmailDef)exceptionThrown.Emails[0];
			email.Create(Factory);
			AssertEquals("Email subject", "Cost posting errors for S00010001", email.Subject);
			AssertEquals("Email body", @"Cost posting was run as a workflow action, and an attempt to post cost(s) for S00010001 failed because of the following errors:
Please enter charges before posting.", email.Body);
		}

		public void TestInvalidArguments()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new LocalSisterCompanyChargePosterEmail(null, "some error"));
			AssertExceptionThrown(typeof(ArgumentException), () => new LocalSisterCompanyChargePosterEmail(job, string.Empty));
			AssertExceptionThrown(typeof(ArgumentException), () => new LocalSisterCompanyChargePosterEmail(job, null));
		}

		protected override Type EmailDefType
		{
			get
			{
				return typeof(LocalSisterCompanyChargePosterEmail);
			}
		}

		ForwardingShipment shipment;
		Job job;
		protected override void SetUp()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Now.AddDays(-1));
			shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			job = TestObjectCreator.CreateJob(shipment, true, false);
			var jobCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			jobCharge.JR_APInvoiceNum = "AP001";
			jobCharge.JR_APInvoiceDate = ZDateTime.Today;
			jobCharge.JR_PaymentDate = ZDateTime.Today;
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
