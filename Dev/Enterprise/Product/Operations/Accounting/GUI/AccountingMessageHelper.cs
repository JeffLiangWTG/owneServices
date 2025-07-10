using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI
{
	class AccountingMessageHelper
	{
		public bool ShowOKCancelMessageReturnsCancel(string message, string caption)
		{
			bool result = false;
			if (!string.IsNullOrEmpty(message))
			{
				result = (Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel);
			}
			return result;
		}

		public bool ShowMessageIfChequeBookUsesSamePrinterReturnsCancel(AccChequeBook chequeBook)
		{
			bool result = false;
			if (chequeBook != null)
			{
				result = ShowOKCancelMessageReturnsCancel(chequeBook.MessageIfChequeBookUsesSamePrinter(), AccChequeBook.WarningSamePrinterMessageCaption);
			}
			return result;
		}

		public static bool ConfirmInvalidateFinalisedComplianceReport(string caption)
		{
			var invalidateComplianceReportMessage = (NoResString)"You are going to change the status of Compliance Report that uses the GLD data from \"FIN - Report Finalised\" to \"INV - Report Invalidated by Transactions Updated\"";
			var enterConfirmMessage = (NoResString)"I am invalidating finalised compliance reports that uses the regenerated GLD data.";
			var invalidateComplianceReportConfirmResult = Globals.Message.ShowConfirmation(invalidateComplianceReportMessage, caption, enterConfirmMessage, MessageBoxIcon.Warning, MessageBoxButtons.OKCancel);

			return invalidateComplianceReportConfirmResult == DialogResult.OK;
		}
	}
}
