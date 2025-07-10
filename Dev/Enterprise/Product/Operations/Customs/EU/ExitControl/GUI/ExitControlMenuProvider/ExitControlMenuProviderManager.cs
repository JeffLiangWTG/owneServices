using System.Collections;
using CargoWise.Application;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public static class ExitControlMenuProviderManager
	{
		public static IExitControlMenuProvider GetMenuProvider(string countryOrGroupingCode)
		{
			IExitControlMenuProvider provider = null;
			var providers = ObjectFactory.Get<Hashtable>("ExitControlMenuProviders");
			if (!string.IsNullOrEmpty(countryOrGroupingCode))
			{
				var objectHandle = (ObjectHandle)providers[countryOrGroupingCode];
				provider = (IExitControlMenuProvider)objectHandle?.GetObject();
			}
			return provider;
		}
	}
}
