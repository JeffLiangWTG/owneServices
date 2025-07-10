using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.Rohlig.Bellin
{
	public class BellinDataExporter : FlatFileAccountingTransactionExporter
	{
		public BellinDataExporter(BusinessObjectFactory factory) : base(factory)
		{
			FilterProvider.IncludeAccrualsPosting = false;
			FilterProvider.IncludeAccrualsReversing = false;
			FilterProvider.IncludeWIPsPosting = false;
			FilterProvider.IncludeWIPsReversing = false;

			FilterProvider.IncludeAPAdjustmentNotes = true;
			FilterProvider.IncludeAPCreditNotes = true;
			FilterProvider.IncludeAPInvoices = true;

			FilterProvider.IncludeARAdjustmentNotes = false;
			FilterProvider.IncludeARCreditNotes = false;
			FilterProvider.IncludeARInvoices = false;

			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = false;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = false;

			FilterProvider.CurrentBatchNo = 0;
		}

		#region Converter

		protected override AccountingFlatFileConverter Converter
		{
			get
			{
				if (fConverter == null)
				{
					fConverter = new BellinFlatFileConverter(Notify, Factory);
				}
				return fConverter;
			}
		}

		AccountingFlatFileConverter fConverter;

		#endregion

		#region Format

		protected override FlatFileFormat Format
		{
			get
			{
				if (fFormat == null)
				{
					fFormat = new CsvFlatFileFormat(false);
				}
				return fFormat;
			}
		}

		FlatFileFormat fFormat;

		#endregion

		#region InvoiceBatchFilter

		public override FinancialInvoiceTransactionExportFilter InvoiceBatchFilter
		{
			get
			{
				if (BillinFilter == null)
				{
					BillinFilter = new BellinInvoiceBatchFilter(Factory, FilterProvider);
				}
				return BillinFilter;
			}
		}
		BellinInvoiceBatchFilter BillinFilter;

		#endregion
	}
}

#region Implementation
#endregion
