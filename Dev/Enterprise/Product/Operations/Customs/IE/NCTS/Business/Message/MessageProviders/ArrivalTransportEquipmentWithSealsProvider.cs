using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class ArrivalTransportEquipmentWithSealsProvider : ITransportEquipmentWithSeals
	{
		public ArrivalTransportEquipmentWithSealsProvider(NctsArrivalHeaderContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
		}
		readonly NctsArrivalHeaderContainer container;

		public string ContainerIdentificationNumber => container.BC_ContainerNum;

		public int NumberOfSeals => Seals.Count;

		public IReadOnlyCollection<string> Seals => seals ?? (seals = container.SealsForMessaging
			.Where(x => MessageProviderHelper.StatusIsNewOrMissing(x.BK_UnloadingState))
			.Select(x => x.BK_SealNumber.ToString())
			.ToArray());
		string[] seals;

		public IReadOnlyCollection<string> GoodsReferences
		{
			get
			{
				if (goodsReferences == null)
				{
					goodsReferences = new List<string>();
					var containerPK = container.PK;
					foreach (var bill in container.NctsArrival.Bills)
					{
						foreach (var goodsItem in bill.ArrivalGoodsItems)
						{
							if (goodsItem.BY_UnloadedState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.DEC }) &&
								goodsItem.Packages.Cast<EU.NCTS.Business.NctsPackage>().Any(
									package => package.ContainersPivot.Cast<GenPivot>().Any(
										pivot => pivot.XX_Relation2ID == containerPK)))
							{
								goodsReferences.Add(goodsItem.BY_LineNo.ToString());
							}
						}
					}
				}
				return goodsReferences;
			}
		}

		public bool ContainerIsFull => false;

		List<string> goodsReferences;
	}
}
