using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class JobInvoiceHeaderProcessorCreatorTest : TestCaseWithFactory
	{
		public void TestCreateRevenuePoster()
		{
			AssertNull("JobRevenuePosterCreator should not create a Revenue Poster for a null provider", JobInvoiceHeaderProcessorCreator.CreateJobInvoiceHeaderProcessor(null));
			AssertNull("JobRevenuePosterCreator should not create a Revenue Poster if the provider is not an IJobInvoicingPlugIn", JobInvoiceHeaderProcessorCreator.CreateJobInvoiceHeaderProcessor(new DummyIWorkflowProvider()));
			AssertNotNull("JobRevenuePosterCreator should create a Revenue Poster for an forwarding shipment", JobInvoiceHeaderProcessorCreator.CreateJobInvoiceHeaderProcessor(Factory.New<ForwardingShipment>()));
			AssertNull("JobRevenuePosterCreator should not create a Revenue Poster for a forwarding consol", JobInvoiceHeaderProcessorCreator.CreateJobInvoiceHeaderProcessor(Factory.New<ForwardingConsol>()));
		}

		#region Implementation

		JobInvoiceHeaderProcessorCreator JobInvoiceHeaderProcessorCreator
		{
			get { return new JobInvoiceHeaderProcessorCreator(); }
		}

		#endregion
	}
}