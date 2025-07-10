using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class ReceiptPaymentVoucherProvider : TransactionWithoutLinesVoucherProvider
	{
		public ReceiptPaymentVoucherProvider(AccTransactionHeader transaction)
			: base(transaction, new ControlAccountProvider())
		{
		}

		protected override ZGuid GetGLAccountPKFromTransactionHeader()
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
