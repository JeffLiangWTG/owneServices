using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Argument = CargoWise.Common.Argument;
using EUAdditionalInfo = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo;
using ISupportingDocument = CargoWise.Customs.IT.MessageContracts.NCTS.Departure.ISupportingDocument;
using NctsAdditionalInfoSubTypeCodes = Enterprise.Customs.EU.Business.AdditionalInfoSubTypeList.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class ConsignmentItemCustomsMessageWrapper : IConsignmentItemCustomsMessageWrapper
{
	public ConsignmentItemCustomsMessageWrapper(NctsDepartureCargoDesc goodsItem)
	{
		this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		InitializeLazy();
	}

	string IConsignmentItemCustomsMessageWrapper.DeclarationType => lazyDeclarationType.Value;
	Lazy<string> lazyDeclarationType;

	int IConsignmentItemCustomsMessageWrapper.GoodsItemNumber => lazyGoodsItemNumber.Value;
	Lazy<int> lazyGoodsItemNumber;

	int IConsignmentItemCustomsMessageWrapper.DeclarationGoodsItemNumber => lazyDeclarationGoodsItemNumber.Value;
	Lazy<int> lazyDeclarationGoodsItemNumber;

	IReadOnlyCollection<IConsignmentItemPreviousDocument> IConsignmentItemCustomsMessageWrapper.PreviousDocuments => lazyPreviousDocuments.Value;
	Lazy<IReadOnlyCollection<IConsignmentItemPreviousDocument>> lazyPreviousDocuments;

	IReadOnlyCollection<IAdditionalInformation> IConsignmentItemCustomsMessageWrapper.AdditionalInformation => lazyAdditionalInformation.Value;
	Lazy<IReadOnlyCollection<IAdditionalInformation>> lazyAdditionalInformation;

	IReadOnlyCollection<ISupportingDocument> IConsignmentItemCustomsMessageWrapper.SupportingDocuments => lazySupportingDocuments.Value;
	Lazy<IReadOnlyCollection<ISupportingDocument>> lazySupportingDocuments;

	IReadOnlyCollection<ITransportDocument> IConsignmentItemCustomsMessageWrapper.TransportDocuments => lazyTransportDocuments.Value;
	Lazy<IReadOnlyCollection<ITransportDocument>> lazyTransportDocuments;

	IReadOnlyCollection<IAdditionalReference> IConsignmentItemCustomsMessageWrapper.AdditionalReferences => lazyAdditionalReferences.Value;
	Lazy<IReadOnlyCollection<IAdditionalReference>> lazyAdditionalReferences;

	string IConsignmentItemCustomsMessageWrapper.Ucr => lazyUcr.Value;
	Lazy<string> lazyUcr;

	ITrader IConsignmentItemCustomsMessageWrapper.Consignee => lazyConsignee.Value;
	Lazy<ITrader> lazyConsignee;

	IReadOnlyCollection<IAdditionalSupplyChainActor> IConsignmentItemCustomsMessageWrapper.AdditionalSupplyChainActors => lazyAdditionalSupplyChainActors.Value;
	Lazy<IReadOnlyCollection<IAdditionalSupplyChainActor>> lazyAdditionalSupplyChainActors;

	string IConsignmentItemCustomsMessageWrapper.TransportChargesMethodOfPayment => lazyTransportChargesMethodOfPayment.Value;
	Lazy<string> lazyTransportChargesMethodOfPayment;

	string IConsignmentItemCustomsMessageWrapper.CountryOfDispatch => lazyCountryOfDispatch.Value;
	Lazy<string> lazyCountryOfDispatch;

	string IConsignmentItemCustomsMessageWrapper.CountryOfDestination => lazyCountryOfDestination.Value;
	Lazy<string> lazyCountryOfDestination;

	decimal? IConsignmentItemCustomsMessageWrapper.NetMass => lazyNetMass.Value;
	Lazy<decimal?> lazyNetMass;

	decimal? IConsignmentItemCustomsMessageWrapper.GrossMass => lazyGrossMass.Value;
	Lazy<decimal?> lazyGrossMass;

	decimal? IConsignmentItemCustomsMessageWrapper.SupplementaryUnits => lazySupplementaryUnits.Value;
	Lazy<decimal?> lazySupplementaryUnits;

	string IConsignmentItemCustomsMessageWrapper.DescriptionOfGoods => lazyDescriptionOfGoods.Value;
	Lazy<string> lazyDescriptionOfGoods;

	IReadOnlyCollection<IPackage> IConsignmentItemCustomsMessageWrapper.Packages => lazyPackages.Value;
	Lazy<IReadOnlyCollection<IPackage>> lazyPackages;

	string IConsignmentItemCustomsMessageWrapper.CusCode => lazyCusCode.Value;
	Lazy<string> lazyCusCode;

	string IConsignmentItemCustomsMessageWrapper.HsTariffCode => lazyHsTariffCode.Value;
	Lazy<string> lazyHsTariffCode;

	string IConsignmentItemCustomsMessageWrapper.NcTariffCode => lazyNcTariffCode.Value;
	Lazy<string> lazyNcTariffCode;

	IReadOnlyCollection<string> IConsignmentItemCustomsMessageWrapper.DangerousGoodsCodes => lazyDangerousGoodsCodes.Value;
	Lazy<IReadOnlyCollection<string>> lazyDangerousGoodsCodes;

	void InitializeLazy()
	{
		lazyDeclarationType = new Lazy<string>(() => goodsItem.BY_Type);
		lazyGoodsItemNumber = new Lazy<int>(() => goodsItem.BY_LineNo);
		lazyDeclarationGoodsItemNumber = new Lazy<int>(() => goodsItem.BY_DeclarationGoodsItemNumber);
		lazyAdditionalInformation = new Lazy<IReadOnlyCollection<IAdditionalInformation>>(GetAdditionalInformation);
		lazySupportingDocuments = new Lazy<IReadOnlyCollection<ISupportingDocument>>(GetSupportingDocuments);
		lazyTransportDocuments = new Lazy<IReadOnlyCollection<ITransportDocument>>(GetTransportDocuments);
		lazyAdditionalReferences = new Lazy<IReadOnlyCollection<IAdditionalReference>>(GetAdditionalReferences);
		lazyUcr = new Lazy<string>(() => goodsItem.BY_CommercialReferenceNumber);
		lazyConsignee = new Lazy<ITrader>(GetConsignee);
		lazyAdditionalSupplyChainActors = new Lazy<IReadOnlyCollection<IAdditionalSupplyChainActor>>(GetAdditionalSupplyChainActors);
		lazyTransportChargesMethodOfPayment = new Lazy<string>(GetTransportChargesMethodOfPayment);
		lazyCountryOfDispatch = new Lazy<string>(() => goodsItem.BY_RN_NKCountryOfDispatch);
		lazyCountryOfDestination = new Lazy<string>(GetCountryOfDestination);
		lazyNetMass = new Lazy<decimal?>(GetNetMass);
		lazyGrossMass = new Lazy<decimal?>(() => goodsItem.GrossMassInKilograms.GetValueOrNullIfZero());
		lazySupplementaryUnits = new Lazy<decimal?>(() => goodsItem.BY_CustomsSecondQuantity.GetValueOrNullIfZero());
		lazyDescriptionOfGoods = new Lazy<string>(() => goodsItem.BY_Description);
		lazyPackages = new Lazy<IReadOnlyCollection<IPackage>>(GetPackages);
		lazyCusCode = new Lazy<string>(() => goodsItem.BY_CusC4Number);
		lazyHsTariffCode = new Lazy<string>(() => goodsItem.BY_HarmonisedTariff.Left(HsTariffCodeLength));
		lazyNcTariffCode = new Lazy<string>(GetNcTariffCode);
		lazyDangerousGoodsCodes = new Lazy<IReadOnlyCollection<string>>(() => goodsItem.UNDGs.UniqueUNNumbers);
		lazyPreviousDocuments = new Lazy<IReadOnlyCollection<IConsignmentItemPreviousDocument>>(GetPreviousDocuments);
	}

	string GetCountryOfDestination()
	{
		var resolver = SharedValueMapResolverProvider.GetCountryOfDestinationMapResolver();
		return resolver.GetValueForLine(goodsItem);
	}

	IReadOnlyCollection<IAdditionalInformation> GetAdditionalInformation()
		=> GetAdditionalDocuments<IAdditionalInformation>(NctsAdditionalInfoSubTypeCodes.AdditionalInformation, x => new AdditionalInformationWrapper(x));

	IReadOnlyCollection<ITransportDocument> GetTransportDocuments()
	{
		if (!IsInPhase5TransitionPeriod)
		{
			return Array.Empty<ITransportDocument>();
		}

		return goodsItem
			.Bill
			.AdditionalDocuments
			.Cast<EUAdditionalInfo>()
			.Where(x => x.CSI_SubType == NctsAdditionalInfoSubTypeCodes.TransportDocument)
			.Select(x => new TransportDocumentWrapper(x))
			.ToCollection();
	}

	IReadOnlyCollection<IAdditionalReference> GetAdditionalReferences()
		=> GetAdditionalDocuments<IAdditionalReference>(NctsAdditionalInfoSubTypeCodes.AdditionalReference, x => new AdditionalReferenceWrapper(x));

	IReadOnlyCollection<T> GetAdditionalDocuments<T>(string subType, Func<EUAdditionalInfo, T> funcMapper)
	{
		return goodsItem
			.AdditionalInfos
			.Where(x => x.CSI_SubType == subType)
			.Select(funcMapper)
			.ToCollection();
	}

	IReadOnlyCollection<ISupportingDocument> GetSupportingDocuments()
	{
		return goodsItem
			.SupportingDocuments
			.Select(x => new SupportingDocumentWrapper(x))
			.ToCollection();
	}

	ITrader GetConsignee()
	{
		var consignee = goodsItem.Consignee;
		return !consignee.IsEmpty
			? new IT.Business.MessageSending.AidaXml.Export.EoriOrTcuTraderWrapper(consignee)
			: null;
	}

	IReadOnlyCollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActors()
	{
		return goodsItem
			.CusSupplyChainActorReferences
			.Select(x => new AdditionalSupplyChainActorWrapper(x))
			.ToCollection();
	}

	string GetTransportChargesMethodOfPayment()
	{
		if (IsInPhase5TransitionPeriod)
		{
			var resolver = SharedValueMapResolverProvider.GetTransportChargesMethodOfPaymentMapResolver();
			return resolver.GetValueForLine(goodsItem);
		}
		return goodsItem.BY_TransportChargesMethodOfPayment;
	}

	decimal? GetNetMass()
	{
		if (!goodsItem.Header.MovementHeader.BM_ReducedDatasetIndicator
			|| goodsItem.Bill.PreviousDocuments.Any(x => x.CSI_Code == NctsConstants.NctsTypeOfPreviousDocument.Codes.N830))
		{
			return goodsItem.NetMassInKilograms.GetValueOrNullIfZero();
		}
		return null;
	}

	IReadOnlyCollection<IPackage> GetPackages()
	{
		return goodsItem
			.Packages
			.Cast<NctsPackage>()
			.Select(x => new PackageWrapper(x))
			.ToCollection();
	}

	string GetNcTariffCode()
	{
		var hasCL112OfficeOfDepartureCountry = goodsItem
			.Header?
			.CustomsOfficesForDeparture
			.HasCL112OfficeOfDepartureCountry(goodsItem.Factory) ?? false;

		if (hasCL112OfficeOfDepartureCountry)
		{
			return null;
		}
		return goodsItem.BY_HarmonisedTariff.SubstringSafe(HsTariffCodeLength, NcTariffCodeLength);
	}

	IReadOnlyCollection<IConsignmentItemPreviousDocument> GetPreviousDocuments()
	{
		return goodsItem
			.PreviousDocuments
			.Select(x => new ConsignmentItemPreviousDocument(x))
			.ToCollection();
	}

	bool IsInPhase5TransitionPeriod => CachedValueHelper.GetValue(ref isInPhase5TransitionPeriod, () => goodsItem.IsInPhase5TransitionPeriod);
	CachedValue<bool> isInPhase5TransitionPeriod;

	const int HsTariffCodeLength = 6;
	const int NcTariffCodeLength = 2;

	readonly NctsDepartureCargoDesc goodsItem;
}
