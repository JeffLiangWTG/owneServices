using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class APPaymentApprovalMatching : PaymentApprovalMatchingBase
	{
		public APPaymentApprovalMatching(BusinessObjectFactory factory, PaymentApprovalBase paymentApprovalDetail)
			: base(factory, paymentApprovalDetail)
		{
		}

		protected override MiscellaneousTransactionCreator GetNewMiscellaneousTransactionCreator(BusinessObjectFactory factory)
		{
			return new MiscellaneousTransactionCreatorAP(factory);
		}

		public override MatchingFilterBusinessObject MatchingFilterBizO
		{
			get
			{
				if (matchingFilterBizO == null)
				{
					matchingFilterBizO = new APMatchingFilterBusinessObject();
				}

				return matchingFilterBizO;
			}
		}

		#region LedgerType

		protected override ZString LedgerTypeCore => LedgerTypes.AccountsPayable;

		#endregion

		public override CashAdvanceFilterBusinessObjectForMatchingBase CashAdvanceFilter => cashAdvanceFilter ?? (cashAdvanceFilter = new APCashAdvanceFilterBusinessObjectForMatchingBase(MatchingFilterBizO as APMatchingFilterBusinessObject));
		CashAdvanceFilterBusinessObjectForMatchingBase cashAdvanceFilter;

		protected override bool CanCashAdvanceRequestBeMatchedCore
		{
			get
			{
				var checker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
				return checker.IsPayablesCashAdvanceFunctionalityEnabled &&
					!checker.IsManualSettingOfPayablesCashAdvanceRequestStatusToPaidAllowed;
			}
		}
	}
}
