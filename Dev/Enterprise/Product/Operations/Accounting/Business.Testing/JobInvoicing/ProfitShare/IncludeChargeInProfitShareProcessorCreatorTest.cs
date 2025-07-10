using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class IncludeChargeInProfitShareProcessorCreatorTest : TestCaseWithFactory
	{
		public void TestIncludeChargeInProfitShareProcessorCreator()
		{
			AssertNull("IncludeChargeInProfitShareProcessorCreator should not create a processor creator for a null provider", IncludeChargeInProfitShareProcessorCreator.CreateIncludeChargeInProfitShareProcessor(null));
			AssertNull("IncludeChargeInProfitShareProcessorCreator should not create a processor creator if the provider is not an IJobInvoicingPlugIn", IncludeChargeInProfitShareProcessorCreator.CreateIncludeChargeInProfitShareProcessor(new DummyIWorkflowProvider()));
			AssertNotNull("IncludeChargeInProfitShareProcessorCreator should create a processor creator for an forwarding shipment", IncludeChargeInProfitShareProcessorCreator.CreateIncludeChargeInProfitShareProcessor(Factory.New<ForwardingShipment>()));
			AssertNull("IncludeChargeInProfitShareProcessorCreator should not create a processor creator for a forwarding consol", IncludeChargeInProfitShareProcessorCreator.CreateIncludeChargeInProfitShareProcessor(Factory.New<ForwardingConsol>()));
		}

		#region Implementation

		IncludeChargeInProfitShareProcessorCreator IncludeChargeInProfitShareProcessorCreator
		{
			get { return new IncludeChargeInProfitShareProcessorCreator(); }
		}

		#endregion
	}
}