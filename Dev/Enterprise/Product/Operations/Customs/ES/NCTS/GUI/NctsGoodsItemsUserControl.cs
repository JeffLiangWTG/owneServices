using System;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class NctsGoodsItemsUserControl : EU.NCTS.GUI.NctsGoodsItemsUserControl
	{
		public NctsGoodsItemsUserControl()
		{
			InitializeComponent();
		}

		#region overrides

		protected override Type GetPreviousDocumentsUserControlType() => typeof(NctsPreviousDocumentsUserControl);

		protected override Type GetItemDetailsUserControlType() => typeof(ItemDetailsUserControl);

		#endregion
	}
}
