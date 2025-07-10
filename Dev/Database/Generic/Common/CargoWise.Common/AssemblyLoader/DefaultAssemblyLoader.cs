using System.IO;
using System.Reflection;

namespace CargoWise.Common
{
	/// <summary>
	/// The default IAssemblyLoader instance that loads the assembly from the default load context.
	/// </summary>
	[WTG.StaticAnalysis.Annotation.Immutable]
	public sealed class DefaultAssemblyLoader : IAssemblyLoader
	{
		public Assembly LoadAssembly(AssemblyName assemblyName)
		{
			var result = Assembly.Load(assemblyName);
			return result;
		}

		/// <summary>
		/// Gets the bin path of the assembly loader.
		/// In .NET 8.0, the assembly location is in the \net8.0\ folder, which is returned by this method.
		/// </summary>
		public string GetBinPath()
		{
			var result = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			return result;
		}

		/// <summary>
		/// Gets the parent bin path of the assembly loader.
		/// This is due to Net8.0 Assembles referening the \net8.0\ folder in the path.
		/// </summary>
		public string GetParentBinPath()
		{
			var result = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName;
			return result;
		}
	}
}
