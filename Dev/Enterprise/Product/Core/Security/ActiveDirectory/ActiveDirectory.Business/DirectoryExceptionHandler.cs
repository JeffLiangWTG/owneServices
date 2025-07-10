using System;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Authentication;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory
{
	public static class DirectoryExceptionHandler
	{
		public static void ExecuteWithExceptionHandling(Action action, string identifier = null)
		{
			try
			{
				action();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!TryHandleDirectoryException(ex, identifier))
				{
					throw;
				}
			}
		}

		public static void ExecuteWithExceptionHandling(Action action, Type[] exceptionTypesToHandle, string identifier = null)
		{
			try
			{
				action();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (!TryHandleDirectoryException(ex, exceptionTypesToHandle, identifier))
				{
					throw;
				}
			}
		}

		public static bool TryHandleDirectoryException(Exception ex, string identifier = null)
		{
			Type[] defaultExceptionsToHandle = new Type[] { typeof(AuthenticationException), typeof(DirectoryServicesCOMException), typeof(ActiveDirectoryObjectNotFoundException), typeof(DirectoryServicesException) };
			return TryHandleDirectoryException(ex, defaultExceptionsToHandle, identifier);
		}

		static bool TryHandleDirectoryException(Exception ex, Type[] exceptionTypeToHandle, string identifier)
		{
			var exceptionToHandle = ex;
			string messageToReport = null;
			bool containDomainRelatedException = false;

			while (exceptionToHandle != null)
			{
				if (exceptionTypeToHandle.Any(type => type == exceptionToHandle.GetType()))
				{
					if (IsDomainRelatedException(exceptionToHandle))
					{
						containDomainRelatedException = true;
					}

					messageToReport = GetExceptionMessage(messageToReport, exceptionToHandle);
				}

				exceptionToHandle = exceptionToHandle.InnerException;
			}

			if (containDomainRelatedException)
			{
				messageToReport = GetGenericADErrorMessage(messageToReport);
			}

			// We only extend the message with identifier in non-interactive mode for exception that is handled.
			if (messageToReport != null && !string.IsNullOrEmpty(identifier) && !Globals.IsUserInteractive)
			{
				var identityMessage = ResString.GetMultilingualString("D5FD6248-0ADE-4868-BBC7-49E5A3EDB17D", "Cannot process entity {0}", identifier);
				messageToReport = string.Join(System.Environment.NewLine, identityMessage, messageToReport);
			}

			if (messageToReport == null)
			{
				return false;
			}

			ADNotification.ShowError(messageToReport);

			if (ActiveDirectoryRegistry.Instance.ReportHandledDirectoryExceptions.Value)
			{
				ErrorReporter.ReportOnce(ex.Message, ex);
			}

			return true;
		}

		static bool IsDomainRelatedException(Exception exception) => exception is AuthenticationException || exception is DirectoryServicesCOMException || exception is ActiveDirectoryObjectNotFoundException;

		static bool ShouldShowStackTrace => !Globals.IsUserInteractive && (EnvProxy.IsHostedWithCargowise || ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem());

		static string GetExceptionMessage(string previousMessages, Exception exception)
		{
			var innerMessage = IsDomainRelatedException(exception) && ShouldShowStackTrace ? exception.ToString() : exception.Message;
			if (Globals.IsUserInteractive)
			{
				return innerMessage;
			}
			else
			{
				return previousMessages == null ? innerMessage : string.Join("--" + System.Environment.NewLine, previousMessages, innerMessage);
			}
		}

		static string GetGenericADErrorMessage(string message) => ResString.GetMultilingualString("0655363d-3059-4d7b-aebf-de005e4ecfd6", @"An error occurred while communicating with the Active Directory controller. Please contact your System Administrator.

Error message is as follow:
{0}", message);
	}
}
