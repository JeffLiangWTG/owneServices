using System;
using System.Text;
using Enterprise.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.Progress
{
	public class ProgressFormAdapterLogger : ProgressFormAdapter, ILogger
	{
		public ProgressFormAdapterLogger(ProgressForm form) : base(form)
		{
		}

		public void Log(LogType type, string message)
		{
			SetStatusAndPercentComplete(message, 0);
			Message.AppendLine(message);
			if (type == LogType.Error || type == LogType.Warning)
			{
				ErrorMessage.AppendLine(message);
			}
		}

		public void Log(LogType type, string message, Exception ex) => Log(type, $"{message} {ex}");

		public override string ToString() => ErrorMessage.ToString();

		public string Logs => Message.ToString();

		readonly StringBuilder ErrorMessage = new StringBuilder();

		readonly StringBuilder Message = new StringBuilder();
	}
}

