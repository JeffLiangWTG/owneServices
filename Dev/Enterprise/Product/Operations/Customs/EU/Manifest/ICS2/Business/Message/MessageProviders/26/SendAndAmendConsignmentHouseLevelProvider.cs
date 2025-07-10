using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend26;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend26ConsignmentHouseLevelProvider :  IConsignmentHouseLevel
	{
		public static SendAndAmend26ConsignmentHouseLevelProvider NewOrNull(AsycudaBill bill) =>
			bill is null ? null : new (bill);

		public SendAndAmend26ConsignmentHouseLevelProvider(AsycudaBill bill)
		{
			helper = new ConsignmentHouseLevelProviderHelper(Argument.NotNull(bill, nameof(bill)));
		}

		readonly ConsignmentHouseLevelProviderHelper helper;

		public string ContainerIndicator => "0";

		public IReadOnlyCollection<IGoodsItem> GoodsItems => helper.GoodsItems;

		public IUNLOCO PlaceOfAcceptance => helper.PlaceOfAcceptance;

		public IUNLOCO PlaceOfDelivery => helper.PlaceOfDelivery;

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocumentsHouseLevel => helper.SupportingDocumentsHouseLevel;

		public string PaymentMethod => helper.PaymentMethod;

		public IParty NotifyParty => helper.NotifyParty;

		public string UCRNumber => helper.UCRNumber;

		public decimal TotalGrossMass => helper.TotalGrossMass;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationCollection => helper.AdditionalInformationCollection;

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActors => helper.AdditionalSupplyChainActors;

		public string CarrierIdentificationNumber => helper.CarrierIdentificationNumber;

		public IParty Consignee => helper.Consignee;

		public IParty Consignor => helper.Consignor;

		public IIdentifierTypePair TransportDocumentHouseLevel => helper.TransportDocumentHouseLevel;

		public IIdentifierTypePair TransportDocumentMasterLevel => helper.TransportDocumentMasterLevel;

		public IReadOnlyCollection<IIdentifierTypePair> SupplementaryDeclarants => helper.SupplementaryDeclarants;

		public IReadOnlyCollection<IItinerary> Itineraries => helper.Itineraries;
	}
}
