using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class ArrivalTransportEquipmentProvider : ITransportEquipment
	{
		public ArrivalTransportEquipmentProvider(NctsArrivalHeaderContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
		}

		public int SequenceNumber => container.BC_SequenceNumber;

		public string ContainerIdentificationNumber => container.BC_ContainerNum;

		public int NumberOfSeals => Seals.Count;

		public IReadOnlyCollection<ISeal> Seals => seals ?? (seals = container.Seals
			.Where(x => x.BK_UnloadingState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS }))
			.Select(x => new SealsProvider(x.BK_SealNumber, x.BK_SequenceNumber))
			.OrderBy(x => x.SequenceNumber)
			.ToArray());
		ISeal[] seals;

		public IReadOnlyCollection<IGoodsReference> GoodsReferences
		{
			get
			{
				if (goodsReferences == null)
				{
					goodsReferences = new List<IGoodsReference>();
					var index = 1;
					var containerPK = container.PK;
					foreach (var bill in container.NctsArrival.Bills)
					{
						foreach (var goodsItem in bill.ArrivalGoodsItems)
						{
							if (goodsItem.BY_UnloadedState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.DEC }) &&
								goodsItem.Packages.Cast<NctsPackage>().Any(
									package => package.ContainersPivot.Cast<GenPivot>().Any(
										pivot => pivot.XX_Relation2ID == containerPK)))
							{
								goodsReferences.Add(new GoodsReferenceProvider(goodsItem, index++));
							}
						}
					}
				}
				return goodsReferences;
			}
		}
		List<IGoodsReference> goodsReferences;

		readonly NctsArrivalHeaderContainer container;
	}
}
