using System;
using System.Text;
using Enterprise.Integration;
using MailKit;

namespace Enterprise.RemotePrinting.Client
{
	sealed class LoggerxTAdaptor : ILogger
	{
		public LoggerxTAdaptor(JPNACCSxTController controller)
		{
			this.controller = controller;
		}

		public void Log(LogType type, string message)
		{
			controller.Log(type, message);
		}

		public void Log(LogType type, string message, Exception ex)
		{
			Log(type, message);
		}

		readonly JPNACCSxTController controller;
	}

	sealed class NullLogger : ILogger
	{
		public void Log(LogType type, string message)
		{
		}

		public void Log(LogType type, string message, Exception ex)
		{
		}
	}

	sealed class ProtocolLoggerAdaptor : IProtocolLogger
	{
		const string PasswordLogPrefix = "PASS ";
		const string AuthenticateLogPrefix = "AUTH ";

		public ProtocolLoggerAdaptor(INotifications notifications)
		{
			this.notifications = notifications;
			encoding = Encoding.GetEncoding(NACCSConstants.Encodings.DefaultEncoding);
		}

		readonly Encoding encoding;
		readonly INotifications notifications;

		public IAuthenticationSecretDetector AuthenticationSecretDetector { get; set; }

		public void LogClient(byte[] buffer, int offset, int count)
		{
			var log = GetSafeLog(buffer, offset, count);
			notifications.AddMessage($"C: {log}");
		}

		public void LogConnect(Uri uri)
		{
			notifications.AddMessage($"Connect: {uri}");
		}

		public void LogServer(byte[] buffer, int offset, int count)
		{
			notifications.AddMessage($"S: {Encoding.GetEncoding(NACCSConstants.Encodings.DefaultEncoding).GetString(buffer, offset, count)}");
		}

		public void Dispose()
		{
		}

		string GetSafeLog(byte[] buffer, int offset, int count)
		{
			var log = encoding.GetString(buffer, offset, count);

			if (log.StartsWith(PasswordLogPrefix) || log.StartsWith(AuthenticateLogPrefix))
			{
				var prefix = log.Substring(0, 5);
				var content = new string('*', log.Length - 5);

				log = prefix + content;
			}

			return log;
		}
	}
}
