using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class JobConsolCostOnlyPosterTest : ConsolPostingWorkflowProcessorTest
	{
		public void TestConsolCostPostingWhenOrgIsDeactivated()
		{
			var creditor = TestObjectCreator.CreateOrgHeader("TESTORG", true, false);
			OrganisationActivator.ActivateOrDeactivate(new[] { creditor.PK }, false);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001002");
			consol.JK_UniqueConsignRef = "C0001001";
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;

			var shipment1 = TestObjectCreator.CreateShipment("S00010003", "AUSYD", "NZAKL", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1);
			job1.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.FRT, "Desc 1", TestObjectCreator.AUD, 50M, creditor, TestObjectCreator.AUD, 50M, TestObjectCreator.ABIGAS);

			var shipment2 = TestObjectCreator.CreateShipment("S00010004", "AUSYD", "NZAKL", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2);
			job2.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.FRT, "Desc 2", TestObjectCreator.AUD, 50M, creditor, TestObjectCreator.AUD, 50M, TestObjectCreator.ABIGAS);

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, creditor);
			consolCost.E6_OSCostAmount = 100M;
			consolCost.E6_InvoiceNum = "789123";
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			consolCost.E6_PaymentDate = ZDateTime.Today;

			var currentStaffMember = TestObjectCreator.CreateStaff("ABC");
			currentStaffMember.GS_EmailAddress = "testmail@wisetechglobal.com";
			var group = TestObjectCreator.CreateStaffGroup("STF");
			group.Staff.Add(currentStaffMember);
			AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			charge1.JR_APInvoiceNum = consolCost.E6_InvoiceNum;
			charge1.JR_APInvoiceDate = consolCost.E6_InvoiceDate;
			charge2.JR_APInvoiceNum = consolCost.E6_InvoiceNum;
			charge2.JR_APInvoiceDate = consolCost.E6_InvoiceDate;

			var consolPostingProcessor = CreateConsolPostingProcessor(consol);
			var notifications = new NotificationBuffer();

			Factory.Save();

			Assert(!creditor.OH_IsActive);

			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => consolPostingProcessor.Process(notifications));
			AssertNotNull("Emails in exception", exceptionThrown.Emails);

			//Test if the new case JobInvoicingPostingOption.ConsolCosts in PostManagerValidation.IsCostEligibleToPost works.
			AssertContains("Creditor is inactive", notifications.AsString.Trim());
			AssertEquals("Should not send email as factory save is not permitted", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var mail = (AccountingEmailDef)exceptionThrown.Emails[0];
			mail.Create(Factory);
			AssertContains(@"Should contain 'Creditor is inactive'", "Creditor is inactive", mail.Body);
		}

		public void TestIsCostEligibleToPost_ConsolCosts()
		{
			consolCost.E6_InvoiceNum = "123456";
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			consolCost.E6_PaymentDate = ZDateTime.Today;
			charge1.JR_APInvoiceNum = consolCost.E6_InvoiceNum;
			charge1.JR_APInvoiceDate = consolCost.E6_InvoiceDate;
			charge2.JR_APInvoiceNum = consolCost.E6_InvoiceNum;
			charge2.JR_APInvoiceDate = consolCost.E6_InvoiceDate;
			Factory.Save();

			var validation = new PostManagerValidationForTest(null, JobInvoicingPostingOption.ConsolCosts, null);
			AssertEquals("Should be eligible to post.", true, validation.IsCostEligibleToPost_Exposed(charge1, null));
		}

		protected override IProcessor CreateConsolPostingProcessor(ForwardingConsol consol)
		{
			return new JobConsolCostOnlyPoster(consol);
		}
	}
}
