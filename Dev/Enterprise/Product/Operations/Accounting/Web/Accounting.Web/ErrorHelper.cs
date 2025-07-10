using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CargoWise.Common;
using Enterprise.Accounting.Web.Exceptions;

namespace Enterprise.Accounting.Web;

public static class ErrorHelper
{
	public static string ReportError(Exception ex, [CallerMemberName] string callerName = "")
	{
		try
		{
			var webEx = new AccountingWebReportableException(ex.Message, ex) { ShouldReportAlwaysInReportOnce = false };

			if (!(ex is SqlException && ex.Message.Equals("ConcurrencyError")))
			{
				var errorReporterKey = $"Accounting.Web_{GetErrorPath()}_{ex.GetType().Name}";
				ErrorReporter.ReportOnce(errorReporterKey, webEx.Message, webEx);
			}

			var errorMessage = $"An unexpected error occurred. Please try again later.{(string.IsNullOrEmpty(webEx.ErrorReportID) ? "" : $" Error Report Id: {webEx.ErrorReportID}.")}";

			return errorMessage;
		}
		catch (Exception errorReportException)
		{
			return $"An unexpected error occurred in error reporter. Please try again later. {errorReportException.Message}";
		}

		string GetErrorPath()
		{
			try
			{
				var frame = new StackTrace().GetFrame(2);

				if (frame == null)
				{
					return callerName;
				}

				var method = frame.GetMethod();
				var callerClassName = method.DeclaringType.Name;
				var callerMethodName = method.Name;

				return $"{callerClassName}.{callerMethodName}";
			}
			catch
			{
				return callerName;
			}
		}
	}
}
