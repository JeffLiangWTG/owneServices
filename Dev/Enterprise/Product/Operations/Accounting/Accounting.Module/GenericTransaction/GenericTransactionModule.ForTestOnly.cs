#if DEBUG

using System;

namespace Enterprise.Accounting.Module
{
	public partial class GenericTransactionModule
	{
		public void HandleExportClick_ForTestOnly(object sender, EventArgs e) => HandleExportClick(sender, e);
	}
}

#endif
