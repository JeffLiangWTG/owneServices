using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class EnRouteIncidentTransportMeansWrapper : ITransportMeans
	{
		EnRouteIncidentTransportMeansWrapper(EnRouteIncident enRouteIncident)
		{
			this.enRouteIncident = Argument.NotNull(enRouteIncident, nameof(enRouteIncident));
		}

		readonly EnRouteIncident enRouteIncident;

		public static EnRouteIncidentTransportMeansWrapper New(EnRouteIncident enRouteIncident) => enRouteIncident == null ? null : new EnRouteIncidentTransportMeansWrapper(enRouteIncident);

		public string TypeOfIdentification => typeOfIdentification ?? (typeOfIdentification = enRouteIncident.BN_TransportAtDepartureType);
		string typeOfIdentification;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = enRouteIncident.BN_TransportAtDepartureID);
		string identificationNumber;

		public string Nationality => nationality ?? (nationality = enRouteIncident.BN_RN_NKTransportAtDepartureIDNationality);
		string nationality;
	}
}
