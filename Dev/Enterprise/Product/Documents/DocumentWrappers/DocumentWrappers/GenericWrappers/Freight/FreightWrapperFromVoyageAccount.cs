using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromVoyageAccount : FreightWrapper
	{
		public FreightWrapperFromVoyageAccount(VoyageAccount voyageAccountBO, BusinessObjectFactory factory)
			: base(voyageAccountBO, factory)
		{
			VoyageAccountBO = voyageAccountBO;
		}
		readonly VoyageAccount VoyageAccountBO;

		protected override ZString GetJobNumber()
		{
			return VoyageAccountBO.NA_JobNumber;
		}

		protected override PlaceAndDateWrapper GetOrigin()
		{
			if (ShipmentRoutes.Count > 0)
			{
				RouteWrapper firstLeg = ShipmentRoutes[0];
				return new PlaceAndDateWrapper(firstLeg.Origin.UNLOCO, firstLeg.EstimatedDeparture, firstLeg.ActualDeparture, Factory);
			}
			return null;
		}

		protected override PlaceAndDateWrapper GetDestination()
		{
			if (ShipmentRoutes.Count > 0)
			{
				RouteWrapper lastLeg = ShipmentRoutes[ShipmentRoutes.Count - 1];
				return new PlaceAndDateWrapper(lastLeg.Destination.UNLOCO, lastLeg.EstimatedArrival, lastLeg.ActualArrival, Factory);
			}
			return null;
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(VoyageAccountBO.Voyage, Factory);
		}

		protected override OrganisationWrapper GetPrincipal()
		{
			return new OrganisationWrapper(OrganisationUsageType.Principal, VoyageAccountBO.Principal, ContactType.ShippingLine, Factory);
		}
	}
}
