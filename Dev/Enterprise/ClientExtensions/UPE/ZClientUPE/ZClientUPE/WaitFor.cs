using System;
using System.Threading;
using CargoWise.Common;

namespace Enterprise.Client.UPE
{
	public sealed class WaitFor<TResult>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used for Non Winzor, will be fixed once cut over to .Net 8")]
		readonly TimeSpan _timeout;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used for Non Winzor, will be fixed once cut over to .Net 8")]
		readonly ManualResetEvent resetEvent = new ManualResetEvent(false);

		//This class should be removed and replace with async code that works better for .NET8
		//The code originally used Thread.Abort which is not supported in .NET8 and replaced temporary with Thread.Interrupt.
		//The best way forward is to replace it with proper async code using cancellation tokens, however this may prove difficult due
		//the usage of SSH.Net which does not support cancellation tokens for its SFTP client, There is ongoing work in the SSH.Net
		//library see https://github.com/sshnet/SSH.NET/pull/1515.
		public WaitFor(TimeSpan timeout)
		{
			_timeout = timeout;
		}

		public TResult Run(Func<TResult> function)
		{
			Argument.NotNull(function, nameof(function));

			var sync = new object();
			var isCompleted = false;

			WaitCallback watcher = obj =>
			{
				var watchedThread = obj as Thread;

				lock (sync)
				{
					if (!isCompleted)
					{
						resetEvent.Set();
						Monitor.Wait(sync, _timeout);
					}
				}

				if (!isCompleted)
				{
					watchedThread.Interrupt();
				}
			};

			try
			{
				ThreadPool.QueueUserWorkItem(watcher, Thread.CurrentThread);
				if (resetEvent.WaitOne(_timeout))
				{
					return function();
				}

				throw new TimeoutException($"The operation has timed out after {_timeout}.");
			}
			catch (ThreadInterruptedException)
			{
				throw new TimeoutException($"The operation has timed out after {_timeout}.");
			}
			finally
			{
				lock (sync)
				{
					isCompleted = true;
					Monitor.Pulse(sync);
				}
			}
		}

		public static TResult Run(TimeSpan timeout, Func<TResult> function)
		{
			return new WaitFor<TResult>(timeout).Run(function);
		}
	}
}
