using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(PlaceOfLoadingDataProvider))]
sealed class PlaceOfLoadingDataProviderTest : BasePlaceOfLoadingOrUnloadingDataProviderTest<PlaceOfLoadingDataProvider>
{
	protected override PlaceOfLoadingDataProvider CreateDataProvider(NctsDepartureMovementHeader movementHeader) => PlaceOfLoadingDataProvider.New(movementHeader);

	protected override void SetPortCode(ZString portCode) => NctsHeader.MovementHeader.BM_PortOfPresentationCode = portCode;

	protected override void SetPortLocation(ZString portLocation) => NctsHeader.MovementHeader.BM_PlaceOfLoading = portLocation;
}
