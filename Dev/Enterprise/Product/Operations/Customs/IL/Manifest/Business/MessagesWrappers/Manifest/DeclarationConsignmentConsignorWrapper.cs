using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentConsignorWrapper : IDeclarationConsignmentConsignor
	{
		DeclarationConsignmentConsignorWrapper(AsycudaBill asycudaBill)
		{
			this.asycudaBill = asycudaBill;
		}

		public static DeclarationConsignmentConsignorWrapper NewOrNull(AsycudaBill asycudaBill)
			=> asycudaBill != null ? new DeclarationConsignmentConsignorWrapper(asycudaBill) : null;

		public ICollection<IDeclarationConsignmentConsignorAddress> Address
			=> new Collection<IDeclarationConsignmentConsignorAddress>() { DeclarationConsignmentConsignorAddressWrapper.NewOrNull(asycudaBill) };

		public ICollection<IManifestCommunication> Communication
			=> new Collection<IManifestCommunication>
			{
				ManifestPartnerPhoneCommunicationWrapper.NewOrNull(asycudaBill.ABL_ShipperPhone)
			};

		public IIDType Id
			=> asycudaBill.Header.AMA_TransportMode == TransportModes.Road
				? IDTypeWrapper.NewOrNull(asycudaBill.ABL_ShipperRegNo)
				: null;

		public ITextType Name => TextTypeWrapper.NewOrNull(asycudaBill.ABL_ShipperName);

		readonly AsycudaBill asycudaBill;
	}
}
