#if DEBUG

using System;

namespace Enterprise.Accounting.GUI
{
	public partial class AlternateGLAccountsForm
	{
		public void OnShown_ForTestOnly(EventArgs e)
		{
			OnShown(e);
		}

		public void SaveToRecentItems_ForTestOnly()
		{
			SaveToRecentItems();
		}
	}
}

#endif
