#if DEBUG

using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class CsvAccountsImportModule
	{
		public ZPopupController GetNewController_ForTestOnly()
		{
			return GetNewController();
		}
	}
}

#endif
