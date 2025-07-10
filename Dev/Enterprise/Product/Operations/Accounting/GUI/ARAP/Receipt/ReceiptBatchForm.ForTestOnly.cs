#if DEBUG

using System.Windows.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class ReceiptBatchForm
	{
		public void MatchReceipts_ForTestOnly()
		{
			MatchReceipts();
		}

		public ZGrid ReceiptBatchGrid_ForTestOnly
		{
			get { return ReceiptBatchGrid; }
			set { ReceiptBatchGrid = value; }
		}

		public MenuItem DeleteMenuItem_ForTestOnly => DeleteMenuItem;
	}
}

#endif
