using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class SisterCompanyChargePosterTest : JobPostingWorkflowProcessorSupportingARCreditNoteLevelAuthorizationTest
	{
		public void TestChargePostingWhenOrgIsDeactivated()
		{
			SetUpDataForTestChargePostingWhenOrgIsDeactivated();
			Assert(!Debtor.OH_IsActive);

			AssertEquals("Should not have sent email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => JobPostingProcessorWhenOrgIsDeactivated.Process(NotificationsWhenOrgIsDeactivated));
			AssertNotNull("Email in exception", exceptionThrown.Emails);

			//Test if the new case JobInvoicingPostingOption.AllSisterCompanyCharges in PostManagerValidation.IsSellEligibleToPost works.
			AssertContains("Debtor is inactive", NotificationsWhenOrgIsDeactivated.AsString.Trim());

			AccountingEmailDef mail = (AccountingEmailDef)exceptionThrown.Emails[0];
			mail.Create(Factory);
			AssertContains(@"Email body", "Debtor is inactive", mail.Body);
		}

		public void TestIsSellEligibleToPost_AllSisterCompanyCharges()
		{
			var validation = new PostManagerValidationForTest(null, JobInvoicingPostingOption.AllSisterCompanyCharges, null);
			AssertEquals("Should be eligible to post.", true, validation.IsSellEligibleToPost_Exposed(Charge, null));
		}

		protected override IProcessor CreateJobPostingProcessor(IJobInvoicingPlugIn plugin)
		{
			return new SisterCompanyChargePoster(plugin, GUIProvider, InvoiceDateOverride, PostDateOverride);
		}

		protected override void SetUp()
		{
			var orgHeader = TestObjectCreator.CreateOrgHeader("ORGAU1", false, true, "AUSYD");
			var companyAU = TestObjectCreator.CreateNewCompany("AU1", "AU");
			companyAU.GC_OH_OrgProxy = orgHeader.PK;

			Factory.Save();

			Shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			Job = TestObjectCreator.CreateJob(Shipment, false, false);

			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, 100M, orgHeader);

			Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			Factory.Save();

			base.SetUp();
		}
	}
}
