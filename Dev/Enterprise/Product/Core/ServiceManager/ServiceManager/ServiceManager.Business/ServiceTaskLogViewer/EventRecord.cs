using System;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using ServiceManager.Common.Abstractions;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Business
{
	public class EventRecord : AutoEventRecord
	{
		public EventRecord(ZDateTime dateTime, ZString type, ZString message, int processId, string hostName, int elasticSequenceId)
			: base(dateTime, type, message)
		{
			ProcessId = processId;
			HostName = hostName;
			ElasticSequenceId = elasticSequenceId;
		}

		#region Properties

		[ResourceStringData("049F7B6C-D019-498D-92D2-2A3F013AEB4E", Caption = "Message")]
		[MaxLength(Schema.MessageMaxLength)]
		public ZString FormattedMessage
		{
			get
			{
				return Logger.GetUnescapedMultilineString(Message);
			}
		}

		public ZPropertyInfo FormattedMessageInfo
		{
			get { return GetZPropertyInfo(nameof(FormattedMessage)); }
		}

		public ZBlob FormattedMessageAsBlob => new ZBlob(System.Text.Encoding.UTF8.GetBytes(FormattedMessage));

		public ZBlob FormattedMessageAsBlob_HTML => new ZBlob(System.Text.Encoding.UTF8.GetBytes(FormattedMessage));

		public ZInt SequenceNumber
		{
			get;
			internal set;
		}

		public ZInt ElasticSequenceId
		{
			get;
			private set;
		}

		public ZInt ProcessId
		{
			get;
			private set;
		}

		public ZString HostName
		{
			get;
			private set;
		}

		public ZDateTime DateTimeLocal
		{
			get
			{
				var localTime = (DateTime.IsValid) ?
					Enterprise.Environment.Env.Time.GetLocalTimeFromUtc(DateTime.ToDateTime())
					: ZDateTime.Invalid;
				return localTime;
			}
		}

		public ZString DateTimeString
		{
			get
			{
				return DateTimeLocal.ToString(LogStringFormats.LoggerTimeMask);
			}
		}

		public ZPropertyInfo DateTimeStringInfo
		{
			get { return GetZPropertyInfo(nameof(DateTimeString)); }
		}

		public int DateTimeString_MaxLength
		{
			get { return LogStringFormats.LoggerTimeMask.Length; }
		}

		public ZString DateTimeUtcString
		{
			get { return DateTime.ToString(LogStringFormats.LoggerTimeMask); }
		}

		public ZPropertyInfo DateTimeUtcStringInfo
		{
			get { return GetZPropertyInfo(nameof(DateTimeUtcString)); }
		}

		public int DateTimeUtcString_MaxLength
		{
			get { return LogStringFormats.LoggerTimeMask.Length; }
		}

		#endregion

		public static EventRecord FromString(StringInterner stringInterner, string line)
		{
			// logs used to not include Elastic SequenceId
			var sequenceId = 0;
			if (line.Length >= Logger.LoggerFieldLength && Int32.TryParse(line.Substring(0, Logger.LoggerFieldLength), out sequenceId))
			{
				line = line.Substring(Logger.LoggerFieldLength);
			}

			string dateTimeString;
			var processId = 0;
			if (line.Length > Logger.LoggerFieldLength)
			{
				dateTimeString = line.Substring(0, Logger.LoggerFieldLength);
				line = line.Substring(Logger.LoggerFieldLength);
			}
			else
			{
				dateTimeString = line;
				line = string.Empty;
			}
			dateTimeString = dateTimeString.TrimEnd();
			ZDateTime dateTime;
			if (!ZDateTime.TryParseExact(dateTimeString, out dateTime, LogStringFormats.LoggerTimeMask)
				//log files used to not include milliseconds
				&& !ZDateTime.TryParseExact(dateTimeString, out dateTime, LogStringFormats.LoggerTimeMaskWithoutMilliseconds))
			{
				dateTime = ZDateTime.Invalid;
			}

			string type;
			if (line.Length > Logger.LoggerFieldLength)
			{
				type = line.Substring(0, Logger.LoggerFieldLength);
				line = line.Substring(Logger.LoggerFieldLength);
			}
			else
			{
				type = line;
				line = string.Empty;
			}
			type = type.TrimEnd();

			// logs used to not include ProcessID
			if (line.Length >= Logger.LoggerFieldLength && Int32.TryParse(line.Substring(0, Logger.LoggerFieldLength), out processId))
			{
				line = line.Substring(Logger.LoggerFieldLength);
			}

			// logs used to not include HostName, FileSystem logging will never have a host name in log files
			string host = null;
			if (line.Length >= Logger.LoggerFieldLength && TryParseHostName(line, out host, out var skipLength))
			{
				line = line.Substring(skipLength);
			}

			var message = line;

			return new EventRecord(dateTime, stringInterner.InternValue(type), stringInterner.InternValue(message), processId, stringInterner.InternValue(host), sequenceId);
		}

		static bool TryParseHostName(string message, out string hostName, out int skipLength)
		{
			var match = Regex.Match(message, @"hostname\[(.*?)\].*"); // hostname[eye.wtg.zone]
			if (match.Success)
			{
				hostName = match.Groups[1].Value;
				var hostColumnLength = $"hostname[{hostName}]".Length;
				skipLength = hostColumnLength <= Logger.LoggerFieldLength ? Logger.LoggerFieldLength : hostColumnLength;
				return true;
			}
			hostName = null;
			skipLength = 0;
			return false;
		}
	}
}

