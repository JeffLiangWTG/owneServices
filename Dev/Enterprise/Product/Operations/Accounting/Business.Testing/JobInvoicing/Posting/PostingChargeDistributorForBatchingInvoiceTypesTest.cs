using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class PostingChargeDistributorForBatchingInvoiceTypesTest : PostingChargeDistributorTest
	{
		protected override string GetCorrectedInvoiceType(string invoiceType)
		{
			return InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(invoiceType);
		}
	}
}