using System;
using System.Collections;
using System.IO;
#if NETCOREAPP
using System.Linq;
using System.Runtime.Loader;
#endif
using System.Reflection;
using CargoWise.Common;
using Enterprise.Upgrades;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public static class AssemblyRetriever
	{
		/// <summary>
		/// Loads assembly objects from the application's startup directory, given simple assemly names. Invalid DLL or EXE files are silently ignored.
		/// </summary>
		/// <param name="assemblies">Simple assembly names, e.g. "Core", "Enterprise.SomeModule.Business"</param>
		/// <returns>An array of Assembly objects</returns>
		public static Assembly[] LoadAsssembliesFromSimpleNames(string[] assemblies)
		{
			ArrayList result = new ArrayList();

			if (assemblies != null)
			{
				for (int assemblyNo = 0; assemblyNo < assemblies.Length; assemblyNo++)
				{
					string assemblyPrefix = Path.Combine(AssemblyLoader.GetBinPath(), assemblies[assemblyNo]);
					Assembly assemblyToAdd;

					try
					{
						if (File.Exists(assemblyPrefix + ".DLL"))
						{
#if NETCOREAPP
							var existingAssembly = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(a => a.GetName().Name == assemblies[assemblyNo]);
							if (existingAssembly != null)
							{
								assemblyToAdd = existingAssembly;
							}
							else
							{
								assemblyToAdd = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPrefix + ".DLL");
							}
#else
							assemblyToAdd = Assembly.LoadFrom(assemblyPrefix + ".DLL");
#endif
							result.Add(assemblyToAdd);
						}
						else if (File.Exists(assemblyPrefix + ".EXE"))
						{
#if NETCOREAPP
							var existingAssembly = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(a => a.GetName().Name == assemblies[assemblyNo]);
							if (existingAssembly != null)
							{
								assemblyToAdd = existingAssembly;
							}
							else
							{
								assemblyToAdd = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPrefix + ".EXE");
							}
#else
							assemblyToAdd = Assembly.LoadFrom(assemblyPrefix + ".EXE");
#endif
							result.Add(assemblyToAdd);
						}
					}
					catch (BadImageFormatException) { }
				}
			}

			return (Assembly[])result.ToArray(typeof(Assembly));
		}

		class Test : TestCase
		{
			public void TestAssembliesAreRetrieved()
			{
				var mainFile = ExeFileNames.CargoWiseWindowsDesktopExe;
				var assemblyName = Path.GetFileNameWithoutExtension(mainFile);
				Assembly[] result = AssemblyRetriever.LoadAsssembliesFromSimpleNames(new string[] { "Enterprise.ZArchitecture.Core", assemblyName });
				AssertEquals("Should have found both Enterprise.ZArchitecture.Core.dll and " + mainFile, 2, result.Length);
				AssertEquals("Enterprise.ZArchitecture.Core", result[0].GetName().Name);
				AssertEquals(assemblyName, result[1].GetName().Name);
			}
		}
	}
}
