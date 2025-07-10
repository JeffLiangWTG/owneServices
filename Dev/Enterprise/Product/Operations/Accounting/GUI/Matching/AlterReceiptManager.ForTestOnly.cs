#if DEBUG

using System;

namespace Enterprise.Accounting.GUI
{
	public partial class AlterReceiptManager
	{
		public Business.ARAP.ReceiptPayment.Receipt PrepareReceipt_ForTestOnly()
		{
			return PrepareReceipt();
		}

		public void AlterReceiptFormClosed_ForTestOnly(object sender, EventArgs e)
		{
			AlterReceiptFormClosed(sender, e);
		}

		public Business.Base.Matching.MatchingBase FMatchingBase_ForTestOnly
		{
			get { return fMatchingBase; }
			set { fMatchingBase = value; }
		}
	}
}

#endif
