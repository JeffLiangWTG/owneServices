using System.Collections.Generic;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ShipmentJobFactLoader : BaseJobFactLoader, IShipmentJobFactLoader
	{
		public ShipmentJobFactLoader(RulesContextType rulesContextType)
			: base(rulesContextType)
		{
		}

		protected override IEnumerable<IInputFact> GetFactsCore(IJobInvoicingPlugIn parentPlugin, FactAccumulator factAccumulator)
		{
			var invoiceSupporter = parentPlugin.InvoicingSupporter;
			var origin = invoiceSupporter.Origin;
			var originFact = CreateOrNull(origin, factAccumulator.CreateUniqueUNLOCOFact);
			var destination = invoiceSupporter.Destination;
			var destinationFact = CreateOrNull(destination, factAccumulator.CreateUniqueUNLOCOFact);
			var consignee = invoiceSupporter.Consignee;
			var consigneeFact = CreateOrNull(consignee, factAccumulator.CreateUniqueOrganisationFact);
			var consignor = invoiceSupporter.Consignor;
			var consignorFact = CreateOrNull(consignor, factAccumulator.CreateUniqueOrganisationFact);
			var controllingAgent = invoiceSupporter.ControllingAgent;
			var controllingAgentFact = CreateOrNull(controllingAgent, factAccumulator.CreateUniqueOrganisationFact);
			var controllingCustomer = invoiceSupporter.ControllingCustomer;
			var controllingCustomerFact = CreateOrNull(controllingCustomer, factAccumulator.CreateUniqueOrganisationFact);
			var pickupAgent = invoiceSupporter.PickUpAgent;
			var pickupAgentFact = CreateOrNull(pickupAgent, factAccumulator.CreateUniqueOrganisationFact);
			var deliveryAgent = invoiceSupporter.DeliveryAgent;
			var deliveryAgentFact = CreateOrNull(deliveryAgent, factAccumulator.CreateUniqueOrganisationFact);

			IOrganisationWithMainAddressFact pickupLocalTransportFact = null;
			IOrganisationWithMainAddressFact deliveryLocalTransportFact = null;
			if (parentPlugin is ForwardingShipment shipment)
			{
				//ConsolJobFact arrivalConsolFact = null;
				//var arrivalConsol = shipment.ArrivalConsol;
				//if (arrivalConsol != null)
				//{
				//	arrivalConsolFact = new ConsolJobFact(arrivalConsol, environmentFact);
				//}
				//facts.Add(arrivalConsolFact);

				var pickupLocalTransport = shipment.DocsAndCartage?.PickupCartageCo;
				pickupLocalTransportFact = CreateOrNull(pickupLocalTransport, factAccumulator.CreateUniqueOrganisationFact);
				var deliveryLocalTransport = shipment.DocsAndCartage?.DeliveryCartageCo;
				deliveryLocalTransportFact = CreateOrNull(deliveryLocalTransport, factAccumulator.CreateUniqueOrganisationFact);
			}

			var jobFact = RulesContextType == RulesContextType.JobBillingTaxBranchDefaulting
				? new ShipmentJobForTaxBranchFact(parentPlugin,
					EnvironmentFact,
					jobBranchDepartmentFact: JobBranchDepartmentFact,
					localClientFact: LocalClientFact,
					salesRepFact: SalesRepFact,
					origin: origin,
					originFact: originFact,
					destination: destination,
					destinationFact: destinationFact,
					consigneeFact: consigneeFact,
					consignorFact: consignorFact,
					controllingAgent: controllingAgent,
					controllingAgentFact: controllingAgentFact,
					controllingCustomer: controllingCustomer,
					controllingCustomerFact: controllingCustomerFact,
					pickupAgent: pickupAgent,
					pickupAgentFact: pickupAgentFact,
					deliveryAgent: deliveryAgent,
					deliveryAgentFact: deliveryAgentFact,
					pickupLocalTransportFact: pickupLocalTransportFact,
					deliveryLocalTransportFact: deliveryLocalTransportFact,
					consolSendingAgentFact: GetTaxAgentFact(Core.Constants.ChargeCodeBranchDefaultingRule.SendingAgent),
					consolReceivingAgentFact: GetTaxAgentFact(Core.Constants.ChargeCodeBranchDefaultingRule.ReceivingAgent))
				: new ShipmentJobFact(parentPlugin, EnvironmentFact,
				localClientFact: LocalClientFact,
				salesRepFact: SalesRepFact,
				origin: origin,
				originFact: originFact,
				destination: destination,
				destinationFact: destinationFact,
				consigneeFact: consigneeFact,
				consignorFact: consignorFact,
				controllingAgent: controllingAgent,
				controllingAgentFact: controllingAgentFact,
				controllingCustomer: controllingCustomer,
				controllingCustomerFact: controllingCustomerFact,
				pickupAgent: pickupAgent,
				pickupAgentFact: pickupAgentFact,
				deliveryAgent: deliveryAgent,
				deliveryAgentFact: deliveryAgentFact,
				pickupLocalTransportFact: pickupLocalTransportFact,
				deliveryLocalTransportFact: deliveryLocalTransportFact);

			return new[] { jobFact };

			IOrganisationWithMainAddressFact GetTaxAgentFact(string defaultingRule)
			{
				var agent = invoiceSupporter.GetOrganisationByBranchDefaultingRule(defaultingRule);
				return CreateOrNull(agent, factAccumulator.CreateUniqueOrganisationFact);
			}
		}
	}
}
