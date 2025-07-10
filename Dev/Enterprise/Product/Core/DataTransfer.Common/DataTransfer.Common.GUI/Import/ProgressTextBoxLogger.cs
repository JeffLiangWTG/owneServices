using System;
using System.Windows.Forms;
using Enterprise.Integration;

namespace Enterprise.DataTransfer.Common.GUI.Import
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
	public class ProgressTextBoxLogger : ILogger
	{
		public ProgressTextBoxLogger(TextBox logger)
		{
			this.logger = logger;
		}

		public void Log(LogType type, string message)
		{
			if (!logger.IsDisposed && type != LogType.Debug)
			{
				logger.AppendText(message + System.Environment.NewLine);
			}
			Application.DoEvents();
		}

		public void Log(LogType type, string message, Exception ex)
		{
			if (!logger.IsDisposed)
			{
				logger.AppendText(message);
				logger.AppendText(System.Environment.NewLine + System.Environment.NewLine);
				logger.AppendText(ex.StackTrace);
				logger.AppendText(System.Environment.NewLine);
			}
			Application.DoEvents();
		}

		readonly TextBox logger;
	}
}
