using System;
using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.LogWalker.Testing
{
	[Serializable]
	public class LoggerForTesting : ILogger
	{
		public bool AllowDebug { get; set; }

		public virtual void Log(LogType logType, string message)
		{
			if (logType != LogType.Debug || AllowDebug)
			{
				lock (this)
				{
					notifiedEventList.Add(message);
				}
			}
		}

		public virtual void Log(LogType logType, string message, Exception ex)
		{
			Log(logType, message);
		}

		public void Clear()
		{
			lock (this)
			{
				notifiedEventList.Clear();
			}
		}

		public List<string> NotifiedEventList
		{
			get
			{
				lock (this)
				{
					return new List<string>(notifiedEventList);
				}
			}
		}

		readonly List<string> notifiedEventList = new List<string>();

		public override string ToString()
		{
			lock (this)
			{
				return string.Join(System.Environment.NewLine, notifiedEventList);
			}
		}
	}
}
