using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Policy;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.DocumentScanning.Integration;
using Enterprise.RemoteDesktopServices;
using Enterprise.Semaphores.Common;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

using static Enterprise.ZArchitecture.Core.ZExceptionExtensions;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Core
{
	#region SuppressResourceStringsCheckRegion

	delegate bool ExceptionHandler(Exception exceptionToHandle, Exception outermostExceptionForErrorReport);

	public class TopLevelExceptionHandler
	{
		public static string NewLine => System.Environment.NewLine;
		public static string SecurityErrorText => Constants.ProductName + " is unable to continue because of a .NET Framework security restriction on this computer.\r\nPlease contact your system administrator or IT support staff.\r\n" + Constants.ProductName + " and all dependent assemblies require FullTrust permissions.";

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is the top level handler, bubbling out of this may cause an infinite loop")]
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Always use invariant format for now")]
		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "Not thrown, only reported")]
		public void HandleUnhandledException(Exception ex, Action<string, Exception> unhandledCallback)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (ex != null && !HandleSpecificExceptions(ex, out var exceptionDuringHandle))
				{
					if (Globals.IsTest && !Globals.GetIsUnitTestingProductionFunctionality())
					{
						ErrorReporter.ReportOnce(ex.Message, ex);
					}
					else
					{
						var exToReport = (exceptionDuringHandle != null) ?
							new Exception(string.Format("Exception occured during HandleException (See Inner Stack Trace for original): {0} ", exceptionDuringHandle.ToString()), ex) :
							ex;

						unhandledCallback("", exToReport);
					}
				}

				if (ex is AggregateException aggregate)
				{
					foreach (var inner in aggregate.InnerExceptions.Skip(1))
					{
						HandleUnhandledException(new AggregateExceptionItem(aggregate, inner), unhandledCallback);
					}
				}
			}
		}

		#region Handle Specific Exceptions

		public bool HandleSpecificExceptions(Exception ex)
		{
			return HandleSpecificExceptions(ex, out var _);
		}

		public bool HandleSpecificExceptions(Exception ex, out Exception exceptionDuringHandle)
		{
			exceptionDuringHandle = null;
			try
			{
				return HandleSpecificExceptions(ex, ex, HandleException);
			}
			catch (DatabaseUpgradeException databaseUpgradeException)
			{
				OnDatabaseUpgradeExceptionWhileHandlingException(databaseUpgradeException);
				// Give up on the original exception and say we handled it. The upgrade is more important.
				return true;
			}
			catch (OutOfMemoryException) { return true; } //Just give up and say we handle it in various cases where the system is resource exhausted, corrupted or unstable.
			catch (FileLoadException) { return true; }
			catch (CannotLoadObjectTypeException) { return true; }
			catch (Exception exception)
			{
				exceptionDuringHandle = exception;
				return false;
			}
		}

		static bool HandleSpecificExceptions(Exception ex, Exception outermostExceptionForErrorReport, ExceptionHandler exceptionHandler)
		{
			try
			{
				var exceptions = ExceptionExtensions.FlattenInnerExceptions(ex).ToList();

				if (exceptions.OfType<UpgradeException>().Any())
				{
					//still filter out if there's an infrastructure error; no point in making issues for these
					if (!exceptions.OfType<SqlException>().Any(x => new DbErrorMatch(x).IsInfrastructureDbError))
					{
						return false;
					}
				}

				foreach (var exception in exceptions)
				{
					if (ex is AggregateException aggregateEx)
					{
						return aggregateEx
							.InnerExceptions
							.All(innerException => HandleSpecificExceptions(innerException, outermostExceptionForErrorReport, exceptionHandler));
					}

					var handled = exceptionHandler(exception, outermostExceptionForErrorReport);
					ErrorReporter.PopulateExceptionsBuffer(exception);

					if (handled)
					{
						return true;
					}
				}

				return false;
			}
			catch (Exception exception)
			{
				SafeEventLogExtensions.SafeWriteEntryToApplicationLog(BuildExceptionServiceLog(ex), EventLogEntryType.Error, ignoreAllExceptions: true);
				SafeEventLogExtensions.SafeWriteEntryToApplicationLog(BuildExceptionServiceLog(exception), EventLogEntryType.Error, ignoreAllExceptions: true);
				throw;
			}
		}

		static string BuildExceptionServiceLog(Exception ex)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Exception: {ex.Message}");
			stringBuilder.AppendLine($"Stack trace: {ex.StackTrace}");
			return stringBuilder.ToString();
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "Can't be made simpler really.")]
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Can't be made simpler really.")]
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is the top level handler, bubbling out of this may cause an infinite loop")]
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		[SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison")]
		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		[SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Baseline")]
		protected virtual bool HandleException(Exception exceptionToHandle, Exception outermostExceptionForErrorReport)
		{
			var handled = false;

			if (exceptionToHandle is IExceptionReporterExtender extender)
			{
				handled = extender.HandleException();
			}
			else if (exceptionToHandle is SecurityException || exceptionToHandle is PolicyException)
			{
				ShowError(SecurityErrorText, Res.GetString("d256349c-38f2-4c7b-a764-3248d69b7d42", "Access Denied"));
				handled = true;
			}
			else if (exceptionToHandle is SystemRegistrationKeyMissingInformationException)
			{
				handled = HandleSystemRegistrationKeyException();
			}
			else if (exceptionToHandle is DatabaseUpgradeException)
			{
				DbEnv.Instance.ConnectionGuiPlugin.HandleDatabaseUpgradeException((DatabaseUpgradeException)exceptionToHandle);
				handled = true;
			}
			else if (exceptionToHandle is SqlLockLostException)
			{
				return HandleSqlLockLostException();
			}
			else if (exceptionToHandle is LoginException)
			{
				ShutdownEnterprise(exceptionToHandle.Message);
				handled = true;
			}
			else if (exceptionToHandle is DBConcurrencyException || exceptionToHandle is IConcurrencyException)
			{
				HandleDbConcurrencyException(exceptionToHandle, outermostExceptionForErrorReport);
				handled = true;
			}
			else if (exceptionToHandle is OutOfMemoryException)
			{
				handled = HandleOutOfMemoryException();
			}
			else if (exceptionToHandle.IsPagingFileTooSmall())
			{
				ShowError("An error has occured because this program is running low on memory. Save all your work and restart this program. If problem persists contact your system administrator to have your page file size increased.");
				handled = true;
			}
			else if (exceptionToHandle is DbException sqlException)
			{
				switch (new DbErrorMatch(sqlException).ExceptionType)
				{
					case DbErrorType.ErrorLoadingUntrustedAssembly:
					case DbErrorType.ModuleBeingExecutedIsNotTrusted:
						handled = HandleSqlLoadingUntrustedAssemblyException();
						break;
					case DbErrorType.OwnerSIDDiffersFromMaster:
						handled = HandleOwnerSIDDiffersFromMaster(exceptionToHandle);
						break;
					case DbErrorType.TempdbIsOutOfSpace:
						handled = true;
						ShowError(exceptionToHandle.Message);
						break;
					case DbErrorType.DatabaseInEmergencyModeOrDamaged:
						handled = true;
						break;
					default:
						handled = HandleSqlException(sqlException, outermostExceptionForErrorReport);
						break;
				}
			}
			else if (exceptionToHandle is CommunicationException)
			{
				handled = HandleCommunicationException((CommunicationException)exceptionToHandle);
			}
			else if (exceptionToHandle is InvalidOperationException && exceptionToHandle.Message.Contains("BufferedGraphicsContext"))
			{
				handled = HandleBufferedGraphicsContextCannotBeDisposed(exceptionToHandle);
			}
			else if (exceptionToHandle is InvalidOperationException && ObjectFactory.Get<IProgramRestarter>().IsAlreadyClosing && exceptionToHandle.Message.Contains("requires an open and available"))
			{
				handled = true;
			}
			else if (exceptionToHandle is InvalidOperationException && exceptionToHandle.Message.Contains("The connection is closed."))
			{
				handled = true;
			}
			else if (exceptionToHandle is ObjectDisposedException && exceptionToHandle.StackTrace != null && exceptionToHandle.StackTrace.Contains("System.Windows.Input.PenThreadWorker.ThreadProc()") && exceptionToHandle.StackTrace.Contains("System.Threading.EventWaitHandle.Set()"))
			{
				handled = true;
			}
			else if (exceptionToHandle is ZBlobReadException || exceptionToHandle is SqlStreamReaderRowNotFoundException)
			{
				ShowWarning("Whilst you were working another user has deleted some information you are attempting to see. Please close this form and retry your action.");
				handled = true;
			}
			else if (exceptionToHandle is ExternalStorageException externalStorageException)
			{
				externalStorageException.ReportExceptionForDeveloper();

				var caption = Res.GetString("D90569B5-C75F-4FE9-9C48-B00A2691C229", "Failed to access eDocs");
				ShowError(externalStorageException.UnableToAccessStorageFriendlyMessage, caption);
				handled = true;
			}
			else if (exceptionToHandle is VirusDetectedException virusDetectedException)
			{
				ShowError(virusDetectedException.VirusDetectedFriendlyMessage);
				handled = true;
			}
			else if (exceptionToHandle is TransactionException)
			{
				ShowWarning(exceptionToHandle.Message);
				handled = true;
			}
			else if (IsUserVisibleException(exceptionToHandle))
			{
				handled = HandleUserVisibleException(exceptionToHandle);
			}
			else if (IsCorruptedInstallationException(exceptionToHandle))
			{
#if DEBUG
				if (TestingState.IsRunningOnDAT && !TestingState.IsRunningTests)
				{
					return false;
				}
#endif
				if (!Globals.IsWinzor)
				{
					handled = HandleCorruptedInstallationException(exceptionToHandle);
				}
			}
			else if (exceptionToHandle is DocumentEngineIntegration.ExcelInterfaceExceptionBase)
			{
				ShowError(exceptionToHandle.Message);
				handled = true;
			}
			else if (exceptionToHandle is IOException ioException)
			{
				if (ioException.IsDiskFull())
				{
					ShowWarning("There is not enough space on the disk or you have exceeded your quota. Please contact your system administrator.");
				}
				else
				{
					ErrorReporter.ReportOnce("TopLevelExceptionHandler_IOException", ioException.Message, ioException);
					HandleFailedFileOperation(ioException);
				}

				handled = true;
			}
			else if (exceptionToHandle is SecurityAccessDeniedException)
			{
				ShowError(((SecurityAccessDeniedException)exceptionToHandle).Message, Res.GetString("d256349c-38f2-4c7b-a764-3248d69b7d42", "Access Denied"));
				handled = true;
			}
			else if (exceptionToHandle is HeartbeatIsNotAliveException)
			{
				ShutdownEnterprise("Server is unavailable. Application will be terminated");
				handled = true;
			}
			else if (exceptionToHandle is OperationCanceledException)
			{
				try
				{
					if (exceptionToHandle.Source?.StartsWith("Enterprise.RemoteDesktopServices") ?? false)
					{
						string clientExceptionMessage = string.Empty;
						if (exceptionToHandle.Message.StartsWith(TerminalService.ClientExceptionMessage, StringComparison.OrdinalIgnoreCase) && exceptionToHandle.InnerException != null)
						{
							clientExceptionMessage = $"\r\nInner Exception Message: {exceptionToHandle.InnerException.Message}";
						}
						ShowError($"Operation failed due to an interruption in the Remote Desktop Services connection. Restart {Constants.ProductName} and try again.{clientExceptionMessage}");
					}
					else
					{
						ShowError(exceptionToHandle.Message);
					}
				}
				catch (Exception) { }
				handled = true;
			}
			else if (exceptionToHandle is Win32Exception win32Exception)
			{
				switch (win32Exception.NativeErrorCode)
				{
					case 121:
					case 1326:
					case 10054:
					case 258:
						try
						{
							ShowError(ObjectFactory.Get<TerminalService>().IsRemoteAppSession ? "RDP Timeout" : win32Exception.Message);
							handled = true;
						}
						catch (Exception) { handled = false; }
						break;

					case 8:
						try
						{
							if (win32Exception.Message == "Not enough storage is available to process this command")
							{
								ShowError("Not enough storage is available to process this command. Try closing and restarting programs and rebooting if problems persist.");
								handled = true;
							}
							else if (win32Exception.Message == "Error creating window handle.")
							{
								ShowError("There was an exception creating Windows controls. The system's graphical resources may be exhausted. Try to close unnecessary CargoWise forms and repeat your operation. If the problem persists, please contact your system administrator. If this problem will need to be escalated to support, please take a memory dump of CargoWise application for analysis.");
							}
						}
						catch (Exception) { handled = false; }
						break;

					case 1406:
						break;
					case 1158:
						handled = true;
						break;
					case 1816:
						try
						{
							handled = IsFlakyShutdownException(win32Exception);
							if (!handled)
							{
								ShowError("Not enough quota is available to process this command. Try closing and restarting programs and rebooting if problems persist. You can also try increasing the size of your paging file.");
								handled = IsTooManyWindowsMessagesException(win32Exception) || IsMainFormFailedToShowException(win32Exception) || IsTimerFailedToRestartException(win32Exception);
							}
						}
						catch (Exception) { handled = false; }
						break;
				}

				if (!handled && IsMainFormFailedToShowException(win32Exception)) // It happens for several different NativeErrorCode
				{
					ShowError("Error showing main form. Try closing and restarting programs and rebooting if problems persist. You can also try increasing the size of your paging file." + System.Environment.NewLine + win32Exception.Message);
					handled = true;
				}
				if (!handled && IsTimerFailedToRestartException(win32Exception)) // seen it for 0 and 87
				{
					ShowError("Error initializing heartbeat timer. Try closing and restarting programs and rebooting if problems persist.");
					handled = true;
				}
			}
			else if (exceptionToHandle is InvalidOperationException)
			{
				if (exceptionToHandle.Message.Contains("DataBinding cannot find a row in the list"))
				{
					ShowError("'DataBinding cannot find a row in the list that is suitable for all bindings.'\r\nThere is a mismatch between the database schema and what the program expected.\r\nHelp > Recreate Database Synonyms is likely to resolve this problem.");
					handled = true;
				}
				else if (exceptionToHandle.Message.Contains("An error has occurred while attempting to communicate with the"))
				{
					ShowError(exceptionToHandle.Message);
					handled = true;
				}
			}
			else if (exceptionToHandle is NullReferenceException)
			{
				handled = HandleNullReferenceException(exceptionToHandle);
			}
			else if (exceptionToHandle is CannotAddNewRowToTableException cannotAddRowToTableException)
			{
				ErrorReporter.ReportOnce("CannotAddRowToTableException", cannotAddRowToTableException.Message, cannotAddRowToTableException);
				ShowError("There was an internal error processing new " + cannotAddRowToTableException.NewBusinessObject.HumanReadableName + ". Please close current form and retry your action.");

				handled = true;
			}
			else if (exceptionToHandle is CannotSetCurrentLanguageToENGException cannotSetCurrentLanguageToENGException)
			{
				ErrorReporter.ReportOnce("CannotSetCurrentLanguageToENGException", cannotSetCurrentLanguageToENGException.Message, cannotSetCurrentLanguageToENGException);
				handled = true;
			}
			else if (exceptionToHandle is UnauthorizedAccessException)
			{
				handled = HandleFailedFileOperation(exceptionToHandle);
			}
			else if (exceptionToHandle is DatabaseMissingException)
			{
				ShowError(Res.GetString("BA1D5868-BEA0-4ED9-A91B-57EBABD28A09", "{0}\r\nPlease contact your system administrator.", exceptionToHandle.Message));
				handled = true;
			}
			else if (exceptionToHandle is ThreadAbortException)
			{
				handled = true;
			}
			else if (exceptionToHandle is COMException)
			{
				handled = HandleCOMException(exceptionToHandle);
			}
			else if (exceptionToHandle is UseVfpOleDbProviderIn64BitPlatformException)
			{
				ShowError(exceptionToHandle.Message);
				handled = true;
			}
			else if (exceptionToHandle.Message.Contains("Logon failure: unknown user name or bad password."))
			{
				ShowError(Res.GetString("332717C9-06BD-419B-912D-F10E8911437D", "An Active Directory authentication failure has occurred. Either your username/password/domain is incorrect/has changed, or this is an intermittent failure. If problems persist, please contact your system administrator"));
				handled = true;
			}
			else if (exceptionToHandle is EmailHasNoFromAddressException)
			{
				ShowError(Res.GetString("F6CE7E30-0623-47CE-852A-F7BF763F68FB", "Could not send an email because the 'from address' was empty. Message: {0}", exceptionToHandle.Message));
				handled = true;
			}
			else if (exceptionToHandle is CultureNotFoundException)
			{
				ShowError(Res.GetString("4226843b-c321-4a3b-8387-7c8f2258f5e3", "{0}\r\nMake sure that the culture/language selected is valid, has been installed on this computer, and that your Operating System is up to date."
					, exceptionToHandle.Message));
				handled = true;
			}
			else if (exceptionToHandle is ArgumentException)
			{
				if (exceptionToHandle.StackTrace?.Contains("System.Drawing") ?? false)
				{
					//we're in a state of resource exhaustion, so ShowError may fail.
					try
					{
						ShowError(Res.GetString("f2b2ddf4-d0fc-4d55-b599-5f808d4e0511", "If you just tried to upload an image: Invalid image format. Otherwise: The system's graphical resources are nearly exhausted. Some forms may not display properly. If problems persist, please contact your system administrator."));
					}
					catch (Exception e) when (!e.IsCriticalException()) { }
					handled = true;
				}
				else if (exceptionToHandle.Message.Equals("Width and Height must be non-negative."))
				{
					handled = true;
				}
			}
			else if (exceptionToHandle is ICriticalException critical && critical.IsCriticalException)
			{
				handled = true;
			}
			else if (exceptionToHandle.GetType().Name == "KDataBindingException" && exceptionToHandle.Message.Contains("Invalid object name")
				&& exceptionToHandle.Message.Contains("_SD") && !EnvProxy.Instance.IsProductionSystem)
			{
				ShowError(exceptionToHandle.Message);
				handled = true;
			}
			else if (exceptionToHandle is SEHException)
			{
				ShowError(Res.GetString("50901AB1-B7E2-4CFA-A4B2-D0BCD9C026AE", @"An unspecified external exception occurred.
 
The access to some files may have been denied or some files may have been corrupted.
The causes may include, but are not limited to, anti-viruses that block documents, network resources that are disconnected or timed out, corrupted files, applications, or Windows Updates.
 
Please retry your operation. Notify your system administrator if the error persists."));
				handled = true;
			}

			if (!handled)
			{
				if (ObjectFactory.Get(ExtraExceptionHandlersListName) is IEnumerable extraExceptionHandlers)
				{
					foreach (IExtraExceptionHandler extraExceptionHandler in extraExceptionHandlers.OfType<IExtraExceptionHandler>())
					{
						handled = extraExceptionHandler.HandleException(exceptionToHandle);
						if (handled)
						{
							break;
						}
					}
				}
			}

			return handled;
		}

		public const string ExtraExceptionHandlersListName = "ExtraExceptionHandlers";

		bool IsTooManyWindowsMessagesException(Win32Exception ex) => (ex.Source?.Contains("PresentationCore", StringComparison.Ordinal) ?? false) && (ex.StackTrace?.Contains("HwndTarget.UpdateWindowSettings") ?? false);

		bool IsMainFormFailedToShowException(Win32Exception ex) =>
			((ex.StackTrace?.Contains("MainForm.WndProc", StringComparison.Ordinal) ?? false) && (ex.StackTrace?.Contains("Control.WmShowWindow", StringComparison.Ordinal) ?? false)) ||
			(ex.StackTrace?.Contains("MainForm.SetVisibleCore", StringComparison.Ordinal) ?? false);

		bool IsTimerFailedToRestartException(Win32Exception ex) => (ex.StackTrace?.Contains("Timer.set_Enabled", StringComparison.Ordinal) ?? false) && (ex.StackTrace?.Contains("Timer.OnTick", StringComparison.Ordinal) ?? false);

		bool IsFlakyShutdownException(Win32Exception ex) // https://github.com/dotnet/roslyn/issues/9247
			=> ex.StackTrace.Contains("MS.Internal.ShutDownListener.HandleShutDown");

		internal bool HandleOwnerSIDDiffersFromMaster(Exception exceptionToHandle)
		{
			try
			{
				var dbName = GetDatabaseNameFromOwnerSIDDiffersFromMasterExceptionMessage(exceptionToHandle.Message);
				if (string.IsNullOrEmpty(dbName) || !Db.Connection.DatabaseExists(dbName))
				{
					return false;
				}

				try
				{
					using var connection = Db.NewAdminConnection();
					DataUtils.AlterDbAuthorisation(connection, dbName);
				}
				catch (OperationCanceledException ex) when (ex.Message.Contains("AlterDbAuthorisation_App_Lock"))
				{
					// Acquiring "AlterDbAuthorisation_App_Lock" timed out means `AlterDbAuthorisation` is already being performed by another process.
					return true;
				}
			}
			catch (DbException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.OwnerSIDDiffersFromMaster)
			{
				return false;
			}

			ShowError(Res.GetString("E3137DF9-3AF8-43B6-84EF-22E40818DB23", "Database settings were updated. Please try again."));
			return true;
		}

		internal string GetDatabaseNameFromOwnerSIDDiffersFromMasterExceptionMessage(string message)
		{
			var dbname = message.Split('\'');
			if (dbname.Length < 3)
			{
				return string.Empty;
			}
			return dbname[1];
		}

		static bool HandleTraditionalServiceException(CommunicationException exceptionToHandle)
		{
			try
			{
				if (IsTraditionalServiceUnavailableException(exceptionToHandle))
				{
					Db.Connection.EnsureIsOpen();
					return true;
				}
			}
			catch (CommunicationException e) when (IsTraditionalServiceUnavailableException(e))
			{
				return true;
			}
			return false;
		}

		static bool IsTraditionalServiceUnavailableException(CommunicationException exceptionToHandle)
		{
			const string traditionalServiceUrl = "Traditional/GlowLoader.svc";
			return exceptionToHandle is EndpointNotFoundException endpointNotFoundException && endpointNotFoundException.Message.Contains(traditionalServiceUrl, StringComparison.OrdinalIgnoreCase);
		}

		bool HandleCOMException(Exception exceptionToHandle)
		{
			bool handled = false;
			if (exceptionToHandle.Message.Contains("The user name or password is incorrect"))
			{
				ShowError(Res.GetString("332717C9-06BD-419B-912D-F10EC111437D", "The operation could not be completed because your log in credentials are not valid for this server"));
				handled = true;
			}
			else if (exceptionToHandle.IsWPFError())
			{
				ShowError(string.Format(CultureInfo.InvariantCulture, @"Oops. Something went wrong.

Error Message: {0}

Consider checking whether your video card drivers are up to date.

To prevent this from happening again, please visit this website for more details and steps on how to fix this: https://blogs.msdn.microsoft.com/dsui_team/2013/11/18/wpf-render-thread-failures/", exceptionToHandle.Message));
				handled = true;
			}
			else if (exceptionToHandle.IsHResult(-2147418113))
			{
				ShowError("COMException has been thrown. HRESULT: 0x8000FFFF (E_UNEXPECTED).\r\n\r\n Details for developers:\r\n" + exceptionToHandle.ToString());
				handled = true;
			}
			else if (exceptionToHandle.Message.Contains("Logon failure: unknown user name or bad password."))
			{
				ShowError(Res.GetString("332717C9-06BD-419B-912D-F10E8911437D", "An Active Directory authentication failure has occurred. Either your username/password/domain is incorrect/has changed, or this is an intermittent failure. If problems persist, please contact your system administrator"));
				handled = true;
			}
			return handled;
		}

		bool HandleFailedFileOperation(Exception outer)
		{
			var message = new StringBuilder("File operation failed. ")
				.Append("Details: ").Append(outer.Message);

			if (!Globals.CanShowDialogs)
			{
				message.AppendLine()
					.AppendLine("Additional Information: ")
					.AppendLine(AdditionalContextDetails ?? "N/A")
					.AppendLine()
					.AppendLine("Details: ");

				foreach (var ex in outer.FlattenInnerExceptions())
				{
					message.Append("Type: ").AppendLine(ex.GetType().FullName)
						.Append("Message: ").AppendLine(ex.Message)
						.AppendLine("Callstack: ")
						.Append(ex.StackTrace)
						.AppendLine();
				}
			}

			ShowError(message.ToString());
			return true;
		}

		bool HandleBufferedGraphicsContextCannotBeDisposed(Exception ex)
		{
			ShowWarning("A problem has occurred as a result of a graphics issue with the Microsoft .NET framework. If this problem persists, try restarting your computer (" + ex.Message + ").");
			return true;
		}

		bool IsUserVisibleException(Exception ex)
		{
			return ExceptionVisibilityAttribute.Evaluate(ex) == ExceptionVisibility.User;
		}

		bool HandleUserVisibleException(Exception ex)
		{
			ShowError(Res.GetString("8355ba44-403d-43b6-b800-53ef404bf603", "The current action was unable to be completed.\r\n\r\nReason : {0}", ex.Message));
			return true;
		}

		bool HandleOutOfMemoryException()
		{
			bool handled = false;

			try
			{
				ShowError(string.Format(CultureInfo.InvariantCulture, @"Out Of Memory Exception occurred.
If problems persist, please contact your system administrator and show them this information:

{0}", ResourcesMessage()));
				handled = true;
			}
			catch (Exception e)
			{
				if (!HandleSqlOrDatabaseUpgradeExceptionWhileHandlingException(e))
				{
					throw;
				}
			}

			return handled;
		}

		static string BytesToMBString(long bytes)
		{
			return (bytes / 1024 / 1024).ToString(CultureInfo.InvariantCulture) + " MB";
		}

		static string MegaBytesToString(ulong megabytes)
		{
			return megabytes.ToString(CultureInfo.InvariantCulture) + " MB";
		}

		internal class NativeMethods
		{
			[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool GetDiskFreeSpaceEx(string directoryName, ref long freeBytesForUser, ref long bytesForUser, ref long bytesOnDisk);
		}

		public static string GetDiskFreeSpace(string directoryName, ref long freeBytesForUser, ref long bytesForUser, ref long bytesOnDisk)
		{
			bool result = NativeMethods.GetDiskFreeSpaceEx(directoryName, ref freeBytesForUser, ref bytesForUser, ref bytesOnDisk);
			return result ? "" : Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture);
		}

		public static string ResourcesMessage()
		{
			var result = new StringBuilder();

			result.Append("Machine Name: ");
			result.AppendLine(System.Environment.MachineName);
			result.Append("UTC Time: ");
			result.AppendLine(DateTime.UtcNow.ToString());
			result.AppendLine();
			result.AppendLine("RAM");
			try
			{
				result.AppendLine("Physical");
				result.Append("Available: ");
				result.AppendLine(MegaBytesToString(ZSystemInformation.Instance.AvailablePhysicalMemory));
				result.Append("Total: ");
				result.AppendLine(MegaBytesToString(ZSystemInformation.Instance.TotalPhysicalMemory));

				result.AppendLine();
				result.AppendLine("Virtual");
				result.Append("Available: ");
				result.AppendLine(MegaBytesToString(ZSystemInformation.Instance.AvailableVirtualMemory));
				result.Append("Total: ");
				result.AppendLine(MegaBytesToString(ZSystemInformation.Instance.TotalVirtualMemory));

				result.AppendLine();
				result.AppendLine("CommittableMemory");
				result.Append("Available: ");
				result.AppendLine(MegaBytesToString(ZSystemInformation.Instance.AvailablePageFileSize));
				result.Append("Current Commit Limit: ");
				result.AppendLine(MegaBytesToString(ZSystemInformation.Instance.TotalPageFileSize));

				result.Append("Available System Wide: ");
				result.AppendLine(MegaBytesToString(ZSystemInformation.Instance.CommittableAvailableSystemWide));

				result.Append("Committed System Wide: ");
				result.AppendLine(MegaBytesToString(ZSystemInformation.Instance.CommittedTotalSystemWide));

				result.Append("Commit Limit System Wide: ");
				result.AppendLine(MegaBytesToString(ZSystemInformation.Instance.CommittLimitSystemWide));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result.AppendLine("Failed to get RAM information:");
				result.AppendLine(ex.Message);
			}

			result.AppendLine();

			try
			{
				result.AppendLine($"IsPageFileEnabled: {IsPageFileEnabled}");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result.AppendLine("Failed to get PagingFiles Registry:");
				result.AppendLine(ex.Message);
			}

			result.AppendLine();

			result.AppendLine("DiskSpace");

			try
			{
				var elementName = "EnterpriseDirectory";
				var path = EnvProxy.Instance.ApplicationStartupPath;
				result.Append(elementName);
				result.Append(" Path ");
				result.AppendLine(path);

				long freeBytesForUser = -1;
				long bytesForUser = -1;
				long freeBytesOnDisk = -1;

				string error = GetDiskFreeSpace(path, ref freeBytesForUser, ref bytesForUser, ref freeBytesOnDisk);
				if (string.IsNullOrEmpty(error))
				{
					result.Append("FreeForUser: ");
					result.AppendLine(BytesToMBString(freeBytesForUser));
					result.Append("AvailableToUser: ");
					result.AppendLine(BytesToMBString(bytesForUser));
					result.Append("FreeForSystem: ");
					result.AppendLine(BytesToMBString(freeBytesOnDisk));
				}
				else
				{
					result.Append("GetDiskFreeSpaceEx returned with error: ");
					result.AppendLine(error);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				result.Append("DiskSpaceError: Exception while processing disk space: ");
				result.AppendLine(e.ToString());
			}

			result.AppendLine();

			try
			{
				var elementName = "TempDirectory";
				var path = EnvProxy.Instance.TempPath;
				result.Append(elementName);
				result.Append(" Path ");
				result.AppendLine(path);

				long freeBytesForUser = -1;
				long bytesForUser = -1;
				long freeBytesOnDisk = -1;

				string error = GetDiskFreeSpace(path, ref freeBytesForUser, ref bytesForUser, ref freeBytesOnDisk);
				if (string.IsNullOrEmpty(error))
				{
					result.Append("FreeForUser: ");
					result.AppendLine(BytesToMBString(freeBytesForUser));
					result.Append("AvailableToUser: ");
					result.AppendLine(BytesToMBString(bytesForUser));
					result.Append("FreeForSystem: ");
					result.AppendLine(BytesToMBString(freeBytesOnDisk));
				}
				else
				{
					result.Append("GetDiskFreeSpaceEx returned with error: ");
					result.AppendLine(error);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				result.Append("DiskSpaceError: Exception while processing disk space: ");
				result.AppendLine(e.ToString());
			}

			result.AppendLine();

			result.AppendLine("SystemResourcesUsage");

			try
			{
				result.Append(ObjectFactory.Get<IFormsErrorReportDetailsProvider>().GetSystemResourcesUsageText());

				result.Append("ManagedHeapUsage: ");
				result.Append((GC.GetTotalMemory(false) / 1024).ToString(CultureInfo.InvariantCulture));
				result.AppendLine("KB"); // GetTotalMemory(false) is ok.

				try
				{
					long workingSetSize = Process.GetCurrentProcess().WorkingSet64;  // Try to get it for NT based OSs, catch the exception otherwise
					result.Append("WorkingSetSize: ");
					result.Append((workingSetSize / 1024).ToString(CultureInfo.InvariantCulture));
					result.AppendLine("KB");
				}
				catch (PlatformNotSupportedException)
				{
					result.AppendLine("WorkingSetSize: Working set not supported on this OS.");
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result.AppendLine("Failed to get system resource usage information:");
				result.AppendLine(ex.Message);
			}

			return result.ToString();
		}

		static bool IsPageFileEnabled
		{
			get
			{
				var pagingFilesStrings = (string[])Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "PagingFiles", null);
				return pagingFilesStrings?.Length > 0 && pagingFilesStrings.Any(p => !string.IsNullOrEmpty(p));
			}
		}

		bool HandleNullReferenceException(Exception ex)
		{
			// Detect the issue caused by System.Windows.Forms when a citrix session is detected.
			if (!string.IsNullOrEmpty(ex.Source) && ex.Source.Contains("System.Windows.Forms") && !string.IsNullOrEmpty(ex.StackTrace) && ex.StackTrace.Contains("get_BitsPerPixel"))
			{
				string sessionName = System.Environment.GetEnvironmentVariable("sessionname");
				if (!string.IsNullOrEmpty(sessionName) && sessionName.StartsWith("ica", StringComparison.OrdinalIgnoreCase))
				{
					ShowError("A GUI error has occurred as the primary screen is not detected on the current computer. This is a known issue with .Net applications running on a Citrix server in multi-screen mode and with primary screen set on right. Try to change the multi-screen setting by moving the primary screen to the leftmost. For more information, please refer to the link: https://connect.microsoft.com/VisualStudio/feedback/details/391340/menustrip-does-not-support-lack-of-primary-screen.");
					return true;
				}
			}
			// More null reference exception handlings can be added here.
			return false;
		}

		bool HandleCommunicationException(CommunicationException ex)
		{
			if (!HandleTraditionalServiceException(ex))
			{
				ShowError(ex.ToString(), "Problems communicating with server");
			}
			return true;
		}

		bool HandleSqlLockLostException()
		{
			try
			{
				Db.Connection.EnsureIsOpen();
			}
			catch (SqlLockLostException)
			{
				return false;
			}

			ShowError("Connection to the database was lost, please try again.");
			return true;
		}

		bool HandleSqlLoadingUntrustedAssemblyException()
		{
			try
			{
				using (var connection = Db.NewAdminConnection())
				{
					DataUtils.SetTrustworthyOn(connection, Db.DatabaseName);
				}
			}
			catch (SqlException ex) when (new[] { DbErrorType.ModuleBeingExecutedIsNotTrusted, DbErrorType.ErrorLoadingUntrustedAssembly }.Contains(new DbErrorMatch(ex).ExceptionType))
			{
				return false;
			}

			ShowError(DbSettingUpdatedMessage);
			return true;
		}
		const string DbSettingUpdatedMessage = "Database settings were updated. Please try again your last action.";

		internal bool HandleAnySqlExceptionsSilently(Exception outerException)
		{
			return HandleSpecificExceptions(outerException, outerException, HandleSqlExceptionSilently);

			bool HandleSqlExceptionSilently(Exception ex, Exception outermostExceptionForErrorReport)
			{
				var sqlException = ex as DbException;
				return sqlException != null
					&& HandleSqlExceptionCore(sqlException, outermostExceptionForErrorReport, handleSilently: true);
			}
		}
		/// <summary>
		/// Handles SqlException
		///   - Sends a silent exception report depending on the type of SQL error
		///   - Shows a user-friendly message
		/// </summary>
		internal bool HandleSqlException(DbException ex, Exception outermostExceptionForErrorReport)
			=> HandleSqlExceptionCore(ex, outermostExceptionForErrorReport, handleSilently: false);

		bool HandleSqlExceptionFromUpgrade(DbException ex, DbErrorHandler errorHandler)
		{
			try
			{
				if (Db.IsUpgradeLockoutError(ex))
				{
					DbEnv.Instance.ConnectionGuiPlugin.HandleDatabaseUpgradeException(new DatabaseUpgradeInProgressException());
					return true;
				}
				else if (DbErrorMatch.IsNetworkError(errorHandler.ExceptionType) && ex.Message.Contains("SHUTDOWN is in progress."))
				{
					return true;
				}
			}
			catch (Exception e)
			{
				if (!HandleSqlOrDatabaseUpgradeExceptionWhileHandlingException(e))
				{
					throw;
				}
			}

			return false;
		}

		bool HandleSqlExceptionDatabasePrincipleError(DbException ex, DbErrorHandler errorHandler)
		{
			try
			{
				if (DbErrorMatch.IsCannotExecuteAsDatabasePrincipalError(errorHandler.ExceptionType))
				{
					using (var conn = Db.NewAdminConnection())
					{
						DataUtils.AlterDbAuthorisation(conn, Db.DatabaseName, ex);
					}
					ShowError(DbSettingUpdatedMessage);
					return true;
				}
			}
			catch (Exception e)
			{
				if (!HandleSqlOrDatabaseUpgradeExceptionWhileHandlingException(e))
				{
					throw;
				}
			}

			return false;
		}

		bool HandleSqlExceptionUserAccessMode(DbErrorHandler errorHandler)
		{
			try
			{
				if (DbErrorMatch.IsDatabaseInSingleUserModeError(errorHandler.ExceptionType)
					&& GetDatabaseUserAccessMode(Db.DatabaseName) == "SINGLE_USER")
				{
					SetDatabaseIntoAccessMode(Db.DatabaseName, "MULTI_USER");
					Db.Connection.EnsureIsOpen();
					return true;
				}
			}
			catch (Exception e)
			{
				if (!HandleSqlOrDatabaseUpgradeExceptionWhileHandlingException(e))
				{
					throw;
				}
			}

			return false;
		}

		bool HandleSqlExceptionDdlDisconnection(DbErrorHandler errorHandler)
		{
			try
			{
				if (DbErrorMatch.SqlExceptionDdlDisconnection(errorHandler.ExceptionType))
				{
					Db.Connection.EnsureIsOpen();
					return true;
				}
			}
			catch (Exception e)
			{
				if (!HandleSqlOrDatabaseUpgradeExceptionWhileHandlingException(e))
				{
					throw;
				}
			}

			return false;
		}

		bool HandleSqlExceptionInvalidColumnName(DbException ex)
		{
			try
			{
				if (new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidColumnName)
				{
					const string groupName = "columnName";
					var columnRegex = new Regex(@$"Invalid column name '(?<{groupName}>\w+)'");
					var columnName = columnRegex.Match(ex.Message).Groups[groupName].Value;

					if (columnName.StartsWith(StorageDocsSchema.PK.ColumnPrefix) && Db.Connection.Exists("FROM sys.columns WHERE name = @columnName", cmd => cmd.AddParameter("@columnName", SqlDbType.NVarChar, 128, columnName)))
					{
						GlobalsMessageShowWithErrorHandling(Res.GetString("FF4363A1-5D49-4918-B03B-EA09E55D87BF", "An error occurred while accessing the eDoc. This can be due to an outdated eDocs database schema. Please inform your System Administrator and have them ensure the eDocs database(s) are restored correctly."));

						return true;
					}

					return false;
				}
			}
			catch (Exception e)
			{
				if (!HandleSqlOrDatabaseUpgradeExceptionWhileHandlingException(e))
				{
					throw;
				}
			}

			return false;
		}

		bool HandleSqlExceptionCore(DbException ex, Exception outermostExceptionForErrorReport, bool handleSilently)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var errorHandler = new DbErrorHandler(ex, Db.Connection);

				if (errorHandler.ExceptionType != DbErrorType.TimeoutExpired)
				{
					if (HandleSqlExceptionInvalidColumnName(ex))
					{
						return true;
					}

					if (HandleSqlExceptionFromUpgrade(ex, errorHandler))
					{
						return true;
					}

					if (HandleSqlExceptionDatabasePrincipleError(ex, errorHandler))
					{
						return true;
					}

					if (HandleSqlExceptionUserAccessMode(errorHandler))
					{
						return true;
					}

					if (HandleSqlExceptionDdlDisconnection(errorHandler))
					{
						return true;
					}
				}

				var friendlyMessage = errorHandler.GetDBErrorUserFriendlyMessage();
				if (!string.IsNullOrWhiteSpace(friendlyMessage))
				{
					try
					{
						if (errorHandler.ExceptionType == DbErrorType.FailedToInitClrDueToMemPressure)
						{
							var result = SendEmailForFailedToInitClrDueToMemPressureError(ex, outermostExceptionForErrorReport);

							if (result.Length > 0)
							{
								friendlyMessage = friendlyMessage + "\n" + result;
							}
						}
						else if (!errorHandler.IsInfrastructureDbError && ReportAllNonCritialSqlErrors_UnlessWeCannotConnectToTheDatabase(errorHandler.ExceptionType, outermostExceptionForErrorReport))
						{
							var errorKey = "SQLUserFriendlyError";
							var exWrapper = new SqlExceptionWrapper(ex);
							if (exWrapper.Errors.Count > 0)
							{
								errorKey += ":" + exWrapper.Errors[0].Number;
							}

							ExceptionReporter.Instance.ReportSilently(
								errorKey,
								friendlyMessage + NewLine + NewLine + errorHandler.GetExtraDebugInformation(),
								outermostExceptionForErrorReport);
						}
					}
					catch (Exception e)
					{
						if (!HandleSqlOrDatabaseUpgradeExceptionWhileHandlingException(e))
						{
							throw;
						}
					} // We may hit the DB and we may not be able to hit the DB at all at this point

					try
					{
						if (!handleSilently)
						{
							ShowErrorDialogIfNotSilentlyHandled(errorHandler, friendlyMessage, outermostExceptionForErrorReport);
						}
					}
					catch (Exception e)
					{
						if (!HandleSqlOrDatabaseUpgradeExceptionWhileHandlingException(e))
						{
							throw;
						}
					}
					return true;
				}
				else
				{
					try
					{
						Db.Connection.EnsureIsOpen();
					}
					catch (DbException)
					{
					}
					catch (DatabaseUpgradeException)
					{
						Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
						return true;
					}
				}

				return false;
			}
		}

		static bool HandleDatabaseUpgradeExceptionWhileHandlingException(Exception ex)
		{
			var databaseUpgradeException = ex.Find<DatabaseUpgradeException>();
			if (databaseUpgradeException == null)
			{
				return false;
			}

			OnDatabaseUpgradeExceptionWhileHandlingException(databaseUpgradeException);
			return true;
		}

		static void OnDatabaseUpgradeExceptionWhileHandlingException(DatabaseUpgradeException databaseUpgradeException)
		{
			Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
			DbEnv.Instance.ConnectionGuiPlugin.HandleDatabaseUpgradeException(databaseUpgradeException);
		}

		static bool HandleSqlOrDatabaseUpgradeExceptionWhileHandlingException(Exception ex)
		{
			return HandleDatabaseUpgradeExceptionWhileHandlingException(ex)
				|| ex.Find<DbException>() != null;
		}

		bool ReportAllNonCriticalSqlErrors
		{
			get
			{
				return ReportAllUnhandledSqlExceptions || DataRegistry.Instance.ReportAllNonCriticalSqlErrors;
			}
		}

		protected virtual bool ReportAllUnhandledSqlExceptions => false;

		bool ReportAllNonCritialSqlErrors_UnlessWeCannotConnectToTheDatabase(DbErrorType errorType, Exception outermostExceptionForErrorReport)
		{
			if (errorType == DbErrorType.TimeoutExpired && Db.Connection.State != ConnectionState.Open)
			{
				return false;
			}
			else if (outermostExceptionForErrorReport is RefDataException)
			{
				return true;
			}
			else
			{
				return ReportAllNonCriticalSqlErrors;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal static string GetDatabaseUserAccessMode(string dbName)
		{
			using (var adminConnection = Db.NewAdminConnection(Db.ServerName, Db.SqlMasterDb))
			using (adminConnection.UseMasterDb())
			{
				var sqltext = "select user_access_desc from sys.databases where name = @DbName";
				using (var command = adminConnection.Command(sqltext))
				{
					command.AddParameter("@DbName", SqlDbType.VarChar, dbName);

					return (string)command.ExecuteScalar();
				}
			}
		}

		internal static void SetDatabaseIntoAccessMode(string dbName, string accessMode)
		{
			using (var adminConnection = Db.NewAdminConnection(Db.ServerName, Db.SqlMasterDb))
			{
				//We need to kill all connected spids to dbName first.
				DbConnectionKiller.KillOtherConnections(adminConnection, dbName);

				var sqltext = FormattableString.Invariant($@"
SET DEADLOCK_PRIORITY HIGH
ALTER DATABASE [{dbName}] SET {accessMode} WITH NO_WAIT
ALTER DATABASE [{dbName}] SET {accessMode} WITH ROLLBACK IMMEDIATE");

				adminConnection.ExecuteNonQuery(sqltext);
			}
		}

		string SendEmailForFailedToInitClrDueToMemPressureError(DbException ex, Exception outermostExceptionForErrorReport)
		{
			var subject = "Failed to initialize the Common Language Runtime (CLR) v2.0.50727 due to memory pressure.";
			var body = "Outer most exception message: " + outermostExceptionForErrorReport.Message +
				"\n\tStack trace: " + outermostExceptionForErrorReport.StackTrace +
				"\n\n" +
				"SqlException message: \n" + ex.Message +
				"\n\tStack trace: " + ex.StackTrace;

			return SendEmail.ToDatabaseHealthCheckNotificationGroup(subject, body);
		}

		protected virtual void ShowErrorDialogIfNotSilentlyHandled(DbErrorHandler errorHandler, string friendlyMessage, Exception outermostExceptionForErrorReport)
		{
			var dbFileFullErrors = new[] { DbErrorType.LogIsFull, DbErrorType.DbFilegroupIsFull };
			if (dbFileFullErrors.Contains(errorHandler.ExceptionType))
			{
				ShutdownEnterprise(friendlyMessage);
			}
			else
			{
				if (Globals.Message is UnattendedUserNotification unattended) // Batch Processor
				{
					var now = DateTime.UtcNow;
					var timeSinceLastMessage = now - lastUnattendedMessage;
					lastUnattendedMessage = now;

					if (timeSinceLastMessage > unattendedMessageWaitLimit && errorHandler.IsInfrastructureDbError)
					{
						unattended.ShowInfrastructureError(
							UnattendedErrorMessage
							+ "\r\n\r\n" + friendlyMessage + "\r\n\r\nError Details:"
							+ new ExceptionDetails(outermostExceptionForErrorReport).GetStackTraceAndMessage(), outermostExceptionForErrorReport.GetType().ToString());
					}
				}
				else
				{
					GlobalsMessageShowErrorWithErrorHandling(friendlyMessage);
				}
			}
		}
		DateTime lastUnattendedMessage = DateTime.MinValue;
		readonly TimeSpan unattendedMessageWaitLimit = TimeSpan.FromSeconds(5);

		/// <summary>
		/// Handles uncaught System.Data.DBConcurrencyException
		///   - Sends a silent exception
		///   - Shows a user-friendly message
		/// </summary>
		protected void HandleDbConcurrencyException(Exception ex, Exception mostOuterException)
		{
			DbEnv.Instance.ConnectionGuiPlugin.HandleDbConcurrencyException(ex);

			if (!ex.NotifyUserWithoutErrorReport() && ex.Source != nameof(ConcurrencyExceptionHandler))
			{
				var caughtByTopException = new DeveloperNotificationException("Put Factory.Save() in a try/catch. This exception was caught by the top level exception reporter.", mostOuterException);
				ExceptionReporter.Instance.ReportDeveloperException(caughtByTopException.Message, caughtByTopException);
			}
		}

		bool HandleSystemRegistrationKeyException()
		{
#if DEBUG
			ShowError(
				"Your System Registration Key is out of date. " +
				"This happens because this system is not registered within ediProd.\r\n" +
				"The simplest work around in a test system is to delete the registration key from the registry " +
				"by running the following command on Z Query Analyzer or SQL Server Management Studio:\r\n\r\n" +
				"DELETE dbo.StmData WHERE SD_Name = 'FreightNotesHeaderLength';");
#else
			ShowError("System Registration is invalid. This system is not correctly licenced. Please contact your sales representative for an updated licence.");
#endif
			return true;
		}

		static bool HandleCorruptedInstallationException(Exception exceptionToHandle)
		{
			SafeEventLogExtensions.SafeWriteEntryToApplicationLog(exceptionToHandle.ToString(), EventLogEntryType.Error, false);

			if (Globals.IsUserInteractive)
			{
				try
				{
					var dialogResult = GlobalsMessageShowWithErrorHandling(string.Format(CultureInfo.InvariantCulture, @"{0} was unable to load a program file into memory.

Error Message: {1}
Full error details have been written to the Windows Event Log.

This may be due to one of the following:
- A problem with the local file system, physical hard drive, or available disk space
- Another program (such as an anti-virus) holding exclusive access to one or more {0} files
- A problem with the installation of {0}

Would you like to attempt to repair the {0} installation?", Constants.ProductName, exceptionToHandle.Message),
					Constants.ProductName + " Program File Error", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Error, ZDialogResult.Yes);
					if (dialogResult == ZDialogResult.Yes)
					{
						RepairCurrentInstallation();
					}
					else
					{
						GlobalsMessageShowWithErrorHandling("Please close and re-open " + Constants.ProductName + " before continuing.");
					}
				}
				catch (Win32Exception)
				{
					//swallow - no point in having it bubble and reporting it to us, and we can't safely show it to the user anymore
				}
			}
			else
			{
				GlobalsMessageShowWithErrorHandling(string.Format(CultureInfo.InvariantCulture, @"{0} was unable to load a program file into memory.

Error Message: {1}
Full error details have been written to the Windows Event Log.

This may be due to one of the following:
- A problem with the local file system, physical hard drive, or available disk space
- Another program (such as an anti-virus) holding exclusive access to one or more {0} files
- A problem with the installation of {0}", Constants.ProductName, exceptionToHandle.Message));
			}

			return true;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is the top level handler, bubbling out of this may cause an infinite loop")]
		public static void RepairCurrentInstallation()
		{
			try
			{
				UpgradeManager upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
				var currentVersion = upgradeManager.QueryCurrentVersion();
				if (currentVersion == null)
				{
					GlobalsMessageShowErrorWithErrorHandling("The current version of " + Constants.ProductName + " could not determined.", Constants.ProductName + " Installation Repair Failed");
				}
				else
				{
					if (!Version.TryParse(Path.GetFileName(AssemblyLoader.GetBinPath()), out Version binPathVersion) || binPathVersion != currentVersion.Version)
					{
						GlobalsMessageShowErrorWithErrorHandling("You are not running the current version of " + Constants.ProductName + " from a valid Program Files directory.", Constants.ProductName + " Installation Repair Failed");
					}
					else
					{
						upgradeManager.Repair(currentVersion, AssemblyLoader.GetBinPath());
						GlobalsMessageShowWithErrorHandling("Repair complete. Please close and re-open " + Constants.ProductName + " before continuing.");
					}
				}
			}
			catch (Exception ex)
			{
				GlobalsMessageShowErrorWithErrorHandling("Repair failed: " + ex.Message);
			}
		}

		public virtual void ShutdownEnterprise(string userMessage)
		{
			ObjectFactory.Get<IProgramRestarter>().ShutdownEnterpriseWithMessage(userMessage);
		}

		public virtual void ShowError(string message)
		{
			using (ObjectFactory.Get<INeedToShowMessage>().SuppressNewFormInTransactionWarning())
			{
				GlobalsMessageShowErrorWithErrorHandling(message);
			}
		}

		protected virtual void ShowError(string message, string caption)
		{
			using (ObjectFactory.Get<INeedToShowMessage>().SuppressNewFormInTransactionWarning())
			{
				GlobalsMessageShowErrorWithErrorHandling(message, caption);
			}
		}

		static void GlobalsMessageShowErrorWithErrorHandling(string message)
		{
			try
			{
				Globals.Message.ShowError(message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				//swallow - no point in having it bubble and reporting it to us, and we can't safely show it to the user anymore
			}
		}

		static void GlobalsMessageShowErrorWithErrorHandling(string message, string caption)
		{
			try
			{
				Globals.Message.ShowError(message, caption);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				//swallow - no point in having it bubble and reporting it to us, and we can't safely show it to the user anymore
			}
		}

		static void GlobalsMessageShowWarningWithErrorHandling(string message)
		{
			try
			{
				Globals.Message.ShowWarning(message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				//swallow - no point in having it bubble and reporting it to us, and we can't safely show it to the user anymore
			}
		}

		static void GlobalsMessageShowWithErrorHandling(string message)
		{
			try
			{
				Globals.Message.Show(message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				//swallow - no point in having it bubble and reporting it to us, and we can't safely show it to the user anymore
			}
		}

		static ZDialogResult GlobalsMessageShowWithErrorHandling(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, ZDialogResult defaultResult, IComponent parent = null)
		{
			ZDialogResult result = ZDialogResult.None;
			try
			{
				result = Globals.Message.Show(message, caption, buttons, icon, defaultResult, parent);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				//swallow - no point in having it bubble and reporting it to us, and we can't safely show it to the user anymore
			}
			return result;
		}

		protected virtual void ShowWarning(string message)
		{
			using (ObjectFactory.Get<INeedToShowMessage>().SuppressNewFormInTransactionWarning())
			{
				GlobalsMessageShowWarningWithErrorHandling(message);
			}
		}

		#endregion
	}

	#endregion
}
