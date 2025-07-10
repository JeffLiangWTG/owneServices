using CargoWise.Schema;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class UnallocatedTransactionExportFilterTest : TransactionExportFilterTestBase
	{
		protected override TransactionExportFilter GetNewExportFilter()
		{
			return new UnallocatedTransactionExportFilter(Factory, FilterProvider);
		}

		protected override int ExpectedNumberOfHighWaterMarkParams
		{
			get
			{
				return (FilterProvider.IncludeUnallocatedAPCreditNotes ? 1 : 0) + (FilterProvider.IncludeUnallocatedAPInvoices ? 1 : 0);
			}
		}

		protected override SchemaDateTimeColumn ExpectedSystemLastEditTimeColumn
		{
			get { return AccTransactionHeaderSchema.AH_SystemLastEditTimeUtc; }
		}

		protected override void AddTransactionsToDataBase()
		{
			unAllocatedInvoice = Factory.New<TransactionPendingAllocation>();
			unAllocatedInvoice.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			unAllocatedInvoice.AH_OSExTaxAmount = 100m;
			unAllocatedInvoice.AH_OSTaxAmount = 10m;
			unAllocatedInvoice.AH_Desc = "Description";
			unAllocatedInvoice.AH_TransactionNum = "ABC";

			unAllocatedCreditNote = Factory.New<TransactionPendingAllocation>();
			unAllocatedCreditNote.AH_OSExTaxAmount = -100m;
			unAllocatedCreditNote.AH_OSTaxAmount = -10m;
			unAllocatedCreditNote.AH_TransactionNum = "ABC";

			Factory.Save();
		}
	}
}
