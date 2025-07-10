
using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.STI.Navision
{
	public abstract class NavisionAccFlatFileExporter : FlatFileAccountingTransactionExporter
	{
		public NavisionAccFlatFileExporter(BusinessObjectFactory factory) : base(factory)
		{
			FilterProvider.IncludeARInvoices = true;
			FilterProvider.IncludeARCreditNotes = true;
			FilterProvider.IncludeARAdjustmentNotes = false;

			FilterProvider.IncludeAPInvoices = false;
			FilterProvider.IncludeAPCreditNotes = false;
			FilterProvider.IncludeAPAdjustmentNotes = false;

			FilterProvider.ExcludeJobRelatedTransactionsForAP = false;
			FilterProvider.ExcludeJobRelatedTransactionsForAR = false;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = false;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = false;

			FilterProvider.IncludeAccrualsPosting = false;
			FilterProvider.IncludeAccrualsReversing = false;
			FilterProvider.IncludeWIPsPosting = false;
			FilterProvider.IncludeWIPsReversing = false;

			FilterProvider.CurrentBatchNo = 0;
		}

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
	}
}
