#if DEBUG

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public partial class DependentTransactionLine
	{
		public bool AllowUserGSTOverride_ForTestOnly => AllowUserGSTOverride;

		public bool IsCurrentChargeGLAccount_ForTestOnly => IsCurrentChargeGLAccount;
	}
}

#endif
