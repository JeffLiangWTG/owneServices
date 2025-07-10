using System;
using System.IO;
using System.Text;
using CargoWise.Common;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Core
{
	public class VerboseLogger : ILogger
	{
		public VerboseLogger(string filename)
		{
			Filename = filename;
			if (!File.Exists(Filename))
			{
				try
				{
					File.Create(Filename).Close();
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}
				}
			}
		}

		public readonly string Filename;

		public bool? canLog;
		public bool CanLog
		{
			get
			{
				if (canLog == null)
				{
					canLog = File.Exists(Filename);
				}
				return canLog.Value;
			}
		}

		public bool HasLogs { get; private set; }

		#region ILogger Members

		public void Log(LogType type, string message, Exception ex)
		{
			Log(type, message + "\r\n" + ex.Message);
		}

		public void Log(LogType type, string message)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(CargoWise.Types.ZDateTime.UtcNow.ToString());
			sb.AppendLine(type.ToString());
			sb.AppendLine(message);
			sb.AppendLine("*******************************************");
			sb.AppendLine();
			for (int i = 0; i < 5; i++)
			{
				try
				{
					File.AppendAllText(Filename, sb.ToString());
					break;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					System.Threading.Thread.Sleep(100);
				}
			}
			HasLogs = true;
		}

		#endregion
	}
}
