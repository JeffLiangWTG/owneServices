using CargoWise.Types;

namespace Enterprise.Customs.CH.NCTS.Business;

public class PlaceOfLoadingDataProvider : BasePlaceOfLoadingOrUnloadingDataProvider
{
	public static PlaceOfLoadingDataProvider New(NctsDepartureMovementHeader movementHeader) => movementHeader == null || HasNoPlaceOfLoading(movementHeader) ? null : new PlaceOfLoadingDataProvider(movementHeader);

	static bool HasNoPlaceOfLoading(NctsDepartureMovementHeader movementHeader) => movementHeader.BM_PortOfPresentationCode.IsEmpty && movementHeader.BM_PlaceOfLoading.IsEmpty;

	PlaceOfLoadingDataProvider(NctsDepartureMovementHeader movementHeader) : base(movementHeader) { }

	protected override ZString PortCode => movementHeader.BM_PortOfPresentationCode;

	protected override ZString PortLocation => movementHeader.BM_PlaceOfLoading;
}
