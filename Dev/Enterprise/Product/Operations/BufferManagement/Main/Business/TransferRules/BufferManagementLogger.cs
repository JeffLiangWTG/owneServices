using System;
using System.Text;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class BufferManagementLogger : ILogger
	{
		public void Log(LogType type, string message, Exception ex)
		{
			Log(type, message + " " + ex.Message);
		}

		public void Log(LogType type, string message)
		{
			if (type != LogType.Information)
			{
				message = type.ToString() + " - " + message;
			}

			lock (stringBuilder)
			{
				stringBuilder.AppendLine(message);
			}
		}

		readonly StringBuilder stringBuilder = new StringBuilder();

		public override string ToString()
		{
			lock (stringBuilder)
			{
				return stringBuilder.ToString();
			}
		}

		public void Clear() => stringBuilder.Clear();
	}
}
