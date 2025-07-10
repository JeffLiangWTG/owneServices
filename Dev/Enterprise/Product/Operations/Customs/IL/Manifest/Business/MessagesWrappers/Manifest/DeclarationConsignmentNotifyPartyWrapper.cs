using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentNotifyPartyWrapper : IDeclarationConsignmentNotifyParty
	{
		DeclarationConsignmentNotifyPartyWrapper(AsycudaBill asycudaBill)
		{
			this.asycudaBill = asycudaBill;
		}

		public static DeclarationConsignmentNotifyPartyWrapper NewOrNull(AsycudaBill asycudaBill)
			=> asycudaBill == null || asycudaBill.ABL_OA_NotifyParty.IsEmpty ? null : new DeclarationConsignmentNotifyPartyWrapper(asycudaBill);

		public ICollection<IDeclarationConsignmentNotifyPartyAddress> Address
			=> new Collection<IDeclarationConsignmentNotifyPartyAddress> { DeclarationConsignmentNotifyPartyAddressWrapper.NewOrNull(asycudaBill) };

		public ICollection<IManifestCommunication> Communication
			=> new Collection<IManifestCommunication>
			{
				ManifestPartnerPhoneCommunicationWrapper.NewOrNull(asycudaBill.ABL_NotifyPartyPhone)
			};

		public ITextType Name => TextTypeWrapper.NewOrNull(asycudaBill.ABL_NotifyPartyName);

		readonly AsycudaBill asycudaBill;
	}
}
