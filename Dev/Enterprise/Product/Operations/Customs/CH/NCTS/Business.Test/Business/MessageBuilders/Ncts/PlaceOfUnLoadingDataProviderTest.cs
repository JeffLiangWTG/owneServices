using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(PlaceOfUnloadingDataProvider))]
sealed class PlaceOfUnloadingDataProviderTest : BasePlaceOfLoadingOrUnloadingDataProviderTest<PlaceOfUnloadingDataProvider>
{
	protected override PlaceOfUnloadingDataProvider CreateDataProvider(NctsDepartureMovementHeader movementHeader) => PlaceOfUnloadingDataProvider.New(NctsHeader.MovementHeader);

	protected override void SetPortCode(ZString portCode) => NctsHeader.MovementHeader.BM_ForeignDestPortKCode = portCode;

	protected override void SetPortLocation(ZString portLocation) => NctsHeader.MovementHeader.BM_PlaceOfUnloading = portLocation;
}
