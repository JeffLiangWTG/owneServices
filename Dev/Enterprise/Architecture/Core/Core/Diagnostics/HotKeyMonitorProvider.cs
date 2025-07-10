using CargoWise.Application;

namespace Enterprise.ZArchitecture.Core
{
	public static class HotKeyMonitorProvider
	{
		public static IHotKeyMonitor GetHotKeyMonitor()
		{
			return ObjectFactory.Get<IHotKeyMonitor>();
		} 
	}
}
