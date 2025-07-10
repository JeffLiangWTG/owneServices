using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend10;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend10ConsignmentHouseLevelProvider : IConsignmentHouseLevel
	{
		public static SendAndAmend10ConsignmentHouseLevelProvider NewOrNull(AsycudaBill bill) => bill is null ? null : new(bill);

		SendAndAmend10ConsignmentHouseLevelProvider(AsycudaBill bill)
		{
			helper = new ConsignmentHouseLevelProviderHelper(Argument.NotNull(bill, nameof(bill)));
		}
		readonly ConsignmentHouseLevelProviderHelper helper;

		public string ContainerIndicator => helper.ContainerIndicator;

		public decimal TotalGrossMass => helper.TotalGrossMass;

		public IUNLOCO PlaceOfAcceptance => helper.PlaceOfAcceptance;

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocuments => helper.SupportingDocuments;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformation => helper.AdditionalInformationCollection;

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActor => helper.AdditionalSupplyChainActors;

		public IParty Consignee => helper.Consignee;

		public IReadOnlyCollection<IGoodsItem> GoodsItem => helper.GoodsItems;

		public IParty Consignor => helper.Consignor;

		public string PaymentMethod => helper.PaymentMethod;

		public IUNLOCO PlaceOfDelivery => helper.PlaceOfDelivery;

		public IParty GoodsShipmentBuyer => helper.Buyer;

		public IParty GoodsShipmentSeller => helper.Seller;

		public IParty NotifyParty => helper.NotifyParty;

		public IReadOnlyCollection<IPassiveBorderTransportMeans> PassiveBorderTransportMeans => helper.TransportMeansFromBill;

		public IReadOnlyCollection<ITransportEquipment> TransportEquipment => helper.TransportEquipmentCollection;

		public string UCRNumber => helper.UCRNumber;
	}
}
