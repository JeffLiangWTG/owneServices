using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise;

namespace WebDeployBuilder
{
	/// <summary>
	/// Application that builds a ZIP file required for a distribution of
	/// Enterprise web projects
	/// The resulting file contains all files except for the contents of C:\Dev\Bin
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Console application only")]
	class WebDeployBuilder
	{
		[ModuleInitializer]
		public static void InitializeModule()
		{
			NetCoreAssemblyResolver.Setup();
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			int result;

			Console.WriteLine("Creating web deployment package. Please wait...");

			var binDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))!;
			var xmlFilePath = args.Length > 0 ? args[0] : Path.Combine(Path.GetDirectoryName(binDirectory)!, "Build.xml");
			var resultFile = args.Length > 1 ? args[1] : binDirectory;

			try
			{
				var webPackageBuilder = new BuildProcessor(xmlFilePath);
				webPackageBuilder.PackageCreated += WebPackageBuilder_PackageCreated;
				webPackageBuilder.Process(resultFile);
				result = 0;
			}
			catch (Exception e) // CriticalExceptionIsHandled Reason = Top level handler.
			{
				Console.WriteLine("ERROR: package not created: " + e);
				result = -1;
			}

			return result;
		}

		static void WebPackageBuilder_PackageCreated(object sender, BuildProcessor.PackageCreatedEventArgs e)
		{
			Console.WriteLine("Package {0} created.", e.PackageFile);
		}
	}
}

