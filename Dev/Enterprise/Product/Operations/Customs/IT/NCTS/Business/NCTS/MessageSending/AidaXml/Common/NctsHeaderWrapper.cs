using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using Enterprise.Core;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class NctsHeaderWrapper : INctsHeaderWrapper
{
	public NctsHeaderWrapper(NctsHeader header)
	{
		this.header = Argument.NotNull(header, nameof(header));
		InitializeLazy();
	}

	readonly NctsHeader header;

	int INctsHeaderWrapper.GetBindingItinerary() => header.CountriesOfRouting.Any() ? 1 : 0;

	CarrierTypeDataProviderAbstractClass INctsHeaderWrapper.GetCarrier()
		=> CarrierWrapper.NewOrNullCarrierType(header.MovementHeader?.Carrier?.Organisation, header.Principal?.Organisation);

	int? INctsHeaderWrapper.GetContainerIndicator() => lazyContainerIndicator.Value;
	Lazy<int?> lazyContainerIndicator;

	IReadOnlyCollection<GoodsReferenceTypeDataProviderAbstractClass> INctsHeaderWrapper.GetGoodsReference(NctsDepartureHeaderContainer container)
	{
		return header.Bills
			.SelectMany(bill => bill.GoodsItems)
			.Where(goodsItem => goodsItem
				.Packages
				.Cast<NctsPackage>()
				.Any(package => package.ContainersPivot.Any(pivot => pivot.XX_Relation2ID == container.PK)))
			.Select(goodsItem => (int)goodsItem.BY_DeclarationGoodsItemNumber)
			.ToHashSet()
			.Select(n => new GoodReferenceType(n))
			.ToArray();
	}

	void InitializeLazy()
	{
		lazyContainerIndicator = new Lazy<int?>(GetContainerIndicatorCore);
		lazyConsignee = new Lazy<ITrader>(GetConsigneeCore);
	}

	int? GetContainerIndicatorCore()
	{
		var hasContainers = header.DepartureHeaderContainers
			.Any(hc => hc.BC_Mode == Constants.ContainerModes.Containerised);
		return hasContainers ? 1 : 0;
	}

	ITrader GetConsigneeCore()
	{
		var jobDocAddress = header.Consignee;
		var movementHeader = header.MovementHeader;

		if (header.IsInPhase5TransitionPeriod)
		{
			return !jobDocAddress.IsEmpty
				? new EoriOrTcuTraderWrapper(jobDocAddress)
				: null;
		}

		if (!movementHeader.IsSecurityTypeNONOrENT
			&& !header.GetC0009CountryCodes().Contains(movementHeader.BM_RL_NKDestinationPort)
			&& (header.Has30600AdditionalInformation || header.Bills.Any(x => x.Has30600AdditionalInformation)))
		{
			return null;
		}

		return SharedValueMapResolverProvider.GetConsigneeMapResolver()
			.GetValueForHeader(header);
	}

	ITrader INctsHeaderWrapper.GetConsignee() => lazyConsignee.Value;
	Lazy<ITrader> lazyConsignee;
}

sealed class GoodReferenceType : GoodsReferenceTypeDataProviderAbstractClass
{
	public GoodReferenceType(int number)
	{
		DeclarationGoodsItemNumber = number;
	}

	public override int SequenceNumber => default;

	public override int DeclarationGoodsItemNumber { get; }
}
