using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public interface IEPaymentFundingInfoProvider
	{
		ZGuid GetFundingBankAccount();
		ZString GetFundingCurrency();
		ZString GetOriginalFundingCurrency();
	}
}
