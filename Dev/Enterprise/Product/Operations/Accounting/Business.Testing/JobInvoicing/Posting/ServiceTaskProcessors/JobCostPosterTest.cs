using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class JobCostPosterTest : JobPostingWorkflowProcessorTest
	{
		public void TestNoErrorWhenJobIsReadyForFinancialClosure()
		{
			Job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;
			Factory.Save();
			AssertNoErrors(Job);

			Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false;
			Job.JH_Status = "JFC";
			Factory.Save();

			Notifications.Clear();
			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => CreateJobPostingProcessor(Shipment).Process(Notifications));
			AssertNotNull("Email in exception", exceptionThrown.Emails);
			AssertEquals("No Post Error", string.Empty, Notifications.AsString);
		}

		protected override string ExpectedMessageForNothingWasPosted => string.Empty;

		protected override IProcessor CreateJobPostingProcessor(IJobInvoicingPlugIn plugin)
		{
			return new JobCostPoster(plugin);
		}

		protected override void SetUp()
		{
			SetUpCore();

			base.SetUp();
		}

		protected virtual void SetUpCore()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Now.AddDays(-1));

			Shipment = TestObjectCreator.CreateShipment("S00010001", "AUSYD", "NZAKL", null);
			Job = TestObjectCreator.CreateJob(Shipment, false, false);

			Charge = TestObjectCreator.CreateCharge(Job, TestObjectCreator.CC1, "Desc 1", TestObjectCreator.AUD, 50M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			Charge.JR_APInvoiceNum = "AP001";
			Charge.JR_APInvoiceDate = ZDateTime.Today;
			Charge.JR_PaymentDate = ZDateTime.Today;

			Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			Factory.Save();
		}

		protected override void ToggleChargePostable(bool shouldBePostable)
		{
			if (shouldBePostable)
			{
				Charge.JR_APInvoiceNum = "AP001";
				Charge.JR_APInvoiceDate = ZDateTime.Today;
				Charge.JR_PaymentDate = ZDateTime.Today;
			}
			else
			{
				Charge.JR_APInvoiceNum = string.Empty;
				Charge.JR_APInvoiceDate = ZDateTime.Empty;
				Charge.JR_PaymentDate = ZDateTime.Empty;
			}
		}
	}
}
