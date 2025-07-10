using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using CargoWise.Common;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

[assembly: InternalsVisibleTo("Enterprise.ZArchitecture.Web.Tests, PublicKey=" + CommonAssemblyInfo.PublicKey)]

namespace Enterprise.ZArchitecture.Web.Common
{
	public class TopLevelWebExceptionHandler : TopLevelExceptionHandler
	{
		public TopLevelWebExceptionHandler()
		{
			lazySiteInfo = new Lazy<SiteInformation>(() => SiteInformation.Create(databaseName: Db.DatabaseName, serverName: Db.ServerName));
		}

		[ThreadSafe]
		public static readonly Lazy<TopLevelWebExceptionHandler> LazyInstance =
			new Lazy<TopLevelWebExceptionHandler>(() => new TopLevelWebExceptionHandler());

		protected override bool ReportAllUnhandledSqlExceptions => true;

		public override void ShowError(string message)
		{
			Log(message, EventLogEntryType.Error, SiteInfo);
		}
		protected override void ShowError(string message, string caption)
		{
			Log($"Caption: {caption}\n{message}", EventLogEntryType.Error, SiteInfo);
		}
		protected override void ShowWarning(string message)
		{
			Log(message, EventLogEntryType.Warning, SiteInfo);
		}
		protected override void ShowErrorDialogIfNotSilentlyHandled(DbErrorHandler errorHandler, string friendlyMessage, Exception outermostExceptionForErrorReport)
		{
			Log(friendlyMessage, EventLogEntryType.Error, SiteInfo);
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public static void Log(string message, EventLogEntryType entryType, SiteInformation siteInformation)
		{
			if (siteInformation == null)
			{
				Log(message, entryType);
				return;
			}
			using (var eventLog = new EventLog())
			{
				eventLog.Source = WebInfrastructureConstants.EventSourceName;
				try
				{
					var eventInstance = new EventInstance(0, 0, entryType);
					eventLog.WriteEvent(
					eventInstance,
					FormattableString.Invariant($"{message}\n{siteInformation}"),
					siteInformation.PID,
					siteInformation.ApplicationDomain,
					siteInformation.ApplicationDomainVirtualPath,
					siteInformation.ApplicationPhysicalPath,
					siteInformation.RunningVersion,
					siteInformation.ServerName,
					siteInformation.DatabaseName,
					siteInformation.UserName,
					siteInformation.SiteName,
					siteInformation.RunningPath,
					siteInformation.TargetVersionPath);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// ignored
				}
			}
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public static void Log(string message, EventLogEntryType entryType)
		{
			using (var eventLog = new EventLog())
			{
				eventLog.Source = WebInfrastructureConstants.EventSourceName;
				try
				{
					eventLog.SafeWriteEntry(message, entryType);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// ignored
				}
			}
		}

		protected override bool HandleException(Exception exceptionToHandle, Exception outermostExceptionForErrorReport)
		{
			return
				HandleWebApplicationException(exceptionToHandle, outermostExceptionForErrorReport)
				|| base.HandleException(exceptionToHandle, outermostExceptionForErrorReport);
		}

		protected virtual bool HandleWebApplicationException(Exception exceptionToHandle, Exception outermostExceptionForErrorReport)
		{
			switch (exceptionToHandle)
			{
				case DatabaseUpgradedException _:
					return HandleDatabaseUpgradedException();
				case DatabaseUpgradeInProgressException _:
					return HandleDatabaseUpgradeInProgressException();
				default:
					return exceptionToHandle.IsCriticalException() || exceptionToHandle.IsIgnorable();
			}
		}

		bool HandleDatabaseUpgradedException()
		{
			OnDatabaseUpgradedException();
			WebUpgradeManager.NotifyUpgradeRequired();
			return true;
		}

		bool HandleDatabaseUpgradeInProgressException()
		{
			OnDatabaseUpgradeInProgressException();
			return true;
		}

		protected virtual void OnDatabaseUpgradedException()
		{
			WriteResponseUpgradeInProgress((NoResString)"Web Application is being upgraded");
		}

		protected virtual void OnDatabaseUpgradeInProgressException()
		{
			WriteResponseUpgradeInProgress((NoResString)"Database is upgrading");
		}

		static void WriteResponseUpgradeInProgress(string upgradeHeader)
		{
			var upgradeInProgressMessage =
				(NoResString)"The Web Application you attempted to access is currently being upgraded. Please try again shortly.";
			WriteResponseMessage(upgradeInProgressMessage, HttpStatusCode.ServiceUnavailable, upgradeHeader);
		}
		static void WriteResponseInternalServerError(Exception ex)
		{
			var responseMessage =
				HttpContext.Current?.IsDebuggingEnabled ?? false
					? $"{ex}"
					: $"Internal server error: {HttpContext.Current?.Request.Path}";

			WriteResponseMessage(responseMessage, HttpStatusCode.InternalServerError);
			ReportInternalServerError(ex, HttpContext.Current?.Request.Path);
		}

		internal static void ReportInternalServerError(Exception ex, string requestPath)
		{
			var inner = ex.GetBaseException();
			var methodCausingException = inner.TargetSite?.Name;
			var simplifedAsciiMessage = new string(inner.Message.Where(c => c < 128 && (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))).ToArray());
			var firstLetterOfWords = simplifedAsciiMessage.Split(' ', '\t', '\n', '\r')
				.Where(x => !string.IsNullOrWhiteSpace(x) && char.IsLetter(x[0]))
				.Select(x => x[0]);
			var keyPartFromMessage = new string(firstLetterOfWords.Take(4).ToArray());
			var key = "TopLevelWeb_" + inner.GetType().Name;

			if (!string.IsNullOrEmpty(methodCausingException))
			{
				key += "_" + methodCausingException;
			}

			if (keyPartFromMessage.Length > 0)
			{
				key += "_" + keyPartFromMessage;
			}

			if (Db.IsDatabaseUpgraded)
			{
				key += "_AfterDbUpgrade";
			}

			using (Db.IsDatabaseUpgraded ? Db.DisableSchemaVersionCheckOnCurrentThread() : null)
			{
				ErrorReporter.ReportOnce(key, "Internal server error: " + requestPath, ex);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Http response content type requires ANSI string")]
		static void WriteResponseMessage(string responseMessage, HttpStatusCode statusCode, string upgradeHeader = null)
		{
			if (HttpContext.Current == null)
			{
				return;
			}

			try
			{
				HttpContext.Current.Server.ClearError();
				var response = HttpContext.Current.Response;

				response.StatusCode = (int)statusCode;
				response.StatusDescription = $"{statusCode}";
				response.ContentType = "text/plain;charset=utf-8";

				// Can't test it at the moment because Headers can't be accessed
				if (upgradeHeader != null)
				{
					response.AddHeader("wtg-upgrade", upgradeHeader);
				}
				response.Write(responseMessage);
				response.End();

				if (HttpContext.Current.ApplicationInstance != null)
				{
					HttpContext.Current.ApplicationInstance.CompleteRequest();
				}

				if (HttpContext.Current.Session != null)
				{
					HttpContext.Current.Session.Abandon();
				}
			}
			catch (HttpException)
			{
				// ignore Response not available
			}
		}

		public static bool HandleUnhandledException(Exception ex)
		{
			try
			{
				if (LazyInstance.Value.HandleException(ex, ex))
				{
					return true;
				}
			}
			catch (Exception exception)
			{
				WriteResponseInternalServerError(ex);

				try
				{
					WebUpgradeManager.Log($"{exception}", EventLogEntryType.Warning);
				}
				catch (Exception reportOnceException) when (!reportOnceException.IsCriticalException())
				{
					// ignored
				}

				return exception.IsIgnorable();
			}

			WriteResponseInternalServerError(ex);
			return false;
		}

		internal SiteInformation SiteInfo => lazySiteInfo?.Value;
		readonly Lazy<SiteInformation> lazySiteInfo;
	}
}
