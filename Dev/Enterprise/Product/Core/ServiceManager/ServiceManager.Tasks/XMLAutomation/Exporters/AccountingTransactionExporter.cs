using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer;
using Enterprise.DataTransfer.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class AccountingTransactionExporter : XmlAccountingTransactionExporter
	{
		public AccountingTransactionExporter(BusinessObjectFactory factory) : base(factory) { }

		protected override void ExportObjectsToEndPoint(BusinessObject bizObj, IValueObjectDataAdapter dataAdapter, ZString status)
		{
			base.ExportObjectsToEndPoint(bizObj, dataAdapter, status);
			Invoice invoice = bizObj as Invoice;
			CreditNote creditNote = bizObj as CreditNote;

			if (invoice != null)
			{
				if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					ExportedARInvoices.Add(invoice);
					totalARAmount += invoice.AH_InvoiceAmount + invoice.AH_GSTAmount;
				}
				else
					if (invoice.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					ExportedAPInvoices.Add(invoice);
					totalAPAmount += invoice.AH_InvoiceAmount + invoice.AH_GSTAmount;
				}
			}
			else if (creditNote != null)
			{
				if (creditNote.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					ExportedARCreditNote.Add(creditNote);
					totalARAmount += creditNote.AH_InvoiceAmount + creditNote.AH_GSTAmount;
				}
				else if (creditNote.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					ExportedAPCreditNote.Add(creditNote);
					totalAPAmount += creditNote.AH_InvoiceAmount + creditNote.AH_GSTAmount;
				}
			}
		}

		internal void ClearExportedLists()
		{
			ExportedARInvoices.Clear();
			ExportedAPInvoices.Clear();
			ExportedARCreditNote.Clear();
			ExportedAPCreditNote.Clear();
			totalAPAmount = 0;
			totalARAmount = 0;
		}

		List<InvoicingBase> exportedARInvoices;
		internal List<InvoicingBase> ExportedARInvoices
		{
			get { return exportedARInvoices ?? (exportedARInvoices = new List<InvoicingBase>()); }
		}

		List<InvoicingBase> exportedAPInvoices;
		internal List<InvoicingBase> ExportedAPInvoices
		{
			get { return exportedAPInvoices ?? (exportedAPInvoices = new List<InvoicingBase>()); }
		}

		List<InvoicingBase> exportedAPCreditNote;
		internal List<InvoicingBase> ExportedAPCreditNote
		{
			get { return exportedAPCreditNote ?? (exportedAPCreditNote = new List<InvoicingBase>()); }
		}

		List<InvoicingBase> exportedARCreditNote;
		internal List<InvoicingBase> ExportedARCreditNote
		{
			get { return exportedARCreditNote ?? (exportedARCreditNote = new List<InvoicingBase>()); }
		}

		ZDecimal totalARAmount;

		internal ZDecimal TotalARAmount
		{
			get { return totalARAmount; }
		}
		ZDecimal totalAPAmount;

		internal ZDecimal TotalAPAmount
		{
			get { return totalAPAmount; }
		}
	}
}
