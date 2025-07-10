using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class APPaymentBatchApprovalMatching : APPaymentApprovalMatching
	{
		public APPaymentBatchApprovalMatching(BusinessObjectFactory factory, PaymentApprovalBase paymentApprovalDetail)
			: base(factory, paymentApprovalDetail)
		{
		}

		public override MatchingFilterBusinessObject MatchingFilterBizO
		{
			get
			{
				if (matchingFilterBizO == null)
				{
					matchingFilterBizO = new APMatchingFilterBusinessObject();
					((APMatchingFilterBusinessObject)MatchingFilterBizO).SetFilterForDBReloadIsNoResultQuery();
				}

				return matchingFilterBizO;
			}
		}

		protected override ZDecimal BalanceCore
		{
			get
			{
				if (IsPaymentAddedToMatchedTransactions)
				{
					return base.BalanceCore;
				}
				else
				{
					return base.BalanceCore + PaymentApprovalDetail.AV_Calc_LocalAmount;
				}
			}
		}

		ZBool IsPaymentAddedToMatchedTransactions
		{
			get
			{
				return MatchedTransactions.Contains(PaymentApprovalDetail);
			}
		}

		protected override void ValidateZeroBalance()
		{
			if (!PaymentApprovalDetail.IsCancelled)
			{
				base.ValidateZeroBalance();
			}
		}

#if DEBUG
		public bool IsPaymentAddedToMatchedTransactions_ForTestOnly => IsPaymentAddedToMatchedTransactions;
#endif
	}
}
