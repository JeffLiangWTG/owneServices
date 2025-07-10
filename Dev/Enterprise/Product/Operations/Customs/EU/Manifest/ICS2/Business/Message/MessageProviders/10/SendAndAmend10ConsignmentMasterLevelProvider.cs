using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend10;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend10ConsignmentMasterLevelProvider : IConsignmentMasterLevel
	{
		public static SendAndAmend10ConsignmentMasterLevelProvider NewOrNull(AsycudaManifestHeader manifestHeader) => manifestHeader == null ? null : new SendAndAmend10ConsignmentMasterLevelProvider(manifestHeader);

		SendAndAmend10ConsignmentMasterLevelProvider(AsycudaManifestHeader manifestHeader)
		{
			this.manifestHeader = manifestHeader;
			helper = new MessageHeaderProviderHelper(Argument.NotNull(manifestHeader, nameof(manifestHeader)));
		}
		readonly AsycudaManifestHeader manifestHeader;
		readonly MessageHeaderProviderHelper helper;

		public IParty Carrier => CachedValueHelper.GetValue(ref carrier, () => PartyProvider.NewOrNull(manifestHeader.Carrier));
		CachedValue<IParty> carrier;

		public IConsignmentHouseLevel ConsignmentHouseLevel => CachedValueHelper.GetValue(ref consignmentHouseLevel, () => SendAndAmend10ConsignmentHouseLevelProvider.NewOrNull(manifestHeader.Bills.Cast<AsycudaBill>().FirstOrDefault()));
		CachedValue<IConsignmentHouseLevel> consignmentHouseLevel;

		public IUNLOCO PlaceOfLoading => CachedValueHelper.GetValue(ref placeOfLoading, () => new UNLOCOProvider(manifestHeader.AMA_RL_NKPortOfLoading, string.Empty));
		CachedValue<IUNLOCO> placeOfLoading;

		public IIdentifierTypePair TransportDocumentMasterLevel => helper.TransportDocument;

		public IUNLOCO PlaceOfUnloading => CachedValueHelper.GetValue(ref placeOfUnloading, () => new UNLOCOProvider(manifestHeader.AMA_RL_NKPortOfDischarge, string.Empty));
		CachedValue<IUNLOCO> placeOfUnloading;
	}
}
