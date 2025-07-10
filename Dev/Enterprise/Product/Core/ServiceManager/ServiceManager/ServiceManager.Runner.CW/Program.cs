#if NET
using System.Runtime.CompilerServices;
#endif

namespace Enterprise.ServiceManager.Runner
{
	public static class Program
	{
#if NET
		[ModuleInitializer]
		public static void InitializeModule()
		{
			CargoWise.NetCoreAssemblyResolver.Setup();
		}
#endif

		[STAThread]
		public static int Main(string[] args)
		{
			return ApplicationStarter.Main(args);
		}
	}
}
