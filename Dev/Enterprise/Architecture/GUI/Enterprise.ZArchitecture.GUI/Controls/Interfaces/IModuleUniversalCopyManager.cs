using System;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IModuleUniversalCopyManager : IUniversalCopyManager
	{
		void AddMenuItems(ZMenuItem newMenuItem, EventHandler onNewClick);
	}
}