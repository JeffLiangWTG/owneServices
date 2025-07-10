using System.Windows.Forms;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI
{
	public static class B2LineAsAccountedAsClaimedHelper
	{
		public static void UnHookInvoiceLineEvent(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine != null)
			{
				invoiceLine.OnRefreshSIMAMeasureEvent = null;
			}
		}

		public static void HookInvoiceLineEvent(JobComInvoiceLine invoiceLine, UserControl userControl)
		{
			if (invoiceLine != null)
			{
				invoiceLine.OnRefreshSIMAMeasureEvent += delegate
				{
					var simaMeasuresForm = new SIMADumpingNumberForm(invoiceLine.SIMAMeasures, invoiceLine.JI_Tariff);
					var parentForm = userControl.FindForm();
					simaMeasuresForm.Icon = parentForm.Icon;
					if (simaMeasuresForm.ShowDialog(parentForm) == DialogResult.OK)
					{
						return simaMeasuresForm.SelectedDumpingNumber;
					}

					return null;
				};
			}
		}
	}
}
