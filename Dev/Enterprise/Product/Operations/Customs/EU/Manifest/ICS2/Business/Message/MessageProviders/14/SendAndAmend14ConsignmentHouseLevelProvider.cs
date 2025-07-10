using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend14;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend14ConsignmentHouseLevelProvider : IConsignmentHouseLevel
	{
		public static SendAndAmend14ConsignmentHouseLevelProvider NewOrNull(AsycudaBill bill) =>
			bill is null ? null : new(bill);

		public SendAndAmend14ConsignmentHouseLevelProvider(AsycudaBill bill)
		{
			helper = new ConsignmentHouseLevelProviderHelper(Argument.NotNull(bill, nameof(bill)));
		}

		readonly ConsignmentHouseLevelProviderHelper helper;

		public string ContainerIndicator => helper.ContainerIndicator;

		public decimal TotalGrossMass => helper.TotalGrossMass;

		public IUNLOCO PlaceOfAcceptance => helper.PlaceOfAcceptance;

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocuments => helper.SupportingDocuments;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationCollection => helper.AdditionalInformationCollection;

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActors => helper.AdditionalSupplyChainActors;

		public IIdentifierTypePair TransportDocumentMasterLevel => helper.TransportDocumentMasterLevel;

		public string CarrierIdentificationNumber => helper.CarrierIdentificationNumber;

		public IParty Consignee => helper.Consignee;

		public IReadOnlyCollection<IGoodsItem> GoodsItems => helper.GoodsItems;

		public IParty Consignor => helper.Consignor;

		public string PaymentMethod => helper.PaymentMethod;

		public IUNLOCO PlaceOfDelivery => helper.PlaceOfDelivery;

		public IReadOnlyCollection<IItinerary> Itineraries => helper.Itineraries;

		public IParty NotifyParty => helper.NotifyParty;

		public IReadOnlyCollection<IIdentifierTypePair> SupplementaryDeclarants => helper.SupplementaryDeclarants;

		public IIdentifierTypePair TransportDocumentHouseLevel => helper.TransportDocumentHouseLevel;

		public IReadOnlyCollection<ITransportEquipment> TransportEquipmentCollection => helper.TransportEquipmentCollection;

		public string UCRNumber => helper.UCRNumber;

		public IReadOnlyCollection<IPassiveBorderTransportMeans> PassiveBorderTransportMeans => Array.Empty<IPassiveBorderTransportMeans>();
	}
}
