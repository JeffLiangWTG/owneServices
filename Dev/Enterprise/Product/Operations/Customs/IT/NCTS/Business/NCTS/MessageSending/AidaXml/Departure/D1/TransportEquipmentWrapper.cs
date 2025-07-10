using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class TransportEquipmentWrapper : ITransportEquipment
{
	public TransportEquipmentWrapper(NctsDepartureHeaderContainer headerContainer)
	{
		this.headerContainer = Argument.NotNull(headerContainer, nameof(headerContainer));
		nctsHeader = Argument.NotNull(headerContainer.Header, nameof(headerContainer.Header));

		lazyContainerID = new Lazy<string>(() => headerContainer.EffectiveContainerNumber);
		lazyNumberOfSeals = new Lazy<int>(() => headerContainer.TotalSealCount);
		lazyLinkedGoodsItemNumbers = new Lazy<IReadOnlyCollection<int>>(GetLinkedGoodsItemNumbers);
		lazySeals = new Lazy<IReadOnlyCollection<string>>(GetSeals);
	}

	string ITransportEquipment.ContainerID => lazyContainerID.Value;
	readonly Lazy<string> lazyContainerID;

	int ITransportEquipment.NumberOfSeals => lazyNumberOfSeals.Value;
	readonly Lazy<int> lazyNumberOfSeals;

	IReadOnlyCollection<int> ITransportEquipment.LinkedGoodsItemNumbers => lazyLinkedGoodsItemNumbers.Value;
	readonly Lazy<IReadOnlyCollection<int>> lazyLinkedGoodsItemNumbers;

	IReadOnlyCollection<string> ITransportEquipment.Seals => lazySeals.Value;
	readonly Lazy<IReadOnlyCollection<string>> lazySeals;

	IReadOnlyCollection<int> GetLinkedGoodsItemNumbers()
	{
		var headerContainerPK = headerContainer.PK;
		return nctsHeader
			.Bills
			.SelectMany(bill => bill.GoodsItems)
			.Where(goodsItem => goodsItem
				.Packages
				.Cast<NctsPackage>()
				.Any(package => package.ContainersPivot.Any(pivot => pivot.XX_Relation2ID == headerContainerPK)))
			.Select(goodsItem => (int)goodsItem.BY_DeclarationGoodsItemNumber)
			.ToHashSet();
	}

	List<string> GetSeals()
	{
		var sealList = new HashSet<string>();
		AppendSealList(sealList, headerContainer.Seal1, headerContainer.Seal2);
		AppendSealList(sealList, headerContainer.AdditionalSeals.Select(p => p.BK_SealNumber).ToArray());

		return sealList.ToList();
	}

	void AppendSealList(HashSet<string> list, params ZString[] seals)
	{
		foreach (var seal in seals)
		{
			if (!seal.IsEmpty)
			{
				list.Add(seal);
			}
		}
	}

	readonly NctsDepartureHeaderContainer headerContainer;
	readonly EU.NCTS.Business.NctsHeader nctsHeader;
}
