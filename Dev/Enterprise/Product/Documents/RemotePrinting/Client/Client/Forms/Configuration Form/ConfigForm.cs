using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.RemotePrinting.Client.Forms;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Client
{
	public partial class ConfigForm : Form
	{
		readonly IWindowsServicesHelper windowsServicesHelper;

		public ConfigForm(IWindowsServicesHelper windowsServicesHelper = null)
		{
			this.windowsServicesHelper = windowsServicesHelper ?? new WindowsServicesHelper();

			InitializeComponent();

			if (!IsServiceComponentInstalled)
			{
				ServiceTabPage.Dispose();
			}

			ServiceStartModeComboBox.Items.AddRange(new[] { StartModeAutomatic, StartModeManual });
			ServiceStartModeComboBox.SelectedIndex = 0;
		}

		string longStatusMessage;
		string innerExceptionMessage;

		protected void ConfigForm_Load(object sender, EventArgs e)
		{
			StatusChanged += ConfigForm_StatusChanged;

			ConfigForm_Reload(sender, e);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "StartModeAutomatic")]
		const string StartModeAutomatic = "Automatic";
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "StartModeManual")]
		const string StartModeManual = "Manual";

		void ConfigForm_StatusChanged(object sender, LogEventArgs e)
		{
			var message = e.Message;
			var fullError = new StringBuilder(e.Message, 2500);

			if (!string.IsNullOrWhiteSpace(innerExceptionMessage))
			{
				fullError.AppendLine();
				fullError.AppendLine();
				fullError.Append(innerExceptionMessage);
			}

			longStatusMessage = fullError.ToString();
			var statusMessage = (message.Length > 75) ? message.Substring(0, 75) + "..." : message;
			StatusLabel.Text = statusMessage;
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			SaveConnectionConfiguration();

			SaveLogConfiguration();
		}

		void DialogStatusStrip_DoubleClick(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(longStatusMessage))
			{
				MessageBox.Show(longStatusMessage);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SaveConnectionConfiguration")]
		public void SaveConnectionConfiguration()
		{
			var config = GetWebClientConfigurationFromCurrentSettings();
			RegistryManager.SaveRemotePrintingRegistryValues(config);

			OnStatusChanged("Save successful. Your changes will take effect when you restart the Client.");
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "ShowMessage")]
		public void SaveLogConfiguration()
		{
			var daysToKeepText = LogFileDayToKeepTextBox.Text;
			var daysToKeep = string.IsNullOrEmpty(daysToKeepText) ? -1 : int.Parse(daysToKeepText);
			if (0 <= daysToKeep && daysToKeep <= 99)
			{
				LogFileManager.Instance.SaveLogConfiguration(new WebClientLogConfiguration(int.Parse(LogFileDayToKeepTextBox.Text), EnableCleaningOldLogCheckBox.Checked));
			}
			else
			{
				ShowMessage("Number of day to keep log files must be withing the range of [1 - 99] days", "Log Setting Error");
			}
		}

		protected void LoadConnectionConfigurations()
		{
			ConfigurationsComboBox.Items.Clear();

			var namesInRegistry = GetNamesFromRegistry();
			if (namesInRegistry.Length == 0)
			{
				ConfigurationsComboBox.Items.Add("DEFAULT");
			}
			else
			{
				ConfigurationsComboBox.Items.AddRange(namesInRegistry);
			}
		}

		protected virtual string[] GetNamesFromRegistry() => RegistryManager.GetConfigurationNamesInRegistry(Constants.RegistryManager.CargoWiseWebPrintTopKeyName);

		protected virtual ConnectionRegistryManager RegistryManager => ConnectionRegistryManager.Instance;

		protected WebClientConfiguration GetWebClientConfigurationFromCurrentSettings()
		{
			if (!int.TryParse(RequestPauseInSecondsTextBox.Text.Trim(), out var requestPauseInSeconds))
			{
				requestPauseInSeconds = ConnectionRegistryManager.DefaultValues.RequestPause;
			}

			if (!int.TryParse(ProxyPortTextBox.Text.Trim(), out var proxyPort))
			{
				proxyPort = ConnectionRegistryManager.DefaultValues.ProxyPort;
			}

			if (!int.TryParse(textBoxKeepAliveTime.Text.Trim(), out var keepAliveTime))
			{
				keepAliveTime = ConnectionRegistryManager.DefaultValues.KeepAliveTime;
			}
			if (!int.TryParse(textBoxKeepAliveInterval.Text.Trim(), out var keepAliveInterval))
			{
				keepAliveInterval = ConnectionRegistryManager.DefaultValues.KeepAliveInterval;
			}

			if (!int.TryParse(NumberOfLoopsToCheckForUpdateTextBox.Text.Trim(), out var numberOfLoopsToCheckForUpdate))
			{
				numberOfLoopsToCheckForUpdate = ConnectionRegistryManager.DefaultValues.NumberOfLoopsToCheckForUpdate;
			}
			if (!int.TryParse(RemotePrintingServiceTimeoutInSecondsTextBox.Text.Trim(), out var remotePrintingServiceTimeoutInSeconds))
			{
				remotePrintingServiceTimeoutInSeconds = ConnectionRegistryManager.DefaultValues.RemotePrintingServiceTimeoutInSeconds;
			}

			if (!int.TryParse(ConnectionRetryAttemptsTextBox.Text.Trim(), out var connectionRetryAttempts))
			{
				connectionRetryAttempts = ConnectionRegistryManager.DefaultValues.ConnectionRetryAttemps;
			}
			if (!int.TryParse(RetryDelayTextBox.Text.Trim(), out var retryDelay))
			{
				retryDelay = ConnectionRegistryManager.DefaultValues.RetryDelay;
			}

			if (!int.TryParse(textBoxJobPrintingTimeout.Text.Trim(), out var jobPrintingTimeout))
			{
				jobPrintingTimeout = ConnectionRegistryManager.DefaultValues.JobPrintingTimeout;
			}

			if (!int.TryParse(MemoryUsageMonitoringTextBox.Text.Trim(), out var memoryUsageMonitoringLimit))
			{
				memoryUsageMonitoringLimit = ConnectionRegistryManager.DefaultValues.MemoryUsageMonitoringLimit;
			}

			if (!int.TryParse(ReconnectionAttemptsTextBox.Text.Trim(), out var reconnectionAttempts))
			{
				reconnectionAttempts = ConnectionRegistryManager.DefaultValues.ReconnectionAttempts;
			}

			if (!int.TryParse(ReconnectionLimitMinutesTextBox.Text.Trim(), out var reconnectionLimitMinutes))
			{
				reconnectionLimitMinutes = ConnectionRegistryManager.DefaultValues.ReconnectionLimitMinutes;
			}

			if (!int.TryParse(PauseSignalRMinutesTextBox.Text.Trim(), out var pauseSignalRMinutes))
			{
				pauseSignalRMinutes = ConnectionRegistryManager.DefaultValues.PauseSignalRMinutes;
			}

			if (!int.TryParse(PauseAutoUpdateHoursTextBox.Text.Trim(), out var pauseAutoUpdateHours))
			{
				pauseAutoUpdateHours = ConnectionRegistryManager.DefaultValues.PauseAutomaticUpdateHours;
			}

			var config = new WebClientConfiguration
			{
				WebServiceUrl = WebServiceUrlTextBox.Text.Trim(),
				WebServiceUser = WebServiceUserTextBox.Text.Trim(),
				WebServicePwd = ProtectedDataHelper.Protect(WebServicePwdTextBox.Text.Trim()),
				RequestPauseInSeconds = requestPauseInSeconds,
				LocalMachineName = LocalMachineNameTextBox.Text.Trim(),

				ProxyEnabled = ProxyEnabledCheckBox.Checked,
				ProxyAddress = ProxyAddressTextBox.Text.Trim(),
				ProxyPort = proxyPort,
				ProxyUser = ProxyUserTextBox.Text.Trim(),
				ProxyPwd = ProtectedDataHelper.Protect(ProxyPwdTextBox.Text.Trim()),
				ProxyUseDefaultSystemSettings = ProxyUseDefaultSystemSettingsCheckBox.Checked,

				SecondsBetweenScanForNewPrinters = GetValueFromRefreshTimeForNewPrintersScanTextBox(out _),

				KeepAliveEnabled = checkBoxKeepAlive.Checked,
				KeepAliveTime = keepAliveTime,
				KeepAliveInterval = keepAliveInterval,

				NumberOfLoopsToCheckForUpdate = numberOfLoopsToCheckForUpdate,
				RemotePrintingServiceTimeoutInSeconds = remotePrintingServiceTimeoutInSeconds,
				EnableSignalR = EnableSignalRCheckBox.Checked,
				ReconnectionAttempts = reconnectionAttempts,
				ReconnectionLimitMinutes = reconnectionLimitMinutes,
				PauseSignalRMinutes = pauseSignalRMinutes,
				Expect100Continue = EnableExpect100ContinueCheckBox.Checked,

				ConnectionRetryAttempts = connectionRetryAttempts,
				RetryDelay = retryDelay,

				EnableVerboseLogging = checkBoxEnableVerboseLogging.Checked,
				JobPrintingTimeout = jobPrintingTimeout,

				EnableMemoryUsageMonitoring = EnableMemoryUsageMonitoring.Checked,
				MemoryUsageMonitoringLimit = memoryUsageMonitoringLimit,
			};

			if (!checkBoxAutoUpdateAfterNDays.Checked || !int.TryParse(textBoxUpdateNDays.Text.Trim(), out var updateNDays))
			{
				updateNDays = 0;
			}

			var updateConfig = new WebClientUpdateConfiguration
			{
				Mode = radioButtonUpdateCustom.Checked ? WebClientUpdateConfiguration.UpdateMode.Custom :
					radioButtonUpdateManual.Checked ? WebClientUpdateConfiguration.UpdateMode.Manual :
					WebClientUpdateConfiguration.UpdateMode.Automatic,

				SendDailyNotificationAboutNewVersion = checkBoxUpdateNotification.Checked,
				AutomaticUpdateToMajorVersion = checkBoxAutoUpdateMajorVersion.Checked,
				AutomaticUpdateToMinorVersion = checkBoxAutoUpdateMinorVersion.Checked,
				ForceAutomaticUpdateAfterNDays = updateNDays,

				UpdateAllowedTimeFromSafe = dateTimeUpdateFrom.Value,
				UpdateAllowedTimeToSafe = dateTimeUpdateTo.Value,

				UpdateAllowedOnMonday = checkBoxUpdateOnMonday.Checked,
				UpdateAllowedOnTuesday = checkBoxUpdateOnTuesday.Checked,
				UpdateAllowedOnWednesday = checkBoxUpdateOnWednesday.Checked,
				UpdateAllowedOnThursday = checkBoxUpdateOnThursday.Checked,
				UpdateAllowedOnFriday = checkBoxUpdateOnFriday.Checked,
				UpdateAllowedOnSaturday = checkBoxUpdateOnSaturday.Checked,
				UpdateAllowedOnSunday = checkBoxUpdateOnSunday.Checked,

				NotifyBeforeUpdate = checkBoxNotifyBeforeUpdate.Checked,
				NotifyAfterUpdate = checkBoxNotifyAfterUpdate.Checked,
				PauseAutomaticUpdateHours = pauseAutoUpdateHours,
			};

			config.UpdateConfiguration = updateConfig;

			return config;
		}

		void ResetWebClientConfigurationCurrentSettings()
		{
			WebServiceUrlTextBox.Text = string.Empty;
			WebServiceUserTextBox.Text = string.Empty;
			WebServicePwdTextBox.Text = string.Empty;

			RequestPauseInSecondsTextBox.Text = string.Empty;

			LocalMachineNameTextBox.Text = string.Empty;
			ProxyEnabledCheckBox.Checked = false;
			ProxyAddressTextBox.Text = string.Empty;

			ProxyPortTextBox.Text = string.Empty;

			ProxyUserTextBox.Text = string.Empty;
			ProxyPwdTextBox.Text = string.Empty;
			ProxyUseDefaultSystemSettingsCheckBox.Checked = false;

			RefreshTimeForNewPrintersScanTextBox.Text = string.Empty;

			checkBoxKeepAlive.Checked = false;
			textBoxKeepAliveTime.Text = ConnectionRegistryManager.DefaultValues.KeepAliveTime.ToString(CultureInfo.InvariantCulture);
			textBoxKeepAliveInterval.Text = ConnectionRegistryManager.DefaultValues.KeepAliveInterval.ToString(CultureInfo.InvariantCulture);

			EnableSignalRCheckBox.Checked = false;
			EnablePauseSignalRCheckBox.Checked = false;
			ReconnectionAttemptsTextBox.Text = ConnectionRegistryManager.DefaultValues.ReconnectionAttempts.ToString(CultureInfo.InvariantCulture);
			ReconnectionLimitMinutesTextBox.Text = ConnectionRegistryManager.DefaultValues.ReconnectionLimitMinutes.ToString(CultureInfo.InvariantCulture);
			PauseSignalRMinutesTextBox.Text = ConnectionRegistryManager.DefaultValues.PauseSignalRMinutes.ToString(CultureInfo.InvariantCulture);
			RemotePrintingServiceTimeoutInSecondsTextBox.Text = ConnectionRegistryManager.DefaultValues.RemotePrintingServiceTimeoutInSeconds.ToString(CultureInfo.InvariantCulture);
			NumberOfLoopsToCheckForUpdateTextBox.Text = ConnectionRegistryManager.DefaultValues.NumberOfLoopsToCheckForUpdate.ToString(CultureInfo.InvariantCulture);

			ConnectionRetryAttemptsTextBox.Text = ConnectionRegistryManager.DefaultValues.ConnectionRetryAttemps.ToString(CultureInfo.InvariantCulture);
			RetryDelayTextBox.Text = ConnectionRegistryManager.DefaultValues.RetryDelay.ToString(CultureInfo.InvariantCulture);

			checkBoxEnableVerboseLogging.Checked = false;
			textBoxJobPrintingTimeout.Text = ConnectionRegistryManager.DefaultValues.JobPrintingTimeout.ToString(CultureInfo.InvariantCulture);

			EnableMemoryUsageMonitoring.Checked = false;
			MemoryUsageMonitoringTextBox.Text = ConnectionRegistryManager.DefaultValues.MemoryUsageMonitoringLimit.ToString(CultureInfo.InvariantCulture);

			PauseAutoUpdateHoursTextBox.Text = ConnectionRegistryManager.DefaultValues.PauseAutomaticUpdateHours.ToString(CultureInfo.InvariantCulture);
			SetProxyFieldsEditability();
			SetKeepAliveFieldsEditability();
			SetUpdateFieldsEditability();
		}

		protected void ConfigForm_Reload(object sender, EventArgs e)
		{
			LoadConnectionConfigurations();
			ConfigurationsComboBox.SelectedIndex = 0;
		}

		void LoadConfiguration(string configName)
		{
			var config = GetWebClientConfiguration(configName);

			WebServiceUrlTextBox.Text = config.WebServiceUrl;
			WebServiceUserTextBox.Text = config.WebServiceUser;
			WebServicePwdTextBox.Text = ProtectedDataHelper.Unprotect(config.WebServicePwd);
			RequestPauseInSecondsTextBox.Text = config.RequestPauseInSeconds.ToString(CultureInfo.InvariantCulture);
			LocalMachineNameTextBox.Text = config.LocalMachineName;

			ProxyEnabledCheckBox.Checked = config.ProxyEnabled;
			ProxyAddressTextBox.Text = config.ProxyAddress;
			if (config.ProxyPort != 0)
			{
				ProxyPortTextBox.Text = config.ProxyPort.ToString(CultureInfo.InvariantCulture);
			}
			ProxyUserTextBox.Text = config.ProxyUser;
			ProxyPwdTextBox.Text = ProtectedDataHelper.Unprotect(config.ProxyPwd);
			ProxyUseDefaultSystemSettingsCheckBox.Checked = config.ProxyUseDefaultSystemSettings;

			var defaultConfig = Configurator.GetProxyDefaultSystemSettings();
			DefaultProxyAddressTextBox.Text = defaultConfig.ProxyAddress;
			if (defaultConfig.ProxyPort != 0)
			{
				DefaultProxyPortTextBox.Text = defaultConfig.ProxyPort.ToString(CultureInfo.InvariantCulture);
			}

			RefreshTimeForNewPrintersScanTextBox.Text = config.SecondsBetweenScanForNewPrinters.ToString(CultureInfo.InvariantCulture);

			checkBoxKeepAlive.Checked = config.KeepAliveEnabled;
			textBoxKeepAliveTime.Text = config.KeepAliveTime.ToString(CultureInfo.InvariantCulture);
			textBoxKeepAliveInterval.Text = config.KeepAliveInterval.ToString(CultureInfo.InvariantCulture);

			EnableSignalRCheckBox.Checked = config.EnableSignalR;
			EnablePauseSignalRCheckBox.Checked = config.EnableSignalRPause;
			ReconnectionAttemptsTextBox.Text = config.ReconnectionAttempts.ToString(CultureInfo.InvariantCulture);
			ReconnectionLimitMinutesTextBox.Text = config.ReconnectionLimitMinutes.ToString(CultureInfo.InvariantCulture);
			PauseSignalRMinutesTextBox.Text = config.PauseSignalRMinutes.ToString(CultureInfo.InvariantCulture);
			NumberOfLoopsToCheckForUpdateTextBox.Text = config.NumberOfLoopsToCheckForUpdate.ToString(CultureInfo.InvariantCulture);
			RemotePrintingServiceTimeoutInSecondsTextBox.Text = config.RemotePrintingServiceTimeoutInSeconds.ToString(CultureInfo.InvariantCulture);

			EnableExpect100ContinueCheckBox.Checked = config.Expect100Continue;

			ConnectionRetryAttemptsTextBox.Text = config.ConnectionRetryAttempts.ToString(CultureInfo.InvariantCulture);
			RetryDelayTextBox.Text = config.RetryDelay.ToString(CultureInfo.InvariantCulture);

			checkBoxEnableVerboseLogging.Checked = config.EnableVerboseLogging;

			textBoxJobPrintingTimeout.Text = config.JobPrintingTimeout.ToString(CultureInfo.InvariantCulture);
			PauseAutoUpdateHoursTextBox.Text = config.UpdateConfiguration.PauseAutomaticUpdateHours.ToString(CultureInfo.InvariantCulture);

			radioButtonUpdateCustom.Checked = false;
			radioButtonUpdateManual.Checked = false;
			radioButtonAutoUpdate.Checked = false;
			PauseAutoUpdateHoursTextBox.Enabled = false;
			switch (config.UpdateConfiguration.Mode)
			{
				case WebClientUpdateConfiguration.UpdateMode.Custom:
					radioButtonUpdateCustom.Checked = true;
					break;
				case WebClientUpdateConfiguration.UpdateMode.Manual:
					radioButtonUpdateManual.Checked = true;
					break;
				default:
					radioButtonAutoUpdate.Checked = true;
					PauseAutoUpdateHoursTextBox.Enabled = true;
					break;
			}

			checkBoxUpdateNotification.Checked = config.UpdateConfiguration.SendDailyNotificationAboutNewVersion;
			checkBoxAutoUpdateMajorVersion.Checked = config.UpdateConfiguration.AutomaticUpdateToMajorVersion;
			checkBoxAutoUpdateMinorVersion.Checked = config.UpdateConfiguration.AutomaticUpdateToMinorVersion;

			checkBoxAutoUpdateAfterNDays.Checked = config.UpdateConfiguration.ForceAutomaticUpdateAfterNDays > 0;
			textBoxUpdateNDays.Text = config.UpdateConfiguration.ForceAutomaticUpdateAfterNDays.ToString(CultureInfo.InvariantCulture);

			dateTimeUpdateFrom.Value = config.UpdateConfiguration.UpdateAllowedTimeFromSafe;
			dateTimeUpdateTo.Value = config.UpdateConfiguration.UpdateAllowedTimeToSafe;

			checkBoxUpdateOnMonday.Checked = config.UpdateConfiguration.UpdateAllowedOnMonday;
			checkBoxUpdateOnTuesday.Checked = config.UpdateConfiguration.UpdateAllowedOnTuesday;
			checkBoxUpdateOnWednesday.Checked = config.UpdateConfiguration.UpdateAllowedOnWednesday;
			checkBoxUpdateOnThursday.Checked = config.UpdateConfiguration.UpdateAllowedOnThursday;
			checkBoxUpdateOnFriday.Checked = config.UpdateConfiguration.UpdateAllowedOnFriday;
			checkBoxUpdateOnSaturday.Checked = config.UpdateConfiguration.UpdateAllowedOnSaturday;
			checkBoxUpdateOnSunday.Checked = config.UpdateConfiguration.UpdateAllowedOnSunday;

			checkBoxNotifyBeforeUpdate.Checked = config.UpdateConfiguration.NotifyBeforeUpdate;
			checkBoxNotifyAfterUpdate.Checked = config.UpdateConfiguration.NotifyAfterUpdate;

			EnableMemoryUsageMonitoring.Checked = config.EnableMemoryUsageMonitoring;
			MemoryUsageMonitoringTextBox.Text = config.MemoryUsageMonitoringLimit.ToString(CultureInfo.InvariantCulture);

			SetProxyFieldsEditability();
			SetKeepAliveFieldsEditability();
			SetUpdateFieldsEditability();
			SetMemoryUsageMonitoringEditability();
			SetPrintNudgingFieldsEditability();
			SetPausePrintNudgingFieldsEditability();

			SetServiceSettings(ConfigurationsComboBox.Text);
			SetLogSettings();
		}

		protected virtual WebClientConfiguration GetWebClientConfiguration(string configName)
		{
			return RegistryManager.GetWebClientConfiguration(configName);
		}

		#region Functions

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "ShowDialog")]
		protected void NewConfigurationButton_Click(object sender, EventArgs e)
		{
			ResetStatusLabel();
			if (ConfigurationsComboBox.Items.Count < MaxConfigurationNumber)
			{
				var result = ShowDialog("Please enter a name for your configuration :", "New Client Configuration");
				if (result.Confirmation == DialogResult.OK)
				{
					if (IsInComboBox(result.Name))
					{
						OnStatusChanged("Name already in use.");
					}
					else
					{
						ResetWebClientConfigurationCurrentSettings();
						ConfigurationsComboBox.Items.Add(result.Name);
						ConfigurationsComboBox.SelectedIndex = ConfigurationsComboBox.Items.Count - 1;
						OnStatusChanged("Successfully added.");
					}
				}
			}
			else
			{
				OnStatusChanged("You have reached the maximum number of configurations (" + MaxConfigurationNumber + ").");
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "ShowMessage")]
		protected void RenameConfigurationButton_Click(object sender, EventArgs e)
		{
			// cannot rename directly a registry key - needs to create a copy then delete the old one !
			ResetStatusLabel();
			if (HasInstalledServiceForCurrentConfig)
			{
				var message = $"You cannot rename the selected configuration because it has installed Service, please delete the service before renaming this configuration.";
				ShowMessage(message, "Service attached to current configuration", MessageBoxButtons.OK);
			}
			else
			{
				var result = ShowDialog("Please enter a name for your configuration :", "New Client Configuration", ConfigurationsComboBox.SelectedItem.ToString());
				if (result.Confirmation == DialogResult.OK)
				{
					if (IsInComboBox(result.Name))
					{
						OnStatusChanged("Name already in use.");
					}
					else
					{
						RenameKey(ConfigurationsComboBox.SelectedItem.ToString(), result.Name);
						var currentIndex = ConfigurationsComboBox.SelectedIndex;
						ConfigurationsComboBox.Items.RemoveAt(currentIndex);
						ConfigurationsComboBox.Items.Insert(currentIndex, result.Name);
						ConfigurationsComboBox.SelectedIndex = currentIndex;
						OnStatusChanged("Successfully renamed.");
					}
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "ShowMessage")]
		protected void DuplicateConfigurationButton_Click(object sender, EventArgs e)
		{
			ResetStatusLabel();
			if (ShowMessage("Are you sure you want to duplicate the current configuration ?", "Confirmation message", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				var result = ShowDialog("Please enter a name for your configuration :", "Duplicate this Client Configuration");
				if (result.Confirmation == DialogResult.OK)
				{
					if (IsInComboBox(result.Name))
					{
						OnStatusChanged("Name already in use.");
					}
					else
					{
						DuplicateToKey(result.Name, false);
						ConfigurationsComboBox.Items.Add(result.Name);
						ConfigurationsComboBox.SelectedIndex = ConfigurationsComboBox.Items.Count - 1;
						OnStatusChanged("Successfully duplicated.");
					}
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "ShowMessage")]
		protected void DeleteConfigurationButton_Click(object sender, EventArgs e)
		{
			ResetStatusLabel();
			if (ConfigurationsComboBox.Items.Count > MinConfigurationNumber)
			{
				if (HasInstalledServiceForCurrentConfig)
				{
					var message = $"You cannot delete the selected configuration because it has installed Service, please delete the service before deleting this configuration.";
					ShowMessage(message, "Cannot delete current service");
				}
				else
				{
					var result = ShowMessage("Are you sure you want to delete the current configuration ?", "Confirmation message", MessageBoxButtons.YesNo);
					if (result == DialogResult.Yes)
					{
						DeleteKey(ConfigurationsComboBox.SelectedItem.ToString());
						ConfigurationsComboBox.Items.RemoveAt(ConfigurationsComboBox.SelectedIndex);
						ConfigurationsComboBox.SelectedIndex = 0;
						OnStatusChanged("Successfully deleted.");
					}
				}
			}
			else
			{
				OnStatusChanged("You cannot delete the last configuration.");
			}
		}

		void ConfigurationsComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			ResetStatusLabel();
			LoadConfiguration(ConfigurationsComboBox.SelectedItem.ToString());
		}

		void ConfigurationsComboBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = true; // so we don't allow typing in the box
		}

		void LogFileDayToKeepTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			CheckDigitOnly(sender, e);
		}

		void RequestPauseInSecondsTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			CheckDigitOnly(sender, e);
		}

		void ProxyPortTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			CheckDigitOnly(sender, e);
		}

		void RefreshTimeForNewPrintersScanTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			CheckDigitOnly(sender, e);
		}

		void RetryDelayTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			CheckDigitOnly(sender, e);
		}

		void ConnectionRetryAttemptsTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			CheckDigitOnly(sender, e);
		}

		void CheckDigitOnly(object sender, KeyPressEventArgs e)
		{
			if (!(char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar)))
			{
				e.Handled = true;
				SystemSounds.Beep.Play();
			}
		}

		protected virtual WebClient GetWebClient() => Configurator.GetWebService(ConfigurationsComboBox.SelectedItem.ToString());

		[SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Cannot use ODesignableForm as it doesn't reference Enterprise.Core")]
		protected void CheckUpdateButton_Click(object sender, EventArgs e)
		{
			try
			{
				ResetStatusLabel();
				var webClient = GetWebClient();
				IClientUpdate update = webClient.CheckClientUpdate(OnStatusChanged);
				var processor = new UpdateProcessor(update, responseProcessor: webClient.ResponseProcessor);
				if (processor.IsUpdateRequired())
				{
					var result = ShowMessage("An update is available. Would you like to install it?",
						"Information", MessageBoxButtons.YesNo);
					if (result == DialogResult.Yes)
					{
						OnStatusChanged("Please wait ...");

						Cursor.Current = Cursors.WaitCursor;
						processor.Updated += OnUpdated;
						try
						{
							processor.Process();
						}
						finally
						{
							processor.Updated -= OnUpdated;
							Cursor.Current = Cursors.Default;
						}
					}
				}
				else
				{
					OnStatusChanged("No updates available");
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				OnStatusChangedWithExceptionMessage("Check update failed. " + ex.Message, ErrorReporter.GetExceptionMessage(ex.InnerException));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Cannot use ODesignableForm as it doesn't reference Enterprise.Core")]
		void OnUpdated(object sender, EventArgs args)
		{
			MessageBox.Show("An application was successfully updated. Press OK to restart.",
				"Information", MessageBoxButtons.OK);
			Application.Restart();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "OnStatusChanged")]
		void TestConnectionButton_Click(object sender, EventArgs e)
		{
			ResetStatusLabel();
			OnStatusChanged("Starting test connection");

			try
			{
				Configurator.GetWebService(GetWebClientConfigurationFromCurrentSettings()).CheckClientUpdate(OnStatusChanged);

				OnStatusChanged("Test connection succeeded.");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				OnStatusChangedWithExceptionMessage("Test connection failed. " + ex.Message, ErrorReporter.GetExceptionMessage(ex.InnerException));
			}
		}

		protected virtual void OnStatusChanged(string statusMessage) => StatusChanged?.Invoke(this, new LogEventArgs(statusMessage));

		protected virtual void OnStatusChangedWithExceptionMessage(string statusMessage, string innerExceptionMessage)
		{
			this.innerExceptionMessage = innerExceptionMessage;
			OnStatusChanged(statusMessage);
		}

		public event EventHandler<LogEventArgs> StatusChanged;

		void ProxyEnabledCheckBox_Click(object sender, EventArgs e)
		{
			SetProxyFieldsEditability();
		}

		void SetProxyFieldsEditability()
		{
			ProxyUseDefaultSystemSettingsCheckBox.Enabled = ProxyEnabledCheckBox.Checked;
			ProxyUserTextBox.Enabled = ProxyEnabledCheckBox.Checked;
			ProxyPwdTextBox.Enabled = ProxyEnabledCheckBox.Checked;

			var nonDefaultSettingsEnabled = !ProxyUseDefaultSystemSettingsCheckBox.Checked && ProxyEnabledCheckBox.Checked;
			ProxyAddressTextBox.Enabled = nonDefaultSettingsEnabled;
			ProxyPortTextBox.Enabled = nonDefaultSettingsEnabled;
			ProxyAddressTextBox.Font = new Font(ProxyAddressTextBox.Font, (nonDefaultSettingsEnabled ? SelectedFontStyle : UnselectedFontStyle));
			ProxyPortTextBox.Font = new Font(ProxyPortTextBox.Font, (nonDefaultSettingsEnabled ? SelectedFontStyle : UnselectedFontStyle));

			var defaultSettingsEnabled = ProxyUseDefaultSystemSettingsCheckBox.Checked && ProxyEnabledCheckBox.Checked;
			DefaultProxyAddressTextBox.Font = new Font(DefaultProxyAddressTextBox.Font, (defaultSettingsEnabled ? SelectedFontStyle : UnselectedFontStyle));
			DefaultProxyPortTextBox.Font = new Font(DefaultProxyPortTextBox.Font, (defaultSettingsEnabled ? SelectedFontStyle : UnselectedFontStyle));
		}

		void ProxyUseDefaultSystemSettingsCheckBox_Click(object sender, EventArgs e)
		{
			var nonDefaultSettingsEnabled = !ProxyUseDefaultSystemSettingsCheckBox.Checked && ProxyEnabledCheckBox.Checked;
			ProxyAddressTextBox.Enabled = nonDefaultSettingsEnabled;
			ProxyPortTextBox.Enabled = nonDefaultSettingsEnabled;
			ProxyAddressTextBox.Font = new Font(ProxyAddressTextBox.Font, (nonDefaultSettingsEnabled ? SelectedFontStyle : UnselectedFontStyle));
			ProxyPortTextBox.Font = new Font(ProxyPortTextBox.Font, (nonDefaultSettingsEnabled ? SelectedFontStyle : UnselectedFontStyle));

			var defaultSettingsEnabled = ProxyUseDefaultSystemSettingsCheckBox.Checked && ProxyEnabledCheckBox.Checked;
			DefaultProxyAddressTextBox.Font = new Font(DefaultProxyAddressTextBox.Font, (defaultSettingsEnabled ? SelectedFontStyle : UnselectedFontStyle));
			DefaultProxyPortTextBox.Font = new Font(DefaultProxyPortTextBox.Font, (defaultSettingsEnabled ? SelectedFontStyle : UnselectedFontStyle));
		}

		void RequestPauseInSecondsTextBox_Leave(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(RequestPauseInSecondsTextBox.Text.Trim()))
			{
				RequestPauseInSecondsTextBox.Text = ConnectionRegistryManager.DefaultValues.RequestPause.ToString(CultureInfo.InvariantCulture);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "ShowMessage")]
		protected void RefreshTimeForNewPrintersScanTextBox_Leave(object sender, EventArgs e)
		{
			RefreshTimeForNewPrintersScanTextBox.Text = GetValueFromRefreshTimeForNewPrintersScanTextBox(out var lessThan30Sec).ToString(CultureInfo.InvariantCulture);

			if (lessThan30Sec)
			{
				ShowMessage("The minimum value for this setting cannot be less than 30 seconds.", "Pause Between Scan For New Printers", MessageBoxButtons.OK);
			}
		}

		void checkBoxKeepAlive_CheckedChanged(object sender, EventArgs e)
		{
			SetKeepAliveFieldsEditability();
		}

		void SetKeepAliveFieldsEditability()
		{
			textBoxKeepAliveTime.Enabled = checkBoxKeepAlive.Checked;
			textBoxKeepAliveInterval.Enabled = checkBoxKeepAlive.Checked;
		}

		void enableMemoryUsageMonitoring_CheckedChanged(object sender, EventArgs e)
		{
			SetMemoryUsageMonitoringEditability();
		}

		void SetMemoryUsageMonitoringEditability()
		{
			MemoryUsageMonitoringTextBox.Enabled = EnableMemoryUsageMonitoring.Checked;
		}

		void EnableSignalRCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			SetPrintNudgingFieldsEditability();
		}

		void SetPrintNudgingFieldsEditability()
		{
			var isEnableSignalRCustom = EnableSignalRCheckBox.Checked;
			var isEnablePauseSignalRCustom = EnablePauseSignalRCheckBox.Checked && isEnableSignalRCustom;

			EnablePauseSignalRCheckBox.Enabled = isEnableSignalRCustom;
			ReconnectionAttemptsTextBox.Enabled = isEnablePauseSignalRCustom;
			ReconnectionLimitMinutesTextBox.Enabled = isEnablePauseSignalRCustom;
			PauseSignalRMinutesTextBox.Enabled = isEnablePauseSignalRCustom;
		}

		void EnablePauseSignalRCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			SetPausePrintNudgingFieldsEditability();
		}

		void SetPausePrintNudgingFieldsEditability()
		{
			var isCustom = EnablePauseSignalRCheckBox.Checked;

			ReconnectionAttemptsTextBox.Enabled = isCustom;
			ReconnectionLimitMinutesTextBox.Enabled = isCustom;
			PauseSignalRMinutesTextBox.Enabled = isCustom;
		}

		void SetUpdateFieldsEditability()
		{
			checkBoxUpdateNotification.Enabled = !radioButtonAutoUpdate.Checked;
			PauseAutoUpdateHoursTextBox.Enabled = radioButtonAutoUpdate.Checked;

			var isCustom = radioButtonUpdateCustom.Checked;

			checkBoxAutoUpdateMajorVersion.Enabled = isCustom;
			checkBoxAutoUpdateMinorVersion.Enabled = isCustom;

			checkBoxAutoUpdateAfterNDays.Enabled = isCustom;
			textBoxUpdateNDays.Enabled = isCustom && checkBoxAutoUpdateAfterNDays.Checked;

			dateTimeUpdateFrom.Enabled = isCustom;
			dateTimeUpdateTo.Enabled = isCustom;

			checkBoxUpdateOnMonday.Enabled = isCustom;
			checkBoxUpdateOnTuesday.Enabled = isCustom;
			checkBoxUpdateOnWednesday.Enabled = isCustom;
			checkBoxUpdateOnThursday.Enabled = isCustom;
			checkBoxUpdateOnFriday.Enabled = isCustom;
			checkBoxUpdateOnSaturday.Enabled = isCustom;
			checkBoxUpdateOnSunday.Enabled = isCustom;
		}

		void SetLogSettings()
		{
			var webClientLogConfiguration = LogFileManager.Instance.GetLogConfiguration();

			EnableCleaningOldLogCheckBox.Checked = webClientLogConfiguration.ShouldClearOldLog;
			LogFileDayToKeepTextBox.Text = webClientLogConfiguration.DayToKeepOldLogFile.ToString();

			LogPathTextBox.Text = LogWriter.GetOutputDirectory().FullName;
		}

		void textBoxKeepAliveTime_KeyPress(object sender, KeyPressEventArgs e)
		{
			CheckDigitOnly(sender, e);
		}

		void textBoxKeepAliveInterval_KeyPress(object sender, KeyPressEventArgs e)
		{
			CheckDigitOnly(sender, e);
		}

		void textBoxAppSettingInterval_KeyPress(object sender, KeyPressEventArgs e)
		{
			CheckDigitOnly(sender, e);
		}

		void textBoxAppSettingTime_KeyPress(object sender, KeyPressEventArgs e)
		{
			CheckDigitOnly(sender, e);
		}

		void textBoxUpdateNDays_KeyPress(object sender, KeyPressEventArgs e)
		{
			CheckDigitOnly(sender, e);
		}

		protected virtual string GetSelectedConfigForWindowsService()
		{
			return RegistryManager.GetCurrentlySelectedConfigForWindowsService(Constants.RegistryManager.CargoWiseWebPrintTopKeyName);
		}

		public void LoadConfigurationForWindowsService()
		{
			var config = GetSelectedConfigForWindowsService();
			var index = ConfigurationsComboBox.Items.IndexOf(config);
			ConfigurationsComboBox.SelectedIndex = index;
		}

		void ResetStatusLabel()
		{
			OnStatusChanged(string.Empty);
		}

		protected virtual void DuplicateToKey(string keyName, bool keepConfigurationForWindowsService)
		{
			RegistryManager.LoadFromRegistry(keyName);

			var config = GetWebClientConfigurationFromCurrentSettings();
			config.WindowsServiceConfigurationSelected &= keepConfigurationForWindowsService;

			RegistryManager.SaveRemotePrintingRegistryValues(config);
		}

		protected virtual void RenameKey(string oldName, string newName)
		{
			DuplicateToKey(newName, true);
			RegistryManager.DeleteFromRegistry(oldName);
		}

		protected virtual void DeleteKey(string keyName)
		{
			RegistryManager.DeleteFromRegistry(keyName);
		}

		bool IsInComboBox(string name)
		{
			return ConfigurationsComboBox.Items.Cast<string>().Contains(name, StringComparer.OrdinalIgnoreCase);
		}

		protected virtual PromptResult ShowDialog(string message, string caption, string text = null)
		{
			return Prompt.ShowDialog(message, caption, text);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		protected virtual DialogResult ShowMessage(string message, string caption, MessageBoxButtons buttons = MessageBoxButtons.OK)
		{
			return MessageBox.Show(message, caption, buttons); // Project is only using English, so no need for RTL reading
		}

		#endregion

		int GetValueFromRefreshTimeForNewPrintersScanTextBox(out bool lessThan30Sec)
		{
			lessThan30Sec = false;
			if (int.TryParse(RefreshTimeForNewPrintersScanTextBox.Text.Trim(), out var refreshTime))
			{
				if (refreshTime < 30)
				{
					refreshTime = 30;
					lessThan30Sec = true;
				}
			}
			else
			{
				refreshTime = ConnectionRegistryManager.DefaultValues.RefreshPrintersScan;
			}

			return refreshTime;
		}

		const FontStyle SelectedFontStyle = FontStyle.Bold;
		const FontStyle UnselectedFontStyle = FontStyle.Regular;

		const int MinConfigurationNumber = 1;
		const int MaxConfigurationNumber = 10;

		void checkBoxAutoUpdateAfterNDays_CheckedChanged(object sender, EventArgs e)
		{
			SetUpdateFieldsEditability();
		}

		void radioButtonAutoUpdate_CheckedChanged(object sender, EventArgs e)
		{
			SetUpdateFieldsEditability();
		}

		void textBoxJobPrintingTimeout_KeyPress(object sender, KeyPressEventArgs e)
		{
			CheckDigitOnly(sender, e);
		}

		void memoryUsageMonitoringTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			CheckDigitOnly(sender, e);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "ShowMessage")]
		void textBoxJobPrintingTimeout_Leave(object sender, EventArgs e)
		{
			if (!int.TryParse(textBoxJobPrintingTimeout.Text, out var jobPrintingTimeout) || jobPrintingTimeout < 0 || jobPrintingTimeout > 120)
			{
				textBoxJobPrintingTimeout.Text = ConnectionRegistryManager.DefaultValues.JobPrintingTimeout.ToString(CultureInfo.InvariantCulture);
				ShowMessage("Job Printing Timeout should be integer value between 0 and 120 minutes. 0 disables job printing timeout check.", "Job Printing Timeout");
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "No access to Enterprise.ZArchitecture.GUI")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "ShowMessage")]
		void OpenLogDirectoryButton_Click(object sender, EventArgs e)
		{
			if (Directory.Exists(this.LogPathTextBox.Text))
			{
				Process.Start(this.LogPathTextBox.Text);
			}
			else
			{
				ShowMessage("There is not log yet for this configuration.", "No log found.");
			}
		}

		void EnableCleaningOldLogCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			LogFileDayToKeepTextBox.Enabled = EnableCleaningOldLogCheckBox.Checked;
		}

		#region Services utils

		bool HasInstalledServiceForCurrentConfig => IsServiceComponentInstalled && GetCurrentConfigServiceStatus() != WindowsServicesHelper.ServiceNotFound;

		bool IsServiceComponentInstalled
		{
			get
			{
				if (!isServiceComponentInstalled.HasValue)
				{
					isServiceComponentInstalled = CheckIsServiceComponentInstalled();
				}
				return isServiceComponentInstalled.Value;
			}
		}
		bool? isServiceComponentInstalled;

		bool CheckIsServiceComponentInstalled()
		{
			var serviceExeFileName = WindowsServicesHelper.GetServiceExeFileName();
			return File.Exists(serviceExeFileName);
		}

		string LegacySelectedConfigForWindowsService
		{
			get
			{
				if (legacySelectedConfigForWindowsService == null)
				{
					legacySelectedConfigForWindowsService = ConnectionRegistryManager.Instance.GetCurrentlySelectedConfigForWindowsService(Constants.RegistryManager.CargoWiseWebPrintTopKeyName);
				}
				return legacySelectedConfigForWindowsService;
			}
		}
		string legacySelectedConfigForWindowsService;

		void SetServiceSettings(string configName)
		{
			if (IsServiceComponentInstalled)
			{
				ServiceConfigNameTextBox.Text = configName;

				var serviceName = GetServiceNameAndCheckForLegacyService(configName);
				ServiceInstallationNameTextBox.Text = serviceName;

				ServiceDisplayNameTextBox.Text = windowsServicesHelper.GetDisplayName(configName);

				UpdateServiceStatus(serviceName);
			}
		}

		string GetServiceNameAndCheckForLegacyService(string configName)
		{
			// If it is default selected configuration for legacy service task
			if (windowsServicesHelper.GetServiceNameFromConfigName(configName) == Constants.RegistryManager.DefaultWindowsServiceName &&
				configName.Equals(LegacySelectedConfigForWindowsService, StringComparison.OrdinalIgnoreCase) &&
				windowsServicesHelper.CheckServiceControllerStatus(Constants.RegistryManager.DefaultWindowsServiceName) != WindowsServicesHelper.ServiceNotFound)
			{
				// Return legacy service task name
				return Constants.RegistryManager.DefaultWindowsServiceName;
			}

			return windowsServicesHelper.GetServiceNameFromConfigName(configName);
		}

		void UpdateServiceStatus(string serviceName)
		{
			if (IsServiceComponentInstalled)
			{
				var status = windowsServicesHelper.CheckServiceControllerStatus(serviceName);
				ServiceStatusTextBox.Text = status;
				SetServiceButtons(status);
			}
		}

		void SetServiceButtons(string serviceStatus)
		{
			InstallServiceButton.Enabled = serviceStatus == WindowsServicesHelper.ServiceNotFound;
			UninstallServiceButton.Enabled = serviceStatus != WindowsServicesHelper.ServiceNotFound;
			StartServiceButton.Enabled = serviceStatus == WindowsServicesHelper.ServiceControllerStatusStopped;
			StopServiceButton.Enabled = serviceStatus == WindowsServicesHelper.ServiceControllerStatusRunning;
		}

		string GetCurrentConfigServiceStatus()
		{
			if (!IsServiceComponentInstalled)
			{
				return WindowsServicesHelper.ServiceNotFound;
			}

			return windowsServicesHelper.CheckServiceControllerStatus(ServiceInstallationNameTextBox.Text);
		}

		void CheckServiceStatusButton_Click(object sender, EventArgs e)
		{
			UpdateServiceStatus(ServiceInstallationNameTextBox.Text);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "ShowMessage")]
		protected void InstallServiceButton_Click(object sender, EventArgs e)
		{
			var configName = ServiceConfigNameTextBox.Text;
			var serviceName = ServiceInstallationNameTextBox.Text;
			var status = GetCurrentConfigServiceStatus();

			if (status != WindowsServicesHelper.ServiceNotFound)
			{
				ShowMessage(string.Format("A Service is already installed for configuration {0}.", configName), "Error", MessageBoxButtons.OK);
			}
			else
			{
				var (userName, password) = GetServiceCredentials();
				if (userName == null)
				{
					return;
				}

				var startMode =
					ServiceStartModeComboBox.SelectedItem.ToString() == StartModeAutomatic
						? WindowsServicesHelper.ServiceStartModeAuto
						: WindowsServicesHelper.ServiceStartModeDemand;

				if (!windowsServicesHelper.InstallNewService(serviceName, configName, startMode, userName, password, out var errorMessage) ||
					windowsServicesHelper.CheckServiceControllerStatusWithRetry(serviceName, 1000, s => s == WindowsServicesHelper.ServiceNotFound) == WindowsServicesHelper.ServiceNotFound)
				{
					var message = string.IsNullOrEmpty(errorMessage)
						? "Service install has not yet finished, please check the Service status and try again."
						: "Failed to install new WebPrint Client Service, please review output of installation command:\r\n" + errorMessage;
					ShowMessage(message, "Error", MessageBoxButtons.OK);
				}
				else
				{
					ShowMessage(string.Format("The Service for configuration '{0}' was successfully installed.", configName), "Information", MessageBoxButtons.OK);
				}
			}

			UpdateServiceStatus(serviceName);
		}

		protected virtual (string, string) GetServiceCredentials()
		{
			string userName = string.Empty, password = string.Empty;
			using (var serviceCredentialForm = new ServiceAccountDlg())
			{
				var dialogResult = serviceCredentialForm.ShowDialog(this);
				if (dialogResult == DialogResult.OK)
				{
					userName = serviceCredentialForm.UserName;
					password = serviceCredentialForm.Password;
				}
				else if (dialogResult == DialogResult.Cancel)
				{
					userName = null;
					password = null;
				}
			}
			return (userName, password);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "ShowMessage")]
		protected void UninstallServiceButton_Click(object sender, EventArgs e)
		{
			var configName = ServiceConfigNameTextBox.Text;
			var serviceName = ServiceInstallationNameTextBox.Text;
			var status = windowsServicesHelper.CheckServiceControllerStatus(serviceName);
			if (status == WindowsServicesHelper.ServiceNotFound)
			{
				ShowMessage("The Service does not exist", "Custom Service setting");
			}
			else if (status == WindowsServicesHelper.ServiceControllerStatusRunning)
			{
				ShowMessage(string.Format("The Service for configuration '{0}' is running, Please stop the service before continuing.", configName), "Custom Service setting", MessageBoxButtons.OK);
			}
			else if (ShowMessage(string.Format("By clicking Yes the Service for configuration '{0}' will be deleted. Do you want to continue?", serviceName), "Custom Service setting", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				if (!windowsServicesHelper.DeleteService(serviceName, out var errorMessage) ||
					windowsServicesHelper.CheckServiceControllerStatusWithRetry(serviceName, 1000, s => s != WindowsServicesHelper.ServiceNotFound) != WindowsServicesHelper.ServiceNotFound)
				{
					var message = string.IsNullOrEmpty(errorMessage)
						? "Service uninstall has not yet finished, please check the Service status and try again."
						: "Failed to uninstall the WebPrint Client Service, please review output of the service removal command:\r\n" + errorMessage;
					ShowMessage(message, "Error", MessageBoxButtons.OK);
				}
				else
				{
					ShowMessage(string.Format("The Service for configuration '{0}' was successfully uninstalled.", configName), "Information", MessageBoxButtons.OK);
				}
			}

			UpdateServiceStatus(serviceName);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "ShowMessage")]
		protected void StartServiceButton_Click(object sender, EventArgs e)
		{
			var serviceName = ServiceInstallationNameTextBox.Text;
			var status = windowsServicesHelper.CheckServiceControllerStatus(serviceName);
			if (status == WindowsServicesHelper.ServiceControllerStatusStopped)
			{
				if (windowsServicesHelper.StartProcess(serviceName, out var errorMessage))
				{
					ShowMessage("The Service has been started.\r\nNote: If it stops again, please check the service running logs.", "Service status");
				}
				else
				{
					ShowMessage("Failed to start the WebPrint Client Service, please review output of start command:\r\n" + errorMessage, "Service status");
				}
			}
			else if (status == WindowsServicesHelper.ServiceControllerStatusRunning)
			{
				ShowMessage("The Service is already running.", "Service status");
			}
			else if (status == WindowsServicesHelper.ServiceNotFound)
			{
				ShowMessage("There is no Service installed for this configuration.", "Service status");
			}

			UpdateServiceStatus(serviceName);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "ShowMessage")]
		protected void StopServiceButton_Click(object sender, EventArgs e)
		{
			var serviceName = ServiceInstallationNameTextBox.Text;
			var status = windowsServicesHelper.CheckServiceControllerStatus(serviceName);
			if (status == WindowsServicesHelper.ServiceControllerStatusRunning)
			{
				if (windowsServicesHelper.StopProcess(serviceName, out var errorMessage))
				{
					if (windowsServicesHelper.CheckServiceControllerStatusWithRetry(serviceName, 1000, s => s != WindowsServicesHelper.ServiceControllerStatusStopped) == WindowsServicesHelper.ServiceControllerStatusStopped)
					{
						ShowMessage("The Service has been stopped.", "Service status");
					}
					else
					{
						ShowMessage("The Service has been marked to stop.", "Service status");
					}
				}
				else
				{
					ShowMessage("Failed to stop the WebPrint Client Service, please review output of stop command:\r\n" + errorMessage, "Service status");
				}
			}
			else
			{
				ShowMessage("The Service for this configuration is not running.", "Service status");
			}

			UpdateServiceStatus(serviceName);
		}

		void WebPrintClientTab_Selected(object sender, TabControlEventArgs e)
		{
			if (IsServiceComponentInstalled && e.TabPage == ServiceTabPage)
			{
				UpdateServiceStatus(ServiceInstallationNameTextBox.Text);
			}
		}

		#endregion

		#region Application restarts via WndProc

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

		public void RestartApplication() => RestartApplicationCore();

		protected virtual void RestartApplicationCore() => Application.Restart();
		#endregion
	}

	[SuppressMessage("Microsoft.Performance", "CA1815", Justification = "No need to compare 2 PromptResults")]
	public struct PromptResult
	{
		public PromptResult(DialogResult res, string name)
		{
			Confirmation = res;
			Name = name;
		}

		public DialogResult Confirmation { get; private set; }
		public string Name { get; private set; }
	}

	public static class Prompt
	{
		public static PromptResult ShowDialog(string text, string caption, string name = null)
		{
			var prompt = GetForm(text, caption);
			var textBox = new TextBox() { Left = 10, Top = 70, Width = 310 };
			prompt.Controls.Add(textBox);
			var confirmationFound = prompt.Controls.Find("ConfirmationButton", false)[0];
			textBox.TextChanged += (sender, e) => { if (string.IsNullOrEmpty(textBox.Text.Trim())) { confirmationFound.Enabled = false; } else { confirmationFound.Enabled = true; } };
			confirmationFound.Enabled = false;
			textBox.Text = name;
			textBox.Focus();

			return prompt.ShowDialog() == DialogResult.OK ? new PromptResult(DialogResult.OK, textBox.Text) : new PromptResult(DialogResult.Cancel, "");
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Text")]
		static Form GetForm(string text, string caption)
		{
			var prompt = new Form()
			{
				Width = 340,
				Height = 140,
				FormBorderStyle = FormBorderStyle.FixedDialog,
				Text = caption,
				StartPosition = FormStartPosition.CenterParent
			};
			var textLabel = new Label() { Left = 10, Top = 20, Width = 300, Text = text };
			var confirmation = new Button() { Text = "OK", Left = 250, Width = 70, Top = 15, DialogResult = DialogResult.OK, Name = "ConfirmationButton" };
			var cancel = new Button() { Text = "Cancel", Left = 250, Width = 70, Top = 40, DialogResult = DialogResult.Cancel };
			prompt.Controls.Add(confirmation);
			prompt.Controls.Add(cancel);
			prompt.Controls.Add(textLabel);
			prompt.CancelButton = cancel;
			prompt.AcceptButton = confirmation;
			confirmation.Click += (sender, e) => { prompt.Close(); };

			return prompt;
		}
	}
}
