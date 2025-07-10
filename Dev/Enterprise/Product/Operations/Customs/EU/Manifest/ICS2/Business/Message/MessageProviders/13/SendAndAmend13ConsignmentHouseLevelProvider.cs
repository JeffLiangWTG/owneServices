using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend13;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend13ConsignmentHouseLevelProvider : IConsignmentHouseLevel
	{
		public static SendAndAmend13ConsignmentHouseLevelProvider NewOrNull(AsycudaBill bill) =>
			bill is null ? null : new(bill);

		SendAndAmend13ConsignmentHouseLevelProvider(AsycudaBill bill)
		{
			helper = new ConsignmentHouseLevelProviderHelper(Argument.NotNull(bill, nameof(bill)));
			asycudaBill = bill;
		}

		readonly AsycudaBill asycudaBill;

		readonly ConsignmentHouseLevelProviderHelper helper;

		public string ContainerIndicator => helper.ContainerIndicator;

		public decimal TotalGrossMass => helper.TotalGrossMass;

		public IUNLOCO PlaceOfAcceptance => helper.PlaceOfAcceptance;

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocuments => helper.SupportingDocuments;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationCollection => helper.AdditionalInformationCollection;

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActors => helper.AdditionalSupplyChainActors;

		public IParty Consignee => helper.Consignee;

		public IReadOnlyCollection<IGoodsItem> GoodsItems => helper.GoodsItems;

		public IParty Consignor => helper.Consignor;

		public string PaymentMethod => helper.PaymentMethod;

		public IUNLOCO PlaceOfDelivery => helper.PlaceOfDelivery;

		public IParty NotifyParty => helper.NotifyParty;

		public IIdentifierTypePair SupplementaryDeclarants => CachedValueHelper.GetValue(ref supplementaryDeclarants, () => SupplementaryDeclarantProvider.NewOrNull(asycudaBill.SupplementaryDeclarants?.FirstOrDefault()));
		CachedValue<IIdentifierTypePair> supplementaryDeclarants;

		public IReadOnlyCollection<ITransportEquipment> TransportEquipmentCollection => helper.TransportEquipmentCollection;

		public string UCRNumber => helper.UCRNumber;
	}
}
