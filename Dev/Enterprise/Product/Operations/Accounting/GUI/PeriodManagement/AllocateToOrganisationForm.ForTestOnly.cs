#if DEBUG

using System;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class AllocateToOrganisationForm
	{
		public void AllocateButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			AllocateButton_Click(sender, e);
		}

		public Business.eNett.ComPayRegisteredOrganisation ComPayLine_ForTestOnly
		{
			get { return comPayLine; }
			set { comPayLine = value; }
		}
	}
}

#endif
