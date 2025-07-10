using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class APMatchingBase : MatchingBase
	{
		public APMatchingBase(BusinessObjectFactory factory, bool isLoadedFromGUI = true)
			: base(factory, isLoadedFromGUI)
		{
		}

		public APMatchingBase(BusinessObjectFactory factory, ReceiptPaymentBase receiptPaymentDetail)
			: base(factory, receiptPaymentDetail)
		{
		}

		public APMatchingBase(BusinessObjectFactory factory, ReceiptPaymentBase receiptPaymentDetail, bool isLoadFromGUI)
			: base(factory, receiptPaymentDetail, isLoadFromGUI)
		{
		}

		public APMatchingBase(BusinessObjectFactory factory, Journal journal, bool isLoadFromGUI)
			: base(factory, journal, isLoadFromGUI)
		{
		}

		#region Matching Overrides

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

		#endregion

		#region Cash Advance

		protected override bool CanCashAdvanceRequestBeMatchedCore
		{
			get
			{
				var checker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
				return checker.IsPayablesCashAdvanceFunctionalityEnabled &&
					!checker.IsManualSettingOfPayablesCashAdvanceRequestStatusToPaidAllowed;
			}
		}

		#endregion
	}
}
