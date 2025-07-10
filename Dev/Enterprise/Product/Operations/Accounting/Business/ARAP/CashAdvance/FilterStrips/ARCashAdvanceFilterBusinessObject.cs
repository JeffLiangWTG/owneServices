using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class ARCashAdvanceFilterBusinessObject : CashAdvanceFilterBusinessObject
	{
		protected override ZString LedgerType
		{
			get { return LedgerTypes.AccountsReceivable; }
		}
	}
}
