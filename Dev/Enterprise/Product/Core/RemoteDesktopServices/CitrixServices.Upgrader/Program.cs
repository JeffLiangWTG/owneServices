using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.CitrixServices.Upgrader;
using CargoWise.Loader.Common;

[assembly: AssemblyTitle(UpgraderStartupDirector.PluginProductName + " Upgrader")]

namespace CargoWise.CitrixServices.Upgrader
{
	static class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			AssemblyResolver.Initialize();
			var result = Run(args);
			Environment.Exit(result);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static int Run(string[] args)
		{
			return new UpgraderStartupDirector().StartApplication(args);
		}
	}
}
