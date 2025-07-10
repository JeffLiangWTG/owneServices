using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Enterprise.RemotePrinting.Client.Service
{
	class ClientPrintService : ServiceBase
	{
		Thread printClientThread;
		PrintController remotePrintingController;
		readonly IWindowsServicesHelper windowsServicesHelper;

		public ClientPrintService(string[] args)
		{
			this.Args = args;
			this.windowsServicesHelper = new WindowsServicesHelper();
			controllers = new List<WeakReference<Controller>>();
		}

		readonly List<WeakReference<Controller>> controllers;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "LogWriter")]
		protected override void OnStart(string[] args)
		{
			base.OnStart(args);
			try
			{
				StartServiceApplication();
			}
			catch (Exception e)
			{
				LogWriter.Append(WebPrintEventLogEntryType.Error, "An error occured while starting the Windows Service: " + e.ToString());
				throw;
			}
		}

		protected override void OnStop()
		{
			controllers.ForEach((controllerRef) =>
			{
				if (controllerRef.TryGetTarget(out var controller))
				{
					controller?.Stop();
				}
			});

			controllers.Clear();
			printClientThread.Join(5000);

			base.OnStop();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "MoveToFirstPositionTemporarily")]
		void StartServiceApplication()
		{
			using (AuthenticationModulePrioritiser.MoveToFirstPositionTemporarily("Digest"))
			{
				InitialiseEventLoggers();
				var configName = InitialiseController();
				StartPrintClientThread(configName);
			}
		}

		#region Initialise Loggers

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "RegisterEventLogTarget")]
		void InitialiseEventLoggers()
		{
			LogWriter.RegisterEventLogTarget("WebPrint Client Service", WebPrintEventLogEntryType.SystemInformation, WebPrintEventLogEntryType.Error);
		}

		const string ServiceLogFileWriterPrefix = "ServiceLog";
		const string CNSWMessageSenderLogFileWriterPrefix = "CNSWMessageSender";
		const string CNSWMessageReceiverLogFileWriterPrefix = "CNSWMessageReceiver";
		const string TWNCATKMessageSenderLogFileWriterPrefix = "TWNCATKMessageSender";
		const string CLSMSMessageReceiverLogFileWriterPrefix = "CLSMSMessageReceiver";
		const string CLSMSMessageSenderLogFileWriterPrefix = "CLSMSMessageSender";
		const string JPNACCSLogFileWriterPrefix = "JPNACCSLog";

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "WebPrintEventLogEntryType")]
		string InitialiseController()
		{
			var configName = windowsServicesHelper.GetConfigNameArgumentValue(Args);

			if (!string.IsNullOrEmpty(configName) && windowsServicesHelper.CheckServiceControllerStatus(configName) == WindowsServicesHelper.ServiceNotFound)
			{
				LogWriter.Append(WebPrintEventLogEntryType.Error, "No Service installed for this configuration! Please use Configurator and install a service.");
				throw new ArgumentException("No Service installed for this configuration! Please use Configurator and install a service.");
			}

			if (string.IsNullOrEmpty(configName))
			{
				configName = ConnectionRegistryManager.Instance.GetCurrentlySelectedConfigForWindowsService(Constants.RegistryManager.CargoWiseWebPrintTopKeyName);

				if (string.IsNullOrEmpty(configName))
				{
					LogWriter.Append(WebPrintEventLogEntryType.Error, "No Service installed for this configuration! Please use Configurator and install a service.");
					throw new ArgumentException("No Configuration selected for the Windows Service! Please use Configurator and set the Windows Service configuration.");
				}
			}

			var serviceName = windowsServicesHelper.GetServiceNameFromConfigName(configName);

			if (string.IsNullOrEmpty(serviceName))
			{
				LogWriter.Append(WebPrintEventLogEntryType.Error, "No Service installed for this configuration! Please use Configurator and install a service.");
				throw new ArgumentException("No Service installed for this configuration! Please use Configurator and install a service.");
			}

			ConnectionRegistryManager.Instance.LoadFromRegistry(configName);

			var logFileFullName = GetOutputDirectoryForConfig(configName);
			var localMachineName = ConnectionRegistryManager.Instance.GetLocalMachineName();

			remotePrintingController = new PrintController(localMachineName);
			InitialiseController(remotePrintingController, configName, logFileFullName, ServiceLogFileWriterPrefix);
			remotePrintingController.ProcessStarted += OnPrintControllerProcessStarted;
			remotePrintingController.OnHubClientControllerCreated = OnHubClientControllerCreated;
			remotePrintingController.RestartApplication += OnRestartApplication;

			cts = new CancellationTokenSource();

			messageSenderController = new CNSWMessageSenderController(localMachineName, cts.Token);
			InitialiseController(messageSenderController, configName, logFileFullName, CNSWMessageSenderLogFileWriterPrefix);

			messageReceiverController = new CNSWMessageReceiverController(localMachineName, cts.Token);
			InitialiseController(messageReceiverController, configName, logFileFullName, CNSWMessageReceiverLogFileWriterPrefix);

			twSenderController = new TWNCATKMessageSenderController(localMachineName, cts.Token);
			InitialiseController(twSenderController, configName, logFileFullName, TWNCATKMessageSenderLogFileWriterPrefix);

			clSenderController = new CLSMSMessageSenderController(localMachineName, cts.Token);
			InitialiseController(clSenderController, configName, logFileFullName, CLSMSMessageSenderLogFileWriterPrefix);

			clReceiverController = new CLSMSMessageReceiverController(localMachineName, cts.Token);
			InitialiseController(clReceiverController, configName, logFileFullName, CLSMSMessageReceiverLogFileWriterPrefix);

#if DEBUG // JP NACCS function is under development. So it will be off until tests passed.
			jpSenderController = new JPNACCSMessageSenderController(localMachineName, cts.Token);
			InitialiseController(jpSenderController, configName, logFileFullName, JPNACCSLogFileWriterPrefix);

			jpReceiverController = new JPNACCSMessageReceiverController(localMachineName, cts.Token);
			InitialiseController(jpReceiverController, configName, logFileFullName, JPNACCSLogFileWriterPrefix);
#endif

			return configName;
		}

		void InitialiseController(Controller controller, string configName, string logFileFullName, string namePattern)
		{
			controller.ConfigName = configName;

			LogWriter.RegisterFileTarget(logFileFullName, namePattern, WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error, namePattern);

			controller.ShowInformation += (sender, e) => AppendMessageToLogger(controller, e.Message, WebPrintEventLogEntryType.Information, namePattern);
			controller.ProcessStarting += (sender, e) => AppendMessageToLogger(controller, e.Message, WebPrintEventLogEntryType.SystemInformation, namePattern);
			controller.ProcessStopped += (sender, e) => AppendMessageToLogger(controller, e.Message, WebPrintEventLogEntryType.SystemInformation, namePattern);
			controller.ProcessFailed += (sender, e) => AppendMessageToLogger(controller, e.Message, WebPrintEventLogEntryType.Error, namePattern);
			controller.ShowError += (sender, e) => AppendMessageToLogger(controller, e.Message, WebPrintEventLogEntryType.Error, namePattern);

			controllers.Add(new WeakReference<Controller>(controller));
		}

		void AppendMessageToLogger(Controller controller, string message, WebPrintEventLogEntryType entryType, string namePattern)
		{
			if (controller.LoggingEnable)
			{
				LogWriter.Append(entryType, message, namePattern);
			}
		}

		void LogDebugMessage(string message)
		{
			LogWriter.Append(WebPrintEventLogEntryType.Information, message, ServiceLogFileWriterPrefix);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "LogDebugMessage")]
		void TestConnection(string configName, WebClient client = null)
		{
			if (client == null)
			{
				if (string.IsNullOrEmpty(configName))
				{
					return;
				}

				LogDebugMessage("Initiating test connection for configuration: " + configName);
				client = Configurator.GetWebService(configName);
			}

			try
			{
				var upd = client.CheckClientUpdate(LogDebugMessage);
				LogDebugMessage("Test connection passed" + Environment.NewLine);
			}
			catch
			{
				// Additional options are always enabled (AuthenticationModulePrioritiser), but need positive message so customers don't raise defects.
				LogDebugMessage("Test connection lapsed, will enable additional options" + Environment.NewLine);
			}
		}

		void StartPrintClientThread(string configName)
		{
			printClientThread = new Thread(() => OnPrintClientThreadStart(configName));
			printClientThread.SetApartmentState(ApartmentState.STA);
			printClientThread.IsBackground = true;
			printClientThread.Start();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "MoveToFirstPositionTemporarily")]
		void OnPrintClientThreadStart(string configName)
		{
			// Following is needed for some cases, particularly to connect to SAND build web services.
			// -
			// I do not know why it is needed,
			// but trying to connect in new thread before setting AuthenticationModulePrioritiser even though unsuccessfull,
			// will allow remotePrintingController later to successfully setup its own connection.
			TestConnection(configName);

			// Seems this needs to be repeated in new thread.
			// This works with both SAND and local web services (local also works without it).
			using (AuthenticationModulePrioritiser.MoveToFirstPositionTemporarily("Digest"))
			{
				remotePrintingController.Run();
			}

			// Don't do other stuff on the PrintClient thread!
			this.Stop();
		}

		void OnPrintControllerProcessStarted(object sender, EventArgs e)
		{
			if (messageSenderTask == null)
			{
				StartCNSWMessageSenderTask();
			}

			if (messageReceiverTask == null)
			{
				StartCNSWMessageReceiverTask();
			}

			if (twTWNCATKMessageSenderTask == null)
			{
				StartTWNCATKMessageSenderTask();
			}

			if (clSMSMessageReceiverTask == null)
			{
				StartCLSMSMessageReceiverTask();
			}

			if (clSMSMessageSenderTask == null)
			{
				StartCLSMSMessageSenderTask();
			}

			if (jpNACCSMessageSenderTask == null)
			{
				StartJPNACCSMessageSenderTask();
			}

			if (jpNACCSMessageReceiverTask == null)
			{
				StartJPNACCSMessageReceiverTask();
			}
		}

		void OnHubClientControllerCreated(HubClientController hubClient)
		{
			hubClient.CNSWClientSettingStaled += (sender, e) =>
			{
				controllers.ForEach((controllerRef) =>
				{
					if (controllerRef.TryGetTarget(out var controller) && controller is CustomsMessageController customsMessageController)
					{
						customsMessageController.RefreshCustomseHubClientSetting();
					}
				});
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const strings")]
		void OnRestartApplication(object sender, RestartApplicationEventArgs e)
		{
			var configName = windowsServicesHelper.GetConfigNameArgumentValue(Args);
			var registryKey = Registry.LocalMachine.OpenSubKey(Constants.RegistryManager.WebPrintRestartData, true);
			if (registryKey != null)
			{
				var restarted = Convert.ToBoolean(registryKey.GetValue(configName, false), CultureInfo.InvariantCulture);
				if (restarted)
				{
					LogWriter.Append(WebPrintEventLogEntryType.Error, "Service has been restarted and cannot be restarted again." + Environment.NewLine + e.Message, ServiceLogFileWriterPrefix);
					registryKey.SetValue(configName, false, RegistryValueKind.DWord);
					return;
				}
			}
			else
			{
				registryKey = Registry.LocalMachine.CreateSubKey(Constants.RegistryManager.WebPrintRestartData);
				registryKey.SetValue(configName, true, RegistryValueKind.DWord);
			}

			Thread.Sleep(TimeSpan.FromSeconds(30)); // Wait 30 seconds then restart service

			e.Restarted = true;
			LogWriter.Append(WebPrintEventLogEntryType.Error, e.Message + Environment.NewLine + "Restarting service ... ", ServiceLogFileWriterPrefix);

			using (var process = new Process())
			{
				var args = string.Format(CultureInfo.InvariantCulture, "\"-SrvRestart\" \"{0}{1}\"", WindowsServicesHelper.ConfigNameArg, configName);
				var startInfo = new ProcessStartInfo(WindowsServicesHelper.GetServiceExeFileName(), args)
				{
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardInput = false,
				};
				process.StartInfo = startInfo;

				process.Start();
			}
		}

		string GetOutputDirectoryForConfig(string configName)
		{
			return Path.Combine(LogWriter.GetOutputDirectory().FullName, configName);
		}

		public void DebugStart()
		{
			StartServiceApplication();
		}

		#region CNSWMessageTask

		Task messageSenderTask;
		CNSWMessageSenderController messageSenderController;

		Task messageReceiverTask;
		CNSWMessageReceiverController messageReceiverController;

		Task twTWNCATKMessageSenderTask;
		TWNCATKMessageSenderController twSenderController;

		Task clSMSMessageReceiverTask;
		CLSMSMessageReceiverController clReceiverController;

		Task clSMSMessageSenderTask;
		CLSMSMessageSenderController clSenderController;

		Task jpNACCSMessageSenderTask;
		JPNACCSMessageSenderController jpSenderController;

		Task jpNACCSMessageReceiverTask;
		JPNACCSMessageReceiverController jpReceiverController;

		CancellationTokenSource cts;

		void StartCNSWMessageSenderTask()
		{
			messageSenderTask = Task.Run(() =>
			{
				if (messageSenderController != null)
				{
					messageSenderController.Run();
				}
			}, cts.Token);
		}

		void StartCNSWMessageReceiverTask()
		{
			messageReceiverTask = Task.Run(() =>
			{
				if (messageReceiverController != null)
				{
					messageReceiverController.Run();
				}
			}, cts.Token);
		}

		void StartTWNCATKMessageSenderTask()
		{
			twTWNCATKMessageSenderTask = Task.Run(() =>
			{
				if (twSenderController != null)
				{
					twSenderController.Run();
				}
			}, cts.Token);
		}

		void StartCLSMSMessageReceiverTask()
		{
			clSMSMessageReceiverTask = Task.Run(() =>
			{
				if (clReceiverController != null)
				{
					clReceiverController.Run();
				}
			}, cts.Token);
		}

		void StartCLSMSMessageSenderTask()
		{
			clSMSMessageSenderTask = Task.Run(() =>
			{
				if (clSenderController != null)
				{
					clSenderController.Run();
				}
			}, cts.Token);
		}

		void StartJPNACCSMessageSenderTask()
		{
			jpNACCSMessageSenderTask = Task.Run(() =>
			{
				if (jpSenderController != null)
				{
					jpSenderController.Run();
				}
			}, cts.Token);
		}

		void StartJPNACCSMessageReceiverTask()
		{
			jpNACCSMessageReceiverTask = Task.Run(() =>
			{
				if (jpReceiverController != null)
				{
					jpReceiverController.Run();
				}
			}, cts.Token);
		}

		#endregion

		#region IDisposable

		public StackTrace DisposeStack => disposeStack;
		StackTrace disposeStack;

		public bool IsDisposed => isDisposed;
		bool isDisposed;
		readonly string[] Args;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				isDisposed = true;
				disposeStack = new StackTrace();

				if (cts != null)
				{
					try
					{
						cts.Cancel();
					}
					catch
					{
						// cancel tasks exception
					}
					finally
					{
						cts.Dispose();
					}
				}

				LogWriter.UnregisterAllLogTargets();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
