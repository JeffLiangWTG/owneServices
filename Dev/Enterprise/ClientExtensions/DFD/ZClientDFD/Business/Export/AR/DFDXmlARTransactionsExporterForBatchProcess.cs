
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices;

namespace Enterprise.Client.DFD.Export
{
	public class DFDXmlARTransactionsExporterForBatchProcess : DFDXmlARTransactionsExporter
	{
		public DFDXmlARTransactionsExporterForBatchProcess(BusinessObjectFactory factory, ZDateTime lastRun, ZDateTime nextRun)
			: base(factory)
		{
			this.LastRun = (lastRun.IsValid && !lastRun.IsEmpty) ? lastRun.AddDays(-1) : nextRun.AddDays(-1);
			this.NextRun = nextRun.AddDays(1);
		}

		public override FinancialInvoiceTransactionExportFilter InvoiceBatchFilter
		{
			get
			{
				if (fInvoiceBatchFilter == null)
				{
					fInvoiceBatchFilter = new DFDFinancialInvoiceTransactionExportFilterForBatchProcess(Factory, FilterProvider, LastRun, NextRun);
				}
				return fInvoiceBatchFilter;
			}
		}

		public override bool IsHighWaterMarkEnabled
		{
			get { return false; }
		}

		readonly internal ZDateTime LastRun;
		readonly internal ZDateTime NextRun;

		DFDFinancialInvoiceTransactionExportFilter fInvoiceBatchFilter;
	}
}
