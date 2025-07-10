using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	class ProfitShareChargeWorkflowProcessorCreatorTest : TestCaseWithFactory
	{
		public void TestProfitShareChargeWorkflowProcessorCreator()
		{
			AssertNull("ProfitShareChargeWorkflowProcessorCreator should not create a processor creator for a null provider", IncludeChargeInProfitShareProcessorCreator.CreateProfitShareChargeWorkflowProcessor(null));
			AssertNull("ProfitShareChargeWorkflowProcessorCreator should not create a processor creator if the provider is not an IJobInvoicingPlugIn", IncludeChargeInProfitShareProcessorCreator.CreateProfitShareChargeWorkflowProcessor(new DummyIWorkflowProvider()));
			AssertNotNull("ProfitShareChargeWorkflowProcessorCreator should create a processor creator for an forwarding shipment", IncludeChargeInProfitShareProcessorCreator.CreateProfitShareChargeWorkflowProcessor(Factory.New<ForwardingShipment>()));
			AssertNotNull("ProfitShareChargeWorkflowProcessorCreator should not create a processor creator for a forwarding consol", IncludeChargeInProfitShareProcessorCreator.CreateProfitShareChargeWorkflowProcessor(Factory.New<ForwardingConsol>()));
		}

		#region Implementation

		ProfitShareChargeWorkflowProcessorCreator IncludeChargeInProfitShareProcessorCreator
		{
			get { return new ProfitShareChargeWorkflowProcessorCreator(); }
		}

		#endregion
	}
}