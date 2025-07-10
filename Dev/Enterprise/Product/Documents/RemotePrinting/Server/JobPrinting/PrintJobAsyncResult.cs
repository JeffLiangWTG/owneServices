using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using Enterprise.RemotePrinting.Server.JobPrinting;

namespace Enterprise.RemotePrinting.Server
{
	public class PrintJobAsyncResult : IAsyncResult
	{
		public static PrintJobAsyncResult Synchronous<T>(List<T> jobs) where T : ServerPrintJob, new()
		{
			var result = new PrintJobAsyncResult();
			result.AsyncState = jobs;
			result.IsCompleted = true;
			result.CompletedSynchronously = true;
			return result;
		}

		PrintJobAsyncResult()
		{ }

		public PrintJobAsyncResult(string serverName)
		{
			Argument.NotNull(serverName, nameof(serverName));

			this.ServerName = serverName;
		}

		public string ServerName
		{
			get;
			private set;
		}

		public object AsyncState
		{
			get;
			set;
		}

		public WaitHandle AsyncWaitHandle
		{
			get { return asyncWaitHandle ?? (asyncWaitHandle = new ManualResetEvent(IsCompleted)); }
		}
		ManualResetEvent asyncWaitHandle;

		public bool CompletedSynchronously
		{
			get;
			private set;
		}

		public bool IsCompleted
		{
			get;
			private set;
		}

		public void SingleComplete()
		{
			IsCompleted = true;
			if (asyncWaitHandle != null)
			{
				asyncWaitHandle.Set();
			}
		}
	}
}
