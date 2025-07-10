using System;
using System.Globalization;
using System.Text;
using ServiceManager.Common.Abstractions;

namespace ServiceManager.Logging.CW
{
	public static class Logger
	{
		public static string GetFormattedLogLine(int sequenceId, DateTime currentDateTime, string logType, string hostName, int processId, string message, Exception ex)
		{
			var formattedMessage = string.Format("{0}{1}", message, FormatMessage(string.Empty, ex));
			var dateTimeStamp = currentDateTime.ToString(LogStringFormats.LoggerTimeMask, CultureInfo.InvariantCulture);

			logType = logType switch
			{
				"Info" => "Information",
				"Warn" => "Warning",
				null => string.Empty,
				_ => logType
			};

			return
				sequenceId.ToString(CultureInfo.InvariantCulture).PadRight(LoggerFieldLength)
				+ dateTimeStamp.PadRight(LoggerFieldLength)
				+ logType.PadRight(LoggerFieldLength)
				+ processId.ToString(CultureInfo.InvariantCulture).PadRight(LoggerFieldLength)
				+ $"hostname[{hostName}]".PadRight(LoggerFieldLength)
				+ formattedMessage;
		}

		internal static string FormatMessage(string message, Exception? ex)
		{
			if (ex != null)
			{
				message = string.Format(CultureInfo.InvariantCulture, "{0}\n{1}", message, ex.ToString());
			}
			return GetEscapedSingleLineString(message);
		}

		internal static string GetEscapedSingleLineString(string unformattedMessage)
		{
			if (string.IsNullOrEmpty(unformattedMessage))
			{
				return unformattedMessage;
			}

			var sb = new StringBuilder(unformattedMessage.Length);

			foreach (var ch in unformattedMessage)
			{
				switch (ch)
				{
					case '\\':
						sb.Append("\\\\");
						break;
					case '\t':
						sb.Append("\u2192");
						break;
					case '\u2192':
						sb.Append("\\\u2192");
						break;
					case '\n':
						sb.Append("\u21B5");
						break;
					case '\u21B5':
						sb.Append("\\\u21B5");
						break;
					case '\r':
						break;
					default:
						sb.Append(ch);
						break;
				}
			}

			return sb.ToString();
		}

		public static string GetUnescapedMultilineString(string formattedMessage)
		{
			if (string.IsNullOrEmpty(formattedMessage))
			{
				return formattedMessage;
			}

			var sb = new StringBuilder(formattedMessage.Length);
			bool escaped = false;
			foreach (var ch in formattedMessage)
			{
				if (escaped)
				{
					escaped = false;
					sb.Append(ch);
				}
				else
				{
					switch (ch)
					{
						case '\u2192':
							sb.Append("\t");
							break;
						case '\u21B5':
							sb.Append('\n');
							break;
						case '\\':
							escaped = true;
							break;
						default:
							sb.Append(ch);
							break;
					}
				}
			}

			return sb.ToString();
		}

		public static readonly short LoggerFieldLength = 25;
		public static readonly string LogFileExtension = ".txt";
	}
}
