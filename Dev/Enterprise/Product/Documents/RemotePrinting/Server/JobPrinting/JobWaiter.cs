using System;
using CargoWise.Common;

namespace Enterprise.RemotePrinting.Server.JobPrinting
{
	public class JobWaiter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public JobWaiter(AsyncCallback callback, PrintJobAsyncResult asyncResult)
		{
			Argument.NotNull(asyncResult, nameof(asyncResult));
			Argument.NotNull(callback, nameof(callback));
			Argument.NotNull(asyncResult.ServerName, nameof(asyncResult.ServerName));

			this.callback = callback;
			this.asyncResult = asyncResult;
			startTime = DateTime.UtcNow;
		}

		public string ServerName
		{
			get
			{
				return asyncResult.ServerName;
			}
		}

		readonly AsyncCallback callback;
		readonly PrintJobAsyncResult asyncResult;
		readonly DateTime startTime;

		public void Signal(Exception exception)
		{
			asyncResult.AsyncState = exception;
			Signal();
		}

		public void Signal(bool hasItems)
		{
			asyncResult.AsyncState = hasItems;
			Signal();
		}

		void Signal()
		{
			callback(asyncResult);
			asyncResult.SingleComplete();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public bool WaitTimeExpired
		{
			get { return DateTime.UtcNow.Subtract(startTime).TotalSeconds > WaitingTimeoutInSeconds; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public const int WaitingTimeoutInSeconds = 60;
	}
}
