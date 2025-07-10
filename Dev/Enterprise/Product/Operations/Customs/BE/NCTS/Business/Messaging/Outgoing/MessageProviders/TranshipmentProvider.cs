using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class TranshipmentProvider : ITranshipment
	{
		readonly EnRouteIncident incident;

		public TranshipmentProvider(EnRouteIncident incident)
		{
			this.incident = Argument.NotNull(incident, nameof(incident));
		}

		public bool ContainerIndicator => incident.IncidentContainers.Count > 0;

		public int TypeOfIdentification => int.TryParse(incident.BN_TransportAtDepartureType, out int value) ? value : 0;

		public string IdentificationNumber => incident.BN_TransportAtDepartureID;

		public string Nationality => incident.BN_RN_NKTransportAtDepartureIDNationality;
	}
}
