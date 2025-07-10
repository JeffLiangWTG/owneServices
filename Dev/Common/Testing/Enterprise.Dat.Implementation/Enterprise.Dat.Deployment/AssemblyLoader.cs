using System;
using System.IO;
using System.Reflection;
using System.Threading;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Dat.Deployment
{
	public static class AssemblyLoader
	{
		public static void Enable()
		{
			if (Interlocked.CompareExchange(ref enabled, value: 1, comparand: 0) == 0)
			{
				AppDomain.CurrentDomain.AssemblyResolve += AppDomainAssemblyResolve;
			}
		}

		[ThreadSafe]
		static int enabled;

		static Assembly AppDomainAssemblyResolve(object sender, ResolveEventArgs args)
		{
			var assemblyShortName = new AssemblyName(args.Name).Name;
			var baseDirectory = Path.GetDirectoryName(typeof(AssemblyLoader).Assembly.Location);
			var assemblyPath = Path.Combine(baseDirectory, assemblyShortName + ".dll");
			if (File.Exists(assemblyPath))
			{
				return Assembly.LoadFrom(assemblyPath);
			}

			return null;
		}
	}
}
