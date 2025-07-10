
using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.NZP.CMS
{
	public abstract class CMSDataExporter : FlatFileAccountingTransactionExporter
	{
		public CMSDataExporter(int batchNumber, BusinessObjectFactory factory) : base(factory)
		{
			SetFilterProvider(batchNumber);
		}

		#region SetFilterProvider

		void SetFilterProvider(int batchNumber)
		{
			FilterProvider.IncludeAccrualsPosting = false;
			FilterProvider.IncludeAccrualsReversing = false;
			FilterProvider.IncludeWIPsPosting = false;
			FilterProvider.IncludeWIPsReversing = false;

			FilterProvider.IncludeAPAdjustmentNotes = false;
			FilterProvider.IncludeAPCreditNotes = false;
			FilterProvider.IncludeAPInvoices = false;

			FilterProvider.IncludeARAdjustmentNotes = true;
			FilterProvider.IncludeARCreditNotes = true;
			FilterProvider.IncludeARInvoices = true;

			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = false;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = false;

			FilterProvider.CurrentBatchNo = batchNumber;
		}

		#endregion

		#region Format

		protected override FlatFileFormat Format
		{
			get
			{
				if (fFormat == null)
				{
					fFormat = new CMSFlatFileFormat();
				}
				return fFormat;
			}
		}
		PipeDelimitedFlatFileFormat fFormat;

		#endregion
	}
}
