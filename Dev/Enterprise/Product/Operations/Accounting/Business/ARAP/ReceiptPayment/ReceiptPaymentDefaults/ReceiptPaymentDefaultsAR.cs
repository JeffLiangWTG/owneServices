using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class ReceiptPaymentDefaultsAR : ReceiptPaymentDefaults
	{
		public ReceiptPaymentDefaultsAR(TransactionHeader recPay)
			: base(recPay)
		{
		}

		public override ZGuid GetDefaultBankAccount()
		{
			return GetDefaultBankAccount(ReceiptPayment.Header);
		}

		public static ZGuid GetDefaultBankAccount(OrgHeader header)
		{
			AccBankAccount defaultBankAccount = null;
			if (header != null && header.CompanyData != null)
			{
				defaultBankAccount = AccBankAccount.GetDefaultReceiptBankAccountForDebtor(header.PK, header.CompanyData.OB_RX_NKARDDefltCurrency, GlbBranch.CurrentBranch, header.Factory);
			}
			return defaultBankAccount != null ? defaultBankAccount.PK : ZGuid.Empty;
		}
	}
}
