using CargoWise.Types;

namespace Enterprise.Customs.CH.NCTS.Business;

public class PlaceOfUnloadingDataProvider : BasePlaceOfLoadingOrUnloadingDataProvider
{
	public static PlaceOfUnloadingDataProvider New(NctsDepartureMovementHeader movementHeader) => movementHeader == null || HasNoPlaceOfUnloading(movementHeader) ? null : new PlaceOfUnloadingDataProvider(movementHeader);

	static bool HasNoPlaceOfUnloading(NctsDepartureMovementHeader movementHeader) => movementHeader.BM_ForeignDestPortKCode.IsEmpty && movementHeader.BM_PlaceOfUnloading.IsEmpty;

	PlaceOfUnloadingDataProvider(NctsDepartureMovementHeader movementHeader) : base(movementHeader) { }

	protected override ZString PortCode => movementHeader.BM_ForeignDestPortKCode;

	protected override ZString PortLocation => movementHeader.BM_PlaceOfUnloading;
}
