using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Build.Locator;

namespace CargoWise.BuildTools
{
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	public static class DevMSBuildLocator
	{
		/// <summary>
		/// Returns the current MSBuild Instance for Dev.
		/// </summary>
		/// <returns>VisualStudioInstance</returns>
		public static VisualStudioInstance GetMSBuildInstance()
		{
#if NETFRAMEWORK
			const int DefaultVersionNumber = 17; // Ahh a Hard Coded Default Version! 
			const string VisualStudioNamePrefix = "Visual Studio";

			if (!int.TryParse(BuildXml.Instance.GetMSBuildVersion(), out var versionInt))
			{
				versionInt = DefaultVersionNumber;
			}
#else
			const int DefaultVersionNumber = 8; // Ahh a Hard Coded Default Version! 
			const string VisualStudioNamePrefix = ".NET Core SDK";
			var versionInt = DefaultVersionNumber;
#endif

			var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances();
			var devInstance = visualStudioInstances
				.Where(vs => vs.Version.Major >= versionInt)
				.OrderByDescending(vs => vs.Name.StartsWith(VisualStudioNamePrefix, StringComparison.OrdinalIgnoreCase))
				.ThenByDescending(vs => vs.Version)
				.FirstOrDefault()
				?? throw new InvalidOperationException($"DevMSBuildLocator could not locate the required Dev Instance = {string.Join(", ", visualStudioInstances.Select(x => x.Name + " (" + x.VisualStudioRootPath + ")"))}");

			return devInstance;
		}
	}
}
