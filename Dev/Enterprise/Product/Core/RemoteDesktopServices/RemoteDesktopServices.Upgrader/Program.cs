using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Loader.Common;
using CargoWise.RemoteDesktopServices.Upgrader;

[assembly: AssemblyTitle(UpgraderStartupDirector.PluginProductName + " Upgrader")]

namespace CargoWise.RemoteDesktopServices.Upgrader
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
