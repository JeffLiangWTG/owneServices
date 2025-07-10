using System;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public partial class NctsGoodsItemsUserControl : EU.NCTS.GUI.NctsGoodsItemsUserControl
	{
		public NctsGoodsItemsUserControl()
		{
			InitializeComponent();
		}

		protected override Type GetPreviousDocumentsUserControlType() => typeof(NctsPreviousDocumentsUserControl);

		protected override Type GetGoodsItemContainersUserControl() => typeof(NctsGoodsItemContainersUserControl);
	}
}
