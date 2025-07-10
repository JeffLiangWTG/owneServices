using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class PaymentApprovalFundingInfoProvider : IEPaymentFundingInfoProvider
	{
		public PaymentApprovalFundingInfoProvider(AccPaymentApproval paymentApproval)
		{
			this.paymentApproval = paymentApproval;
		}

		public ZGuid GetFundingBankAccount()
		{
			if (paymentApproval == null || !paymentApproval.AV_AB_FundingBankAccount.IsValid)
			{
				return ZGuid.Empty;
			}

			return paymentApproval.AV_AB_FundingBankAccount;
		}

		public ZString GetFundingCurrency()
		{
			return paymentApproval?.FundingBankAccount?.AB_RX_NKAccountCurrency ?? Env.CurrentCompany.LocalCurrency.Code;
		}

		public ZString GetOriginalFundingCurrency()
		{
			var originalFundingBankAccountPK = (ZGuid)(paymentApproval?.AV_AB_FundingBankAccountInfo?.OriginalValue ?? ZGuid.Empty);
			if (originalFundingBankAccountPK != ZGuid.Empty)
			{
				return paymentApproval.Factory.Load<AccBankAccount>(originalFundingBankAccountPK).AB_RX_NKAccountCurrency;
			}

			return Env.CurrentCompany.LocalCurrency.Code;
		}

		readonly AccPaymentApproval paymentApproval;
	}
}
