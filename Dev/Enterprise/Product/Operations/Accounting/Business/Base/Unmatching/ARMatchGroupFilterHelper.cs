using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Unmatching
{
	public class ARMatchGroupFilterHelper : MatchGroupFilterHelper
	{
		public ARMatchGroupFilterHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override ZString Ledger
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsReceivable; }
		}
	}
}
