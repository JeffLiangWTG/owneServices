using CargoWise.Common;
using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public interface ITaxCoreEInvoiceChecker
	{
		bool CheckAndApplyAuthorisation(AccEInvoicingBatch invoiceBatch, TaxCoreEInvoiceResponse response);
	}

	public class TaxCoreEInvoiceChecker : ITaxCoreEInvoiceChecker
	{
		bool ITaxCoreEInvoiceChecker.CheckAndApplyAuthorisation(AccEInvoicingBatch invoiceBatch, TaxCoreEInvoiceResponse response) => CheckAndApplyAuthorisation(invoiceBatch, response);

		protected virtual bool CheckAndApplyAuthorisation(AccEInvoicingBatch invoiceBatch, TaxCoreEInvoiceResponse response)
		{
			Argument.NotNull(invoiceBatch, nameof(invoiceBatch));
			Argument.NotNull(response, nameof(response));

			if (!string.IsNullOrEmpty(response.InvoiceNumber) && invoiceBatch.AIB_GovernmentAllocatedNumber.IsEmpty)
			{
				invoiceBatch.AIB_GovernmentAllocatedNumber = response.InvoiceNumber;
				return true;
			}

			return false;
		}
	}
}
