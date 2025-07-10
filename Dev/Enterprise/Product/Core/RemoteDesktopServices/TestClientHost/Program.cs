using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.RemoteDesktopServices.Client;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.TestClientHost
{
	static class Program
	{
		static readonly LogService logService = new LogService();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		[STAThread]
		static int Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

			try
			{
				UserNotification.LogService = logService;
				UserNotification.MessageBoxService = new EmptyMessageBoxService();

				UserNotification.WriteLog("ClientHost", "Waiting for READY input");
				WaitForConsoleReadyInput();
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				TestCase.BaseSourcePath = args[0];
				Application.OleRequired();

				UserNotification.WriteLog("ClientHost", "Start initialize channel");
				MockChannelManager.InitializeChannel();

				UserNotification.WriteLog("ClientHost", "Start setup test handler");
				SetupTestHandler();
				var channelNumber = args.Length > 1 ? int.Parse(args[1]) : 1;

				for (var i = 0; i < channelNumber; i++)
				{
					MockChannelManager.ConnectChannel(); // Multiple channels
				}

				UserNotification.WriteLog("ClientHost", "Waiting for END input");
				string line;
				while (!string.Equals(line = Console.In.ReadLine(), End, StringComparison.OrdinalIgnoreCase))
				{
					if (line != null)
					{
						MockChannelManager.ReceiveMessage(Convert.FromBase64String(line));
					}
					else
					{
						Thread.Sleep(1);
					}
				}
				UserNotification.WriteLog("ClientHost", "Process is exiting...");
				return 1;
			}
			catch (TypeLoadException e)
			{
				logService.Log($@"Please check {e.TypeName}, it causes this error. Also, the XmlSerializer:{Assembly.GetAssembly(typeof(Microsoft.Xml.Serialization.GeneratedAssembly.CreateDesktopShortcutMessageSerializer)).Location} is related.
{e.Message}
{e.StackTrace}", null);
			}
			catch (TypeInitializationException e)
			{
				logService.Log($@"Please check {e.TypeName}, it causes this error. Also, the XmlSerializer:{Assembly.GetAssembly(typeof(Microsoft.Xml.Serialization.GeneratedAssembly.CreateDesktopShortcutMessageSerializer)).Location} is related.
{e.Message}
{e.StackTrace}", null);
			}
			catch (Exception e)
			{
				logService.Log("Exception", e);
			}

			return -1;
		}

		static void WaitForConsoleReadyInput()
		{
			var consoleInput = string.Empty;
			while (!string.Equals(consoleInput, Ready, StringComparison.OrdinalIgnoreCase))
			{
				ProcessConsoleInput(consoleInput);

				Thread.Sleep(10);
				consoleInput = Console.In.ReadLine();
			}
		}

		static void ProcessConsoleInput(string consoleInput)
		{
			if (string.Equals(consoleInput, End, StringComparison.OrdinalIgnoreCase))
			{
				Environment.Exit(0);
			}
		}

		static void SetupTestHandler()
		{
			MessageHandlers.Register(EnterpriseChannelMessageTypes.Test, new TestClientHostMessageHandler());
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			logService.Log($"{ClientExtensions.GetCurrentClient()} Services Error", e.ExceptionObject as Exception);
		}

		static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			logService.Log($"{ClientExtensions.GetCurrentClient()} Services Error", e.Exception);
		}

		const string Ready = "Ready";
		const string End = "END";
	}
}
