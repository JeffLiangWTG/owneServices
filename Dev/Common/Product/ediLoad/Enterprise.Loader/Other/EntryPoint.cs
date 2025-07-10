using System;
using System.Runtime.CompilerServices;
using CargoWise.Loader.Common;
using Microsoft.Extensions.Logging;

namespace Enterprise.Loader
{
	static class EntryPoint
	{
		[STAThread]
		static int Main(string[] args)
		{
			AssemblyResolver.Initialize();
			Application.ConfigureApplicationServices();

			try
			{
				return Run(args);
			}
			finally
			{
				Application.ShutdownApplicationServices();
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static int Run(string[] args)
		{
			var logger = Application.LoggerFactory.CreateApplicationLogger(nameof(EntryPoint));

			try
			{
				using var activity = logger.ActivitySource.StartActivity(nameof(Run));

				logger.LogInformation($"Run {nameof(EnterpriseStartupDirector.StartApplication)}");

				return new EnterpriseStartupDirector().StartApplication(args);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, message: ex.Message);

				throw;
			}
		}
	}
}
