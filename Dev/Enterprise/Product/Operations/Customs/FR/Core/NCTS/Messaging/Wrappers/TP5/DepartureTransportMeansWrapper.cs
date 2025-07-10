using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class DepartureTransportMeansWrapper : IDepartureTransportMeans
	{
		DepartureTransportMeansWrapper(NctsDepartureMovementHeader movementHeader)
		{
			this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		}

		readonly NctsDepartureMovementHeader movementHeader;

		public static DepartureTransportMeansWrapper New(NctsDepartureMovementHeader movementHeader) => movementHeader == null ? null : new DepartureTransportMeansWrapper(movementHeader);

		public string TypeOfIdentification => typeOfIdentification ?? (typeOfIdentification = movementHeader.TransportTypeAtDeparture);
		string typeOfIdentification;

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = movementHeader.TransportAtDeparture);
		string identificationNumber;

		public string Nationality => nationality ?? (nationality = movementHeader.TransportCountryAtDeparture);
		string nationality;
	}
}
