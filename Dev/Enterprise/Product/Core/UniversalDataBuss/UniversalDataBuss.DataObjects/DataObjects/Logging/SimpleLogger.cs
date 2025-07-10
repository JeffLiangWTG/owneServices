using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment.Email.Html;

namespace Enterprise.UniversalDataBuss.Management
{
	public class SimpleLogger : ISimpleLogger
	{
		public void Log(LogType type, string message)
		{
			LogCore(type, message);
		}

		protected virtual void LogCore(LogType type, string message)
		{
			if (ShouldIncludeTimestampInLogs())
			{
				logs.Add(new SimpleLog(type, Env.Time.CurrentUtcDateTime, message));
			}
			else
			{
				logs.Add(new SimpleLog(type, message));
			}

			if (type == LogType.Error)
			{
				HasErrors = true;
			}
			else if (type == LogType.Warning)
			{
				HasWarnings = true;
			}
		}

		protected readonly List<ISimpleLog> logs = new List<ISimpleLog>();

		public bool HasErrors
		{
			get;
			private set;
		}

		public bool HasWarnings
		{
			get;
			private set;
		}

		public override string ToString()
		{
			return string.Join("\r\n", LogLines);
		}

		protected IEnumerable<string> LogLines
		{
			get { return logs.Select(log => log.ToString()); }
		}

		public string ToHtml(bool logOnlyWarningAndErrorsAndLastLineOnly = true)
		{
			var builder = new StringBuilder();
			builder.Append(EmailHtmlTags.UL_Start);

			for (int i = 0; i < logs.Count; i++)
			{
				bool isResultLine = i == logs.Count - 1;
				bool isNotWarningAndError = logs[i].Type != LogType.Error && logs[i].Type != LogType.Warning;

				if (logOnlyWarningAndErrorsAndLastLineOnly && isNotWarningAndError && !isResultLine)
				{
					continue;
				}

				builder.Append(WebUtility.HtmlEncode(logs[i].ToString())).Append(EmailHtmlTags.BR);
			}

			builder.Append(EmailHtmlTags.UL_End);
			return builder.ToString();
		}

		protected void ResetWarningIndicator()
		{
			HasWarnings = logs.Any(x => x.Type == LogType.Warning);
		}

		#region ISimpleLogResult Members

		IEnumerable<ISimpleLog> ISimpleLogResult.Logs
		{
			get { return logs; }
		}

		bool ShouldIncludeTimestampInLogs()
		{
			var result = eAdaptorRegistry.Instance.UniversalXMLTimestampsInProcessingLogs.Value;

#if DEBUG
			result = result && EnableTimesTimestampsForTest.IsOverriden;
#endif

			return result;
		}

		#endregion

#if DEBUG
		public static Overridable<bool> EnableTimesTimestampsForTest { get; } = new Overridable<bool>();
#endif
	}
}
