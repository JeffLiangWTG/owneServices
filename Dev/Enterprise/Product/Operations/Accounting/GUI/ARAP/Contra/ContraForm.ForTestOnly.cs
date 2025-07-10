#if DEBUG

using System;

namespace Enterprise.Accounting.GUI.ARAP.Contra
{
	public partial class ContraForm
	{
		public void OnShown_ForTestOnly(EventArgs e)
		{
			OnShown(e);
		}
	}
}

#endif
