using System.Windows.Forms;
using Enterprise.Accounting.Business.AccountingVoucherPrint;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI
{
	public class AccountingVoucherPrintHelper
	{
		public PrintTask GetAccountingVoucherPrintTask(TransactionHeader[] transactionsToPrint)
		{
			var task = (PrintTask)null;
			var voucherPrintWrapper = new AccountingVoucherPrintWrapper();
			var voucherDocWrappers = voucherPrintWrapper.GenerateDocWrapper(transactionsToPrint, true);
			if (CanPrintAccountingVoucher(voucherDocWrappers))
			{
				task = voucherPrintWrapper.GetAccountingVoucherPrintTask(transactionsToPrint);
			}
			return task;
		}

		bool CanPrintAccountingVoucher(DocumentWrapper[] voucherDocWrappers)
		{
			if (voucherDocWrappers.Length == 0)
			{
				Globals.Message.ShowInformation(Res.GetString("B2556CDA-D576-4C2E-A860-0C37C035685B", "There are no accounting vouchers within given selection criteria."));
				return false;
			}
			if (!Globals.IsTest && voucherDocWrappers.Length > 1)
			{
				if (Globals.Message.Show(Res.GetString("78208B93-CA7C-462F-A6C3-255D32BC4815", "There are {0} accounting vouchers to print. Do you want to proceed?", voucherDocWrappers.Length),
					Res.GetString("13642222-418E-4494-976C-9F04BE72A305", "Print Account Voucher"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
				{
					return false;
				}
			}

			return true;
		}
	}
}
