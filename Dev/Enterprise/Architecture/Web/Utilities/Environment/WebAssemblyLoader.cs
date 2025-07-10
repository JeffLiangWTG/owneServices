using System.IO;
using System.Reflection;
using CargoWise.Common;
using Enterprise.Environment;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment
{
	public class WebAssemblyLoader : IAssemblyLoader
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "directory name")]
		public string GetBinPath()
		{
			return Path.Combine(Env.ApplicationStartupPath, "Bin");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "parent directory of bin")]
		public string GetParentBinPath() => GetBinPath();

		public Assembly LoadAssembly(AssemblyName assemblyName)
		{
			return Assembly.LoadFrom(Path.Combine(GetBinPath(), assemblyName + ".dll"));
		}
	}
}
