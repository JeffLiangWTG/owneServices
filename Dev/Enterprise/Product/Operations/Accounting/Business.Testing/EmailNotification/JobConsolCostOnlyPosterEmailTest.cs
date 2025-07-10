using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	class JobConsolCostOnlyPosterEmailTest : AccountingEmailDefTest
	{
		public void TestEmailSubjectAndBody_WhenAllowZeroValueARInvoices()
		{
			AssertEmailSubjectAndBody(true);
		}

		public void TestEmailSubjectAndBody_WhenDisallowZeroValueARInvoices()
		{
			AssertEmailSubjectAndBody(false);
		}

		void AssertEmailSubjectAndBody(bool allowZeroValueARInvoices)
		{
			AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, allowZeroValueARInvoices);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var jobConsolCostOnlyPoster = new JobConsolCostOnlyPoster(consol);

			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => jobConsolCostOnlyPoster.Process(new NotificationBuffer()));
			AssertNotNull("Emails in exception", exceptionThrown.Emails);
			AssertEquals("Emails Count", 1, exceptionThrown.Emails.Length);
			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var expectedZeroValueMsg = allowZeroValueARInvoices
				? string.Empty
				: System.Environment.NewLine + "* Total of the charges being posted is zero and your system is configured to disallow zero value invoices.";

			var expectedMessage = $@"No appropriate charges were found for posting. This may be because:{expectedZeroValueMsg}
* All appropriate charges were to be appended to an existing transaction, and you elected to skip them.
* Charges pertaining to existing transactions contain differing AP details or currencies, and so cannot be appended.
* You are trying to post revenue but a shipment has invoicing on hold.
* You are trying to post revenue / cost but a shipment has Ready For Financial Closure status.
You may want to check the data you entered on Job Invoicing tabs on each shipment attached to this consol, and on the Costing tab of this form. Make sure that amounts are not zero and all appropriate information is entered for AP Invoices (if applicable).";

			var email = (AccountingEmailDef)exceptionThrown.Emails[0];
			email.Create(Factory);

			var expectedSubject = "Consol cost only posting errors for Consol C0001000";
			AssertEquals("Mail subject", expectedSubject, email.Subject);
			var expectedBody = @"The workflow trigger action 'Post Consol Cost Only' ran for Consol C0001000, but it failed due to the following errors:" + System.Environment.NewLine + expectedMessage;
			AssertEquals("Email body", expectedBody.Trim(), email.Body.Trim());
		}

		public void TestInvalidArguments()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new JobConsolCostOnlyPosterEmail(null, "some error"));
			AssertExceptionThrown(typeof(ArgumentException), () => new JobConsolCostOnlyPosterEmail(consol, string.Empty));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new JobConsolCostOnlyPosterEmail(consol, null));
		}

		protected override Type EmailDefType
		{
			get
			{
				return typeof(JobConsolCostOnlyPosterEmail);
			}
		}

		ForwardingConsol consol;
		ForwardingShipment shipment1, shipment2;
		Job job1, job2;
		JobConsolCost consolCost;
		protected override void SetUp()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Now.AddDays(-1));
			consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001001");
			consol.JK_UniqueConsignRef = "C0001000";
			shipment1 = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", consol);
			shipment2 = TestObjectCreator.CreateShipment("S00010002", "AUSYD", "NZAKL", consol);
			job1 = TestObjectCreator.CreateJob(shipment1);
			job2 = TestObjectCreator.CreateJob(shipment2);
			job1.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			job2.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			TestObjectCreator.CreateCharge(job1, TestObjectCreator.FRT, "Desc 1", TestObjectCreator.AUD, 50M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 50M, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateCharge(job2, TestObjectCreator.FRT, "Desc 2", TestObjectCreator.AUD, 50M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 50M, TestObjectCreator.ABIGAS);
			consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI);
			consolCost.E6_OSCostAmount = 100M;
			var currentStaffMember = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaffMember.GS_EmailAddress = "somebody@somedomain.com";
			var group = Factory.New<GlbGroup>();
			group.Staff.Add(currentStaffMember);
			AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			Factory.Save();
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
