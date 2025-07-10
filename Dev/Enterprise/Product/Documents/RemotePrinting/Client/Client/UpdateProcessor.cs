using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using CargoWise.Common;
using Microsoft.Win32;

namespace Enterprise.RemotePrinting.Client
{
	public class UpdateProcessor : IUpdateProcessor
	{
		public event EventHandler Updated = delegate { };

		readonly IClientUpdate update;

		readonly IUpdateConfigurationProvider updateConfigurationProvider;
		readonly WebClientConfiguration systemConfiguration;
		WebClientUpdateConfiguration updateConfiguration;
		readonly IErrorResponseWebRequestProcessor responseProcessor;
		readonly Action<string> onShowInformation;

		public UpdateProcessor(IClientUpdate update, IUpdateConfigurationProvider updateConfigurationProvider = null, IErrorResponseWebRequestProcessor responseProcessor = null, Action<string> onShowInformation = null)
		{
			this.update = update;
			this.updateConfigurationProvider = updateConfigurationProvider;
			this.systemConfiguration = updateConfigurationProvider?.SystemConfiguration ?? new WebClientConfiguration();
			this.updateConfiguration = updateConfigurationProvider?.UpdateConfiguration ?? new WebClientUpdateConfiguration { Mode = WebClientUpdateConfiguration.UpdateMode.Automatic };
			this.responseProcessor = responseProcessor;
			this.onShowInformation = onShowInformation;
		}

		public void Process()
		{
			if (!IsUpdateRequired())
			{
				return;
			}

			try
			{
				var uri = new Uri(update.Link);
				var localFile = Path.Combine(
					System.Environment.GetFolderPath(System.Environment.SpecialFolder.InternetCache),
					uri.Segments[uri.Segments.Length - 1]
				);

				Download(localFile);
				Run(localFile);
			}
			catch (Exception ex)
			{
				throw new UpdateException(ex);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "WebPrintEventLogEntryType")]
		public bool IsUpdateRequired()
		{
			var v = GetInstalledVersion();
			if (string.IsNullOrEmpty(v))
			{
				LogWriter.Append(WebPrintEventLogEntryType.Information, "Cannot get client installed version");
				return false;
			}

			var installed = new Version(v);
			NotifyUpdateResultIfNeeded(installed);

			var onserver = new Version(update.Version);
			if (onserver.Major == 0 && onserver.Minor == 0 && !string.IsNullOrEmpty(update.Link))
			{
				LogWriter.Append(WebPrintEventLogEntryType.SystemInformation, "Remote Printing Server did not provide Client update package with error message:" + update.Link.Replace("\n", System.Environment.NewLine));
				return false;
			}

			if (installed > onserver)
			{
				SendNotificationWithClientHasNewerVersionThanServer(updateConfiguration, update, updateConfigurationProvider.MachineName, updateConfigurationProvider.SendNotification, AppendLog);
			}

			var canUpdate = new UpdateConfigurationChecker(updateConfigurationProvider).CanUpdate(installed, onserver);
			if (!canUpdate && installed < onserver)
			{
				LogWriter.Append(WebPrintEventLogEntryType.SystemInformation, "Cannot update to new version at this time.");
			}

			return canUpdate;
		}

		void AppendLog(string message) => LogWriter.Append(WebPrintEventLogEntryType.Information, message);

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "updateConfigurationProvider")]
		void NotifyUpdateResultIfNeeded(Version installed)
		{
			if (updateConfiguration.NotifyAfterUpdate && !string.IsNullOrEmpty(updateConfiguration.VersionBeforeUpdate))
			{
				try
				{
					var previousVersion = new Version(updateConfiguration.VersionBeforeUpdate);

					if (previousVersion == installed)
					{
						updateConfigurationProvider.SendNotification("Did not updated Remote Printing Client on print server " +
							updateConfigurationProvider.MachineName + " to new version, it remains on version " + installed);
					}
					else if (previousVersion < installed)
					{
						updateConfigurationProvider.SendNotification("Updated Remote Printing Client on print server " +
							updateConfigurationProvider.MachineName + " to version " + installed);
					}
				}
				catch (ArgumentOutOfRangeException ex)
				{
					ErrorReporter.ReportError("VersionBeforeUpdate_BadValue", ex);
				}
				catch (FormatException) { }
				catch (OverflowException) { }
			}
		}

		protected void Download(string localFile)
		{
			try
			{
				DownloadCore(localFile, update.Link);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var serviceUri = new Uri(systemConfiguration.WebServiceUrl);
				var updateUri = new Uri(update.Link);
				if (serviceUri.Scheme == updateUri.Scheme)
				{
					throw;
				}

				var newDownloadUrl = new UriBuilder(serviceUri.Scheme, updateUri.Host, serviceUri.Port, updateUri.PathAndQuery).ToString();
				onShowInformation?.Invoke($@"Download update file failed: {ex.Message}
Retry download update file with Url: {newDownloadUrl}");
				DownloadCore(localFile, newDownloadUrl);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "onShowInformation")]
		void DownloadCore(string localFile, string downloadUrl)
		{
			Stream remoteStream = null;
			Stream localStream = null;
			WebResponse response = null;

			try
			{
				WebRequest request = NewWebRequest(downloadUrl);
				if (request != null)
				{
					response = request.GetResponse();
					if (response != null)
					{
						if (responseProcessor != null && responseProcessor.ShouldHandleRedirectResponse(response as HttpWebResponse, out var newUrl))
						{
							onShowInformation?.Invoke("Download update file with Url: " + newUrl);

							request = NewWebRequest(newUrl);
							if (request == null)
							{
								return;
							}

							response = request.GetResponse();
							if (response == null)
							{
								return;
							}
						}

						remoteStream = response.GetResponseStream();
						localStream = File.Create(localFile);

						byte[] buffer = new byte[1024];
						int bytesRead;

						do
						{
							bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
							localStream.Write(buffer, 0, bytesRead);
						}
						while (bytesRead > 0);
					}
				}
			}
			finally
			{
				if (response != null)
				{
					response.Close();
				}

				if (remoteStream != null)
				{
					remoteStream.Close();
				}

				if (localStream != null)
				{
					localStream.Close();
				}
			}
		}

#if DEBUG
		protected virtual
#endif
		WebRequest NewWebRequest(string requestUriString)
		{
			var webRequest = WebRequest.Create(requestUriString);

			if (!string.IsNullOrWhiteSpace(systemConfiguration.WebServiceUser) && !string.IsNullOrWhiteSpace(systemConfiguration.WebServicePwd))
			{
				webRequest.Credentials = new NetworkCredential(systemConfiguration.WebServiceUser, ProtectedDataHelper.Unprotect(systemConfiguration.WebServicePwd));
			}

			var proxy = Configurator.GetProxy(systemConfiguration);
			if (proxy != null)
			{
				webRequest.Proxy = proxy;
			}

			if (webRequest is HttpWebRequest request)
			{
				request.AllowAutoRedirect = false; // Set it to false implicitly (default value is true)
			}

			return webRequest;
		}

		[SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Run message")]
		void Run(string localFile)
		{
			var v = GetInstalledVersion();

			if (updateConfiguration.NotifyBeforeUpdate)
			{
				var message = "Starting update of Remote Printing Client on print server " + updateConfigurationProvider.MachineName + " to new version " + update.Version;

				if (!string.IsNullOrEmpty(v))
				{
					message += System.Environment.NewLine + "Current installed version is " + v;
				}

				updateConfigurationProvider.SendNotification(message);
			}

			updateConfiguration.VersionBeforeUpdate = v;
			updateConfiguration.UpdateRunningDate = DateTime.Now;
			updateConfigurationProvider.SaveUpdateTrackingConfiguration(updateConfiguration);

			SetPackageNameRegistryValue(localFile);

			var logFilePath = Path.Combine(LogWriter.GetOutputDirectory().FullName, string.Format(CultureInfo.InvariantCulture, "InstallLog{0:_yyyyMMdd_HHmmss}.txt", DateTime.Now));
			var args = string.Format(CultureInfo.InvariantCulture, @"/i ""{0}"" MSIRESTARTMANAGERCONTROL=Disable REINSTALL=ALL REINSTALLMODE=vomus /quiet /norestart /l*v ""{1}""", localFile, logFilePath);

			var info = new ProcessStartInfo("msiexec.exe", args);
			info.CreateNoWindow = true;
			info.UseShellExecute = false;

			var process = System.Diagnostics.Process.Start(info);
			process.WaitForExit();

			//if (process.ExitCode == 0)
			Updated(this, EventArgs.Empty);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "These strings will not be displayed to the users.")]
		[SuppressMessage("Enterprise", "EDI012", Justification = "Registry path to .Net")]
		void SetPackageNameRegistryValue(string localFile)
		{
			try
			{
				var products = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Classes\\Installer\\Products");
				if (products != null)
				{
					foreach (var product in products.GetSubKeyNames())
					{
						var webprintClient = products.OpenSubKey(product);
						if (webprintClient?.GetValue("ProductName") as string == Constants.ApplicationName)
						{
							var sourceList = webprintClient.OpenSubKey("SourceList", true);
							sourceList?.SetValue("PackageName", Path.GetFileName(localFile), RegistryValueKind.String);
							break;
						}
					}
				}
			}
			catch (UnauthorizedAccessException unauthorizedAccessException)
			{
				LogWriter.Append(WebPrintEventLogEntryType.Error, $"Failed to set registry item value with an UnauthorizedAccessException thrown, the error message is {unauthorizedAccessException.Message}");
			}
			catch (SecurityException securityException)
			{
				LogWriter.Append(WebPrintEventLogEntryType.Error, $"Failed to set registry item value with a SecurityException thrown, the error message is {securityException.Message}");
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Log Invoke")]
		public static void SendNotificationWithClientHasNewerVersionThanServer(WebClientUpdateConfiguration updateConfiguration, IClientUpdate clientUpdate, string localMachineName, Action<string> sendNotificationEmail, Action<string> log)
		{
			log?.Invoke("WebPrint Client appears to have newer version than WebPrint Server.");

			var installedVersion = new Version(GetInstalledVersion());
			var newVersion = new Version(clientUpdate.Version);
			var today = DateTime.Today;
			if ((today - updateConfiguration.UpdateWithNotMatchingLastNotificationDate.Date).TotalDays >= 1)
			{
				var emailBody = new StringBuilder();
				emailBody
					.AppendLine($"There is a newer version of the WebPrint Client that is installed on the {localMachineName}.")
					.AppendLine($"The version of the WebPrint Client that is installed is {installedVersion}");

				if (newVersion.Major != 0)
				{
					emailBody.AppendLine($"The version of the WebPrint Client that is available on the Remote server is {newVersion}");
				}

				emailBody
					.AppendLine()
					.AppendLine($@"Please follow these steps to update the WebPrint Client to the version on the Remote server:
1. Uninstall the current version of the WebPrint Client on the {localMachineName}.
2. Download the latest WebPrint Client from {clientUpdate.Link} to the CargoWiseOneWebPrintClientIntSetup.msi.
3. Install the WebPrint Client on the {localMachineName}.
4. Start the WebPrint Client.");

				try
				{
					sendNotificationEmail?.Invoke(emailBody.ToString());
				}
				catch (Exception exception) when (!exception.IsCriticalException())
				{
					log?.Invoke("API for SendNotificationEmail does not exist on WebPrint Server");
				}

				updateConfiguration.UpdateWithNotMatchingLastNotificationDate = today;
				RegistryManager.SaveRemotePrintingUpdateWithNotMatchingConfigRegistryValuesCore(updateConfiguration);
			}
		}

		static ConnectionRegistryManager RegistryManager
		{
			get
			{
#if DEBUG
				if (connectionRegistryManagerForTest != null)
				{
					return connectionRegistryManagerForTest;
				}
#endif
				return ConnectionRegistryManager.Instance;
			}
		}

		public static string GetInstalledVersion()
		{
#if DEBUG
			if (installedVersionForTest != null)
			{
				return installedVersionForTest;
			}
#endif
			var version = ReadResource(VersionResourceName);
			if (string.IsNullOrEmpty(version))
			{
				version = GetProductProperty("{D8BCA8FF-5AB8-481B-9AFF-2FBD05446A4B}", "VersionString");
			}
			return version;
		}

#if DEBUG
		public static IDisposable OverrideConnectionRegistryManagerForTest(ConnectionRegistryManager registryManagerForTest)
		{
			connectionRegistryManagerForTest = registryManagerForTest;
			return new DisposableAction(() => { connectionRegistryManagerForTest = null; });
		}

		[ThreadStatic]
		static ConnectionRegistryManager connectionRegistryManagerForTest;

		public static IDisposable OverrideInstalledVersionForTest(string versionForTest)
		{
			installedVersionForTest = versionForTest;
			return new DisposableAction(() => { installedVersionForTest = null; });
		}

		[ThreadStatic]
		static string installedVersionForTest;
#endif

		#region Embedded Resource

#if DEBUG
		protected
#endif
		static string ReadResource(string name)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourcePath = assembly.GetManifestResourceNames().SingleOrDefault(str => str.EndsWith(name));
			if (!string.IsNullOrEmpty(resourcePath))
			{
				using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
				{
					if (stream != null)
					{
						using (StreamReader reader = new StreamReader(stream))
						{
							return reader.ReadToEnd();
						}
					}
				}
			}

			return string.Empty;
		}

#if DEBUG
		protected
#endif
		const string VersionResourceName = "Setup.version";

#endregion

#region Win32

		const string MsiDll = "msi.dll";

		public static string GetProductProperty(string product, string property)
		{
			int size = 2048;
			StringBuilder value = new StringBuilder(size, size);
			MsiGetProductInfo(product, property, value, ref size);
			return value.ToString();
		}

		[DllImport(MsiDll, CharSet = CharSet.Auto)]
		static extern uint MsiGetProductInfo(
				string product, string property, StringBuilder value, ref int size);

#endregion
	}

	[Serializable]
	public class UpdateException : Exception
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Base message")]
		public UpdateException(Exception innerException) : base("Update procedure failed", innerException) { }

#if NETFRAMEWORK
		protected UpdateException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
