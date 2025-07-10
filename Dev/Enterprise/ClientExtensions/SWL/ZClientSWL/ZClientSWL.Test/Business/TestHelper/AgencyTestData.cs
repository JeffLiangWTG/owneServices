using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Client.SWL.Business.Testing
{
	internal static class AgencyTestData
	{
		public const string Vessel1 = "MAJAPAHIT";
		public static JobVoyage CreateVoyage(BusinessObjectFactory factory, string vessel, string voyageNo)
		{
			JobVoyage voyage = factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel;
			voyage.JV_VoyageFlight = voyageNo;
			return voyage;
		}

		public static JobSailing FindOrCreateSailing(JobVoyage voyage, string load, string discharge)
		{
			bool requireGenerate = false;
			if (voyage.Origins.GetOriginFromLoading(load) == null)
			{
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = load;
				requireGenerate = true;
			}

			if (voyage.Destinations.GetDestinationFromDischarge(discharge) == null)
			{
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = discharge;
				requireGenerate = true;
			}

			if (requireGenerate)
			{
				voyage.GenerateSailings();
			}

			return voyage.Sailings.GetSailingFromLoadAndDischarge(load, discharge);
		}

		public static AgencyShipment NewAgencyShipment(BusinessObjectFactory factory, string uniqueConsignRef, string vessel, string voyageNo, string load, string discharge)
		{
			AgencyShipment shipment = factory.New<AgencyShipment>();
			shipment.JS_UniqueConsignRef = uniqueConsignRef;
			shipment.JS_JX = FindOrCreateSailing(CreateVoyage(factory, vessel, voyageNo), load, discharge).PK;
			return shipment;
		}
	}
}
