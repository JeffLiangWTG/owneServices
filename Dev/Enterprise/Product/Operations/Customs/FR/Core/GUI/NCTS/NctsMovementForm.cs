using System;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public partial class NctsMovementForm : EU.NCTS.GUI.NctsMovementForm
	{
		public NctsMovementForm(NctsHeader nctsMovement)
			: base(nctsMovement)
		{
			InitializeComponent();
		}

		protected override Type GetDeclarationDetailsUserControlType()
		{
			return typeof(DeclarationDetailsTabUserControl);
		}

		protected override Type GetArrivalNotificationUserControlType()
		{
			return typeof(NctsArrivalUserControl);
		}

		protected override Type GetGoodsItemUserControlType() => typeof(NctsGoodsItemsUserControl);

		protected override Type GetUnloadingRemarksUserControlType() => typeof(FRUnloadingRemarksUserControl);
	}
}
