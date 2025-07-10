using System;

namespace CargoWise.Data
{
	public class SqlCommandExecutedEventArgs : EventArgs
	{
		public SqlCommandExecutedEventArgs(string sqlText, Exception exception, DateTime time)
		{
			Text = sqlText;
			Exception = exception;
			Time = time;
		}

		public string Text { get; set; }

		public Exception Exception { get; set; }

		public DateTime Time { get; set; }
	}
}
