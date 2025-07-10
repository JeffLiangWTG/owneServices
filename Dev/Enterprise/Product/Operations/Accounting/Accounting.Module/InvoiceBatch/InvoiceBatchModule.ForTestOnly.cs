#if DEBUG

using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Module
{
	public partial class InvoiceBatchModule
	{
		public MenuItem[] GetNewStandardMenuItems_ForTestOnly()
		{
			return GetNewStandardMenuItems();
		}

		public MultilingualString NewBatchText_ForTestOnly => NewBatchText;

		public MultilingualString NewBulkBatchText_ForTestOnly => NewBulkBatchText;

		public MultilingualString PrintMenuText_ForTestOnly => PrintMenuText;

		public void Print_ForTestOnly(Business.ARAP.Invoicing.InvoiceBatchHeader batchHeader)
		{
			Print(batchHeader);
		}
	}
}

#endif
