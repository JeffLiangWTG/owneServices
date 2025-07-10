using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.NCTS.Business.NCTS.MessageSending.AidaXml;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class D2AdditionalDataProvider : IDeclarationD2AdditionalDataProvider
{
	public D2AdditionalDataProvider(INctsEntityWrapperProvider entityWrapperProvider, bool isInPhase5TransitionPeriod)
	{
		this.entityWrapperProvider = Argument.NotNull(entityWrapperProvider, nameof(entityWrapperProvider));
		this.isInPhase5TransitionPeriod = isInPhase5TransitionPeriod;
	}
	readonly INctsEntityWrapperProvider entityWrapperProvider;
	readonly bool isInPhase5TransitionPeriod;

	int IDeclarationD2AdditionalDataProvider.GetSecurity(NctsDepartureMovementHeader @object) => GetNctsMovementHeaderWrapper(@object).GetSecurityType();

	int IDeclarationD2AdditionalDataProvider.GetReducedDatasetIndicator(NctsDepartureMovementHeader @object)
		=> GetNctsMovementHeaderWrapper(@object).GetReducedDatasetIndicator();

	string IDeclarationD2AdditionalDataProvider.GetSpecificCircumstanceIndicator(NctsDepartureMovementHeader @object)
		=> GetNctsMovementHeaderWrapper(@object).GetSpecificCircumstanceIndicator();

	DateTime? IDeclarationD2AdditionalDataProvider.GetLimitDate(NctsDepartureMovementHeader @object)
		=> GetNctsMovementHeaderWrapper(@object).GetLimitDate();

	int IDeclarationD2AdditionalDataProvider.GetBindingItinerary(NctsHeader @object) => GetNctsHeaderWrapper(@object).GetBindingItinerary();

	CarrierTypeDataProviderAbstractClass IDeclarationD2AdditionalDataProvider.GetCarrier(NctsHeader @object) => GetNctsHeaderWrapper(@object).GetCarrier();

	PlaceOfLoadingTypeDataProviderAbstractClass IDeclarationD2AdditionalDataProvider.GetPlaceOfLoading(NctsHeader @object)
		=> GetNctsMovementHeaderWrapper(@object.MovementHeader).GetPlaceOfLoading();

	PlaceOfUnloadingTypeDataProviderAbstractClass IDeclarationD2AdditionalDataProvider.GetPlaceOfUnloading(NctsHeader @object)
		=> GetNctsMovementHeaderWrapper(@object.MovementHeader).GetPlaceOfUnloading();

	LocationOfGoodsTypeDataProviderAbstractClass IDeclarationD2AdditionalDataProvider.GetLocationOfGoods(NctsHeader @object)
		=> GetNctsMovementHeaderWrapper(@object.MovementHeader).GetLocationOfGoods();

	int? IDeclarationD2AdditionalDataProvider.GetContainerIndicator(NctsHeader @object)
		=> GetNctsHeaderWrapper(@object).GetContainerIndicator();

	IReadOnlyCollection<DepartureTransportMeansTypeDataProviderAbstractClass> IDeclarationD2AdditionalDataProvider.GetDepartureTransportMeans(NctsHeader @object)
		=> GetNctsMovementHeaderWrapper(@object.MovementHeader).GetDepartureTransportMeans();

	IReadOnlyCollection<ActiveBorderTransportMeansTypeDataProviderAbstractClass> IDeclarationD2AdditionalDataProvider.GetActiveBorderTransportMeans(NctsHeader @object)
		=> GetNctsMovementHeaderWrapper(@object.MovementHeader).GetActiveBorderTransportMeans();

	IReadOnlyCollection<GoodsReferenceTypeDataProviderAbstractClass> IDeclarationD2AdditionalDataProvider.GetGoodsReference(NctsDepartureHeaderContainer @object)
		=> GetNctsHeaderWrapper(@object.Header as NctsHeader).GetGoodsReference(@object);

	IReadOnlyCollection<SealTypeDataProviderAbstractClass> IDeclarationD2AdditionalDataProvider.GetSeal(NctsDepartureHeaderContainer @object)
	{
		if (@object is null)
		{
			return null;
		}

		var seals = ((ITransportEquipment)new TransportEquipmentWrapper(@object)).Seals;
		return seals.Select(s => new SealType(s)).ToArray();
	}

	int? IDeclarationD2AdditionalDataProvider.GetNumberOfPackages(NctsPackage @object)
	{
		if (@object is NctsPackage nctsPackage && new PackageWrapper(nctsPackage) is IPackage packageWrapper)
		{
			return packageWrapper.NumberOfPacks;
		}

		return null;
	}

	string IDeclarationD2AdditionalDataProvider.GetCountryOfDestination(NctsHeader header)
	{
		if (header.IsInPhase5TransitionPeriod)
		{
			return SharedValueMapResolverProvider.GetCountryOfDestinationMapResolver()
				.GetValueForHeader(header.MovementHeader).ToString();
		}
		return SharedValueMapResolverProvider.GetCountryOfDestinationMapHeaderResolver()
			.GetValueForHeader(header.MovementHeader).ToString();
	}

	string IDeclarationD2AdditionalDataProvider.GetCountryOfDestination(NctsBill bill)
	{
		if (bill.Header.IsInPhase5TransitionPeriod)
		{
			return null;
		}
		return SharedValueMapResolverProvider.GetCountryOfDestinationMapHeaderResolver()
			.GetValueForLine(bill).ToString();
	}

	string IDeclarationD2AdditionalDataProvider.GetCountryOfDestination(NctsDepartureCargoDesc goodsItem)
	{
		if (goodsItem.Header.IsInPhase5TransitionPeriod)
		{
			return SharedValueMapResolverProvider.GetCountryOfDestinationMapResolver()
				.GetValueForLine(goodsItem).ToString();
		}
		return SharedValueMapResolverProvider.GetCountryOfDestinationMapLineResolver()
			.GetValueForLine(goodsItem).ToString();
	}

	public string GetName(JobDocAddress address)
	{
		return address.GetTraderNameOrNull(isInPhase5TransitionPeriod);
	}

	public AddressTypeDataProviderAbstractClass GetAddress(JobDocAddress address)
	{
		return address.GetTraderAddressOrNull(isInPhase5TransitionPeriod);
	}

	INctsDepartureMovementHeaderWrapper GetNctsMovementHeaderWrapper(NctsDepartureMovementHeader movementHeader)
		=> entityWrapperProvider.GetNctsDepartureMovementHeaderWrapper(movementHeader);

	INctsHeaderWrapper GetNctsHeaderWrapper(NctsHeader header) => entityWrapperProvider.GetNctsHeaderWrapper(header);

	IHouseConsignmentCustomsMessageWrapper GetHouseConsignmentWrapper(NctsBill bill) => entityWrapperProvider.GetHouseConsignmentWrapper(bill);

	public IReadOnlyCollection<HouseConsignmentToBeDeletedTypeDataProviderAbstractClass> GetHouseConsignmentToBeDeleted(NctsHeaderMessageSendingObject businessObject)
		=> businessObject.Amendment?.HouseConsignmentToBeDeleted;

	IReadOnlyCollection<DangerousGoodsTypeDataProviderAbstractClass> IDeclarationD2AdditionalDataProvider.GetDangerousGoods(NctsDepartureCargoDesc businessObject)
	{
		return businessObject
			.UNDGs
			.UniqueUNNumbers
			.Select((unNumber, sequenceNumber) => new DangerousGoodsTypeDataProvider(++sequenceNumber, unNumber))
			.ToList();
	}

	string IDeclarationD2AdditionalDataProvider.GetMethodOfPayment(NctsDepartureMovementHeader movementHeader)
	{
		if (movementHeader.IsInPhase5TransitionPeriod)
		{
			var resolver = SharedValueMapResolverProvider.GetTransportChargesMethodOfPaymentMapResolver();
			return resolver.GetValueForHeader(movementHeader);
		}
		else
		{
			var resolver = SharedValueMapResolverProvider.GetTransportBillMethodOfPaymentMapResolver();
			return resolver.GetValueForHeader(movementHeader);
		}
	}

	string IDeclarationD2AdditionalDataProvider.GetMethodOfPayment(NctsDepartureCargoDesc goodsItem)
	{
		if (goodsItem.IsInPhase5TransitionPeriod)
		{
			var resolver = SharedValueMapResolverProvider.GetTransportChargesMethodOfPaymentMapResolver();
			return resolver.GetValueForLine(goodsItem);
		}
		return goodsItem.BY_TransportChargesMethodOfPayment;
	}

	string IDeclarationD2AdditionalDataProvider.GetMethodOfPayment(NctsBill bill)
	{
		if (!bill.IsInPhase5TransitionPeriod)
		{
			var resolver = SharedValueMapResolverProvider.GetTransportBillMethodOfPaymentMapResolver();
			return resolver.GetValueForLine(bill);
		}
		return bill.B0_TransportPaymentMethod;
	}

	string IDeclarationD2AdditionalDataProvider.GetReferenceNumber(NctsSupportingDocument businessObject) => businessObject.GetReferenceNumberWithYearOfIssueAndCountry();

	IReadOnlyCollection<DepartureTransportMeansHCTypeDataProviderAbstractClass> IDeclarationD2AdditionalDataProvider.GetTransportInformation(NctsBill bill)
		=> bill.IsInPhase5TransitionPeriod
			? Array.Empty<DepartureTransportMeansHCTypeDataProviderAbstractClass>()
			: DepartureMeansOfTransportHCWrapper.CollectFromBill(bill);

	ConsigneeTypeDataProviderAbstractClass IDeclarationD2AdditionalDataProvider.GetConsignee(NctsHeader businessObject)
	{
		var consignee = GetNctsHeaderWrapper(businessObject).GetConsignee();
		return consignee != null
			? new ConsigneeTypeDataProvider(consignee)
			: null;
	}

	ConsigneeTypeDataProviderAbstractClass IDeclarationD2AdditionalDataProvider.GetConsignee(NctsBill businessObject)
	{
		var consignee = GetHouseConsignmentWrapper(businessObject).Consignee;
		return consignee != null
			? new ConsigneeTypeDataProvider(consignee)
			: null;
	}

	#region GetReferenceNumberUCR

	public string GetReferenceNumberUCR(NctsHeader header)
	{
		if (header.IsInPhase5TransitionPeriod)
		{
			return header.MovementHeader.BM_UniqueConsignmentReference;
		}
		return header.ResolveUcr();
	}

	public string GetReferenceNumberUCR(NctsBill bill)
	{
		var header = bill.Header;
		if (header.IsInPhase5TransitionPeriod)
		{
			return bill.B0_ReferenceID;
		}
		return bill.ResolveUcr();
	}

	public string GetReferenceNumberUCR(NctsDepartureCargoDesc goodsItem)
	{
		var header = goodsItem.Bill.Header;
		if (header.IsInPhase5TransitionPeriod)
		{
			return goodsItem.BY_CommercialReferenceNumber;
		}
		return goodsItem.ResolveUcr();
	}

	#endregion

	#region DangerousGoodsTypeDataProvider

	sealed class DangerousGoodsTypeDataProvider : DangerousGoodsTypeDataProviderAbstractClass
	{
		public DangerousGoodsTypeDataProvider(int sequenceNumber, string unNumber)
		{
			SequenceNumber = sequenceNumber;
			UNNumber = unNumber;
		}

		public override int SequenceNumber { get; }

		public override string UNNumber { get; }
	}

	#endregion

	#region ConsigneeTypeDataProvider

	sealed class ConsigneeTypeDataProvider : ConsigneeTypeDataProviderAbstractClass
	{
		public ConsigneeTypeDataProvider(ITrader consignee)
		{
			Argument.NotNull(consignee, nameof(consignee));
			IdentificationNumber = consignee.IdentificationNumber.Trim();
			Name = consignee.Address?.Name;
			Address = consignee.Address != null ? new AddressTypeWrapper(consignee.Address) : null;
		}

		public override string IdentificationNumber { get; }

		public override string Name { get; }

		public override AddressTypeDataProviderAbstractClass Address { get; }
	}

	#endregion
}
