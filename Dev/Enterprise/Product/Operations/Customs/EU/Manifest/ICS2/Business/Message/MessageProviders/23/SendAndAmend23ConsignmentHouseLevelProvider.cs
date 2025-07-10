using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend23;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend23ConsignmentHouseLevelProvider : IConsignmentHouseLevel
	{
		public static SendAndAmend23ConsignmentHouseLevelProvider NewOrNull(AsycudaBill bill) =>
			bill is null ? null : new(bill);

		public SendAndAmend23ConsignmentHouseLevelProvider(AsycudaBill bill)
		{
			helper = new ConsignmentHouseLevelProviderHelper(Argument.NotNull(bill, nameof(bill)));
		}

		readonly ConsignmentHouseLevelProviderHelper helper;

		public decimal TotalGrossMass => helper.TotalGrossMass;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationCollection => helper.AdditionalInformationCollection;

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActors => helper.AdditionalSupplyChainActors;

		public IParty Consignee => helper.Consignee;

		public IReadOnlyCollection<IGoodsItem> GoodsItems => helper.GoodsItems;

		public IParty Consignor => helper.Consignor;

		public IIdentifierTypePair TransportDocumentHouseLevel => helper.TransportDocumentHouseLevel;
	}
}
