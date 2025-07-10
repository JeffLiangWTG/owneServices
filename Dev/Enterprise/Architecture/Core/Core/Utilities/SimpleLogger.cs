using System;
using System.Text;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Core
{
	public class SimpleLogger : ILogger
	{
		readonly StringBuilder contents = new StringBuilder();

		public void Log(LogType type, string message, Exception ex)
		{
			Log(type, message);

			if (ex != null)
			{
				contents.AppendLine(ex.ToString());
			}
		}

		public void Log(LogType type, string message)
		{
			if (type == LogType.Error || type == LogType.Warning)
			{
				contents.Append(type);
				contents.Append(": ");
			}

			contents.AppendLine(message);
		}

		public override string ToString()
		{
			return contents.ToString();
		}
	}
}
