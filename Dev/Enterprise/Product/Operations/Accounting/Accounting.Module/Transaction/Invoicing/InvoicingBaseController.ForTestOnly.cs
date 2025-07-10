#if DEBUG

using Enterprise.Security;

namespace Enterprise.Accounting.Module
{
	public partial class InvoicingBaseController
	{
		public SecurityCheckpoint CheckPointForNew_ForTestOnly => CheckPointForNew;
	}
}

#endif
