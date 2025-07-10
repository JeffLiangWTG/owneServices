#if DEBUG

using System;

namespace Enterprise.Accounting.Module
{
	public partial class UnapprovedIntercompanyTransactionModule
	{
		public void ContextMenu_Popup_ForTestOnly(object sender, EventArgs e)
		{
			ContextMenu_Popup(sender, e);
		}
	}
}

#endif
