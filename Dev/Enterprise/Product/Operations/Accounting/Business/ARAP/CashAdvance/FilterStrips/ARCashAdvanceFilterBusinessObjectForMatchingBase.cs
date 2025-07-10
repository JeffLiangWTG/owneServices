using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class ARCashAdvanceFilterBusinessObjectForMatchingBase : CashAdvanceFilterBusinessObjectForMatchingBase
	{
		public ARCashAdvanceFilterBusinessObjectForMatchingBase()
			: base()
		{
		}

		public ARCashAdvanceFilterBusinessObjectForMatchingBase(ARMatchingFilterBusinessObject matchingFilterBizO)
			: base(matchingFilterBizO)
		{
		}

		protected override ZString LedgerType => LedgerTypes.AccountsReceivable;

		protected override string LayoutContextName => "ARMatchingForm";
	}
}
