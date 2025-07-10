using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class ReceiptPaymentDefaultsAP : ReceiptPaymentDefaults
	{
		public ReceiptPaymentDefaultsAP(TransactionHeader recPay)
			: base(recPay)
		{
		}

		public static ZGuid GetDefaultBankAccount(OrgHeader header)
		{
			ZGuid defaultBankAccount = ZGuid.Empty;
			if (header != null && header.CompanyData != null)
			{
				AccBankAccount bank = header.Factory.Load(typeof(AccBankAccount), header.CompanyData.OB_AB_APDefaultBankAccount) as AccBankAccount;

				if (bank != null && bank.AB_GC == GlbCompany.CurrentCompany.PK)
				{
					defaultBankAccount = header.CompanyData.OB_AB_APDefaultBankAccount;
				}
			}
			return defaultBankAccount;
		}

		public override ZGuid GetDefaultBankAccount()
		{
			return GetDefaultBankAccount(ReceiptPayment.Header);
		}
	}
}
