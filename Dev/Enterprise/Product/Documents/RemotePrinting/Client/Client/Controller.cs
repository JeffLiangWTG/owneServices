using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Services.Protocols;
using Enterprise.RemotePrinting.Types;
using Microsoft.AspNet.SignalR.Client;

namespace Enterprise.RemotePrinting.Client
{
	public abstract class Controller : INotifications
	{
		readonly TimeSpan intervalToLogMemoryUsage = TimeSpan.FromMinutes(10);

		protected DateTime lastLogMemoryUsage = DateTime.MinValue;
		protected DateTime lastCleanOldLogFiles = DateTime.MinValue;
		readonly TimeSpan intervalToCleanOldLogFiles = TimeSpan.FromDays(1);

		public event EventHandler<LogEventArgs> ShowInformation;
		public event EventHandler<LogEventArgs> ShowError;
		public event EventHandler<LogEventArgs> ProcessStarting;
		public event EventHandler<EventArgs> ProcessStarted;
		public event EventHandler<LogEventArgs> ProcessFailed;
		public event EventHandler<LogEventArgs> ProcessStopped;
		public event EventHandler<LogEventArgs> Updated;
		public event EventHandler<RestartApplicationEventArgs> RestartApplication;

		protected volatile bool ShouldStop;
		int loops;

		const long MByte = 1024 * 1024;
		const int ForceGCThreshold = 150;
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Defines standard admin contact message.")]
		public const string ContactAdminMessage = "Please contact your system administrator if the problem persists.";

		protected abstract bool IsMainController { get; }
		public bool IsRunning { get; private set; }

		protected Controller(ISynchronizeInvoke syncInvoke)
		{
			SyncInvoke = syncInvoke;
			LoggingEnable = true;
		}

		public void Run()
		{
			if (IsMainController)
			{
				SingletonApplicationRunner appRunner = new SingletonApplicationRunner();

				if (!appRunner.Run(RunCore, ConfigName))
				{
					OnProcessStopped(appRunner.AnotherCopyRunningMessage);
				}
			}
			else
			{
				RunCore();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Manages process execution and cleanup.")]
		void RunCore()
		{
			try
			{
				ShouldStop = false;
				IsRunning = true;
				InitialiseWebServiceClient();
				OnProcessStarted();
				Process();
				OnProcessStopped("Process Completed");
			}
			catch (Exception ex)
			{
				HandlePreLoopException(ex);
			}
			finally
			{
				ResetWebServiceClient();
				IsRunning = false;
			}
		}

		protected abstract void Process();

		public virtual void Stop()
		{
			ShouldStop = true;
		}

		protected virtual void InitialiseWebServiceClient()
		{
			ConnectionRegistryManager.ProtectWebServicePasswordForAllConfigurations(OnShowInformation);

			var config = ConfigSetting;
			NumberOfLoopsToCheckForUpdate = config.NumberOfLoopsToCheckForUpdate;

			LogProcessStartingInfo(config);

			WebServiceClient.SetWebServiceUrlAndCredentials(config, OnShowInformation);
			if (IsMainController)
			{
				UpdateWithRetry();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logs information when a process starts.")]
		protected void LogProcessStartingInfo(WebClientConfiguration config)
		{
			var message = new StringBuilder();
			message.AppendLine("Process Started").AppendLine();
			message.AppendFormat("Client Build Version: {0}", UpdateProcessor.GetInstalledVersion()).AppendLine();
			message.AppendFormat("Configuration Name: {0}", ConfigName).AppendLine();
			message.AppendFormat("WebService URL: {0}", config.WebServiceUrl).AppendLine();
			OnProcessStarting(message.ToString());
		}

		protected void CleanOldLogFiles()
		{
			if ((DateTime.UtcNow - lastCleanOldLogFiles) > intervalToCleanOldLogFiles)
			{
				try
				{
					var logManagerInstance = LogFileManager.Instance;
					var webClientLogConfiguration = logManagerInstance.GetLogConfiguration();

					if (webClientLogConfiguration.ShouldClearOldLog)
					{
						OnShowInformation($"Cleaning log file(s) older than {webClientLogConfiguration.DayToKeepOldLogFile} day(s).");

						logManagerInstance.CleanOldLogFiles(webClientLogConfiguration.DayToKeepOldLogFile, OnShowInformation);
					}
				}
				catch (Exception ex)
				{
					var message = new StringBuilder();
					message.AppendLine($"Cleaning log file went wrong.").AppendLine();
					message.AppendLine(ex.Message).AppendLine();
					OnShowInformation(message.ToString());
				}
				finally
				{
					lastCleanOldLogFiles = DateTime.UtcNow;
				}
			}
		}

		protected virtual void ResetWebServiceClient()
		{
		}

		protected virtual ConnectionRegistryManager ConnectionRegistryManager => ConnectionRegistryManager.Instance;

		public string ConfigName
		{
			get => configName;
			set
			{
				if (configName != value)
				{
					configName = value;
					configSetting = null;
				}
			}
		}
		string configName;

		protected WebClientConfiguration ConfigSetting
		{
			get
			{
				if (configSetting == null)
				{
					configSetting = GetNewConfigSetting(ConfigName);
				}
				return configSetting.Value;
			}
			set
			{
				configSetting = value;
			}
		}
		WebClientConfiguration? configSetting;

		protected void ResetConfigSetting() => configSetting = null;

		protected virtual WebClientConfiguration GetNewConfigSetting(string configName) => ConnectionRegistryManager.GetWebClientConfiguration(configName);

		public bool LoggingEnable { get; protected set; }

		public int NumberOfLoopsToCheckForUpdate
		{
			get => numberOfLoopsToCheckForUpdate > 0 ? numberOfLoopsToCheckForUpdate : 100;
			set => numberOfLoopsToCheckForUpdate = value > 0 ? value : 100;
		}
		int numberOfLoopsToCheckForUpdate;

		#region Update

		protected bool CheckForUpdateAndUpdateInstalled()
		{
			if (++loops >= NumberOfLoopsToCheckForUpdate)
			{
				loops = 0;
				return UpdateInstantly();
			}

			return false;
		}

		void ForceUpdateCheck()
		{
			loops = NumberOfLoopsToCheckForUpdate;
		}

		bool UpdateWithRetry()
		{
			var config = ConfigSetting;
			var maxAttempts = config.ConnectionRetryAttempts;
			var delay = config.RetryDelay;
			var attempts = 0;

			do
			{
				try
				{
					return UpdateInstantlyCore();
				}
				catch (Exception ex)
				{
					if (attempts >= maxAttempts)
					{
						throw;
					}
					else
					{
						attempts++;
					}

					if (!HandleServerException(ex, false, false))
					{
						throw;
					}
					else
					{
						Thread.Sleep(delay * 1000);
					}

					OnShowInformation($"Update failed after {attempts} attempts: {ex.Message}");
				}
			}
			while (true);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Handles and reports update failures.")]
		bool UpdateInstantly()
		{
			try
			{
				return UpdateInstantlyCore();
			}
			catch (Exception e)
			{
				while (e != null)
				{
					if (e is WebException webException)
					{
						OnShowInformation("Update failed: " + webException.Message);
						return false;
					}

					e = e.InnerException;
				}

				throw;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Handles update checks and processing.")]
		bool UpdateInstantlyCore()
		{
			OnShowInformation("Checking for update ...");

			var update = GetClientUpdate();

			var updateConfigProvider = new UpdateConfigurationProvider(ConnectionRegistryManager, ConfigName, ConfigSetting.LocalMachineName, WebServiceClient.SendNotificationEmail);
			var processor = new UpdateProcessor(update, updateConfigProvider, WebServiceClient.ResponseProcessor, OnShowInformation);

			if (processor.IsUpdateRequired())
			{
				OnShowInformation($"Updating to version {update.Version} ...{System.Environment.NewLine}Link to new update file: {update.Link}");

				processor.Updated += OnUpdated;
				try
				{
					processor.Process();
				}
				finally
				{
					processor.Updated -= OnUpdated;
				}

				return true;
			}
			else
			{
				OnShowInformation("No updates available" + System.Environment.NewLine);
				return false;
			}
		}

		protected virtual IClientUpdate GetClientUpdate()
		{
			return WebServiceClient.CheckClientUpdate(OnShowInformation);
		}

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logs and monitors memory usage, sending alerts if limits are exceeded.")]
		protected void LogMemoryUsage()
		{
			if ((DateTime.UtcNow - lastLogMemoryUsage) > intervalToLogMemoryUsage)
			{
				ForceGCCollectIfNeeded();
				var totalMemory = GetCurrentProcessMemory() / MByte;
				OnShowInformation(string.Format("Current memory usage: {0} MB.", totalMemory));
				lastLogMemoryUsage = DateTime.Now;

				if (ConfigSetting.EnableMemoryUsageMonitoring)
				{
					if (totalMemory >= ConfigSetting.MemoryUsageMonitoringLimit)
					{
						OnShowInformation(string.Format("Memory exceeded limit of {0} MB.", ConfigSetting.MemoryUsageMonitoringLimit));

						var message = GetEmailBodyMemoryDump(totalMemory);

						SendMessageToNotificationGroup(message, WebServiceClient.SendNotificationEmail);
					}
				}
			}
		}

		[SuppressMessage("Enterprise", "EDI012", Justification = "Cargowise configurator is not product name.")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Generates email body for memory dump instructions.")]
		string GetEmailBodyMemoryDump(long totalMemory)
		{
			var emailBody = new StringBuilder();
			emailBody.AppendLine(string.Format("The memory usage for the WebPrint Client on {0} has exceeded the value set in the Application Settings on the General Settings tab of the {1}.", ConfigSetting.LocalMachineName, Constants.ConfigurationName));
			emailBody.AppendLine("System informations :");
			emailBody.AppendLine(string.Format("Print Server Name: {0}", ConfigSetting.LocalMachineName));
			emailBody.AppendLine(string.Format("Current memory usage: {0} MB.", totalMemory));
			emailBody.AppendLine(string.Format("Interactive Session: {0}", System.Environment.UserInteractive ? "Yes" : "No"));
			emailBody.AppendLine(" Please restart the WebPrint Client, and if the memory usage continues to exceed the memory threshold, please obtain a memory dump by following the steps below :");
			emailBody.AppendLine("1. Download Process Explorer from <a href='https://docs.microsoft.com/en-us/sysinternals/downloads/process-explorer'>here</a>:- it will give you a zip file containing 4 files. Unzip them and put them in a folder somewhere accessible.There is nothing to install, you can run this tool as is. ");
			emailBody.AppendLine("2. Run ProcExp.exe.(accept the terms and conditions that come up). Once it starts, click the “Private Bytes” column header to sort the processes using lots of memory to the top. Right click on the process you want to diagnose, select[Create Dump > Create Full Dump...].");
			emailBody.AppendLine("3. You will be prompted to select a location for the file, and a file name. Once you have done this, Process Explorer will create a file that might be quite large. Please, zip it to reduce the size. To do so, open the folder you saved the file to using Windows Explorer, right click on the file, select[Send to > Compressed(zipped) folder].");
			emailBody.AppendLine("4. The compressed file should be a lot smaller. Make the file available to WTG staff in a location where we can download the dump file.");
			emailBody.AppendLine("4.1 Either use a file transfer service like <a href='https://wetransfer.com/'>wetransfer.com</a> or <a href='https://www.sharefile.com/'>sharefile.com</a>");
			emailBody.AppendLine("4.2 If you have an FTP or SFTP server, give us login details and we can use that to download.");

			return emailBody.ToString();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Sends notification with log memory usage message.")]
		void SendMessageToNotificationGroup(string message, Action<string, string> sendNotificationAction)
		{
			sendNotificationAction?.Invoke("Remote Printing Client Log Memory Usage", message);
		}

		protected virtual long GetCurrentProcessMemory()
		{
			using (var currentProcess = System.Diagnostics.Process.GetCurrentProcess())
			{
				return currentProcess.PrivateMemorySize64;
			}
		}

		[SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", Justification = "CS00604384 - Possible WebPrint memory leak issue")]
		[SuppressMessage("CargoWiseOne", "CW1071:DoNotUseGCWaitForPendingFinalizersOrGetTotalMemory", Justification = "Baseline")]
		protected void ForceGCCollectIfNeeded()
		{
			using (var currentProcess = System.Diagnostics.Process.GetCurrentProcess())
			{
				long totalMemory = currentProcess.PrivateMemorySize64 / MByte;
				if (totalMemory > ForceGCThreshold)
				{
					GC.Collect();
					GC.WaitForPendingFinalizers();
					GC.Collect();
				}
			}
		}

		#region Handle Exception

		protected void HandleExceptionWhenProcessing(Exception ex, string title = "Failed", bool shouldHandleTimeout = true, bool shouldSleepIfProtocolError = true)
		{
			if (!HandleServerException(ex, shouldHandleTimeout, shouldSleepIfProtocolError))
			{
				OnShowError($"* * * {title} * * *\r\n{GetExceptionMessageAndStacktrace(ex)}\r\n\r\n");
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Handles exceptions and generates failure messages.")]
		protected void HandlePreLoopException(Exception ex)
		{
			if (!HandleServerException(ex, false, false))
			{
				string message = "* * * Process Failed * * *\r\n" + GetExceptionMessageAndStacktrace(ex);

				if (ex is UriFormatException)
				{
					message += "\r\nPlease check your configuration.";
				}

				OnProcessFailed(message + "\r\n\r\n");
			}
		}

		protected static string GetExceptionMessageAndStacktrace(Exception ex)
		{
			if (ex == null)
			{
				return string.Empty;
			}

			var sb = new StringBuilder();

			ErrorReporter.FillExceptionMessageAndStacktrace(ex, sb);

			return sb.ToString();
		}

		protected bool HandleServerException(Exception ex, bool shouldHandleTimeout, bool shouldSleepIfProtocolError)
		{
			bool result = false;
			string operationName = null;

			while (ex != null)
			{
				if (ex is OperationRetryException retryException)
				{
					operationName = retryException.OperationName;
				}

				if (HandleSoapException(ex as SoapException))
				{
					result = true;
					break;
				}

				if (HandleWebException(ex as WebException, shouldHandleTimeout, shouldSleepIfProtocolError, operationName))
				{
					result = true;
					break;
				}

				if (HandleSignalRHttpClientException(ex as HttpClientException))
				{
					result = true;
					break;
				}

				if (HandleSignalRObjectDisposedException(ex as ObjectDisposedException))
				{
					result = true;
					break;
				}

				if (ex is AggregateException aggregateEx && aggregateEx.InnerExceptions.Count > 1)
				{
					result = aggregateEx.InnerExceptions.All(innerEx => HandleServerException(innerEx, shouldHandleTimeout, shouldSleepIfProtocolError));
					break; // Stop even if result == false - some of exceptions in aggregateEx.InnerExceptions was not handled
				}

				ex = ex.InnerException;
			}

			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Handles ObjectDisposedException for SignalR.")]
		bool HandleSignalRObjectDisposedException(ObjectDisposedException objectDisposedException)
		{
			if (objectDisposedException != null && objectDisposedException.StackTrace != null && objectDisposedException.StackTrace.Contains("Microsoft.AspNet.SignalR.Client"))
			{
				OnShowInformation(string.Format("SignalR occurs ObjectDisposedException with error message: {0}, StackTrace: {1}", objectDisposedException.Message, objectDisposedException.StackTrace));
				return true;
			}

			return false;
		}

		protected bool HandleSoapException(SoapException soapEx)
		{
			bool handled = false;

			if (soapEx != null)
			{
				Match m = Regex.Match(soapEx.Message, ".*SoapException:(?<msg>.+)--->.*Exception:(?<innerMsg>.+)\\n", RegexOptions.IgnoreCase);

				if (m.Success)
				{
					string message = m.Groups["msg"].Value;
					string innerMessage = m.Groups["innerMsg"].Value;
					OnShowInformation(message.Trim() + " " + innerMessage.Trim() + System.Environment.NewLine + ContactAdminMessage);
					handled = true;
				}
			}

			return handled;
		}

		protected static bool IsBelongToWebOperationTimeout(WebException webEx)
		{
			return webEx.Status == WebExceptionStatus.Timeout
				|| webEx.Status == WebExceptionStatus.KeepAliveFailure
				|| webEx.Status == WebExceptionStatus.RequestCanceled
				|| webEx.Status == WebExceptionStatus.SendFailure
				|| webEx.Status == WebExceptionStatus.ReceiveFailure;
		}

		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "ConfigName is not product name.")]
		[SuppressMessage("CargoWiseOne", "CW1066:DoNotUseGoToCaseOrDefault", Justification = "Baseline")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Handles various WebException scenarios.")]
		protected bool HandleWebException(WebException webEx, bool shouldHandleTimeout, bool shouldSleepIfProtocolError, string operationName)
		{
			var handled = false;

			if (webEx != null)
			{
				if (shouldHandleTimeout && IsBelongToWebOperationTimeout(webEx))
				{
					var messageBuilder = new StringBuilder();
					messageBuilder.Append("Timeout while executing web operation: ").AppendLine(webEx.Message);
					ErrorReporter.FillWebExceptionDetails(webEx, messageBuilder);
					messageBuilder.Append("Timeout interval: ").Append(ConfigSetting.RemotePrintingServiceTimeoutInSeconds).AppendLine("s");

					OnShowInformation(messageBuilder.ToString());
					handled = true;
				}
				if (webEx.Status == WebExceptionStatus.NameResolutionFailure)
				{
					OnShowInformation("NameResolutionFailure while executing web operation: " + webEx.Message);
					handled = true;
				}
				else if (webEx.Status == WebExceptionStatus.ConnectFailure)
				{
					OnShowInformation("ConnectFailure while executing web operation: " + webEx.Message);
					handled = true;
				}
				else if (webEx.Status == WebExceptionStatus.ProtocolError)
				{
					var responseCode = 0;
					var errorDetails = string.Empty;
					var shouldDisplayFullException = false;

					if (webEx.Response is HttpWebResponse httpResponse)
					{
						responseCode = (int)httpResponse.StatusCode;
						errorDetails = webEx.Message;
					}
					else
					{
						var statusMatch = WebExceptionRegex.Match(webEx.Message);
						if (statusMatch.Success)
						{
							var codeText = statusMatch.Groups["num"].Value;
							if (!int.TryParse(codeText, out responseCode))
							{
								responseCode = 0;
							}

							errorDetails = statusMatch.Groups["msg"].Value;
						}
					}

					if (responseCode != 0)
					{
						var message = System.Environment.NewLine + "Error details:" + System.Environment.NewLine + errorDetails;

						var errorMessageBuilder = new StringBuilder();
						errorMessageBuilder
							.AppendLine()
							.Append("Web Print suspended due to ");

						switch (responseCode)
						{
							case (int)HttpStatusCode.BadRequest: //400
								errorMessageBuilder.Append("bad request.");
								shouldSleepIfProtocolError = false;
								break;

							case (int)HttpStatusCode.Unauthorized: // 401
								errorMessageBuilder.Append(string.Format(@"incorrect credentials.
Please verify that the User and Password in WebPrint Configuration for '{0}' match the login details in CargoWise One Registry at Web > Web Services in registry items Web Services User Login, Web Services User Password, and Web Services Alternative Credentials.", ConfigName));
								shouldSleepIfProtocolError = false;
								break;

							case (int)HttpStatusCode.RequestTimeout: // 408
								errorMessageBuilder.Append("a slow connection or connection break.");
								shouldSleepIfProtocolError = false;
								break;

							case (int)HttpStatusCode.RequestEntityTooLarge: // 413
								errorMessageBuilder.Append("the request entity too large.");
								shouldSleepIfProtocolError = false;
								break;

							case (int)HttpStatusCode.InternalServerError: // 500
								errorMessageBuilder.Append("an internal server error.");
								shouldDisplayFullException = true;
								shouldSleepIfProtocolError = true;
								break;

							case (int)HttpStatusCode.BadGateway: // 502
								errorMessageBuilder.Append("a gateway error.");
								shouldDisplayFullException = true;
								shouldSleepIfProtocolError = false;
								errorMessageBuilder.AppendLine(message);
								break;

							case (int)HttpStatusCode.ServiceUnavailable: // 503
								errorMessageBuilder.Append("the service being unavailable.");
								shouldDisplayFullException = true;
								errorMessageBuilder.AppendLine(message);
								break;

							case (int)HttpStatusCode.GatewayTimeout: // 504
								errorMessageBuilder.Append("a slow connection or connection break to gateway.");
								shouldSleepIfProtocolError = false;
								break;

							case 525:
								errorMessageBuilder.Append("the server not being properly configured.");
								shouldDisplayFullException = true;
								shouldSleepIfProtocolError = false;
								break;

							case 530:
								errorMessageBuilder.Append("a general database access error.");
								shouldDisplayFullException = true;
								errorMessageBuilder.AppendLine(message);
								shouldSleepIfProtocolError = false;
								break;

							case 531:
								shouldDisplayFullException = true;
								shouldSleepIfProtocolError = true;
								if (message.IndexOf("Cannot open database", StringComparison.InvariantCultureIgnoreCase) < 0)
								{
									errorMessageBuilder.Append("cannot open database.");
									errorMessageBuilder.AppendLine(message);
								}
								else
								{
									goto default;
								}

								break;

							case 532:
								shouldDisplayFullException = true;
								shouldSleepIfProtocolError = true;
								if (message.IndexOf("Database in single user mode", StringComparison.InvariantCultureIgnoreCase) < 0)
								{
									errorMessageBuilder.Append("database in single user mode. Please check if your Systems Admin is running an upgrade.");
									errorMessageBuilder.AppendLine(message);
								}
								else
								{
									goto default;
								}

								break;

							case 533:
								shouldDisplayFullException = true;
								shouldSleepIfProtocolError = true;
								if (message.IndexOf("Please check your database login", StringComparison.InvariantCultureIgnoreCase) < 0)
								{
									errorMessageBuilder.Append("incorrect database login/password. Please check it.");
									errorMessageBuilder.AppendLine(message);
								}
								else
								{
									goto default;
								}

								break;

							case 534:
								shouldDisplayFullException = true;
								shouldSleepIfProtocolError = true;
								if (message.IndexOf("Network error or database server is unavailable", StringComparison.InvariantCultureIgnoreCase) < 0)
								{
									errorMessageBuilder.Append("network error or unavailable database server.");
									errorMessageBuilder.AppendLine(message);
								}
								else
								{
									goto default;
								}

								break;

							case 535:
								shouldDisplayFullException = false;
								shouldSleepIfProtocolError = true;
								errorMessageBuilder.Append("the database needs to be upgraded.");
								errorMessageBuilder.AppendLine(message);
								break;

							case 536:
								shouldDisplayFullException = false;
								shouldSleepIfProtocolError = true;
								errorMessageBuilder.Append("the RemotePrinting Web Service needs to be upgraded and restarted.");
								errorMessageBuilder.AppendLine(message);
								break;

							case 537:
								shouldDisplayFullException = false;
								shouldSleepIfProtocolError = true;
								errorMessageBuilder.Append("the database is in the process of being upgraded.");
								errorMessageBuilder.AppendLine(message);
								break;

							case 538:
								shouldDisplayFullException = false;
								shouldSleepIfProtocolError = true;
								errorMessageBuilder.Append("all pooled database connections are in use, please try again later or contact us if the problem persists.");
								errorMessageBuilder.AppendLine(message);
								break;

							default:
								shouldDisplayFullException = true;
								errorMessageBuilder.Append("an error.");
								errorMessageBuilder.AppendLine(message);
								break;
						}

						errorMessageBuilder
							.AppendLine()
							.AppendLine(ContactAdminMessage);

						if (shouldDisplayFullException)
						{
							errorMessageBuilder
								.Append("Exception details: ")
								.AppendLine(GetExceptionMessageAndStacktrace(webEx))
								.AppendLine();
						}

						if (shouldSleepIfProtocolError)
						{
							errorMessageBuilder
								.AppendLine()
								.AppendLine("Retrying in 120 seconds.");
						}

						OnShowError(errorMessageBuilder.ToString());

						handled = responseCode != (int)HttpStatusCode.BadRequest || WebServiceClient.ValidOperationExecutedSuccessfullyBefore(operationName);

						// If there is a problem connecting to the server, wait a few minutes before retrying
						if (shouldSleepIfProtocolError)
						{
							ForceUpdateCheck();

							for (int i = 0; i < 60; i++)
							{
								Thread.Sleep(2000);
								if (ShouldStop)
								{
									break;
								}
							}
						}
					}
				}
			}

			return handled;
		}

		Regex WebExceptionRegex =>
			webExceptionRegex ?? (webExceptionRegex =
				new Regex(@"^(\w+\s+)+status\s+(?<num>400|401|408|413|500|502|503|504|525|530|531|532|533|534|535|536|537|538)\b[^a-z]*(?<msg>.+)$",
					RegexOptions.IgnoreCase | RegexOptions.Compiled));
		Regex webExceptionRegex;

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Handles HttpClientException for SignalR.")]
		bool HandleSignalRHttpClientException(HttpClientException clientException)
		{
			var handled = false;

			if (clientException != null)
			{
				var error = clientException.GetError();

				var errorMessageBuilder = new StringBuilder();
				errorMessageBuilder
					.AppendLine("SignalR has some errors, the error response body as below: ")
					.AppendLine(error.ResponseBody);

				if (error.StatusCode == HttpStatusCode.BadRequest)
				{
					handled = true;
				}

				errorMessageBuilder
					.AppendLine("Exception details: ")
					.AppendLine(GetExceptionMessageAndStacktrace(clientException));

				OnShowInformation(errorMessageBuilder.ToString());
			}

			return handled;
		}

		#endregion

		#region Event Firing

		readonly ISynchronizeInvoke SyncInvoke;
		protected void OnShowInformation(string message)
		{
			ShowInformation?.Invoke(SyncInvoke, new LogEventArgs(message));
		}

		protected void OnShowInformation(object sender, LogEventArgs e)
		{
			OnShowInformation(e.Message);
		}

		protected void OnProcessStarting(string message)
		{
			ProcessStarting?.Invoke(SyncInvoke, new LogEventArgs(message));
		}

		protected void OnProcessStopped(string message)
		{
			ProcessStopped?.Invoke(SyncInvoke, new LogEventArgs(message));
		}

		protected void OnProcessStarted()
		{
			ProcessStarted?.Invoke(SyncInvoke, new EventArgs());
		}

		protected void OnProcessFailed(string message)
		{
			ProcessFailed?.Invoke(SyncInvoke, new LogEventArgs(message));
		}

		protected void OnUpdated(string message)
		{
			Updated?.Invoke(SyncInvoke, new LogEventArgs(message));
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Notifies of update success.")]
		protected void OnUpdated(object sender, EventArgs e)
		{
			OnUpdated("Update successful");
		}

		protected void OnShowError(string message)
		{
			ShowError?.Invoke(SyncInvoke, new LogEventArgs(message));
		}

		protected void OnShowError(object sender, LogEventArgs e)
		{
			OnShowError(e.Message);
		}

		protected void OnRestartApplication(object sender, RestartApplicationEventArgs e)
		{
			RestartApplication?.Invoke(SyncInvoke, e);
		}

		#endregion

		#region Remote Printing WebService Client

		protected virtual WebClient WebServiceClient
		{
			get
			{
				if (fWebServiceClient == null)
				{
					fWebServiceClient = new WebClient(new RemotePrintingServiceAdaptor(ConfigSetting, this));
				}

				return fWebServiceClient;
			}
		}

		protected WebClient fWebServiceClient;

		#endregion

		#region INotifications

		void INotifications.AddMessage(string message)
		{
			OnShowInformation(message);
		}

		#endregion
	}
}
