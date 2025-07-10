using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentConsigneeWrapper : IDeclarationConsignmentConsignee
	{
		DeclarationConsignmentConsigneeWrapper(AsycudaBill asycudaBill)
		{
			this.asycudaBill = asycudaBill;
		}

		public static DeclarationConsignmentConsigneeWrapper NewOrNull(AsycudaBill asycudaBill)
			=> asycudaBill != null ? new DeclarationConsignmentConsigneeWrapper(asycudaBill) : null;

		public ICollection<IDeclarationConsignmentConsigneeAddress> Address
			=> new Collection<IDeclarationConsignmentConsigneeAddress>() { DeclarationConsignmentConsigneeAddressWrapper.NewOrNull(asycudaBill) };

		public ICollection<IManifestCommunication> Communication
			=> new Collection<IManifestCommunication>
			{
				ManifestPartnerPhoneCommunicationWrapper.NewOrNull(asycudaBill.ABL_ConsigneePhone)
			};

		public IIDType Id => IDTypeWrapper.NewOrNull(asycudaBill.ABL_ConsigneeRegNo);

		public ITextType Name => TextTypeWrapper.NewOrNull(asycudaBill.ABL_ConsigneeName);

		readonly AsycudaBill asycudaBill;
	}
}
