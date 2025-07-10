#if DEBUG

using Enterprise.Security;

namespace Enterprise.Accounting.Module
{
	public partial class ZAPPaymentController
	{
		public SecurityCheckpoint CheckPointForNew_ForTestOnly => CheckPointForNew;
	}
}

#endif
