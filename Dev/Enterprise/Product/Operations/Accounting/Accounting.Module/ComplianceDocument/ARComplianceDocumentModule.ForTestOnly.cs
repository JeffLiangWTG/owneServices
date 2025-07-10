#if DEBUG

using System;
using System.Windows.Forms;

namespace Enterprise.Accounting.Module
{
	public partial class ARComplianceDocumentModule
	{
		public MenuItem[] GetNewActionMenuItems_ForTestOnly()
		{
			return GetNewActionMenuItems();
		}

		public void HandleResetStatusToQueued_ForTestOnly(object sender, EventArgs e)
		{
			HandleResetStatusToQueued(sender, e);
		}
	}
}

#endif
