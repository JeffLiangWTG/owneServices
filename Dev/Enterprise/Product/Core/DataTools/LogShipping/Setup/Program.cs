using System;
using System.Threading;
using System.Windows.Forms;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration;
using CargoWise.DataProtection.Administration.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.LogShipping.Setup
{
	public static class Program
	{
		internal static IServiceCollection CreateApplicationServiceComposition()
		{
			var services = new ServiceCollection();
			services.ConfigureProtectedDataFactoryServices();
			services.ConfigureProtectedDataSqlExtensions(ApplicationType.Default);
			services.ConfigureProtectedDataAdministrationServices();
			services.ConfigureProtectedDataSqlServerAdministrationServices();
			services.AddSingleton<IProtectedDataAdministrationSqlExecutionContextManager, LogShippingSqlAdministrationContextManager>();
			return services;
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			ServiceProvider = CreateApplicationServiceComposition().BuildServiceProvider();
			if (TryGetAppLock())
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new GUI.LogShippingSetupForm());
				Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
			}
			else
			{
				MessageBox.Show(AnotherCopyRunningMessage, CannotRunAppCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		static void Application_ApplicationExit(object sender, EventArgs e)
		{
			ReleaseLock();
		}

		static bool TryGetAppLock()
		{
			bool acquired = false;

			lock (MutexKey)
			{
				mutex = new Mutex(false, MutexKey);
				try
				{
					acquired = mutex.WaitOne(TimeSpan.Zero, false);
				}
				catch (AbandonedMutexException)
				{
					acquired = true;
				}
			}

			return acquired;
		}

		static void ReleaseLock()
		{
			if (mutex != null)
			{
				mutex.ReleaseMutex();
				mutex.Close();
			}
		}

		[ThreadStatic]
		static Mutex mutex;
		internal static ServiceProvider ServiceProvider { get; set; }
		const string MutexKey = "Global\\Enterprise.LogShipping.Setup.C953B27C6C294859A2E4C12C7B57E34F";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant String")]
		const string AnotherCopyRunningMessage = "Another instance of the CargoWise One Log Shipping Setup Tool is already running on this computer.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant String")]
		const string CannotRunAppCaption = "Cannot run application";
	}
}
