using System;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class EndorsmentProvider : IEndorsement
	{
		readonly EnRouteIncident incident;

		public EndorsmentProvider(EnRouteIncident incident)
		{
			this.incident = Argument.NotNull(incident, nameof(incident));
		}

		public DateTime Date => incident.BN_EndorsementDate.IsValid ? DataProviderHelper.GetProviderDateTime(incident.BN_EndorsementDate) : DateTime.MinValue;

		public string Authority => incident.BN_EndorsementAuthority;

		public string Place => incident.BN_EndorsementPlace;

		public string Country => incident.BN_EndorsementCountryCode;
	}
}
