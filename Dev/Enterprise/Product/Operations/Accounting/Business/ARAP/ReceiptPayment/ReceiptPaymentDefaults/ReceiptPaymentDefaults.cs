using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public abstract class ReceiptPaymentDefaults
	{
		public ReceiptPaymentDefaults(TransactionHeader receiptPayment)
		{
			fReceiptPayment = receiptPayment;
		}

		public abstract ZGuid GetDefaultBankAccount();

		public TransactionHeader ReceiptPayment
		{
			get { return fReceiptPayment; }
		}

		readonly TransactionHeader fReceiptPayment;
	}
}
