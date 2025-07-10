using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend43;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend43ConsignmentHouseLevelProvider : IConsignmentHouseLevel
	{
		public static SendAndAmend43ConsignmentHouseLevelProvider NewOrNull(AsycudaBill bill) =>
			bill is null ? null : new(bill);

		public SendAndAmend43ConsignmentHouseLevelProvider(AsycudaBill bill)
		{
			helper = new ConsignmentHouseLevelProviderHelper(Argument.NotNull(bill, nameof(bill)));
		}

		readonly ConsignmentHouseLevelProviderHelper helper;

		public decimal TotalGrossMass => helper.TotalGrossMass;

		public IIdentifierTypePair AdditionalFiscalReferences => helper.AdditionalFiscalReferences;

		public IReadOnlyCollection<IIdentifierTypePair> SupportingDocumentsHouseLevel => helper.SupportingDocumentsHouseLevel;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationCollection => helper.AdditionalInformationCollection;

		public IReadOnlyCollection<IIdentifierTypePair> AdditionalSupplyChainActors => helper.AdditionalSupplyChainActors;

		public IParty Consignee => helper.Consignee;

		public IReadOnlyCollection<IGoodsItem> GoodsItems => helper.GoodsItems;

		public IParty Consignor => helper.Consignor;

		public IPostalCharge PostalCharges => helper.PostalCharges;

		public IIdentifierTypePair TransportDocumentHouseLevel => helper.TransportDocumentHouseLevel;

		public string UCRNumber => helper.UCRNumber;
	}
}
