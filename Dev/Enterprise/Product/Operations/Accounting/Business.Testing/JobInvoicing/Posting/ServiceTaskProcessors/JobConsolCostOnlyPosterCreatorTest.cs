using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class JobConsolCostOnlyPosterCreatorTest : TestCaseWithFactory
	{
		public void TestCreateConsolCostOnlyPoster()
		{
			AssertNull("JobConsolCostOnlyPosterCreator should not create a ConsolCostOnly Poster for a null provider", JobConsolCostOnlyPosterCreator.CreateConsolCostOnlyPoster(null));
			AssertNull("JobConsolCostOnlyPosterCreator should not create a ConsolCostOnly Poster if the provider is not an IJobInvoicingPlugIn", JobConsolCostOnlyPosterCreator.CreateConsolCostOnlyPoster(new DummyIWorkflowProvider()));
			AssertNull("JobConsolCostOnlyPosterCreator should not create a ConsolCostOnly Poster for an shipment", JobConsolCostOnlyPosterCreator.CreateConsolCostOnlyPoster(Factory.New<ForwardingShipment>()));
			AssertNotNull("JobConsolCostOnlyPosterCreator should create a ConsolCostOnly Poster for a forwarding consol", JobConsolCostOnlyPosterCreator.CreateConsolCostOnlyPoster(Factory.New<ForwardingConsol>()));
		}

		#region Implementation

		JobConsolCostOnlyPosterCreator JobConsolCostOnlyPosterCreator
		{
			get { return new JobConsolCostOnlyPosterCreator(); }
		}

		#endregion
	}
}