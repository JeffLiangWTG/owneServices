using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend41;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmendHeader41Provider : ICS2BaseMessageProvider, ISendAndAmendHeader41
	{
		public SendAndAmendHeader41Provider(AsycudaManifestHeader header)
			: base(header)
		{
			helper = new MessageHeaderProviderHelper(Argument.NotNull(header, nameof(header)));
		}

		readonly MessageHeaderProviderHelper helper;

		public string SpecificCircumstanceIndicator => helper.SpecificCircumstanceIndicator;

		public int ReentryIndicator => manifestHeader.ReEntryIndicator ? 1 : 0;

		public IParty Representative => helper.Representative;

		public IActiveBorderTransportMeans ActiveBorderTransportMeans => CachedValueHelper.GetValue(ref activeBorderTransportMeans, () => ActiveBorderTransportMeansProvider.NewOrNull(manifestHeader));
		CachedValue<IActiveBorderTransportMeans> activeBorderTransportMeans;

		public int ContainerIndicator => manifestHeader.Containers.Any(c => !c.ACN_ContainerNumber.IsEmpty) ? 1 : 0;

		public IReadOnlyCollection<string> ReceptacleIdentificationNumbers => helper.ReceptacleIdentificationNumbers;

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocuments => helper.SupportingDocumentsMasterLevel;

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActors => additionalSupplyChainActors ??= manifestHeader.CusSupplyChainActorReferences.ToArray(sca => new AdditionalSupplyChainActorProvider(sca));
		IReadOnlyCollection<IIdentifierTypePair> additionalSupplyChainActors;

		public IParty Carrier => CachedValueHelper.GetValue(ref carrier, () => PartyProvider.NewOrNull(manifestHeader.Carrier));
		CachedValue<IParty> carrier;

		public IParty Consignee => CachedValueHelper.GetValue(ref consignee, () => BillPartyProvider.NewOrNull(manifestHeader.Bills.Cast<AsycudaBill>().FirstOrDefault(), AsycudaBillAddress.AddressType.Consignee));
		CachedValue<IParty> consignee;

		public IParty Consignor => CachedValueHelper.GetValue(ref consignor, () => BillPartyProvider.NewOrNull(manifestHeader.Bills.Cast<AsycudaBill>().FirstOrDefault(), AsycudaBillAddress.AddressType.Shipper));
		CachedValue<IParty> consignor;

		public IUNLOCO PlaceOfLoading => CachedValueHelper.GetValue(ref placeOfLoading, () => new UNLOCOProvider(manifestHeader.AMA_RL_NKPortOfLoading, string.Empty));
		CachedValue<IUNLOCO> placeOfLoading;

		public IReadOnlyCollection<IPassiveBorderTransportMeans> PassiveBorderTransportMeans => passiveBorderTransportMeans ??= manifestHeader.Bills.Cast<AsycudaBill>().FirstOrDefault()?.AsycudaTransportMeans.ToArray<AsycudaTransportMeans, IPassiveBorderTransportMeans>(tm => new PassiveBorderTransportMeansProvider(tm)) ?? Array.Empty<IPassiveBorderTransportMeans>();
		IReadOnlyCollection<IPassiveBorderTransportMeans> passiveBorderTransportMeans;

		public IIdentifierTypePair TransportDocumentMasterLevel => helper.TransportDocument;

		public IReadOnlyCollection<ITransportEquipment> TransportEquipments => transportEquipments ??= manifestHeader.Containers.ToArray<AsycudaContainer, ITransportEquipment>(TransportEquipmentProvider.NewOrNull);
		IReadOnlyCollection<ITransportEquipment> transportEquipments;

		public string UCRNumber => null;

		public IUNLOCO PlaceOfUnloading => CachedValueHelper.GetValue(ref placeOfUnloading, () => new UNLOCOProvider(manifestHeader.AMA_RL_NKPortOfDischarge, string.Empty));
		CachedValue<IUNLOCO> placeOfUnloading;

		public string EntryCustomsOfficeReferenceNumber => manifestHeader.AMA_CustomsOffice;
	}
}
