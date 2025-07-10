using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSIncidentBaseProvider : INCTSIncidentBase
	{
		public static NCTSIncidentBaseProvider NewOrNull(EnRouteIncident incident) => incident == null ? null : new NCTSIncidentBaseProvider(incident);

		protected NCTSIncidentBaseProvider(EnRouteIncident incident)
		{
			this.incident = Argument.NotNull(incident, nameof(incident));
		}
		protected readonly EnRouteIncident incident;

		public string Code => incident.BN_IncidentCode.ValueOrNullIfEmpty();

		public string Information => incident.BN_Information.ValueOrNullIfEmpty();

		public INCTSEndorsement Endorsement => endorsement ?? (endorsement = NCTSEndorsementProvider.NewOrNull(incident));
		INCTSEndorsement endorsement;
	}
}
