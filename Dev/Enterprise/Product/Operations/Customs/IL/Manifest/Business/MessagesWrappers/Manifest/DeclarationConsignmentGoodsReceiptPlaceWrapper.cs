using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentGoodsReceiptPlaceWrapper : IDeclarationConsignmentGoodsReceiptPlace
	{
		DeclarationConsignmentGoodsReceiptPlaceWrapper(AsycudaBill asycudaBill)
		{
			this.asycudaBill = asycudaBill;
		}

		public static DeclarationConsignmentGoodsReceiptPlaceWrapper NewOrNull(AsycudaBill asycudaBill)
			=> asycudaBill != null ? new DeclarationConsignmentGoodsReceiptPlaceWrapper(asycudaBill) : null;

		public IIDType Id => IDTypeWrapper.NewOrNull(asycudaBill.Header.Consol?.JK_RL_NKDischargePort ?? ZString.Empty);

		readonly AsycudaBill asycudaBill;
	}
}
