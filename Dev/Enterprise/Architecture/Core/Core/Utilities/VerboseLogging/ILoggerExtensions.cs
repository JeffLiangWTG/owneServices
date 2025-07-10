using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Core
{
	public static class ILoggerExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging an inability to log can be in English methinks")]
		public static void Log(this ILogger logger, LogType type, Exception exceptionToLog, Func<string> getPotentiallyUnsafeLogMessageString)
		{
			string message = "";
			try
			{
				message = getPotentiallyUnsafeLogMessageString();
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}
				message = string.Format("(Could not get the text to be logged, exception was thrown. {0})", exception.Message);
				ErrorReporter.ReportOnce(message, exception);
			}
			logger.Log(type, message, exceptionToLog);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging an inability to log can be in English methinks")]
		public static void Log(this ILogger logger, LogType type, Func<string> getPotentiallyUnsafeLogMessageString)
		{
			string message = "";
			try
			{
				message = getPotentiallyUnsafeLogMessageString();
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}
				message = string.Format("(Could not get the text to be logged, exception was thrown. {0})", exception.Message);
				ErrorReporter.ReportOnce(message, exception);
			}
			logger.Log(type, message);
		}

		public static void ErrorWithSummary(this ILogger logger, string summaryMessage, IEnumerable<string> errorMessages)
		{
			if (!errorMessages.Any())
			{
				return;
			}

			var sb = new ZStringBuilder(summaryMessage);
			foreach (var errorMessage in errorMessages)
			{
				var errorWithBulletPoint = Res.IsRightToLeft(Res.CurrentLanguage)
					? errorMessage + "  " + BulletPoint + "  "
					: BulletPoint + "  " + errorMessage;

				sb.AppendLine(errorWithBulletPoint);
			}

			logger.Log(LogType.Error, sb.ToStringWithNewLineBetweenAppends(), null);
		}

		public static void WarningWithSummary(this ILogger logger, string summaryMessage, IEnumerable<string> warningMessages)
		{
			if (!warningMessages.Any())
			{
				return;
			}

			var sb = new ZStringBuilder(summaryMessage);
			foreach (var warningMessage in warningMessages)
			{
				var warningWithBulletPoint = Res.IsRightToLeft(Res.CurrentLanguage)
					? warningMessage + "  " + BulletPoint + "  "
					: BulletPoint + "  " + warningMessage;

				sb.AppendLine(warningWithBulletPoint);
			}

			logger.Log(LogType.Warning, sb.ToStringWithNewLineBetweenAppends(), null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Non translateable text, it's the bulletpoint character")]
		const string BulletPoint = "\u2022";
	}
}
