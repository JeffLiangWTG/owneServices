using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend50;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend50ConsignmentHouseLevelProvider : IConsignmentHouseLevel
	{
		public static SendAndAmend50ConsignmentHouseLevelProvider NewOrNull(AsycudaBill bill) =>
		bill is null ? null : new(bill);

		public SendAndAmend50ConsignmentHouseLevelProvider(AsycudaBill bill)
		{
			this.bill = bill;
			helper = new ConsignmentHouseLevelProviderHelper(Argument.NotNull(bill, nameof(bill)));
		}

		readonly AsycudaBill bill;
		readonly ConsignmentHouseLevelProviderHelper helper;

		public string ContainerIndicator => helper.ContainerIndicator;

		public decimal TotalGrossMass => helper.TotalGrossMass;

		public IUNLOCO PlaceOfAcceptance => helper.PlaceOfAcceptance;

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocuments => helper.SupportingDocuments;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationCollection => helper.AdditionalInformationCollection;

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActors => helper.AdditionalSupplyChainActors;

		public IReadOnlyCollection<CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend50.IGoodsItem> GoodsItems => goodsItems ?? (goodsItems = GetGoodsItems());
		IReadOnlyCollection<CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend50.IGoodsItem> goodsItems;

		IReadOnlyCollection<CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend50.IGoodsItem> GetGoodsItems()
		{
			return bill.Packs.Cast<AsycudaPack>().Select(c => new SendAndAmend50GoodsItemProvider(c)).ToArray();
		}

		public string TransportChargesMethodOfPayment => helper.PaymentMethod;

		public IUNLOCO PlaceOfDelivery => helper.PlaceOfDelivery;

		public IParty GoodsShipmentBuyer => helper.Buyer;

		public IParty GoodsShipmentSeller => helper.Seller;

		public IReadOnlyCollection<IItinerary> Itineraries => helper.Itineraries;

		public IReadOnlyCollection<IPassiveBorderTransportMeans> PassiveBorderTransportMeans => helper.TransportMeansFromBill;

		public IIdentifierTypePair TransportDocumentHouseLevel => helper.TransportDocumentHouseLevel;

		public IReadOnlyCollection<ITransportEquipment> TransportEquipment => helper.TransportEquipmentCollection;

		public string UCRNumber => helper.UCRNumber;
	}
}
