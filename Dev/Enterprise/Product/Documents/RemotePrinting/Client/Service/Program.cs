using System.ServiceProcess;

namespace Enterprise.RemotePrinting.Client.Service
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			if (args.Length > 0 && args[0] == "-SrvDebug")
			{
				RunDebug(args);
			}
			else if (args.Length > 0 && args[0] == "-SrvRestart")
			{
				RestartService(args);
			}
			else
			{
				RunService(args);
			}
		}

		static void RunDebug(string[] args)
		{
			ClientPrintService service = new ClientPrintService(args);
			service.DebugStart();

			System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
		}

		static void RunService(string[] args)
		{
			ServiceBase.Run(new ServiceBase[] { new ClientPrintService(args) });
		}

		static void RestartService(string[] args)
		{
			new RestartServicesProcessor(new WindowsServicesHelper()).RestartService(args);
		}
	}
}
