using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	class InvoicePrintingUserControlHelper
	{
		public static void OverrideTransactionDescription(ZGrid parentGrid)
		{
			if (parentGrid.SelectedElements.Length > 0)
			{
				OverrideTransactionDescriptionHelper helper = new OverrideTransactionDescriptionHelper(new BusinessObjectFactory(), parentGrid.SelectedElements.Select(bizo => bizo.PK).ToArray());
				ZFormModaliser.Show(new OverrideTransactionDescriptionForm(helper), parentGrid.FindForm());
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}
		public static void ShowNoSelectedMessage()
		{
			Globals.Message.Show(Res.GetString("86d31d6c-d32c-491d-9960-ac1d408e7ec6", "Please select a record in the grid."), Res.GetString("d9043649-9cc2-415c-b71a-1c1286ad0b74", "No record selected..."), MessageBoxButtons.OK, MessageBoxIcon.Information, DialogResult.OK);
		}
	}
}

