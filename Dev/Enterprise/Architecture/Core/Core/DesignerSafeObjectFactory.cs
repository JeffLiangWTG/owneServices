using System;
using System.Diagnostics;
using System.Reflection;

using CargoWise.Application;

namespace Enterprise.ZArchitecture.Core
{
	public static class DesignerSafeObjectFactory
	{
		public static T GetDesignerSafe<T>()
		{
#if DEBUG
			ConfigureIfRequired();
#endif
			return ObjectFactory.GetDesignerSafe<T>(Assembly.GetCallingAssembly());
		}

		public static T GetDesignerSafe<T>(string name)
		{
#if DEBUG
			ConfigureIfRequired();
#endif
			return ObjectFactory.GetDesignerSafe<T>(name, Assembly.GetCallingAssembly());
		}

		public static Type GetTypeDesignerSafe(string name)
		{
#if DEBUG
			ConfigureIfRequired();
#endif
			return ObjectFactory.GetTypeDesignerSafe(name, Assembly.GetCallingAssembly());
		}

#if DEBUG

		static void ConfigureIfRequired()
		{
			if (!isConfigured && IsVisualStudio)
			{
				lock (mutex)
				{
					if (!isConfigured)
					{
						EnterpriseApplicationConfiguration.ConfigureObjectFactory();
						isConfigured = true;
					}
				}
			}
		}

		static readonly object mutex = new object();

		static bool isConfigured;

		static bool IsVisualStudio
		{
			get { return isVisualStudio ?? (bool)(isVisualStudio = (Process.GetCurrentProcess().ProcessName == "devenv")); }
		}
		[ThreadStatic]
		static bool? isVisualStudio;

#endif
	}
}
