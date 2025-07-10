using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.RemotePrinting.Engine;
using Microsoft.AspNet.SignalR.Client;
using WTG.ErrorReporting;

namespace Enterprise.RemotePrinting.Client
{
	public class ErrorReporter
	{
		public static void Enable()
		{
			_ = Instance;
		}

		public static ErrorReporter Instance
		{
			get
			{
				if (instance == null)
				{
					lock (locker)
					{
						instance = instance ?? SetInstance(GetDefaultInstance());
					}
				}
				return instance;
			}

			set
			{
				lock (locker)
				{
					SetInstance(value);
				}
			}
		}

		static readonly object locker = new object();
		const int MaxRetries = 3;

		static ErrorReporter GetDefaultInstance()
		{
#if DEBUG
			if (ClientRunningTestingState.IsRunningTests)
			{
				return null;
			}
#endif
			return new ErrorReporter();
		}

		static ErrorReporter SetInstance(ErrorReporter reporter)
		{
			instance?.UnhookUnhandledExceptions();
			instance = reporter;
			instance?.HookUnhandledExceptions();
			return instance;
		}

		internal static ErrorReporter InstanceNoInitialize => instance;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Static fields in this class do not need to be thread-static")]
		static ErrorReporter instance;

		public static void ReportError(string message, Exception ex = null)
		{
			ReportError(null, message, ex);
		}

		public static void ReportError(string key, string message, Exception ex = null)
		{
#if DEBUG
			SetLastReported(key, message, ex);
#endif

			Instance?.ReportErrorCoreAsync(key, message, ex);
		}

		protected virtual Task ReportErrorCoreAsync(string key, string message, Exception ex)
		{
			var reportBuilder = CreateReportBuilder();

			reportBuilder.SetRandomErrorReportID();

			if (!string.IsNullOrEmpty(key))
			{
				reportBuilder.SetKey(key);
			}

			FillSessionDetails(reportBuilder);

			// Keep message (exception description) at the end of root level information just before exception details
			reportBuilder.SetExceptionDescription(message);

			if (ex != null)
			{
				reportBuilder.SetRootException(ex);
			}
			else
			{
				// Create temporary exception and stack trace to include into error report
				reportBuilder.SetRootException(new ApplicationException(message), new StackTrace());
			}

			FillHttpRequestDetails(reportBuilder, ex);

			FillWindowsEventErrors(reportBuilder);

			var client = GetErrorReportingClient(new Uri(WellKnownServiceUris.Production));

			return PostCrashReportAsync(client, reportBuilder, 0);
		}

		protected virtual Task PostCrashReportAsync(IErrorReportingClient client, EnterpriseErrorReportBuilder reportBuilder, int retryCounter)
		{
			return client.PostCrashReportAsync(reportBuilder).ContinueWith(t =>
			{
				if (t.Exception != null && !HandleException(t.Exception))
				{
					throw t.Exception;
				}

				retryCounter++;

				if (retryCounter <= MaxRetries)
				{
					Thread.Sleep(100);
					PostCrashReportAsync(client, reportBuilder, retryCounter);
				}
			}, TaskContinuationOptions.OnlyOnFaulted);
		}

		bool HandleException(Exception ex)
		{
			var result = false;

			while (ex != null)
			{
				switch (ex)
				{
					case WebException webException:
						result = HandleWebException(webException);
						break;
					case HttpRequestException httpRequestException when !(httpRequestException.InnerException is WebException):
						result = HandleHttpRequestException(httpRequestException);
						break;
					case TaskCanceledException _: // Probably caused by timeout on error reporting service. As error report could have been delivered to the service before timeout on receiving reply, we should not try to resend the same error report.
						result = true;
						break;
				}

				if (ex is AggregateException aggregateEx && aggregateEx.InnerExceptions.Count > 1)
				{
					result = aggregateEx.InnerExceptions.All(HandleException);
					break;
				}

				ex = ex.InnerException;
			}

			return result;
		}

#region Report once

		public static void ReportOnce(string key, string message, Exception ex = null)
		{
			Instance?.ReportOnceCore(key, message, ex);
		}

		void ReportOnceCore(string key, string message, Exception ex)
		{
			if (string.IsNullOrEmpty(key) && ex != null)
			{
				key = GetExceptionKey(ex);
			}

			Argument.NotNull(key, nameof(key));

			var now = GetCurrentTimeUtc();
			var lastReported = GetLastReportedTimeUtc(key);

			if ((now - lastReported).TotalDays >= 1.0)
			{
				ReportedTimes[key] = now;
				ReportErrorCoreAsync(key, message, ex);
				CleanOldReportedTimesIfNeeded();
			}
		}

		public static string GetExceptionKey(Exception exception)
		{
			Argument.NotNull(exception, nameof(exception));

			if (exception is AggregateException aggregateException &&
				aggregateException.InnerExceptions.Count <= 1 &&
				aggregateException.InnerException != null)
			{
				exception = aggregateException.InnerException;
			}

			var baseException = exception.GetBaseException() ?? exception;
			var keyForLookup = baseException.GetType().FullName + baseException.Message;

			var stackTrace = baseException.StackTrace;
			if (string.IsNullOrEmpty(stackTrace))
			{
				stackTrace = new StackTrace().ToString();
			}
			using (var md5 = MD5.Create())
			{
				var hash = Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(stackTrace)));
				keyForLookup += hash;
			}

			return keyForLookup;
		}

		protected virtual DateTime GetCurrentTimeUtc() => DateTime.UtcNow;

		protected virtual DateTime GetLastReportedTimeUtc(string key)
		{
			if (ReportedTimes.TryGetValue(key, out var lastReportedTimeUtc))
			{
				return lastReportedTimeUtc;
			}

			return DateTime.MinValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "It uses snapshot array and safe TryRemove and TryAdd")]
		void CleanOldReportedTimesIfNeeded()
		{
			if (isCleaningOldReportedTimes || ReportedTimes.Count < CleanOldReportedTimesMinNumber)
			{
				return;
			}
			lock (cleanOldReportedTimesLock)
			{
				if (isCleaningOldReportedTimes || ReportedTimes.Count < CleanOldReportedTimesMinNumber)
				{
					return;
				}
				isCleaningOldReportedTimes = true;
				try
				{
					var oldDate = GetCurrentTimeUtc().AddDays(-1);

					var allItems = ReportedTimes.ToArray();
					var oldKeys = allItems.Where(kvp => kvp.Value <= oldDate).Select(kvp => kvp.Key).ToArray();

					foreach (var oldKey in oldKeys)
					{
						if (ReportedTimes.TryRemove(oldKey, out var currentDate) && currentDate > oldDate)
						{
							// Date for this item was already updated - put it back
							ReportedTimes.TryAdd(oldKey, currentDate);
						}
					}
				}
				finally
				{
					isCleaningOldReportedTimes = false;
				}
			}
		}

		ConcurrentDictionary<string, DateTime> ReportedTimes => reportedTimes ?? (reportedTimes = new ConcurrentDictionary<string, DateTime>());
		ConcurrentDictionary<string, DateTime> reportedTimes;

		bool isCleaningOldReportedTimes;
		const int CleanOldReportedTimesMinNumber = 1000;
		readonly object cleanOldReportedTimesLock = new object();

#endregion

		bool HandleWebException(WebException webException)
		{
			return webException.Status == WebExceptionStatus.NameResolutionFailure
				|| webException.Status == WebExceptionStatus.SecureChannelFailure
				|| webException.Status == WebExceptionStatus.SendFailure
				|| webException.Status == WebExceptionStatus.ReceiveFailure;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Handles specific HTTP request errors for better diagnostics.")]
		internal static bool HandleHttpRequestException(HttpRequestException httpRequestException, StringBuilder errorMessageBuilder = null)
		{
			if (HttpRequestExceptionPattern.IsMatch(httpRequestException.Message))
			{
				if (errorMessageBuilder != null)
				{
					errorMessageBuilder.Append(" Error message: ").Append(GetExceptionMessage(httpRequestException));
				}
				return true;
			}

			if (httpRequestException.InnerException is IOException ioException && ioException.HResult == ConnectionDroppedWhileSending)
			{
				if (errorMessageBuilder != null)
				{
					errorMessageBuilder.Append(" Error message: ").Append(GetExceptionMessage(ioException));
				}
				return true;
			}

			return false;
		}

		public const int ConnectionDroppedWhileSending = unchecked((int)0x80131620); //- 2146232800 The underlying connection has been closed. An error occurred while sending.

		static readonly Regex HttpRequestExceptionPattern = new Regex(@"^(?<message>.+):\s+(?<code>401|403|404|408|503)\s+\((?<reason>.+)\).$", RegexOptions.Compiled);

#region Report building

		int sequence;

		public void ResetSequence() => sequence = 0;

		static EnterpriseErrorReportBuilder CreateReportBuilder()
		{
			var reportBuilder = new EnterpriseErrorReportBuilder();
			reportBuilder.AddContributor(new WebExceptionExtension());
			reportBuilder.AddContributor(new HttpClientExceptionExtension());
			return reportBuilder;
		}

#if DEBUG
		public static EnterpriseErrorReportBuilder CreateReportBuilderExposedForTest() => CreateReportBuilder();
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "commandLine")]
		void FillSessionDetails(EnterpriseErrorReportBuilder reportBuilder)
		{
			reportBuilder.SetTimeOfException(DateTime.UtcNow);

			reportBuilder.SetCompany(GetWebServiceUrl());
			reportBuilder.SetSessionID(GetSessionId());
			reportBuilder.SetSequence((++sequence).ToString(CultureInfo.InvariantCulture));

			reportBuilder.SetProcessDetails(Process.GetCurrentProcess());

			var versionString = UpdateProcessor.GetInstalledVersion();
			if (!string.IsNullOrEmpty(versionString) && Version.TryParse(versionString, out var version))
			{
				reportBuilder.SetVersion(version);
			}

			SetExeCreationTime(reportBuilder);

			string commandLine;
			try
			{
				commandLine = System.Environment.CommandLine;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				commandLine = "Failed to get command line information: " + ex.Message;
			}
			reportBuilder.SetCustomValue("CommandLine", commandLine);
		}

		string GetSessionId()
		{
			return SessionId;
		}

		readonly string SessionId = Guid.NewGuid().ToString();

		protected string GetWebServiceUrl()
		{
			string url;
			if (ConnectionRegistryManager.Instance.EdiWebPrintKeyIsLoaded)
			{
				url = ConnectionRegistryManager.Instance.GetRemotePrintingWebServiceUrl() ?? string.Empty;
			}
			else
			{
				url = "ediWebPrint";
			}

			url = url.Replace("http://", string.Empty);
			url = url.Replace("https://", string.Empty);
			url = url.Replace("/RemotePrintingService.asmx", string.Empty);
			return url;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1089:DoNotUseAssemblyGetEntryAssemblyAnalyzer", Justification = "Baseline")]
		void SetExeCreationTime(EnterpriseErrorReportBuilder reportBuilder)
		{
			var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
			if (string.IsNullOrEmpty(assembly.Location))
			{
				return;
			}

			var fileInfo = new FileInfo(assembly.Location);
			if (!fileInfo.Exists)
			{
				return;
			}

			fileInfo.Refresh();
			reportBuilder.SetExeCreationTime(fileInfo.CreationTimeUtc);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Property")]
		void FillHttpRequestDetails(EnterpriseErrorReportBuilder reportBuilder, Exception ex)
		{
			while (ex != null)
			{
				// Can only get request from HttpClientException but not from WebException
				if ((ex is HttpClientException httpClientException) && (httpClientException.Response?.RequestMessage != null))
				{
					var requestMessage = httpClientException.Response.RequestMessage;

					var requestUrl = requestMessage.RequestUri.ToString();
					reportBuilder.SetRequestURL(requestUrl);
					reportBuilder.SetHttpRequestClientAddress(requestUrl);

					foreach (var header in requestMessage.Headers)
					{
						reportBuilder.AddHttpRequestHeader(header.Key, header.Value == null ? string.Empty : string.Join(",", header.Value));
					}

					foreach (var property in requestMessage.Properties)
					{
						reportBuilder.AddHttpRequestHeader("[Property] " + property.Key, property.Value == null ? string.Empty : property.Value.ToString());
					}

					break;
				}

				ex = ex.InnerException;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Application")]
		void FillWindowsEventErrors(EnterpriseErrorReportBuilder reportBuilder)
		{
			var eventLog = new EventLog
			{
				Log = "Application"
			};

			try
			{
				var lastErrors = new List<EventLogEntry>();

				for (int index = eventLog.Entries.Count - 1; index > 0; index--)
				{
					var eventLogEntry = eventLog.Entries[index];

					if (eventLogEntry.EntryType == EventLogEntryType.Error)
					{
						if (eventLogEntry.Source.Equals(".NET Runtime", StringComparison.OrdinalIgnoreCase) ||
							eventLogEntry.Source.IndexOf("Remote Printing", StringComparison.OrdinalIgnoreCase) >= 0 ||
							eventLogEntry.Source.IndexOf("RemotePrinting", StringComparison.OrdinalIgnoreCase) >= 0 ||
							eventLogEntry.Source.IndexOf("WebPrint", StringComparison.OrdinalIgnoreCase) >= 0)
						{
							lastErrors.Add(eventLogEntry);
						}

						if (lastErrors.Count >= 3)
						{
							break;
						}
					}

					if ((DateTime.Now - eventLogEntry.TimeGenerated).TotalDays > 1)
					{
						break;
					}
				}

				for (var i = 0; i < lastErrors.Count; i++)
				{
					var errorLogEntry = lastErrors[i];

					var messageSB = new StringBuilder();
					messageSB
						.Append(errorLogEntry.TimeGenerated.ToString("yyyy-MM-dd HH:mm:ss"))
						.Append(" - ").Append(errorLogEntry.Source)
						.Append(": ").Append(errorLogEntry.Message);

					reportBuilder.SetCustomValueWithSection("WindowsEventLog", "Event", messageSB.ToString());
				}
			}
			catch (Exception ex)
			{
				reportBuilder.SetCustomValueWithSection("WindowsEventLog", "ErrorRetrievingLogs", ex.Message);
			}
		}

		#endregion

		#region Unhandled exceptions events

		public void HookUnhandledExceptions()
		{
			UnhookUnhandledExceptions();
			HookUnhandledExceptionsCore();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1067:DoNotApplicationThreadException", Justification = "Baseline")]
		protected virtual void HookUnhandledExceptionsCore()
		{
			Application.ThreadException += HandleThreadException;
			AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
			TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
		}

		public void UnhookUnhandledExceptions()
		{
			UnhookUnhandledExceptionsCore();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1067:DoNotApplicationThreadException", Justification = "Baseline")]
		protected virtual void UnhookUnhandledExceptionsCore()
		{
			Application.ThreadException -= HandleThreadException;
			AppDomain.CurrentDomain.UnhandledException -= HandleUnhandledException;
			TaskScheduler.UnobservedTaskException -= HandleUnobservedTaskException;
		}

		void HandleThreadException(object sender, ThreadExceptionEventArgs e)
		{
			HandleOrReport(e.Exception, true);
		}

		void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			HandleOrReport(e.ExceptionObject as Exception, false);
		}

		void HandleUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			e.SetObserved();
			HandleOrReport(e.Exception, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "AppendLine")]
		public void HandleOrReport(Exception ex, bool reportOnce)
		{
			if (ex is CargoWise.PdfiumWrapper.CannotFoundPdfiumLibraryException || ex is CargoWise.PdfiumWrapper.CannotDestroyPdfiumLibraryException)
			{
				if (RestartApplication != null)
				{
					var args = new RestartApplicationEventArgs(ex.Message);
					RestartApplication.Invoke(Instance, args);
					if (args.Restarted)
					{
						return;
					}
				}
			}

			var errorHandlingArgs = new ErrorHandlingArgs(ex, false);

			HandleError?.Invoke(Instance, errorHandlingArgs);

			if (!errorHandlingArgs.Handled)
			{
				var sb = new StringBuilder();
				sb.AppendLine(ex?.Message ?? "Unhandled exception");
				if (HandleError == null)
				{
					sb.AppendLine("HandleError is not hooked");
				}

				if (reportOnce && ex != null)
				{
					var key = GetExceptionKey(ex);
					ReportOnceCore(key, sb.ToString(), ex);
				}
				else
				{
					ReportErrorCoreAsync(null, sb.ToString(), ex);
				}
			}
		}

		public event EventHandler<ErrorHandlingArgs> HandleError;

		#endregion

		internal static string GetExceptionMessage(Exception ex)
		{
			switch (ex)
			{
				case null:
					return string.Empty;
				case AggregateException agg when agg.InnerExceptions.Count == 1:
					return GetExceptionMessage(agg.InnerException);
				case AggregateException agg:
					var messages = agg.InnerExceptions.Select(GetExceptionMessage).Where(m => !string.IsNullOrEmpty(m)).ToList();
					return string.Join(System.Environment.NewLine, messages);
				case HttpRequestException httpEx when httpEx.InnerException is WebException webEx:
					return webEx.Message;
				case TargetInvocationException targetInvocationException when targetInvocationException.InnerException != null:
					return GetExceptionMessage(targetInvocationException.InnerException);
				default:
					return ex.InnerException is not null ? GetExceptionMessage(ex.InnerException) : ex.Message;
			}
		}

		internal static void FillExceptionMessageAndStacktrace(Exception ex, StringBuilder sb)
		{
			while (ex != null)
			{
				sb.AppendLine(ex.Message);
				sb.AppendLine(ex.GetType().FullName);

				if (ex is WebException wex)
				{
					FillWebExceptionDetails(wex, sb);
				}

				sb.AppendLine(ex.StackTrace);

				ex = ex.InnerException;
				if (ex != null)
				{
					sb.AppendLine("-----");
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Formats WebException details for logging purposes.")]
		public static void FillWebExceptionDetails(WebException wex, StringBuilder sb)
		{
			sb.Append("Web Exception Status: ").AppendLine(wex.Status.ToString());
			if (wex.Response is HttpWebResponse httpResponse)
			{
				sb.Append("Response Code: ").AppendLine(httpResponse.StatusCode.ToString());
			}
		}

		public void SendNotificationEmailOrReport(string subject, string body)
		{
			var args = new SendNotificationEmailForErrorReporterEventArgs(subject, body);

			SendNotificationEmail?.Invoke(Instance, args);

			if (!args.Handled)
			{
				ReportError(subject, body);
			}
		}

		public event EventHandler<SendNotificationEmailForErrorReporterEventArgs> SendNotificationEmail;

		protected virtual IErrorReportingClient GetErrorReportingClient(Uri serviceUri) => new ErrorReportingClient(serviceUri);

		#region Restart

		public event EventHandler<RestartApplicationEventArgs> RestartApplication;

		#endregion

		#region Test stuff
#if DEBUG

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "For testing only")]
		public static string LastKeyReported { get; private set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "For testing only")]
		public static string LastMessageReported { get; internal set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "For testing only")]
		public static Exception LastExceptionReported { get; private set; }

		public static void ClearLastReported()
		{
			LastKeyReported = "";
			LastMessageReported = "";
			LastExceptionReported = null;
		}

		static void SetLastReported(string key, string message, Exception ex)
		{
			LastKeyReported = key;
			LastMessageReported = message;
			LastExceptionReported = ex;
		}

#endif
		#endregion
	}

	public class ErrorHandlingArgs : EventArgs
	{
		public Exception Exception { get; }

		public bool Handled { get; set; }

		public ErrorHandlingArgs(Exception ex, bool handled)
		{
			Exception = ex;
			Handled = handled;
		}
	}

	public class SendNotificationEmailForErrorReporterEventArgs : SendNotificationEmailEventArgs
	{
		public bool Handled { get; set; }

		public SendNotificationEmailForErrorReporterEventArgs(string subject, string body)
			: base(subject, body)
		{
		}
	}

	public class RestartApplicationEventArgs : EventArgs
	{
		public RestartApplicationEventArgs(string message)
		{
			Message = message;
		}

		public bool Restarted { get; set; }

		public string Message { get; }
	}

#if DEBUG

	public static class ClientRunningTestingState
	{
		public static bool IsRunningTests
		{
			get
			{
				var type = Type.GetType("NUnit.Framework.TestingState, NUnitCore");
				if (type != null)
				{
					var isRunningTestsMethod = type.GetProperty("IsRunningTests", BindingFlags.Static | BindingFlags.Public);
					return isRunningTestsMethod != null && (bool)isRunningTestsMethod.GetValue(null);
				}

				return false;
			}
		}
	}
#endif
}
