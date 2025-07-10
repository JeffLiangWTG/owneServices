using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.NCTS.Business;

public class ModeOfTransportDataProvider : IModeOfTransport
{
	public static ModeOfTransportDataProvider New(NctsDepartureMovementHeader nctsMovementHeader) => nctsMovementHeader == null ? null : new ModeOfTransportDataProvider(nctsMovementHeader);

	ModeOfTransportDataProvider(NctsDepartureMovementHeader nctsMovementHeader)
	{
		this.nctsMovementHeader = nctsMovementHeader;
	}
	readonly NctsDepartureMovementHeader nctsMovementHeader;

	public string InlandModeOfTransport => nctsMovementHeader.BM_InlandTransportMode;

	public ITransportMeans TransportMeans => transportMeans ?? (transportMeans = TransportMeansAtDepartureDataProvider.New(nctsMovementHeader));
	ITransportMeans transportMeans;
}
