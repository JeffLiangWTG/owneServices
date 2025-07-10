using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class PaymentBatchFundingInfoProvider : IEPaymentFundingInfoProvider
	{
		public PaymentBatchFundingInfoProvider(AccPaymentBatch paymentBatch)
		{
			this.paymentBatch = paymentBatch;
		}

		public ZGuid GetFundingBankAccount()
		{
			if (paymentBatch == null || !paymentBatch.APB_AB_FundingBankAccount.IsValid)
			{
				return ZGuid.Empty;
			}

			return paymentBatch.FundingBankAccount.PK;
		}

		public ZString GetFundingCurrency()
		{
			return paymentBatch?.FundingBankAccount?.AB_RX_NKAccountCurrency ?? Env.CurrentCompany.LocalCurrency.Code;
		}

		public ZString GetOriginalFundingCurrency()
		{
			var originalFundingBankAccountPK = (ZGuid)(paymentBatch?.APB_AB_FundingBankAccountInfo?.OriginalValue ?? ZGuid.Empty);
			if (originalFundingBankAccountPK != ZGuid.Empty)
			{
				return paymentBatch.Factory.Load<AccBankAccount>(originalFundingBankAccountPK).AB_RX_NKAccountCurrency;
			}

			return Env.CurrentCompany.LocalCurrency.Code;
		}

		readonly AccPaymentBatch paymentBatch;
	}
}
