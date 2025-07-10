using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class ARMatchingBase : MatchingBase
	{
		public ARMatchingBase(BusinessObjectFactory factory, bool isLoadedFromGUI = true)
			: base(factory, isLoadedFromGUI)
		{
		}

		public ARMatchingBase(BusinessObjectFactory factory, ReceiptPaymentBase receiptPaymentDetail)
			: base(factory, receiptPaymentDetail)
		{
		}

		public ARMatchingBase(BusinessObjectFactory factory, ReceiptPaymentBase receiptPaymentDetail, bool isLoadedFromGUI)
			: base(factory, receiptPaymentDetail, isLoadedFromGUI)
		{
		}

		public ARMatchingBase(BusinessObjectFactory factory, Journal journal, bool isLoadFromGUI)
			: base(factory, journal, isLoadFromGUI)
		{
		}

		#region Matching Overrides

		protected override MiscellaneousTransactionCreator GetNewMiscellaneousTransactionCreator(BusinessObjectFactory factory)
		{
			return new MiscellaneousTransactionCreatorAR(factory);
		}

		public override MatchingFilterBusinessObject MatchingFilterBizO {
			get
			{
				if (matchingFilterBizO == null)
				{
					matchingFilterBizO = new ARMatchingFilterBusinessObject();
				}

				return matchingFilterBizO;
			}
		}

		#endregion

		#region Show Related Disbursement Transactions

		public override bool ShouldShowRelatedDisbursementTransactions => AccountingUtils.ShouldShowRelatedDisbursementTransactions();

		#endregion

		#region Cash Advance

		public override CashAdvanceFilterBusinessObjectForMatchingBase CashAdvanceFilter => cashAdvanceFilter ?? (cashAdvanceFilter = new ARCashAdvanceFilterBusinessObjectForMatchingBase(MatchingFilterBizO as ARMatchingFilterBusinessObject));
		CashAdvanceFilterBusinessObjectForMatchingBase cashAdvanceFilter;

		protected override bool CanCashAdvanceRequestBeMatchedCore
		{
			get
			{
				var checker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
				return checker.IsReceivablesCashAdvanceFunctionalityEnabled &&
					!checker.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed;
			}
		}

		#region LedgerType

		protected override ZString LedgerTypeCore => LedgerTypes.AccountsReceivable;

		#endregion

		#endregion
	}
}
