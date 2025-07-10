using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class TransportEquipmentDataProvider : ITransportEquipment
{
	public static IEnumerable<TransportEquipmentDataProvider> NewCollection(INctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, EU.NCTS.Business.NctsHeader> headerContainers)
	{
		return headerContainers?.Select((headerContainer, index) => new TransportEquipmentDataProvider(headerContainer, index + 1));
	}

	protected TransportEquipmentDataProvider(AutoCusInBondContainer container, int sequenceNumber)
	{
		this.container = container;
		this.headerContainer = container as NctsDepartureHeaderContainer;
		SequenceNumber = sequenceNumber;
	}
	protected readonly AutoCusInBondContainer container;
	protected readonly NctsDepartureHeaderContainer headerContainer;

	public int SequenceNumber { get; }

	public string ContainerIdentificationNumber => container.BC_Mode == Constants.ContainerModes.Containerised ? container.BC_ContainerNum.ReturnNullIfEmpty() : null;

	public int? NumberOfSeals => GetNumberOfSealsCore();

	public IReadOnlyCollection<ISeal> Seals => seals ?? (seals = GetSealDataProviders()?.ToArray());
	IReadOnlyCollection<ISeal> seals;

	protected virtual IEnumerable<ISeal> GetSealDataProviders() => SealDataProvider.NewCollection(headerContainer);

	protected virtual int? GetNumberOfSealsCore() => headerContainer?.TotalSealCount ?? 0;

	public IReadOnlyCollection<IGoodsReference> GoodsReferences => goodsReferences ?? (goodsReferences = GoodsReferenceDataProvider.NewCollection(headerContainer)?.ToArray());
	IReadOnlyCollection<IGoodsReference> goodsReferences;
}
