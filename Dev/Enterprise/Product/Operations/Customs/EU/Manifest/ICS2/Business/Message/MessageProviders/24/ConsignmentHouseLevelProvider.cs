using System.Collections.Generic;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend24;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend24ConsignmentHouseLevelProvider : IConsignmentHouseLevel
	{
		public static SendAndAmend24ConsignmentHouseLevelProvider NewOrNull(AsycudaBill bill) =>
			bill is null ? null : new (bill);

		public SendAndAmend24ConsignmentHouseLevelProvider(AsycudaBill bill)
		{
			helper = new ConsignmentHouseLevelProviderHelper(Argument.NotNull(bill, nameof(bill)));
		}

		readonly ConsignmentHouseLevelProviderHelper helper;

		public decimal TotalGrossMass => helper.TotalGrossMass;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationCollection => helper.AdditionalInformationCollection;

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActors => helper.AdditionalSupplyChainActors;

		public IIdentifierTypePair TransportDocumentMasterLevel => helper.TransportDocumentMasterLevel;

		public string CarrierIdentificationNumber => helper.CarrierIdentificationNumber;

		public IParty Consignee => helper.Consignee;

		public IParty Consignor => helper.Consignor;

		public IIdentifierTypePair TransportDocumentHouseLevel => helper.TransportDocumentHouseLevel;

		public IReadOnlyCollection<IGoodsItem> GoodsItems => helper.GoodsItems;
	}
}
