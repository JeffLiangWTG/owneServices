using System;

namespace CargoWise.BuildTools
{
	public static class AssemblyChecker
	{
		public static bool IsNotTargetPrefix(string assemblyName)
		{
			bool isNetCoreTargetFrameworkPrefix = assemblyName.StartsWith(CommonAssemblyInfo.CWNetCoreSubfolder, StringComparison.OrdinalIgnoreCase);
#if NETFRAMEWORK
			return isNetCoreTargetFrameworkPrefix;
#elif NET
			return !isNetCoreTargetFrameworkPrefix;
#else
#error Unexpected target platform
#endif
		}
	}
}
