#if DEBUG

using System;

namespace Enterprise.Accounting.GUI.Riba
{
	public partial class BackDatePostForm
	{
		public void CancelButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			CancelButton_Click(sender, e);
		}

		public void OKButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			OKButton_Click(sender, e);
		}
	}
}

#endif
