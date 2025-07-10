using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IQueueInvoiceForTransmissionProvider
	{
		bool ShouldShowQueueInvoiceForTransmissionMenuItem(string ledgerType);

		bool ShouldQueueTransactionForTransmission(TransactionHeader header);

		string GetAdditionalErrorMessage();
	}
}
