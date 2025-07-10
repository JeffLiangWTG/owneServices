using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management
{
	public class InlineLogger : ISimpleLogger
	{
		public InlineLogger(string prefix)
		{
			this.prefix = Argument.NotNull(prefix, "prefix");
		}
		readonly string prefix;

		readonly List<SimpleLog> logs = new List<SimpleLog>();

		public void Log(LogType type, string message)
		{
			logs.Add(new SimpleLog(type, message));
		}

		public ISimpleLog Result
		{
			get
			{
				LogType type = LogType.Debug;
				var resultText = new ZStringBuilder(prefix);
				foreach (var log in logs)
				{
					resultText.Append(log.Message);
					type = log.Type < type ? log.Type : type;
				}
				return new SimpleLog(type, resultText.ToStringWithDelimiterBetweenAppends(" "));
			}
		}

		IEnumerable<ISimpleLog> ISimpleLogResult.Logs
		{
			get { return new[] { Result }; }
		}
	}
}
