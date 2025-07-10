using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ConsignmentHouseLevelProviderHelper
	{
		public ConsignmentHouseLevelProviderHelper(AsycudaBill bill)
		{
			asycudaBill = Argument.NotNull(bill, nameof(bill));
		}

		readonly AsycudaBill asycudaBill;

		public decimal TotalGrossMass => asycudaBill.GrossWeightInKG;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationCollection => additionalInformation ??= GetAdditionalInformation();
		IReadOnlyCollection<IAdditionalInformation> additionalInformation;

		IReadOnlyCollection<IAdditionalInformation> GetAdditionalInformation()
		{
			return asycudaBill.AdditionalInfos.ToArray<AdditionalInfo, IAdditionalInformation>(c => new AdditionalInformationProvider(c));
		}

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActors => additionalSupplyChainActor ??= GetAdditionalSupplyChainActor();
		IReadOnlyCollection<IIdentifierTypePair> additionalSupplyChainActor;

		IReadOnlyCollection<IIdentifierTypePair> GetAdditionalSupplyChainActor()
		{
			return asycudaBill.CusSupplyChainActorReferences.ToArray<CusSupplyChainActorReference, IIdentifierTypePair>(c => new AdditionalSupplyChainActorProvider(c));
		}

		public IIdentifierTypePair TransportDocumentMasterLevel => CachedValueHelper.GetValue(ref transportDocumentMasterLevel, () => TransportDocumentProvider.NewOrNull(asycudaBill.Header.AMA_MasterBill, asycudaBill.Header.MasterBill.TransportDocumentType));
		CachedValue<IIdentifierTypePair> transportDocumentMasterLevel;

		public string CarrierIdentificationNumber => asycudaBill.Header.Carrier?.Header?.GetICS2EoriDetails() ?? string.Empty;

		public IParty Consignee => CachedValueHelper.GetValue(ref consignee, () => BillPartyProvider.NewOrNull(asycudaBill, ManifestBase.AsycudaBillAddress.AddressType.Consignee));
		CachedValue<IParty> consignee;

		public IParty Consignor => CachedValueHelper.GetValue(ref consignor, () => BillPartyProvider.NewOrNull(asycudaBill, ManifestBase.AsycudaBillAddress.AddressType.Shipper));
		CachedValue<IParty> consignor;

		public IParty Buyer => CachedValueHelper.GetValue(ref buyer, () => BillPartyProvider.NewOrNull(asycudaBill, ManifestBase.AsycudaBillAddress.AddressType.Buyer));
		CachedValue<IParty> buyer;

		public IParty Seller => CachedValueHelper.GetValue(ref seller, () => BillPartyProvider.NewOrNull(asycudaBill, ManifestBase.AsycudaBillAddress.AddressType.Seller));
		CachedValue<IParty> seller;

		public IIdentifierTypePair TransportDocumentHouseLevel => CachedValueHelper.GetValue(ref transportDocumentHouseLevel, () => TransportDocumentProvider.NewOrNull(asycudaBill.ABL_BillNumber, asycudaBill.TransportDocumentType));
		CachedValue<IIdentifierTypePair> transportDocumentHouseLevel;

		public IIdentifierTypePair AdditionalFiscalReferences => CachedValueHelper.GetValue(ref additionalFiscalReferences, () => MessageProviderHelper.GetFirstAdditionalFiscalReference(asycudaBill.AdditionalFiscalReferences));
		CachedValue<IIdentifierTypePair> additionalFiscalReferences;

		public IPostalCharge PostalCharges => CachedValueHelper.GetValue(ref postalCharges, () => new PostalChargesProvider(asycudaBill.ABL_FreightValue, asycudaBill.ABL_RX_NKFreightValueCurrency));
		CachedValue<IPostalCharge> postalCharges;

		public IReadOnlyCollection<IGoodsItem> GoodsItems => goodsItems ??= GetGoodsItem();
		IReadOnlyCollection<IGoodsItem> goodsItems;

		IReadOnlyCollection<IGoodsItem> GetGoodsItem()
		{
			return asycudaBill.Packs.Cast<AsycudaPack>().Select(c => new GoodsItemProvider(c)).OrderBy(o => o.GoodsItemNumber).ToArray();
		}

		public IUNLOCO PlaceOfAcceptance => CachedValueHelper.GetValue(ref placeOfAcceptance, () => new UNLOCOProvider(asycudaBill.ABL_RL_NKOrigin, string.Empty));
		CachedValue<IUNLOCO> placeOfAcceptance;

		public IUNLOCO PlaceOfDelivery => CachedValueHelper.GetValue(ref placeOfDelivery, () => new UNLOCOProvider(asycudaBill.ABL_RL_NKFinalDestination, string.Empty));
		CachedValue<IUNLOCO> placeOfDelivery;

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocumentsHouseLevel => supportingDocumentsHouseLevel ??= MessageProviderHelper.GetSupportingDocuments(asycudaBill.SupportingDocuments);
		IReadOnlyCollection<IIdentifierTypePair> supportingDocumentsHouseLevel;

		public string PaymentMethod => asycudaBill.ABL_PrepaidCollect;

		public IParty NotifyParty => CachedValueHelper.GetValue(ref notifyParty, () => BillPartyProvider.NewOrNull(asycudaBill, ManifestBase.AsycudaBillAddress.AddressType.NotifyParty));
		CachedValue<IParty> notifyParty;

		public string UCRNumber => asycudaBill.ABL_UCRNumber;

		public IReadOnlyCollection<IIdentifierTypePair> SupplementaryDeclarants => supplementaryDeclarants ??= GetSupplementaryDeclarants();
		IReadOnlyCollection<IIdentifierTypePair> supplementaryDeclarants;

		IReadOnlyCollection<IIdentifierTypePair> GetSupplementaryDeclarants()
		{
			return MessageProviderHelper.ToArray<SupplementaryDeclarant, IIdentifierTypePair>(asycudaBill.SupplementaryDeclarants, (supplementaryDeclarant) => SupplementaryDeclarantProvider.NewOrNull(supplementaryDeclarant));
		}

		public IReadOnlyCollection<IItinerary> Itineraries => itineraries ??= GetItineraries();
		IReadOnlyCollection<IItinerary> itineraries;

		IReadOnlyCollection<IItinerary> GetItineraries()
		{
			return MessageProviderHelper.ToArray<RouteEntry, IItinerary>(asycudaBill.Header.Itinerary, (itinerary) => new ItineraryProvider(itinerary));
		}

		public IReadOnlyCollection<IPassiveBorderTransportMeans> TransportMeansFromBill => transportMeansFromBill ??= GetTransportMeansFromBill();
		IReadOnlyCollection<IPassiveBorderTransportMeans> transportMeansFromBill;

		IReadOnlyCollection<IPassiveBorderTransportMeans> GetTransportMeansFromBill() =>
			asycudaBill.Packs.Any()
				? Array.Empty<IPassiveBorderTransportMeans>()
				: asycudaBill.AsycudaTransportMeans.ToArray<AsycudaTransportMeans, IPassiveBorderTransportMeans>(tm => new PassiveBorderTransportMeansProvider(tm));

		public IReadOnlyCollection<ITransportEquipment> TransportEquipmentCollection => transportEquipmentCollection ??= GetTransportEquipmentCollection();
		IReadOnlyCollection<ITransportEquipment> transportEquipmentCollection;

		public IReadOnlyCollection<ITransportEquipment> GetTransportEquipmentCollection()
		{
			var containers = asycudaBill.Header.Containers.Where(c => asycudaBill.Packs.Any(p => ((AsycudaPack)p).ContainerPK == c.PK)).ToArray();
			return MessageProviderHelper.ToArray<AsycudaContainer, ITransportEquipment>(containers, TransportEquipmentProvider.NewOrNull);
		}

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocuments => supportingDocuments ??= GetSupportingDocuments();

		IReadOnlyCollection<IIdentifierTypePair> supportingDocuments;

		public IReadOnlyCollection<IIdentifierTypePair> GetSupportingDocuments()
		{
			return MessageProviderHelper.ToArray<SupportingDocument, IIdentifierTypePair>(asycudaBill.SupportingDocuments, SupportingDocumentProvider.NewOrNull);
		}

		public string ReceptacleIdentificationNumber => asycudaBill.ReceptacleId;

		public string ContainerIndicator => asycudaBill.Header.Containers.Any(c => !string.IsNullOrEmpty(c.ACN_ContainerNumber)) ? "1" : "0";
	}
}
