#if DEBUG

using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class ReopenPeriodForm
	{
		public void ReopenButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			ReopenButton_Click(sender, e);
		}

		public ZCheckBox ReopenSubLedgerPeriodCheckBox_ForTestOnly
		{
			get { return ReopenSubLedgerPeriodCheckBox; }
			set { ReopenSubLedgerPeriodCheckBox = value; }
		}

		public ZCheckBox ReopenGeneralLedgerPeriodCheckBox_ForTestOnly
		{
			get { return ReopenGeneralLedgerPeriodCheckBox; }
			set { ReopenGeneralLedgerPeriodCheckBox = value; }
		}

		public ZCheckBox ReopenForAdjustmentsCheckBox_ForTestOnly
		{
			get { return ReopenForAdjustmentsCheckBox; }
			set { ReopenForAdjustmentsCheckBox = value; }
		}
	}
}

#endif
