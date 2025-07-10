using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Web.Services.Protocols;
using CargoWise.Common;
using Enterprise.RemotePrinting.Client.RemotePrintServer;
using Microsoft.Win32;
using static System.FormattableString;

namespace Enterprise.RemotePrinting.Client
{
	public class RemotePrintingServiceAdaptor : IRemotePrintingServiceAdaptor
	{
		public RemotePrintingServiceAdaptor(WebClientConfiguration config, INotifications notifications = null)
			: this(config, new RemotePrintingService(), new RetryProcessor(notifications))
		{
		}

		public RemotePrintingServiceAdaptor(WebClientConfiguration config, RetryProcessor retryProcessor)
			: this(config, new RemotePrintingService(), retryProcessor)
		{
		}

		public RemotePrintingServiceAdaptor(WebClientConfiguration config, RemotePrintingService printingService, RetryProcessor retryProcessor)
		{
			this.printingService = printingService;
			this.printingService.Timeout = config.RemotePrintingServiceTimeoutInSeconds * 1000;
			this.printingService.CookieContainer = new CookieContainer();

			this.ResponseProcessor = new ErrorResponseWebRequestProcessor(printingService);

			this.retryProcessor = retryProcessor;
			this.retryProcessor.SimpleRequestDelegate = this.printingService.CheckClientUpdate;
			this.retryProcessor.ResponseProcessor = ResponseProcessor;
			this.IsSupportUser = config.WebServiceUser != null && config.WebServiceUser.StartsWith(SupportUserPrefix, StringComparison.OrdinalIgnoreCase);
		}

		const string SupportUserPrefix = "CWSupport-";
		public bool IsSupportUser { get; }

#if DEBUG
		protected
#endif
		readonly RemotePrintingService printingService;
		readonly RetryProcessor retryProcessor;
		internal ClientInfo clientInfo;

#if DEBUG
		protected
#endif
		readonly TimeSpan NoTimeLimit = TimeSpan.Zero;

#if DEBUG
		protected
#endif
		/// <summary>
		/// Will allow to retry operation on immediate/quick failure, but will prevent retry after slow failures (e.g. timeout after 100 seconds).
		/// To be used with operations, where immediate retry is not necessary to continue normal work.
		/// </summary>
		readonly TimeSpan Limit10Seconds = TimeSpan.FromSeconds(10);

		Action<string> OnShowInformation { get; set; }

		public HashSet<string> ExecutedSuccessfullyOperations => retryProcessor.RetrySuccessfullyOperations;

		public IErrorResponseWebRequestProcessor ResponseProcessor { get; }

		public void SetWebServiceUrlAndCredentials(WebClientConfiguration config, Action<string> onShowInformation = null)
		{
			OnShowInformation = onShowInformation;

			SetupClientInfo(config);

			printingService.Url = config.WebServiceUrl;
			printingService.Credentials = new NetworkCredential(config.WebServiceUser, ProtectedDataHelper.Unprotect(config.WebServicePwd));
			printingService.AllowAutoRedirect = false; // Set it to false implicitly (default value is true)
			printingService.SoapVersion = SoapProtocolVersion.Soap12;
			printingService.RemotePrintingSoapHeaderValue = PrepareSoapHeader();

			ResponseProcessor.LogInfo = onShowInformation;

			var proxy = Configurator.GetProxy(config);
			if (proxy != null)
			{
				printingService.Proxy = proxy;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1031:DoNotUseGetVersionExOrEnvironmentDotOSVersionRule", Justification = "Cannot access CargoWise.Common from WebPrint Client")]
		void SetupClientInfo(WebClientConfiguration config)
		{
			clientInfo = new ClientInfo
			{
				ClientVersion = UpdateProcessor.GetInstalledVersion(),
				OSVersion = System.Environment.OSVersion.Version.ToString(),
				DotNetVersion = GetDotNetVersionFromRegistry(config),
				MachineName = !string.IsNullOrWhiteSpace(config.LocalMachineName) ? config.LocalMachineName : System.Environment.MachineName,
			};
		}

		RemotePrintingSoapHeader PrepareSoapHeader()
		{
			var soapHeader = new RemotePrintingSoapHeader
			{
				VersionInfo = clientInfo.ClientVersion,
				OSVersion = clientInfo.OSVersion,
				InstalledDotNetVersion = clientInfo.DotNetVersion,
				LocalMachineName = clientInfo.MachineName,
			};

			if (string.IsNullOrEmpty(soapHeader.InstalledDotNetVersion) || string.IsNullOrEmpty(soapHeader.OSVersion))
			{
				// Issue 01275466
				// to ensure that empty value does not come from here

				var message = Invariant(
$@"RemotePrintingServiceAdaptor.PrepareSoapHeader() - some system details are empty.
VersionInfo: {soapHeader.VersionInfo}
OSVersion: {soapHeader.OSVersion}
InstalledDotNetVersion: {soapHeader.InstalledDotNetVersion}
LocalMachineName: {soapHeader.LocalMachineName}");

				ErrorReporter.ReportError("PrepareSoapHeader_EmptySystemDetails", message);
			}

			return soapHeader;
		}

		[SuppressMessage("CargoWiseOne", "CW1031:DoNotUseGetVersionExOrEnvironmentDotOSVersionRule", Justification = "VersionHelper does not contain required info")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant DotNetV4SetupSubKey")]
		string GetDotNetVersionFromRegistry(WebClientConfiguration config)
		{
			var requiredDotNetVersion = new Version(4, 8);

			const string DotNetV4SetupSubKey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
			using (var registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(DotNetV4SetupSubKey))
			{
				if (registryKey == null)
				{
					var message = GetErrorMessage("Cannot open Dot Net Framework release info registry key", null, null);
					ErrorReporter.ReportError("DotNetVersionCannotOpenRegistryKey", message);
				}
				else
				{
					var value = registryKey.GetValue("Release");
#if DEBUG
					if (installedDotNetReleaseForTest != null)
					{
						value = installedDotNetReleaseForTest;
					}
#endif
					if (value is int releaseNumber)
					{
						var netVersionText = CheckFor45PlusVersion(releaseNumber);

						if (netVersionText != null)
						{
							if (!Version.TryParse(netVersionText, out var netVersion))
							{
								var message = GetErrorMessage("Dot Net version is not parseable", releaseNumber.ToString(), netVersionText);
								ErrorReporter.ReportError("DotNetVersionCannotParse", message);
							}
							else
							{
								// At this point .Net Version was parsed without errors, web print server will check it with ClientRequirementDotnet version.
								return netVersionText;
							}
						}
					}
					else
					{
						var message = GetErrorMessage("Dot Net release number is not an integer", value?.ToString() ?? "[null]", null);
						ErrorReporter.ReportError("DotNetVersionReleaseIsNotNumber", message);
					}
				}

				// If came here - Dot Net version was not patched or was not recognized.
				// Return minimum required version as this application was built with that target version and if it runs there must be a compatible version available.
				OnShowInformation?.Invoke(
					Invariant($"There were an issue checking installed .Net Framework version. Please check and ensure that this machine has .Net Framework of version {requiredDotNetVersion} or higher installed."));

				return requiredDotNetVersion.ToString();
			}

			// Checking the version using >= enables forward compatibility.	
			string CheckFor45PlusVersion(int releaseNumber)
			{
				if (releaseNumber >= 533320)
				{ return "4.8.1"; }
				if (releaseNumber >= 528040)
				{ return "4.8"; }
				if (releaseNumber >= 461808)
				{ return "4.7.2"; }
				if (releaseNumber >= 461308)
				{ return "4.7.1"; }
				if (releaseNumber >= 460798)
				{ return "4.7"; }
				if (releaseNumber >= 394802)
				{ return "4.6.2"; }
				if (releaseNumber >= 394254)
				{ return "4.6.1"; }
				if (releaseNumber >= 393295)
				{ return "4.6"; }
				if (releaseNumber >= 379893)
				{ return "4.5.2"; }
				if (releaseNumber >= 378675)
				{ return "4.5.1"; }
				if (releaseNumber >= 378389)
				{ return "4.5"; }

				var message = GetErrorMessage("No match found for Dot Net release number", releaseNumber.ToString(), null);
				ErrorReporter.ReportError("DotNetVersionCantMatchReleaseNumber", message);

				return null;
			}

			string GetErrorMessage(string message, string releaseNumber, string dotNetVersion)
			{
				var sb = new StringBuilder();
				sb.AppendLine(message);
				sb.Append("VersionInfo: ").AppendLine(UpdateProcessor.GetInstalledVersion());
				sb.Append("OSVersion: ").AppendLine(System.Environment.OSVersion.Version.ToString());

				if (releaseNumber != null)
				{
					sb.Append("InstalledDotNetRelease: ").AppendLine(releaseNumber);
				}
				if (dotNetVersion != null)
				{
					sb.Append("InstalledDotNetVersion: ").AppendLine(dotNetVersion);
				}

				sb.Append("LocalMachineName: ").Append(!string.IsNullOrWhiteSpace(config.LocalMachineName) ? config.LocalMachineName : System.Environment.MachineName);

				return sb.ToString();
			}
		}

		public ClientUpdate CheckClientUpdate()
		{
			return retryProcessor.InvokeFunc(printingService.CheckClientUpdate, RetryProcessor.MaxRetries, NoTimeLimit);
		}

		public ClientUpdate CheckClientUpdate2()
		{
			return retryProcessor.InvokeFunc(printingService.CheckClientUpdate2, RetryProcessor.MaxRetries, NoTimeLimit, clientInfo);
		}

		public ServerPrintQueue[] GetChangedQueues(string serverName, string[] changedQueueNames)
		{
			return retryProcessor.InvokeFunc(printingService.GetChangedQueues, RetryProcessor.MaxRetries, NoTimeLimit, serverName, changedQueueNames);
		}

		public ServerPrintJob[] GetJobs(string serverName)
		{
			return retryProcessor.InvokeFunc(printingService.GetJobs, RetryProcessor.MaxRetries, NoTimeLimit, serverName);
		}

		public ServerPrintJobEx[] GetJobsCompressed(string serverName)
		{
			return retryProcessor.InvokeFunc(printingService.GetJobsCompressed, RetryProcessor.MaxRetries, NoTimeLimit, serverName);
		}

		public ServerPrintJobEx[] GetJobsCompressed2(string serverName)
		{
			return retryProcessor.InvokeFunc(printingService.GetJobsCompressed2, RetryProcessor.MaxRetries, NoTimeLimit, serverName);
		}

		public ServerWatermark GetWatermarkInfo()
		{
			return retryProcessor.InvokeFunc(printingService.GetWatermarkInfo, RetryProcessor.MaxRetries, NoTimeLimit);
		}

		public void SetJobFailure(PrintJobFailed[] processedPrintJobs)
		{
			retryProcessor.InvokeAction(printingService.SetJobFailureV2, RetryProcessor.MaxRetries, Limit10Seconds, processedPrintJobs);
		}

		public void SetJobSuccess(Guid[] processedPrintJobPks)
		{
			retryProcessor.InvokeAction(printingService.SetJobSuccess, RetryProcessor.MaxRetries, Limit10Seconds, processedPrintJobPks);
		}

		public void SetQueues(string serverName, string[] printQueueNames)
		{
			retryProcessor.InvokeAction(printingService.SetQueues, RetryProcessor.MaxRetries, Limit10Seconds, serverName, printQueueNames);
		}

		public void SetQueuesEx(string serverName, PrintQueueInfo[] printQueueNames)
		{
			retryProcessor.InvokeAction(printingService.SetQueuesEx, RetryProcessor.MaxRetries, Limit10Seconds, serverName, printQueueNames);
		}

		public void SendNotificationEmail(string subject, string body)
		{
			retryProcessor.InvokeAction(printingService.SendNotificationEmail, RetryProcessor.MaxRetries, NoTimeLimit, subject, body);
		}

		public CNSWClientSetting GetCNSWClientApplicationSetting(string machineName)
		{
			return retryProcessor.InvokeFunc(printingService.GetCNSWClientSetting, RetryProcessor.MaxRetries, NoTimeLimit, machineName);
		}

		public TWNCATKClientSetting GetTWNCATKClientSetting(string machineName)
		{
			return retryProcessor.InvokeFunc(printingService.GetTWNCATKClientSetting, RetryProcessor.MaxRetries, NoTimeLimit, machineName);
		}

		public void UpdateClientLogs(string recipientEmail, string fileName, byte[] fileData, string comments)
		{
			retryProcessor.InvokeAction(printingService.UpdateClientLogs, RetryProcessor.MaxRetries, NoTimeLimit, recipientEmail, fileName, fileData, comments);
		}

		public CLSMSClientSetting GetCLSMSClientSetting(string machineName)
		{
			return retryProcessor.InvokeFunc(printingService.GetCLSMSClientSetting, RetryProcessor.MaxRetries, NoTimeLimit, machineName);
		}

		public JPNACCSClientSetting GetJPNACCSClientSetting(string machineName)
		{
			return retryProcessor.InvokeFunc(printingService.GetJPNACCSClientSetting, RetryProcessor.MaxRetries, NoTimeLimit, machineName);
		}

#if DEBUG

		public static IDisposable OverrideInstalledDotNetVersionForTest(object versionForTest)
		{
			installedDotNetReleaseForTest = versionForTest;
			return new DisposableAction(() => { installedDotNetReleaseForTest = null; });
		}

		[ThreadStatic]
		static object installedDotNetReleaseForTest;

#endif
	}
}
