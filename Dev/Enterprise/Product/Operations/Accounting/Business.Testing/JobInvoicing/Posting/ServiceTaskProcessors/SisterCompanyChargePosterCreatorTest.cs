using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class SisterCompanyChargePosterCreatorTest : TestCaseWithFactory
	{
		public void TestSisterCompanyChargePosterCreator()
		{
			AssertNull("JobRevenuePosterCreator should not create a Revenue Poster for a null provider", sisterCompanyChargePosterCreator.CreateSisterCompanyChargePoster(null));
			AssertNull("JobRevenuePosterCreator should not create a Revenue Poster if the provider is not an IJobInvoicingPlugIn", sisterCompanyChargePosterCreator.CreateSisterCompanyChargePoster(new DummyIWorkflowProvider()));
			var poster = sisterCompanyChargePosterCreator.CreateSisterCompanyChargePoster(Factory.New<ForwardingShipment>()) as JobPostingWorkflowProcessor;
			AssertNotNull("JobRevenuePosterCreator should create a Revenue Poster for an IJobInvoicingPlugIn", poster);
			AssertNull("JobRevenuePosterCreator should not create a Revenue Poster for a forwarding consol", sisterCompanyChargePosterCreator.CreateSisterCompanyChargePoster(Factory.New<ForwardingConsol>()));
		}

		#region Implementation

		SisterCompanyChargePosterCreator sisterCompanyChargePosterCreator
		{
			get { return new SisterCompanyChargePosterCreator(); }
		}

		#endregion
	}
}
