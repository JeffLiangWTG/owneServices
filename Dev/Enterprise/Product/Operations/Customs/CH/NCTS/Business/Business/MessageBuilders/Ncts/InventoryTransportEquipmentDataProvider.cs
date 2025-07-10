using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class InventoryTransportEquipmentDataProvider : TransportEquipmentDataProvider
{
	public static IEnumerable<InventoryTransportEquipmentDataProvider> NewCollection(NctsArrivalHeaderContainerCollection headerContainers)
	{
		return headerContainers?.Cast<NctsArrivalHeaderContainer>()
			.Where(c => c.BC_UnloadedState.IsUnloadingStateNEWorMISorDIF() || c.Seals.Cast<CusSeal>().Any(s => s.BK_UnloadingState.IsUnloadingStateNEWorMISorDIF()))
			.Select(headerContainer => new InventoryTransportEquipmentDataProvider(headerContainer));
	}

	InventoryTransportEquipmentDataProvider(NctsArrivalHeaderContainer arrivalHeaderContainer) : base(arrivalHeaderContainer, arrivalHeaderContainer.BC_SequenceNumber)
	{
		this.arrivalHeaderContainer = arrivalHeaderContainer;
	}
	readonly NctsArrivalHeaderContainer arrivalHeaderContainer;

	protected override IEnumerable<ISeal> GetSealDataProviders() => InventorySealDataProvider.NewCollection(arrivalHeaderContainer.SealsForMessaging);

	protected override int? GetNumberOfSealsCore()
	{
		var seals = arrivalHeaderContainer.Seals.Cast<CusSeal>();
		return seals.Any(seal => seal.BK_UnloadingState.IsUnloadingStateNEWorMIS()) ? seals.Count(seal => seal.BK_UnloadingState.IsUnloadingStateDECorNEWorDIForDAM()) : null;
	}
}
