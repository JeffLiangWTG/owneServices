using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.EU.MessageContracts.ICS2.SendAndAmend13;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SendAndAmend13ConsignmentMasterLevelProvider : IConsignmentMasterLevel
	{
		SendAndAmend13ConsignmentMasterLevelProvider(AsycudaManifestHeader manifestHeader)
		{
			this.manifestHeader = Argument.NotNull(manifestHeader, nameof(manifestHeader));
			helper = new MessageHeaderProviderHelper(Argument.NotNull(manifestHeader, nameof(manifestHeader)));
		}

		public static SendAndAmend13ConsignmentMasterLevelProvider NewOrNull(AsycudaManifestHeader mh) => mh == null ? null : new SendAndAmend13ConsignmentMasterLevelProvider(mh);

		readonly AsycudaManifestHeader manifestHeader;
		readonly MessageHeaderProviderHelper helper;

		public IParty Carrier => CachedValueHelper.GetValue(ref carrier, () => PartyProvider.NewOrNull(manifestHeader.Carrier));
		CachedValue<IParty> carrier;

		public IConsignmentHouseLevel ConsignmentHouseLevel => CachedValueHelper.GetValue(ref consignmentHouseLevel, () => SendAndAmend13ConsignmentHouseLevelProvider.NewOrNull(manifestHeader.Bills.Cast<AsycudaBill>().FirstOrDefault()));
		CachedValue<IConsignmentHouseLevel> consignmentHouseLevel;

		public IUNLOCO PlaceOfLoading => CachedValueHelper.GetValue(ref placeOfLoading, () => new UNLOCOProvider(manifestHeader.AMA_RL_NKPortOfLoading, string.Empty));
		CachedValue<IUNLOCO> placeOfLoading;

		public IIdentifierTypePair TransportDocumentMasterLevel => helper.TransportDocument;

		public IUNLOCO PlaceOfUnloading => CachedValueHelper.GetValue(ref placeOfUnloading, () => new UNLOCOProvider(manifestHeader.AMA_RL_NKPortOfDischarge, string.Empty));
		CachedValue<IUNLOCO> placeOfUnloading;
	}
}
