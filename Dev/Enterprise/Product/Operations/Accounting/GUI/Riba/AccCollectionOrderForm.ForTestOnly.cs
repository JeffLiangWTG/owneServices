#if DEBUG

using System;
using System.Windows.Forms;

namespace Enterprise.Accounting.GUI.Riba
{
	public partial class AccCollectionOrderForm
	{
		public void OnShown_ForTestOnly(EventArgs e)
		{
			OnShown(e);
		}

		public MenuItem ActionsMenuItem_ForTestOnly => ActionsMenuItem;
	}
}

#endif
