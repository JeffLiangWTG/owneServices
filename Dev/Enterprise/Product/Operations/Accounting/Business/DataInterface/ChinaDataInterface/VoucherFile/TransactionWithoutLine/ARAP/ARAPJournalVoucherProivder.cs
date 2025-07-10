using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class ARAPJournalVoucherProivder : TransactionWithoutLinesVoucherProvider
	{
		public ARAPJournalVoucherProivder(AccTransactionHeader transaction)
			: base(transaction, new ControlAccountProvider())
		{
		}

		protected override ZGuid GetGLAccountPKFromTransactionHeader()
		{
			ZGuid accountPK = ZGuid.Empty;
			if (Transaction.GLHeader != null)
			{
				accountPK = Transaction.GLHeader.PK;
			}
			return accountPK;
		}
	}
}
