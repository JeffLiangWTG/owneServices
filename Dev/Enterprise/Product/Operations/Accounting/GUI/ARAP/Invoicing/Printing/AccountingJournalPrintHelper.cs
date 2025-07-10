using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI
{
	public static class AccountingJournalPrintHelper
	{
		public static void PrintAccountingJournal(IEnumerable<TransactionHeader> transactionsToPrint)
		{
			PrintAccountingJournalCore(new AccoutningJournalWithHeaderPrinter(transactionsToPrint.Select(x => x.PK), null));
		}

		public static void PrintAccountingJournal(IEnumerable<BaseWIPAccrual> transactionsToPrint)
		{
			PrintAccountingJournalCore(new AccoutningJournalWithoutHeaderPrinter(transactionsToPrint.Select(x => x.PK), null));
		}

		public static void PrintAccountingJournal(IEnumerable<TransactionHeader> transactionsHeaderToPrint, IEnumerable<BaseWIPAccrual> transactionsLinesToPrint)
		{
			PrintAccountingJournalCore(new AccoutningJournalPrinterWithHeaderAndLines(transactionsHeaderToPrint.Select(x => x.PK), transactionsLinesToPrint.Select(x => x.PK), null));
		}

		public static void PrintAccountingJournalForReportingBook(IEnumerable<TransactionHeader> transactionsHeaderToPrint, IEnumerable<BaseWIPAccrual> transactionsLinesToPrint, List<ZGuid> additionalHeaderPKs, ZGuid reportingBookPK, ZDateTime startPostDate, ZDateTime endPostDate)
		{
			PrintAccountingJournalCore(new AccoutningJournalPrinterWithHeaderAndLines(transactionsHeaderToPrint.Select(x => x.PK), transactionsLinesToPrint.Select(x => x.PK), additionalHeaderPKs, null, reportingBookPK, startPostDate, endPostDate));
		}

		static void PrintAccountingJournalCore(AccountingJournalPrinter printer)
		{
			try
			{
				var message = printer.CanPrint();
				var isMessageNullOrEmpty = string.IsNullOrEmpty(message);
				if (isMessageNullOrEmpty && AskForConfirmation(printer))
				{
					printer.PrintDocuments();
				}
				else if (!isMessageNullOrEmpty)
				{
					Globals.Message.ShowError(message, PrintAccountingJournalText);
				}
			}
			catch (MissingGLHeaderException mex)
			{
				Globals.Message.ShowError(mex.Message, PrintAccountingJournalText);
			}
			catch (InvalidAccountingJournalOperationException nex)
			{
				Globals.Message.ShowError(nex.Message, PrintAccountingJournalText);
			}
		}

		static bool AskForConfirmation(AccountingJournalPrinter printer)
		{
			if (printer.NumberOfDocumentToPrint > 1)
			{
				if (Globals.Message.Show(Res.GetString("b4ac8d62-de14-4428-bdb1-83d77c532047", "There are {0} accounting journals to print. Do you want to proceed?", printer.NumberOfDocumentToPrint),
					PrintAccountingJournalText, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
				{
					return false;
				}
			}

			return true;
		}

		public static MultilingualString PrintAccountingJournalText
		{
			get { return ResString.GetMultilingualString("f5f7be91-52fb-4b43-ba52-35697dca1686", "Print Accounting Journal"); }
		}

		public static MultilingualString PrintAccountingJournalPromptText
		{
			get { return ResString.GetMultilingualString("1C671E76-1A80-47DA-B33A-BA0EB3B63B7A", "Please select transaction(s) or Journal(s) to print."); }
		}
	}
}
