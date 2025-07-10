using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class TransportEquipmentsProvider : ITransportEquipment
	{
		readonly NctsDepartureHeaderContainer container;

		public TransportEquipmentsProvider(NctsDepartureHeaderContainer nctsContainer, int sequenceNumber)
		{
			this.container = Argument.NotNull(nctsContainer, nameof(nctsContainer));
			SequenceNumber = sequenceNumber;
		}

		public int SequenceNumber { get; }

		public string ContainerIdentificationNumber => container.BC_ContainerNum;

		public int NumberOfSeals => Seals.Count;

		public IReadOnlyCollection<ISeal> Seals => seals ?? (seals = GetSeals().ToArray());
		ISeal[] seals;

		IEnumerable<ISeal> GetSeals()
		{
			var seal1 = container.Seal1;
			if (!seal1.IsEmpty)
			{
				yield return new SealsProvider(seal1, 1);
			}
			var seal2 = container.Seal2;
			if (!seal2.IsEmpty)
			{
				yield return new SealsProvider(seal2, 2);
			}
		}

		public IReadOnlyCollection<IGoodsReference> GoodsReferences => goodsReferences ?? //(goodsReferences = container is NctsContainer nctsContainer ? nctsContainer.GoodsReferences.GetElementsHaving(Constants.CusCodeDataTypes.ITEM)?.Cast<GoodsReference>().Select((rf, index) => new GoodsReferenceProvider(rf, index + 1)).ToArray<IGoodsReference>() : Array.Empty<IGoodsReference>());
			(goodsReferences = Array.Empty<IGoodsReference>());
		IGoodsReference[] goodsReferences;
	}
}
