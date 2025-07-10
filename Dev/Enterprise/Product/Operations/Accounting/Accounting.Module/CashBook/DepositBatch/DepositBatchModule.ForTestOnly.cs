#if DEBUG

using System;

namespace Enterprise.Accounting.Module
{
	public partial class DepositBatchModule
	{
		public void HandlePrint_ForTestOnly(object sender, EventArgs e)
		{
			HandlePrint(sender, e);
		}
	}
}

#endif
