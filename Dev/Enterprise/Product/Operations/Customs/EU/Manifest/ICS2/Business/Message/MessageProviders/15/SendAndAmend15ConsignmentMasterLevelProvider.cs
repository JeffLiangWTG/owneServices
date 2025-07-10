using System.Collections.Generic;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend15;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend15ConsignmentMasterLevelProvider : IConsignmentMasterLevel
	{
		SendAndAmend15ConsignmentMasterLevelProvider(AsycudaManifestHeader manifestHeader)
		{
			this.manifestHeader = manifestHeader;
		}

		public static SendAndAmend15ConsignmentMasterLevelProvider NewOrNull(AsycudaManifestHeader mh) => mh == null ? null : new SendAndAmend15ConsignmentMasterLevelProvider(mh);

		readonly AsycudaManifestHeader manifestHeader;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformationCollection => additionalInformations ??= manifestHeader.AdditionalInfos.ToArray<AdditionalInfo, IAdditionalInformation>(c => new AdditionalInformationProvider(c));
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;

		public IReadOnlyCollection<IConsignmentHouseLevel> ConsignmentHouseLevelCollection => consignments ??= manifestHeader.Bills.ToArray<AsycudaBill, IConsignmentHouseLevel>(SendAndAmend15ConsignmentHouseLevelProvider.NewOrNull);
		IReadOnlyCollection<IConsignmentHouseLevel> consignments;
	}
}
