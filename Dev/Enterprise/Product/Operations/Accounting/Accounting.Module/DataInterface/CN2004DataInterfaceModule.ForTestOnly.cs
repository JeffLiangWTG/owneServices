#if DEBUG

using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class CN2004DataInterfaceModule
	{
		public ZPopupController GetNewController_ForTestOnly()
		{
			return GetNewController();
		}
	}
}

#endif
