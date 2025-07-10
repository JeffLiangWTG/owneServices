using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class FactLoaderProvider : IFactLoaderProvider
	{
		public IFactLoader GetFactLoader(string consumerType, RulesContextType rulesContextType)
		{
			IFactLoader loader = null;
			switch (consumerType)
			{
				case JobInvoicingConsumerTypes.ShipmentCode:
					loader = ObjectFactory.Get<IShipmentJobFactLoader>(nameof(IShipmentJobFactLoader), rulesContextType);
					break;
				case JobInvoicingConsumerTypes.GatewayConsolCode:
					loader = ObjectFactory.Get<IConsolJobFactLoader>(nameof(IConsolJobFactLoader), rulesContextType);
					break;
				case JobInvoicingConsumerTypes.BrokerageCode:
					loader = ObjectFactory.Get<IDeclarationJobFactLoader>(nameof(IDeclarationJobFactLoader), rulesContextType);
					break;
				case JobInvoicingConsumerTypes.QuotedBookingCode:
					loader = ObjectFactory.Get<IQuickBookingJobFactLoader>(nameof(IQuickBookingJobFactLoader), rulesContextType);
					break;
				case JobInvoicingConsumerTypes.WarehouseInwardsCode:
				case JobInvoicingConsumerTypes.WarehouseOutwardsCode:
				case JobInvoicingConsumerTypes.WarehouseStorageCode:
				case JobInvoicingConsumerTypes.WarehouseAdHocServiceJobCode:
				case JobInvoicingConsumerTypes.WarehouseStocktakeCode:
					loader = ObjectFactory.Get<IWarehouseJobFactLoader>(nameof(IWarehouseJobFactLoader), rulesContextType);
					break;
				case JobInvoicingConsumerTypes.WorkItemCode:
					if (rulesContextType == RulesContextType.JobBillingTaxBranchDefaulting)
					{
						loader = ObjectFactory.Get<IWorkItemJobFactLoader>(nameof(IWorkItemJobFactLoader), rulesContextType);
					}
					break;
				case JobInvoicingConsumerTypes.TransportConsignmentCode:
					loader = ObjectFactory.Get<ILandTransportJobFactLoader>(nameof(ILandTransportJobFactLoader), rulesContextType);
					break;
				case JobInvoicingConsumerTypes.TransitReceiveCode:
					loader = ObjectFactory.Get<ITransitReceiveConsignmentJobFactLoader>(nameof(ITransitReceiveConsignmentJobFactLoader), rulesContextType);
					break;
				case JobInvoicingConsumerTypes.TransitDispatchCode:
					loader = ObjectFactory.Get<ITransitDispatchConsignmentJobFactLoader>(nameof(ITransitDispatchConsignmentJobFactLoader), rulesContextType);
					break;
				case JobInvoicingConsumerTypes.TransitReceiveTransportationUnitCode:
					loader = ObjectFactory.Get<ITransitReceiveTransportationUnitJobFactLoader>(nameof(ITransitReceiveTransportationUnitJobFactLoader), rulesContextType);
					break;
				case JobInvoicingConsumerTypes.TransitDispatchTransportationUnitCode:
					loader = ObjectFactory.Get<ITransitDispatchTransportationUnitJobFactLoader>(nameof(ITransitDispatchTransportationUnitJobFactLoader), rulesContextType);
					break;
			}

			return loader;
		}
	}
}
