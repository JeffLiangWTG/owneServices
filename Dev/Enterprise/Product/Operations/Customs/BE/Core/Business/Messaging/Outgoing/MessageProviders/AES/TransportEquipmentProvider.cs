using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class TransportEquipmentProvider : ITransportEquipment
{
	readonly CusContainer container;
	public TransportEquipmentProvider(CusContainer container, int sequence)
	{
		this.container = Argument.NotNull(container, nameof(container));
		SequenceNumber = sequence;
	}

	public int SequenceNumber { get; }

	public string ContainerIdentificationNumber => container.CO_ContainerNumber;

	public int NumberOfSeals => Seals.Count;

	public IReadOnlyCollection<ISeal> Seals => seals ?? (seals = CreateListOfSeals(AdditionalSeals, container.CO_Seal, container.CO_SecondSeal));
	IReadOnlyCollection<ISeal> seals;

	public IReadOnlyCollection<ISeal> AdditionalSeals => additionalSeals ?? (additionalSeals = container.AdditionalSeals.Where(s => !s.BK_SealNumber.IsEmpty).Select(s => new SealProvider(s)).ToArray());
	IReadOnlyCollection<ISeal> additionalSeals;

	static IReadOnlyCollection<ISeal> CreateListOfSeals(IEnumerable<ISeal> additionalSealCollection, ZString sealNumber, ZString secondSealNumber)
	{
		var allSeals = new List<ISeal>();
		if (!sealNumber.IsEmpty)
		{
			var sealProviderItemForSeal = new SealProvider(sealNumber, 1);
			allSeals.Add(sealProviderItemForSeal);
		}
		if (!secondSealNumber.IsEmpty)
		{
			var sealProviderItemForSecondSeal = new SealProvider(secondSealNumber, 2);
			allSeals.Add(sealProviderItemForSecondSeal);
		}

		allSeals.AddRange(additionalSealCollection);
		return allSeals;
	}

	public IReadOnlyCollection<IGoodsReference> GoodsReferences => goodsReferences ?? (goodsReferences = container.InvoiceLinePivotCollection.Cast<CusContainerInvoiceLinePivot>().Select((i, index) => new GoodsReferenceProvider(i, index + 1)).ToArray());
	IReadOnlyCollection<IGoodsReference> goodsReferences;
}
