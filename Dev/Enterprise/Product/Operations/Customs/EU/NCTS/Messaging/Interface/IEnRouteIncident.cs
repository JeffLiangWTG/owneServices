using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface IEnRouteIncident
	{
		ZBool IncidentFlag { get; }

		ZString EndorsementDate { get; }

		ZString EndorsementAuthority { get; }

		ZString EndorsementAuthorityLanguage { get; }

		ZString EndorsementPlace { get; }

		ZString EndorsementPlaceLanguage { get; }

		ZString EndorsementCountry { get; }

		ZString IncidentInformation { get; }

		ZString IncidentInformationLanguage { get; }
	}
}
