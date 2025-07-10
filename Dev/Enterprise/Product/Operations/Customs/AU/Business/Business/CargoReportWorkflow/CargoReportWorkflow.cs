using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CargoReportWorkflow : Customs.Business.CargoReportWorkflow, Integration.Customs.AU.ICargoReportWorkflow
	{
		public CargoReportWorkflow(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZDateTime ScheduledCargoReportDate(ForwardingShipment shipment)
		{
			ZDateTime result = ZDateTime.Empty;

			try
			{
				result = CustomsSuppliedImpendingArrivalDate(shipment);

				if (result.IsEmpty)
				{
					result = shipment.FallbackPortOfFirstArrivalETA;
				}

				if (!result.IsEmpty)
				{
					ZInt lateTimeframe = 0;
					if (shipment.IsAir)
					{
						lateTimeframe = AUCustomsDataRegistry.Instance.AirMandatoryLatestCargoReportingTimeframe.Value;
					}
					else if (shipment.IsSea)
					{
						lateTimeframe = AUCustomsDataRegistry.Instance.SeaMandatoryLatestCargoReportingTimeframe.Value;
					}

					result = result.AddHours(-lateTimeframe - shipment.CargoReportAcceptedEventSafetyMargin);
				}
			}
			catch (SqlException)
			{
				result = ZDateTime.Empty;
			}

			return result;
		}

#if DEBUG
		protected virtual
#endif
 ZDateTime CustomsSuppliedImpendingArrivalDate(ForwardingShipment shipment)
		{
			ZDateTime result = ZDateTime.Empty;
			if (shipment.IsSea && shipment.ArrivalConsol != null && shipment.ArrivalConsol.Vessel != null)
			{
				ZQuery seaArrivalsQuery = new ZQuery(CMRSeaImpendingArrivalsSchema.SI_VesselID, shipment.ArrivalConsol.Vessel.RV_LloydsNumber);
				seaArrivalsQuery.AddToFilter(CMRSeaImpendingArrivalsSchema.SI_VoyageNumber, shipment.ArrivalConsol.JK_JX_JV_VoyageFlight);
				seaArrivalsQuery.AddToFilter(CMRSeaImpendingArrivalsSchema.SI_WithdrawnIndicator, SQLComparisonOperator.Equal, ZString.Empty);
				seaArrivalsQuery.OrderBy = CMRSeaImpendingArrivalsSchema.Constants.SI_OriginalMessageTimestamp + " desc";
				CMRSeaImpendingArrivals[] seaArrivals = shipment.Factory.Load<CMRSeaImpendingArrivals>(seaArrivalsQuery);

				CMRSeaImpendingArrivals seaArrival = null;
				if (seaArrivals.Length > 1)
				{
					for (int i = 0; i < seaArrivals.Length; i++)
					{
						if (seaArrivals[i].SI_OriginalFirstPortCode == shipment.FallbackPortOfFirstArrival)
						{
							seaArrival = seaArrivals[i];
							break;
						}
					}
				}
				if (seaArrival == null && seaArrivals.Length > 0)
				{
					seaArrival = seaArrivals[0];
				}

				if (seaArrival != null)
				{
					result = seaArrival.SI_OriginalETA;
				}
			}
			return result;
		}

		protected override bool ReportNoCustomsSuppliedImpendingArrivalDate(ForwardingShipment shipment)
		{
			return shipment.IsSea && CustomsSuppliedImpendingArrivalDate(shipment).IsEmpty;
		}

		protected override string CommentForNoCustomsSuppliedImpendingArrivalDate(ForwardingShipment shipment)
		{
			string result = "";
			if (shipment.IsSea && shipment.ArrivalConsol != null && shipment.ArrivalConsol.Vessel != null)
			{
				ZQuery seaArrivalsQuery = new ZQuery(CMRSeaImpendingArrivalsSchema.SI_VesselID, shipment.ArrivalConsol.Vessel.RV_LloydsNumber);
				seaArrivalsQuery.AddToFilter(CMRSeaImpendingArrivalsSchema.SI_WithdrawnIndicator, SQLComparisonOperator.Equal, ZString.Empty);
				seaArrivalsQuery.OrderBy = CMRSeaImpendingArrivalsSchema.Constants.SI_VoyageNumber + " asc," + CMRSeaImpendingArrivalsSchema.Constants.SI_OriginalFirstPortCode + " asc";
				foreach (CMRSeaImpendingArrivals impendingArrival in shipment.Factory.Load<CMRSeaImpendingArrivals>(seaArrivalsQuery))
				{
					result += impendingArrival.SI_VoyageNumber + "/" + impendingArrival.SI_OriginalFirstPortCode + "/" + impendingArrival.SI_OriginalETA.ToShortDateString() + " ";
				}
				if (!string.IsNullOrEmpty(result))
				{
					result = "Impending arrivals for this vessel: " + result;
				}
			}
			return result;
		}
	}
}
