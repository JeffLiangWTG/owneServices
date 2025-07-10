using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentAcceptancePlaceWrapper : IDeclarationConsignmentAcceptancePlace
	{
		DeclarationConsignmentAcceptancePlaceWrapper(AsycudaBill asycudaBill)
		{
			this.asycudaBill = asycudaBill;
		}

		public static DeclarationConsignmentAcceptancePlaceWrapper NewOrNull(AsycudaBill asycudaBill)
			=> asycudaBill != null ? new DeclarationConsignmentAcceptancePlaceWrapper(asycudaBill) : null;

		public ITextType Name => TextTypeWrapper.NewOrNull(asycudaBill.ABL_RL_NKOrigin);

		readonly AsycudaBill asycudaBill;
	}
}
