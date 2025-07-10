using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend51;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmendHeader51Provider : ICS2BaseMessageProvider, ISendAndAmendHeader51
	{
		public SendAndAmendHeader51Provider(AsycudaManifestHeader header) : base(header)
		{
			helper = new MessageHeaderProviderHelper(Argument.NotNull(header, nameof(header)));
		}

		protected readonly MessageHeaderProviderHelper helper;

		public string ReferralRequestReference => GetReferralRequestReferenceCore();

		protected virtual string GetReferralRequestReferenceCore() => null;

		public string SpecificCircumstanceIndicator => helper.SpecificCircumstanceIndicator;

		public int ReentryIndicator => helper.ReEntryIndicator;

		public IActiveBorderTransportMeans ActiveBorderTransportMeans => CachedValueHelper.GetValue(ref activeBorderTransportMeans, () => ActiveBorderTransportMeansProvider.NewOrNull(manifestHeader));
		CachedValue<IActiveBorderTransportMeans> activeBorderTransportMeans;

		public IParty Representative => helper.Representative;

		public IUNLOCO PlaceOfAcceptance => CachedValueHelper.GetValue(ref placeOfAcceptance, () => new UNLOCOProvider(manifestHeader.AMA_RL_NKOrigin, string.Empty));
		CachedValue<IUNLOCO> placeOfAcceptance;

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocuments => helper.SupportingDocumentsMasterLevel;

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActors => additionalSupplyChainActors ??= manifestHeader.CusSupplyChainActorReferences.ToArray(sca => new AdditionalSupplyChainActorProvider(sca));
		IReadOnlyCollection<IIdentifierTypePair> additionalSupplyChainActors;

		public IParty Carrier => CachedValueHelper.GetValue(ref carrier, () => PartyProvider.NewOrNull(manifestHeader.Carrier));
		CachedValue<IParty> carrier;

		public IParty Consignee => CachedValueHelper.GetValue(ref consignee, () => BillPartyProvider.NewOrNull(manifestHeader.Bills.Cast<AsycudaBill>().FirstOrDefault(), AsycudaBillAddress.AddressType.Consignee));
		CachedValue<IParty> consignee;

		public IParty Consignor => CachedValueHelper.GetValue(ref consignor, () => BillPartyProvider.NewOrNull(manifestHeader.Bills.Cast<AsycudaBill>().FirstOrDefault(), AsycudaBillAddress.AddressType.Shipper));
		CachedValue<IParty> consignor;

		public string TransportChargesMethodOfPayment => manifestHeader.AMA_PaymentMethod;

		public IUNLOCO PlaceOfDelivery => CachedValueHelper.GetValue(ref placeOfDelivery, () => new UNLOCOProvider(manifestHeader.AMA_RL_NKFinalDestination, string.Empty));
		CachedValue<IUNLOCO> placeOfDelivery;

		public IReadOnlyCollection<IConsignmentHouseLevel> Consignments => consignments ??= manifestHeader.Bills.ToArray<AsycudaBill, IConsignmentHouseLevel>(SendAndAmend51ConsignmentHouseLevelProvider.NewOrNull);
		IReadOnlyCollection<IConsignmentHouseLevel> consignments;

		public IUNLOCO PlaceOfLoading => CachedValueHelper.GetValue(ref placeOfLoading, () => new UNLOCOProvider(manifestHeader.AMA_RL_NKPortOfLoading, string.Empty));
		CachedValue<IUNLOCO> placeOfLoading;

		public IIdentifierTypePair TransportDocumentMasterLevel => helper.TransportDocument;

		public string UCRNumber => null;

		public IUNLOCO PlaceOfUnloading => CachedValueHelper.GetValue(ref placeOfUnloading, () => new UNLOCOProvider(manifestHeader.AMA_RL_NKPortOfDischarge, string.Empty));
		CachedValue<IUNLOCO> placeOfUnloading;

		public string EntryCustomsOfficeReferenceNumber => helper.CustomsOfficeReferenceNumber;
	}
}
