using System.Collections.Generic;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend14;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend14ConsignmentMasterLevelProvider : IConsignmentMasterLevel
	{
		SendAndAmend14ConsignmentMasterLevelProvider(AsycudaManifestHeader manifestHeader)
		{
			this.manifestHeader = manifestHeader;
		}

		public static SendAndAmend14ConsignmentMasterLevelProvider NewOrNull(AsycudaManifestHeader mh) => mh == null ? null : new SendAndAmend14ConsignmentMasterLevelProvider(mh);

		readonly AsycudaManifestHeader manifestHeader;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationCollection => additionalInformations ??= manifestHeader.AdditionalInfos.ToArray<AdditionalInfo, IAdditionalInformation>(c => new AdditionalInformationProvider(c));
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;

		public IReadOnlyCollection<IConsignmentHouseLevel> ConsignmentHouseLevelCollection => consignments ??= manifestHeader.Bills.ToArray<AsycudaBill, IConsignmentHouseLevel>(SendAndAmend14ConsignmentHouseLevelProvider.NewOrNull);
		IReadOnlyCollection<IConsignmentHouseLevel> consignments;
	}
}
