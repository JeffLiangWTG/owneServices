using System;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Messaging.Business
{
	public class Logger : ILogger
	{
		#region Implementation of ILogger

		public void Log(LogType type, string message)
		{
			builder.Append(message);
			if (type == LogType.Error)
			{
				HasAnyErrorMessage = true;
			}
		}

		public void Log(LogType type, string message, Exception ex)
		{
			Log(type, message);
		}

		#endregion

		public override string ToString()
		{
			return builder.ToStringWithNewLineBetweenAppends();
		}

		public string ToStringWithExtraNewLine()
		{
			return builder.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine + System.Environment.NewLine);
		}

		public bool HasAnyErrorMessage { get; private set; }

		readonly ZStringBuilder builder = new ZStringBuilder();
	}
}
