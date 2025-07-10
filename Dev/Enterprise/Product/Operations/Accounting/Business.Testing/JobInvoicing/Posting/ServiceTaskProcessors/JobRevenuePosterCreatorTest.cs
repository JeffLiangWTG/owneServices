using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class JobRevenuePosterCreatorTest : TestCaseWithFactory
	{
		public void TestCreateRevenuePoster()
		{
			AssertNull("JobRevenuePosterCreator should not create a Revenue Poster for a null provider", JobRevenuePosterCreator.CreateRevenuePoster(null));
			AssertNull("JobRevenuePosterCreator should not create a Revenue Poster if the provider is not an IJobInvoicingPlugIn", JobRevenuePosterCreator.CreateRevenuePoster(new DummyIWorkflowProvider()));
			var poster = JobRevenuePosterCreator.CreateRevenuePoster(Factory.New<ForwardingShipment>()) as JobPostingWorkflowProcessor;
			AssertNotNull("JobRevenuePosterCreator should create a Revenue Poster for an IJobInvoicingPlugIn", poster);
			AssertNull("JobRevenuePosterCreator should not create a Revenue Poster for a forwarding consol", JobRevenuePosterCreator.CreateRevenuePoster(Factory.New<ForwardingConsol>()));
		}

		#region Implementation

		JobRevenuePosterCreator JobRevenuePosterCreator
		{
			get { return new JobRevenuePosterCreator(); }
		}

		#endregion
	}
}
