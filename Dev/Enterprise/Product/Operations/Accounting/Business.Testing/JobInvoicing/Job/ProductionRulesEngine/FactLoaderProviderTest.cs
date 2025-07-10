using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	public class FactLoaderProviderTest : TestCaseWithFactory
	{
		public void TestGetFactLoader()
		{
			var contextType = RulesContextType.JobBillingTaxBranchDefaulting;
			var factLoaderProvider = new FactLoaderProvider();
			var factLoader = factLoaderProvider.GetFactLoader(JobInvoicingConsumerTypes.ShipmentCode, contextType);
			AssertNotNull(factLoader);
			AssertType<ShipmentJobFactLoader>(factLoader);

			factLoader = null;

			factLoader = factLoaderProvider.GetFactLoader(JobInvoicingConsumerTypes.GatewayConsolCode, contextType);
			AssertNotNull(factLoader);
			AssertType<ConsolJobFactLoader>(factLoader);

			factLoader = null;

			factLoader = factLoaderProvider.GetFactLoader(JobInvoicingConsumerTypes.BrokerageCode, contextType);
			AssertNotNull(factLoader);
			AssertType<DeclarationJobFactLoader>(factLoader);

			factLoader = null;

			factLoader = factLoaderProvider.GetFactLoader(JobInvoicingConsumerTypes.WarehouseInwardsCode, contextType);
			AssertNotNull(factLoader);
			AssertType<WarehouseJobFactLoader>(factLoader);

			factLoader = null;

			factLoader = factLoaderProvider.GetFactLoader(JobInvoicingConsumerTypes.WarehouseOutwardsCode, contextType);
			AssertNotNull(factLoader);
			AssertType<WarehouseJobFactLoader>(factLoader);

			factLoader = null;

			factLoader = factLoaderProvider.GetFactLoader(JobInvoicingConsumerTypes.WarehouseStorageCode, contextType);
			AssertNotNull(factLoader);
			AssertType<WarehouseJobFactLoader>(factLoader);

			factLoader = null;

			factLoader = factLoaderProvider.GetFactLoader(JobInvoicingConsumerTypes.WarehouseStocktakeCode, contextType);
			AssertNotNull(factLoader);
			AssertType<WarehouseJobFactLoader>(factLoader);

			factLoader = null;

			factLoader = factLoaderProvider.GetFactLoader(JobInvoicingConsumerTypes.WarehouseAdHocServiceJobCode, contextType);
			AssertNotNull(factLoader);
			AssertType<WarehouseJobFactLoader>(factLoader);

			factLoader = null;

			factLoader = factLoaderProvider.GetFactLoader(JobInvoicingConsumerTypes.WorkItemCode, contextType);
			AssertNotNull(factLoader);
			AssertType<WorkItemJobFactLoader>(factLoader);

			factLoader = null;

			factLoader = factLoaderProvider.GetFactLoader(JobInvoicingConsumerTypes.TransportConsignmentCode, contextType);
			AssertNotNull(factLoader);
			AssertType<LandTransportJobFactLoader>(factLoader);

			factLoader = null;

			factLoader = factLoaderProvider.GetFactLoader(JobInvoicingConsumerTypes.TransitReceiveCode, contextType);
			AssertNotNull(factLoader);
			AssertType<TransitReceiveConsignmentJobFactLoader>(factLoader);

			factLoader = null;

			factLoader = factLoaderProvider.GetFactLoader(JobInvoicingConsumerTypes.TransitDispatchCode, contextType);
			AssertNotNull(factLoader);
			AssertType<TransitDispatchConsignmentJobFactLoader>(factLoader);

			factLoader = null;

			factLoader = factLoaderProvider.GetFactLoader(JobInvoicingConsumerTypes.TransitReceiveTransportationUnitCode, contextType);
			AssertNotNull(factLoader);
			AssertType<TransitReceiveTransportationUnitJobFactLoader>(factLoader);

			factLoader = null;

			factLoader = factLoaderProvider.GetFactLoader(JobInvoicingConsumerTypes.TransitDispatchTransportationUnitCode, contextType);
			AssertNotNull(factLoader);
			AssertType<TransitDispatchTransportationUnitJobFactLoader>(factLoader);
		}

		public void TestGetFactLoader_WorkItem_ReturnsNull()
		{
			var factLoaderProvider = new FactLoaderProvider();
			AssertNull(factLoaderProvider.GetFactLoader(JobInvoicingConsumerTypes.WorkItemCode, RulesContextType.JobBillingBranchDefaulting));
		}

		public void TestGetFactLoader_WithUnsupportedType_ReturnsNull()
		{
			var factLoaderProvider = new FactLoaderProvider();
			AssertNull(factLoaderProvider.GetFactLoader("XXX", RulesContextType.DummyForTesting));
		}
	}
}
