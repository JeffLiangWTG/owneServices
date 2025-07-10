using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalFormDataIncidentWrapper : IArrivalFormData
	{
		public ArrivalFormDataIncidentWrapper(EnRouteIncident incident)
		{
			this.incident = Argument.NotNull(incident, nameof(incident));
		}
		readonly EnRouteIncident incident;

		public ZDateTime FormDate => incident.BN_EndorsementDate;

		public ZString FormAuthority => incident.BN_EndorsementAuthority;

		public ZString FormAuthorityLanguage => ZString.Empty;

		public ZString FormLocation => incident.BN_EndorsementPlace;

		public ZString FormLocationLanguage => ZString.Empty;

		public ZString FormCountry => incident.BN_EndorsementCountryCode;

		public ZString FormText => incident.BN_Information;

		public ZString FormTextLanguage => ZString.Empty;
	}
}
