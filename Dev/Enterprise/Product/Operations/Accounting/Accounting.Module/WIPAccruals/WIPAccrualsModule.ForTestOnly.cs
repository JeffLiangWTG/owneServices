#if DEBUG

using System;

namespace Enterprise.Accounting.Module
{
	public partial class WIPAccrualsModule
	{
		public void HandlePrintAccountingJournal_ForTestOnly(object sender, EventArgs e)
		{
			HandlePrintAccountingJournal(sender, e);
		}
	}
}

#endif
