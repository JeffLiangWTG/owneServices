#if DEBUG

using System;
using System.Windows.Forms;

namespace Enterprise.Accounting.Module
{
	public partial class AccCollectionOrderModule
	{
		public MenuItem[] GetNewAdditionalMenuItems_ForTestOnly()
		{
			return GetNewAdditionalMenuItems();
		}

		public MenuItem[] GetNewActionMenuItems_ForTestOnly()
		{
			return GetNewActionMenuItems();
		}

		public void HandleRejectOrder_ForTestOnly(object sender, EventArgs e)
		{
			HandleRejectOrder(sender, e);
		}

		public void HandleCreateReceiptsAndIndividualDepositBatch_ForTestOnly(object sender, EventArgs e)
		{
			HandleCreateReceiptsAndIndividualDepositBatch(sender, e);
		}
	}
}

#endif