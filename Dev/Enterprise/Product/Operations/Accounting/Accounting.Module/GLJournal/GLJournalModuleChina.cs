using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class GLJournalModuleChina : GLJournalModule
	{
		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			MenuItem printAccountingVoucherMenuItem = new ZMenuItem(PrintAccountingVoucherMenuItemText, new EventHandler(HandlePrintAccountingVoucher));
			menuItems.Add(printAccountingVoucherMenuItem);
			return menuItems.ToArray();
		}

		protected MultilingualString PrintAccountingVoucherMenuItemText
		{
			get { return ResString.GetMultilingualString("19cb808e-5578-465f-b9e0-cb0ab16a6791", "Print Accounting Voucher"); }
		}

		protected void HandlePrintAccountingVoucher(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(Res.GetString("D625B787-019A-4DA9-80A4-EF1BD68372CA", "Please select transaction(s) to print"));
			}
			else
			{
				var transactionHeader = Grid.GetSelectedElements<TransactionHeader>();
				if (transactionHeader.Any(x => x.AH_TransactionType == TransactionTypes.GLNoteJournal))
				{
					Globals.Message.Show(Res.GetString("5df08ae5-a94e-4d07-9816-be480b0fb498", "Accounting Voucher cannot be printed for 'NJL' General Ledger Journal"));
				}
				else
				{
					AccountingVoucherPrintHelper accountingVoucherPrintHelper = new AccountingVoucherPrintHelper();
					PrintTask task = accountingVoucherPrintHelper.GetAccountingVoucherPrintTask(transactionHeader);
					if (task != null)
					{
						task.Run(Env.Security.None);
					}
				}
			}
		}
	}
}
