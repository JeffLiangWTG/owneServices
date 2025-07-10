#if DEBUG

using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing
{
	public partial class InvoicePrintTask
	{
		public bool TestAboveMaxPreviewCount_ForTestOnly
		{
			get { return TestAboveMaxPreviewCount; }
			set { TestAboveMaxPreviewCount = value; }
		}

		public void AddInvoiceToPackCore_ForTestOnly(InvoicingBase invoice, List<DocumentPack> packs, params ZString[] nameOfMenus)
		{
			AddInvoiceToPackCore(invoice, packs, nameOfMenus);
		}

		public void AddInvoices_ForTestOnly(params Base.Transaction.TransactionHeader[] transactions)
		{
			AddInvoices(transactions);
		}

		public DocumentPrintSet Task_ForTestOnly
		{
			get { return Task; }
			set { Task = value; }
		}

		public bool IsLegacyDocument_ForTestOnly => isLegacyDocument;

		public void UpdateTransactionsAsPrinted_ForTestOnly()
		{
			UpdateTransactionsAsPrinted();
		}

		public Base.Transaction.TransactionHeader[] Transactions_ForTestOnly(ZQuery filter, ZGuid[] invoicesOrder)
		{
			return Transactions(filter, invoicesOrder);
		}

		public Base.Transaction.TransactionHeader[] TransactionsToPrint_ForTestOnly => transactionsToPrint;

		public DocumentCommand GetEntryPrintCommand_ForTestOnly(JobHeader job)
		{
			return GetEntryPrintCommand(job);
		}

		public List<DocumentPack> CreatePacks_ForTestOnly(InvoicingBase invoice, ZString menuName)
		{
			return CreatePacks(invoice, menuName);
		}

		public static ZQuery FilterForInvoices_ForTestOnly(BusinessObject[] invoices)
		{
			return FilterForInvoices(invoices);
		}
	}
}

#endif
