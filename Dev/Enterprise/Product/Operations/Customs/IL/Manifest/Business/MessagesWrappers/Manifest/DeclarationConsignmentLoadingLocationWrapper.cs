using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentLoadingLocationWrapper : IDeclarationConsignmentLoadingLocation
	{
		DeclarationConsignmentLoadingLocationWrapper(ASYCUDA.Business.AsycudaBill masterBill)
		{
			this.asycudaBill = masterBill;
		}

		public static DeclarationConsignmentLoadingLocationWrapper NewOrNull(ASYCUDA.Business.AsycudaBill masterBill)
			=> masterBill != null
			? new DeclarationConsignmentLoadingLocationWrapper(masterBill)
			: null;

		public IIDType Id => IDTypeWrapper.NewOrNull(asycudaBill.ABL_RL_NKPortOfLoading);

		readonly ASYCUDA.Business.AsycudaBill asycudaBill;
	}
}
