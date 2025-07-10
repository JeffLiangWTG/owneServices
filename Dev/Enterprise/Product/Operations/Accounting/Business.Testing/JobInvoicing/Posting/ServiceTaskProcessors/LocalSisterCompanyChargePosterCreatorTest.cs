using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class LocalSisterCompanyChargePosterCreatorTest : TestCaseWithFactory
	{
		public void TestLocalSisterCompanyChargePosterCreator()
		{
			AssertNull("JobRevenuePosterCreator should not create a Revenue Poster for a null provider", localSisterCompanyChargePosterCreator.CreateLocalSisterCompanyChargePoster(null));
			AssertNull("JobRevenuePosterCreator should not create a Revenue Poster if the provider is not an IJobInvoicingPlugIn", localSisterCompanyChargePosterCreator.CreateLocalSisterCompanyChargePoster(new DummyIWorkflowProvider()));
			var poster = localSisterCompanyChargePosterCreator.CreateLocalSisterCompanyChargePoster(Factory.New<ForwardingShipment>()) as JobPostingWorkflowProcessor;
			AssertNotNull("JobRevenuePosterCreator should create a Revenue Poster for an IJobInvoicingPlugIn", poster);
			AssertNull("JobRevenuePosterCreator should not create a Revenue Poster for a forwarding consol", localSisterCompanyChargePosterCreator.CreateLocalSisterCompanyChargePoster(Factory.New<ForwardingConsol>()));
		}

		#region Implementation

		LocalSisterCompanyChargePosterCreator localSisterCompanyChargePosterCreator
		{
			get { return new LocalSisterCompanyChargePosterCreator(); }
		}

		#endregion
	}
}
