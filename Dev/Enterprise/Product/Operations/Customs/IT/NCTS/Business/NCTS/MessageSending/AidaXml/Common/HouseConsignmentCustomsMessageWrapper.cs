using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.MasterFiles.Business;
using Argument = CargoWise.Common.Argument;
using EUAdditionalInfo = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo;
using ITShared = Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using NctsAdditionalInfoSubTypeCodes = Enterprise.Customs.EU.Business.AdditionalInfoSubTypeList.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class HouseConsignmentCustomsMessageWrapper : IHouseConsignmentCustomsMessageWrapper
{
	public HouseConsignmentCustomsMessageWrapper(NctsBill bill)
	{
		this.bill = Argument.NotNull(bill, nameof(bill));
		InitializeLazy(bill);
	}

	#region IHouseConsignmentCustomsMessageWrapper

	int IHouseConsignmentCustomsMessageWrapper.SequenceNumber => lazySequenceNumber.Value;
	Lazy<int> lazySequenceNumber;

	IReadOnlyCollection<IPreviousDocument> IHouseConsignmentCustomsMessageWrapper.PreviousDocuments => lazyPreviousDocuments.Value;
	Lazy<IReadOnlyCollection<IPreviousDocument>> lazyPreviousDocuments;

	IReadOnlyCollection<ISupportingDocument> IHouseConsignmentCustomsMessageWrapper.SupportingDocuments => lazySupportingDocuments.Value;
	Lazy<IReadOnlyCollection<ISupportingDocument>> lazySupportingDocuments;

	IReadOnlyCollection<IAdditionalReference> IHouseConsignmentCustomsMessageWrapper.AdditionalReferences => lazyAdditionalReferences.Value;
	Lazy<IReadOnlyCollection<IAdditionalReference>> lazyAdditionalReferences;

	IReadOnlyCollection<ITransportDocument> IHouseConsignmentCustomsMessageWrapper.TransportDocuments => lazyTransportDocuments.Value;
	Lazy<IReadOnlyCollection<ITransportDocument>> lazyTransportDocuments;

	string IHouseConsignmentCustomsMessageWrapper.Ucr => lazyUcr.Value;
	Lazy<string> lazyUcr;

	IReadOnlyCollection<IAdditionalInformation> IHouseConsignmentCustomsMessageWrapper.AdditionalInformation => lazyAdditionalInformation.Value;
	Lazy<IReadOnlyCollection<IAdditionalInformation>> lazyAdditionalInformation;

	ITrader IHouseConsignmentCustomsMessageWrapper.Consignor => lazyConsignor.Value;
	Lazy<ITrader> lazyConsignor;

	ITrader IHouseConsignmentCustomsMessageWrapper.Consignee => lazyConsignee.Value;
	Lazy<ITrader> lazyConsignee;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IHouseConsignmentCustomsMessageWrapper.AdditionalSupplyChainActors => lazyAdditionalSupplyChainActors.Value;
	Lazy<IReadOnlyCollection<IAdditionalSupplyChainActor>> lazyAdditionalSupplyChainActors;

	string IHouseConsignmentCustomsMessageWrapper.TransportChargesMethodOfPayment => lazyTransportChargesMethodOfPayment.Value;
	Lazy<string> lazyTransportChargesMethodOfPayment;

	string IHouseConsignmentCustomsMessageWrapper.CountryOfDispatch => lazyCountryOfDispatch.Value;
	Lazy<string> lazyCountryOfDispatch;

	decimal IHouseConsignmentCustomsMessageWrapper.GrossMass => lazyGrossMass.Value;
	Lazy<decimal> lazyGrossMass;

	IReadOnlyCollection<IMeansOfTransport> IHouseConsignmentCustomsMessageWrapper.DepartureMeansOfTransports => lazyDepartureMeansOfTransports.Value;
	Lazy<IReadOnlyCollection<IMeansOfTransport>> lazyDepartureMeansOfTransports;

	#endregion

	#region Implementation

	void InitializeLazy(NctsBill bill)
	{
		lazySequenceNumber = new Lazy<int>(() => bill.SequenceNumber);
		lazyPreviousDocuments = new Lazy<IReadOnlyCollection<IPreviousDocument>>(GetPreviousDocuments);
		lazySupportingDocuments = new Lazy<IReadOnlyCollection<ISupportingDocument>>(GetSupportingDocuments);
		lazyAdditionalReferences = new Lazy<IReadOnlyCollection<IAdditionalReference>>(GetAdditionalReferences);
		lazyTransportDocuments = new Lazy<IReadOnlyCollection<ITransportDocument>>(GetTransportDocuments);
		lazyUcr = new Lazy<string>(() => bill.B0_ReferenceID);
		lazyAdditionalInformation = new Lazy<IReadOnlyCollection<IAdditionalInformation>>(GetAdditionalInformation);
		lazyConsignor = new Lazy<ITrader>(() => GetTrader(bill.Consignor));
		lazyConsignee = new Lazy<ITrader>(GetConsignee);
		lazyAdditionalSupplyChainActors = new Lazy<IReadOnlyCollection<IAdditionalSupplyChainActor>>(GetAdditionalSupplyChainActors);
		lazyTransportChargesMethodOfPayment = new Lazy<string>(GetTransportBillMethodOfPayment);
		lazyCountryOfDispatch = new Lazy<string>(() => bill.B0_RN_NKCountryOfExport);
		lazyGrossMass = new Lazy<decimal>(() => bill.B0_Weight);
		lazyDepartureMeansOfTransports = new Lazy<IReadOnlyCollection<IMeansOfTransport>>(GetDepartureMeansOfTransports);
	}

	string GetTransportBillMethodOfPayment()
	{
		if (!IsInTransitionPeriod)
		{
			var resolver = SharedValueMapResolverProvider.GetTransportBillMethodOfPaymentMapResolver();
			return resolver.GetValueForLine(bill);
		}
		return bill.B0_TransportPaymentMethod;
	}

	IReadOnlyCollection<IPreviousDocument> GetPreviousDocuments()
	{
		return bill
			.PreviousDocuments
			.Select(p => new PreviousDocumentWrapper(p))
			.ToCollection();
	}

	IReadOnlyCollection<ISupportingDocument> GetSupportingDocuments()
	{
		return bill
			.SupportingDocuments
			.Select(s => new SupportingDocumentWrapper(s))
			.ToCollection();
	}

	IReadOnlyCollection<IAdditionalReference> GetAdditionalReferences()
	{
		return GetAdditionalDocuments<IAdditionalReference>(NctsAdditionalInfoSubTypeCodes.AdditionalReference, ar => new AdditionalReferenceWrapper(ar));
	}

	IReadOnlyCollection<ITransportDocument> GetTransportDocuments()
	{
		var transportDocuments =
			!IsInTransitionPeriod
			? GetAdditionalDocuments<ITransportDocument>(NctsAdditionalInfoSubTypeCodes.TransportDocument, td => new TransportDocumentWrapper(td))
			: null;
		return transportDocuments ?? Array.Empty<ITransportDocument>();
	}

	IReadOnlyCollection<IAdditionalInformation> GetAdditionalInformation()
	{
		return GetAdditionalDocuments<IAdditionalInformation>(NctsAdditionalInfoSubTypeCodes.AdditionalInformation, ai => new ITShared.AdditionalInformationWrapper(ai));
	}

	ITrader GetTrader(JobDocAddress jobDocAddress)
	{
		return !jobDocAddress.IsEmpty
			? new EoriOrTcuTraderWrapper(jobDocAddress)
			: null;
	}

	ITrader GetConsignee()
	{
		var consignee = bill.Consignee;
		if (IsInTransitionPeriod)
		{
			return !consignee.IsEmpty
				? new EoriOrTcuTraderWrapper(consignee)
				: null;
		}

		var header = bill.Header;
		var movementHeader = header.MovementHeader;

		if (!movementHeader.IsSecurityTypeNONOrENT
			&& !header.GetC0009CountryCodes().Contains(movementHeader.BM_RL_NKDestinationPort)
			&& (header.Has30600AdditionalInformation || bill.Has30600AdditionalInformation))
		{
			return null;
		}

		return SharedValueMapResolverProvider.GetConsigneeMapResolver()
			.GetValueForLine(bill);
	}

	IReadOnlyCollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActors()
	{
		return bill
			.CusSupplyChainActorReferences
			.Cast<CusSupplyChainActorReference>()
			.Select(a => new ITShared.AdditionalSupplyChainActorWrapper(a))
			.ToCollection();
	}

	IReadOnlyCollection<IMeansOfTransport> GetDepartureMeansOfTransports()
	{
		if (IsInTransitionPeriod)
		{
			return [];
		}

		return SharedValueMapResolverProvider.GetDepartureTransportMeansMapResolver().GetValueForLine(bill) ?? [];
	}

	IReadOnlyCollection<T> GetAdditionalDocuments<T>(string subType, Func<EUAdditionalInfo, T> funcMapper)
	{
		return bill
			.AdditionalDocuments
			.Cast<EUAdditionalInfo>()
			.Where(x => x.CSI_SubType == subType)
			.Select(funcMapper)
			.ToCollection();
	}

	#endregion

	bool IsInTransitionPeriod => bill.IsInPhase5TransitionPeriod;

	readonly NctsBill bill;
}
