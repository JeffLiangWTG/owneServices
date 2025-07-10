using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class TranshipmentProvider : ITranshipment
	{
		public TranshipmentProvider(EnRouteIncident incident)
		{
			this.incident = Argument.NotNull(incident, nameof(incident));
		}

		public bool ContainerIndicator => incident.IncidentContainers.Count > 0;

		public int TypeOfIdentification => int.TryParse(incident.BN_TransportAtDepartureType, out int value) ? value : 0;

		public string IdentificationNumber => incident.BN_TransportAtDepartureID;

		public string Nationality => incident.BN_RN_NKTransportAtDepartureIDNationality;

		readonly EnRouteIncident incident;
	}
}
