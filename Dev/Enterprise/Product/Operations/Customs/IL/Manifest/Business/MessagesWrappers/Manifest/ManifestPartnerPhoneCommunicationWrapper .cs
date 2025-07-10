using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class ManifestPartnerPhoneCommunicationWrapper : IManifestCommunication
	{
		ManifestPartnerPhoneCommunicationWrapper(ZString partnerPhone)
		{
			this.partnerPhone = partnerPhone;
		}

		public static ManifestPartnerPhoneCommunicationWrapper NewOrNull(ZString partnerPhone)
			=> partnerPhone.IsEmpty
			? null
			: new ManifestPartnerPhoneCommunicationWrapper(partnerPhone);

		public IIDType Id => IDTypeWrapper.NewOrNull(partnerPhone);

		public IIDType TypeId => IDTypeWrapper.NewOrNull(Constants.MessagesWrappers.CommunicationType.Phone);

		readonly ZString partnerPhone;
	}
}
