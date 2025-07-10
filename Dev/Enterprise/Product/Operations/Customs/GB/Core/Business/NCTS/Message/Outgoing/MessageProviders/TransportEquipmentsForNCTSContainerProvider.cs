using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class TransportEquipmentsForNCTSContainerProvider : ITransportEquipment
	{
		public TransportEquipmentsForNCTSContainerProvider(NctsContainer nctsContainer, int sequenceNumber)
		{
			this.container = Argument.NotNull(nctsContainer, nameof(nctsContainer));
			SequenceNumber = sequenceNumber;
		}

		public int SequenceNumber { get; }

		public string ContainerIdentificationNumber => container.ContainerNumber;

		public int NumberOfSeals => Seals.Count;

		public IReadOnlyCollection<ISeal> Seals => seals ?? (seals = GetSeals().ToArray());
		ISeal[] seals;

		IEnumerable<ISeal> GetSeals()
		{
			var idx = 1;
			var seal1 = container.BC_Seal1;
			if (!seal1.IsEmpty)
			{
				yield return new SealsProvider(seal1, idx++);
			}

			var seal2 = container.BC_Seal2;
			if (!seal2.IsEmpty)
			{
				yield return new SealsProvider(seal2, idx++);
			}

			foreach (var additionalSealNumber in container.SealsForMessaging
				.Cast<CusSeal>()
				.OrderBy(s => s.BK_SequenceNumber)
				.Select(s => s.BK_SealNumber))
			{
				yield return new SealsProvider(additionalSealNumber, idx++);
			}
		}

		public IReadOnlyCollection<IGoodsReference> GoodsReferences => goodsReferences ?? (goodsReferences = container.ItemNumbers.GetElementsHaving(Constants.CusCodeDataTypes.ITEM)?.Select((rf, index) => new GoodsReferenceProvider(rf, index + 1)).ToList<IGoodsReference>());
		List<IGoodsReference> goodsReferences;

		readonly NctsContainer container;
	}
}
