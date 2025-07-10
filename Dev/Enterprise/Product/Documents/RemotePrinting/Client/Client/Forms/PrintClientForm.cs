using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.RemotePrinting.Client.Properties;
using Enterprise.RemotePrinting.Types;
using Microsoft.Win32;

namespace Enterprise.RemotePrinting.Client
{
	public partial class PrintClientForm : Form
	{
		public PrintClientForm()
		{
			InitializeComponent();
			InitialiseComboBoxConfiguration();

			string installedVersion = UpdateProcessor.GetInstalledVersion();
			if (!string.IsNullOrEmpty(installedVersion))
			{
				Text += " " + installedVersion;
			}
			InitialiseAutoStart();
		}

		PrintController remotePrintingController;
		Thread printClientThread;

		void InitialiseTargets()
		{
			LogWriter.RegisterFileTarget(GetLogOutPutDirectory(), LogWriter.FileNamePrefix, WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error);
			LogWriter.RegisterTextBoxBaseTarget(this.OutputRichTextBox, WebPrintEventLogEntryType.Information, WebPrintEventLogEntryType.Error);
			LogWriter.RegisterEventLogTarget(EventLogTargetSource, WebPrintEventLogEntryType.SystemInformation, WebPrintEventLogEntryType.Error);
		}

		string GetLogOutPutDirectory()
		{
			return Path.Combine(LogWriter.GetOutputDirectory().FullName, remotePrintingController.ConfigName);
		}

		public void InitialiseAutoStart()
		{
			string autoStartConfigName = null;
			var applock = ApplicationRestartHelper.TryGetAppLock(ApplicationRestartHelper.AutoConfigRegistryAccess);
			if (applock != null)
			{
				try
				{
					registryKey?.Dispose();
					registryKey = GetWebPrintAutoStartData();
					if (registryKey != null)
					{
						var configNameList = registryKey.GetValueNames();
						if (configNameList.Any())
						{
							var configDataRegistry = configNameList.First();

							if (configDataRegistry != null)
							{
								autoStartConfigName = configDataRegistry;
								registryKey.DeleteValue(autoStartConfigName);
							}
						}
					}
				}
				finally
				{
					applock.Dispose();
				}
			}

			if (!string.IsNullOrEmpty(autoStartConfigName))
			{
				ConfigurationsComboBox.SelectedItem = autoStartConfigName;
				remotePrintingController.ConfigName = autoStartConfigName;

				if (!SettingsHelper.AutoStart)
				{
					StartRunning();
				}
			}
		}

		protected virtual RegistryKey GetWebPrintAutoStartData() => Registry.LocalMachine.OpenSubKey(Constants.RegistryManager.WebPrintAutoStartData, true);

		protected virtual ConnectionRegistryManager RegistryManager => ConnectionRegistryManager.Instance;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "EventLogTargetSource")]
		const string EventLogTargetSource = "WebPrint Client Desktop";

		void InitialiseWebClient()
		{
			InitialiseControlValues();
			InitialiseController();
		}

		void InitialiseControlValues()
		{
			LocalComputerNameTextBox.Text = RegistryManager.GetLocalMachineName();
		}

		void InitialiseController()
		{
			remotePrintingController = new PrintController(LocalComputerNameTextBox.Text, this);

			remotePrintingController.ShowInformation += WrapInvoke(Controller_OnShowInformation);
			remotePrintingController.ShowError += WrapInvoke(Controller_OnShowError);
			remotePrintingController.ProcessStarting += WrapInvoke(Controller_OnProcessStarting);
			remotePrintingController.ProcessStopped += WrapInvoke(Controller_OnProcessStopped);
			remotePrintingController.ProcessFailed += WrapInvoke(Controller_OnProcessFailed);
			remotePrintingController.Updated += WrapInvoke(Controller_OnUpdated);
			remotePrintingController.RestartApplication += Controller_RestartApplication;
		}

		EventHandler<LogEventArgs> WrapInvoke(EventHandler<LogEventArgs> inner)
		{
			return (o, e) =>
			{
				if (InvokeRequired)
				{
					BeginInvoke(inner, o, e);
				}
				else
				{
					inner(o, e);
				}
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "LogMessage")]
		void InitialiseComboBoxConfiguration()
		{
			var configNames = RegistryManager.GetConfigurationNamesInRegistry(Constants.RegistryManager.CargoWiseWebPrintTopKeyName);
			var lastConfigurationName = RegistryManager.GetLastConfigurationValue();
			if (configNames.Length == 0)
			{
				ConfigurationsComboBox.Enabled = false;
				StartButton.Enabled = false;
				LogMessage(@"No configurations were found. 
Please use the Configurator tool to create a configuration before running the WebPrint Client again.", WebPrintEventLogEntryType.Information);
			}
			else
			{
				ConfigurationsComboBox.Items.Clear();
				ConfigurationsComboBox.Items.AddRange(configNames);
				if (configNames.Contains(lastConfigurationName))
				{
					ConfigurationsComboBox.SelectedItem = lastConfigurationName;
				}
				else
				{
					ConfigurationsComboBox.SelectedIndex = 0;
				}
			}
		}

		void Start_SetControlState()
		{
			FileMenuItem.Enabled = false;
			StartButton.Enabled = false;
			CloseButton.Enabled = false;
			ConfigurationsComboBox.Enabled = false;
			StopButton.Enabled = true;
			ScanForNewPrintersButton.Enabled = true;
			OutputRichTextBox.Text = "";
			remotePrintingController.ConfigName = ConfigurationsComboBox.SelectedItem.ToString();
		}

		void Start_StartPrintClientThread()
		{
			printClientThread = new Thread(new ThreadStart(OnPrintClientThreadStart));

			if (threadMonitorForm != null)
			{
				threadMonitorForm.PrintClientThread = printClientThread;
			}
			//printClientThread.SetApartmentState(ApartmentState.STA);
			printClientThread.IsBackground = true;
			printClientThread.Start();
		}

		void OnPrintClientThreadStart()
		{
			remotePrintingController.Run();

			// Don't do it on the PrintClient thread!
			Invoke(new ThreadStart(FinalisePrintClientProcess));
		}

		void FinalisePrintClientProcess()
		{
			StopButton.Enabled = false;
			StartButton.Enabled = true;
			CloseButton.Enabled = true;
			FileMenuItem.Enabled = true;
			ConfigurationsComboBox.Enabled = true;
			ScanForNewPrintersButton.Enabled = false;
		}

		#region Event Handling

		#region Enterprise_RemotePrinting_Client_Controller Events

		void Controller_OnShowInformation(object sender, LogEventArgs e)
		{
			LogWriter.Append(WebPrintEventLogEntryType.Information, e.Message);
		}

		void Controller_OnShowError(object sender, LogEventArgs e)
		{
			LogWriter.Append(WebPrintEventLogEntryType.Error, e.Message);
		}

		void Controller_OnProcessStarting(object sender, LogEventArgs e)
		{
			LogMessage(e.Message, WebPrintEventLogEntryType.SystemInformation);
		}

		void Controller_OnProcessStopped(object sender, LogEventArgs e)
		{
			LogMessage(e.Message, WebPrintEventLogEntryType.SystemInformation);
		}

		void Controller_OnProcessFailed(object sender, LogEventArgs e)
		{
			LogMessage(e.Message, WebPrintEventLogEntryType.Error);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Append")]
		void Controller_OnUpdated(object sender, LogEventArgs e)
		{
			remotePrintingController.Stop();

			var message = new StringBuilder(e.Message).AppendLine();
			message.Append("Restarting application ...");
			LogWriter.Append(WebPrintEventLogEntryType.SystemInformation, message.ToString());

			SettingsHelper.SaveAutoStart(true);

			ApplicationRestartHelper.RestartAllInstances();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Append")]
		void Controller_RestartApplication(object sender, RestartApplicationEventArgs e)
		{
			var message = new StringBuilder(e.Message).AppendLine();

			if (Settings.Default.RestartedStatus)
			{
				Settings.Default.RestartedStatus = false;
				Settings.Default.Save();

				message.Append("Application has been restarted and cannot be restarted again.");
				LogWriter.Append(WebPrintEventLogEntryType.SystemInformation, message.ToString());
			}
			else
			{
				e.Restarted = true;

				message.Append("Restarting application ...");
				LogWriter.Append(WebPrintEventLogEntryType.SystemInformation, message.ToString());

				Settings.Default.RestartedStatus = true;
				Settings.Default.Save();

				Thread.Sleep(TimeSpan.FromSeconds(30)); // Wait 30 seconds then restart client
				RestartApplication();
			}
		}

		void LogMessage(string message, WebPrintEventLogEntryType entryType)
		{
			var messagetoLog = new StringBuilder();
			messagetoLog.AppendLine();
			messagetoLog.Append(message);
			messagetoLog.AppendLine("--------------------------------------------------\r\n\r\n\r\n");

			LogWriter.Append(entryType, messagetoLog.ToString());
		}

		#endregion

		#region Form Events

		void StartButton_Click(object sender, EventArgs e)
		{
			StartRunning();
		}

		protected virtual void StartRunning()
		{
			Start_SetControlState();
			InitialiseTargets();
			Start_StartPrintClientThread();
			SaveCurrentConfiguration(null, null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Append")]
		void StopButton_Click(object sender, EventArgs e)
		{
			remotePrintingController.Stop();

			var message = new StringBuilder().AppendLine("");
			message.Append("Print Client Process marked to stop on next iteration");
			LogWriter.Append(WebPrintEventLogEntryType.Information, message.ToString());
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void ScanForNewPrintersButton_Click(object sender, EventArgs e)
		{
			remotePrintingController.UploadQueueListToServerAndReturnQueueCount();
		}

		void ConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ConfigForm webConfigForm = new ConfigForm();
			webConfigForm.StartPosition = FormStartPosition.CenterParent;
			webConfigForm.ShowDialog(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "No access to Enterprise.ZArchitecture.GUI")]
		void LogFilesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(LogWriter.GetOutputDirectory().FullName);
		}

		ThreadMonitorForm threadMonitorForm;

		void ThreadMonitorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (threadMonitorForm != null)
			{
				threadMonitorForm.Invoke(new Action(delegate { threadMonitorForm.Activate(); }));
			}
			else
			{
				threadMonitorForm = new ThreadMonitorForm(Thread.CurrentThread);
				Thread thread = new Thread(() =>
				{
					threadMonitorForm.ShowDialog();
					threadMonitorForm = null;
				});
				try
				{
					thread.SetApartmentState(ApartmentState.STA);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
				thread.Start();
			}
		}

		void ExitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void PrintClientForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (remotePrintingController != null)
			{
				remotePrintingController.Stop();
			}
		}

		void ConfigurationsComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			RegistryManager.GetWebClientConfiguration(ConfigurationsComboBox.SelectedItem.ToString());
			InitialiseWebClient();
		}

		void SaveCurrentConfiguration(object sender, EventArgs e)
		{
			var currentSelection = (string)ConfigurationsComboBox.SelectedItem;
			RegistryManager.SetLastUsedConfigurationValue(currentSelection);
		}

		#endregion

		void PrintClientForm_Load(object sender, EventArgs e)
		{
			if (SettingsHelper.AutoStart)
			{
				SettingsHelper.SaveAutoStart(false);

				StartRunning();
			}

			Closing += SaveCurrentConfiguration;
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == ApplicationRestartHelper.WM_WPR_RESTART)
			{
				var appLock = ApplicationRestartHelper.TryGetAppLock(ApplicationRestartHelper.AutoConfigRegistryAccess);
				if (appLock != null)
				{
					try
					{
						if ((m.WParam.ToInt32() == ApplicationRestartHelper.wParam) && (m.LParam.ToInt32() == ApplicationRestartHelper.lParam))
						{
							RestartApplication();
						}
					}
					finally
					{
						appLock.Dispose();
					}
				}
			}
			else
			{
				base.WndProc(ref m);
			}
		}
		RegistryKey registryKey;

		public void RestartApplication() => RestartApplicationCore();

		protected virtual void RestartApplicationCore()
		{
			if (printClientThread != null && printClientThread.IsAlive)
			{
				if (remotePrintingController != null && remotePrintingController.IsRunning)
				{
					registryKey = RegistryManager.FindKey(Constants.RegistryManager.WebPrintAutoStartData);

					registryKey.SetValue(remotePrintingController.ConfigName, remotePrintingController.ConfigName, RegistryValueKind.String);
					Task.Run(remotePrintingController.Stop).Wait();
				}
			}
			Application.Restart();
		}

		#endregion
	}
}
