using System;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class EndorsementWrapper : IEndorsement
	{
		EndorsementWrapper(EnRouteIncident incident)
		{
			this.incident = Argument.NotNull(incident, nameof(incident));
		}
		readonly EnRouteIncident incident;

		public static EndorsementWrapper New(EnRouteIncident incident) => incident == null ? null : new EndorsementWrapper(incident);

		public DateTime Date => incident.BN_EndorsementDate.ToDateTime();

		public string Authority => authority ?? (authority = incident.BN_EndorsementAuthority);
		string authority;

		public string Place => place ?? (place = incident.BN_EndorsementPlace);
		string place;

		public string Country => country ?? (country = incident.BN_EndorsementCountryCode);
		string country;
	}
}
