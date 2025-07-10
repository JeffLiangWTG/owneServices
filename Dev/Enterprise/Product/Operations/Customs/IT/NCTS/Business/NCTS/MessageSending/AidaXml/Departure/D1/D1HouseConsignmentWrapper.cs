using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.EU.NCTS.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class D1HouseConsignmentWrapper : ID1HouseConsignment
{
	public D1HouseConsignmentWrapper(NctsBill bill, IMessageSendingWrapperFactory messageSendingWrapperFactory)
	{
		Argument.NotNull(bill, nameof(bill));
		Argument.NotNull(messageSendingWrapperFactory, nameof(messageSendingWrapperFactory));
		InitializeLazy(bill, messageSendingWrapperFactory);
	}

	#region ID1HouseConsignment

	int ID1HouseConsignment.SequenceNumber => HouseConsignmentCustomsMessageWrapper.SequenceNumber;

	IReadOnlyCollection<IPreviousDocument> ID1HouseConsignment.PreviousDocuments => HouseConsignmentCustomsMessageWrapper.PreviousDocuments ?? Array.Empty<IPreviousDocument>();

	IReadOnlyCollection<ISupportingDocument> ID1HouseConsignment.SupportingDocuments => HouseConsignmentCustomsMessageWrapper.SupportingDocuments ?? Array.Empty<ISupportingDocument>();

	IReadOnlyCollection<IAdditionalReference> ID1HouseConsignment.AdditionalReferences => HouseConsignmentCustomsMessageWrapper.AdditionalReferences ?? Array.Empty<IAdditionalReference>();

	IReadOnlyCollection<ITransportDocument> ID1HouseConsignment.TransportDocuments => HouseConsignmentCustomsMessageWrapper.TransportDocuments ?? Array.Empty<ITransportDocument>();

	string ID1HouseConsignment.Ucr => lazyUcr.Value;
	Lazy<string> lazyUcr;

	IReadOnlyCollection<IAdditionalInformation> ID1HouseConsignment.AdditionalInformation => HouseConsignmentCustomsMessageWrapper.AdditionalInformation ?? Array.Empty<IAdditionalInformation>();

	ITrader ID1HouseConsignment.Consignor => HouseConsignmentCustomsMessageWrapper.Consignor;

	ITrader ID1HouseConsignment.Consignee => HouseConsignmentCustomsMessageWrapper.Consignee;

	IReadOnlyCollection<IAdditionalSupplyChainActor> ID1HouseConsignment.AdditionalSupplyChainActors => HouseConsignmentCustomsMessageWrapper.AdditionalSupplyChainActors ?? Array.Empty<IAdditionalSupplyChainActor>();

	string ID1HouseConsignment.TransportChargesMethodOfPayment => HouseConsignmentCustomsMessageWrapper.TransportChargesMethodOfPayment;

	string ID1HouseConsignment.CountryOfDispatch => lazyCountryOfDispatch.Value;
	Lazy<string> lazyCountryOfDispatch;

	string ID1HouseConsignment.CountryOfDestination => lazyCountryOfDestination.Value;
	Lazy<string> lazyCountryOfDestination;

	decimal ID1HouseConsignment.GrossMass => HouseConsignmentCustomsMessageWrapper.GrossMass;

	IReadOnlyCollection<IMeansOfTransport> ID1HouseConsignment.DepartureMeansOfTransports => HouseConsignmentCustomsMessageWrapper.DepartureMeansOfTransports ?? Array.Empty<IMeansOfTransport>();

	IReadOnlyCollection<ID1ConsignmentItem> ID1HouseConsignment.ConsignmentItems => lazyConsignmentItems.Value;
	Lazy<IReadOnlyCollection<ID1ConsignmentItem>> lazyConsignmentItems;

	#endregion

	void InitializeLazy(NctsBill bill, IMessageSendingWrapperFactory messageSendingWrapperFactory)
	{
		houseConsignmentCustomsMessageWrapper = new Lazy<IHouseConsignmentCustomsMessageWrapper>(() => messageSendingWrapperFactory.GetNewHouseConsignmentCustomsMessageWrapper(bill));
		lazyCountryOfDispatch = new Lazy<string>(() => GetCountryOfDispatch(bill));
		lazyCountryOfDestination = new Lazy<string>(() => GetCountryOfDestination(bill));
		lazyConsignmentItems = new Lazy<IReadOnlyCollection<ID1ConsignmentItem>>(() => GetConsignmentItems(bill.GoodsItems, messageSendingWrapperFactory));
		lazyUcr = new Lazy<string>(() => GetUcr(bill));
	}

	string GetCountryOfDispatch(NctsBill bill)
	{
		return SharedValueMapResolverProvider
			.GetCountryOfDispatchMapHeaderResolver()
			.GetValueForLine(bill);
	}

	string GetCountryOfDestination(NctsBill bill)
	{
		if (bill.Header.IsInPhase5TransitionPeriod)
		{
			return null;
		}
		return SharedValueMapResolverProvider.GetCountryOfDestinationMapHeaderResolver()
			.GetValueForLine(bill);
	}

	string GetUcr(NctsBill bill)
	{
		var header = bill.Header;
		if (header.IsInPhase5TransitionPeriod)
		{
			return HouseConsignmentCustomsMessageWrapper.Ucr;
		}
		return bill.ResolveUcr();
	}

	IReadOnlyCollection<ID1ConsignmentItem> GetConsignmentItems(INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> goodsItems, IMessageSendingWrapperFactory messageSendingWrapperFactory)
	{
		return goodsItems
			.Where(x => !x.IsCustomsStatusDeletionRequested && !x.IsCustomsStatusDeleted)
			.Select(x => new D1ConsignmentItemWrapper(x, messageSendingWrapperFactory))
			.ToCollection();
	}

	IHouseConsignmentCustomsMessageWrapper HouseConsignmentCustomsMessageWrapper => houseConsignmentCustomsMessageWrapper.Value;
	Lazy<IHouseConsignmentCustomsMessageWrapper> houseConsignmentCustomsMessageWrapper;
}
