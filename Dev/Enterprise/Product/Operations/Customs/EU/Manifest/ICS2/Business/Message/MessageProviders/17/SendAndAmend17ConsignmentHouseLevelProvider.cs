using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend17;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend17ConsignmentHouseLevelProvider : IConsignmentHouseLevel
	{
		public static SendAndAmend17ConsignmentHouseLevelProvider NewOrNull(AsycudaBill bill) =>
			bill is null ? null : new(bill);

		public SendAndAmend17ConsignmentHouseLevelProvider(AsycudaBill bill)
		{
			helper = new ConsignmentHouseLevelProviderHelper(Argument.NotNull(bill, nameof(bill)));
		}

		readonly ConsignmentHouseLevelProviderHelper helper;

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocuments => helper.SupportingDocumentsHouseLevel;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => helper.AdditionalInformationCollection;

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActors => helper.AdditionalSupplyChainActors;

		public IIdentifierTypePair TransportDocumentMasterLevel => helper.TransportDocumentMasterLevel;

		public string CarrierIdentificationNumber => helper.CarrierIdentificationNumber;

		public IParty GoodsShipmentBuyer => helper.Buyer;

		public IParty GoodsShipmentSeller => helper.Seller;

		public IIdentifierTypePair TransportDocumentHouseLevel => helper.TransportDocumentHouseLevel;

		public string UCRNumber => helper.UCRNumber;
	}
}
