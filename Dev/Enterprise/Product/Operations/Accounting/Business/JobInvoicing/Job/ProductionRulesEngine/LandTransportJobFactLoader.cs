using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.LandTransport;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using WTG.ProductionRules.Core;
using static Enterprise.Freight.Forwarding.Business.ForwardingShipment;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class LandTransportJobFactLoader : BaseJobFactLoader, ILandTransportJobFactLoader
	{
		public LandTransportJobFactLoader(RulesContextType rulesContextType)
			: base(rulesContextType)
		{
		}

		protected override IEnumerable<IInputFact> GetFactsCore(IJobInvoicingPlugIn parentPlugin, FactAccumulator factAccumulator)
		{
			LandTransportJobShipmentFact shipmentFact = null;
			LandTransportJobWarehouseFact warehouseFact = null;

			var parentOrder = LoadParentOrder(parentPlugin);

			if (parentOrder is IWhsOrder)
			{
				warehouseFact = new LandTransportJobWarehouseFact(parentPlugin, parentOrder);
			}
			else if (parentOrder is IForwardingShipment)
			{
				var shipmentInvoiceSupporter = new ForwardingShipmentInvoicingSupporter(parentOrder as ForwardingShipment);
				var origin = shipmentInvoiceSupporter.Origin;
				var originFact = CreateOrNull(origin, factAccumulator.CreateUniqueUNLOCOFact);
				var destination = shipmentInvoiceSupporter.Destination;
				var destinationFact = CreateOrNull(destination, factAccumulator.CreateUniqueUNLOCOFact);
				shipmentFact = new LandTransportJobShipmentFact(shipmentInvoiceSupporter, originFact, destinationFact);
			}

			var jobFact = new LandTransportJobFact(parentPlugin, EnvironmentFact, shipmentFact, warehouseFact, LocalClientFact, SalesRepFact);

			return new[] { jobFact };
		}

		BusinessObject LoadParentOrder(IJobInvoicingPlugIn jobInvoicingPlugIn)
		{
			if (jobInvoicingPlugIn == null || !(jobInvoicingPlugIn is IDtbConsignment))
			{
				return null;
			}

			var consignment = jobInvoicingPlugIn as IDtbConsignment;
			var factory = jobInvoicingPlugIn.Factory;
			var transportBooking = factory?.Load<IDtbBooking>(consignment.LTC_KM_Booking);

			if (transportBooking == null)
			{
				return null;
			}

			var bookingConsolidation = factory.Load<IDtbBookingConsolidation>(transportBooking.KM_KB_Booking);

			if (bookingConsolidation == null)
			{
				return null;
			}

			return factory.Load(bookingConsolidation.KB_ParentTableCode, bookingConsolidation.KB_ParentID);
		}
	}
}
