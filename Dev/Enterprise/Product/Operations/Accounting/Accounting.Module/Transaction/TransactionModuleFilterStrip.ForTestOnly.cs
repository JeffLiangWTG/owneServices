#if DEBUG

using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module
{
	public partial class TransactionModuleFilterStrip
	{
		public Control[] GetCurrentFilterControls_ForTestOnly(ModuleFilter currentModuleFilter)
		{
			return GetCurrentFilterControls(currentModuleFilter);
		}
	}
}

#endif
