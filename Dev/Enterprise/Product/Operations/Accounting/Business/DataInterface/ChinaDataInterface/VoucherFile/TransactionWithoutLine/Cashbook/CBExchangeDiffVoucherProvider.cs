using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class CBExchangeDiffVoucherProvider : TransactionWithoutLinesVoucherProvider
	{
		public CBExchangeDiffVoucherProvider(AccTransactionHeader transaction)
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

		protected override ZDecimal GetExchangeRate()
		{
			return 1m;
		}

		protected override ZString GetCurrencyCode()
		{
			return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		protected override ZDecimal GetOSCreditAmount()
		{
			return base.GetCreditAmount();
		}

		protected override ZDecimal GetOSDebitAmount()
		{
			return base.GetDebitAmount();
		}

		protected override ZDecimal GetControlOSCreditAmount()
		{
			return base.GetControlCreditAmount();
		}

		protected override ZDecimal GetControlOSDebitAmount()
		{
			return base.GetControlDebitAmount();
		}
	}
}
