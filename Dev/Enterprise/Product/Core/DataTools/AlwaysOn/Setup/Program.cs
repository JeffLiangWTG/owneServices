using System;
using System.Threading;
using System.Windows.Forms;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration;
using CargoWise.DataProtection.Administration.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.AlwaysOn.Setup
{
	static class Program
	{
		public static IServiceCollection GetApplicationServiceComposition()
		{
			var services = new ServiceCollection();
			services.ConfigureProtectedDataFactoryServices();
			services.ConfigureProtectedDataSqlExtensions(ApplicationType.Default);
			services.ConfigureProtectedDataAdministrationServices();
			services.ConfigureProtectedDataSqlServerAdministrationServices();
			services.AddSingleton<IProtectedDataAdministrationSqlExecutionContextManager, AlwaysOnSqlExecutionContextManager>();
			return services;
		}

		public static AlwaysOnSqlExecutionContextManager SqlContextManager => (AlwaysOnSqlExecutionContextManager)ServiceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>();

		public static void SetupServiceProvider()
		{
			var services = GetApplicationServiceComposition();
			ServiceProvider = services.BuildServiceProvider();
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			SetupServiceProvider();
			if (TryGetAppLock())
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new GUI.MainForm());
				Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
			}
			else
			{
				MessageBox.Show(AnotherCopyRunningMessage, CannotRunAppCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning); // Standalone Application. No external references.
			}
		}

		static void Application_ApplicationExit(object sender, EventArgs e)
		{
			ReleaseLock();
		}

		static bool TryGetAppLock()
		{
			var acquired = false;

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
		const string MutexKey = "Global\\Enterprise.AlwaysOn.Setup.C953B27C6C294859A2E4C12C7B57E34F";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		const string AnotherCopyRunningMessage = "Another instance of the CargoWise AlwaysOn Availability Group Setup Tool is already running on this computer.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		const string CannotRunAppCaption = "Cannot run application";
		internal static ServiceProvider ServiceProvider { get; set; }
	}

	[Serializable]
	public class AlwaysOnException : Exception
	{
		public AlwaysOnException(string message)
			: base(message)
		{ }

#if NETFRAMEWORK
		protected AlwaysOnException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
