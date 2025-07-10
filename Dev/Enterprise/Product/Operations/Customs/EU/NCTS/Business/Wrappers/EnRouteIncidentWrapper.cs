using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class EnRouteIncidentWrapper : IEnRouteIncident
	{
		public EnRouteIncidentWrapper(EnRouteIncident enRouteIncident)
		{
			this.enRouteIncident = Argument.NotNull(enRouteIncident, nameof(enRouteIncident));
		}

		public ZBool IncidentFlag => enRouteIncident.IsEventFlagYes;

		public ZString EndorsementDate => enRouteIncident.BN_EndorsementDate.GetLongDate();

		public ZString EndorsementAuthority => enRouteIncident.BN_EndorsementAuthority;

		public ZString EndorsementAuthorityLanguage => ZString.Empty;

		public ZString EndorsementPlace => enRouteIncident.BN_EndorsementPlace;

		public ZString EndorsementPlaceLanguage => ZString.Empty;

		public ZString EndorsementCountry => enRouteIncident.BN_EndorsementCountryCode;

		public ZString IncidentInformation => enRouteIncident.BN_Information;

		public ZString IncidentInformationLanguage => ZString.Empty;

		readonly EnRouteIncident enRouteIncident;
	}
}
