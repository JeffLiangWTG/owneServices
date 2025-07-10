using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class D1ConsignmentItemWrapper : ID1ConsignmentItem
{
	public D1ConsignmentItemWrapper(NctsDepartureCargoDesc goodsItem, IMessageSendingWrapperFactory messageSendingWrapperFactory)
	{
		this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));
		consignmentItemCustomsMessageWrapper = messageSendingWrapperFactory.GetNewConsignmentItemCustomsMessageWrapper(goodsItem);

		lazyCountryOfDispatch = new Lazy<string>(GetCountryOfDispatch);
		lazyCountryOfDestination = new Lazy<string>(GetCountryOfDestination);
		lazyUcr = new Lazy<string>(GetUcr);
	}

	#region ID1ConsignmentItem Members

	string ID1ConsignmentItem.DeclarationType => consignmentItemCustomsMessageWrapper.DeclarationType;

	int ID1ConsignmentItem.GoodsItemNumber => consignmentItemCustomsMessageWrapper.GoodsItemNumber;

	int ID1ConsignmentItem.DeclarationGoodsItemNumber => consignmentItemCustomsMessageWrapper.DeclarationGoodsItemNumber;

	IReadOnlyCollection<IConsignmentItemPreviousDocument> ID1ConsignmentItem.PreviousDocuments
		=> consignmentItemCustomsMessageWrapper.PreviousDocuments ?? Array.Empty<IConsignmentItemPreviousDocument>();

	IReadOnlyCollection<IAdditionalInformation> ID1ConsignmentItem.AdditionalInformation
		=> consignmentItemCustomsMessageWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	IReadOnlyCollection<ISupportingDocument> ID1ConsignmentItem.SupportingDocuments
		=> consignmentItemCustomsMessageWrapper.SupportingDocuments ?? Array.Empty<ISupportingDocument>();

	IReadOnlyCollection<ITransportDocument> ID1ConsignmentItem.TransportDocuments
		=> consignmentItemCustomsMessageWrapper.TransportDocuments ?? Array.Empty<ITransportDocument>();

	IReadOnlyCollection<IAdditionalReference> ID1ConsignmentItem.AdditionalReferences
		=> consignmentItemCustomsMessageWrapper.AdditionalReferences ?? Array.Empty<IAdditionalReference>();

	string ID1ConsignmentItem.Ucr => lazyUcr.Value;

	ITrader ID1ConsignmentItem.Consignee => consignmentItemCustomsMessageWrapper.Consignee;

	IReadOnlyCollection<IAdditionalSupplyChainActor> ID1ConsignmentItem.AdditionalSupplyChainActors
		=> consignmentItemCustomsMessageWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	string ID1ConsignmentItem.TransportChargesMethodOfPayment => consignmentItemCustomsMessageWrapper.TransportChargesMethodOfPayment;

	string ID1ConsignmentItem.CountryOfDispatch => lazyCountryOfDispatch.Value;

	string ID1ConsignmentItem.CountryOfDestination => lazyCountryOfDestination.Value;

	decimal? ID1ConsignmentItem.NetMass => consignmentItemCustomsMessageWrapper.NetMass;

	decimal? ID1ConsignmentItem.GrossMass => consignmentItemCustomsMessageWrapper.GrossMass;

	decimal? ID1ConsignmentItem.SupplementaryUnits => consignmentItemCustomsMessageWrapper.SupplementaryUnits;

	string ID1ConsignmentItem.DescriptionOfGoods => consignmentItemCustomsMessageWrapper.DescriptionOfGoods;

	IReadOnlyCollection<IPackage> ID1ConsignmentItem.Packages
		=> consignmentItemCustomsMessageWrapper.Packages ?? Array.Empty<IPackage>();

	string ID1ConsignmentItem.CusCode => consignmentItemCustomsMessageWrapper.CusCode;

	string ID1ConsignmentItem.HsTariffCode => consignmentItemCustomsMessageWrapper.HsTariffCode;

	string ID1ConsignmentItem.NcTariffCode => consignmentItemCustomsMessageWrapper.NcTariffCode;

	IReadOnlyCollection<string> ID1ConsignmentItem.DangerousGoodsCodes
		=> consignmentItemCustomsMessageWrapper.DangerousGoodsCodes ?? Array.Empty<string>();

	#endregion

	string GetCountryOfDispatch()
	{
		return SharedValueMapResolverProvider
			.GetCountryOfDispatchMapLineResolver()
			.GetValueForLine(goodsItem);
	}

	string GetCountryOfDestination()
	{
		if (goodsItem.Header.IsInPhase5TransitionPeriod)
		{
			return consignmentItemCustomsMessageWrapper.CountryOfDestination;
		}
		return SharedValueMapResolverProvider.GetCountryOfDestinationMapLineResolver()
			.GetValueForLine(goodsItem);
	}

	string GetUcr()
	{
		var header = goodsItem.Bill.Header;
		if (header.IsInPhase5TransitionPeriod)
		{
			return consignmentItemCustomsMessageWrapper.Ucr;
		}
		return goodsItem.ResolveUcr();
	}

	readonly IConsignmentItemCustomsMessageWrapper consignmentItemCustomsMessageWrapper;
	readonly NctsDepartureCargoDesc goodsItem;
	readonly Lazy<string> lazyCountryOfDispatch;
	readonly Lazy<string> lazyCountryOfDestination;
	readonly Lazy<string> lazyUcr;
}
