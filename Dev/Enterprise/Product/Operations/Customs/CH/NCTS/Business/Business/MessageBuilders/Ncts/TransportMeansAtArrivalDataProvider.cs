using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.NCTS.Business;

public class TransportMeansAtArrivalDataProvider : ITransportMeans
{
	public static TransportMeansAtArrivalDataProvider New(NctsArrivalMovementHeader nctsMovementHeader) => nctsMovementHeader == null ? null : new TransportMeansAtArrivalDataProvider(nctsMovementHeader);

	TransportMeansAtArrivalDataProvider(NctsArrivalMovementHeader nctsMovementHeader)
	{
		this.nctsMovementHeader = nctsMovementHeader;
	}
	readonly NctsArrivalMovementHeader nctsMovementHeader;

	public string Nationality => nctsMovementHeader.BM_RN_NKTransportAtArrivalIDNationality;

	public string IdentificationNumber => nctsMovementHeader.BM_TransportAtArrivalID;

	public string TypeOfIdentification => nctsMovementHeader.BM_TransportAtArrivalType;
}
