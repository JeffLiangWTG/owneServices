using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class TransportEquipmentDataProvider : ITransportEquipment
{
	public static IEnumerable<ITransportEquipment> NewCollection(IEnumerable<BaseCusContainer> containers) => containers?.Select((c, index) => new TransportEquipmentDataProvider(c, index + 1));

	TransportEquipmentDataProvider(BaseCusContainer container, int sequenceNumber)
	{
		this.container = container;
		SequenceNumber = sequenceNumber;
	}
	readonly BaseCusContainer container;

	public int SequenceNumber { get; }

	public string ContainerIdentificationNumber => container.CO_ContainerNumber;

	public int? NumberOfSeals => Seals.Count;

	public IReadOnlyCollection<ISeal> Seals => seals ?? (seals = SealDataProvider.NewCollection(container.CO_Seal).ToArray());
	IReadOnlyCollection<ISeal> seals;

	public IReadOnlyCollection<IGoodsReference> GoodsReferences => goodsReferences ?? (goodsReferences = GoodsReferenceDataProvider.NewCollection(container).ToArray());
	IReadOnlyCollection<IGoodsReference> goodsReferences;
}
