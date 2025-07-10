using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;
using NctsPackage = Enterprise.Customs.FR.Business.NCTS.NctsPackage;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5;

public class CC044CTransportEquipmentWrapper : TransportEquipmentWrapper
{
	CC044CTransportEquipmentWrapper(NctsArrivalHeaderContainer container, ICollection<string> goodsItemsNumbers) : base(container, goodsItemsNumbers)
	{
		this.container = Argument.NotNull(container, nameof(container));
	}
	protected new readonly NctsArrivalHeaderContainer container;

	public static CC044CTransportEquipmentWrapper New(NctsArrivalHeaderContainer container, ICollection<string> goodsItemsNumbers) => new CC044CTransportEquipmentWrapper(container, goodsItemsNumbers);

	protected override ICollection<IGoodsReference> GetGoodsReferenceCollection()
	{
		if (goodsReferences == null)
		{
			var goodsItemsNumberInt = new List<int>();
			goodsItemsNumbers.ForEach(x => goodsItemsNumberInt.Add(int.Parse(x)));

			goodsReferences = new List<IGoodsReference>();
			var containerPK = container.PK;
			foreach (var bill in container.NctsArrival.Bills)
			{
				foreach (var goodsItem in bill.ArrivalGoodsItems.Where(x => goodsItemsNumberInt.Contains(x.BY_DeclarationGoodsItemNumber)))
				{
					if (goodsItem.BY_UnloadedState.In(new ZString[] { NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.DIF }) &&
						goodsItem.Packages.Cast<NctsPackage>().Any(
							package => package.ContainersPivot.Cast<GenPivot>().Any(
								p => p.XX_Relation2ID == containerPK)))
					{
						goodsReferences.Add(new GoodsReferenceWrapper(goodsItem.BY_DeclarationGoodsItemNumber.ToString()));
					}
				}
			}
		}
		return goodsReferences;
	}
	List<IGoodsReference> goodsReferences;

	public override string ContainerIdentificationNumber => containerIdentificationNumber ?? (containerIdentificationNumber = (ContainerStatusIsNew || ContainerStatusIsDiff) ? container.BC_ContainerNum : null);
	string containerIdentificationNumber;

	protected override ICollection<ISeal> GetSealCollection()
	{
		var list = new List<SealWrapper>();

		if (container is NctsArrivalHeaderContainer arrivalHeaderContainer)
		{
			list.AddRange(arrivalHeaderContainer.Seals.Select(s => SealWrapper.New(s.BK_SealNumber, s.BK_UnloadingState)));
		}

		return new Collection<ISeal>(list.ToArray());
	}

	bool ContainerStatusIsNew => container.BC_UnloadedState == EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
	bool ContainerStatusIsDiff => container.BC_UnloadedState == EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
}
