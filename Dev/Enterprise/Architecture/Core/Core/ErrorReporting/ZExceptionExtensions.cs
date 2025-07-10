using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	#region SuppressResourceStringsCheckRegion

	public static class ZExceptionExtensions
	{
		public static bool IsDbUpgradeOrIsInfrastructureDbError(this Exception ex) =>
			ex is DatabaseUpgradeException
			|| ex is System.Data.Common.DbException sqlEx && new DbErrorHandler(sqlEx, Db.Connection).IsInfrastructureDbError;

		public static bool IsCausedByNotBeingAbleToOpenForms(Exception ex) => ex is Win32Exception win32
			&& (win32.NativeErrorCode == 1406 || win32.NativeErrorCode == 1158 || win32.NativeErrorCode == 8 ||
				(win32.ErrorCode == -2147467259 && win32.NativeErrorCode == 0)); // Error creating window handle.

		public static bool IsInfrastructureDbError(System.Data.Common.DbException exception)
			=> (new DbErrorHandler(exception, Db.Connection)).IsInfrastructureDbError;

		public static bool IsSqlExceptionReportable(System.Data.Common.DbException exception)
		{
			var errorHandler = new DbErrorHandler(exception, Db.Connection);
			var friendlyMessage = errorHandler.GetDBErrorUserFriendlyMessage();
			return string.IsNullOrWhiteSpace(friendlyMessage);
		}

		public static bool HasInnerExceptionOfType<T>(this Exception ex, out T exception)
			where T : Exception
		{
			var innerException = ex;
			do
			{
				if (innerException is T)
				{
					exception = innerException as T;
					return true;
				}
				innerException = innerException.InnerException;
			}
			while (innerException != null);

			exception = null;
			return false;
		}

		public static string SecurityErrorMessage
			=> Constants.ProductName + " is unable to continue because of a .NET Framework security restriction on this computer.\r\nPlease contact your system administrator or IT support staff.\r\n" + Constants.ProductName + " and all dependent assemblies require FullTrust permissions.";

		public static string AdditionalContextDetails
			=> ExceptionReporter.Instance?.AdditionalContextDetails;

		internal static bool IsCorruptedInstallationException(Exception exception)
		{
			foreach (var ex in ExceptionExtensions.FlattenInnerExceptions(exception))
			{
				switch (ex)
				{
					case FileNotFoundException fileNotFoundException when IsCorruptedInstallationFileName(fileNotFoundException.FileName):
					case DirectoryNotFoundException directoryNotFoundException when directoryNotFoundException.Message.Contains(AssemblyLoader.GetBinPath(), StringComparison.OrdinalIgnoreCase):
					case FileLoadException fileLoadException when IsCorruptedInstallationFileName(fileLoadException.FileName):
					case BadImageFormatException badImageFormatException when IsCorruptedInstallationFileName(badImageFormatException.FileName):
					case TypeLoadException _:
						return true;
				}
			}
			return false;
		}

		static bool IsCorruptedInstallationFileName(string fileName)
		{
			return string.IsNullOrEmpty(fileName)
					|| Path.GetFileName(fileName) == fileName
					|| fileName.StartsWith(AssemblyLoader.GetBinPath(), StringComparison.OrdinalIgnoreCase);
		}

		public static string UnattendedErrorMessage
		{
			get
			{
				var product = Globals.IsWeb ? "Web Service" : "Batch Processor";

				return FormattableString.Invariant($"{product} has encountered an unexpected error. Additional context information: {AdditionalContextDetails}. This may be due to a network or hardware problem. Please check that the batch processor is running correctly.");
			}
		}

		public static string GetFullMessage(this Exception exception)
		{
			IEnumerable<string> messages = exception
				.GetAllExceptions()
				.Where(e => !string.IsNullOrWhiteSpace(e.Message))
				.Select(e => e.Message);
			string result = string.Join(" --> ", messages);
			return result;
		}

		public static IEnumerable<Exception> GetAllExceptions(this Exception exception)
		{
			yield return exception;

			if (exception is AggregateException aggrEx)
			{
				foreach (Exception innerEx in aggrEx.InnerExceptions.SelectMany(e => e.GetAllExceptions()))
				{
					yield return innerEx;
				}
			}
			else if (exception.InnerException != null)
			{
				foreach (Exception innerEx in exception.InnerException.GetAllExceptions())
				{
					yield return innerEx;
				}
			}
		}

		public static bool NotifyUserWithoutErrorReport(this Exception ex)
			=> ex is ZSaveConcurrencyException concurrencyException && concurrencyException.NotifyUserWithoutErrorReport;
	}

	#endregion
}
