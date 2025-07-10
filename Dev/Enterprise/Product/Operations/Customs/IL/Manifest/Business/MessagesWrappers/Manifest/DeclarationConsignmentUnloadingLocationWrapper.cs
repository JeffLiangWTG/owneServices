using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentUnloadingLocationWrapper : IDeclarationConsignmentUnloadingLocation
	{
		DeclarationConsignmentUnloadingLocationWrapper(AsycudaBill asycudaBill)
		{
			this.asycudaBill = asycudaBill;
		}

		public static DeclarationConsignmentUnloadingLocationWrapper NewOrNull(AsycudaBill asycudaBill)
			=> asycudaBill != null ? new DeclarationConsignmentUnloadingLocationWrapper(asycudaBill) : null;

		public string ArrivalDateTime => null;

		public IIDType Id => IDTypeWrapper.NewOrNull(asycudaBill.ABL_RL_NKPortOfDischarge);

		readonly AsycudaBill asycudaBill;
	}
}
