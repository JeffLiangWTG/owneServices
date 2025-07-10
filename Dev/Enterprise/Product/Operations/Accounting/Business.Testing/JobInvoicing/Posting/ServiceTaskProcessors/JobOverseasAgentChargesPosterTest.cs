using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class JobOverseasAgentChargesPosterTest : JobPostingWorkflowProcessorSupportingARCreditNoteLevelAuthorizationTest
	{
		protected override IProcessor CreateJobPostingProcessor(IJobInvoicingPlugIn plugin)
		{
			return new JobOverseasAgentChargesPoster(plugin, GUIProvider, InvoiceDateOverride, PostDateOverride);
		}

		protected override void SetUp()
		{
			Shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			Job = TestObjectCreator.CreateJob(Shipment, false, false);

			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, null, TestObjectCreator.AUD, 100M, TestObjectCreator.Agent);

			Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			Factory.Save();

			base.SetUp();
		}
	}
}
