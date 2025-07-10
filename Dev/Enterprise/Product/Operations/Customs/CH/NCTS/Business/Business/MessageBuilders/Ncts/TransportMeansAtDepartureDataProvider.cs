using System.Text.RegularExpressions;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class TransportMeansAtDepartureDataProvider : ITransportMeans
{
	public static TransportMeansAtDepartureDataProvider New(NctsDepartureMovementHeader nctsMovementHeader) => nctsMovementHeader == null ? null : new TransportMeansAtDepartureDataProvider(nctsMovementHeader);

	TransportMeansAtDepartureDataProvider(NctsDepartureMovementHeader nctsMovementHeader)
	{
		this.nctsMovementHeader = nctsMovementHeader;
	}
	readonly NctsDepartureMovementHeader nctsMovementHeader;

	public string Nationality => nctsMovementHeader.BM_RN_NKTransportAtDepartureCountry;

	public string IdentificationNumber => identificationNumber ?? (identificationNumber = GetIdentificationNumber());
	string identificationNumber;

	public string TypeOfIdentification => typeOfIdentification ?? (typeOfIdentification = GetTypeOfIdentification());
	string typeOfIdentification;

	string GetIdentificationNumber()
	{
		if (nctsMovementHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._4_AirTransport && TypeOfIdentification == NctsTransportTypeOfIdList.Codes._41)
		{
			return nctsMovementHeader.BM_AircraftIDAtDeparture;
		}

		return nctsMovementHeader.BM_TransportAtDeparture;
	}

	string GetTypeOfIdentification()
	{
		var inlandTransportMode = nctsMovementHeader.BM_InlandTransportMode;
		var isTransportAtDepartureEmpty = nctsMovementHeader.BM_TransportAtDeparture.IsEmpty;

		if (inlandTransportMode == ModeOfTransportList.Codes._2_RailTransport && !isTransportAtDepartureEmpty)
		{
			return NctsTransportTypeOfIdList.Codes._21;
		}

		if (inlandTransportMode == ModeOfTransportList.Codes._3_RoadTransport && !isTransportAtDepartureEmpty)
		{
			return NctsTransportTypeOfIdList.Codes._30;
		}

		if (inlandTransportMode == ModeOfTransportList.Codes._4_AirTransport)
		{
			if (!nctsMovementHeader.BM_AircraftIDAtDeparture.IsEmpty)
			{
				return NctsTransportTypeOfIdList.Codes._41;
			}
			else if (!isTransportAtDepartureEmpty)
			{
				return NctsTransportTypeOfIdList.Codes._40;
			}
		}

		if (inlandTransportMode == ModeOfTransportList.Codes._8_InlandWaterwayTransport)
		{
			return Regex.IsMatch(nctsMovementHeader.VesselNameAtDeparture, @"^\d{8}$") ? NctsTransportTypeOfIdList.Codes._80 : NctsTransportTypeOfIdList.Codes._81;
		}

		if (inlandTransportMode == ModeOfTransportList.Codes._9_OwnPropulsion)
		{
			return nctsMovementHeader.BM_TransportAtDepartureType;
		}

		return string.Empty;
	}
}
