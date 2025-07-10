using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend15;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend15ConsignmentHouseLevelProvider : IConsignmentHouseLevel
	{
		public static SendAndAmend15ConsignmentHouseLevelProvider NewOrNull(AsycudaBill bill) =>
			bill is null ? null : new (bill);

		public SendAndAmend15ConsignmentHouseLevelProvider(AsycudaBill bill)
		{
			helper = new ConsignmentHouseLevelProviderHelper(Argument.NotNull(bill, nameof(bill)));
		}
		readonly ConsignmentHouseLevelProviderHelper helper;

		public string ContainerIndicator => helper.ContainerIndicator;

		public decimal TotalGrossMass => helper.TotalGrossMass;

		public IUNLOCO PlaceOfAcceptance => helper.PlaceOfAcceptance;

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocuments => helper.SupportingDocumentsHouseLevel;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformation => helper.AdditionalInformationCollection;

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActor => helper.AdditionalSupplyChainActors;

		public IIdentifierTypePair TransportDocumentMasterLevel => helper.TransportDocumentMasterLevel;

		public string CarrierIdentificationNumber => helper.CarrierIdentificationNumber;

		public IParty Consignee => helper.Consignee;

		public IReadOnlyCollection<IGoodsItem> GoodsItem => helper.GoodsItems;

		public IParty Consignor => helper.Consignor;

		public string PaymentMethod => helper.PaymentMethod;

		public IUNLOCO PlaceOfDelivery => helper.PlaceOfDelivery;

		public IParty GoodsShipmentBuyer => helper.Buyer;

		public IParty GoodsShipmentSeller => helper.Seller;

		public IReadOnlyCollection<IItinerary> Itineraries => helper.Itineraries;

		public IParty NotifyParty => helper.NotifyParty;

		public IReadOnlyCollection<IPassiveBorderTransportMeans> PassiveBorderTransportMeans => Array.Empty<IPassiveBorderTransportMeans>();

		public IReadOnlyCollection<IIdentifierTypePair> SupplementaryDeclarant => helper.SupplementaryDeclarants;

		public IIdentifierTypePair TransportDocumentHouseLevel => helper.TransportDocumentHouseLevel;

		public IReadOnlyCollection<ITransportEquipment> TransportEquipment => helper.TransportEquipmentCollection;

		public string UCRNumber => helper.UCRNumber;
	}
}
