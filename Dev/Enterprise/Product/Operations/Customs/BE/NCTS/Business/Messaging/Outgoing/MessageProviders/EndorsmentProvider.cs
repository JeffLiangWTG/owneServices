using System;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class EndorsmentProvider : IEndorsement
	{
		readonly EnRouteIncident incident;

		public EndorsmentProvider(EnRouteIncident incident)
		{
			this.incident = Argument.NotNull(incident, nameof(incident));
		}

		public DateTime Date => incident.BN_EndorsementDate.IsValid ? incident.BN_EndorsementDate.ToDateTime() : DateTime.MinValue;

		public string Authority => incident.BN_EndorsementAuthority;

		public string Place => incident.BN_EndorsementPlace;

		public string Country => incident.BN_EndorsementCountryCode;
	}
}
