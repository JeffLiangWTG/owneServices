using System;
using System.Windows.Forms;
using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.StlAnalysis.Load
{
	static class Program
	{
		internal static IServiceProvider ServiceProvider { get; private set; }

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			ConfigureServiceProvider();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new STLAnalysisForm());
		}

		static void ConfigureServiceProvider()
		{
			IServiceCollection services = new ServiceCollection();
			services.ConfigureProtectedDataFactoryServices();
			services.ConfigureProtectedDataSqlExtensions(ApplicationType.Default);
			ServiceProvider = services.BuildServiceProvider();
		}
	}
}
