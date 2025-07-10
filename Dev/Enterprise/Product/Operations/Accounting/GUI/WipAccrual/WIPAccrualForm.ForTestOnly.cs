#if DEBUG

using System;

namespace Enterprise.Accounting.GUI.WipAccrual
{
	public abstract partial class WIPAccrualForm
	{
		public void OnPostButtonClick_ForTestOnly(object sender, EventArgs e)
		{
			OnPostButtonClick(sender, e);
		}
	}
}

#endif
