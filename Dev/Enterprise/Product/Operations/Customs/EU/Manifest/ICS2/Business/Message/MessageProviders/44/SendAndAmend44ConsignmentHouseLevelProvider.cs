using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend44;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend44ConsignmentHouseLevelProvider : IConsignmentHouseLevel
	{
		public static SendAndAmend44ConsignmentHouseLevelProvider NewOrNull(AsycudaBill bill) =>
			bill is null ? null : new(bill);

		public SendAndAmend44ConsignmentHouseLevelProvider(AsycudaBill bill)
		{
			helper = new ConsignmentHouseLevelProviderHelper(Argument.NotNull(bill, nameof(bill)));
		}
		readonly ConsignmentHouseLevelProviderHelper helper;

		public string ReceptacleIdentificationNumber => helper.ReceptacleIdentificationNumber;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationCollection => helper.AdditionalInformationCollection;

		public IIdentifierTypePair TransportDocumentHouseLevel => helper.TransportDocumentHouseLevel;
	}
}
