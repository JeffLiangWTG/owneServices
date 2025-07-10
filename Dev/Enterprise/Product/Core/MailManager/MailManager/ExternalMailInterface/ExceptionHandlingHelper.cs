using System;
using System.IO;
using System.Text;
using Enterprise.ZArchitecture.Core;
using MailKit.Security;

namespace Enterprise.MailManager.ExternalMailInterface
{
	internal static class ExceptionHandlingHelper
	{
		internal static void ThrowAuthLoginNotSupportedException(MailServerConnectionConfiguration config)
		{
			throw new SmtpConfigurationException($"AUTH LOGIN is not supported by {config.Server} but AUTH LOGIN credentials have been configured");
		}

		internal static void ThrowStartTlsNotSupportedException(MailServerConnectionConfiguration config)
		{
			throw new SmtpConfigurationException($"STARTTLS is not supported by {config.Server} but secure connection using TLS is configured");
		}

		internal static void ThrowFailedToConnectException(Exception exception, Exception retryException)
		{
			throw new FailedToConnectException(exception.Message + GetSslHandshakeExceptionInfo(exception as SslHandshakeException) + "\r\nRetry message: " + retryException.Message + GetSslHandshakeExceptionInfo(retryException as SslHandshakeException), exception.InnerException);
		}

		internal static void ThrowFailedToAuthenticateException(Exception exception)
		{
			throw new FailedToAuthenticateException(exception.Message, exception.InnerException ?? exception);
		}

		internal static void ThrowFailedToSendMessageException(Exception exception)
		{
			throw new FailedToSendMessageException(exception.Message, exception.InnerException ?? exception);
		}

		internal static bool ShouldReconnect(this Exception exception)
		{
			return exception is IOException || exception is SslHandshakeException;
		}

		internal const string StartTls = "StartTls";

		internal static string GetSslHandshakeExceptionInfo(SslHandshakeException e)
		{
			if (e == null)
			{
				return null;
			}
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("--");
			stringBuilder.AppendLine((NoResString)"Server Certificate: ");
			stringBuilder.AppendLine(e.ServerCertificate?.ToString());
			stringBuilder.AppendLine("--");
			stringBuilder.AppendLine((NoResString)"Root Certificate Authority: ");
			stringBuilder.AppendLine(e.RootCertificateAuthority?.ToString());
			return stringBuilder.ToString();
		}
	}
}
