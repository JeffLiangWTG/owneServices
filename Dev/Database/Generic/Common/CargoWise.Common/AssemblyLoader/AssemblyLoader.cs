using System.Reflection;

namespace CargoWise.Common
{
	/// <summary>
	/// Loads assemblies.
	/// </summary>
	[WTG.StaticAnalysis.Annotation.ThreadSafe]
	public interface IAssemblyLoader
	{
		/// <summary>
		/// Loads an assembly from the given name.
		/// </summary>
		Assembly LoadAssembly(AssemblyName assemblyName);

		string GetBinPath();
		string GetParentBinPath();
	}

	/// <summary>
	/// Loads assemblies in a generic way that can be switched out. For example loading an assembly
	/// at design time is a different process to loading an assembly at runtime.
	/// </summary>
	public static class AssemblyLoader
	{
		/// <summary>
		/// The currently active IAssemblyLoader instance.
		/// </summary>
		public static IAssemblyLoader Instance
		{
			get
			{
				return instance ?? (instance = new DefaultAssemblyLoader());
			}
			set { instance = value; }
		}

		static IAssemblyLoader instance;

		/// <summary>
		/// Loads an assembly from the given name.
		/// </summary>
		public static Assembly LoadAssembly(string assemblyName)
		{
			return LoadAssembly(new AssemblyName(assemblyName));
		}

		/// <summary>
		/// Loads an assembly from the given name.
		/// </summary>
		public static Assembly LoadAssembly(AssemblyName assemblyName)
		{
			Argument.NotNull(assemblyName, nameof(assemblyName));
			return Instance.LoadAssembly(assemblyName);
		}

		public static string GetBinPath()
		{
			return Instance.GetBinPath();
		}

		public static string GetParentBinPath()
		{
			return Instance.GetParentBinPath();
		}
	}
}
