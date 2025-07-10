using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Unmatching
{
	public class APMatchGroupFilterHelper : MatchGroupFilterHelper
	{
		public APMatchGroupFilterHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override ZString Ledger
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsPayable; }
		}
	}
}
