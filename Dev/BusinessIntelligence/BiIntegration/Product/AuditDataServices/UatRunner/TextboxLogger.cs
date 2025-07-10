namespace Enterprise.AuditDataServices.UatRunner
{
	using System;
	using System.Windows.Forms;
	using Enterprise.Integration;

	class TextboxLogger : ILogger
	{
		public TextboxLogger(TextBox loggingBox)
		{
			this.loggingBox = loggingBox;
		}

		void ILogger.Log(LogType type, string message)
		{
			AppendToLoggingBox(type, message);
		}

		void ILogger.Log(LogType type, string message, Exception ex)
		{
			AppendToLoggingBox(type, message);
		}

		void AppendToLoggingBox(LogType type, string message)
		{
			if (loggingBox.InvokeRequired)
			{
				loggingBox.BeginInvoke(new Action(() => AppendToLoggingBox(type, message)));
				return;
			}

			loggingBox.AppendText(
				((type == LogType.Information) ? "" : type.ToString() + " - ")
				+ message
				+ "\r\n");
		}

		readonly TextBox loggingBox;
	}
}
