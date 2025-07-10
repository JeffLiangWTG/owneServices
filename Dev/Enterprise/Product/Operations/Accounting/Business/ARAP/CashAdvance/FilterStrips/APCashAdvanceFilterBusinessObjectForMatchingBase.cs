using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class APCashAdvanceFilterBusinessObjectForMatchingBase : CashAdvanceFilterBusinessObjectForMatchingBase
	{
		public APCashAdvanceFilterBusinessObjectForMatchingBase()
			: base()
		{
		}

		public APCashAdvanceFilterBusinessObjectForMatchingBase(APMatchingFilterBusinessObject matchingFilterBizO)
			: base(matchingFilterBizO)
		{
		}

		protected override ZString LedgerType => LedgerTypes.AccountsPayable;

		protected override string LayoutContextName => "APMatchingForm";
	}
}
