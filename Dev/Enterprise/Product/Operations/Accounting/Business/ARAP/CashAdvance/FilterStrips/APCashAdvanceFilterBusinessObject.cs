using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class APCashAdvanceFilterBusinessObject : CashAdvanceFilterBusinessObject
	{
		protected override ZString LedgerType
		{
			get { return LedgerTypes.AccountsPayable; }
		}
	}
}
