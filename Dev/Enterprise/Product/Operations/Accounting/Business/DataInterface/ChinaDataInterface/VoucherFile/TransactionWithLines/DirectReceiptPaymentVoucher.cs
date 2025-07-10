using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class DirectReceiptPaymentVoucher : TransactionWithLinesVoucherProvider
	{
		public DirectReceiptPaymentVoucher(AccTransactionHeader transactionHeader, IControlAccountProvider controlAccount)
			: base(transactionHeader, controlAccount)
		{
		}

		public DirectReceiptPaymentVoucher(AccTransactionHeader transactionHeader)
			: base(transactionHeader)
		{
		}

		protected override ZGuid GetGLAccountPKFromControlAccount()
		{
			ZGuid accountPK = ZGuid.Empty;
			if (Transaction.BankAccount != null)
			{
				accountPK = Transaction.BankAccount.GLHeader.PK;
			}
			return accountPK;
		}
	}
}
