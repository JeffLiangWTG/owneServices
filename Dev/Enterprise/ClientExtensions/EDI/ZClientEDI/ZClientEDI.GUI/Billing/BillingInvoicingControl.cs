using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class BillingInvoicingControl : ZUserControl
	{
		public BillingInvoicingControl()
		{
			InitializeComponent();
			AddContextMenus();
		}

		void AddContextMenus()
		{
			depositGrid.ContextMenu.MenuItems.Add("Recalculate Balance", RecalculateBalance);
		}

		void RecalculateBalance(object sender, EventArgs e)
		{
			var previousCursor = Cursor.Current;

			try
			{
				Cursor.Current = Cursors.WaitCursor;

				if (DepositBalance.UpdateSafe(true))
				{
					(CurrentDataItem as LicenceCompany)?.DepositBalances?.Load();
					Globals.Message.ShowInformation("Recalculation succeeded.");
				}
				else
				{
					Globals.Message.ShowError("Recalculation failed.");
				}
			}
			finally
			{
				Cursor.Current = previousCursor;
			}
		}
	}
}

