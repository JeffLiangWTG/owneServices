using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using CargoWise.Common.Async.CausalityTracking;
using CargoWise.Common.ErrorManagement;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Common
{
	[ThreadSafe]
	public interface IErrorReporter
	{
		void Clear();
		void Report(string key, string message, Exception exception);
		void ReportDeveloperExceptionOrHandleSilently(string key, string message, Exception ex);
	}

	/// <summary>
	/// Reports application errors.
	/// </summary>
	public static class ErrorReporter
	{
		public static bool SuppressReportingOfErrors
		{
			get => suppressReportingOfErrors.Value;
			set => suppressReportingOfErrors.Value = value;
		}

		readonly static Overridable<bool> suppressReportingOfErrors = new Overridable<bool>(false);

		public static IDisposable SuppressErrorReporting()
		{
			var oldSuppressReportingOfErrors = SuppressReportingOfErrors;
			SuppressReportingOfErrors = true;

			return new DisposableAction(() =>
			{
				SuppressReportingOfErrors = oldSuppressReportingOfErrors;
			});
		}

		[ThreadSafe(ThreadSafeAttribute.Mechanism.Interlocked)]
		static int deferErrorRequestCount;

		[ThreadSafe]
		static readonly ConcurrentQueue<DeferredError> DeferredErrors = new ();

		public static IDisposable DeferReportingTemporarily()
		{
			Interlocked.Increment(ref deferErrorRequestCount);
			return new DisposableAction(() =>
			{
				if (Interlocked.Decrement(ref deferErrorRequestCount) == 0)
				{
					try
					{
						ProcessDeferredErrors();
					}
					catch
					{
						// Ignore exceptions as any exceptions thrown during dispose will hide exceptions that happened during using block.
						// The responsibility for handling exceptions during reporting is assumed upon the IErrorReporter implementations.
					}
				}
			});
		}

		static void QueueDeferredError(DeferredError deferredError)
		{
			DeferredErrors.Enqueue(deferredError);
		}

		static void ProcessDeferredErrors()
		{
			var elementsToProcess = new List<DeferredError>();
			//clear errors queue first so other threads won't add more errors while still processing
			while (DeferredErrors.TryDequeue(out var error))
			{
				elementsToProcess.Add(error);
			}

			ReportDeferredErrors(elementsToProcess);
		}

		static void ReportDeferredErrors(IEnumerable<DeferredError> elementsToProcess)
		{
			foreach (var deferredError in elementsToProcess)
			{
				InvokeReportStrategy(deferredError.Key, deferredError.Message, deferredError.Exception, deferredError.ReportStrategy);
			}
		}

		public static IEnumerable<Exception> FlattenInnerExceptions(this Exception ex)
		{
			yield return ex;

			while ((ex = ex.InnerException) != null)
			{
				yield return ex;
			}
		}

		public static int TotalErrorCount
		{
			get { return totalErrorCount; }
		}
		[ThreadSafe(WTG.StaticAnalysis.Annotation.ThreadSafeAttribute.Mechanism.Interlocked)]
		static int totalErrorCount;

		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static ErrorReporter()
		{
#if DEBUG
			LastKeyReported = "";
			LastMessageReported = "";
#endif
		}

		/// <summary>
		/// Get or set the error reporting handler interface.
		/// </summary>
		public static IErrorReporter Instance { get; set; }

		/// <summary>
		/// Clear any errors that have been collected for reporting.
		/// </summary>
		public static void Clear()
		{
			InstanceInternal.Clear();
			reportedKeys.Clear();
#if DEBUG
			_exceptionsThrown?.Clear();
			LastKeyReported = "";
			LastMessageReported = "";
			LastExceptionReported = null;
			totalErrorCount = 0;
#endif
		}

		#region LastException reported

		[ThreadSafe]
		static Queue<string> _exceptionsThrown;

#if DEBUG
		/// <summary>
		/// Getting the _exceptionsThrown Queue for test use only. RELEASE should use the LastExceptionsReported() method.
		/// <summary>
		public static Queue<string> ExceptionsThrown => _exceptionsThrown ??= new Queue<string>(15);
#endif
		readonly static object StaticLock = new object();

		public static void PopulateExceptionsBuffer(Exception ex)
		{
			lock (StaticLock)
			{
				_exceptionsThrown ??= new Queue<string>(15);

				if (_exceptionsThrown.Count > 14)
				{
					_exceptionsThrown.Dequeue();
				}

				_exceptionsThrown.Enqueue(ExceptionMessageBuilder());
			}

			string ExceptionMessageBuilder()
			{
				var message = new StringBuilder();
				message.AppendLine("At " + DateTime.UtcNow.ToString()).AppendLine("Type :" + ex.GetType().ToString());  // Default Values
				message.AppendLine("Message :" + ex.Message).AppendLine("Stacktrace :" + ex.StackTrace);                // Default Values
				return message.ToString();
			}
		}

		public static List<string> LastExceptionsReported()
		{
			lock (StaticLock)
			{
				return (_exceptionsThrown is not null) ? _exceptionsThrown.ToList() : new List<string>();
			}
		}
		#endregion

		/// <summary>
		/// Reports a message describing an error caused by incorrect program logic.
		/// </summary>
		/// <param name="message">The message to include in the error report.</param>
		/// <param name="exception">The exception to report.</param>
		public static void ReportOnce(string message, Exception exception = null)
		{
			ReportOnce(null, message, exception, null);
		}

		/// <summary>
		/// Reports a message describing an error caused by incorrect program logic, once for the current application domain.
		/// </summary>
		/// <param name="key">Identifier to prevent the developer error being reported more than once.</param>
		/// <param name="message">The message to show the developer or include in the error report.</param>
		public static void ReportOnce(string key, string message)
		{
			ReportOnce(key, message, null, null);
		}

		/// <summary>
		/// Reports an exception describing an error caused by incorrect program logic, once for the current application domain.
		/// </summary>
		/// <param name="key">The key to use for matching.</param>
		/// <param name="message">The message to show the developer or include in the error report.</param>
		/// <param name="exception">The exception to report.</param>
		public static void ReportOnce(string key, string message, Exception exception)
		{
			ReportOnce(key, message, exception, null);
		}

		/// <summary>
		/// Reports an exception describing an error caused by incorrect program logic, once for the current application domain.
		/// </summary>
		/// <param name="message">The message to show the developer or include in the error report.</param>
		/// <param name="exception">The exception to report.</param>
		/// <param name="additionalInfoFilterKeys">Filtered categories for additional information.</param>
		public static void ReportOnceWithAdditionalInfo(string message, Exception exception = null, params string[] additionalInfoFilterKeys)
		{
			ReportOnce(null, message, exception, additionalInfoFilterKeys);
		}

		/// <summary>
		/// Reports an exception describing an error caused by incorrect program logic, once for the current application domain.
		/// </summary>
		/// <param name="key">The key to use for matching.</param>
		/// <param name="message">The message to show the developer or include in the error report.</param>
		/// <param name="additionalInfoFilterKeys">Filtered categories for additional information.</param>
		public static void ReportOnceWithAdditionalInfo(string key, string message, params string[] additionalInfoFilterKeys)
		{
			ReportOnce(key, message, null, additionalInfoFilterKeys);
		}

		/// <summary>
		/// Reports an exception describing an error caused by incorrect program logic, once for the current application domain.
		/// </summary>
		/// <param name="key">The key to use for matching.</param>
		/// <param name="message">The message to show the developer or include in the error report.</param>
		/// <param name="exception">The exception to report.</param>
		/// <param name="additionalInfoFilterKeys">Filtered categories for additional information.</param>
		public static void ReportOnceWithAdditionalInfo(string key, string message, Exception exception, params string[] additionalInfoFilterKeys)
		{
			ReportOnce(key, message, exception, additionalInfoFilterKeys);
		}

		/// <summary>
		/// Reports an exception describing an error caused by incorrect program logic, once for the current application domain.
		/// </summary>
		/// <param name="key">The key to use for matching.</param>
		/// <param name="message">The message to show the developer or include in the error report.</param>
		/// <param name="exception">The exception to report.</param>
		/// <param name="additionalInfoFilterKeys">Filtered categories for additional information.</param>
		static void ReportOnce(string key, string message, Exception exception, params string[] additionalInfoFilterKeys)
		{
			//Implementation of IErrorReporter.Report() method in BaseExceptionReporter calls HandleException() and does not actually report exception if it is handled.
			//Check implementation of BaseExceptionReporter.HandleException(Exception exceptionToHandle, Exception outermostExceptionForErrorReport)
			ReportCore(key, message, exception, InstanceInternal.Report, additionalInfoFilterKeys);
		}

		public static void ReportOnce(this IErrorReporter errorReporterInstance, string key, string message, Exception exception, params string[] additionalInfoFilterKeys)
		{
			ReportCore(key, message, exception, errorReporterInstance.Report, additionalInfoFilterKeys);
		}

		/// <summary>
		/// Reports an exception describing a internal developer error, which should not perform any GUI or error web page.
		/// </summary>
		/// <param name="message">The message to show the developer or include in the error report.</param>
		/// <param name="exception">The exception to report or use Enterprise.ZArchitecture.Environment.DeveloperNotificationException.</param>
		public static void ReportDeveloperExceptionOnce(string message, Exception exception)
		{
			ReportDeveloperExceptionOnce(null, message, exception);
		}

		/// <summary>
		/// Reports an exception describing a internal developer error, which should not perform any GUI or error web page.
		/// </summary>
		/// <param name="key">The key to use for matching.</param>
		/// <param name="message">The message to show the developer or include in the error report.</param>
		/// <param name="exception">The exception to report or use Enterprise.ZArchitecture.Environment.DeveloperNotificationException.</param>
		public static void ReportDeveloperExceptionOnce(string key, string message, Exception exception)
		{
			ReportCore(key, message, exception, InstanceInternal.ReportDeveloperExceptionOrHandleSilently);
		}

		static void ReportCore(string key, string message, Exception exception, Action<string, string, Exception> reportStrategy, params string[] additionalInfoFilterKeys)
		{
#if DEBUG
			if (VisualStudioDetector.IsVisualStudio)
			{
				// Some UI code issues cause exceptions in VS designer to be processed and reported, which cause other exceptions (e.g. access to uninitialized DB connection).
				// Also going into this method will later call BaseExceptionReporter.HookStaticUnhandledExceptions(), which will hook to Unhandled and Thread exception events of VS process.
				// DesignModeFinder.IsDesigning just returns VisualStudioDetector.IsVisualStudio, and it is defined in E.ZA.Core, which is not accessible here.
				return;
			}
#endif

			if (SuppressReportingOfErrors)
			{
				Interlocked.Increment(ref totalErrorCount);
			}
			else
			{
				var keyForLookup = key;
				if (string.IsNullOrEmpty(keyForLookup))
				{
					using (var md5 = MD5.Create())
					{
						// no key provided - but will make one up to stop duplicates being sent to us
						if (exception != null)
						{
							var ex = exception.GetBaseException();
							if (ex != null)
							{
								keyForLookup = ex.GetType().FullName;
							}
						}

						var stackTrace = GetExceptionStackTrace(exception)
										 ?? new StackTrace().ToString();

						var hash = Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(stackTrace)));
						keyForLookup += hash;
					}
				}

				var isFirstReport = keyForLookup != null && reportedKeys.TryAdd(keyForLookup);
				var forceReport = exception is IErrorReporterExtender extender && extender.ShouldReportAlwaysInReportOnce;
				if (isFirstReport || forceReport)
				{
					if (exception is AggregateException aggregateException)
					{
						message = string.Join(Environment.NewLine,
							new[] { message }.Concat(
								aggregateException
									.Flatten()
									.InnerExceptions
									.SelectMany(ex => ex.Message.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
								));
					}

					if (additionalInfoFilterKeys != null)
					{
						message += GetAdditionalInformation(additionalInfoFilterKeys);
					}

					InvokeReportStrategy(key, message, exception, reportStrategy);
				}
				ThrowExceptionForDebugger();
			}

			string GetExceptionStackTrace(Exception ex)
			{
				string exceptionStackTrace = null;
				if (ex != null)
				{
					if (!CausalityTracker.TryGetDependencyChainedStack(exception.GetBaseException(), out exceptionStackTrace))
					{
						exceptionStackTrace = ex.StackTrace;
					}
				}

				return exceptionStackTrace;
			}
		}

		static void InvokeReportStrategy(string key, string message, Exception exception, Action<string, string, Exception> reportStrategy)
		{
			if (deferErrorRequestCount > 0)
			{
				QueueDeferredError(DeferredError.New(key, message, exception, reportStrategy));
				return;
			}
			
			reportStrategy.Invoke(key, message, exception);
			SetLastReported(key, message, exception);
			Interlocked.Increment(ref totalErrorCount);
		}

		/// <summary>
		/// Sets Additional Information as a string with no category. This information is printed when inside a GatherAdditionalInformation() disposable and when ReportOnceWithAdditionalInfo is used.
		/// </summary>
		/// <param name="key">The key to use for storage.</param>
		/// <param name="value">String value set</param>
		public static void SetAdditionalInfo(string key, string value)
		{
			SetAdditionalInfoCore(key, () => value);
		}

		/// <summary>
		/// Sets Additional Information as a string for a specific category. This information is printed when inside a GatherAdditionalInformation() disposable and when ReportOnceWithAdditionalInfo is used.
		/// </summary>
		/// <param name="category">The category use for key aggregation.</param>
		/// <param name="key">The key to use for storage.</param>
		/// <param name="value">String value set</param>
		public static void SetAdditionalInfo(string category, string key, string value)
		{
			if (AdditionalInformation != null)
			{
				SetAdditionalInfoCore(category, key, () => value);
			}
		}

		/// <summary>
		/// Sets Additional Information as a lazy function (Processed by reference on ErrorReport) with no category. This information is printed when inside a GatherAdditionalInformation() disposable and when ReportOnceWithAdditionalInfo is used.
		/// </summary>
		/// <param name="key">The key to use for storage.</param>
		/// <param name="value">Lazy string value</param>
		public static void SetAdditionalInfo(string key, Func<string> value)
		{
			SetAdditionalInfoCore(key, value);
		}

		/// <summary>
		/// Sets Additional Information as a lazy function (Processed by reference on ErrorReport) for a specific category. This information is printed when inside a GatherAdditionalInformation() disposable and when ReportOnceWithAdditionalInfo is used.
		/// </summary>
		/// <param name="category">The category use for key aggregation.</param>
		/// <param name="key">The key to use for storage.</param>
		/// <param name="value">Lazy string value</param>
		public static void SetAdditionalInfo(string category, string key, Func<string> value)
		{
			SetAdditionalInfoCore(category, key, value);
		}

		/// <summary>
		/// Adds a Stack Trace to Additional Information for a specific category. This information is printed when inside a GatherAdditionalInformation() disposable and when ReportOnceWithAdditionalInfo is used.
		/// </summary>
		/// <param name="category">The category use for key aggregation.</param>
		/// <param name="key">The key to use for storage.</param>
		public static void AddStackTraceToAdditionalInfo(string category, string key)
		{
			if (AdditionalInformation != null)
			{
				var stackTrace = new StackTrace().ToString();
				SetAdditionalInfoCore(category, key, () => stackTrace);
			}
		}

		/// <summary>
		/// Sets Additional Information as a string if conditional flag has been set by CreateInfoFlag. This information is printed when inside a GatherAdditionalInformation() disposable and when ReportOnceWithAdditionalInfo is used.
		/// </summary>
		/// <param name="key">The key to use for storage.</param>
		/// <param name="value">String value set</param>
		/// <param name="conditionalFlagKey">Conditional flag key</param>
		public static void SetAdditionalInfoIfFlagSet(string key, string value, string conditionalFlagKey)
		{
			if (AdditionalInformation != null)
			{
				if (AdditionalInformation.HasInfoFlag(conditionalFlagKey))
				{
					SetAdditionalInfoCore(key, () => value);
				}
			}
		}

		/// <summary>
		/// Sets Additional Information as a lazy function (Processed by reference on ErrorReport) with no category. This information is printed when inside a GatherAdditionalInformation() disposable and when ReportOnceWithAdditionalInfo is used.
		/// </summary>
		/// <param name="key">The key to use for storage.</param>
		/// <param name="value">Lazy string value</param>
		/// <param name="conditionalFlagKey">Conditional flag key</param>
		public static void SetAdditionalInfoIfFlagSet(string key, Func<string> value, string conditionalFlagKey)
		{
			if (AdditionalInformation != null)
			{
				if (AdditionalInformation.HasInfoFlag(conditionalFlagKey))
				{
					SetAdditionalInfoCore(key, value);
				}
			}
		}

		/// <summary>
		/// Sets Additional Information as a string for a specific category if conditional flag has been set by CreateInfoFlag. This information is printed when inside a GatherAdditionalInformation() disposable and when ReportOnceWithAdditionalInfo is used.
		/// </summary>
		/// <param name="category">The category use for key aggregation.</param>
		/// <param name="key">The key to use for storage.</param>
		/// <param name="value">String value set</param>
		/// <param name="conditionalFlagKey">Conditional flag key</param>
		public static void SetAdditionalInfoIfFlagSet(string category, string key, string value, string conditionalFlagKey)
		{
			if (AdditionalInformation != null)
			{
				if (AdditionalInformation.HasInfoFlag(conditionalFlagKey))
				{
					SetAdditionalInfoCore(category, key, () => value);
				}
			}
		}

		/// <summary>
		/// Sets Additional Information as a lazy function (Processed by reference on ErrorReport) for a specific category if conditional flag has been set by CreateInfoFlag.. This information is printed when inside a GatherAdditionalInformation() disposable and when ReportOnceWithAdditionalInfo is used.
		/// </summary>
		/// <param name="category">The category use for key aggregation.</param>
		/// <param name="key">The key to use for storage.</param>
		/// <param name="value">Lazy string value</param>
		/// <summary>
		public static void SetAdditionalInfoIfFlagSet(string category, string key, Func<string> value, string conditionalFlagKey)
		{
			if (AdditionalInformation != null)
			{
				if (AdditionalInformation.HasInfoFlag(conditionalFlagKey))
				{
					SetAdditionalInfoCore(category, key, value);
				}
			}
		}

		/// <summary>
		/// Adds a Stack Trace to Additional Information for a specific category if conditional flag has been set by CreateInfoFlag. This information is printed when inside a GatherAdditionalInformation() disposable and when ReportOnceWithAdditionalInfo is used.
		/// </summary>
		/// <param name="category">The category use for key aggregation.</param>
		/// <param name="key">The key to use for storage.</param>
		/// <param name="conditionalFlagKey">Conditional flag key</param>
		public static void AddStackTraceToAdditionalInfoIfFlagSet(string category, string key, string conditionalFlagKey)
		{
			if (AdditionalInformation != null)
			{
				if (AdditionalInformation.HasInfoFlag(conditionalFlagKey))
				{
					var stackTrace = new StackTrace().ToString();
					SetAdditionalInfoCore(category, key, () => stackTrace);
				}
			}
		}

		static void SetAdditionalInfoCore(string key, Func<string> value)
		{
			SetAdditionalInfoCore(string.Empty, key, value);
		}

		static void SetAdditionalInfoCore(string category, string key, Func<string> value)
		{
			if (AdditionalInformation != null)
			{
				AdditionalInformation.SetAdditionalInfo(category, key, value);
			}
		}

		/// <summary>
		/// Creates a context for SetAdditionalInfo. This information is printed when ReportOnceWithAdditionalInfo is used.
		/// </summary>
		public static IDisposable GatherAdditionalInformation()
		{
			if (AdditionalInformation == null)
			{
				additionalInformationStore.Value = new AdditionalInformationStore();

				return new DisposableAction(() =>
				{
					additionalInformationStore.Value = null;
				});
			}

			return null;
		}

		/// <summary>
		/// Creates an Info Flag within the current thread. To be used along side GatheringInformation for conditional information.
		/// </summary>
		public static void CreateInfoFlag(string flagKey)
		{
			if (AdditionalInformation != null)
			{
				AdditionalInformation.CreateInfoFlag(flagKey);
			}
		}

		/// <summary>
		/// Removes an InfoFlag by key within current thread.
		/// </summary>
		public static void RemoveInfoFlag(string flagKey)
		{
			if (AdditionalInformation != null)
			{
				AdditionalInformation.RemoveInfoFlag(flagKey);
			}
		}

		[ThreadSafe]
		static readonly ThreadLocal<AdditionalInformationStore> additionalInformationStore = new ThreadLocal<AdditionalInformationStore>();

		static AdditionalInformationStore AdditionalInformation => additionalInformationStore.Value;

		static string GetAdditionalInformation(string[] additionalInfoFilterKeys)
		{
			if (AdditionalInformation != null)
			{
				var context = AdditionalInformation.GetContextReport(additionalInfoFilterKeys);
				if (!string.IsNullOrEmpty(context))
				{
					return $"\r\n\r\n-- Additional Information --\r\n\r\n{context}";
				}
			}

			return string.Empty;
		}

		class AdditionalInformationStore
		{
			readonly Dictionary<string, Dictionary<string, Func<string>>> KeyValueStore = new Dictionary<string, Dictionary<string, Func<string>>>();

			readonly HashSet<string> FlagStore = new HashSet<string>();

			public void CreateInfoFlag(string key)
			{
				FlagStore.Add(key);
			}

			public void RemoveInfoFlag(string key)
			{
				FlagStore.Remove(key);
			}

			public bool HasInfoFlag(string key)
			{
				return FlagStore.Contains(key);
			}

			public void SetAdditionalInfo(string category, string key, Func<string> value)
			{
				if (!KeyValueStore.TryGetValue(category, out var store))
				{
					store = new Dictionary<string, Func<string>>();
					KeyValueStore[category] = store;
				}
				store[key] = value;
			}

			public string GetContextReport(string[] additionalInfoFilterKeys)
			{
				var context = new StringBuilder();
				foreach (var keyPair in KeyValueStore)
				{
					if (additionalInfoFilterKeys.Length == 0 || additionalInfoFilterKeys.Contains(keyPair.Key))
					{
						var category = !string.IsNullOrEmpty(keyPair.Key) ? keyPair.Key : "NONE";
						context.Append($"CATEGORY: {category}\r\n");
						foreach (var innerKeyPair in keyPair.Value)
						{
							var value = innerKeyPair.Value.Invoke();
							context.Append($"{innerKeyPair.Key}:\r\n{value}\r\n");
						}
						context.Append("\r\n");
					}
				}
				return context.ToString().TrimEnd();
			}
		}

		/// <summary>
		/// Checks if a report for the key has already been sent. Meaning costly samplings for the report unnecessary anymore.
		/// </summary>
		/// <param name="key">The key to use for matching.</param>
		public static bool HasBeenReported(string key)
		{
			Argument.NotNull(key, nameof(key));
			return reportedKeys.Contains(key);
		}

		[Conditional("DEBUG")]
		static void ThrowExceptionForDebugger()
		{
			try
			{
				throw new InvalidOperationException(nameof(ThrowExceptionForDebugger));
			}
			catch (InvalidOperationException)
			{
			}
		}

		#region Implementation

		static readonly ConcurrentHashSet<string> reportedKeys = new ConcurrentHashSet<string>();

		static readonly DefaultErrorReporter DefaultReporter = new DefaultErrorReporter();

		public static IErrorReporter InstanceInternal
		{
			get
			{
				return Instance ?? DefaultReporter;
			}
		}

		#endregion

		#region DEBUG
#if DEBUG

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "For testing only")]
		public static string LastKeyReported { get; private set; }

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "For testing only")]
		public static string LastMessageReported { get; internal set; }

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "For testing only")]
		public static Exception LastExceptionReported { get; private set; }

		static void SetLastReported(string key, string message, Exception ex)
		{
			LastKeyReported = key;
			LastMessageReported = message;
			LastExceptionReported = ex;
		}

		public static IDisposable SetTemporaryInstanceForTest(IErrorReporter tempErrorReporter)
		{
			var prevErrorReporter = Instance;
			Instance = tempErrorReporter;
			return new DisposableAction(() => Instance = prevErrorReporter);
		}

#else
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "exception")]
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "key")]
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "message")]
		static void SetLastReported(string key, string message, Exception exception)
		{
		}

#endif
		#endregion
	}
}
