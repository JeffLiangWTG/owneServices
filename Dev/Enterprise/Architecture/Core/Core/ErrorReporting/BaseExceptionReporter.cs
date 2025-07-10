using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.ZArchitecture.Core.ZExceptionExtensions;
using Constants = Enterprise.Core.Constants;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

#if DEBUG
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
#endif

namespace Enterprise.ZArchitecture.Core
{
	#region SuppressResourceStringsCheckRegion

	public class BaseExceptionReporter : IErrorReporter
	{
		public void Enable()
		{
			if (!ExceptionReporter.IsEnabled || ExceptionReporter.Instance != this)
			{
				ExceptionReporter.SetInstance(this);
			}
		}

		internal void UnHookUnhandledExceptions()
		{
			AppDomain.CurrentDomain.UnhandledException -= new UnhandledExceptionEventHandler(HandleUnhandledException);
			TaskScheduler.UnobservedTaskException -= HandleUnhandledException;
		}

		internal void HookStaticUnhandledExceptions()
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(HandleUnhandledException);
			TaskScheduler.UnobservedTaskException += HandleUnhandledException;
		}

		internal readonly TopLevelExceptionHandler exceptionHandler = new TopLevelExceptionHandler();

		internal bool SuppressGuiFlag
		{
			set;
			private get;
		}

		/// <summary>
		/// This method works differently depending on the following.
		/// Unit Tests: Remembers the exception so that it can be detected later.
		/// Debug Mode: Shows exception report form.
		/// Release Mode: Uploads information about an exception to WiseTech Global and
		/// displays a simple "Error Has Occurred" Message with an Exception Report ID. Also
		/// stores a copy in Odyssey that is viewable through Enterprise.
		/// so clients can contact support.
		/// Doesn't necessarily send report to WTG if message is sent to user
		/// </summary>
		public void ReportException(string key, Exception ex)
		{
#if DEBUG  // to help debugging - will stop here when break on exception is on.
			try
			{
				throw new Exception();
			}
			catch
			{
			}
#endif
			if (!exceptionHandler.HandleSpecificExceptions(ex, out var exceptionDuringHandle))
			{
				if (exceptionDuringHandle != null)
				{
					var newException = new Exception(string.Format("Exception occured during HandleException (See Inner Stack Trace for original) : {0} ", exceptionDuringHandle.ToString()), ex);
					DoReportException(newException, key, "");
				}
				else
				{
					DoReportException(ex, key, "");
				}
			}
		}

		public bool HandleSpecificExceptions(Exception ex)
		{
			return exceptionHandler.HandleSpecificExceptions(ex, out var exceptionDuringHandle);
		}

		public void HandleUnhandledException(Exception ex)
		{
			HandleOrReport(ex);
		}

		public bool TryHandleWithoutReporting(Exception ex)
		{
			var wasHandled = true;
			exceptionHandler.HandleUnhandledException(ex, (s, e) => wasHandled = false);
			return wasHandled;
		}

#if DEBUG
		public bool SaveReportOutsideTransactionDuringTest { get; set; }
#endif

		protected string SendErrorReport(string report)
		{
			using (Db.DisposableActionForDbConnection())
			{
				string initialStatus = StmErrorReportTransmitStatus.Codes.Queued;

				var useNewDbConnection =
#if DEBUG
 (!Globals.IsTest || SaveReportOutsideTransactionDuringTest) &&
#endif
 (Db.Connection.AppTransactionCount > 0 || Db.Connection.UndisposedLocksPresentAfterReconnect) && Db.Connection != Db.AdminConnection;

				QueueReport(report, initialStatus, useNewDbConnection);

				return initialStatus;
			}
		}

		protected void SendReport(IErrorReporter reporter, ExceptionReportArgs reportArgs)
		{
			if (Globals.IsDBUpgSkipped)
			{
				return;
			}

			reportArgs.SessionId = sessionId;
			reportArgs.Sequence = totalReportCount++;

			var reportBuilder = GetNewExceptionReportBuilder(reportArgs);
			var report = reportBuilder.GenerateReport();

			var initialStatus = SendErrorReport(report);

			var isSilentException = reporter == null || reportArgs.IsSlient;

			if (reportArgs.Ex is IHasErrorReportID devEx)
			{
				devEx.ErrorReportID = reportBuilder.ErrorReportID;
			}

			if (!isSilentException && !dontShowMessageSentInfo)
			{
				string pastTenseActionDescription;

				switch (initialStatus)
				{
					case StmErrorReportTransmitStatus.Codes.Sent:
						pastTenseActionDescription = "sent";
						break;

					case StmErrorReportTransmitStatus.Codes.Queued:
						pastTenseActionDescription = "queued to send";
						break;

					case StmErrorReportTransmitStatus.Codes.Failed:
						pastTenseActionDescription = "failed to send";
						break;

					default:
#if DEBUG
						throw new NotImplementedException(); // If you add a new StmErrorReportTransmitStatus, add a new case
#else
						pastTenseActionDescription = "sent"; // Just go along with it in release
						break;
#endif
				}

				string message = string.Format("The details have been {0} to CargoWise", pastTenseActionDescription);

				if (reportBuilder.IsValidErrorReportId)
				{
					message += " with Error Report ID " + reportBuilder.ErrorReportID + ".";
				}

				message += "\r\n" + ContactNominatedCallerMessage;

				Globals.Message.ShowInformation(message);
			}
		}

		public static string ContactNominatedCallerMessage = "If this error is preventing you from performing critical business functions, please contact your internal nominated caller for " + Constants.ProductName + " Support issues and then raise a support incident with CargoWise.";

		public void SendAllUnsentDeveloperExceptions()
		{
			for (int i = reportsThatCouldNotBeSent.Count - 1; i >= 0; i--)
			{
				try
				{
					var args = (ExceptionReportArgs)reportsThatCouldNotBeSent[i];
					args.ErrorReportID ??= GenerateErrorReportID();
					SendReport(null, args);
					reportsThatCouldNotBeSent.RemoveAt(i);
				}
				catch (Exception)
				{
					//Ignore this exception.  We have already thrown an exception once whilst trying to send this report
				}
			}
		}

		public bool SuppressExceptionsThatOccurWhilstReportingDeveloperExceptions;

		/// <summary>
		/// This method works similarly to ReportException, except in Release Mode the form is not shown.
		/// </summary>
		public void ReportDeveloperException(string key, string message, Exception ex)
		{
			if (ShouldShowDeveloperError)
			{
				DoReportException(ex, key, message);
			}
			else
			{
				ReportSilently(key, message, ex);
			}
		}

		/// <summary>
		/// This method works similarly to ReportException, except in Release Mode the form is not shown.
		/// </summary>
		public void ReportDeveloperException(string message, Exception ex)
		{
			ReportDeveloperException("", message, ex);
		}

		/// <summary>
		/// This method works similarly to ReportException, except in Release Mode the form is not shown.
		/// Exceptions that can be handled silently will not be sent as an error report.
		/// </summary>
		public void ReportDeveloperExceptionOrHandleSilently(string message, Exception ex)
		{
			ReportDeveloperExceptionOrHandleSilently("", message, ex);
		}

		/// <summary>
		/// This method works similarly to ReportException, except in Release Mode the form is not shown.
		/// Exceptions that can be handled silently will not be sent as an error report.
		/// </summary>
		public void ReportDeveloperExceptionOrHandleSilently(string key, string message, Exception ex)
		{
			if (!exceptionHandler.HandleAnySqlExceptionsSilently(ex))
			{
				ReportDeveloperException(key, message, ex);
			}
		}

		internal void ReportSilently(string key, string message, Exception ex)
		{
			using (ObjectFactory.Get<IUserEventTracker>().TemporarilyDisable())
			{
				SendAllUnsentDeveloperExceptions();
				string errorReportID = null;
				ExceptionReportArgs args = null;

				try
				{
					errorReportID = GenerateErrorReportID();
					args = CreateExceptionReportSilentlyArgs(ex, errorReportID, key, message);
					SendReport(null, args);
				}
				catch (Exception exception)
				{
					args ??= CreateExceptionReportSilentlyArgs(ex, errorReportID, key, message);
					reportsThatCouldNotBeSent.Add(args);

					// Avoid throwing exceptions during database upgrade, as attempts to establish new connections when generating errorReportID may fail.
					if (!SuppressExceptionsThatOccurWhilstReportingDeveloperExceptions && exception is not DatabaseUpgradeInProgressException)
					{
						throw;
					}
				}

#if DEBUG
				ExceptionReportedSilentlyForTest = ex;
#endif
			}
		}
#if DEBUG
		internal Exception ExceptionReportedSilentlyForTest { get; set; }
#endif

		ExceptionReportArgs CreateExceptionReportSilentlyArgs(Exception ex, string reportID, string key, string message)
		{
			return new ExceptionReportArgs(ex, reportID, key, message)
			{
				SubjectPrefix = AutoGeneratedSubjectPrefix
			};
		}

		public virtual string AutoGeneratedSubjectPrefix
		{
			get { return "AUTO-GENERATED (Silent)"; }
		}

		protected virtual ExceptionReportBuilder GetNewExceptionReportBuilder(ExceptionReportArgs reportArgs)
		{
			return new ExceptionReportBuilder(reportArgs);
		}

		internal readonly ArrayList reportsThatCouldNotBeSent = new ArrayList();

		#region IReportsException Members

		void IErrorReporter.Clear()
		{
#if DEBUG
			ExceptionReporterTestListener.Instance.Clear();
#endif
			((UserNotificationBase)Globals.Message).ClearShownErrorKeys();
		}

		public virtual void Report(string key, string message, Exception exception)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (exception == null)
				{
					Globals.Message.ShowDeveloperErrorAlways(key ?? "", message, "");
				}
				else if (!exceptionHandler.HandleSpecificExceptions(exception))
				{
					Globals.Message.ShowDeveloperException(key ?? "", message, exception);
				}
			}
		}

		#endregion

		#region Implementation

		internal DateTime timeOfLastException;
		internal int sequentialErrorCount;
		internal readonly Guid sessionId;

		public int TotalReportCount
		{
			get { return ExceptionReporter.Instance.totalReportCount; }
#if DEBUG
			set { totalReportCount = value; }
#endif
		}
		internal int totalReportCount;

		public BaseExceptionReporter()
			: this(new TopLevelExceptionHandler()) { }

		public BaseExceptionReporter(TopLevelExceptionHandler handler)
		{
			exceptionHandler = Argument.NotNull(handler, nameof(handler));
			timeOfLastException = DateTime.MinValue;
			sequentialErrorCount = 1;
			sessionId = Guid.NewGuid();
			totalReportCount = 0;
		}

		public BaseExceptionReporter(Guid? sessionId)
			: this(new TopLevelExceptionHandler())
		{
			this.sessionId = (Guid)sessionId;
		}

		internal void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			HandleOrReport(e.ExceptionObject as Exception);
		}

		internal void HandleUnhandledException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			e.SetObserved();
			if (!IgnoreUnobservedTaskException(e) && !e.Exception.IsCriticalException())
			{
#if DEBUG
				if (Globals.IsTest)
				{
					ErrorReporter.ReportOnce("UnobservedTaskException occurred.", e.Exception);
					return;
				}
#endif
				ReportSilently(null, null, e.Exception);
			}
		}

		bool IgnoreUnobservedTaskException(UnobservedTaskExceptionEventArgs e)
		{
			if (e.Exception.GetInnermostException() is TaskCanceledException)
			{
				return true;
			}

			if (Globals.IsWinzor)
			{
				var innerException = e.Exception.InnerException;
				if (innerException.GetType().FullName == "Microsoft.JSInterop.JSDisconnectedException")
				{
					return true;
				}

				// The following methods are being called on a circuit that has already been disposed, causing NullReferenceException.
				// The issue is reported here: https://github.com/dotnet/aspnetcore/issues/45980#issuecomment-2295532614
				if (innerException is NullReferenceException
					&& innerException.TargetSite.DeclaringType.FullName == "Microsoft.AspNetCore.Components.Server.Circuits.RemoteJSRuntime"
					&& (innerException.TargetSite.Name == "EndInvokeDotNet"
						|| innerException.TargetSite.Name == "SendByteArray"
						|| innerException.TargetSite.Name == "TransmitStreamAsync"))
				{
					return true;
				}
				if (innerException is NullReferenceException && innerException.TargetSite.DeclaringType.FullName == "Microsoft.AspNetCore.SignalR.ClientProxyExtensions" && innerException.TargetSite.Name == "SendAsync")
				{
					return true;
				}

				if (innerException is NullReferenceException && innerException.TargetSite.DeclaringType.FullName == "Microsoft.JSInterop.Infrastructure.DotNetDispatcher" && innerException.TargetSite.Name == "EndInvokeDotNetAfterTask")
				{
					// This issue will be handled in GlobalExceptionHandler in Winzor.
					return true;
				}
				if (innerException is NullReferenceException && innerException.TargetSite.DeclaringType.FullName == "System.Threading.Tasks.ContinuationTaskFromTask" && innerException.TargetSite.Name == "InnerInvoke")
				{
					// This issue will be handled in GlobalExceptionHandler in Winzor, it happens when app server is shutting down.
					return true;
				}
			}
			return false;
		}

		public void HandleOrReport(Exception ex)
			=> exceptionHandler.HandleUnhandledException(ex, ReportException);

		public bool ShouldShowDeveloperError
			=> (Globals.IsTest && !Globals.GetIsUnitTestingProductionFunctionality()) ||
				(EnvProxy.Instance.CurrentUser?.IsDeveloperLogin ?? false);

		void QueueReport(string report, string initialStatus, bool useNewDbConnection)
		{
			GetExceptionQueuer().QueueExceptionReportForLaterSending(report, initialStatus, useNewDbConnection);
		}

		internal virtual ExceptionQueuer GetExceptionQueuer()
		{
			return new ExceptionQueuer();
		}

		#region Repetitive Exception Handling

		internal bool IsExceptionRepeated
		{
			get
			{
				var timeSinceLastException = LocalNow - timeOfLastException;
				return timeSinceLastException.TotalSeconds < 5.0D;
			}
		}

		internal void UpdatePreviousException(bool repeatedException)
		{
			timeOfLastException = LocalNow;

			if (!repeatedException)
			{
				sequentialErrorCount = 1;
			}
			else
			{
				Interlocked.Increment(ref sequentialErrorCount);
			}
		}

		internal bool IsErrorCountFatal(int errorCount)
		{
			return errorCount >= 5;
		}

		DateTime LocalNow
		{
			get { return DateTime.Now; } // This is used for relative comparisons in short periods of time. No point risking a DB hit when potentially in a DB Error.
		}

		#endregion

		#region Send Report

		public bool IsReportingException
		{
			get { return exceptionBeingReported != null; }
		}

#if DEBUG
		public Overridable<bool> TestingDoReportException { get; } = new Overridable<bool>(false);
#endif

		protected virtual void DoReportException(Exception ex, string key, string message)
		{
			if (ex != null)
			{
				if (IsReportingException)
				{
					WriteToEventLog(ex, exceptionBeingReported);
				}
				else
				{
					using (ObjectFactory.Get<IUserEventTracker>().TemporarilyDisable())
					{
						exceptionBeingReported = ex;

						try
						{
							ReportExceptionCore(ex, key, message);
						}
						finally
						{
							exceptionBeingReported = null;
						}
					}
				}
			}
#if DEBUG
			else if (Globals.IsTest && !TestingDoReportException.Value)
			{
				ExceptionReporterTestListener.Instance.Add(CreateDummyException(message), key, message);
			}
#endif
		}

		static Exception CreateDummyException(string message)
		{
			try
			{
				throw new DeveloperNotificationException(message);
			}
			catch (Exception e)
			{
				return e;
			}
		}

		// Presuming only one BaseExceptionReporter exists and never change
		[ThreadStatic]
		static Exception exceptionBeingReported;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Test code")]
		void ReportExceptionCore(Exception ex, string key, string message)
		{
#if DEBUG
			if (Globals.IsTest && !TestingDoReportException.Value)
			{
				ExceptionReporterTestListener.Instance.Add(ex, key, message);
			}
			else if (TestingState.IsRunningOnDAT && !TestingDoReportException.Value && !Globals.IsWinzor)
			{
				Console.Error.WriteLine("Error occured during Dat outside unit tests, " + Constants.ProductName + " will exit.");
				Console.Error.WriteLine(ex.ToString());
				Console.Error.Flush();
				System.Environment.Exit(ExitCodes.TestExitCode);
			}
			else
#endif
			if (!IsExceptionRepeated && Globals.IsUserInteractive) // Here be Enterprise
			{
				if (SuppressGuiFlag || IsCausedByNotBeingAbleToOpenForms(ex) || !Globals.CanShowDialogs)
				{
					SendErrorReportHandler(new ExceptionReportArgs(ex, GenerateErrorReportID(), key, "( No report form showed )" + message));
				}
				else
				{
					try
					{
						ShowReportForm(ex, GenerateErrorReportID(), key, message, true);
					}
					catch (Exception ex2) when (!ex2.IsCriticalException())
					{
						SendErrorReportHandler(new ExceptionReportArgs(ex, GenerateErrorReportID(), key, "(An exception occurred while showing the form: " + ex2.ToString() + "\r\n No report form showed) " + message));
						Globals.Message.ShowError(Res.GetString("AECB2050-20B3-48A1-B34E-CA65BB99601E", "The application has encountered a problem and may need to close. Sorry for the inconvenience.")
							+ "\r\n" + Res.GetString("3F602BE4-4F0F-48B7-8D5C-D0315E134E22", "If you were in the middle of something, the information you were working on might be lost."));
					}
				}
			}
			else if (!IsExceptionRepeated && !Globals.IsUserInteractive) // Here be Batch Processor / Web Services
			{
				if (ex is System.Data.Common.DbException sqlEx && IsInfrastructureDbError(sqlEx))
				{
					exceptionHandler.ShowError(UnattendedErrorMessage + "\r\n\r\n" + new ExceptionDetails(ex).GetStackTraceAndMessage());
				}
				else
				{
					ReportSilently(key, string.Format(CultureInfo.InvariantCulture, "AdditionalContextDetails: {0} Exception Message: {1}", AdditionalContextDetails ?? "N/A", ex.Message), ex);
				}
				AdditionalContextDetails = null;
			}
			else
			{
				UpdatePreviousException(true);

				if (IsErrorCountFatal(sequentialErrorCount))
				{
					exceptionHandler.ShutdownEnterprise(Constants.ProductName + " has encountered a serious problem and must close.");
				}
			}
		}

		#region GenerateErrorReportID

		public const string InvalidReportId = "ReportIDFailed";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected internal string GenerateErrorReportID()
		{
			var errorReportId = InvalidReportId;
			var count = 3;
			while (count > 0)
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						var productRegistrationKey = ObjectFactory.Get<IProductRegistration>().Key;
						errorReportId = string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}",
							GetNextErrorID(),
							productRegistrationKey.EnterpriseCode,
							productRegistrationKey.ServerCode);
					}
					break;
				}
				catch (DatabaseUpgradeException)
				{
					throw;
				}
				catch
				{
					count--;
				}
			}

			return errorReportId;
		}

		string GetNextErrorID()
		{
			string result;

			if (!Db.Connection.IsInTransaction)
			{
				result = GetNextErrorID(Db.Connection);
			}
			else
			{
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					result = GetNextErrorID(connection);
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		static string GetNextErrorID(DbConnection connection)
		{
			string result = InvalidReportId;

			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				result = EnvProxy.Instance.NumberFountains.ErrorReporting.GetNextFormatted(connection);
				transactionManager.CommitTransaction();
			}

			return result;
		}

		#endregion

		public string AdditionalContextDetails { get; set; } //for use with Batch Processor - report what service task failed if stack trace is ambiguous

		static void WriteToEventLog(Exception exceptionBeingReportedRecursively, Exception originalException)
		{
			var message = string.Format("Recursive error reporting was attempted here (the original exception being reported is below):\r\n{0}\r\n\r\nORIGINAL EXCEPTION:\r\n{1}",
				/*0*/ exceptionBeingReportedRecursively.ToString(),
				/*1*/ originalException.ToString());

			SafeEventLogExtensions.SafeWriteEntryToApplicationLog(message, EventLogEntryType.Warning, ignoreAllExceptions: true);
		}

		#endregion

		protected virtual void ShowReportForm(Exception ex, string errorReportId, string key, string message, bool isFullMode)
		{
			dontShowMessageSentInfo = !ObjectFactory.Get<IExceptionReportingFormManager>().ShowReportForm(ex, errorReportId, key, message, isFullMode, SendErrorReportHandler);
		}
		bool dontShowMessageSentInfo;

		protected virtual void SendErrorReportHandler(ExceptionReportArgs args)
		{
			SendReport(this, args);
			UpdatePreviousException(false);
		}

		#endregion

		#region Internal Properties

		internal void SendReportInternal(IErrorReporter reporter, ExceptionReportArgs reportArgs) => SendReport(reporter, reportArgs);
		internal void ShowReportFormInternal(Exception ex, string errorReportId, string key, string message, bool isFullMode) => ShowReportForm(ex, errorReportId, key, message, isFullMode);

		#endregion
	}

	#endregion
}
