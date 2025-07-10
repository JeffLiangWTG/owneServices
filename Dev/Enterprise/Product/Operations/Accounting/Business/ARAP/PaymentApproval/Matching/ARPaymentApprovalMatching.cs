using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class ARPaymentApprovalMatching : PaymentApprovalMatchingBase
	{
		public ARPaymentApprovalMatching(BusinessObjectFactory factory, PaymentApprovalBase paymentApprovalDetail)
			: base(factory, paymentApprovalDetail)
		{
		}

		protected override MiscellaneousTransactionCreator GetNewMiscellaneousTransactionCreator(BusinessObjectFactory factory)
		{
			return new MiscellaneousTransactionCreatorAR(factory);
		}

		public override MatchingFilterBusinessObject MatchingFilterBizO
		{
			get
			{
				if (matchingFilterBizO == null)
				{
					matchingFilterBizO = new ARMatchingFilterBusinessObject();
				}

				return matchingFilterBizO;
			}
		}

		#region LedgerType

		protected override ZString LedgerTypeCore => ZArchitecture.Core.LedgerTypes.AccountsReceivable;

		#endregion

		#region Show Related Disbursement Transactions

		public override bool ShouldShowRelatedDisbursementTransactions => AccountingUtils.ShouldShowRelatedDisbursementTransactions();

		#endregion
	}
}
