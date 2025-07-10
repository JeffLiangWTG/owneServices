#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public partial class DependentTransactionLineCollection
	{
		public void SetDefaultsForNewChild_ForTestOnly(BusinessObject child)
		{
			SetDefaultsForNewChild(child);
		}
	}
}

#endif
