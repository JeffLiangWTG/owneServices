#if DEBUG

using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Module
{
	public partial class AccCollectionBatchModule
	{
		public MenuItem[] GetNewActionMenuItems_ForTestOnly()
		{
			return GetNewActionMenuItems();
		}

		public IBusinessObjectCollection GetNewGridCollection_ForTestOnly()
		{
			return GetNewGridCollection();
		}

		public void HandleCreateReceiptsAndOneDepositBatch_ForTestOnly(object sender, EventArgs e)
		{
			HandleCreateReceiptsAndOneDepositBatch(sender, e);
		}

		public void HandleCreateReceiptsAndIndividualDepositBatch_ForTestOnly(object sender, EventArgs e)
		{
			HandleCreateReceiptsAndIndividualDepositBatch(sender, e);
		}
	}
}

#endif